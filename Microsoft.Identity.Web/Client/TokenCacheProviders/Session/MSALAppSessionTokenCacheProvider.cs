// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Authentication.AzureAD.UI;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Client;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Identity.Web.Client.TokenCacheProviders
{
    /// <summary>
    /// An implementation of token cache for Confidential clients backed by Http session.
    /// </summary>
    /// <seealso cref="https://aka.ms/msal-net-token-cache-serialization"/>
    public class MSALAppSessionTokenCacheProvider : IMSALAppTokenCacheProvider
    {
        /// <summary>
        /// The application cache key
        /// </summary>
        internal string AppCacheId;

        /// <summary>
        /// The HTTP context being used by this app
        /// </summary>
        internal HttpContext HttpContext { get { return httpContextAccessor.HttpContext; } }

        /// <summary>
        /// HTTP context accessor
        /// </summary>
        internal IHttpContextAccessor httpContextAccessor;

        /// <summary>
        /// The duration till the tokens are kept in memory cache. In production, a higher value , upto 90 days is recommended.
        /// </summary>
        private readonly DateTimeOffset cacheDuration = DateTimeOffset.Now.AddHours(12);

        private static ReaderWriterLockSlim SessionLock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);

        /// <summary>
        /// The App's whose cache we are maintaining.
        /// </summary>
        private string AppId;

        /// <summary>Initializes a new instance of the <see cref="MSALAppSessionTokenCacheProvider"/> class.</summary>
        /// <param name="azureAdOptionsAccessor">The azure ad options accessor.</param>
        /// <exception cref="ArgumentNullException">AzureADOptions - The app token cache needs {nameof(AzureADOptions)}</exception>
        public MSALAppSessionTokenCacheProvider(IOptionsMonitor<AzureADOptions> azureAdOptionsAccessor, IHttpContextAccessor httpContextAccessor)
        {
            this.httpContextAccessor = httpContextAccessor;
            if (azureAdOptionsAccessor.CurrentValue == null && string.IsNullOrWhiteSpace(azureAdOptionsAccessor.CurrentValue.ClientId))
            {
                throw new ArgumentNullException(nameof(AzureADOptions), $"The app token cache needs {nameof(AzureADOptions)}, populated with clientId to initialize.");
            }

            AppId = azureAdOptionsAccessor.CurrentValue.ClientId;
        }

        /// <summary>Initializes this instance of TokenCacheProvider with essentials to initialize themselves.</summary>
        /// <param name="tokenCache">The token cache instance of MSAL application</param>
        /// <param name="httpcontext">The Httpcontext whose Session will be used for caching.This is required by some providers.</param>
        public void Initialize(ITokenCache tokenCache, HttpContext httpcontext)
        {
            AppCacheId = this.AppId + "_AppTokenCache";

            tokenCache.SetBeforeAccessAsync(this.AppTokenCacheBeforeAccessNotificationAsync);
            tokenCache.SetAfterAccessAsync(this.AppTokenCacheAfterAccessNotificationAsync);
            tokenCache.SetBeforeWrite(this.AppTokenCacheBeforeWriteNotification);
        }

        /// <summary>
        /// if you want to ensure that no concurrent write take place, use this notification to place a lock on the entry
        /// </summary>
        /// <param name="args">Contains parameters used by the MSAL call accessing the cache.</param>
        private void AppTokenCacheBeforeWriteNotification(TokenCacheNotificationArgs args)
        {
            // Since we are using a SessionCache ,whose methods are threads safe, we need not to do anything in this handler.
        }

        /// <summary>
        /// Clears the TokenCache's copy of this user's cache.
        /// </summary>
        public void Clear()
        {
            SessionLock.EnterWriteLock();
            try
            {
                Debug.WriteLine($"INFO: Clearing session {this.HttpContext.Session.Id}, cacheId {this.AppCacheId}");

                // Reflect changes in the persistent store
                this.HttpContext.Session.Remove(this.AppCacheId);
                this.HttpContext.Session.CommitAsync().Wait();
            }
            finally
            {
                SessionLock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Triggered right before MSAL needs to access the cache. Reload the cache from the persistence store in case it changed since the last access.
        /// </summary>
        /// <param name="args">Contains parameters used by the MSAL call accessing the cache.</param>
        private async Task AppTokenCacheBeforeAccessNotificationAsync(TokenCacheNotificationArgs args)
        {
            await this.HttpContext.Session.LoadAsync();

            SessionLock.EnterReadLock();
            try
            {
                byte[] blob;
                if (this.HttpContext.Session.TryGetValue(this.AppCacheId, out blob))
                {
                    Debug.WriteLine($"INFO: Deserializing session {this.HttpContext.Session.Id}, cacheId {this.AppCacheId}");
                    args.TokenCache.DeserializeMsalV3(blob, shouldClearExistingCache: true);
                }
                else
                {
                    Debug.WriteLine($"INFO: cacheId {this.AppCacheId} not found in session {this.HttpContext.Session.Id}");
                }
            }
            finally
            {
                SessionLock.ExitReadLock();
            }
        }

        /// <summary>
        /// Triggered right after MSAL accessed the cache.
        /// </summary>
        /// <param name="args">Contains parameters used by the MSAL call accessing the cache.</param>
        private async Task AppTokenCacheAfterAccessNotificationAsync(TokenCacheNotificationArgs args)
        {
            // if the access operation resulted in a cache update
            if (args.HasStateChanged)
            {
                SessionLock.EnterWriteLock();
                try
                {
                    Debug.WriteLine($"INFO: Serializing session {this.HttpContext.Session.Id}, cacheId {this.AppCacheId}");

                    // Reflect changes in the persistent store
                    byte[] blob = args.TokenCache.SerializeMsalV3();
                    HttpContext.Session.Set(this.AppCacheId, blob);
                    await HttpContext.Session.CommitAsync();
                }
                finally
                {
                    SessionLock.ExitWriteLock();
                }
            }
        }
    }
}