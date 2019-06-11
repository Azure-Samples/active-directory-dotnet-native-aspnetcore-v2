using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Text;

namespace Microsoft.Identity.Web.Resource
{
    public static class ScopesRequiredByWebAPIExtension
    {
        /// <summary>
        /// When applied to an <see cref="HttpContext"/>, verifies that the user authenticated in the Web API has any of the
        /// accepted scopes. If the authentication user does not have any of these <paramref name="acceptedScopes"/>, the
        /// method throws an HTTP Unauthorized with the message telling which scope are missing
        /// </summary>
        /// <param name="acceptedScopes">Scopes accepted by this API</param>
        public static void VerifyUserHasAnyAcceptedScope(this HttpContext context, IEnumerable<string> acceptedScopes)
        {
            if (acceptedScopes == null)
            {
                throw new ArgumentNullException("acceptedScopes");
            }
            Claim scopeClaim = context.User.FindFirst("http://schemas.microsoft.com/identity/claims/scope");
            if (scopeClaim == null || !scopeClaim.Value.Split(' ').Intersect(acceptedScopes).Any())
            {
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                string message = $"The 'scope' claim does not contain scopes '{string.Join(",", acceptedScopes)}' or was not found";
                throw new HttpRequestException(message);
            }
        }

        /// <summary>
        /// When applied to a controller, verifies that the user on behalf of which the Web API is called
        /// has the required scope. If the authentication user does not have any of these <paramref name="requiredScope"/>, the
        /// method throws an HTTP Unauthorized with the message telling which scope are missing
        /// </summary>
        /// <param name="requiredScope">Scope required to call the API</param>
        public static void VerifyUserHasRequiredScope(this HttpContext context, string requiredScope)
        {
            if (requiredScope == null)
            {
                throw new ArgumentNullException("requireScope");
            }
            context.VerifyUserHasAnyAcceptedScope(new[] { requiredScope });
        }
    }
}
