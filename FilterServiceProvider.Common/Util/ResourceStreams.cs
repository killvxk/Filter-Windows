using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CitadelService.Util
{
    public static class ResourceStreams
    {
        private static Stream getStream(Assembly assembly, string resourceName)
        {
            var resourceStream = assembly.GetManifestResourceStream(resourceName);

            return resourceStream;
        }

        public static byte[] Get(string resourceName)
        {
            try
            {
                //var blockedPagePackURI = "CitadelService.Resources.BlockedPage.html";
                // This is an ugly hack to allow us to get resources from FilterServiceProvider.Mac as well as FilterServiceProvider.Common.
                // It might not work in the long term.
                Stream resourceStream = getStream(Assembly.GetEntryAssembly(), resourceName);

                if(resourceStream == null) {
                    resourceStream = getStream(Assembly.GetExecutingAssembly(), resourceName);
                }

                if(resourceStream == null) {
                    return null;
                }

                if(resourceStream.CanRead) {
                    using (TextReader tsr = new StreamReader(resourceStream))
                    {
                        return Encoding.UTF8.GetBytes(tsr.ReadToEnd());
                    }
                }

                return null;
            }
            catch
            {
                return null;
            }
        }
    }
}
