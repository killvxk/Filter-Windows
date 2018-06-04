using System;
using System.Collections.Generic;
using NativeWifi;

using FilterServiceProvider.Common.Platform.Abstractions;

namespace CitadelService.Util
{
    public class WifiManager : IWifiManager
    {
        public List<string> GetWifiSSIDs()
        {
            try
            {
                WlanClient wlanClient = new WlanClient();
                List<string> connectedSsids = new List<string>();

                foreach (WlanClient.WlanInterface wlanInterface in wlanClient.Interfaces)
                {
                    Wlan.Dot11Ssid ssid = wlanInterface.CurrentConnection.wlanAssociationAttributes.dot11Ssid;
                    connectedSsids.Add(new string(Encoding.ASCII.GetChars(ssid.SSID, 0, (int)ssid.SSIDLength)));
                }

                return connectedSsids.ToArray();
            }
            catch (Exception e)
            {
                LoggerUtil.RecursivelyLogException(LoggerUtil.GetAppWideLogger(), e);
                return null;
            }
        }
    }
}
