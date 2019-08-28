// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Authentication.AzureAD.UI;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Client;
using System;

namespace Microsoft.Identity.Web.Client.TokenCacheProviders
{
    /// <summary>
    /// An implementation of token cache for Confidential clients backed by MemoryCache.
    /// MemoryCache is useful in Api scenarios where there is no HttpContext to cache data.
    /// </summary>
    /// <seealso cref="https://aka.ms/msal-net-token-cache-serialization"/>
    public class MSALAppMemoryTokenCacheProvider : IMSALAppTokenCacheProvider
    {
        /// <summary>
        /// The application cache key
        /// </summary>
        internal string AppCacheId;

        /// <summary>
        /// The backing MemoryCache instance
        /// </summary>
        internal IMemoryCache memoryCache;

        private readonly MSALMemoryTokenCacheOptions CacheOptions;

        /// <summary>
        /// The App's whose cache we are maintaining.
        /// </summary>
        private readonly string AppId;

        public MSALAppMemoryTokenCacheProvider(IMemoryCache cache,
            MSALMemoryTokenCacheOptions option,
            IOptionsMonitor<AzureADOptions> azureAdOptionsAccessor)
        {
            if (option != null)
            {
                this.CacheOptions = new MSALMemoryTokenCacheOptions();
            }
            else
            {
                this.CacheOptions = option;
            }

            if (azureAdOptionsAccessor.CurrentValue == null && string.IsNullOrWhiteSpace(azureAdOptionsAccessor.CurrentValue.ClientId))
            {
                throw new ArgumentNullException(nameof(AzureADOptions), $"The app token cache needs {nameof(AzureADOptions)}, populated with clientId to initialize.");
            }

            this.AppId = azureAdOptionsAccessor.CurrentValue.ClientId;
            this.memoryCache = cache;
        }

        /// <summary>Initializes this instance of TokenCacheProvider with essentials to initialize themselves.</summary>
        /// <param name="tokenCache">The token cache instance of MSAL application</param>
        /// <param name="httpcontext">The Httpcontext whose Session will be used for caching.This is required by some providers.</param>
        public void Initialize(ITokenCache tokenCache, HttpContext httpcontext)
        {
            this.AppCacheId = this.AppId + "_AppTokenCache";

            tokenCache.SetBeforeAccess(this.AppTokenCacheBeforeAccessNotification);
            tokenCache.SetAfterAccess(this.AppTokenCacheAfterAccessNotification);
            tokenCache.SetBeforeWrite(this.AppTokenCacheBeforeWriteNotification);
        }

        /// <summary>
        /// if you want to ensure that no concurrent write take place, use this notification to place a lock on the entry
        /// </summary>
        /// <param name="args">Contains parameters used by the MSAL call accessing the cache.</param>
        private void AppTokenCacheBeforeWriteNotification(TokenCacheNotificationArgs args)
        {
            // Since we are using a MemoryCache ,whose methods are threads safe, we need not to do anything in this handler.
        }

        /// <summary>
        /// Clears the token cache for this app
        /// </summary>
        public void Clear()
        {
            this.memoryCache.Remove(this.AppCacheId);
        }

        /// <summary>
        /// Triggered right before MSAL needs to access the cache. Reload the cache from the persistence store in case it changed since the last access.
        /// </summary>
        /// <param name="args">Contains parameters used by the MSAL call accessing the cache.</param>
        private void AppTokenCacheBeforeAccessNotification(TokenCacheNotificationArgs args)
        {
            // Load the token cache from memory
            byte[] tokenCacheBytes = (byte[])this.memoryCache.Get(this.AppCacheId);
            args.TokenCache.DeserializeMsalV3(tokenCacheBytes, shouldClearExistingCache: true);
        }

        /// <summary>
        /// Triggered right after MSAL accessed the cache.
        /// </summary>
        /// <param name="args">Contains parameters used by the MSAL call accessing the cache.</param>
        private void AppTokenCacheAfterAccessNotification(TokenCacheNotificationArgs args)
        {
            // if the access operation resulted in a cache update
            if (args.HasStateChanged)
            {
                // Reflect changes in the persistence store
                this.memoryCache.Set(this.AppCacheId, args.TokenCache.SerializeMsalV3(), CacheOptions.SlidingExpiration);
            }
        }
    }
}