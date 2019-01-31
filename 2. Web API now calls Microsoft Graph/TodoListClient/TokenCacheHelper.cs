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
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;

namespace TodoListClient
{
    public class TokenCacheFileSerializer
    {
        /// <summary>
        /// Ensure the persistence of a TokenCache to a file
        /// </summary>
        /// <param name="userTokenCache">User token cache for which to ensure the persistence to disk</param>
        /// <param name="cacheFilePath">Path on the disk where to persist the token cache</param>
        public void EnsurePersistence(ITokenCache userTokenCache, string cacheFilePath)
        {
            if (_userTokenCache != null)
            {
                _userTokenCache = userTokenCache;
                _userTokenCache.SetBeforeAccess(BeforeAccessNotification);
                _userTokenCache.SetAfterAccess(AfterAccessNotification);
                _cacheFilePath = cacheFilePath;
                if (!_fileLockDictionary.ContainsKey(cacheFilePath))
                {
                    _fileLockDictionary.Add(cacheFilePath, new object());
                }
            }
        }

        static ITokenCache _userTokenCache;

        /// <summary>
        /// Path to the token cache
        /// </summary>
        private string _cacheFilePath;

        /// <summary>
        /// Dictionnary containing the locks for token file serialization
        /// </summary>
        private static readonly Dictionary<string, object> _fileLockDictionary = new Dictionary<string, object>();

        private void BeforeAccessNotification(TokenCacheNotificationArgs args)
        {
            lock (_fileLockDictionary[_cacheFilePath])
            {
                args.TokenCache.Deserialize(File.Exists(_cacheFilePath)
                    ? ProtectedData.Unprotect(File.ReadAllBytes(_cacheFilePath),
                                              null,
                                              DataProtectionScope.CurrentUser)
                    : null);
            }
        }

        private void AfterAccessNotification(TokenCacheNotificationArgs args)
        {
            // if the access operation resulted in a cache update
            if (args.HasStateChanged)
            {
                lock (_fileLockDictionary[_cacheFilePath])
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
}
