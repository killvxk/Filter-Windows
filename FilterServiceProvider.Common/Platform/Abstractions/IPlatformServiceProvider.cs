using System;
namespace FilterServiceProvider.Common.Platform.Abstractions
{
    public delegate void SessionEndingHandler();

    /// <summary>
    /// This interface provides a common API for Windows, MacOS, and Linux services to implement in order to provide a uniform experience.
    /// Not all of these functions get called by the common provider.
    /// </summary>
    public interface IPlatformServiceProvider
    {
        /// <summary>
        /// A function to initialize SessionEnding shutdown capabilities if needed.
        /// </summary>
        void InitializeOnSessionEnding();

        /// <summary>
        /// A function to ensure that our filter has access to the internet through the firewall.
        /// This applies to local only, I believe.
        /// </summary>
        void EnsureFirewallAccess();

        INetworkStatus NetworkStatus { get; }

        IAntitampering Antitampering { get; }

        IDnsEnforcement DnsEnforcement { get; }

        /// <summary>
        /// The fingerprint to use to identify this particular computer.
        /// </summary>
        string Fingerprint { get; }
    }
}
