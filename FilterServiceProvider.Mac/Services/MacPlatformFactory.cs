using System;
using Citadel.Platform.Common.IPC;
using CitadelCore.Net.Proxy;
using CitadelCore.Unix.Net.Proxy;
using CitadelService.Common.Configuration;
using Citadel.Platform.Common.Util;
using FilterServiceProvider.Common.Platform.Abstractions;
using FilterServiceProvider.Mac.Platform;
using Filter.Platform.Mac.IPC;

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
            return new MacIPCClient();
        }

        public IIPCServer NewIPCServer()
        {
            return new MacIPCServer();
        }

        public ProxyServer NewProxyServer(ProxyOptions options)
        {
            options.HttpV4Port = 14300;
            options.HttpsV4Port = 14301;
            options.HttpV6Port = 14302;
            options.HttpsV6Port = 14303;

            return new UnixProxyServer(options);
        }

        public ITrustManagement NewTrustManager()
        {
            return new TrustManager();
        }
    }
}
