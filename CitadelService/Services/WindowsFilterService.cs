using System;
using System.Runtime.InteropServices;
using System.Threading;
using FilterServiceProvider.Common.Platform.Abstractions;

namespace CitadelService.Services
{
    public class WindowsFilterService : IPlatformServiceProvider
    {
        private FilterServiceProvider m_filterServiceProvider;

        private void initServiceProvider()
        {
            if(m_filterServiceProvider == null) {
                m_filterServiceProvider = new FilterServiceProvider();
            }
        }

        public bool Start()
        {
            initServiceProvider();

            try
            {
                Thread thread = new Thread(OnStartup);
                thread.Start();

                //OnStartup();
            }
            catch (Exception e)
            {
                // Critical failure.
                try
                {
                    EventLog.CreateEventSource("FilterServiceProvider", "Application");
                    EventLog.WriteEntry("FilterServiceProvider", $"Exception occurred before logger was bootstrapped: {e.ToString()}");
                }
                catch (Exception e2)
                {
                    File.AppendAllText(@"C:\FilterServiceProvider.FatalCrashLog.log", $"Fatal crash.\r\n{e.ToString()}\r\n{e2.ToString()}");
                }

                //LoggerUtil.RecursivelyLogException(m_logger, e);
                return false;
            }

            return true;
        }

        public bool Stop()
        {
            // We always return false because we don't let anyone tell us that we're going to stop.
            return false;
        }

        public bool Shutdown()
        {
            // Called on a shutdown event.
            Environment.Exit((int)ExitCodes.ShutdownWithSafeguards);
            return true;
        }

        private void OnStartup()
        {
            try
            {
                SystemEvents.OnAppSessionEnding += (sender, args) =>
                {
                    this.OnSessionEnding();
                };

                m_filterServiceProvider.OnStartup();
            }
            catch (Exception e)
            {
                LoggerUtil.RecursivelyLogException(m_logger, e);
            }
        }

        public void OnSessionChanged()
        {
            ReviveGuiForCurrentUser(true);
        }

        /// <summary>
        /// Implements IPlatformServiceProvider.EnsureFirewallAccess()
        /// </summary>
        public void EnsureFirewallAccess()
        {
            try
            {
                string thisProcessName = System.Diagnostics.Process.GetCurrentProcess().ProcessName;
                var thisAssembly = System.Reflection.Assembly.GetExecutingAssembly();

                // Get all existing rules matching our process name and destroy them.
                var myRules = FirewallManager.Instance.Rules.Where(r => r.Name.Equals(thisProcessName, StringComparison.OrdinalIgnoreCase)).ToArray();
                if (myRules != null && myRules.Length > 0)
                {
                    foreach (var rule in myRules)
                    {
                        FirewallManager.Instance.Rules.Remove(rule);
                    }
                }

                // Create inbound/outbound firewall rules and add them.
                var inboundRule = FirewallManager.Instance.CreateApplicationRule(
                    FirewallProfiles.Domain | FirewallProfiles.Private | FirewallProfiles.Public,
                    thisProcessName,
                    FirewallAction.Allow, thisAssembly.Location
                );
                inboundRule.Direction = FirewallDirection.Inbound;

                FirewallManager.Instance.Rules.Add(inboundRule);

                var outboundRule = FirewallManager.Instance.CreateApplicationRule(
                    FirewallProfiles.Domain | FirewallProfiles.Private | FirewallProfiles.Public,
                    thisProcessName,
                    FirewallAction.Allow, thisAssembly.Location
                );
                outboundRule.Direction = FirewallDirection.Outbound;

                FirewallManager.Instance.Rules.Add(outboundRule);
            }
            catch (Exception e)
            {
                m_logger.Error("Error while attempting to configure firewall application exception.");
                LoggerUtil.RecursivelyLogException(m_logger, e);
            }
        }

    }
}
