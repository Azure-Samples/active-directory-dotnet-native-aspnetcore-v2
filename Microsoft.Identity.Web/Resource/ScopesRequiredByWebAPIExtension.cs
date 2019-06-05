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
        /// When applied to a Controller, verifies that the user authenticated in the Web API has any of the
        /// accepted scopes
        /// </summary>
        /// <param name="acceptedScopes">Scopes accepted by this API</param>
        public static void VerifyUserHasAnyAcceptedScope(this HttpContext context, IEnumerable<string> acceptedScopes)
        {
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
        /// has the required scope
        /// </summary>
        /// <param name="requireScope">Scope required to call the API</param>
        public static void VerifyUserHasRequiredScope(this HttpContext context, string requireScope)
        {
            context.VerifyUserHasAnyAcceptedScope(new[] { requireScope });
        }
    }
}
