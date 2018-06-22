/*
* Copyright © 2017 Jesse Nicholson and CloudVeil Technology, Inc.
* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this
* file, You can obtain one at http://mozilla.org/MPL/2.0/.
*/

using CitadelCore.Logging;
using CitadelCore.Diversion;
using CitadelCore.Net.ConnectionAdapters;
using CitadelCore.Net.Handlers;
using Titanium.Web.Proxy;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Titanium.Web.Proxy.Models;
using Titanium.Web.Proxy.Exceptions;

namespace CitadelCore.Net.Proxy
{
    
    /// <summary>
    /// The ProxyServer class holds the core, platform-independent filtering proxy logic. 
    /// </summary>
    public abstract class ProxyServer
    {
        public static ProxyServer Default { get; private set; }

        // This will handle SNI and handshaking.
        //private TlsSniConnectionAdapter m_tlsConnAdapter;

        private ProxyEndPoint m_v4HttpListenerEp = null;
        private ProxyEndPoint m_v4HttpsListenerEp = null;

        private IPEndPoint m_v6HttpListenerEp = new IPEndPoint(IPAddress.IPv6Any, 0);
        private IPEndPoint m_v6HttpsListenerEp = new IPEndPoint(IPAddress.IPv6Any, 0);

        /// <summary>
        /// List of proxying web servers generated for this host. Currently there's always going to
        /// be two, one for IPV4 and one for IPV6.
        /// </summary>
        //private List<IWebHost> m_hosts = new List<IWebHost>();

        private IDiverter m_diverter;

        /// <summary>
        /// Gets the IPV4 endpoint where HTTP connections are being received. This will be ANY:0
        /// until Start has been called.
        /// </summary>
        public ProxyEndPoint V4HttpEndpoint
        {
            get
            {
                return m_v4HttpListenerEp;
            }
        }

        /// <summary>
        /// Gets the IPV4 endpoint where HTTPS connections are being received. This will be ANY:0
        /// until Start has been called.
        /// </summary>
        public ProxyEndPoint V4HttpsEndpoint
        {
            get
            {
                return m_v4HttpsListenerEp;
            }
        }

        /// <summary>
        /// Gets the IPV6 endpoint where HTTP connections are being received. This will be ANY:0
        /// until Start has been called.
        /// </summary>
        public IPEndPoint V6HttpEndpoint
        {
            get
            {
                return m_v6HttpListenerEp;
            }
        }

        /// <summary>
        /// Gets the IPV6 endpoint where HTTPS connections are being received. This will be ANY:0
        /// until Start has been called.
        /// </summary>
        public IPEndPoint V6HttpsEndpoint
        {
            get
            {
                return m_v6HttpsListenerEp;
            }
        }

        /// <summary>
        /// Gets whether or not the server is currently running.
        /// </summary>
        public bool IsRunning
        {
            get
            {
                return m_running;
            }
        }

        /// <summary>
        /// Ref held to firewall callback.
        /// </summary>
        private FirewallCheckCallback m_fwCallback;

        /// <summary>
        /// Flag that indicates if we're running or not.
        /// </summary>
        private volatile bool m_running = false;

        /// <summary>
        /// For synchronizing startup and shutdown. 
        /// </summary>
        private object m_startStopLock = new object();

        private ProxyOptions m_proxyOptions;
        private Titanium.Web.Proxy.ProxyServer m_proxyServer;

        public ProxyOptions ProxyOptions { get { return m_proxyOptions; } }
        public Titanium.Web.Proxy.ProxyServer InternalProxyServer { get { return m_proxyServer; } }

        /// <summary>
        /// Initialize the proxy server. See <see cref="ProxyOptions"/> for more information on parameters.
        /// </summary>
        /// <param name="options">See <see cref="ProxyOptions"/></param>
        public ProxyServer(ProxyOptions options)
        {
            var proxyServer = new Titanium.Web.Proxy.ProxyServer();
            proxyServer.CertificateManager.TrustRootCertificate();

            proxyServer.BeforeRequest += options.BeforeRequest;
            proxyServer.AfterResponse += options.AfterResponse;

            var explicitEndPoint = new ExplicitProxyEndPoint(IPAddress.Any, options.HttpV4Port, false);
            var explicitEndPointHttps = new ExplicitProxyEndPoint(IPAddress.Any, options.HttpsV4Port, true);

            // TODO: Add a property to ProxyOptions for Explicit end point vs transparent end point.

            // TODO: If needed, do this!
            //explicitEndPointHttps.BeforeTunnelConnectRequest

            proxyServer.AddEndPoint(explicitEndPoint);
            proxyServer.AddEndPoint(explicitEndPointHttps);

            m_proxyServer = proxyServer;

            m_fwCallback = options.FirewallCheckCallback ?? throw new ArgumentException("The firewall callback MUST be defined.");

            /*FilterResponseHandlerFactory.Default.MessageBeginCallback = options.MessageBeginCallback
                ?? throw new ArgumentException("The message begin callback MUST be defined.");

            FilterResponseHandlerFactory.Default.MessageEndCallback = options.MessageEndCallback
                ?? throw new ArgumentException("The message end callback MUST be defined.");*/

            FilterResponseHandlerFactory.Default.BadCertificateCallback = options.BadCertificateCallback;
            FilterResponseHandlerFactory.Default.CertificateExemptions = options.CertificateExemptions;

            //FilterResponseHandlerFactory.Default.ServerRequestCallback = options.ServerRequestCallback;

            // TODO: Once again, we need to differentiate between explicit proxy implementations
            // and transparent proxy implementations.
            m_v4HttpListenerEp = explicitEndPoint;
            m_v4HttpsListenerEp = explicitEndPointHttps;
            /*m_v6HttpListenerEp = new IPEndPoint(IPAddress.Any, options.HttpV6Port);
            m_v6HttpsListenerEp = new IPEndPoint(IPAddress.Any, options.HttpsV6Port);*/

            m_proxyOptions = options;

            Default = this;
        }

        private readonly SemaphoreSlim @lock = new SemaphoreSlim(1);

        /// <summary>
        /// Starts the proxy server on both IPV4 and IPV6 address space. 
        /// </summary>
        public void Start()
        {
            lock(m_startStopLock)
            {
                if(m_running)
                {
                    return;
                }

                m_proxyServer.Start();

                foreach (var endPoint in m_proxyServer.ProxyEndPoints)
                {
                    Console.WriteLine($"Listening @ {endPoint.IpAddress}:{endPoint.Port}");
                }
                // TODO: Only initialize m_diverter if using transparent proxy.
                /*m_diverter = CreateDiverter(
                        m_v4HttpListenerEp,
                        m_v4HttpsListenerEp,
                        m_v6HttpListenerEp,
                        m_v6HttpsListenerEp
                    );

                m_diverter.ConfirmDenyFirewallAccess = (procPath) =>
                {
                    return m_fwCallback.Invoke(procPath);
                };

                m_diverter.Start(0);*/

                m_running = true;
            }           
        }

        /// <summary>
        /// Internal call to create the platform specific packet diverter. 
        /// </summary>
        /// <param name="ipv4HttpEp">
        /// The endpoint where the proxy is listening for IPV4 HTTP connections. 
        /// </param>
        /// <param name="ipv4HttpsEp">
        /// The endpoint where the proxy is listening for IPV4 HTTPS connections. 
        /// </param>
        /// <param name="ipv6HttpEp">
        /// The endpoint where the proxy is listening for IPV6 HTTP connections. 
        /// </param>
        /// <param name="ipv6HttpsEp">
        /// The endpoint where the proxy is listening for IPV6 HTTPS connections. 
        /// </param>
        /// <returns>
        /// The platform specific diverter. 
        /// </returns>
        protected abstract IDiverter CreateDiverter(IPEndPoint ipv4HttpEp, IPEndPoint ipv4HttpsEp, IPEndPoint ipv6HttpEp, IPEndPoint ipv6HttpsEp);

        /// <summary>
        /// Stops any running proxy server listeners and allows them to be disposed. 
        /// </summary>
        public void Stop()
        {
            lock(m_startStopLock)
            {
                if(!m_running)
                {
                    return;
                }

                m_proxyServer.Stop();
                m_proxyServer.Dispose();
                m_proxyServer = null;

                // TODO: Handle m_diverter according to transparent vs explicit proxying.
                //m_diverter.Stop();

                m_running = false;
            }
        }
    }
}