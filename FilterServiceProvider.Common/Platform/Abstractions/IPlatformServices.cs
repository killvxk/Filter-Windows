using System;
using Citadel.Platform.Common.IPC;

namespace FilterServiceProvider.Common.Platform.Abstractions
{
    /// <summary>
    /// Not sure how this and IPlatformServiceProvider are going to mesh together yet.
    /// This might be obsolete.
    /// 
    /// Or we might just change this to IPlatformAbstractions
    /// I think this and IPlatformServiceProvider fill two different functions.
    /// </summary>
    public interface IPlatformServices
    {
        IIPCServer NewIPCServer();
        IIPCClient NewIPCClient();

        IDnsEnforcement NewDnsEnforcement();

        ITrustManagement NewTrustManagement();

        IPlatformServiceProvider ServiceProvider { get; }
    }
}
