// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.EntityFrameworkCore;

namespace Microsoft.Identity.Web.Client.TokenCacheProviders
{
    /// <summary>
    /// The DBContext that is used by the TokenCache providers to read and write to a Sql database.
    /// </summary>
    public class TokenCacheDbContext : DbContext
    {
        public TokenCacheDbContext(DbContextOptions<TokenCacheDbContext> options)
        : base(options)
        { }

        /// <summary>
        /// The app token cache table
        /// </summary>
        public DbSet<AppTokenCache> AppTokenCache { get; set; }

        /// <summary>
        /// The user token cache table
        /// </summary>
        public DbSet<UserTokenCache> UserTokenCache { get; set; }
    }
}