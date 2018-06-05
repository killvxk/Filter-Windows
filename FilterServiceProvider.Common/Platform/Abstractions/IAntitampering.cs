/*
* Copyright © 2018 Cloudveil Technology Inc.
* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this
* file, You can obtain one at http://mozilla.org/MPL/2.0/.
*/

using System;
namespace FilterServiceProvider.Common.Platform.Abstractions
{
    public interface IAntitampering
    {
        /// <summary>
        /// This should start a mechanism to keep the filtering service running in event of a naughty user wanting to look at things he shouldn't.
        /// </summary>
        void Initialize();

        /// <summary>
        /// Disables termination of this particular process.
        /// </summary>
        /// <returns>true if the platform implementing this interface can disable process termination.</returns>
        bool DisableProcessTermination();

        /// <summary>
        /// Enables termination of this particular process.
        /// </summary>
        /// <returns><c>true</c> if the platform implementing this interface can disable process termination.</returns>
        bool EnableProcessTermination();
    }
}
