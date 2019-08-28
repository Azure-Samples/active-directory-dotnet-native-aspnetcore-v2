﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Authentication.AzureAD.UI;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Client;
using System.Security.Claims;

namespace Microsoft.Identity.Web.Client.TokenCacheProviders
{
    /// <summary>
    /// MSAL token cache provider interface for application cache
    /// </summary>
    public interface IMSALAppTokenCacheProvider
    {
        /// <summary>Initializes this instance of TokenCacheProvider with essentials to initialize themselves.</summary>
        /// <param name="tokenCache">The token cache instance of MSAL application</param>
        /// <param name="httpcontext">The Httpcontext whose Session will be used for caching.This is required by some providers.</param>
        void Initialize(ITokenCache tokenCache, HttpContext httpcontext);

        /// <summary>
        /// Clears the token cache for this app
        /// </summary>
        void Clear();
    }
}