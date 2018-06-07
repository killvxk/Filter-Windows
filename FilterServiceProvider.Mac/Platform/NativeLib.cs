using System;
using System.Runtime.InteropServices;

namespace FilterServiceProvider.Mac.Platform
{
    public static class NativeLib
    {
        /// <summary>
        /// Gets the current SSID we are connected to, or <see langword="null"/> if not connected to a WIFI network.
        /// </summary>
        /// <returns>The current SSID we are connected to, or <see langword="null"/> if not connected to a WIFI network.</returns>
        [DllImport("libFilterLibs.Platform.Mac.dylib")]
        public static extern string GetCurrentSSID();

        /// <summary>
        /// Attempts to apply the given DNS servers to all interfaces on the user's computer.
        /// </summary>
        /// <param name="dnsServers">The list of DNS servers to set in all interfaces.</param>
        /// <param name="dnsServerCount">This is required because the C library can't see the number of DNS servers in the first parameter.</param>
        [DllImport("libFilterLibs.Platform.Mac.dylib")]
        public static extern void EnforceDNS(string[] dnsServers, int dnsServerCount);

        /// <summary>
        /// Gets the system's serial number for use as the unique identifier for this install.
        /// This is currently the serial number but is not guaranteed to always be the serial number.
        /// </summary>
        /// <returns>The system serial number.</returns>
        [DllImport("libFilterLibs.Platform.Mac.dylib")]
        public static extern string GetSystemFingerprint();
    }
}
