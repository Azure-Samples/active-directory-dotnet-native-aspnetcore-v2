/*
The MIT License (MIT)

Copyright (c) 2015 Microsoft Corporation

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

using System.Security.Claims;

namespace Microsoft.Identity.Web
{
    public static class ClaimsPrincipalExtension
    {
        /// <summary>
        /// Gets the Tenant ID associated with the <see cref="ClaimsPrincipal"/>
        /// </summary>
        /// <param name="claimsPrincipal">the <see cref="ClaimsPrincipal"/> from which to retrieve the tenant id</param>
        /// <returns>Tenant ID of the identity, or <c>null</c> if it cannot be found</returns>
        public static string GetTenantId(this ClaimsPrincipal claimsPrincipal)
        {
            string tenantId = claimsPrincipal.FindFirstValue(ClaimConstants.Tid);
            if (string.IsNullOrEmpty(tenantId))
                tenantId = claimsPrincipal.FindFirstValue(ClaimConstants.TenantId);

            return tenantId;
        }

        /// <summary>
        /// Gets the login-hint associated with a <see cref="ClaimsPrincipal"/>
        /// </summary>
        /// <param name="claimsPrincipal">Identity for which to complete the login-hint</param>
        /// <returns>login-hint for the identity, or <c>null</c> if it cannot be found</returns>
        public static string GetLoginHint(this ClaimsPrincipal claimsPrincipal)
        {
            return GetDisplayName(claimsPrincipal);
        }

        /// <summary>
        /// Gets the domain-hint associated with an identity
        /// </summary>
        /// <param name="claimsPrincipal">Identity for which to compute the domain-hint</param>
        /// <returns>domain-hint for the identity, or <c>null</c> if it cannot be found</returns>
        public static string GetDomainHint(this ClaimsPrincipal claimsPrincipal)
        {
            // Tenant for MSA accounts
            const string msaTenantId = "9188040d-6c67-4c5b-b112-36a304b66dad";

            var tenantId = GetTenantId(claimsPrincipal);
            return string.IsNullOrWhiteSpace(tenantId) ? null : tenantId == msaTenantId ? "consumers" : "organizations";
        }

        /// <summary>
        /// Get the display name for the signed-in user, from the <see cref="ClaimsPrincipal"/>
        /// </summary>
        /// <param name="claimsPrincipal">Claims about the user/account</param>
        /// <returns>A string containing the display name for the user, as brought by Azure AD (v1.0) and Microsoft identity platform (v2.0) tokens,
        /// or <c>null</c> if the claims cannot be found</returns>
        /// <remarks>See https://docs.microsoft.com/en-us/azure/active-directory/develop/id-tokens#payload-claims </remarks>
        public static string GetDisplayName(this ClaimsPrincipal claimsPrincipal)
        {
            // Use the claims in an Microsoft identity platform token first
            string displayName = claimsPrincipal.FindFirstValue(ClaimConstants.PreferredUserName);

            // Otherwise fall back to the claims in an Azure AD v1.0 token
            if (string.IsNullOrWhiteSpace(displayName))
            {
                displayName = claimsPrincipal.FindFirstValue(ClaimsIdentity.DefaultNameClaimType);
            }

            // Finally falling back to name
            if (string.IsNullOrWhiteSpace(displayName))
            {
                displayName = claimsPrincipal.FindFirstValue(ClaimConstants.Name);
            }

            return displayName;
        }
    }
}
