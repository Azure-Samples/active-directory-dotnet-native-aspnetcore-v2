---
services: active-directory
platforms: dotnet
author: jmprieur
level: 300
client: .NET Desktop (WPF)
service: ASP.NET Core Web API
endpoint: Microsoft identity platform
---
# Calling an ASP.NET Core Web API from a console application using Proof of Possession tokens on the Microsoft Identity Platform

[![Build status](https://identitydivision.visualstudio.com/IDDP/_apis/build/status/AAD%20Samples/.NET%20client%20samples/active-directory-dotnet-native-aspnetcore-v2)](https://identitydivision.visualstudio.com/IDDP/_build/latest?definitionId=516)

## About this sample

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

You expose a Web API and you want to protect it so that only authenticated users can access it. You want to enable authenticated users with both work and school accounts
or Microsoft personal accounts (formerly live account) to use your Web API. You want to protect the access token from being replayed by enabling [Proof of possession tokens](https://tools.ietf.org/html/draft-ietf-oauth-signed-http-request-03#page-9)

### Overview

This sample presents a Web API running on ASP.NET Core, protected by Azure AD Proof of Possession (PoP) Authentication. The Web API is exercised by a .NET Desktop Console application.
The .Net application uses the Active Directory Authentication Library [MSAL.NET](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet) to obtain a JWT access token through the [OAuth 2.0](https://docs.microsoft.com/en-us/azure/active-directory/develop/active-directory-protocols-oauth-code) protocol. The access token is sent to the ASP.NET Core Web API, which authenticates the user using the ASP.NET JWT Bearer Authentication middleware.

![Topology](./ReadmeFiles/topology.png)

> This sample is very similar to the [active-directory-dotnet-native-aspnetcore](https://github.com/Azure-Samples/active-directory-dotnet-native-aspnetcore) sample except that that one is for the Azure AD V1 endpoint
> and the token is acquired using [ADAL.NET](https://github.com/AzureAD/azure-activedirectory-library-for-dotnet), whereas this sample is for the Microsoft identity platform endpoint, and the token is acquired using [MSAL.NET](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet). The Web API was also modified to accept both V1 and V2 tokens.

### User experience when using this sample

The Web API (TodoListService) maintains an in-memory collection of to-do items per authenticated user. Several applications signed-in under the same identities share the same to-do list.

The desktop application (TodoListClient) enables a user to:

- Enter an item. The first time the user enters an item, she signs in, a consent screen is presented letting the user consent for the application accessing the TodoList Service and the Azure Active Directory.
- Each time, the user enters an item, she sees the list of to-do items exposed by Web API for the signed-in identity

Next time a user runs the application, the user is signed-in with the same identity as the application maintains a cache on disk. Users can clear the cache (which will also have the effect of signing them out)

![TodoList Client](./ReadmeFiles/todolist-client.png)

## How to run this sample

### Pre-requisites

- This sample only works with .NET Framework (PoP is not yet implemented for .NET Core in MSAL.NET)
- An Internet connection
- An Azure Active Directory (Azure AD) tenant. For more information on how to get an Azure AD tenant, see [How to get an Azure AD tenant](https://azure.microsoft.com/en-us/documentation/articles/active-directory-howto-tenant/)
- A user account in your Azure AD tenant, or a Microsoft personal account

### Step 1:  Clone or download this repository

From your shell or command line:

```Shell
git clone https://github.com/Azure-Samples/active-directory-dotnet-native-aspnetcore-v2.git aspnetcore-webapi
cd "aspnetcore-webapi\4.-Console-app-calls-web-API-with-PoP"
```

or download and extract the repository .zip file.

> Given that the name of the sample is pretty long, and so are the name of the referenced NuGet packages, you might want to clone it in a folder close to the root of your hard drive, to avoid file size limitations on Windows.

### Step 2:  Register the sample with your Azure Active Directory tenant

There are two projects in this sample. Each needs to be separately registered in your Azure AD tenant. To register these projects, you can:

- either follow the steps [Step 2: Register the sample with your Azure Active Directory tenant](#step-2-register-the-sample-with-your-azure-active-directory-tenant) and [Step 3:  Configure the sample to use your Azure AD tenant](#step-3--configure-the-sample-to-use-your-azure-ad-tenant)
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

1. In the application registration page for the `TodoListService-v2` application, select the **Manifest** section
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

#### Register the service app (TodoListService)


1. Navigate to the Microsoft identity platform for developers [App registrations](https://go.microsoft.com/fwlink/?linkid=2083908) page.
1. Select **New registration**.
1. When the **Register an application page** appears, enter your application's registration information:
   - In the **Name** section, enter a meaningful application name that will be displayed to users of the app, for example `TodoListService-v2`.
   - Change **Supported account types** to **Accounts in any organizational directory and personal Microsoft accounts (e.g. Skype, Xbox, Outlook.com)**.
   - In the Redirect URI (optional) section, select **Web** in the combo-box.
   - For the *Redirect URI*, enter the base URL for the sample. By default, this sample uses `https://localhost:44351/`.
   - Select **Register** to create the application.

1. On the app **Overview** page, find the **Application (client) ID** value and record it for later. You'll need it to configure the Visual Studio configuration file for this project (`ClientId` in `TodoListService\appsettings.json`).
1. Select the **Expose an API** section, and:
   - Select **Add a scope**
   - accept the proposed Application ID URI (api://{clientId}) by selecting **Save and Continue**
   - Enter the following parameters:
     - for **Scope name** use `access_as_user`
     - Ensure the **Admins and users** option is selected for **Who can consent**
     - in **Admin consent display name** type `Access TodoListService as a user`
     - in **Admin consent description** type `Accesses the TodoListService Web API as a user`
     - in **User consent display name** type `Access TodoListService as a user`
     - in **User consent description** type `Accesses the TodoListService Web API as a user`
     - Keep **State** as **Enabled**
     - Select **Add scope**
1. [Optional] Select the **Manifest** section
   - in the manifest, search for **"accessTokenAcceptedVersion"**, and see that its value is **2**. This property lets Azure AD know that the Web API accepts v2.0 tokens
   - Select **Save**

> Important: it's up to the Web API to decide which version of token (v1.0 or v2.0) it accepts. Then when clients request a token for your Web API using the identity platform endpoint, they'll get a token which version is accepted by the Web API. The code validating the tokens in this sample was written to accept both versions.

#### Register the client app (TodoListClient)

1. Navigate to the Microsoft identity platform for developers [App registrations](https://go.microsoft.com/fwlink/?linkid=2083908) page.
1. Select **New registration**.
1. When the **Register an application page** appears, enter your application's registration information:
   - In the **Name** section, enter a meaningful application name that will be displayed to users of the app, for example `TodoListClient-v2`.
   - Change **Supported account types** to **Accounts in any organizational directory and personal Microsoft accounts (e.g. Skype, Xbox, Outlook.com)**.
   - Select **Register** to create the application.
1. On the app **Overview** page, find the **Application (client) ID** value and record it for later. You'll need it to configure the Visual Studio configuration file for this project (`ida:ClientId` in `TodoListClient\App.Config`).
1. From the app's Overview page, select the **Authentication** section.
   1. In the **Redirect URIs** list, under **Suggested Redirect URIs for public clients (mobile, desktop)** check the box next to **https://login.microsoftonline.com/common/oauth2/nativeclient**.
   1. Select **Save**.
1. Select the **API permissions** section
   - Click the **Add a permission** button and then,
   - Ensure that the **My APIs** tab is selected
   - In the list of APIs, select the `TodoListService-v2` API, or the name you entered for the Web API.
   - In the **Delegated permissions** section, ensure that the right permissions are checked: **access_as_user**. Use the search box if necessary.
   - Select the **Add permissions** button

### Step 3:  Configure the sample to use your Azure AD tenant

#### Choose which users account to sign in

By default the sample is configured to enable users to sign in with any work and school accounts (AAD) or Microsoft Personal accounts (formerly live account).
This is because `ida:Tenant` in `TodoListClient\App.Config` has the value of `common`.

##### Important note

`common` is **not** a proper tenant. It's just a **convention** to express that the accepted tenants are any Work and School organizations, or Personal Microsoft account (consumer accounts).
Accepted tenants can have the following values:

Value | Meaning
----- | --------
`common` | users can sign in with any Work and School account, or Microsoft Personal account
`organizations` |  users can sign in with any Work and School account
`consumers` |  users can sign in with a Microsoft Personal account
a GUID or domain name | users can only sign in with an account for a specific organization described by its tenant ID (GUID) or domain name

#### Configure the TodoListService C# project

Note: if you used the setup scripts, the changes below will have been applied for you

1. Open the solution in Visual Studio.
1. In the *TodoListService-v2* project, open the `appsettings.json` file.
1. Find the `ClientId` property and replace the value with the Application ID (Client ID) property of the *TodoListService-v2* application, that you registered earlier.
1. [Optional] if you want to limit sign-in to users in your organization, also update the following properties:
- `Domain`, replacing the existing value with your AAD tenant domain, for example, contoso.onmicrosoft.com.
- `TenantId`, replacing the existing value with the Tenant ID.

#### Configure the TodoListClient C# project

Note: if you used the setup scripts, the changes below will have been applied for you

1. In the TodoListClient project, open `App.config`.
1. Find the app key `ida:ClientId` and replace the value with the ApplicationID (Client ID) for the *TodoListClient-v2* app copied from the app registration page.
1. Find the app key `todo:TodoListScope` and replace the value with the scope of the TodoListService-v2 application copied from the app registration in the **Expose an API** tab (of the form ``api://<Application ID of service>/access_as_user`` if you followed the instructions above)
1. [Optional] If you want your application to work only in your organization (only in your tenant) you'll also need to Find the app key `ida:Tenant` and replace the value with your AAD Tenant ID (GUID). Alternatively you can also use your AAD tenant Name (for example, contoso.onmicrosoft.com)
1. [Optional] If you changed the default URL for your service application, find the app key `todo:TodoListBaseAddress` and replace the value with the base address of the TodoListService project.

### Step 4: Run the sample

Clean the solution, rebuild the solution, and run it. You might want to go into the solution properties and set both projects as startup projects, with the service project starting first.

When you start the Web API from Visual Studio, depending on the browser you use, you'll get:

- an empty web page (case with Microsoft Edge)
- or an error HTTP 401 (case with Chrome)

This behavior is expected as you are not authenticated. The WPF application will be authenticated, so it will be able to access the Web API.

Explore the sample by signing in into the TodoList client, adding items to the To Do list, removing the user account (clearing the cache), and starting again.  As explained, if you stop the application without removing the user account, the next time you run the application, you won't be prompted to sign in again. That is because the sample implements a persistent cache for MSAL, and remembers the tokens from the previous run.

NOTE: Remember, the To-Do list is stored in memory in this `TodoListService-v2` sample. Each time you run the TodoListService API, your To-Do list will get emptied.

### Troubleshooting

#### The Web API needs to accept v2.0 tokens to handle users signed-in with Microsoft personal accounts

The following issues make sense, but could happen in migration scenarios where you had an existing Web API, or created the Web API with v1.0 PowerShell scripts:

If `ida:Tenant` is set to `common` or `consumers` in the TodoListClient's **App.Config** and you get the following errors:

- `'The provided value for the input parameter 'scope' is not valid. The scope 'api://{ServiceClientId}/access_as_user offline_access openid profile' is not configured correctly'` when signing-in with a Microsoft personal account

- `'Resource 'api://{ServiceClientId}'  (TodoListService-v2) has a configured token version of '1' and is not supported over the /common or /consumers endpoints.'` when signing-in with a Work and School account

Then you need to set the  `accessTokenAcceptedVersion` property of the Web API to **2** in the manifest.

## How was the code created

### Code for the Console app

The focus of this tutorial is Pop (Proof of Possession).

With PoP, the programming model is a bit different than the way MSAL.NET usually works, as a Pop token contains information about the URL it's for, as well as the HTTP verb (POST, GET). Therefore to get a Pop token you will provide to MSAL an `HttpRequestMessage` and MSAL.NET will automatically populate the Authorization header of this message with a Pop token. You'll need to:

- Instantiate a `IPublicClientApplication` specifying `WithExperimentalFeatures()` as PoP is still an experimental feature for MSAL.NET (and not implemented for all the flows, only public client applications on .NET Framework)

   ```csharp
    var app = PublicClientApplicationBuilder.Create(ClientId)
        .WithAuthority(Authority)
        .WithDefaultRedirectUri()
        .WithExperimentalFeatures() // Needed for PoP
        .Build();
   ```

- Create an `HttpRequestMessage` passing the verb (for instance `HttpMethod.Post`) and the URL of the Web API to call
   ```csharp
    HttpRequestMessage writeRequest =
       new HttpRequestMessage(HttpMethod.Post, new Uri(TodoListApiAddress));
   ```

- Call the `AcquireTokenSilent` or `AcquireTokenInteractive` as usual, but passing the http request message (`writeRequest`) to a `.WithProofOfPossesssion(writeRequest)` call.

   ```csharp
    TodoItem todoItem = ReadItemFromConsole();

    // Add Pop token to the HttpRequestMessage, attempting from the cache
    // and otherwise interactively
    try
    {
        var account = (await app.GetAccountsAsync()).FirstOrDefault();
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


    // Call the Web API
    string json = JsonConvert.SerializeObject(todoItem);
    StringContent content = new StringContent(json,
                                              Encoding.UTF8,
                                              "application/json");
    writeRequest.Content = content;
    await httpClient.SendAsync(writeRequest);
   ```

### Code for the Web API (TodoListService)

The code for the TodoListService starts in Startup.cs, where you will call `AddPop()`
public void ConfigureServices(IServiceCollection services)
{
    services.AddProtectedWebApi(Configuration)
            .AddProtectedApiCallsWebApis(Configuration)
            .AddPop(Configuration)
            .AddInMemoryTokenCaches();
    services.AddControllers();
}

`AddPop`, really leverages the `SignedHttpRequest` feature in `Identity.Model` (middleware library). The incoming tokens ends-up being handled by an ASP.NET Core handler named `SignedHttpRequestAuthenticationHandler`. For details see [SignedHttpRequestAuthenticationHandler.cs](https://github.com/Azure-Samples/active-directory-dotnet-native-aspnetcore-v2/blob/7d999f6180ea90171b9a90ca931a0d3de2c035f5/Microsoft.Identity.Web/SignedHttpRequest/SignedHttpRequestAuthenticationHandler.cs#L44) from line 44.

### Update the `TodoListClient` to call the `TodoListService` running in Azure Web Sites

1. In Visual Studio, go to the `TodoListClient` project.
2. Open `TodoListClient\App.Config`.  Only one change is needed - update the `todo:TodoListBaseAddress` key value to be the address of the website you published,
   for example, [https://TodoListService-contoso.azurewebsites.net](https://TodoListService-contoso.azurewebsites.net).
3. Run the client! If you are trying multiple different client types (for example, .Net, Windows Store, Android, iOS) you can have them all call this one published web API.

> NOTE: Remember, the To-Do list is stored in memory in this TodoListService sample. Azure Web Sites will spin down your web site if it is inactive, and your To Do list will get emptied.
Also, if you increase the instance count of the web site, requests will be distributed among the instances. To Do will, therefore, not be the same on each instance.

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

- To learn more about ASP.NET Core Web APIs: see [Introduction to Identity on ASP.NET Core](https://docs.microsoft.com/en-us/aspnet/core/security/authentication/identity?view=aspnetcore-2.1&tabs=visual-studio%2Caspnetcore2x) and also:
  - [AuthenticationBuilder](https://docs.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.authentication.authenticationbuilder?view=aspnetcore-2.0)
  - [Azure Active Directory with ASP.NET Core](https://docs.microsoft.com/en-us/aspnet/core/security/authentication/azure-active-directory/?view=aspnetcore-2.1)
