/*
The MIT License (MIT)

Copyright (c) 2018 Microsoft Corporation

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

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Identity.Client;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Identity.Web.Client
{
    /// <summary>
    /// Filter used on a controller action to trigger an incremental consent.
    /// </summary>
    /// <example>
    /// The following controller action will trigger
    /// <code>
    /// [MsalUiRequiredExceptionFilter(Scopes = new[] {"Mail.Send"})]
    /// public async Task<IActionResult> SendEmail()
    /// {
    /// }
    /// </code>
    /// </example>
    public class MsalUiRequiredExceptionFilterAttribute : ExceptionFilterAttribute
    {
        public string[] Scopes { get; set; }

        public override void OnException(ExceptionContext context)
        {
            MsalUiRequiredException msalUiRequiredException = context.Exception as MsalUiRequiredException;
            if (msalUiRequiredException == null)
            {
                msalUiRequiredException = context.Exception?.InnerException as MsalUiRequiredException;
            }

            if (msalUiRequiredException != null)
            {
                if (CanBeSolvedByReSignInUser(msalUiRequiredException))
                {
                    var properties =
                        BuildAuthenticationPropertiesForIncrementalConsent(Scopes, msalUiRequiredException, context.HttpContext);
                    context.Result = new ChallengeResult(properties);
                }
            }

            base.OnException(context);
        }

        private bool CanBeSolvedByReSignInUser(MsalUiRequiredException ex)
        {
            // ex.ErrorCode != MsalUiRequiredException.UserNullError indicates a cache problem.
            // When calling an [Authenticate]-decorated controller we expect an authenticated
            // user and therefore its account should be in the cache. However in the case of an
            // InMemoryCache, the cache could be empty if the server was restarted. This is why
            // the null_user exception is thrown.

            return ex.ErrorCode == MsalError.UserNullError;
        }

        /// <summary>
        /// Build Authentication properties needed for an incremental consent.
        /// </summary>
        /// <param name="scopes">Scopes to request</param>
        /// <param name="ex">MsalUiRequiredException instance</param>
        /// <param name="context">current http context in the pipeline</param>
        /// <returns>AuthenticationProperties</returns>
        private AuthenticationProperties BuildAuthenticationPropertiesForIncrementalConsent(
            string[] scopes, MsalUiRequiredException ex, HttpContext context)
        {
            var properties = new AuthenticationProperties();

            // Set the scopes, including the scopes that ADAL.NET / MASL.NET need for the Token cache
            string[] additionalBuildInScopes =
                {OidcConstants.ScopeOpenId, OidcConstants.ScopeOfflineAccess, OidcConstants.ScopeProfile};
            properties.SetParameter<ICollection<string>>(OpenIdConnectParameterNames.Scope,
                                                         scopes.Union(additionalBuildInScopes).ToList());

            // Attempts to set the login_hint to avoid the logged-in user to be presented with an account selection dialog
            var loginHint = context.User.GetLoginHint();
            if (!string.IsNullOrWhiteSpace(loginHint))
            {
                properties.SetParameter(OpenIdConnectParameterNames.LoginHint, loginHint);

                var domainHint = context.User.GetDomainHint();
                properties.SetParameter(OpenIdConnectParameterNames.DomainHint, domainHint);
            }

            // Additional claims required (for instance MFA)
            if (!string.IsNullOrEmpty(ex.Claims))
            {
                properties.Items.Add(OidcConstants.AdditionalClaims, ex.Claims);
            }

            return properties;
        }
    }
}