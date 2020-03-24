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
            // Create the public client application (desktop app), with a default redirect URI
            // for these. Enable PoP
            var app = PublicClientApplicationBuilder.Create(ClientId)
                .WithAuthority(Authority)
                .WithDefaultRedirectUri()
                .WithExperimentalFeatures() // Needed for PoP
                .Build();

            // Enable a simple token cache serialiation so that the user does not need to
            // re-sign-in each time the application is run
            TokenCacheHelper.EnableSerialization(app.UserTokenCache);

            // Add an item to the list maintained in the Web API, and then list the items in the list
            HttpClient httpClient = new HttpClient();
            while (true)
            {
                await AddItemToList(app, httpClient);
                await WriteItemsInList(app, httpClient);
            }
        }

        /// <summary>
        /// Writes on the console the list of items which are maintained in the Web API
        /// </summary>
        /// <param name="app">application</param>
        /// <param name="httpClient">HttpClient used to communicate with the Web API</param>
        private async static Task WriteItemsInList(IPublicClientApplication app, HttpClient httpClient)
        {
            HttpRequestMessage readRequest = new HttpRequestMessage(HttpMethod.Get, new Uri(TodoListApiAddress));
            var account = (await app.GetAccountsAsync()).FirstOrDefault();

            await app.AcquireTokenSilent(Scopes, account)
                                .WithProofOfPosession(readRequest)
                                .ExecuteAsync();

            Console.WriteLine("Items");
            var response = await httpClient.SendAsync(readRequest);
            if (response.IsSuccessStatusCode)
            {
                await ReadItemsFromWebApiAndDisplayList(response);
            }
        }

        /// <summary>
        /// Reads an item on the console and sends it to the Web API
        /// </summary>
        /// <param name="app">application</param>
        /// <param name="httpClient">HttpClient used to communicate with the Web API</param>
        private async static Task AddItemToList(IPublicClientApplication app, HttpClient httpClient)
        {
            Console.WriteLine("Enter an item");
            string itemString;
            itemString = Console.ReadLine();
            var account = (await app.GetAccountsAsync()).FirstOrDefault();

            // Result is not strickly necessary here, but you might want to have a look at it
            // under debugger
            AuthenticationResult result;
            HttpRequestMessage writeRequest = new HttpRequestMessage(HttpMethod.Post, new Uri(TodoListApiAddress));
            try
            {
                result = await app.AcquireTokenSilent(Scopes, account)
                                    .WithProofOfPosession(writeRequest)
                                    .ExecuteAsync();
            }
            catch (MsalUiRequiredException)
            {
                result = await app.AcquireTokenInteractive(Scopes)
                                     .WithProofOfPosession(writeRequest)
                                     .ExecuteAsync();
            }
            await SendItemToWebAPI(writeRequest, httpClient, itemString);
        }

        /// <summary>
        /// Sends a new TodoList item to add to the list maintained by the Web API
        /// </summary>
        /// <param name="writeRequest">HttpRequest containing the item to write</param>
        /// <param name="httpClient">Http client used to communicate with the Web API</param>
        /// <param name="itemString">content of the item</param>
        /// <returns></returns>
        private static async Task SendItemToWebAPI(HttpRequestMessage writeRequest, HttpClient httpClient, string itemString)
        {
            TodoItem todoItem = new TodoItem() { Title = itemString };
            string json = JsonConvert.SerializeObject(todoItem);
            StringContent content = new StringContent(json, Encoding.UTF8, "application/json");
            writeRequest.Content = content;

            await httpClient.SendAsync(writeRequest);
        }

        /// <summary>
        /// Reads the HttpResponse
        /// </summary>
        /// <param name="response"></param>
        /// <returns></returns>
        private static async Task ReadItemsFromWebApiAndDisplayList(HttpResponseMessage response)
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
