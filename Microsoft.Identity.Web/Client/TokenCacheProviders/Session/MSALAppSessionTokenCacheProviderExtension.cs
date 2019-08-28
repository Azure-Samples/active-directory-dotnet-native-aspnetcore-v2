// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Authentication.AzureAD.UI;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Microsoft.Identity.Web.Client.TokenCacheProviders
{
    public static class MSALAppSessionTokenCacheProviderExtension
    {
        /// <summary>Adds both App and per-user session token caches.</summary>
        /// <param name="services">The services collection to add to.</param>
        /// <returns></returns>
        public static IServiceCollection AddSessionTokenCaches(this IServiceCollection services)
        {
            AddSessionAppTokenCache(services);
            AddSessionPerUserTokenCache(services);

            return services;
        }

        /// <summary>Adds the Http session based application token cache to the service collection.</summary>
        /// <param name="services">The services collection to add to.</param>
        /// <returns></returns>
        public static IServiceCollection AddSessionAppTokenCache(this IServiceCollection services)
        {
            services.AddHttpContextAccessor();
            services.AddScoped<IMSALAppTokenCacheProvider>(factory =>
            {
                return new MSALAppSessionTokenCacheProvider(factory.GetRequiredService<IOptionsMonitor<AzureADOptions>>(),
                                                            factory.GetRequiredService<IHttpContextAccessor>());
            });

            return services;
        }

        /// <summary>Adds the http session based per user token cache to the service collection.</summary>
        /// <param name="services">The services collection to add to.</param>
        /// <returns></returns>
        public static IServiceCollection AddSessionPerUserTokenCache(this IServiceCollection services)
        {
            services.AddHttpContextAccessor();
            services.AddScoped<IMSALUserTokenCacheProvider, MSALPerUserSessionTokenCacheProvider>();
            return services;
        }
    }
}