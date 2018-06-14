/*
* Copyright © 2017 Jesse Nicholson
* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this
* file, You can obtain one at http://mozilla.org/MPL/2.0/.
*/

using CitadelCore.Net.Http;
using CitadelCore.Net.Proxy;

namespace CitadelCore.Net.Handlers
{
    internal class FilterResponseHandlerFactory
    {
        private static readonly FilterResponseHandlerFactory s_inst = new FilterResponseHandlerFactory();

        public static FilterResponseHandlerFactory Default
        {
            get
            {
                return s_inst;
            }
        }

        public MessageBeginCallback MessageBeginCallback
        {
            get;
            set;
        }

        public MessageEndCallback MessageEndCallback
        {
            get;
            set;
        }

        public BadCertificateCallback BadCertificateCallback { get; set; }

        //public OnServerRequestCallback ServerRequestCallback { get; set; }

        public ICertificateExemptions CertificateExemptions
        {
            get; set;
        }
    }
}