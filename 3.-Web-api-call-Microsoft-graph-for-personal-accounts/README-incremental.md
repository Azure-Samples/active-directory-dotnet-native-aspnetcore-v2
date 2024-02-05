---
services: active-directory
platforms: dotnet
author: Shama-K
level: 300
client: .NET Desktop (WPF)
service: ASP.NET Core Web API, Microsoft Graph
endpoint: Microsoft identity platform
---
# Sign-in users with Microsoft Personal accounts, using the Microsoft identity platform in a WPF Desktop application and call an ASP.NET Core Web API, which in turn calls Microsoft Graph

![Build badge](https://identitydivision.visualstudio.com/_apis/public/build/definitions/a7934fdd-dcde-4492-a406-7fad6ac00e17/497/badge)

## About this sample

### Table of content

- [About this sample](#about-this-sample)
  - [Scenario](#scenario)
  - [Overview](#overview)
  - [User experience when using this sample](#user-experience-when-using-this-sample)
- [How to run this sample](#how-to-run-this-sample)
  - [Step 1: In the downloaded folder](#step-1-in-the-downloaded-folder)
  - [Step 2: Update the Registration for the sample with your Microsoft Entra tenant](#step-2-update-the-registration-for-the-sample-with-your-azure-active-directory-tenant)
  - [Step 3:  Configure the sample to use your Microsoft Entra tenant](#step-3-configure-the-sample-to-use-your-azure-ad-tenant)
  - [Step 4: Run the sample](#step-4-run-the-sample)
- [How was the code created](#how-was-the-code-created)
- [How to deploy this sample to Azure](#how-to-deploy-this-sample-to-azure)
- [Community Help and Support](#community-help-and-support)
- [Contributing](#contributing)
- [More information](#more-information)

### Scenario

In the third chapter, we present another pattern where a tightly-knit client and Web API share the same client id(app id) and sign-in users with Microsoft Personal Accounts. The sign-in flow and the call to Web API uses the same flow as chapter 2.

The solution used in this sample is same as sample [2. Web API now calls Microsoft Graph](../2.%20Web%20API%20now%20calls%20Microsoft%20Graph).

### Overview

With respect to the previous chapter of the tutorial, this chapter adds the following steps:

3. When the client calls the Web API, the Web API acquires another token to call Microsoft Graph 
4. then the Web API calls Microsoft graph

![Topology](./ReadmeFiles/topology.png)

### User experience when using this sample

The user experience on the client application is similar to the one in the second chapter, except that, users with Microsoft Personal Accounts can sign-in.

![TodoList Client](./ReadmeFiles/todolist-client.png)

## How to run this sample

### Step 1: In the downloaded folder

From your shell or command line:

```Shell
cd "2. Web API now calls Microsoft Graph"
```

### Step 2: Update the Registration for the sample with your Microsoft Entra tenant

There are two projects in this sample. But because we would be using a close knit topology, where both apps will share the same app id. To achieve that the following steps are needed to bundle together into one app registration in your Microsoft Entra tenant. To do this follow the steps below:

#### Update Registration for the service app (TodoListService) 

1. From the app's Overview page, select the **Authentication** section.
   - Click **Add a platform** button.
   - Select **Mobile and desktop applications** on the right blade.
   - In the **Redirect URIs** list, check the box next to **https://login.microsoftonline.com/common/oauth2/nativeclient**.
   - Click **Configure**.
1. Select the **Manifest** section and:
   - in the manifest, search for **"signInAudience"** and it's value should be **"AzureADandPersonalMicrosoftAccount"**
   - search for **"accessTokenAcceptedVersion"**, and see that its value is **2**. This property lets Microsoft Entra ID know that the Web API accepts v2.0 tokens
   - Select **Save**.

### Step 3:  Configure the sample to use your Microsoft Entra tenant

Configure the sample to enable users to sign-in with Microsoft personal accounts.
This constrain is ensured by updating `ida:Tenant` in `TodoListClient\App.Config` and `TenantId` in `TodoListService\appsettings.json` with the value `common`.

#### Configure the TodoListService C# project

1. Open the solution in Visual Studio.
1. In the *TodoListService* project, open the `appsettings.json` file.
1. Find the `ClientSecret` property and replace the existing value with the key you saved during the creation of the `TodoListClient-and-Service` app, in the Microsoft Entra admin center.

#### Configure the TodoListClient C# project

Open the project in your IDE (like Visual Studio) to configure the code.

1. In the TodoListClient project, open App.config.
1. Find the app key ida:ClientId and replace the value with the ApplicationID (Client ID) for copied from the app registration page of the TodoListService.

### Step 4: Run the sample

Clean the solution, rebuild the solution, and run it.

> [Consider taking a moment to share your experience with us.](https://forms.office.com/Pages/ResponsePage.aspx?id=v4j5cvGGr0GRqy180BHbR73pcsbpbxNJuZCMKN0lURpUNDVNMlg5UlVWVDlVNFhJMUZFRlNEMU5LRiQlQCN0PWcu)

## How was the code created

For details about the way the code to protect the Web API was created, see [How was the code created](../2.%20Web%20API%20now%20calls%20Microsoft%20Graph/README.md#How-was-the-code-created) section, of the `README.md` file located in the sibling folder named **2. Web API now calls Microsoft Graph**.

This section addresses the differences in the code for the Web API calling the Microsoft Graph with Microsoft personal accounts.

### Changes to the client side (TodoListClient)

#### App.Config

There is one change in the App.Config, and one thing to check

- The change is that the tenant should be set to `common` in order to let users sign-in with a personal account

    ```XML
    <add key="ida:Tenant" value="common"/>
    ```

- The thing to draw your attention to, is that you now have the same client ID (Application ID) for the client application and the service. This is not usually the case, which is why your attention is especially drawn here.

    ```XML
    <add key="ida:ClientId" value="{clientId}"/>
    <add key="todo:TodoListScope" value="api://{clientId}/access_as_user"/>
    ```

#### Have the client let the user consent for the scopes required for the service

The Web API (TodoList service) does not have the possibility of having an interaction with the user (by definition of a Web API), and therefore cannot let the user consent for the scopes it requests. Given that the Web API and the client have the same client ID, it's possible for the client to request a token for the Web API and let the user pre-consent to the scopes requested by the Web API (in this case "user.read")

This is done in `MainWindow.xaml.cs` in the `SignIn` method, by adding a modifier `.WithExtraScopesToConsent(new[] { "user.read" })` to the `AcquireTokenInteractive` call. Replace below method:

```CSharp
public class MainWindow
{
 private async void SignIn(object sender = null, RoutedEventArgs args = null)
 {
  ...
  // Force a sign-in (PromptBehavior.Always), as the MSAL web browser might contain cookies for the current user, and using .Auto
  // would re-sign-in the same user
  var result = await _app.AcquireTokenInteractive(Scopes)
      .WithAccount(accounts.FirstOrDefault())
      .WithPrompt(Prompt.SelectAccount)
      .ExecuteAsync()
      .ConfigureAwait(false);
   ...
 }
}
```

by

```CSharp
public class MainWindow
{
 private async void SignIn(object sender = null, RoutedEventArgs args = null)
 {
  ...
  // Force a sign-in (PromptBehavior.Always), as the ADAL web browser might contain cookies for the current user, and using .Auto
  // would re-sign-in the same user
  var result = await _app.AcquireTokenInteractive(Scopes)
      .WithAccount(accounts.FirstOrDefault())
      .WithPrompt(Prompt.SelectAccount)
      .WithExtraScopesToConsent(new[] { "user.read" })
      .ExecuteAsync()
      .ConfigureAwait(false);
   ...
 }
}
```

## How to deploy this sample to Azure

See [Readme.md](../5.Deploy-Web-API/README.md) to deploy this sample to Azure.

## Community Help and Support

Use [Stack Overflow](http://stackoverflow.com/questions/tagged/msal) to get support from the community.
Ask your questions on Stack Overflow first and browse existing issues to see if someone has asked your question before.
Make sure that your questions or comments are tagged with [`microsoft-entra-id` `msal` `dotnet`].

If you find a bug in the sample, please raise the issue on [GitHub Issues](../../../issues).

To provide a recommendation, visit the following [User Voice page](https://feedback.azure.com/forums/169401-azure-active-directory).

## Contributing

If you'd like to contribute to this sample, see [CONTRIBUTING.MD](../CONTRIBUTING.md).

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/). For more information, see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.

## More information

For more information, visit the following links:

- To lean more about the application registration, visit:

  - [Quickstart: Register an application with the Microsoft identity platform (Preview)](https://docs.microsoft.com/en-us/azure/active-directory/develop/quickstart-register-app)
  - [Quickstart: Configure a client application to access web APIs (Preview)](https://docs.microsoft.com/en-us/azure/active-directory/develop/quickstart-configure-app-access-web-apis)
  - [Quickstart: Quickstart: Configure an application to expose web APIs (Preview)](https://docs.microsoft.com/en-us/azure/active-directory/develop/quickstart-configure-app-expose-web-apis)

- To learn more about the code, visit [Conceptual documentation for MSAL.NET](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/wiki#conceptual-documentation) and in particular:
  - [Acquiring tokens with authorization codes on web apps](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/wiki/Acquiring-tokens-with-authorization-codes-on-web-apps)
  - [Customizing Token cache serialization](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/wiki/token-cache-serialization)

- Articles about the Microsoft Entra ID V2 endpoint [http://aka.ms/aaddevv2](http://aka.ms/aaddevv2), with a focus on:
  - [Microsoft Entra ID v2.0 and OAuth 2.0 On-Behalf-Of flow](https://docs.microsoft.com/en-us/azure/active-directory/develop/active-directory-v2-protocols-oauth-on-behalf-of)

- [Introduction to Identity on ASP.NET Core](https://docs.microsoft.com/en-us/aspnet/core/security/authentication/identity?view=aspnetcore-2.1&tabs=visual-studio%2Caspnetcore2x)
  - [AuthenticationBuilder](https://docs.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.authentication.authenticationbuilder?view=aspnetcore-2.0)
  - [Microsoft Entra ID with ASP.NET Core](https://docs.microsoft.com/en-us/aspnet/core/security/authentication/azure-active-directory/?view=aspnetcore-2.1)
