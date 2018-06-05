using System;
namespace FilterServiceProvider.Common.Platform.Abstractions
{
    public interface IInternetUtils
    {
        void DisableInternet();

        void EnableInternet();
    }
}
