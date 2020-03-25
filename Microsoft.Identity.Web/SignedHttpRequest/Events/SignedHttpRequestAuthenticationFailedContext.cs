// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;

namespace Microsoft.Identity.Web.SignedHttpRequest.Events
{
    public class SignedHttpRequestAuthenticationFailedContext : ResultContext<SignedHttpRequestOptions>
    {
        public SignedHttpRequestAuthenticationFailedContext(
            HttpContext context,
            AuthenticationScheme scheme,
            SignedHttpRequestOptions options)
            : base(context, scheme, options) { }

        public Exception Exception { get; set; }
    }
}
