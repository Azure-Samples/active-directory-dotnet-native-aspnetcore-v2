// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Microsoft.AspNetCore.Authentication;

namespace Microsoft.Identity.Web.SignedHttpRequest

{
    public static class SignedHttpRequestExtensions
    {
        public static AuthenticationBuilder AddSignedHttpRequest(this AuthenticationBuilder builder)
            => builder.AddSignedHttpRequest(_ => { });

        public static AuthenticationBuilder AddSignedHttpRequest(this AuthenticationBuilder builder, Action<SignedHttpRequestOptions> configureOptions)
             => builder.AddSignedHttpRequest(SignedHttpRequestDefaults.AuthenticationScheme, configureOptions);

        public static AuthenticationBuilder AddSignedHttpRequest(this AuthenticationBuilder builder, string authenticationScheme, Action<SignedHttpRequestOptions> configureOptions)
            => builder.AddSignedHttpRequest(authenticationScheme, displayName: null, configureOptions: configureOptions);

        public static AuthenticationBuilder AddSignedHttpRequest(this AuthenticationBuilder builder, string authenticationScheme, string displayName, Action<SignedHttpRequestOptions> configureOptions)
        {
            return builder.AddScheme<SignedHttpRequestOptions, SignedHttpRequestAuthenticationHandler>(authenticationScheme, displayName, configureOptions);
        }
    }
}
