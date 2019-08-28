﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Client;
using System.Security.Claims;

namespace Microsoft.Identity.Web.Client.TokenCacheProviders
{
    /// <summary>
    /// An implementation of token cache for both Confidential and Public clients backed by MemoryCache.
    /// MemoryCache is useful in Api scenarios where there is no HttpContext.Session to cache data.
    /// </summary>
    /// <seealso cref="https://aka.ms/msal-net-token-cache-serialization"/>
    public class MSALPerUserMemoryTokenCacheProvider : IMSALUserTokenCacheProvider
    {
        /// <summary>
        /// The backing MemoryCache instance
        /// </summary>
        internal IMemoryCache memoryCache;

        private readonly MSALMemoryTokenCacheOptions CacheOptions;

        /// <summary>
        /// Enables the singleton object to access the right HttpContext
        /// </summary>
        private IHttpContextAccessor _httpContextAccessor;

        /// <summary>Initializes a new instance of the <see cref="MSALPerUserMemoryTokenCache"/> class.</summary>
        /// <param name="cache">The memory cache instance</param>
        public MSALPerUserMemoryTokenCacheProvider(IMemoryCache cache, MSALMemoryTokenCacheOptions option, IHttpContextAccessor httpContextAccessor)
        {
            memoryCache = cache;
            _httpContextAccessor = httpContextAccessor;

            if (option == null)
            {
                CacheOptions = new MSALMemoryTokenCacheOptions();
            }
            else
            {
                CacheOptions = option;
            }
        }

        /// <summary>Initializes this instance of TokenCacheProvider with essentials to initialize themselves.</summary>
        /// <param name="tokenCache">The token cache instance of MSAL application</param>
        /// <param name="httpcontext">The Httpcontext whose Session will be used for caching. This is required by some providers.</param>
        /// <param name="user">The signed-in user for whom the cache needs to be established. Not needed by all providers.</param>
        public void Initialize(ITokenCache tokenCache, HttpContext httpcontext, ClaimsPrincipal user)
        {
            tokenCache.SetBeforeAccess(UserTokenCacheBeforeAccessNotification);
            tokenCache.SetAfterAccess(UserTokenCacheAfterAccessNotification);
            tokenCache.SetBeforeWrite(UserTokenCacheBeforeWriteNotification);
        }

        /// <summary>
        /// Clears the TokenCache's copy of this user's cache.
        /// </summary>
        public void Clear(string accountId)
        {
            memoryCache.Remove(accountId);
        }

        /// <summary>
        /// Triggered right after MSAL accessed the cache.
        /// </summary>
        /// <param name="args">Contains parameters used by the MSAL call accessing the cache.</param>
        private void UserTokenCacheAfterAccessNotification(TokenCacheNotificationArgs args)
        {
            // if the access operation resulted in a cache update
            if (args.HasStateChanged)
            {
                string cacheKey = _httpContextAccessor.HttpContext.Request.Headers["Authorization"];

                if (string.IsNullOrWhiteSpace(cacheKey))
                {
                    return;
                }

                // Methods that load and persist should be thread safe.MemoryCache.Get() is thread safe.
                memoryCache.Set(cacheKey, args.TokenCache.SerializeMsalV3(), CacheOptions.SlidingExpiration);
            }
        }

        /// <summary>
        /// Triggered right before MSAL needs to access the cache. Reload the cache from the persistence store in case it
        /// changed since the last access.
        /// </summary>
        /// <param name="args">Contains parameters used by the MSAL call accessing the cache.</param>
        private void UserTokenCacheBeforeAccessNotification(TokenCacheNotificationArgs args)
        {
            string cacheKey = _httpContextAccessor.HttpContext.Request.Headers["Authorization"];

            if (string.IsNullOrWhiteSpace(cacheKey))
            {
                return;
            }

            byte[] tokenCacheBytes = (byte[])memoryCache.Get(cacheKey);
            args.TokenCache.DeserializeMsalV3(tokenCacheBytes, shouldClearExistingCache: true);
        }

        /// <summary>
        /// If you want to ensure that no concurrent write takes place, use this notification to place a lock on the entry
        /// </summary>
        /// <param name="args">Contains parameters used by the MSAL call accessing the cache.</param>
        private void UserTokenCacheBeforeWriteNotification(TokenCacheNotificationArgs args)
        {
        }
    }
}