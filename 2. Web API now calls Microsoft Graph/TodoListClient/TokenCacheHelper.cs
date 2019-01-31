//------------------------------------------------------------------------------
//
// Copyright (c) Microsoft Corporation.
// All rights reserved.
//
// This code is licensed under the MIT License.
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files(the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and / or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions :
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
//
//------------------------------------------------------------------------------

using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;

namespace TodoListClient
{
    /// <summary>
    /// File serializer for a token cache.
    /// </summary>
    public class TokenCacheFileSerializer
    {
        /// <summary>
        /// Ensure the persistence of a TokenCache to a file
        /// </summary>
        /// <param name="userTokenCache">User token cache for which to ensure the persistence to disk</param>
        /// <param name="cacheFilePath">Path on the disk where to persist the token cache</param>
        public void EnsurePersistence(ITokenCache userTokenCache, string cacheFilePath)
        {
            _userTokenCache = userTokenCache ?? throw new ArgumentNullException("userTokenCache");
            _userTokenCache.SetBeforeAccess(BeforeAccessNotification);
            _userTokenCache.SetAfterAccess(AfterAccessNotification);
            _cacheFilePath = cacheFilePath;
        }

        /// <summary>
        /// Token cache to persist in the file
        /// </summary>
        static ITokenCache _userTokenCache;

        /// <summary>
        /// Path of the file where the token cache is persisted
        /// </summary>
        private string _cacheFilePath;

        private void BeforeAccessNotification(TokenCacheNotificationArgs args)
        {
            args.TokenCache.Deserialize(File.Exists(_cacheFilePath)
                ? ProtectedData.Unprotect(File.ReadAllBytes(_cacheFilePath),
                                          null,
                                          DataProtectionScope.CurrentUser)
                : null);
        }

        private void AfterAccessNotification(TokenCacheNotificationArgs args)
        {
            // if the access operation resulted in a cache update
            if (args.HasStateChanged)
            {
                // reflect changes in the persistent store
                File.WriteAllBytes(_cacheFilePath,
                                   ProtectedData.Protect(args.TokenCache.Serialize(),
                                                         null,
                                                         DataProtectionScope.CurrentUser)
                                  );
            }
        }
    }
}
