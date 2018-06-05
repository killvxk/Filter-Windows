using System;
using Citadel.Platform.Common.IPC;
using CitadelCore.Net.Proxy;
using CitadelCore.Unix.Net.Proxy;
using CitadelService.Common.Configuration;
using FilterServiceProvider.Common.Platform.Abstractions;
using FilterServiceProvider.Mac.Platform;

namespace FilterServiceProvider.Mac.Services
{
    public class MacPlatformFactory : IPlatformFactory
    {
        public MacPlatformFactory()
        {
        }

        public IAuthenticationStorage NewAuthenticationStorage()
        {
            // TODO: Implement.

        }

        public IDnsEnforcement NewDnsEnforcement(IPolicyConfiguration configuration, NLog.Logger logger)
        {
            return new DnsEnforcement(configuration, logger);
        }

        public IIPCClient NewIPCClient()
        {
            // TODO: Implement.
        }

        public IIPCServer NewIPCServer()
        {
            // TODO: Implement.
        }

        public ProxyServer NewProxyServer(FirewallCheckCallback firewallCheck, MessageBeginCallback messageBegin, MessageEndCallback messageEnd, BadCertificateCallback badCertificate)
        {
            return new UnixProxyServer(firewallCheck, messageBegin, messageEnd, badCertificate);
        }

        public ITrustManagement NewTrustManager()
        {
            return new TrustManager();
        }
    }
}
