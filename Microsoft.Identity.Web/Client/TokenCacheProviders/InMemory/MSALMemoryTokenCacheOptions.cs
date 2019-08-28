// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;

namespace Microsoft.Identity.Web.Client.TokenCacheProviders
{
    /// <summary>
    /// MSAL's memory token cache options
    /// </summary>
    public class MSALMemoryTokenCacheOptions
    {
        /// <summary>
        /// Gets or sets the value of The fixed date and time at which the cache entry will expire..
        /// The duration till the tokens are kept in memory cache. In production, a higher value , upto 90 days is recommended.
        /// </summary>
        /// <value>
        /// The AbsoluteExpiration value.
        /// </value>
        public TimeSpan SlidingExpiration
        {
            get;
            set;
        }

        public MSALMemoryTokenCacheOptions()
        {
            this.SlidingExpiration = TimeSpan.FromHours(12);
        }
    }
}