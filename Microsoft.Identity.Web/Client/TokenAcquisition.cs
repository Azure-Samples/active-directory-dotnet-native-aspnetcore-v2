/*
The MIT License (MIT)

Copyright (c) 2015 Microsoft Corporation

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

using Microsoft.AspNetCore.Authentication.AzureAD.UI;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using Microsoft.Identity.Client;
using Microsoft.Identity.Web.Client.TokenCacheProviders;
using Microsoft.Net.Http.Headers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Microsoft.Identity.Web.Client
{
    /// <summary>
    /// Token acquisition service
    /// </summary>
    public class TokenAcquisition : ITokenAcquisition
    {
        private readonly AzureADOptions azureAdOptions;
        private ConfidentialClientApplicationOptions _applicationOptions;

        private readonly IMSALAppTokenCacheProvider AppTokenCacheProvider;
        private readonly IMSALUserTokenCacheProvider UserTokenCacheProvider;

        private IConfidentialClientApplication application;

        /// <summary>
        /// Constructor of the TokenAcquisition service. This requires the Azure AD Options to
        /// configure the confidential client application and a token cache provider.
        /// This constructor is called by ASP.NET Core dependency injection
        /// </summary>
        /// <param name="appTokenCacheProvider">The App token cache provider</param>
        /// <param name="userTokenCacheProvider">The User token cache provider</param>
        /// <param name="configuration"></param>
        public TokenAcquisition(IConfiguration configuration, IMSALAppTokenCacheProvider appTokenCacheProvider, IMSALUserTokenCacheProvider userTokenCacheProvider)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            azureAdOptions = new AzureADOptions();
            configuration.Bind("AzureAD", azureAdOptions);

            _applicationOptions = new ConfidentialClientApplicationOptions();
            configuration.Bind("AzureAD", _applicationOptions);

            AppTokenCacheProvider = appTokenCacheProvider;
            UserTokenCacheProvider = userTokenCacheProvider;
        }

        /// <summary>
        /// Scopes which are already requested by MSAL.NET. They should not be re-requested;
        /// </summary>
        private readonly string[] scopesRequestedByMsalNet = new string[]
        {
            OidcConstants.ScopeOpenId,
            OidcConstants.ScopeProfile,
            OidcConstants.ScopeOfflineAccess
        };

        /// <summary>
        /// In a Web App, adds, to the MSAL.NET cache, the account of the user authenticating to the Web App, when the authorization code is received (after the user
        /// signed-in and consented)
        /// An On-behalf-of token contained in the <see cref="AuthorizationCodeReceivedContext"/> is added to the cache, so that it can then be used to acquire another token on-behalf-of the
        /// same user in order to call to downstream APIs.
        /// </summary>
        /// <param name="context">The context used when an 'AuthorizationCode' is received over the OpenIdConnect protocol.</param>
        /// <example>
        /// From the configuration of the Authentication of the ASP.NET Core Web API:
        /// <code>OpenIdConnectOptions options;</code>
        ///
        /// Subscribe to the authorization code received event:
        /// <code>
        ///  options.Events = new OpenIdConnectEvents();
        ///  options.Events.OnAuthorizationCodeReceived = OnAuthorizationCodeReceived;
        /// }
        /// </code>
        ///
        /// And then in the OnAuthorizationCodeRecieved method, call <see cref="AddAccountToCacheFromAuthorizationCode"/>:
        /// <code>
        /// private async Task OnAuthorizationCodeReceived(AuthorizationCodeReceivedContext context)
        /// {
        ///   var tokenAcquisition = context.HttpContext.RequestServices.GetRequiredService<ITokenAcquisition>();
        ///    await _tokenAcquisition.AddAccountToCacheFromAuthorizationCode(context, new string[] { "user.read" });
        /// }
        /// </code>
        /// </example>
        public async Task AddAccountToCacheFromAuthorizationCode(AuthorizationCodeReceivedContext context, IEnumerable<string> scopes)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            if (scopes == null)
                throw new ArgumentNullException(nameof(scopes));

            try
            {
                // As AcquireTokenByAuthorizationCodeAsync is asynchronous we want to tell ASP.NET core that we are handing the code
                // even if it's not done yet, so that it does not concurrently call the Token endpoint. (otherwise there will be a
                // race condition ending-up in an error from Azure AD telling "code already redeemed")
                context.HandleCodeRedemption();

                // The cache will need the claims from the ID token. In the case of guest scenarios
                // If they are not yet in the HttpContext.User's claims, adding them.
                if (!context.HttpContext.User.Claims.Any())
                {
                    (context.HttpContext.User.Identity as ClaimsIdentity).AddClaims(context.Principal.Claims);
                }

                var application = GetOrBuildConfidentialClientApplication(context.HttpContext, context.Principal);

                // Do not share the access token with ASP.NET Core otherwise ASP.NET will cache it and will not send the OAuth 2.0 request in
                // case a further call to AcquireTokenByAuthorizationCodeAsync in the future for incremental consent (getting a code requesting more scopes)
                // Share the ID Token
                var result = await application.AcquireTokenByAuthorizationCode(scopes.Except(scopesRequestedByMsalNet), context.ProtocolMessage.Code)
                                              .ExecuteAsync();
                context.HandleCodeRedemption(null, result.IdToken);
            }
            catch (MsalException ex)
            {
                // brentsch - todo, write to a log
                Debug.WriteLine(ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Typically used from an ASP.NET Core Web App or Web API controller, this method gets an access token
        /// for a downstream API on behalf of the user account
        /// </summary>
        /// <param name="context">HttpContext associated with the Controller or auth operation</param>
        /// <param name="scopes">Scopes to request for the downstream API to call</param>
        /// <returns>An access token to call on behalf of the user, the downstream API characterized by its scopes</returns>
        public async Task<string> GetAccessTokenOnBehalfOfUser(HttpContext context, IEnumerable<string> scopes)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (scopes == null)
            {
                throw new ArgumentNullException(nameof(scopes));
            }

            try
            {
                // Use MSAL to get the right token to call the API
                var application = GetOrBuildConfidentialClientApplication(context, context.User);

                var accounts = await application.GetAccountsAsync();

                var result = await application.AcquireTokenSilent(
                    scopes.Except(scopesRequestedByMsalNet),
                    accounts.FirstOrDefault())
                    .ExecuteAsync();

                return result.AccessToken;
            }
            catch (MsalException)
            {
                string authorizationHeader = context.Request.Headers["Authorization"];

                if(!string.IsNullOrEmpty(authorizationHeader))
                {
                    string accessToken = authorizationHeader.Replace("Bearer ", "");

                    var result = await application.AcquireTokenOnBehalfOf(
                        scopes.Except(scopesRequestedByMsalNet),
                        new UserAssertion(accessToken))
                                        .ExecuteAsync();

                    return result.AccessToken;
                }
                else
                {
                    throw new InvalidOperationException(nameof(authorizationHeader));
                }
            }
        }

        /// <summary>
        /// Removes the account associated with context.HttpContext.User from the MSAL.NET cache
        /// </summary>
        /// <param name="context">RedirectContext passed-in to a <see cref="OnRedirectToIdentityProviderForSignOut"/>
        /// Openidconnect event</param>
        /// <returns></returns>
        public async Task RemoveAccount(RedirectContext context)
        {
            ClaimsPrincipal user = context.HttpContext.User;
            IConfidentialClientApplication app = GetOrBuildConfidentialClientApplication(context.HttpContext, user);
            IAccount account = await app.GetAccountAsync(context.HttpContext.Request.Headers["Authorization"]);

            // Workaround for the guest account
            if (account == null)
            {
                var accounts = await app.GetAccountsAsync();
                account = accounts.FirstOrDefault(a => a.Username == user.GetLoginHint());
            }

            if (account != null)
            {
                await app.RemoveAsync(account);

                UserTokenCacheProvider?.Clear(context.HttpContext.Request.Headers["Authorization"]);
            }
        }

        /// <summary>
        /// Creates an MSAL Confidential client application, if needed
        /// </summary>
        /// <param name="httpContext"></param>
        /// <param name="claimsPrincipal"></param>
        /// <returns></returns>
        private IConfidentialClientApplication GetOrBuildConfidentialClientApplication(HttpContext httpContext, ClaimsPrincipal claimsPrincipal)
        {
            if (application == null)
            {
                application = BuildConfidentialClientApplication(httpContext, claimsPrincipal);
            }
            return application;
        }

        /// <summary>
        /// Creates an MSAL Confidential client application
        /// </summary>
        /// <param name="httpContext"></param>
        /// <param name="claimsPrincipal"></param>
        /// <param name="authenticationProperties"></param>
        /// <param name="signInScheme"></param>
        /// <returns></returns>
        private IConfidentialClientApplication BuildConfidentialClientApplication(HttpContext httpContext, ClaimsPrincipal claimsPrincipal)
        {
            var request = httpContext.Request;
            string currentUri = UriHelper.BuildAbsolute(request.Scheme, request.Host, request.PathBase, azureAdOptions.CallbackPath ?? string.Empty);
            string authority = $"{azureAdOptions.Instance}{azureAdOptions.TenantId}/";

            var app = ConfidentialClientApplicationBuilder.CreateWithApplicationOptions(_applicationOptions)
               .WithRedirectUri(currentUri)
               .WithAuthority(authority)
               .Build();

            // Initialize token cache providers
            if (AppTokenCacheProvider != null)
            {
                AppTokenCacheProvider.Initialize(app.AppTokenCache, httpContext);
            }

            if (UserTokenCacheProvider != null)
            {
                UserTokenCacheProvider.Initialize(app.UserTokenCache, httpContext, claimsPrincipal);
            }

            return app;
        }

        /// <summary>
        /// Used in Web APIs (no user interaction).
        /// Replies to the client through the HttpResponse by sending a 403 (forbidden) and populating wwwAuthenticateHeaders so
        /// the client can trigger an interaction with the user. This way the user can consent to more scopes.
        /// </summary>
        /// <param name="httpContext">HttpContext</param>
        /// <param name="scopes">Scopes to consent to</param>
        /// <param name="msalServiceException"><see cref="MsalUiRequiredException"/> triggering the challenge</param>
        public void ReplyForbiddenWithWwwAuthenticateHeader(HttpContext httpContext, IEnumerable<string> scopes, MsalUiRequiredException msalServiceException)
        {
            // User interaction is required, 
            // we need to report back to the client through an wwww-Authenticate header https://tools.ietf.org/html/rfc6750#section-3.1
            string proposedAction = "consent";
            if (msalServiceException.ErrorCode == MsalError.InvalidGrantError)
            {
                if (AcceptedTokenVersionMismatch(msalServiceException))
                {
                    throw msalServiceException;
                }
            }

            IDictionary<string, string> parameters = new Dictionary<string, string>()
                {
                    { "clientId", azureAdOptions.ClientId },
                    { "claims", msalServiceException.Claims },
                    { "scopes", string.Join(",", scopes) },
                    { "proposedAction", proposedAction }
                };

            string parameterString = string.Join(", ", parameters.Select(p => $"{p.Key}=\"{p.Value}\""));
            string scheme = "Bearer";
            StringValues v = new StringValues($"{scheme} {parameterString}");

            //  StringValues v = new StringValues(new string[] { $"Bearer clientId=\"{jwtToken.Audiences.First()}\", claims=\"{ex.Claims}\", scopes=\" {string.Join(",", scopes)}\"" });
            var httpResponse = httpContext.Response;
            var headers = httpResponse.Headers;
            httpResponse.StatusCode = (int)HttpStatusCode.Forbidden;
            if (headers.ContainsKey(HeaderNames.WWWAuthenticate))
            {
                headers.Remove(HeaderNames.WWWAuthenticate);
            }
            headers.Add(HeaderNames.WWWAuthenticate, v);
        }

        private static bool AcceptedTokenVersionMismatch(MsalUiRequiredException msalSeviceException)
        {
            // Normally app developers should not make decisions based on the internal AAD code
            // however until the STS sends sub-error codes for this error, this is the only
            // way to distinguish the case.
            // This is subject to change in the future
            return (msalSeviceException.Message.Contains("AADSTS50013"));
        }
    }
}