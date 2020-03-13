// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Web.SignedHttpRequest.Events;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Protocols.SignedHttpRequest;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;

namespace Microsoft.Identity.Web.SignedHttpRequest
{
    public class SignedHttpRequestAuthenticationHandler : AuthenticationHandler<SignedHttpRequestOptions>
    {
        private OpenIdConnectConfiguration _configuration;

        public SignedHttpRequestAuthenticationHandler(IOptionsMonitor<SignedHttpRequestOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock)
        : base(options, logger, encoder, clock)
        {
        }
        /// <summary>
        /// The handler calls methods on the events which give the application control at certain points where processing is occurring. 
        /// If it is not provided a default instance is supplied which does nothing when the methods are called.
        /// </summary>
        protected new SignedHttpRequestEvents Events
        {
            get { return (SignedHttpRequestEvents)base.Events; }
            set { base.Events = value; }
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            try
            {
                // Give application opportunity to find from a different location, adjust, or reject token
                var messageReceivedContext = new SignedHttpRequestMessageReceivedContext(Context, Scheme, Options);

                // event can set the token
                //await Events.MessageReceived(messageReceivedContext);
                if (messageReceivedContext.Result != null)
                {
                    return messageReceivedContext.Result;
                }

                // If application retrieved token from somewhere else, use that.
                string signedHttpRequest = messageReceivedContext.Token;

                if (string.IsNullOrEmpty(signedHttpRequest))
                {
                    string authorization = Request.Headers[HeaderNames.Authorization];

                    // If no authorization header found, nothing to process further
                    if (string.IsNullOrEmpty(authorization))
                    {
                        return AuthenticateResult.NoResult();
                    }

                    if (authorization.StartsWith(SignedHttpRequestConstants.AuthorizationHeaderSchemeName, StringComparison.OrdinalIgnoreCase))
                    {
                        signedHttpRequest = authorization.Substring((SignedHttpRequestConstants.AuthorizationHeaderSchemeName + " ").Length).Trim();
                    }

                    // If no token found, no further work possible
                    if (string.IsNullOrEmpty(signedHttpRequest))
                    {
                        return AuthenticateResult.NoResult();
                    }
                }

                if (_configuration == null && Options.ConfigurationManager != null)
                {
                    _configuration = await Options.ConfigurationManager.GetConfigurationAsync(Context.RequestAborted);
                }

                var validationParameters = Options.AccessTokenValidationParameters.Clone();
                if (_configuration != null)
                {
                    var issuers = new[] { _configuration.Issuer };
                    validationParameters.ValidIssuers = validationParameters.ValidIssuers?.Concat(issuers) ?? issuers;

                    validationParameters.IssuerSigningKeys = validationParameters.IssuerSigningKeys?.Concat(_configuration.SigningKeys)
                        ?? _configuration.SigningKeys;
                }

                var signedHttpRequestHandler = new SignedHttpRequestHandler();

                byte[] body = null;
                using (var ms = new MemoryStream(2048))
                {
                    await Request.Body.CopyToAsync(ms);
                    body = ms.ToArray();  // returns base64 encoded string JSON result
                }

                Dictionary<string, IEnumerable<string>> headers = new Dictionary<string, IEnumerable<string>>();

                foreach (var keyValuePair in Request.Headers)
                {
                    headers.Add(keyValuePair.Key, keyValuePair.Value.AsEnumerable());
                }
    
                var httpRequestData = new HttpRequestData()
                {
                    Method = Request.Method,
                    Uri = new Uri(UriHelper.GetEncodedUrl(Request)),
                    Body = body,
                    Headers = headers
                };
               
                var signedHttpRequestValidationContext = new SignedHttpRequestValidationContext(signedHttpRequest, httpRequestData, validationParameters, Options.SignedHttpRequestValidationParameters);
                SignedHttpRequestValidationResult signedHttpRequestValidationResult = null;
                
             
                signedHttpRequestValidationResult = await signedHttpRequestHandler.ValidateSignedHttpRequestAsync(signedHttpRequestValidationContext, CancellationToken.None).ConfigureAwait(false);


                if (!signedHttpRequestValidationResult.IsValid)
                {
                    Logger.TokenValidationFailed(signedHttpRequestValidationResult.Exception);
                    // Refresh the configuration for exceptions that may be caused by key rollovers. The user can also request a refresh in the event.
                    if (Options.RefreshOnIssuerKeyNotFound && Options.ConfigurationManager != null
                        && signedHttpRequestValidationResult.Exception is SecurityTokenSignatureKeyNotFoundException)
                    {
                        Options.ConfigurationManager.RequestRefresh();
                    }

                    var authenticationFailedContext = new SignedHttpRequestAuthenticationFailedContext(Context, Scheme, Options)
                    {
                        Exception = signedHttpRequestValidationResult.Exception
                    };

                    //await Events.AuthenticationFailed(authenticationFailedContext);
                    if (authenticationFailedContext.Result != null)
                    {
                        return authenticationFailedContext.Result;
                    }

                    return AuthenticateResult.Fail(authenticationFailedContext.Exception);
                }

                Logger.TokenValidationSucceeded();

                var principal = new ClaimsPrincipal(signedHttpRequestValidationResult.AccessTokenValidationResult.ClaimsIdentity);

                Request.HttpContext.Items[typeof(SignedHttpRequestValidationResult)] = signedHttpRequestValidationResult;
                var tokenValidatedContext = new SignedHttpRequestValidatedContext(Context, Scheme, Options)
                {
                    Principal = principal,
                    SignedHttpRequestValidationResult = signedHttpRequestValidationResult,
                };

                //await Events.TokenValidated(tokenValidatedContext);
                if (tokenValidatedContext.Result != null)
                {
                    return tokenValidatedContext.Result;
                }

                tokenValidatedContext.Success();
                return tokenValidatedContext.Result;

            }
            catch (Exception ex)
            {
                Logger.ErrorProcessingMessage(ex);

                var authenticationFailedContext = new SignedHttpRequestAuthenticationFailedContext(Context, Scheme, Options)
                {
                    Exception = ex
                };

                //await Events.AuthenticationFailed(authenticationFailedContext);
                if (authenticationFailedContext.Result != null)
                {
                    return authenticationFailedContext.Result;
                }

                throw;
            }
        }

        protected override async Task HandleChallengeAsync(AuthenticationProperties properties)
        {
          
        }
    }
}

