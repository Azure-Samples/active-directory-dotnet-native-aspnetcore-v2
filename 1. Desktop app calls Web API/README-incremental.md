---
services: active-directory
platforms: dotnet
author: jmprieur
level: 200
client: .NET Desktop (WPF)
service: ASP.NET Core Web API
endpoint: Microsoft identity platform
---
# Sign-in a user with the Microsoft Identity Platform in a WPF Desktop application and call an ASP.NET Core Web API

[![Build status](https://identitydivision.visualstudio.com/IDDP/_apis/build/status/Microsoft Entra ID%20Samples/.NET%20client%20samples/active-directory-dotnet-native-aspnetcore-v2)](https://identitydivision.visualstudio.com/IDDP/_build/latest?definitionId=516)

## About this sample

### Table of content

- [About this sample](#about-this-sample)
  - [Scenario](#scenario)
  - [Overview](#overview)
  - [User experience when using this sample](#user-experience-when-using-this-sample)
- [How to run this sample](#how-to-run-this-sample)
  - [Step 1:  In the downloaded folder](#step-1-in-the-downloaded-folder)
  - [Step 2:  Register the sample application with your Microsoft Entra tenant](#step-2-register-the-sample-application-with-your-azure-active-directory-tenant)
  - [Step 3: Run the sample](#step-3-run-the-sample)
- [How was the code created](#how-was-the-code-created)
- [Choosing which scopes to expose](#choosing-which-scopes-to-expose)
- [Next chapter of the tutorial: the Web API itself calls another downstream Web API](#next-chapter-of-the-tutorial-the-web-api-itself-calls-another-downstream-web-api)
- [Community Help and Support](#community-help-and-support)
- [Contributing](#contributing)
- [More information](#more-information)

### Scenario

In the first chapter, we would protect an ASP.Net Core Web API using the Microsoft Identity Platform. The Web API will be protected using Microsoft Entra ID OAuth Bearer Authorization.

An on-demand video was created for the Build 2018 event, featuring this scenario and a previous version of this sample. See the video [Building Web API Solutions with Authentication](https://channel9.msdn.com/Events/Build/2018/THR5000), and the associated [PowerPoint deck](http://video.ch9.ms/sessions/c1f9c808-82bc-480a-a930-b340097f6cc1/BuildWebAPISolutionswithAuthentication.pptx)

### Overview

This sample presents a Web API running on ASP.NET Core, protected by Microsoft Entra ID OAuth Bearer Authorization. The Web API is called by a .NET Desktop WPF application.
The .Net application uses the Microsoft Authentication Library [MSAL.NET](https://aka.ms/msal-net) to obtain a JWT [Access Token](https://docs.microsoft.com/azure/active-directory/develop/access-tokens) through the [OAuth 2.0](https://docs.microsoft.com/azure/active-directory/develop/active-directory-protocols-oauth-code) protocol. The access token is sent to the ASP.NET Core Web API, which authorizes the user using the ASP.NET JWT Bearer Authentication middleware.

This sub-folder contains a Visual Studio solution made of two applications: the desktop application (TodoListClient), and the Web API (TodoListService).

![Topology](./ReadmeFiles/topology.png)

### User experience when using this sample

The Web API (TodoListService) maintains an in-memory collection of to-do items for each authenticated user. Several applications signed-in under the same identity will share the same to-do list.

The WPF application (TodoListClient) allows a user to:

- Sign-in. The first time a user signs in, a consent screen is presented where the user consents for the application accessing the TodoList Service on their behalf.
- When the user has signed-in, the user is presented with a list of to-do items fetched from the Web API for this signed-in identity.
- The user can add more to-do items by clicking on *Add item* button.

Next time a user runs the application, the user is signed-in with the same identity as the WPF application maintains a cache on disk. Users can clear the cache (which will have the effect of them signing out).

![TodoList Client](./ReadmeFiles/todolist-client.png)

## How to run this sample

### Step 1:  In the downloaded folder

From your shell or command line:

```Shell
cd "1. Desktop app calls Web API"
```

### Step 2:  Register the sample application with your Microsoft Entra tenant

There are two projects in this sample. Each needs to be separately registered in your Microsoft Entra tenant. To register these projects, you can:

- either follow the steps below for manual registration
- or use PowerShell scripts that:
  - **automatically** creates the Microsoft Entra applications and related objects (passwords, permissions, dependencies) for you. Note that this works for Visual Studio only.
  - modify the Visual Studio projects' configuration files.

<details>
  <summary>Expand this section if you want to use this automation:</summary>

								   
1. On Windows, run PowerShell and navigate to the root of the cloned directory
1. In PowerShell run:

   ```PowerShell
   Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope Process -Force
   ```

1. Run the script to create your Microsoft Entra application and configure the code of the sample application accordingly.
1. In PowerShell run:

   ```PowerShell
   cd .\AppCreationScripts\
   .\Configure.ps1
   ```

   > Other ways of running the scripts are described in [App Creation Scripts](./AppCreationScripts/AppCreationScripts.md)
   > The scripts also provide a guide to automated application registration, configuration and removal which can help in your CI/CD scenarios.

1. Open the Visual Studio solution and click start to run the code.
																			
</details>

Follow the steps below to manually walk through the steps to register and configure the applications.												  
																
#### Choose the Microsoft Entra tenant where you want to create your applications

As a first step you'll need to:

1. Sign in to the [Microsoft admin center](https://portal.azure.com) using either a work or school account or a personal Microsoft account.
1. If your account is present in more than one Microsoft Entra tenant, select your profile at the top right corner in the menu on top of the page, and then **switch directory**.
   Change your portal session to the desired Microsoft Entra tenant.

#### Register the service app (TodoListService (active-directory-dotnet-native-aspnetcore-v2))

1. Navigate to the Microsoft identity platform for developers [App registrations](https://go.microsoft.com/fwlink/?linkid=2083908) page.
1. Select **New registration**.
1. In the **Register an application page** that appears, enter your application's registration information:
   - In the **Name** section, enter a meaningful application name that will be displayed to users of the app, for example `TodoListService (active-directory-dotnet-native-aspnetcore-v2)`.
   - Under **Supported account types**, select **Accounts in any organizational directory and personal Microsoft accounts (e.g. Skype, Xbox, Outlook.com)**.
1. Select **Register** to create the application.
1. In the app's registration screen, find and note the **Application (client) ID**. You use this value in your app's configuration file(s) later in your code.
1. Select **Save** to save your changes.
1. In the app's registration screen, click on the **Expose an API** blade to the left to open the page where you can declare the parameters to expose this app as an API for which client applications can obtain [access tokens](https://docs.microsoft.com/azure/active-directory/develop/access-tokens) for.
The first thing that we need to do is to declare the unique [resource](https://docs.microsoft.com/azure/active-directory/develop/v2-oauth2-auth-code-flow) URI that the clients will be using to obtain access tokens for this API. To declare an resource URI, follow the following steps:
   - Click `Set` next to the **Application ID URI** to generate a URI that is unique for this app.
   - For this sample, accept the proposed Application ID URI (api://{clientId}) by selecting **Save**.
1. All APIs have to publish a minimum of one [scope](https://docs.microsoft.com/azure/active-directory/develop/v2-oauth2-auth-code-flow#request-an-authorization-code) for the client's to obtain an access token successfully. To publish a scope, follow the following steps:
   - Select **Add a scope** button open the **Add a scope** screen and Enter the values as indicated below:
        - For **Scope name**, use `access_as_user`.
        - Select **Admins and users** options for **Who can consent?**
        - For **Admin consent display name** type `Access TodoListService (active-directory-dotnet-native-aspnetcore-v2)`
        - For **Admin consent description** type `Allows the app to access TodoListService (active-directory-dotnet-native-aspnetcore-v2) as the signed-in user.`
        - For **User consent display name** type `Access TodoListService (active-directory-dotnet-native-aspnetcore-v2)`
        - For **User consent description** type `Allow the application to access TodoListService (active-directory-dotnet-native-aspnetcore-v2) on your behalf.`
        - Keep **State** as **Enabled**
        - Click on the **Add scope** button on the bottom to save this scope.

#### Configure the  service app (TodoListService (active-directory-dotnet-native-aspnetcore-v2)) to use your app registration

Open the project in your IDE (like Visual Studio) to configure the code.
>In the steps below, "ClientID" is the same as "Application ID" or "AppId".	

1. Open the `TodoListService\appsettings.json` file
2. Find the app key `Domain` and replace the existing value with your Microsoft Entra tenant name.
3. Find the app key `TenantId` and replace the existing value with your Microsoft Entra tenant ID.
4. Find the app key `ClientId` and replace the existing value with the application ID (clientId) of the `TodoListService (active-directory-dotnet-native-aspnetcore-v2)` application copied from the Microsoft admin center.

#### Register the client app (TodoListClient (active-directory-dotnet-native-aspnetcore-v2))

1. Navigate to the Microsoft identity platform for developers [App registrations](https://go.microsoft.com/fwlink/?linkid=2083908) page.
1. Select **New registration**.
1. In the **Register an application page** that appears, enter your application's registration information:
   - In the **Name** section, enter a meaningful application name that will be displayed to users of the app, for example `TodoListClient (active-directory-dotnet-native-aspnetcore-v2)`.
   - Under **Supported account types**, select **Accounts in any organizational directory and personal Microsoft accounts (e.g. Skype, Xbox, Outlook.com)**.
1. Select **Register** to create the application.
1. In the app's registration screen, find and note the **Application (client) ID**. You use this value in your app's configuration file(s) later in your code.
1. In the app's registration screen, select **Authentication** in the menu.
   - If you don't have a platform added, select **Add a platform** and select the **Public client (mobile & desktop)** option.
   - In the **Redirect URIs** | **Suggested Redirect URIs for public clients (mobile, desktop)** section, select **https://login.microsoftonline.com/common/oauth2/nativeclient**
1. Select **Save** to save your changes.
1. In the app's registration screen, click on the **API permissions** blade in the left to open the page where we add access to the Apis that your application needs.
   - Click the **Add a permission** button and then,
   - Ensure that the **My APIs** tab is selected.
   - In the list of APIs, select the API `TodoListService (active-directory-dotnet-native-aspnetcore-v2)`.
   - In the **Delegated permissions** section, select the **access_as_user** in the list. Use the search box if necessary.
   - Click on the **Add permissions** button at the bottom.

#### Configure the  client app (TodoListClient (active-directory-dotnet-native-aspnetcore-v2)) to use your app registration

Open the project in your IDE (like Visual Studio) to configure the code.
>In the steps below, "ClientID" is the same as "Application ID" or "AppId".						  
1. Open the `TodoListClient\App.Config` file
2. Find the app key `ida:Tenant` and replace the existing value with your Microsoft Entra tenant name.
3. Find the app key `ida:ClientId` and replace the existing value with the application ID (clientId) of the `TodoListClient (active-directory-dotnet-native-aspnetcore-v2)` application copied from the Microsoft admin center.
4. Find the app key `todo:TodoListScope` and replace the existing value with Scope.
5. Find the app key `todo:TodoListBaseAddress` and replace the existing value with the base address of the TodoListService (active-directory-dotnet-native-aspnetcore-v2) project (by default `https://localhost:44351/`).

### Step 3: Run the sample

Clean the solution, rebuild the solution, and run it. You might want to go into the solution properties and set both projects as startup projects, with the service project starting first.

When you start the Web API from Visual Studio, depending on the browser you use, you'll get:

- an empty web page (with Microsoft Edge)
- or an error HTTP 401 (with Chrome)

This behavior is expected as the browser is not authenticated. The WPF application will be authenticated, so it will be able to access the Web API.

Explore the sample by signing in into the TodoList client, adding items to the To Do list, removing the user account (clearing the cache), and starting again.  As explained, if you stop the application without removing the user account, the next time you run the application, you won't be prompted to sign in again. That is because the sample implements a persistent cache for MSAL, and remembers the tokens from the previous run.

NOTE: Remember, the To-Do list is stored in memory in this `TodoListService-v2` sample. Each time you run the TodoListService API, your To-Do list will get emptied.

> Did the sample not work for you as expected? Did you encounter issues trying this sample? Then please reach out to us using the [GitHub Issues](../../../issues) page.

> [Consider taking a moment to share your experience with us.](https://forms.office.com/Pages/ResponsePage.aspx?id=v4j5cvGGr0GRqy180BHbR73pcsbpbxNJuZCMKN0lURpUNDVNMlg5UlVWVDlVNFhJMUZFRlNEMU5LRiQlQCN0PWcu)

## How was the code created

### Code for the WPF app

The focus of this tutorial is on the Web API. The code for the desktop app is described in [Desktop app that calls web APIs - code configuration](https://docs.microsoft.com/azure/active-directory/develop/scenario-desktop-app-configuration)

### Code for the Web API (TodoListService)

The code for the service was created in the following way:

#### Create the Web API using the ASP.NET Core templates

```Text
md TodoListService
cd TodoListService
dotnet new webapi -au=SingleOrg
```

#### Add a model (TodoListItem) and modify the controller

In the TodoListService project, add a folder named `Models` and then a file named `TodoItem.cs` with the following content:

```CSharp
namespace TodoListService.Models
{
    public class TodoItem
    {
        public string Owner { get; set; }
        public string Title { get; set; }
    }
}
```

Under the `Controllers` folder, rename the file `ValuesController.cs` to `TodoListController.cs` and copy the following content in this file:

```CSharp
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using TodoListService.Models;

namespace TodoListService.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    public class TodoListController : Controller
    {
        static ConcurrentBag<TodoItem> todoStore = new ConcurrentBag<TodoItem>();

        [HttpGet]
        public IEnumerable<TodoItem> Get()
        {
            string owner = (User.FindFirst(ClaimTypes.NameIdentifier))?.Value;
            return todoStore.Where(t => t.Owner == owner).ToList();
        }

        [HttpPost]
        public void Post([FromBody]TodoItem Todo)
        {
            string owner = (User.FindFirst(ClaimTypes.NameIdentifier))?.Value;
            todoStore.Add(new TodoItem { Owner = owner, Title = Todo.Title });
        }
    }
}
```

This code gets the todo list items associated with their owner, which is the identity of the user using the Web API. It also adds todo list items associated with the same user.
There is no persistence as implementing token persistence and todo item persistence on the service side would be beyond the scope of this sample

Add the Microsoft.Identity.Web NuGet package.

```CSharp
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.TokenCacheProviders.InMemory;
```

Replace:

```CSharp
 .AddAzureAdBearer(options => Configuration.Bind("AzureAd", options));
```

With:

```CSharp
services.AddMicrosoftIdentityWebApiAuthentication(Configuration);
```

The method `AddMicrosoftIdentityWebApiAuthentication` in Microsoft.Identity.Web ensures that:

- the tokens are validated with Microsoft Identity Platform 
- the valid audiences are both the ClientID of our Web API (default value of `options.Audience` with the ASP.NET Core template) and api://{ClientID}
- the issuer is validated (for the multi-tenant case)

#### Change the App URL

You want to change the launch URL and application URL to match the application registration:

If you're using Visual Studio 2019:

1. Edit the TodoListService's properties (right click on `TodoListService.csproj`, and choose **Properties**)
1. In the Debug tab:
    1. Check the **Launch browser** field to `https://localhost:44351/api/todolist`
    1. Change the **App URL** field to be `https://localhost:44351` as this URL is the URL registered in the Microsoft Entra application representing the Web API.
    1. Check the **Enable SSL** field

If you are not using Visual Studio, edit the `TodoListService\Properties\launchsettings.json` file.

## Choosing which scopes to expose

This sample exposes a delegated permission (access_as_user) that will be presented in the access token claim. The attribute `[RequiredScope("access_as_user")]` on the controller or controller action, takes care of validating that this is the case:

```csharp
[RequiredScope("access_as_user")]
public IEnumerable<TodoItem> Get()
{
 // process the action
}
```

### For delegated permissions how to access scopes

If a token has delegated permission scopes, they will be in the `scp` or `http://schemas.microsoft.com/identity/claims/scope` claim.

You can also expose app-only permissions if parts of your API needs to be accessed independently of a user (that is by a [daemon application](https://github.com/Azure-Samples/ms-identity-dotnetcore-daemon)).

## Next chapter of the tutorial: the Web API itself calls another downstream Web API

In the next chapter, we will enhance this Web API to call another downstream Web API (Microsoft Graph) on behalf of the user signed in to the WPF application. 

See [2. Web API now calls Microsoft Graph](../2.%20Web%20API%20now%20calls%20Microsoft%20Graph/README-incremental.md)

## Community Help and Support

Use [Stack Overflow](http://stackoverflow.com/questions/tagged/msal) to get support from the community.
Ask your questions on Stack Overflow first and browse existing issues to see if someone has asked your question before.
Make sure that your questions or comments are tagged with [`microsoft-entra-id` `msal` `dotnet`].

To provide a recommendation, visit the following [User Voice page](https://feedback.azure.com/forums/169401-azure-active-directory).

## Contributing

If you'd like to contribute to this sample, see [CONTRIBUTING.MD](../CONTRIBUTING.md).

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/). For more information, see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.

## More information

For more information, visit the following links:

- To lean more about the application registration, visit:

  - [Quickstart: Register an application with the Microsoft identity platform (Preview)](https://docs.microsoft.com/azure/active-directory/develop/quickstart-register-app)
  - [Quickstart: Configure a client application to access web APIs (Preview)](https://docs.microsoft.com/azure/active-directory/develop/quickstart-configure-app-access-web-apis)
  - [Quickstart: Quickstart: Configure an application to expose web APIs (Preview)](https://docs.microsoft.com/azure/active-directory/develop/quickstart-configure-app-expose-web-apis)

- To learn more about ASP.NET Core Web APIs: see [Introduction to Identity on ASP.NET Core](https://docs.microsoft.com/aspnet/core/security/authentication/identity?view=aspnetcore-2.1&tabs=visual-studio%2Caspnetcore2x) and also:
  - [AuthenticationBuilder](https://docs.microsoft.com/dotnet/api/microsoft.aspnetcore.authentication.authenticationbuilder?view=aspnetcore-2.0)
  - [Microsoft Entra ID with ASP.NET Core](https://docs.microsoft.com/aspnet/core/security/authentication/azure-active-directory/?view=aspnetcore-2.1)
