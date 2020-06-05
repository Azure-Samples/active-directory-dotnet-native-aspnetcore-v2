// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.TokenCacheProviders.InMemory;
using System;
using System.Linq;

namespace TodoListService
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddProtectedWebApi(options =>
            {
                Configuration.Bind("AzureAd", options);
                options.Events = new JwtBearerEvents();
                options.Events.OnTokenValidated = async context =>
                {
                    // This check is required to ensure that the Web API only accepts token from it's own client.
                    if (context.Principal.Claims.Any(x => (x.Type == "azp" || x.Type == "appid") && x.Value != Configuration["AzureAd:ClientId"]))
                    {
                        throw new UnauthorizedAccessException("Client is not authorized to access the resource.");
                    }
                    else
                    {
                        throw new UnauthorizedAccessException("Application ID claim does not exist for the client.");
                    }
                };
            }, 
            options =>
            {
                Configuration.Bind("AzureAd", options);
            })
                .AddProtectedWebApiCallsProtectedWebApi(Configuration)
                .AddInMemoryTokenCaches();

            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                // Since IdentityModel version 5.2.1 (or since Microsoft.AspNetCore.Authentication.JwtBearer version 2.2.0),
                // PII hiding in log files is enabled by default for GDPR concerns.
                // For debugging/development purposes, one can enable additional detail in exceptions by setting IdentityModelEventSource.ShowPII to true.
                // Microsoft.IdentityModel.Logging.IdentityModelEventSource.ShowPII = true;
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
