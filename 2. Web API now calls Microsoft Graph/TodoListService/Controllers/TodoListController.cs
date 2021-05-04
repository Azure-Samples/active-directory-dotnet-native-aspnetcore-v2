// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

// The same code for the controller is used in both chapters of the tutorial. 
// In the first chapter this is just a protected API (ENABLE_OBO is not set)
// In this chapter, the Web API calls a downstream API on behalf of the user (OBO)
#define ENABLE_OBO
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.Graph;
using Microsoft.Identity.Client;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.Resource;
using TodoListService.Models;

namespace TodoListService.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [RequiredScope("access_as_user")]
    public class TodoListController : Controller
    {
        public TodoListController(ITokenAcquisition tokenAcquisition, GraphServiceClient graphServiceClient, IOptions<MicrosoftGraphOptions> graphOptions)
        {
            _tokenAcquisition = tokenAcquisition;
            _graphServiceClient = graphServiceClient;
            _graphOptions = graphOptions;
        }

        private readonly ITokenAcquisition _tokenAcquisition;
        private readonly GraphServiceClient _graphServiceClient;
        private readonly IOptions<MicrosoftGraphOptions> _graphOptions;

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
        public async void Post([FromBody] TodoItem todo)
        {
            string owner = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
#if ENABLE_OBO
            // This is a synchronous call, so that the clients know, when they call Get, that the 
            // call to the downstream API (Microsoft Graph) has completed.
            try
            {
                User user = _graphServiceClient.Me.Request().GetAsync().GetAwaiter().GetResult();
                string title = string.IsNullOrWhiteSpace(user.UserPrincipalName) ? todo.Title : $"{todo.Title} ({user.UserPrincipalName})";
                TodoStore.Add(new TodoItem { Owner = owner, Title = title });
            }
            catch (MsalException ex)
            {
                HttpContext.Response.ContentType = "text/plain";
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                await HttpContext.Response.WriteAsync("An authentication error occurred while acquiring a token for downstream API\n" + ex.ErrorCode + "\n" + ex.Message);
            }
            catch (Exception ex)
            {
                if (ex.InnerException is MicrosoftIdentityWebChallengeUserException challengeException)
                {
                    _tokenAcquisition.ReplyForbiddenWithWwwAuthenticateHeader(_graphOptions.Value.Scopes.Split(' '),
                        challengeException.MsalUiRequiredException);
                }
                else
                {
                    HttpContext.Response.ContentType = "text/plain";
                    HttpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    await HttpContext.Response.WriteAsync("An error occurred while calling the downstream API\n" + ex.Message);
                }
            }
#endif
        }
    }
}
