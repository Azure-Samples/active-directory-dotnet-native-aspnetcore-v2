// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.AzureAD.UI;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Identity.Client;
using Microsoft.Identity.Web.Resource;
using Microsoft.Identity.Web.SignedHttpRequest;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace Microsoft.Identity.Web
{
    /// <summary>
    /// Extensions for IServiceCollection for startup initialization of Web APIs.
    /// </summary>
    public static class WebApiServiceCollectionExtensions
    {
        public static IServiceCollection AddPop(
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
            });
                
            return services;
        }
    }
}