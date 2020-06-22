// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Identity.Future;
using Microsoft.Identity.Web.SignedHttpRequest;

namespace Microsoft.Identity.Web
{
    /// <summary>
    /// Extensions for IServiceCollection for startup initialization of Web APIs.
    /// </summary>
    public static class WebApiServiceCollectionExtensions
    {
        public static IServiceCollection AddProofOfPosession(
            this IServiceCollection services,
            IConfiguration configuration,
            string configSectionName = "AzureAd")
        {
            services.AddAuthentication(SignedHttpRequestDefaults.AuthenticationScheme)
                    .AddSignedHttpRequest(options => configuration.Bind(configSectionName, options));
            services.Configure<SignedHttpRequestOptions>(options => configuration.Bind(configSectionName, options));

            services.AddHttpContextAccessor();

            // Change the authentication configuration to accommodate the Microsoft identity platform endpoint (v2.0).
            services.Configure<SignedHttpRequestOptions>(SignedHttpRequestDefaults.AuthenticationScheme, options =>
            {
                options.Authority = options.Instance + options.Domain;

                // This is an Microsoft identity platform Web API
                options.Authority += "/v2.0";

                // The valid audiences are both the Client ID (options.Audience) and api://{ClientID}
                options.AccessTokenValidationParameters.ValidAudiences = new string[]
                {
                    options.ClientId, $"api://{options.ClientId}"
                };

                // Instead of using the default validation (validating against a single tenant, as we do in line of business apps),
                // we inject our own multi-tenant validation logic (which even accepts both v1.0 and v2.0 tokens)
                options.AccessTokenValidationParameters.IssuerValidator = AadIssuerValidator.GetIssuerValidator(options.Authority).Validate;
            });
                
            return services;
        }
    }
}