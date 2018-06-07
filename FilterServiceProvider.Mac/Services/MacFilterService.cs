using System;
using CitadelCore.Net.Proxy;
using FilterServiceProvider.Common.Platform.Abstractions;
using FilterServiceProvider.Mac.Platform;

namespace FilterServiceProvider.Mac.Services
{
    public class MacFilterService : IPlatformServiceProvider
    {
        public MacFilterService()
        {
            NetworkStatus = new NetworkStatus();
            Antitampering = new Antitampering();
            ProcessUtils = new ProcessUtils();
            InternetUtils = new InternetUtils();
            WlanInfo = new WlanInfo();
            Path = new MacPathProvider();
        }

        public INetworkStatus NetworkStatus { get; set; }

        public IAntitampering Antitampering { get; set; }

        public IProcessUtils ProcessUtils { get; set; }

        public IInternetUtils InternetUtils { get; set; }

        public string Fingerprint => NativeLib.GetSystemFingerprint();

        public IWlanInfo WlanInfo { get; set; }

        public IPathProvider Path { get; set; }

        public void EnsureFirewallAccess()
        {
            // TODO: Implement. I'm not sure we'll have firewall access on macOS versions of the filter.
        }

        public void InitializeOnSessionEnding(SessionEndingHandler handler)
        {
            // TODO: Implement. How do we want to do this?
        }

        public void KillAllGuis()
        {
            // TODO: Implement.
        }

        public IAuthenticationStorage NewAuthenticationStorage()
        {
            throw new NotImplementedException();
        }

        public ProxyServer NewProxyServer(FirewallCheckCallback firewallCheck, MessageBeginCallback messageBegin, MessageEndCallback messageEnd, BadCertificateCallback badCertificate)
        {
            return new CitadelCore.Unix.Net.Proxy.UnixProxyServer(firewallCheck, messageBegin, messageEnd, badCertificate);
        }

        public bool OnAppFirewallCheck(string appAbsolutePath)
        {
            return true;
        }

        public void ReviveGuiForCurrentUser(bool runInTray = false)
        {
        }
    }
}
