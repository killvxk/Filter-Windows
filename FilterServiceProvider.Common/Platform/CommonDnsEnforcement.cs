using Citadel.Core.Windows.Util;
using CitadelService.Common.Configuration;
using DNS.Client;
using DNS.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FilterServiceProvider.Common.Platform.Abstractions;
using FilterServiceProvider.Services;
using FilterServiceProvider.Common.Util;

namespace FilterServiceProvider.Common.Platform
{
    public abstract class CommonDnsEnforcement : IDnsEnforcement
    {
        /// <summary>
        /// This timer is used to monitor local NIC cards and enforce DNS settings when they are
        /// configured in the application config.
        /// </summary>
        private Timer m_dnsEnforcementTimer;

        public CommonDnsEnforcement(IPolicyConfiguration configuration, NLog.Logger logger)
        {
            m_logger = logger;
            m_configuration = configuration;
        }

        private object m_dnsEnforcementLock = new object();
        protected NLog.Logger m_logger;
        protected IPolicyConfiguration m_configuration;

        #region DnsEnforcement.Enforce

        /// <summary>
        /// 
        /// </summary>
        /// <param name="enableDnsFiltering">If true, this function enables DNS filtering with entries in the configuration.</param>
        public abstract void TryEnforce(bool enableDnsFiltering = true);

        #endregion

        #region DnsEnforcement.Decision
        /// <summary>
        /// Detects whether the user is behind a captive portal.
        /// </summary>
        /// <returns></returns>
        public async Task<bool> IsBehindCaptivePortal()
        {
            bool active = await IsCaptivePortalActive();

            if (active)
            {
                CaptivePortalHelper.Default.OnCaptivePortalDetected();
                OnCaptivePortalMode?.Invoke(true, true);
                return active;
            }
            else
            {
                bool ret = CaptivePortalHelper.Default.IsCurrentNetworkCaptivePortal();
                OnCaptivePortalMode?.Invoke(ret, active);
                return ret;
            }
        }

        protected DateTime lastDnsCheck = DateTime.MinValue;
        protected bool lastDnsResult = true;

        public void InvalidateDnsResult()
        {
            lastDnsCheck = DateTime.MinValue;
        }

        /// <summary>
        /// Detects whether our DNS servers are down.
        /// 
        /// This one's a little sticky because we don't know whether internet is down for sure.
        /// I think it's easy enough to just assume that if we can't reach our DNS servers we should probably flip the switch.
        /// 
        /// If first server checked is up, no more are checked, and so on.
        /// </summary>
        /// <returns>Returns true if at least one of the servers in the configuration returns a response or if there are none configured. Returns false if all servers tried do not return a response.</returns>
        public async Task<bool> IsDnsUp()
        {
            if(lastDnsCheck.AddMinutes(5) > DateTime.Now)
            {
                return lastDnsResult;
            }

            lastDnsCheck = DateTime.Now;

            bool ret = false;

            if(m_configuration.Configuration == null)
            {
                // We can't really make a decision on enforcement here, but just return true anyway.
                return true;
            }

            string primaryDns = m_configuration.Configuration.PrimaryDns;
            string secondaryDns = m_configuration.Configuration.SecondaryDns;

            if (string.IsNullOrWhiteSpace(primaryDns) && string.IsNullOrWhiteSpace(secondaryDns))
            {
                ret = true;
            }
            else
            {
                List<string> dnsSearch = new List<string>();
                if (!string.IsNullOrWhiteSpace(primaryDns))
                {
                    dnsSearch.Add(primaryDns);
                }

                if (!string.IsNullOrWhiteSpace(secondaryDns))
                {
                    dnsSearch.Add(secondaryDns);
                }

                int failedDnsServers = 0;

                foreach (string dnsServer in dnsSearch)
                {
                    try
                    {
                        DnsClient client = new DnsClient(dnsServer);

                        IList<IPAddress> ips = await client.Lookup("testdns.cloudveil.org");

                        if (ips != null && ips.Count > 0)
                        {
                            ret = true;
                            break;
                        }
                        else
                        {
                            failedDnsServers++;
                        }
                    }
                    catch (Exception ex)
                    {
                        failedDnsServers++;
                        m_logger.Error($"Failed to contact DNS server {dnsServer}");
                        LoggerUtil.RecursivelyLogException(m_logger, ex);
                    }
                }
            }

            lastDnsResult = ret;
            return ret;
        }

        /// <summary>
        /// Detects whether we are blocked by a captive portal and returns result accordingly.
        /// </summary>
        private async Task<bool> IsCaptivePortalActive()
        {
            if(!FilterProvider.Platform.NetworkStatus.HasIpv4InetConnection && !FilterProvider.Platform.NetworkStatus.HasIpv6InetConnection)
            {
                // No point in checking further if no internet available.
                try
                {
                    IPHostEntry entry = Dns.GetHostEntry("connectivitycheck.cloudveil.org");
                }
                catch (Exception ex)
                {
                    m_logger.Info("No DNS servers detected as up by captive portal.");
                    LoggerUtil.RecursivelyLogException(m_logger, ex);

                    return false;
                }

                // Did we get here? This probably means we have internet access, but captive portal may be blocking.
            }

            CaptivePortalDetected ret = checkCaptivePortalState();
            if (ret == CaptivePortalDetected.NoResponseReturned)
            {
                // If no response is returned, this may mean that 
                // a) the network is still initializing
                // b) we have no internet.
                // Schedule a Trigger() for 1.5 second in the future to handle (a)
                
                Task.Delay(1500).ContinueWith((task) =>
                {
                    Trigger();
                });

                return false;
            }
            else if (ret == CaptivePortalDetected.Yes)
            {
                m_logger.Info("Captive portal detected.");
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Checks http://connectivitycheck.cloudveil.org for connectivity.
        /// </summary>
        /// <remarks>
        /// Windows 7 captive portal detection isn't perfect. Somehow in my testing, it got disabled on my test network.
        /// 
        /// Granted, a restart may fix it, but we're not going to ask our customers to do that in order to get their computer working on a captive portal.
        /// </remarks>
        /// <returns>true if captive portal.</returns>
        private CaptivePortalDetected checkCaptivePortalState()
        {
            var netStatus = FilterProvider.Platform.NetworkStatus;
            if (netStatus.BehindIPv4CaptivePortal || netStatus.BehindIPv6CaptivePortal)
            {
                return CaptivePortalDetected.Yes;
            }

            // "Oh, you want to depend on Windows captive portal detection? Haha nope!" -- Boingo Wi-FI
            // Some captive portals indeed let msftncsi.com through and thoroughly break windows captive portal detection.
            // BWI airport wifi is one of them.
            WebClient client = new WebClient();
            string captivePortalCheck = null;
            try
            {
                captivePortalCheck = client.DownloadString("http://connectivitycheck.cloudveil.org/ncsi.txt");

                if (captivePortalCheck.Trim(' ', '\r', '\n', '\t') != "CloudVeil NCSI")
                {
                    return CaptivePortalDetected.Yes;
                }
            }
            catch (WebException ex)
            {
                if (ex.Response == null)
                {
                    return CaptivePortalDetected.NoResponseReturned;
                }

                m_logger.Info("Got an error response from captive portal check. {0}", ex.Status);
                return CaptivePortalDetected.Yes;
            }
            catch (Exception ex)
            {
                LoggerUtil.RecursivelyLogException(m_logger, ex);
                return CaptivePortalDetected.No;
            }

            return CaptivePortalDetected.No;

        }
        #endregion

        // This region includes timers and other event functions in which to run decision functions
        #region DnsEnforcement.Triggers

        private bool isBehindCaptivePortal = false;

        public async void Trigger()
        {
            try
            {
                bool isDnsUp = await IsDnsUp();

                if(!isDnsUp)
                {
                    m_logger.Info("DNS is down.");

                    TryEnforce(enableDnsFiltering: false);
                    return;
                }

                bool isCaptivePortal = await IsBehindCaptivePortal();

                isBehindCaptivePortal = isCaptivePortal;
                TryEnforce(enableDnsFiltering: !isCaptivePortal && isDnsUp);
            }
            catch (Exception ex)
            {
                m_logger.Error("Failed to trigger DnsEnforcement");
                LoggerUtil.RecursivelyLogException(m_logger, ex);
            }

            SetupTimers();
        }

        public void SetupTimers()
        {
            int timerTime = isBehindCaptivePortal ? 5000 : 60000;

            lock(m_dnsEnforcementLock)
            {
                if (m_dnsEnforcementTimer == null)
                {
                    m_dnsEnforcementTimer = new Timer(TriggerTimer, null, timerTime, timerTime);
                }
                else
                {
                    m_dnsEnforcementTimer.Change(TimeSpan.FromMilliseconds(timerTime), TimeSpan.FromMilliseconds(timerTime));
                }
            }
            
        }

        public void OnNetworkChange(object sender, EventArgs e)
        {
            if (m_configuration.Configuration == null)
            {
                EventHandler fn = null;

                fn = (_s, args) =>
                {
                    Trigger();
                    m_configuration.OnConfigurationLoaded -= fn;
                };

                m_configuration.OnConfigurationLoaded += fn;
            }
            else
            {
                Trigger();
            }
        }

        #endregion

        #region DnsEnforcement.Events
        public event DnsEnforcementHandler OnDnsEnforcementUpdate;
        public event CaptivePortalModeHandler OnCaptivePortalMode;
        #endregion

        private void TriggerTimer(object state)
        {
            Trigger();
        }

        public void OnNetworkChanged(object sender, EventArgs e)
        {
            if (m_configuration.Configuration == null)
            {
                EventHandler fn = null;

                fn = (_s, args) =>
                {
                    Trigger();
                    m_configuration.OnConfigurationLoaded -= fn;
                };

                m_configuration.OnConfigurationLoaded += fn;
            }
            else
            {
                Trigger();
            }
        }
    }
}
