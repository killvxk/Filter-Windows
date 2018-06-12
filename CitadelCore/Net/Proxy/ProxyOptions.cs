using CitadelCore.Net.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace CitadelCore.Net.Proxy
{
    public class ProxyOptions
    {
        /// <summary>
        /// The firewall check callback. Used to allow the user to determine if a binary should have
        /// its associated traffic pushed through the filter or not.
        /// </summary>
        public FirewallCheckCallback FirewallCheckCallback { get; set; }

        /// <summary>
        /// Message begin callback enables users to inspect and filter messages immediately after
        /// they begin. Users also have the power to direct how the proxy will continue to handle the
        /// overall transaction that this message belongs to.
        /// </summary>
        public MessageBeginCallback MessageBeginCallback { get; set; }

        /// <summary>
        /// Message end callback enables users to inspect and filter messages once they have completed. 
        /// </summary>
        public MessageEndCallback MessageEndCallback { get; set; }

        /// <summary>
        /// This gets called by the handler function so that the user can provide a custom response.
        /// </summary>
        public BadCertificateCallback BadCertificateCallback { get; set; }

        /// <summary>
        /// Use this callback if you want to provide a local management server for the app.
        /// </summary>
        public OnServerRequestCallback ServerRequestCallback { get; set; }

        /// <summary>
        /// Provides the user with the opportunity to implement a custom certificate exemption store.
        /// </summary>
        public ICertificateExemptions CertificateExemptions { get; set; }

        /// <summary>
        /// The proxy port for HTTP over TCP/IPv4. Set this to 0 for auto.
        /// </summary>
        /// <value>The http v4 port.</value>
        public int HttpV4Port { get; set; } = 0;

        /// <summary>
        /// The proxy port for HTTPS over TCP/IPv4. Set this to 0 for auto.
        /// </summary>
        /// <value>The https v4 port.</value>
        public int HttpsV4Port { get; set; } = 0;

        /// <summary>
        /// The proxy port for HTTP over TCP/IPv6. Set this to 0 for auto.
        /// </summary>
        /// <value>The http v6 port.</value>
        public int HttpV6Port { get; set; } = 0;

        /// <summary>
        /// The proxy port for HTTPS over TCP/IPv6. Set this to 0 for auto.
        /// </summary>
        /// <value>The https v6 port.</value>
        public int HttpsV6Port { get; set; } = 0;
    }
}
