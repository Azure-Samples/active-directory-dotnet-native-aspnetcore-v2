// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Http;
using Microsoft.Identity.Client;
using System;
using System.Diagnostics;
using System.Security.Claims;
using System.Threading;

namespace Microsoft.Identity.Web.Client.TokenCacheProviders
{
    /// <summary>
    /// This is a MSAL's TokenCache implementation for one user. It uses Http session as a back end store
    /// </summary>
    public class MSALPerUserSessionTokenCacheProvider : IMSALUserTokenCacheProvider
    {
        private static ReaderWriterLockSlim SessionLock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);

        /// <summary>
        /// The HTTP context being used by this app
        /// </summary>
        internal HttpContext HttpContext { get { return _httpContextAccessor.HttpContext; } }

        IHttpContextAccessor _httpContextAccessor;

        /// <summary>
        /// Initializes a new instance of the <see cref="MSALPerUserSessionTokenCache"/> class.
        /// </summary>
        public MSALPerUserSessionTokenCacheProvider(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>Initializes the cache instance</summary>
        /// <param name="tokenCache">The <see cref="ITokenCache"/> passed through the constructor</param>
        /// <param name="httpcontext">The current <see cref="HttpContext" /></param>
        /// <param name="user">The signed in user's ClaimPrincipal, could be null.
        /// If the calling app has it available, then it should pass it themselves.</param>
        public void Initialize(ITokenCache tokenCache, HttpContext httpcontext, ClaimsPrincipal user)
        {
            if (tokenCache == null)
            {
                throw new ArgumentNullException(nameof(tokenCache));
            }
            tokenCache.SetBeforeAccess(UserTokenCacheBeforeAccessNotification);
            tokenCache.SetAfterAccess(UserTokenCacheAfterAccessNotification);
            tokenCache.SetBeforeWrite(UserTokenCacheBeforeWriteNotification);
        }

        /// <summary>
        /// Clears the TokenCache's copy of this user's cache.
        /// </summary>
        public void Clear(string accountId)
        {
            string cacheKey = accountId;

            SessionLock.EnterWriteLock();
            try
            {
                Debug.WriteLine($"INFO: Clearing session {HttpContext.Session.Id}, cacheId {cacheKey}");

                // Reflect changes in the persistent store
                HttpContext.Session.Remove(cacheKey);
                HttpContext.Session.CommitAsync().Wait();
            }
            finally
            {
                SessionLock.ExitWriteLock();
            }
        }

        /// <summary>
        /// If you want to ensure that no concurrent write takes place, use this notification to place a lock on the entry
        /// </summary>
        /// <param name="args">Contains parameters used by the MSAL call accessing the cache.</param>
        private void UserTokenCacheBeforeWriteNotification(TokenCacheNotificationArgs args)
        {
            // Since we obtain and release lock right before and after we read the Http session, we need not do anything here.
        }

        /// <summary>
        /// Triggered right after MSAL accessed the cache.
        /// </summary>
        /// <param name="args">Contains parameters used by the MSAL call accessing the cache.</param>
        private void UserTokenCacheAfterAccessNotification(TokenCacheNotificationArgs args)
        {
            // If the access operation resulted in a cache update
            if (args.HasStateChanged)
            {
                string cacheKey = _httpContextAccessor.HttpContext.Request.Headers["Authorization"];

                if (string.IsNullOrWhiteSpace(cacheKey))
                {
                    return;
                }

                SessionLock.EnterWriteLock();
                try
                {
                    Debug.WriteLine($"INFO: Serializing session {HttpContext.Session.Id}, cacheId {cacheKey}");

                    // Reflect changes in the persistent store
                    byte[] blob = args.TokenCache.SerializeMsalV3();
                    HttpContext.Session.Set(cacheKey, blob);
                    HttpContext.Session.CommitAsync().Wait();
                }
                finally
                {
                    SessionLock.ExitWriteLock();
                }
            }
        }

        /// <summary>
        /// Triggered right before MSAL needs to access the cache. Reload the cache from the persistence store in case it changed since the last access.
        /// </summary>
        /// <param name="args">Contains parameters used by the MSAL call accessing the cache.</param>
        private void UserTokenCacheBeforeAccessNotification(TokenCacheNotificationArgs args)
        {
            HttpContext.Session.LoadAsync().Wait();
            string cacheKey = _httpContextAccessor.HttpContext.Request.Headers["Authorization"];

            if (string.IsNullOrWhiteSpace(cacheKey))
            {
                return;
            }

            SessionLock.EnterReadLock();
            try
            {
                if (HttpContext.Session.TryGetValue(cacheKey, out byte[] blob))
                {
                    Debug.WriteLine($"INFO: Deserializing session {HttpContext.Session.Id}, cacheId {cacheKey}");
                    args.TokenCache.DeserializeMsalV3(blob, shouldClearExistingCache: true);
                }
                else
                {
                    Debug.WriteLine($"INFO: cacheId {cacheKey} not found in session {HttpContext.Session.Id}");
                }
            }
            finally
            {
                SessionLock.ExitReadLock();
            }
        }
    }
}