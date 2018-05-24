using System;
namespace Citadel.Platform.Common
{
    public interface IPlatformServices
    {
        IIPCServer NewIPCServer();
        IIPCClient NewIPCClient();
    }
}
