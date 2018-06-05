using System;
namespace FilterServiceProvider.Common.Platform.Abstractions
{
    public interface IProcessUtils
    {
        bool StartProcessAsCurrentUser(string appPath, string cmdLine = null, string workDir = null, bool visible = true);

        string DefaultBrowserPath { get; }
    }
}
