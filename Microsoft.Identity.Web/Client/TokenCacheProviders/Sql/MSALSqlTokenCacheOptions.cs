// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Identity.Web.Client.TokenCacheProviders
{
    /// <summary>
    /// MSAL's Sql token cache options
    /// </summary>
    public class MSALSqlTokenCacheOptions
    {
        /// <summary>
        /// Gets or sets the SQL connection string to the token cache database.
        /// </summary>
        public string SqlConnectionString
        {
            get;
        }

        /// <summary>
        /// Gets or sets the clientId of the application for whom this token cache instance is being created. (Optional)
        /// </summary>
        public string ClientId
        {
            get;
            set;
        }

        /// <summary>Initializes a new instance of the <see cref="MSALSqlTokenCacheOptions"/> class.</summary>
        /// <param name="sqlConnectionString">the SQL connection string to the token cache database.</param>
        public MSALSqlTokenCacheOptions(string sqlConnectionString) :
            this(sqlConnectionString, string.Empty)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="MSALSqlTokenCacheOptions"/> class.</summary>
        /// <param name="sqlConnectionString">The SQL connection string.</param>
        /// <param name="clientId">The the clientId of the application for whom this token cache instance is being created. (Optional for User cache).</param>
        public MSALSqlTokenCacheOptions(string sqlConnectionString, string clientId)
        {
            this.SqlConnectionString = sqlConnectionString;
            this.ClientId = clientId;
        }
    }
}