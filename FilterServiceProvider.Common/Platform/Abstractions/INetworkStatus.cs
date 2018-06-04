/*
* Copyright © 2017 Cloudveil Technology Inc.  
* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this
* file, You can obtain one at http://mozilla.org/MPL/2.0/.
*/

using NLog;
using System;
using System.Runtime.InteropServices.ComTypes;

namespace FilterServiceProvider.Common.Platform.Abstractions
{
    /// <summary>
    /// Handler for network state change notifications.
    /// </summary>
    public delegate void ConnectionStateChangeHandler();

    /// <summary>
    /// Class we use to analayze network state information. This is a bit repetative but abstracts
    /// away our underlying implementation.
    /// </summary>
    public interface INetworkStatus
    {
        event ConnectionStateChangeHandler ConnectionStateChanged;

        /// <summary>
        /// Gets whether or not the device has internet access that is not proxied nor behind a
        /// captive portal.
        /// </summary>
        bool HasUnencumberedInternetAccess { get; }
        /// <summary>
        /// Gets whether or not any of the device IPV4 connections have detected that they are behind a
        /// captive portal.
        /// </summary>
        bool BehindIPv4CaptivePortal { get; }

        /// <summary>
        /// Gets whether or not any of the device IPV6 connections have detected that they are behind a
        /// captive portal.
        /// </summary>
        bool BehindIPv6CaptivePortal { get; }

        /// <summary>
        /// Gets whether or not any of the device IPV4 connections have detected that they are behind a
        /// proxy.
        /// </summary>
        bool BehindIPv4Proxy { get; }

        /// <summary>
        /// Gets whether or not any of the device IPV6 connections have detected that they are behind a
        /// proxy.
        /// </summary>
        bool BehindIPv6Proxy { get; }

        /// <summary>
        /// Gets whether or not any of the device IPV4 connections have been determined to be capable
        /// of reaching the internet.
        /// </summary>
        bool HasIpv4InetConnection { get; }

        /// <summary>
        /// Gets whether or not any of the device IPV6 connections have been determined to be capable
        /// of reaching the internet.
        /// </summary>
        bool HasIpv6InetConnection { get; }
    }
}