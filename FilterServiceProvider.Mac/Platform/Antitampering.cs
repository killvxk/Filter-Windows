using System;
using FilterServiceProvider.Common.Platform.Abstractions;

namespace FilterServiceProvider.Mac.Platform
{
    public class Antitampering : IAntitampering
    {
        public Antitampering()
        {
        }

        public bool DisableProcessTermination()
        {
            return false;
        }

        public bool EnableProcessTermination()
        {
            return false;
        }

        public void Initialize()
        {
            // TODO: MacOS implementation of Sentinel/Warden/FilterServiceProvider loop.
            return;
        }
    }
}
