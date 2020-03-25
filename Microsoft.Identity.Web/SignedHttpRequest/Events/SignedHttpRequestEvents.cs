// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Threading.Tasks;

namespace Microsoft.Identity.Web.SignedHttpRequest.Events
{
    public class SignedHttpRequestEvents
    {
        /// <summary>
        /// Invoked if exceptions are thrown during request processing. The exceptions will be re-thrown after this event unless suppressed.
        /// </summary>
        public Func<SignedHttpRequestAuthenticationFailedContext, Task> OnAuthenticationFailed { get; set; } = context => Task.CompletedTask;

        /// <summary>
        /// Invoked when a protocol message is first received.
        /// </summary>
        public Func<SignedHttpRequestMessageReceivedContext, Task> OnMessageReceived { get; set; } = context => Task.CompletedTask;

        /// <summary>
        /// Invoked after the security token has passed validation and a ClaimsIdentity has been generated.
        /// </summary>
        public Func<SignedHttpRequestValidatedContext, Task> OnTokenValidated { get; set; } = context => Task.CompletedTask;

        public virtual Task AuthenticationFailed(SignedHttpRequestAuthenticationFailedContext context) => OnAuthenticationFailed(context);

        public virtual Task MessageReceived(SignedHttpRequestMessageReceivedContext context) => OnMessageReceived(context);

        public virtual Task TokenValidated(SignedHttpRequestValidatedContext context) => OnTokenValidated(context);
    }
}
