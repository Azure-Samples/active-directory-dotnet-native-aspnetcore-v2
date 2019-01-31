using Microsoft.AspNetCore.Http;
using Microsoft.Identity.Client;
using System.Security.Claims;

namespace Microsoft.AspNetCore.Authentication
{
    /// <summary>
    /// Token cache provider service.
    /// </summary>
    public interface ITokenCachePersistenceProvider
    {
        /// <summary>
        /// Get an MSAL.NET Token cache from the HttpContext, and possibly the AuthenticationProperties and Cookies sign-in scheme
        /// </summary>
        /// <param name="httpContext">HttpContext</param>
        /// <param name="tokenCache">Token cache to serialize/deserialize for the given user</param>
        /// <param name="claimsPrincipal">Information about the user</param>
        /// <param name="authenticationProperties">Authentication properties (for the cookie based ccookie based token cache serialization). Can be <c>null</c>
        /// if you don't want to use cookie based token cache serialization</param>
        /// <param name="signInScheme">[Optional] Authentication properties (for the cookie based cookie based token cache serialization). Can be <c>null</c>
        /// if you don't want to use cookie based token cache serialization</param>
        void EnsurePersistence(HttpContext httpContext, ITokenCache tokenCache, ClaimsPrincipal claimsPrincipal, AuthenticationProperties authenticationProperties, string signInScheme = null);
    }
}
