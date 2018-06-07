using System;
using CitadelCore.Net.Proxy;

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
        void InitializeOnSessionEnding(SessionEndingHandler handler);

        /// <summary>
        /// A function to ensure that our filter has access to the internet through the firewall.
        /// This applies to local only, I believe.
        /// </summary>
        void EnsureFirewallAccess();

        void ReviveGuiForCurrentUser(bool runInTray = false);

        void KillAllGuis();

        bool OnAppFirewallCheck(string appAbsolutePath);

        INetworkStatus NetworkStatus { get; }

        IAntitampering Antitampering { get; }

        IProcessUtils ProcessUtils { get; }

        IInternetUtils InternetUtils { get; }

        IWlanInfo WlanInfo { get; }

        IPathProvider Path { get; }

        /// <summary>
        /// The fingerprint to use to identify this particular computer.
        /// </summary>
        string Fingerprint { get; }
    }
}
