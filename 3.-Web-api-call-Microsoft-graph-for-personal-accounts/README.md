---
services: active-directory
platforms: dotnet
author: jmprieur
level: 400
client: .NET Desktop (WPF)
service: ASP.NET Core Web API, Microsoft Graph
endpoint: Microsoft identity platform
---
# ASP.NET Core Web API calling Microsoft Graph including personal accounts, itself called from a WPF application using the Microsoft identity platform

[![Build status](https://identitydivision.visualstudio.com/IDDP/_apis/build/status/AAD%20Samples/.NET%20client%20samples/active-directory-dotnet-native-aspnetcore-v2)](https://identitydivision.visualstudio.com/IDDP/_build/latest?definitionId=516)

> The sample in this folder is part of a multi-phase tutorial. This folder is about the third phase named **Web API now calls Microsoft Graph including personal accounts**.
> The second phase is available from [2. Web API now calls Microsoft Graph](../2.%20Web%20API%20now%20calls%20Microsoft%20Graph).
>
> This article (README.md) contains the full instructions on how to configure the sample. If you have gone through Phase 1 and have already configured your Web API rather switch to the instructions for an incremental configuration in [README-incremental-instructions.md](README-incremental-instructions.md)

## About this sample

Contrary to the previous chapter, this one shows how to enable users to sign in with a Microsoft personal account

### Table of content

- [About this sample](#About-this-sample)
  - [Scenario](#Scenario)
  - [Overview](#Overview)
  - [User experience when using this sample](#User-experience-when-using-this-sample)
- [How to run this sample](#How-to-run-this-sample)
  - [Step 1:  Clone or download this repository](#step-1--clone-or-download-this-repository)
  - [Step 2:  Register the sample with your Azure Active Directory tenant](#step-2--register-the-sample-with-your-azure-active-directory-tenant)
  - [Step 3:  Configure the sample to use your Azure AD tenant](#step-3--configure-the-sample-to-use-your-azure-ad-tenant)
  - [Step 4:  Run the sample](#step-4-run-the-sample)
  - [Troubleshooting](#Troubleshooting)
- [How was the code created](#How-was-the-code-created)
- [Community Help and Support](#Community-Help-and-Support)
- [Contributing](#Contributing)
- [More information](#More-information)

### Scenario

You expose a Web API and you want to protect it so that only authenticated user can access it. You want to enable authenticated users with both work and school accounts
or Microsoft personal accounts (formerly live account) to use your Web API. Your API calls a downstream API (Microsoft Graph) to provide added value to its client apps.

### Overview

This sample presents a Web API running on ASP.NET Core 2.2, protected by Azure AD OAuth Bearer Authentication. The Web API calls the Microsoft Graph, and is exercised by a .NET Desktop WPF application.
Both applications use the Active Directory Authentication Library [MSAL.NET](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet) to obtain a JWT access token through the [OAuth 2.0](https://docs.microsoft.com/en-us/azure/active-directory/develop/active-directory-protocols-oauth-code) protocol. The desktop application:

1. Acquires an access token for the Web API
2. Calls the ASP.NET Core Web API adding the access token as a bearer token in the authentication header of the Http request. the Web API  authenticates the user using the ASP.NET JWT Bearer Authentication middleware.
3. When the client calls the Web API, the Web API acquires another token to call the Microsoft Graph (3)
4. then the Web API calls the graph

![Topology](./ReadmeFiles/topology.png)

- Developers who wish to gain good familiarity of programming for Microsoft Graph are advised to go through the [An introduction to Microsoft Graph for developers](https://www.youtube.com/watch?v=EBbnpFdB92A) recorded session.

### User experience when using this sample

The Web API (TodoListService) maintains an in-memory collection of to-do items per authenticated user. Several applications signed-in under the same identities share the same to-do list.

The WPF application (TodoListClient) enables a user to:

- Sign in. The first time a user signs in, a consent screen is presented. This consent screen lets the user consent for the application to access the TodoList Service.
- When the user has signed-in, the user sees the list of to-do items exposed by Web API for the signed-in identity
- The user can add more to-do items by clicking on *Add item* button. As they add items, they see that these items appear with their user name between parenthesis

Next time a user runs the application, the user is signed-in with the same identity as the application maintains a cache on disk. Users can clear the cache (which will also have the effect of signing them out)

![TodoList Client](./ReadmeFiles/todolist-client.png)

## How to run this sample

### Pre-requisites

- Install .NET Core for Windows by following the instructions at [dot.net/core](https://dot.net/core), which will include [Visual Studio 2017](https://aka.ms/vsdownload).
- An Internet connection
- An Azure Active Directory (Azure AD) tenant. For more information on how to get an Azure AD tenant, see [How to get an Azure AD tenant](https://azure.microsoft.com/en-us/documentation/articles/active-directory-howto-tenant/)
- A user account in your Azure AD tenant, or a Microsoft personal account

### Step 1:  Clone or download this repository

From your shell or command line:

```Shell
git clone https://github.com/Azure-Samples/active-directory-dotnet-native-aspnetcore-v2.git aspnetcore-webapi
cd "aspnetcore-webapi\3.-Web-api-call-Microsoft-graph-for-personal-accounts"
```

or download and exact the repository .zip file.

> Given that the name of the sample is pretty long, and so are the name of the referenced NuGet packages, you might want to clone it in a folder close to the root of your hard drive, to avoid file size limitations on Windows.

### Step 2:  Register the sample application with your Azure Active Directory tenant

There are two projects in this sample. Each needs to be separately registered in your Azure AD tenant. To register these projects, you can:

- either follow the steps [Step 2: Register the sample with your Azure Active Directory tenant](#step-2-register-the-sample-with-your-azure-active-directory-tenant) and [Step 3:  Configure the sample to use your Azure AD tenant](#choose-the-azure-ad-tenant-where-you-want-to-create-your-applications)
- or use PowerShell scripts that:
  - **automatically** creates the Azure AD applications and related objects (passwords, permissions, dependencies) for you
  - modify the Visual Studio projects' configuration files.

If you want to use this automation:
1. On Windows run PowerShell and navigate to the root of the cloned directory
1. In PowerShell run:
   ```PowerShell
   Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope Process -Force
   ```
1. Run the script to create your Azure AD application and configure the code of the sample application accordinly. 
   ```PowerShell
   .\AppCreationScripts\Configure.ps1
   ```
   > Other ways of running the scripts are described in [App Creation Scripts](./AppCreationScripts/AppCreationScripts.md)

1. In the list of pages for the application registration of the `TodoListService-v2` application, select **Manifest**
      - in the manifest, search for **"accessTokenAcceptedVersion"**, and replace **null** by **2**. This property lets Azure AD know that the Web API accepts v2.0 tokens
      - Select **Save**


   > Tip: Get directly to the app registration portal page for a give app, you can navigate to the links provided in the [AppCreationScripts\createdApps.html](AppCreationScripts\createdApps.html). This file is generated by the scripts during the app registration and configuration.

1. Open the Visual Studio solution and click start

If you don't want to use this automation, follow the steps below

#### Choose the Azure AD tenant where you want to create your applications

If you want to register your apps manually, as a first step you'll need to:

1. Sign in to the [Azure portal](https://portal.azure.com) using either a work or school account or a personal Microsoft account.
1. If your account is present in more than one Azure AD tenant, select your profile at the top right corner in the menu on top of the page, and then **switch directory**.
   Change your portal session to the desired Azure AD tenant.

#### Register the service app

1. Navigate to the Microsoft identity platform for developers [App registrations](https://go.microsoft.com/fwlink/?linkid=2083908) page.
1. Select **New registration**.
1. When the **Register an application page** appears, enter your application's registration information:
   - In the **Name** section, enter a meaningful application name that will be displayed to users of the app, for example `TodoListClient-and-Service`.
   - Change **Supported account types** to **Accounts in any organizational directory and personal Microsoft accounts (e.g. Skype, Xbox, Outlook.com)**.
   - Select **Register** to create the application.
1. On the app **Overview** page, find the **Application (client) ID** value and record it for later. You'll need it to configure the Visual Studio configuration file for both C# projects.
1. From the **Certificates & secrets** page, in the **Client secrets** section, choose **New client secret**:
   - Type a key description (of instance `app secret`),
   - Select a key duration of either **In 1 year**, **In 2 years**, or **Never Expires**.
   - When you press the **Add** button, the key value will be displayed, copy, and save the value in a safe location.
   - You'll need this key later to configure the project in Visual Studio. This key value will not be displayed again, nor retrievable by any other means,
     so record it as soon as it is visible from the Azure portal.
1. Select the **API permissions** section
   - Click the **Add a permission** button and then,
   - Ensure that the **Microsoft APIs** tab is selected
   - In the *Commonly used Microsoft APIs* section, click on **Microsoft Graph**
   - In the **Delegated permissions** section, ensure that the right permissions are checked: **User.Read**. Use the search box if necessary.
   - Select the **Add permissions** button
   - [Optional] if you are a tenant admin, and agree to grant the admin consent to the web api, select **Grant admin consent for {your tenant domain}**. If you don't do
    it, users will be presented a consent screen enabling them to consent to using the web api.
1. Select the **Expose an API** section, and:
   - Select **Add a scope**
   - accept the proposed Application ID URI (api://{clientId}) by selecting **Save and Continue**
   - Enter the following parameters
     - for **Scope name** use `access_as_user`
     - Keep **Admins and users** for **Who can consent**
     - in **Admin consent display name** type `Access TodoListService as a user`
     - in **Admin consent description** type `Accesses the TodoListService Web API as a user`
     - in **User consent display name** type `Access TodoListService as a user`
     - in **User consent description** type `Accesses the TodoListService Web API as a user`
     - Keep **State** as **Enabled**
     - Select **Add scope**

#### Register the client aspect (in the same app)

What **differs from the previous chapter** is that you will use the same application ID for the client part as for the service part

1. On the app **Overview** page, find the **Application (client) ID** value and record it for later. You'll need it to configure the Visual Studio configuration file for this project (`ida:ClientId` in `TodoListClient\App.Config`).
1. From the app's Overview page, select the **Authentication** section.
   - In the **Redirect URIs** list, under **Suggested Redirect URIs for public clients (mobile, desktop)** check the box next to **https://login.microsoftonline.com/common/oauth2/nativeclient**.
   - Select **Save**.

### Step 3:  Configure the sample to use your Azure AD tenant

#### Choose which users account to sign in

By default the sample is configured to enable users to sign in with any work and school accounts (AAD) accounts.
This constraint is ensured by `ida:Tenant` in `TodoListClient\App.Config` having the value `common`.

##### Important note

`common` is **not** a proper tenant. It's just a **convention** to express that the accepted tenants are any Work or School organizations, or Personal Microsoft account (consumer accounts).
Accepted tenants can have the following values:

#### Configure the TodoListService C# project

Note: if you used the setup scripts, the changes below will have been applied for you

1. Open the solution in Visual Studio.
1. In the *TodoListService-v2* project, open the `appsettings.json` file.
1. Find the `ClientId` property and replace the value with the Application ID (Client ID) property of the *TodoListService-v2* application, that you registered earlier.
1. Find the `ClientSecret` property and replace the existing value with the key you saved during the creation of the `TodoListService-v2` app, in the Azure portal.
1. [Optional] if you want to limit sign-in to users in your organization, also update the following properties:
- `Domain`, replacing the existing value with your AAD tenant domain, for example, contoso.onmicrosoft.com.
- `TenantId`, replacing the existing value with the Tenant ID.

#### Configure the TodoListClient C# project

Note: if you used the setup scripts, the changes below will have been applied for you

1. In the TodoListClient project, open `App.config`.
1. Find the app key `ida:ClientId` and replace the value with the ApplicationID (Client ID) for the *TodoListClient-v2* app copied from the app registration page.
1. Find the app key `todo:TodoListScope` and replace the value with the scope of the TodoListService-v2 application copied from the app registration in the **Expose an API** tab, but replace the scope by `.default` (of the form ``api://<Application ID of service>/.default`` if you followed the instructions above)
1. [Optional] If you want your application to work only in your organization (only in your tenant) you'll also need to Find the app key `ida:Tenant` and replace the value with your AAD Tenant ID (GUID). Alternatively you can also use your AAD tenant Name (for example, contoso.onmicrosoft.com)
1. [Optional] If you changed the default URL for your service application, find the app key `todo:TodoListBaseAddress` and replace the value with the base address of the TodoListService project.

### Step 4: Run the sample

Clean the solution, rebuild the solution, and run it.  You might want to go into the solution properties and set both projects as startup projects, with the service project starting first.

When you start the Web API from Visual Studio, depending on the browser you use, you'll get:

- an empty web page (case with Microsoft Edge)
- or an error HTTP 401 (case with Chrome)

This behavior is expected as you are not authenticated. The WPF application will be authenticated, so it will be able to access the Web API.

Explore the sample by signing in into the TodoList client, adding items to the To Do list, removing the user account (clearing the cache), and starting again.  As explained, if you stop the application without removing the user account, the next time you run the application, you won't be prompted to sign in again. That is because the sample implements a persistent cache for MSAL, and remembers the tokens from the previous run.

NOTE: Remember, the To-Do list is stored in memory in this `TodoListService-v2` sample. Each time you run the TodoListService API, your To-Do list will get emptied.

## How was the code created

For details about the code used for protecting a Web API, see [How was the code created](../.%20Web%20API%20now%20calls%20Microsoft%20Graph#How-was-the-code-created) section, of the README.md file located in the sibling folder named **2. Web API now calls Microsoft Graph**.

This section addresses the differences in the code for the Web API calling the Microsoft Graph with Microsoft personal accounts

### Change to the Web API (TodoListService)

### Modify the startup.cs file so that access tokens for Microsoft personal accounts can be refreshed

Update `Startup.cs` file:

- In the `ConfigureServices` method, in the call to `AddProtectedApiCallsWebApis`, you need to add the `offline_access` scope. For this, replace:

```CSharp
public class Startup
{
  ...
  // This method gets called by the runtime. Use this method to add services to the container.
   public void ConfigureServices(IServiceCollection services)
   {
    services.AddProtectedWebApi(Configuration)
            .AddProtectedApiCallsWebApis(Configuration, new string[] { "user.read" })
            .AddInMemoryTokenCaches();
    services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
   }
   ...
}
```

by

```csharp
public class Startup
{
  ...
  public void ConfigureServices(IServiceCollection services)
  {
      services.AddProtectedWebApi(Configuration)
              .AddProtectedApiCallsWebApis(Configuration, new string[] { "user.read", "offline_access" })
              .AddInMemoryTokenCaches();
  
      services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
   }
   ...
}
```

Indeed, ASP.NET core needs to request the `offline_access` scope so that MSAL.NET can get the refresh token

### Changes to the client side (TodoListClient)

### Web.Config

There is one change in the WebApp.Config, and one thing to check

- The change is that the tenant should be set to `common` in order to let users sign-in with a personal account

    ```XML
    <add key="ida:Tenant" value="common"/>
    ```

- the thing to draw your attention to, is that you now have the same client ID (Application ID) for the client application and the service. This is not usually the case, which is why your attention is especially drawn here. Therefore the GUID used in `ida:ClientId` is the same as the one used in the Application ID URI for the service: `todo:TodoListScope`

    ```XML
    <add key="ida:ClientId" value="01234567-89ab-cdef-0123-456789abcdef"/>
    <add key="todo:TodoListScope" value="api://01234567-89ab-cdef-0123-456789abcdef/access_as_user"/>
    ```

### Have the client let the user consent for the scopes required for the service

The Web API (TodoList service) does not have the possibility of having an interaction with the user (by definition of a Web API), and therefore cannot let the user consent for the scopes it requests. Given that the Web API and the client have the same client ID, it's possible for the client to request a token for the Web API and let the user pre-consent to the scopes requested by the Web API (in this case "user.read")

This is done in `MainWindow.xaml.cs` in the `SignIn` method, by replacing adding to the `AcquireTokenInteractive` call, a modifier `.WithExtraScopesToConsent(new[] { "user.read" })`. See [WithExtraScopeToConsent](https://docs.microsoft.com/en-us/azure/active-directory/develop/scenario-desktop-acquire-token#withextrascopetoconsent) for more details.

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

See section [How to deploy this sample to Azure](../1.%20Desktop%20app%20calls%20Web%20API/README.md#How-to-deploy-this-sample-to-Azure) in the first part of this tutorial, as the deployment is exactly the same.

## Community Help and Support

Use [Stack Overflow](http://stackoverflow.com/questions/tagged/msal) to get support from the community.
Ask your questions on Stack Overflow first and browse existing issues to see if someone has asked your question before.
Make sure that your questions or comments are tagged with [`msal` `dotnet`].

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

- Articles about the Microsoft identity platform endpoint [http://aka.ms/aaddevv2](http://aka.ms/aaddevv2), with a focus on:
  - [Microsoft identity platform and OAuth 2.0 On-Behalf-Of flow](https://docs.microsoft.com/en-us/azure/active-directory/develop/active-directory-v2-protocols-oauth-on-behalf-of)

- [Introduction to Identity on ASP.NET Core](https://docs.microsoft.com/en-us/aspnet/core/security/authentication/identity?view=aspnetcore-2.1&tabs=visual-studio%2Caspnetcore2x)
  - [AuthenticationBuilder](https://docs.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.authentication.authenticationbuilder?view=aspnetcore-2.0)
  - [Azure Active Directory with ASP.NET Core](https://docs.microsoft.com/en-us/aspnet/core/security/authentication/azure-active-directory/?view=aspnetcore-2.1)
