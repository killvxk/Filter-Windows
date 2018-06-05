using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using FilterServiceProvider.Common.Platform.Abstractions;

namespace FilterServiceProvider.Mac.Platform
{
    public class WlanInfo : IWlanInfo
    {
        

        public WlanInfo()
        {
        }

        public List<string> GetConnectedSsids()
        {
            string ssid = NativeLib.GetCurrentSSID();

            if (ssid == null)
            {
                return new List<string>();
            } else {
                var list = new List<string>();
                list.Add(ssid);
                return list;
            }
        }
    }
}
