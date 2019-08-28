﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Http;
using Microsoft.Identity.Client;
using System.Security.Claims;

namespace Microsoft.Identity.Web.Client.TokenCacheProviders
{
    /// <summary>
    /// MSAL token cache provider interface for user accounts
    /// </summary>
    public interface IMSALUserTokenCacheProvider
    {
        /// <summary>Initializes this instance of TokenCacheProvider with essentials to initialize themselves.</summary>
        /// <param name="tokenCache">The token cache instance of MSAL application</param>
        /// <param name="httpcontext">The Httpcontext whose Session will be used for caching.This is required by some providers.</param>
        /// <param name="user">The signed-in user for whom the cache needs to be established. Not needed by all providers.</param>
        void Initialize(ITokenCache tokenCache, HttpContext httpcontext, ClaimsPrincipal user);

        /// <summary>
        /// Clears the token cache for this user
        /// </summary>
        void Clear(string accountId);
    }
}