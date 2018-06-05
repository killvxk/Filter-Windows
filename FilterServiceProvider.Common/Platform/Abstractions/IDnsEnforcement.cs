using System;
using System.Threading.Tasks;
using CitadelService.Util;

namespace FilterServiceProvider.Common.Platform.Abstractions
{
    public delegate void CaptivePortalModeHandler(bool isCaptivePortal, bool isActive);
    public delegate void DnsEnforcementHandler(bool isEnforcementActive);

    public interface IDnsEnforcement
    {
        /// <summary>
        /// Initialize the timers for a self-sustaining DNS enforcement module.
        /// Ideally, it should check DNS settings every one to five minutes.
        /// 
        /// Cache DNS up-value for longer than that.
        /// </summary>
        void SetupTimers();

        /// <summary>
        /// The FilterServiceProvider calls this when the network changes.
        /// </summary>
        void OnNetworkChanged(object sender, EventArgs e);

        event DnsEnforcementHandler OnDnsEnforcementUpdate;
        event CaptivePortalModeHandler OnCaptivePortalMode;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="enableDnsFiltering">If true, this function enables DNS filtering with entries in the configuration.</param>
        void TryEnforce(bool enableDnsFiltering);

        /// <summary>
        /// This is an API call to trigger DNS settings enforcement.
        /// </summary>
        void Trigger();

        /// <summary>
        /// Detects whether the user is behind a captive portal.
        /// </summary>
        /// <returns></returns>
        Task<bool> IsBehindCaptivePortal();

        /// <summary>
        /// Clears any result we're caching, so that next call to IsDnsUp() doesn't fetch cached result.
        /// </summary>
        void InvalidateDnsResult();


        /// <summary>
        /// Detects whether our DNS servers are down.
        /// 
        /// This one's a little sticky because we don't know whether internet is down for sure.
        /// I think it's easy enough to just assume that if we can't reach our DNS servers we should probably flip the switch.
        /// 
        /// If first server checked is up, no more are checked, and so on.
        /// </summary>
        /// <returns>Returns true if at least one of the servers in the configuration returns a response or if there are none configured. Returns false if all servers tried do not return a response.</returns>
        Task<bool> IsDnsUp();
    }
}
