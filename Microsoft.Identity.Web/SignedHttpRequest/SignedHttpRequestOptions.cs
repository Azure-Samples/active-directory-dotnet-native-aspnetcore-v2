// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Net.Http;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Identity.Web.SignedHttpRequest.Events;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Protocols.SignedHttpRequest;
using Microsoft.IdentityModel.Tokens;

namespace Microsoft.Identity.Web.SignedHttpRequest
{
    public class SignedHttpRequestOptions : AuthenticationSchemeOptions
    {
        /// <summary>
        /// Gets or sets if HTTPS is required for the metadata address or authority.
        /// The default is true. This should be disabled only in development environments.
        /// </summary>
        public bool RequireHttpsMetadata { get; set; } = true;

        /// <summary>
        /// Gets or sets the discovery endpoint for obtaining metadata
        /// </summary>
        public string MetadataAddress { get; set; }

        /// <summary>
        /// Gets or sets the Authority to use when making OpenIdConnect calls.
        /// </summary>
        public string Authority { get; set; }

        /// <summary>
        /// Gets or sets a single valid audience value for any received OpenIdConnect token.
        /// This value is passed into TokenValidationParameters.ValidAudience if that property is empty.
        /// </summary>
        /// <value>
        /// The expected audience for any received OpenIdConnect token.
        /// </value>
        public string ClientId { get; set; }

        /// <summary>
        /// The object provided by the application to process events raised by the bearer authentication handler.
        /// The application may implement the interface fully, or it may create an instance of JwtBearerEvents
        /// and assign delegates only to the events it wants to process.
        /// </summary>
        public new SignedHttpRequestEvents Events
        {
            get { return (SignedHttpRequestEvents)base.Events; }
            set { base.Events = value; }
        }

        /// <summary>
        /// The HttpMessageHandler used to retrieve metadata.
        /// This cannot be set at the same time as BackchannelCertificateValidator unless the value
        /// is a WebRequestHandler.
        /// </summary>
        public HttpMessageHandler BackchannelHttpHandler { get; set; }

        /// <summary>
        /// Gets or sets the timeout when using the backchannel to make an http call.
        /// </summary>
        public TimeSpan BackchannelTimeout { get; set; } = TimeSpan.FromMinutes(1);

        /// <summary>
        /// Configuration provided directly by the developer. If provided, then MetadataAddress and the Backchannel properties
        /// will not be used. This information should not be updated during request processing.
        /// </summary>
        public OpenIdConnectConfiguration Configuration { get; set; }

        /// <summary>
        /// Responsible for retrieving, caching, and refreshing the configuration from metadata.
        /// If not provided, then one will be created using the MetadataAddress and Backchannel properties.
        /// </summary>
        public IConfigurationManager<OpenIdConnectConfiguration> ConfigurationManager { get; set; }

        /// <summary>
        /// Gets or sets if a metadata refresh should be attempted after a SecurityTokenSignatureKeyNotFoundException. This allows for automatic
        /// recovery in the event of a signature key rollover. This is enabled by default.
        /// </summary>
        public bool RefreshOnIssuerKeyNotFound { get; set; } = true;

        /// <summary>
        /// Gets or sets the parameters used to validate the access token inside the signed http request.
        /// </summary>
        /// <remarks>Contains the types and definitions required for validating a token.</remarks>
        /// <exception cref="ArgumentNullException">if 'value' is null.</exception>
        public TokenValidationParameters AccessTokenValidationParameters { get; set; } = new TokenValidationParameters();

        public SignedHttpRequestValidationParameters SignedHttpRequestValidationParameters { get; set; } = new SignedHttpRequestValidationParameters();

        /// <summary>
        /// 1 day is the default time interval that afterwards, <see cref="ConfigurationManager" /> will obtain new configuration.
        /// </summary>
        public TimeSpan AutomaticRefreshInterval { get; set; } = ConfigurationManager<OpenIdConnectConfiguration>.DefaultAutomaticRefreshInterval;

        /// <summary>
        /// The minimum time between <see cref="ConfigurationManager" /> retrievals, in the event that a retrieval failed, or that a refresh was explicitly requested. 30 seconds is the default.
        /// </summary>
        public TimeSpan RefreshInterval { get; set; } = ConfigurationManager<OpenIdConnectConfiguration>.DefaultRefreshInterval;
    }
}
