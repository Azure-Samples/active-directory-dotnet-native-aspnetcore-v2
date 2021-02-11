// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

// The same code for the controller is used in both chapters of the tutorial. 
// In the first chapter this is just a protected API (ENABLE_OBO is not set)
// In this chapter, the Web API calls a downstream API on behalf of the user (OBO)
#define ENABLE_OBO
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
    [RequiredScope("access_as_user")]
    public class TodoListController : Controller
    {
        static readonly ConcurrentBag<TodoItem> TodoStore = new ConcurrentBag<TodoItem>();

        // GET: api/values
        [HttpGet]
        public IEnumerable<TodoItem> Get()
        {
            string owner = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return TodoStore.Where(t => t.Owner == owner).ToList();
        }

        // POST api/values
        [HttpPost]
        public void Post([FromBody]TodoItem todo)
        {
            string owner = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            TodoStore.Add(new TodoItem { Owner = owner, Title = todo.Title });
        }
    }
}
