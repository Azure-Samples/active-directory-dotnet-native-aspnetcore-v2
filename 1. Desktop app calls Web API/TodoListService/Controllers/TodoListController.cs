/*
 The MIT License (MIT)

Copyright (c) 2018 Microsoft Corporation

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
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web.Resource;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using TodoListService.Models;

namespace TodoListService.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    public class TodoListController : Controller
    {
        static readonly ConcurrentBag<TodoItem> TodoStore = new ConcurrentBag<TodoItem>();

        /// <summary>
        /// The Web API will only accept tokens 1) for users, 2) having the user_impersonation scope for
        /// this API (if you created the app using the App creation script) or 'access_as_user' if you created
        /// it following the README.md.
        /// </summary>
        static string[] scopeRequiredByAPI = new string[] { "user_impersonation", "access_as_user" };

        // GET: api/values
        [HttpGet]
        public IEnumerable<TodoItem> Get()
        {
            HttpContext.VerifyUserHasAnyAcceptedScope(scopeRequiredByAPI);
            string owner = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return TodoStore.Where(t => t.Owner == owner).ToList();
        }

        // POST api/values
        [HttpPost]
        public void Post([FromBody]TodoItem todo)
        {
            HttpContext.VerifyUserHasAnyAcceptedScope(scopeRequiredByAPI);
            string owner = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            TodoStore.Add(new TodoItem { Owner = owner, Title = todo.Title });
        }
    }
}
