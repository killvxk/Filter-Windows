using System;
using Citadel.Platform.Common.IPC;
using Citadel.Platform.Common.Util;
using CitadelService.Common.Configuration;
using CitadelCore.Net.Proxy;

namespace FilterServiceProvider.Common.Platform.Abstractions
{
    /// <summary>
    /// This interface provides ways to instantiate new instance variables
    /// of platform-specific implementations of abstracted classes.
    /// </summary>
    public interface IPlatformFactory
    {
        IIPCServer NewIPCServer();
        IIPCClient NewIPCClient();

        IDnsEnforcement NewDnsEnforcement(IPolicyConfiguration configuration, IAppLogger logger);

        ITrustManagement NewTrustManager();

        ProxyServer NewProxyServer(FirewallCheckCallback firewallCheck, MessageBeginCallback messageBegin, MessageEndCallback messageEnd, BadCertificateCallback badCertificate);

        IAuthenticationStorage NewAuthenticationStorage();

        IApplicationUpdater NewApplicationUpdater(bool is64bit);
    }
}
