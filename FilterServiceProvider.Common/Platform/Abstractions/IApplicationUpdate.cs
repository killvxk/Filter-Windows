using System;
using System.Threading.Tasks;
using Citadel.Platform.Common.Util.Update;

namespace FilterServiceProvider.Common.Platform.Abstractions
{
    public interface IApplicationUpdate
    {
        bool IsRestartRequired { get; }

        DateTime DatePublished { get; }

        string Title { get; }

        Version CurrentVersion { get; }

        Version UpdateVersion { get; }

        UpdateKind Kind { get; }

        string HtmlBody { get; }

        Task DownloadUpdate();

        void BeginInstallUpdateDelayed(int delaySeconds = 10, bool restartApplication = true);
    }
}
