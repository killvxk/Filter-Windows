using System;
using System.IO;
using FilterServiceProvider.Common.Platform.Abstractions;

namespace FilterServiceProvider.Mac.Platform
{
    public class MacPathProvider : IPathProvider
    {
        public string GetAppDataFile(string fileName)
        {
            return Path.Combine(GetAppDataPath(), fileName);
        }

        public string GetAppDataPath()
        {
            return "/usr/local/share/CloudVeil";
        }
    }
}
