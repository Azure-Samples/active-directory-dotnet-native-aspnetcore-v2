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

using Microsoft.Identity.Client;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
// The following using statements were added for this sample.
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
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
        private static string aadInstance = ConfigurationManager.AppSettings["ida:AADInstance"];
        private static string tenant = ConfigurationManager.AppSettings["ida:Tenant"];
        private static string clientId = ConfigurationManager.AppSettings["ida:ClientId"];

        private static string authority = String.Format(CultureInfo.InvariantCulture, aadInstance, tenant);

        //
        // To authenticate to the To Do list service, the client needs to know the service's App ID URI.
        // To contact the To Do list service we need it's URL as well.
        //
        private static string todoListScope = ConfigurationManager.AppSettings["todo:TodoListScope"];
        private static string todoListBaseAddress = ConfigurationManager.AppSettings["todo:TodoListBaseAddress"];
        private static string[] scopes = new string[] { todoListScope };

        private HttpClient httpClient = new HttpClient();
        private PublicClientApplication app = null;

        // Button strings
        const string signInString = "Sign In";
        const string clearCacheString = "Clear Cache";

        public MainWindow()
        {
            InitializeComponent();
            app = new PublicClientApplication(clientId, authority, TokenCacheHelper.GetUserCache());
            GetTodoList();
        }

        private void GetTodoList()
        {
            GetTodoList(SignInButton.Content.ToString() != clearCacheString);
        }

        private async Task GetTodoList(bool isAppStarting)
        {
            var accounts = await app.GetAccountsAsync();
            if (!accounts.Any())
            {
                SignInButton.Content = signInString;
                return;
            }
            //
            // Get an access token to call the To Do service.
            //
            AuthenticationResult result = null;
            try
            {
                result = await app.AcquireTokenSilentAsync(scopes, accounts.FirstOrDefault());
                SignInButton.Content = clearCacheString;
                this.SetUserName(result.Account);
            }
            // There is no access token in the cache, so prompt the user to sign-in.
            catch (MsalUiRequiredException)
            {
                if (!isAppStarting)
                {
                    MessageBox.Show("Please sign in to view your To-Do list");
                    SignInButton.Content = signInString;
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

            // Once the token has been returned by ADAL, add it to the http authorization header, before making the call to access the To Do list service.
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", result.AccessToken);

            // Call the To Do list service.
            HttpResponseMessage response = await httpClient.GetAsync(todoListBaseAddress + "/api/todolist");

            if (response.IsSuccessStatusCode)
            {

                // Read the response and databind to the GridView to display To Do items.
                string s = await response.Content.ReadAsStringAsync();
                List<TodoItem> toDoArray = JsonConvert.DeserializeObject<List<TodoItem>>(s);

                TodoList.ItemsSource = toDoArray.Select(t => new { t.Title });
            }
            else
            {
                if (response.StatusCode == System.Net.HttpStatusCode.Forbidden && response.Headers.WwwAuthenticate.Any())
                {
                    await HandleChallengeFromWebApi(response, result.Account);
                }
                else
                {
                    await DisplayErrorMessage(response);
                }
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

        /// <summary>
        /// When the Web API needs consent, it can sent a 403 with information in the WWW-Authenticate header in 
        /// order to challenge the user
        /// </summary>
        /// <param name="response">HttpResonse received from the service</param>
        /// <returns></returns>
        private async Task HandleChallengeFromWebApi(HttpResponseMessage response, IAccount account)
        {
            const string msaTenantId = "9188040d-6c67-4c5b-b112-36a304b66dad";

            AuthenticationHeaderValue bearer = response.Headers.WwwAuthenticate.FirstOrDefault(v => v.Scheme == "Bearer");
            IEnumerable<string> parameters = bearer.Parameter.Split(',').Select(v => v.Trim());
            string clientId = GetParameter(parameters, "clientId");
            string claims = GetParameter(parameters, "claims");
            string[] scopes = GetParameter(parameters, "scopes")?.Split(',');
            string proposedAction = GetParameter(parameters, "proposedAction");

            string loginHint = account?.Username;
            string domainHint = account?.HomeAccountId.TenantId == msaTenantId ? "consumers" : "organizations";
            string extraQueryParameters = $"claims={claims}&domainHint={domainHint}";

            if (proposedAction == "forceRefresh")
            {
                // Removes the account, but then re-signs-in
                await app.RemoveAsync(account);
                var res = await app.AcquireTokenAsync(scopes, loginHint, UIBehavior.Consent, extraQueryParameters, new string[] { }, app.Authority);
            }
            else if (proposedAction == "consent")
            {
                PublicClientApplication pca = new PublicClientApplication(clientId);
                await pca.AcquireTokenAsync(scopes, loginHint, UIBehavior.Consent, extraQueryParameters, new string[] { }, pca.Authority);
            }
        }

        private static string GetParameter(IEnumerable<string> parameters, string parameterName)
        {
            int offset = parameterName.Length + 1;
            return parameters.FirstOrDefault(p => p.StartsWith($"{parameterName}="))?.Substring(offset)?.Trim('"');
        }

        private async void AddTodoItem(object sender, RoutedEventArgs e)
        {
            var accounts = await app.GetAccountsAsync();

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

            //
            // Get an access token to call the To Do service.
            //
            AuthenticationResult result = null;
            try
            {
                result = await app.AcquireTokenSilentAsync(scopes, accounts.FirstOrDefault());
                this.SetUserName(result.Account);
                UserName.Content = Properties.Resources.UserNotSignedIn;
            }
            // There is no access token in the cache, so prompt the user to sign-in.
            catch (MsalUiRequiredException)
            {
                MessageBox.Show("Please re-sign");
                SignInButton.Content = signInString;
            }
            catch (MsalException ex)
            {
                // An unexpected error occurred.
                string message = ex.Message;
                if (ex.InnerException != null)
                {
                    message += "Error Code: " + ex.ErrorCode + "Inner Exception : " + ex.InnerException.Message;
                }
                UserName.Content = Properties.Resources.UserNotSignedIn;

                return;
            }

            //
            // Call the To Do service.
            //

            // Once the token has been returned by ADAL, add it to the http authorization header, before making the call to access the To Do service.
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", result.AccessToken);

            // Forms encode Todo item, to POST to the todo list web api.
            TodoItem todoItem = new TodoItem() { Title = TodoText.Text };
            string json = JsonConvert.SerializeObject(todoItem);
            StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

            // Call the To Do list service.

            HttpResponseMessage response = await httpClient.PostAsync(todoListBaseAddress + "/api/todolist", content);

            if (response.IsSuccessStatusCode)
            {
                TodoText.Text = "";
                GetTodoList();
            }
            else
            {
                string failureDescription = await response.Content.ReadAsStringAsync();
                MessageBox.Show($"{response.ReasonPhrase}\n {failureDescription}", "An error occurred while posting to /api/todolist", MessageBoxButton.OK);
            }
        }

        private async void SignIn(object sender = null, RoutedEventArgs args = null)
        {
            var accounts = await app.GetAccountsAsync();

            // If there is already a token in the cache, clear the cache and update the label on the button.
            if (SignInButton.Content.ToString() == clearCacheString)
            {
                TodoList.ItemsSource = string.Empty;

                // clear the cache
                while (accounts.Any())
                {
                    await app.RemoveAsync(accounts.First());
                    accounts = await app.GetAccountsAsync();
                }
                // Also clear cookies from the browser control.
                SignInButton.Content = signInString;
                UserName.Content = Properties.Resources.UserNotSignedIn;
                return;
            }

            //
            // Get an access token to call the To Do list service.
            //
            AuthenticationResult result = null;
            try
            {
                // Force a sign-in (PromptBehavior.Always), as the ADAL web browser might contain cookies for the current user, and using .Auto
                // would re-sign-in the same user
                result = await app.AcquireTokenAsync(scopes, accounts.FirstOrDefault(), UIBehavior.SelectAccount, string.Empty, new string[] { "user.read" }, app.Authority);
                SignInButton.Content = clearCacheString;
                SetUserName(result.Account);
                GetTodoList();
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

                UserName.Content = Properties.Resources.UserNotSignedIn;

                return;
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
