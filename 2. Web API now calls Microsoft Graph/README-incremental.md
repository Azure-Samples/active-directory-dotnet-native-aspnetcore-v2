---
services: active-directory
platforms: dotnet
author: jmprieur
level: 300
client: .NET Desktop (WPF)
service: ASP.NET Core Web API, Microsoft Graph
endpoint: Microsoft identity platform
---
# Sign a user into a Desktop application using Microsoft Identity Platform and call a protected ASP.NET Core Web API, which calls Microsoft Graph on-behalf of the user

[![Build status](https://identitydivision.visualstudio.com/IDDP/_apis/build/status/aad%20Samples/.NET%20client%20samples/active-directory-dotnet-native-aspnetcore-v2)](https://identitydivision.visualstudio.com/IDDP/_build/latest?definitionId=516)

## About this sample

### Table of content

- [About this sample](#about-this-sample)
  - [Scenario](#scenario)
  - [Overview](#overview)
  - [User experience when using this sample](#user-experience-when-using-this-sample)
- [How to run this sample](#how-to-run-this-sample)
  - [Step 1: In the downloaded folder](#step-1-in-the-downloaded-folder)
  - [Step 2:  Update the Registration for the sample with your Microsoft Entra tenant](#step-2-update-the-registration-for-the-sample-with-your-azure-active-directory-tenant)
  - [Step 3: Run the sample](#step-3-run-the-sample)
  - [Alternative architecture](#alternative-architecture)
- [How was the code created](#how-was-the-code-created)
- [Next chapter of the tutorial: the Web API itself calls another downstream Web API](#next-chapter-of-the-tutorial-the-web-api-itself-calls-another-downstream-web-api)
- [How to deploy this sample to Azure](#how-to-deploy-this-sample-to-azure)
- [Community Help and Support](#community-help-and-support)
- [Contributing](#contributing)
- [More information](#more-information)

### Scenario

In the second chapter, we extend our protected Web API to call a downstream API (Microsoft Graph) to provide additional value.

### Overview

With respect to the previous chapter of the tutorial, this chapter adds the following steps:

3. When the client calls the Web API, the Web API acquires another token to call Microsoft Graph 
4. then the Web API calls Microsoft graph

![Topology](./ReadmeFiles/topology.png)

### User experience when using this sample

The user experience on the client application is similar to the one in the first chapter, except that, when the signed-in user adds todo list items, the Web API appends the name of the user to the todo item (between parenthesis). This is done by calling Microsoft Graph.

![TodoList Client](./ReadmeFiles/todolist-client.png)

## How to run this sample

### Step 1: In the downloaded folder

From your shell or command line:

```Shell
cd "2. Web API now calls Microsoft Graph"
```

### Step 2:  Update the Registration for the sample with your Microsoft Entra tenant

#### Update Registration for the service app (TodoListService (active-directory-dotnet-native-aspnetcore-v2))

1. In **App registrations** page, find the *TodoListService (active-directory-dotnet-native-aspnetcore-v2)* app
2. In the app's registration screen, click on the **Certificates & secrets** blade in the left to open the page where we can generate secrets and upload certificates.
3. In the **Client secrets** section, click on **New client secret**:
   - Type a key description (for instance `app secret`),
   - Select one of the available key durations (**In 1 year**, **In 2 years**, or **Never Expires**) as per your security concerns.
   - The generated key value will be displayed when you click the **Add** button. Copy the generated value for use in the steps later.
   - You'll need this key later in your code's configuration files. This key value will not be displayed again, and is not retrievable by any other means, so make sure to note it from the Microsoft Entra admin center before navigating to any other screen or blade.		 
4. In the app's registration screen, click on the **API permissions** blade in the left to open the page where we add access to the APIs that your application needs.
   - Click the **Add a permission** button and then,
   - Ensure that the **Microsoft APIs** tab is selected.
   - In the *Commonly used Microsoft APIs* section, click on **Microsoft Graph**
   - In the **Delegated permissions** section, select the **User.Read** in the list. Use the search box if necessary.
   - Click on the **Add permissions** button at the bottom.
																																						
#### Configure the  service app (TodoListService (active-directory-dotnet-native-aspnetcore-v2)) to use your app registration

1. Find the app key `ClientSecret` and replace the existing value with the key you saved during the creation of the `TodoListService (active-directory-dotnet-native-aspnetcore-v2))` app, in the Microsoft Entra admin center.

#### Configure Known Client Applications for service (TodoListService (active-directory-dotnet-native-aspnetcore-v2))

For a middle-tier Web API `TodoListService (active-directory-dotnet-native-aspnetcore-v2))` to be able to call a downstream Web API, the middle tier app needs to be granted the required permissions as well.
However, since the middle tier cannot interact with the signed-in user, it needs to be explicitly bound to the client app in its Microsoft Entra ID registration.
This binding merges the permissions required by both the client and the middle tier Web API and presents it to the end user in a single consent dialog. The user then consent to this combined set of permissions.

To achieve this, you need to add the **Application Id** of the client app, in the Manifest of the Web API in the `knownClientApplications` property. Here's how:																				

1. In the [Microsoft Entra admin center](https://entra.microsoft.com), navigate to your `TodoListService (active-directory-dotnet-native-aspnetcore-v2)` app registration, and select **Manifest** section.
1. In the manifest editor, change the `"knownClientApplications": []` line so that the array contains 
   the Client ID of the client application (`TodoListClient ((active-directory-dotnet-native-aspnetcore-v2)`) as an element of the array.

    For instance:											

    ```json
    "knownClientApplications": ["ca8dca8d-f828-4f08-82f5-325e1a1c6428"],
    ```

2. **Save** the changes to the manifest.

### Step 3: Run the sample

Clean the solution, rebuild the solution, and run it

> [Consider taking a moment to share your experience with us.](https://forms.office.com/Pages/ResponsePage.aspx?id=v4j5cvGGr0GRqy180BHbR73pcsbpbxNJuZCMKN0lURpUNDVNMlg5UlVWVDlVNFhJMUZFRlNEMU5LRiQlQCN0PWcu)
 
### Alternative architecture

This part of the sample uses different client ID for the client and the service. If both the client and the service are part of same application topology, you might want to use the same client ID in the Client and the Service. This approach is described in the third part of the tutorial [3.-Web-api-call-Microsoft-graph-for-personal-accounts](../3.-Web-api-call-Microsoft-graph-for-personal-accounts/README-incremental.md)

## How was the code created

For details about the way the code to protect the Web API was created, see [How was the code created](../1.%20Desktop%20app%20calls%20Web%20API/README.md#How-was-the-code-created) section, of the README.md file located in the sibling folder named **1. Desktop app calls Web API**.

This section, here, is only about the additional code added to let the Web API call the Microsoft Graph

### On the service side

#### Modify the `Startup.cs` file to add a token received by the Web API to the MSAL.NET cache

Update `Startup.cs` file:

- In the `ConfigureServices` method, replace:

  ```CSharp
  services.AddMicrosoftWebApiAuthentication(Configuration);
																			   
   ```

  by

  ```csharp
  services.AddMicrosoftIdentityWebApiAuthentication(Configuration)
          .EnableTokenAcquisitionToCallDownstreamApi()
          .AddInMemoryTokenCaches();
  ```

  `EnableTokenAcquisitionToCallDownstreamApi` subscribes to the `OnTokenValidated` JwtBearerAuthentication event, and in this event, adds the user account into MSAL.NET's user token cache.

  `AddInMemoryTokenCaches` adds an in memory token cache provider, which will cache the Access Tokens acquired for the downstream Web API.																			  
#### Modify the TodoListController.cs file to add information on the todo item about its owner

In the `TodoListController.cs` file, the `Post` method is modified by replacing

```CSharp
todoStore.Add(new TodoItem { Owner = owner, Title = Todo.Title });
```

with

```CSharp
User user = _graphServiceClient.Me.Request().GetAsync().GetAwaiter().GetResult();
string title = string.IsNullOrWhiteSpace(user.UserPrincipalName) ? todo.Title : $"{todo.Title} ({user.UserPrincipalName})";
TodoStore.Add(new TodoItem { Owner = owner, Title = title });
```

The work of calling Microsoft Graph to get the owner name is done by `GraphServiceClient`, which is set up by Microsoft Identity Web.

`GraphServiceClient`

- gets an access token for the Microsoft Graph on behalf of the user (leveraging the in-memory token cache, which was added in the `Startup.cs`), and
- calls the Microsoft Graph `/me` endpoint to retrieve the name of the user.

### Handling required interactions with the user (dynamic consent, MFA, etc.)

Please refer to [Microsoft.Identity.Web\wiki](https://github.com/AzureAD/microsoft-identity-web/wiki/web-apis) on how Web APIs should handle exceptions which require user interaction like `MFA`.

#### On the client side

On the client side, when it calls the Web API and receives a 403 with a www-Authenticate header, the client will call the `HandleChallengeFromWebApi` method, which will
										
- extract the consent URI from the www-Authenticate header,
- navigate to the consent URI provided by the Web API.

The code for `HandleChallengeFromWebApi` method is available from [TodoListClient\MainWindow.xaml.cs](https://github.com/Azure-Samples/active-directory-dotnet-native-aspnetcore-v2/blob/4f9a9bc7f08e79f1a3e908cb513c59f1976470da/2.%20Web%20API%20now%20calls%20Microsoft%20Graph/TodoListClient/MainWindow.xaml.cs)

## Next chapter of the tutorial: the Web API itself calls another downstream Web API

In the next chapter, we will enhance this Web API to call another downstream Web API (Microsoft Graph) on behalf of the user signed in to the WPF application. 

See [3.-Web-api-call-Microsoft-graph-for-personal-accounts](../3.-Web-api-call-Microsoft-graph-for-personal-accounts/README-incremental.md)

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

- to learn more about the scenario, see [Scenario: Web app that calls web APIs](https://docs.microsoft.com/en-us/azure/active-directory/develop/scenario-web-app-call-api-overview)

- to learn more about Microsoft.Identity.Web, see [Microsoft.Identity.Web/README.md](../Microsoft.Identity.Web/README.md)

- To learn more about the application registration, visit:

  - [Quickstart: Register an application with the Microsoft identity platform](https://docs.microsoft.com/en-us/azure/active-directory/develop/quickstart-register-app)
  - [Quickstart: Configure a client application to access web APIs](https://docs.microsoft.com/en-us/azure/active-directory/develop/quickstart-configure-app-access-web-apis)
  - [Quickstart: Quickstart: Configure an application to expose web APIs](https://docs.microsoft.com/en-us/azure/active-directory/develop/quickstart-configure-app-expose-web-apis)

- To learn more about the code, visit [Conceptual documentation for MSAL.NET](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/wiki#conceptual-documentation) and in particular:
  - [Acquiring tokens with the on-behalf-of flow](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/wiki/on-behalf-of)

- Articles about the Microsoft identity platform endpoint [http://aka.ms/aaddevv2](http://aka.ms/aaddevv2), with a focus on:
  - [identity platform and OAuth 2.0 On-Behalf-Of flow](https://docs.microsoft.com/en-us/azure/active-directory/develop/active-directory-v2-protocols-oauth-on-behalf-of)

- [Introduction to Identity on ASP.NET Core](https://docs.microsoft.com/en-us/aspnet/core/security/authentication/identity?view=aspnetcore-2.2&tabs=visual-studio%2Caspnetcore2x)
  - [AuthenticationBuilder](https://docs.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.authentication.authenticationbuilder?view=aspnetcore-2.0)
  - [Microsoft Entra ID with ASP.NET Core](https://docs.microsoft.com/en-us/aspnet/core/security/authentication/azure-active-directory/?view=aspnetcore-2.2)
