// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Protocols.SignedHttpRequest;
using Microsoft.IdentityModel.Tokens;

namespace Microsoft.Identity.Web.SignedHttpRequest.Events
{
    public class SignedHttpRequestValidatedContext : ResultContext<SignedHttpRequestOptions>
    {
        public SignedHttpRequestValidatedContext(
            HttpContext context,
            AuthenticationScheme scheme,
            SignedHttpRequestOptions options)
            : base(context, scheme, options) { }

        public SecurityToken SecurityToken { get; set; }

        public SignedHttpRequestValidationResult SignedHttpRequestValidationResult { get; set; }
    }
}
