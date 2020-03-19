using Microsoft.Identity.Client;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace TodoListClient
{
    class Program
    {
        /// <summary>
        /// Instance of Cloud
        /// </summary>
        private static readonly string AadInstance = ConfigurationManager.AppSettings["ida:AADInstance"];

        /// <summary>
        /// Tenant
        /// </summary>
        private static readonly string Tenant = ConfigurationManager.AppSettings["ida:Tenant"];

        /// <summary>
        /// ClientID of the application
        /// </summary>
        private static readonly string ClientId = ConfigurationManager.AppSettings["ida:ClientId"];

        /// <summary>
        /// Authority
        /// </summary>
        private static readonly string Authority = string.Format(CultureInfo.InvariantCulture, AadInstance, Tenant);

        /// <summary>
        /// Scope of the TodoList action
        /// </summary>
        private static readonly string TodoListScope = ConfigurationManager.AppSettings["todo:TodoListScope"];

        /// <summary>
        /// Base address of the todolist Web API
        /// </summary>
        private static readonly string TodoListBaseAddress = ConfigurationManager.AppSettings["todo:TodoListBaseAddress"];
        private static readonly string[] Scopes = { TodoListScope };
        private static string TodoListApiAddress
        {
            get
            {
                string baseAddress = TodoListBaseAddress;
                return baseAddress.EndsWith("/") ? TodoListBaseAddress + "api/todolist"
                                                 : TodoListBaseAddress + "/api/todolist";
            }
        }

        static async Task Main(string[] args)
        {
            var app = PublicClientApplicationBuilder.Create(ClientId)
                .WithAuthority(Authority)
                .WithDefaultRedirectUri()
                .WithExperimentalFeatures() // Needed for PoP
                .Build();

            AuthenticationResult result;

            HttpClient httpClient = new HttpClient();
            HttpResponseMessage response;

            while (true)
            {
                Console.WriteLine("Enter an item");
                string itemString;
                itemString = Console.ReadLine();
                IAccount account = (await app.GetAccountsAsync()).FirstOrDefault();

                HttpRequestMessage writeRequest = new HttpRequestMessage(HttpMethod.Post, new Uri(TodoListApiAddress));
                try
                {
                    result = await app.AcquireTokenSilent(Scopes, account)
                                        .WithProofOfPosession(writeRequest)
                                        .ExecuteAsync();
                }
                catch(MsalUiRequiredException)
                {
                    result = await app.AcquireTokenInteractive(Scopes)
                                         .WithProofOfPosession(writeRequest)
                                         .ExecuteAsync();
                    account = result.Account;
                }
 
                await WriteItem(writeRequest, httpClient, itemString);


                HttpRequestMessage readRequest = new HttpRequestMessage(HttpMethod.Get, new Uri(TodoListApiAddress));
                result = await app.AcquireTokenSilent(Scopes, account)
                                    .WithProofOfPosession(readRequest)
                                    .ExecuteAsync();

                Console.WriteLine("Items");
                response = await httpClient.SendAsync(readRequest);
                if (response.IsSuccessStatusCode)
                {
                    await ReadAndDisplayList(response);
                }
            }
        }

        private static async Task WriteItem(HttpRequestMessage writeRequest, HttpClient httpClient, string itemString)
        {
            TodoItem todoItem = new TodoItem() { Title = itemString };
            string json = JsonConvert.SerializeObject(todoItem);
            StringContent content = new StringContent(json, Encoding.UTF8, "application/json");
            writeRequest.Content = content;

            await httpClient.SendAsync(writeRequest);
        }

        private static async Task ReadAndDisplayList(HttpResponseMessage response)
        {
            // Read the response and data-bind to the GridView to display To Do items.
            string s = await response.Content.ReadAsStringAsync();
            List<TodoItem> toDoArray = JsonConvert.DeserializeObject<List<TodoItem>>(s);
            foreach (TodoItem item in toDoArray)
            {
                Console.WriteLine(item?.Title);
            }
        }
    }
}
