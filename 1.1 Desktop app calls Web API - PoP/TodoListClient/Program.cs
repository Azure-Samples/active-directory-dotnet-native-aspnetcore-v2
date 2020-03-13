using Microsoft.Identity.Client;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using TodoListClient;

namespace ConsoleApp1
{
    class Program
    {
        private static readonly string AadInstance = "https://login.microsoftonline.com/{0}/v2.0";
        private static readonly string Tenant = "msidentitysamplestesting.onmicrosoft.com";
        private static readonly string ClientId = "f9256f68-0a7a-4396-96c9-a900489b59f4";
        private static readonly string Authority = string.Format(CultureInfo.InvariantCulture, AadInstance, Tenant);
        private static readonly string TodoListScope = "api://ceb39196-5baf-4ac6-b398-ca548d0b2af7/access_as_user";
        private static readonly string TodoListBaseAddress = "https://localhost:44351/";
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
                .WithExperimentalFeatures()
                .Build();

            AuthenticationResult result;

//            Console.WriteLine($"Hello {result.Account.Username}");

            HttpClient httpClient = new HttpClient();
            HttpResponseMessage response;

            while (true)
            {
                Console.WriteLine("Enter an item");
                string itemString;
                itemString = Console.ReadLine();

                HttpRequestMessage writeRequest = new HttpRequestMessage(HttpMethod.Post, new Uri(TodoListApiAddress));
                result = await app.AcquireTokenInteractive(Scopes)
                                    .WithProofOfPosession(writeRequest)
                                    .ExecuteAsync();

                await WriteItem(writeRequest, httpClient, itemString);
                HttpRequestMessage readRequest = new HttpRequestMessage(HttpMethod.Get, new Uri(TodoListApiAddress));
                result = await app.AcquireTokenInteractive(Scopes)
                                    .WithProofOfPosession(readRequest)
                                    .ExecuteAsync();


                response = await httpClient.SendAsync(readRequest);
                if (response.IsSuccessStatusCode)
                {
                    await WriteList(response);
                }
            }
        }

        private static async Task WriteItem(HttpRequestMessage writeRequest, HttpClient httpClient, string itemString)
        {
            Console.WriteLine("Enter items");
            TodoItem todoItem = new TodoItem() { Title = itemString };
            string json = JsonConvert.SerializeObject(todoItem);
            StringContent content = new StringContent(json, Encoding.UTF8, "application/json");
            writeRequest.Content = content;

            await httpClient.SendAsync(writeRequest);
        }

        private static async Task WriteList(HttpResponseMessage response)
        {
            // Read the response and data-bind to the GridView to display To Do items.
            string s = await response.Content.ReadAsStringAsync();
            List<TodoItem> toDoArray = JsonConvert.DeserializeObject<List<TodoItem>>(s);
            foreach (TodoItem item in toDoArray)
            {
                Console.WriteLine(item);
            }
        }
    }
}
