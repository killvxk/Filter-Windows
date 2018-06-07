using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using CitadelService.Common.Configuration;
using Citadel.Platform.Common.Util;
using FilterServiceProvider.Common.Platform;
using FilterServiceProvider.Common.Platform.Abstractions;

namespace FilterServiceProvider.Mac.Platform
{
    public class DnsEnforcement : CommonDnsEnforcement
    {
        public DnsEnforcement(IPolicyConfiguration configuration, IAppLogger logger) : base(configuration, logger)
        {
            
        }

        public override void TryEnforce(bool enableDnsFiltering = true)
        {
            // TODO: Implement enforcement of DNS settings
            // This might have to be in FilterLibs.Platform.Mac
            if (m_configuration.Configuration != null)
            {
                List<string> dnsServers = new List<string>();

                if (!string.IsNullOrEmpty(m_configuration.Configuration.PrimaryDns))
                {
                    dnsServers.Add(m_configuration.Configuration.PrimaryDns);
                }

                if (!string.IsNullOrEmpty(m_configuration.Configuration.SecondaryDns))
                {
                    dnsServers.Add(m_configuration.Configuration.SecondaryDns);
                }

                NativeLib.EnforceDNS(dnsServers.ToArray(), dnsServers.Count);
            }
        }
    }
}
