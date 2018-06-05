using System;
using FilterServiceProvider.Common.Platform.Abstractions;

namespace FilterServiceProvider.Mac.Platform
{
    public class NetworkStatus : INetworkStatus
    {
        public NetworkStatus()
        {
        }

        public bool HasUnencumberedInternetAccess => (HasIpv4InetConnection || HasIpv6InetConnection) && (!BehindIPv4CaptivePortal && !BehindIPv6CaptivePortal) && (!BehindIPv4Proxy && !BehindIPv6Proxy);

        // TODO: These all need to be properly implemented for MacOS.
        // We are doing empty implementations to begin with in order to faster test the filter.
        public bool BehindIPv4CaptivePortal => false;

        public bool BehindIPv6CaptivePortal => false;

        public bool BehindIPv4Proxy => false;

        public bool BehindIPv6Proxy => false;

        public bool HasIpv4InetConnection => false;

        public bool HasIpv6InetConnection => false;

        public event ConnectionStateChangeHandler ConnectionStateChanged;
    }
}
