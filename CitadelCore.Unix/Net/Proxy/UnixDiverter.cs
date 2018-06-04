/*
* Copyright © 2018 Cloudveil Technology Inc.
* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this
* file, You can obtain one at http://mozilla.org/MPL/2.0/.
*/

using System;
using CitadelCore.Diversion;
using CitadelCore.Net.Proxy;

namespace CitadelCore.Unix.Net.Proxy
{
    public class UnixDiverter : IDiverter
    {
        public UnixDiverter()
        {
        }

        public bool IsRunning => true;

        public FirewallCheckCallback ConfirmDenyFirewallAccess { get; set; }

        public void Start(int numThreads)
        {

        }

        public void Stop()
        {
            
        }
    }
}
