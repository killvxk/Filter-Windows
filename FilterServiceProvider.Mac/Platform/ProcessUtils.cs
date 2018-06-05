using System;
using FilterServiceProvider.Common.Platform.Abstractions;

namespace FilterServiceProvider.Mac.Platform
{
    public class ProcessUtils : IProcessUtils
    {
        public ProcessUtils()
        {
        }

        public string DefaultBrowserPath {
            get
            {
                // TODO: Implement DefaultBrowserPath
                return null;
            }
        }

        public bool StartProcessAsCurrentUser(string appPath, string cmdLine = null, string workDir = null, bool visible = true)
        {
            // TODO: Implement StartProcessAsCurrentUser()
            throw new NotImplementedException();
        }
    }
}
