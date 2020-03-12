// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Microsoft.Extensions.Logging;

namespace Microsoft.Identity.Web.SignedHttpRequest
{
    internal static class SignedHttpRequestLoggingExtensions
    {
        private static Action<ILogger, Exception> _tokenValidationFailed;
        private static Action<ILogger, Exception> _tokenValidationSucceeded;
        private static Action<ILogger, Exception> _errorProcessingMessage;

        static SignedHttpRequestLoggingExtensions()
        {
            _tokenValidationFailed = LoggerMessage.Define(
                eventId: 1,
                logLevel: LogLevel.Information,
                formatString: "Failed to validate the token.");
            _tokenValidationSucceeded = LoggerMessage.Define(
                eventId: 2,
                logLevel: LogLevel.Information,
                formatString: "Successfully validated the token.");
            _errorProcessingMessage = LoggerMessage.Define(
                eventId: 3,
                logLevel: LogLevel.Error,
                formatString: "Exception occurred while processing message.");
        }

        public static void TokenValidationFailed(this ILogger logger, Exception ex)
            => _tokenValidationFailed(logger, ex);

        public static void TokenValidationSucceeded(this ILogger logger)
            => _tokenValidationSucceeded(logger, null);

        public static void ErrorProcessingMessage(this ILogger logger, Exception ex)
            => _errorProcessingMessage(logger, ex);
    }
}
