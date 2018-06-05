using System;
using System.IO;

namespace FilterServiceProvider.Common.Platform
{
    public static class PathProvider
    {
        public static string GetAppDataPath()
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "CloudVeil");
        }

        public static string GetAppDataFile(string fileName)
        {
            return Path.Combine(GetAppDataPath(), fileName);
        }
    }
}
