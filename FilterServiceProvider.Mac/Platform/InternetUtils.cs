using System;
using FilterServiceProvider.Common.Platform.Abstractions;

namespace FilterServiceProvider.Mac.Platform
{
    public class InternetUtils : IInternetUtils
    {
        public InternetUtils()
        {
        }

        public void DisableInternet()
        {
            //throw new NotImplementedException();
            // TODO: Implement
            // I don't have a good idea right now on how to disable internet for macOS.
        }

        public void EnableInternet()
        {
            // TODO: Implement
        }
    }
}
