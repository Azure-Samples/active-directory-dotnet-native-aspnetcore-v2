using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Identity.Client;
using System.Security.Claims;
using System.Threading;

namespace Microsoft.AspNetCore.Authentication
{
    /// <summary>
    /// Extension class enabling adding the CookieBasedTokenCache implentation service
    /// </summary>
    public static class SessionBasedTokenCacheExtension
    {
        /// <summary>
        /// Add the token acquisition service.
        /// </summary>
        /// <param name="services">Service collection</param>
        /// <returns>the service collection</returns>
        public static IServiceCollection AddSessionBasedTokenCache(this IServiceCollection services)
        {
            // Token acquisition service
            services.AddSingleton<ITokenCachePersistenceProvider, SessionBasedTokenCacheProvider>();
            return services;
        }
    }

    /// <summary>
    /// Provides an implementation of <see cref="ITokenCachePersistenceProvider"/> for a cookie based token cache implementation
    /// </summary>
    public class SessionBasedTokenCacheProvider : ITokenCachePersistenceProvider
    {
        private SessionTokenCacheHelper _helper;

        /// <summary>
        /// Get an MSAL.NET Token cache from the HttpContext, and possibly the AuthenticationProperties and Cookies sign-in scheme
        /// </summary>
        /// <param name="httpContext">HttpContext</param>
        /// <param name="tokenCache">Token cache to serialize/deserialize for the given user</param>
        /// <param name="claimsPrincipal">Information about the user</param>
        /// <param name="authenticationProperties">Authentication properties (for the cookie based ccookie based token cache serialization). Can be <c>null</c>
        /// if you don't want to use cookie based token cache serialization</param>
        /// <param name="signInScheme">[Optional] Authentication properties (for the cookie based cookie based token cache serialization). Can be <c>null</c>

        public void EnsurePersistence(HttpContext httpContext, ITokenCache tokenCache, ClaimsPrincipal claimsPrincipal, AuthenticationProperties authenticationProperties, string signInScheme)
        {
            string userId = claimsPrincipal.GetMsalAccountId();
            _helper = new SessionTokenCacheHelper(userId, tokenCache, httpContext);
        }
    }

    public class SessionTokenCacheHelper
    {
        private static readonly ReaderWriterLockSlim SessionLock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
        private readonly string _cacheId;
        private readonly ISession _session;

        private readonly ITokenCache _cache;

        public SessionTokenCacheHelper(string userId, ITokenCache tokenCache, HttpContext httpcontext)
        {
            _cache = tokenCache;
            _cacheId = userId + "_TokenCache";
            _session = httpcontext.Session;
            _cache.SetBeforeAccess(BeforeAccessNotification);
            _cache.SetAfterAccess(AfterAccessNotification);
            Load();
        }

        private void Load()
        {
            _session.LoadAsync().Wait();

            SessionLock.EnterReadLock();
            try
            {
                byte[] blob;
                if (_session.TryGetValue(_cacheId, out blob))
                {
                    _cache.Deserialize(blob);
                }
            }
            finally
            {
                SessionLock.ExitReadLock();
            }
        }

        private void Persist()
        {
            SessionLock.EnterWriteLock();

            try
            {
                // Reflect changes in the persistent store
                byte[] blob = _cache.Serialize();
                _session.Set(_cacheId, blob);
                _session.CommitAsync().Wait();
            }
            finally
            {
                SessionLock.ExitWriteLock();
            }
        }

        // Triggered right before MSAL needs to access the cache.
        // Reload the cache from the persistent store in case it changed since the last access.
        private void BeforeAccessNotification(TokenCacheNotificationArgs args)
        {
            Load();
        }

        // Triggered right after MSAL accessed the cache.
        private void AfterAccessNotification(TokenCacheNotificationArgs args)
        {
            // if the access operation resulted in a cache update
            if (args.HasStateChanged)
            {
                Persist();
            }
        }
    }
}
