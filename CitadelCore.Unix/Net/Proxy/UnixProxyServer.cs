/*
* Copyright © 2018 Cloudveil Technology Inc.
* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this
* file, You can obtain one at http://mozilla.org/MPL/2.0/.
*/

using System;
using System.Net;
using CitadelCore.Diversion;
using CitadelCore.Net.Proxy;

namespace CitadelCore.Unix.Net.Proxy
{
    public class UnixProxyServer : ProxyServer
    {
        public UnixProxyServer(ProxyOptions options) : base(options)
        {
            
        }

        // Unix diverter need not exist.
        protected override IDiverter CreateDiverter(IPEndPoint ipv4HttpEp, IPEndPoint ipv4HttpsEp, IPEndPoint ipv6HttpEp, IPEndPoint ipv6HttpsEp)
        {
            return new UnixDiverter();
        }
    }
}
