// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Identity.Client;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
// The following using statements were added for this sample.
using System.Globalization;
using System.IdentityModel.Protocols.WSTrust;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Windows;

namespace TodoListClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //
        // The Client ID is used by the application to uniquely identify itself to Azure AD.
        // The Tenant is the name of the Azure AD tenant in which this application is registered.
        // The AAD Instance is the instance of Azure, for example public Azure or Azure China.
        // The Redirect URI is the URI where Azure AD will return OAuth responses.
        // The Authority is the sign-in URL of the tenant.
        //
        private static readonly string AadInstance = ConfigurationManager.AppSettings["ida:AADInstance"];
        private static readonly string Tenant = ConfigurationManager.AppSettings["ida:Tenant"];
        private static readonly string ClientId = ConfigurationManager.AppSettings["ida:ClientId"];

        private static readonly string Authority = string.Format(CultureInfo.InvariantCulture, AadInstance, Tenant);

        // To authenticate to the To Do list service, the client needs to know the service's App ID URI and URL

        private static readonly string TodoListScope = ConfigurationManager.AppSettings["todo:TodoListScope"];
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

        private readonly HttpClient _httpClient = new HttpClient();
        private readonly IPublicClientApplication _app;

        // Button content
        const string SignInString = "Sign In";
        const string ClearCacheString = "Clear Cache";

        public MainWindow()
        {
            InitializeComponent();
            _app = PublicClientApplicationBuilder.Create(ClientId)
                .WithAuthority(Authority)
                .WithDefaultRedirectUri()
                .WithExperimentalFeatures()
                .Build();

            TokenCacheHelper.EnableSerialization(_app.UserTokenCache);
            GetTodoList();
        }

        private void GetTodoList()
        {
            GetTodoList(SignInButton.Content.ToString() != ClearCacheString).ConfigureAwait(false);
        }

        private async Task GetTodoList(bool isAppStarting)
        {
            var accounts = (await _app.GetAccountsAsync()).ToList();
            if (!accounts.Any())
            {
                SignInButton.Content = SignInString;
                return;
            }

            // Get an access token to call the To Do service.
            AuthenticationResult result = null;
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, new Uri(TodoListApiAddress));

            try
            {
                result = await _app.AcquireTokenSilent(Scopes, accounts.FirstOrDefault())
                    .ExecuteAsync()
                    .ConfigureAwait(false);

                Dispatcher.Invoke(
                    () =>
                    {
                        SignInButton.Content = ClearCacheString;
                        SetUserName(result.Account);
                    });
            }
            // There is no access token in the cache, so prompt the user to sign-in.
            catch (MsalUiRequiredException)
            {
                if (!isAppStarting)
                {
                    MessageBox.Show("Please sign in to view your To-Do list");
                    SignInButton.Content = SignInString;
                }
            }
            catch (MsalException ex)
            {
                // An unexpected error occurred.
                string message = ex.Message;
                if (ex.InnerException != null)
                {
                    message += "Error Code: " + ex.ErrorCode + "Inner Exception : " + ex.InnerException.Message;
                }
                MessageBox.Show(message);

                UserName.Content = Properties.Resources.UserNotSignedIn;
                return;
            }


            // Once the token has been returned by MSAL, add it to the http authorization header, before making the call to access the To Do list service.
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(result.CreateAuthorizationHeader());

            // Call the To Do list service.
            HttpResponseMessage response = await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                // Read the response and data-bind to the GridView to display To Do items.
                string s = await response.Content.ReadAsStringAsync();
                List<TodoItem> toDoArray = JsonConvert.DeserializeObject<List<TodoItem>>(s);

                Dispatcher.Invoke(() =>
                {
                    TodoList.ItemsSource = toDoArray.Select(t => new { t.Title });
                });
            }
            else
            {
                await DisplayErrorMessage(response);
            }
        }

        private static async Task DisplayErrorMessage(HttpResponseMessage httpResponse)
        {
            string failureDescription = await httpResponse.Content.ReadAsStringAsync();
            if (failureDescription.StartsWith("<!DOCTYPE html>"))
            {
                string path = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
                string errorFilePath = Path.Combine(path, "error.html");
                File.WriteAllText(errorFilePath, failureDescription);
                Process.Start(errorFilePath);
            }
            else
            {
                MessageBox.Show($"{httpResponse.ReasonPhrase}\n {failureDescription}", "An error occurred while getting /api/todolist", MessageBoxButton.OK);
            }
        }

        private async void AddTodoItem(object sender, RoutedEventArgs e)
        {
            var accounts = (await _app.GetAccountsAsync()).ToList();

            if (!accounts.Any())
            {
                MessageBox.Show("Please sign in first");
                return;
            }
            if (string.IsNullOrEmpty(TodoText.Text))
            {
                MessageBox.Show("Please enter a value for the To Do item name");
                return;
            }

            // Get an access token to call the To Do service.
            AuthenticationResult result = null;
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, new Uri(TodoListApiAddress));
            try
            {
                result = await _app.AcquireTokenSilent(Scopes, accounts.FirstOrDefault())
                    .ExecuteAsync()
                    .ConfigureAwait(false);

                Dispatcher.Invoke(() =>
                {
                    SetUserName(result.Account);
                    UserName.Content = Properties.Resources.UserNotSignedIn;
                });
            }
            // There is no access token in the cache, so prompt the user to sign-in.
            catch (MsalUiRequiredException)
            {
                MessageBox.Show("Please re-sign");
                SignInButton.Content = SignInString;
            }
            catch (MsalException ex)
            {
                // An unexpected error occurred.
                string message = ex.Message;
                if (ex.InnerException != null)
                {
                    message += "Error Code: " + ex.ErrorCode + "Inner Exception : " + ex.InnerException.Message;
                }

                Dispatcher.Invoke(() =>
               {
                   UserName.Content = Properties.Resources.UserNotSignedIn;
                   MessageBox.Show("Unexpected error: " + message);
               });


                return;
            }

            //
            // Call the To Do service.
            //

            // Once the token has been returned by MSAL, add it to the http authorization header, before making the call to access the To Do service.
            request.Headers.Add("Authorization", result.CreateAuthorizationHeader());

            // Forms encode Todo item, to POST to the todo list web api.
            TodoItem todoItem = new TodoItem() { Title = TodoText.Text };
            string json = JsonConvert.SerializeObject(todoItem);
            StringContent content = new StringContent(json, Encoding.UTF8, "application/json");
            request.Content = content;
            // Call the To Do list service.

            HttpResponseMessage response = await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                TodoText.Text = "";
                GetTodoList();
            }
            else
            {
                    await DisplayErrorMessage(response);
            }
        }

        private async void SignIn(object sender = null, RoutedEventArgs args = null)
        {
            var accounts = (await _app.GetAccountsAsync()).ToList();

            // If there is already a token in the cache, clear the cache and update the label on the button.
            if (SignInButton.Content.ToString() == ClearCacheString)
            {
                TodoList.ItemsSource = string.Empty;

                // clear the cache
                while (accounts.Any())
                {
                    await _app.RemoveAsync(accounts.First());
                    accounts = (await _app.GetAccountsAsync()).ToList();
                }
                // Also clear cookies from the browser control.
                SignInButton.Content = SignInString;
                UserName.Content = Properties.Resources.UserNotSignedIn;
            }

            // Get an access token to call the To Do list service.
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, TodoListApiAddress);
            try
            {
                var result = await _app.AcquireTokenSilent(Scopes, accounts.FirstOrDefault())
                    .ExecuteAsync()
                    .ConfigureAwait(false);

                Dispatcher.Invoke(() =>
                {
                    SignInButton.Content = ClearCacheString;
                    SetUserName(result.Account);
                    GetTodoList();
                }
                );
            }
            catch (MsalUiRequiredException)
            {
            try
            {
                    // Force a sign-in (Prompt.SelectAccount), as the MSAL web browser might contain cookies for the current user
                    // and we don't necessarily want to re-sign-in the same user
                    var result = await _app.AcquireTokenInteractive(Scopes)
                        .WithAccount(accounts.FirstOrDefault())
                        .WithProofOfPosession(request)
                        .WithPrompt(Prompt.SelectAccount)
                        .ExecuteAsync()
                        .ConfigureAwait(false);

                    Dispatcher.Invoke(() =>
                    {
                        SignInButton.Content = ClearCacheString;
                        SetUserName(result.Account);
                        GetTodoList();
                    }
                    );
                }
                catch (MsalException ex)
                {
                    if (ex.ErrorCode == "access_denied")
                    {
                        // The user canceled sign in, take no action.
                    }
                    else
                    {
                        // An unexpected error occurred.
                        string message = ex.Message;
                        if (ex.InnerException != null)
                        {
                            message += "Error Code: " + ex.ErrorCode + "Inner Exception : " + ex.InnerException.Message;
                        }

                    MessageBox.Show(message);
                }

                    Dispatcher.Invoke(() =>
                    {
                        UserName.Content = Properties.Resources.UserNotSignedIn;
                    });
                }
            }
        }

        // Set user name to text box
        private void SetUserName(IAccount userInfo)
        {
            string userName = null;

            if (userInfo != null)
            {
                userName = userInfo.Username;
            }

            if (userName == null)
                userName = Properties.Resources.UserNotIdentified;

            UserName.Content = userName;
        }
    }
}
