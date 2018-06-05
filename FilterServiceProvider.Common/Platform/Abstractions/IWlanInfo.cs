using System;
using System.Collections.Generic;

namespace FilterServiceProvider.Common.Platform.Abstractions
{
    public interface IWlanInfo
    {
        List<string> GetConnectedSsids();
    }
}
