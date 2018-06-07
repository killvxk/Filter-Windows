using System;
using Citadel.Platform.Common.IPC;
using CitadelCore.Net.Proxy;
using CitadelCore.Unix.Net.Proxy;
using CitadelService.Common.Configuration;
using Citadel.Platform.Common.Util;
using FilterServiceProvider.Common.Platform.Abstractions;
using FilterServiceProvider.Mac.Platform;

namespace FilterServiceProvider.Mac.Services
{
    public class MacPlatformFactory : IPlatformFactory
    {
        public MacPlatformFactory()
        {
        }

        public IApplicationUpdater NewApplicationUpdater(bool is64bit)
        {
            return new ApplicationUpdater();
        }

        public IAuthenticationStorage NewAuthenticationStorage()
        {
            // TODO: Implement.
            return new AuthenticationStorage();
        }

        public IDnsEnforcement NewDnsEnforcement(IPolicyConfiguration configuration, IAppLogger logger)
        {
            return new DnsEnforcement(configuration, logger);
        }

        public IIPCClient NewIPCClient()
        {
            return new IPCClient();
        }

        public IIPCServer NewIPCServer()
        {
            return new IPCServer();
        }

        public ProxyServer NewProxyServer(FirewallCheckCallback firewallCheck, MessageBeginCallback messageBegin, MessageEndCallback messageEnd, BadCertificateCallback badCertificate)
        {
            return new UnixProxyServer(firewallCheck, messageBegin, messageEnd, badCertificate, 14300, 14301, 14302, 14303);
        }

        public ITrustManagement NewTrustManager()
        {
            return new TrustManager();
        }
    }
}
