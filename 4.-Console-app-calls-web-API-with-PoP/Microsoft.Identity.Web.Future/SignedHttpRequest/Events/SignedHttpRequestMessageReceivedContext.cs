// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;

namespace Microsoft.Identity.Web.SignedHttpRequest.Events
{
    public class SignedHttpRequestMessageReceivedContext : ResultContext<SignedHttpRequestOptions>
    {
        public SignedHttpRequestMessageReceivedContext(
            HttpContext context,
            AuthenticationScheme scheme,
            SignedHttpRequestOptions options)
            : base(context, scheme, options) { }

        /// <summary>
        /// PoP token. This will give the application an opportunity to retrieve a token from an alternative location.
        /// </summary>
        public string Token { get; set; }
    }
}
