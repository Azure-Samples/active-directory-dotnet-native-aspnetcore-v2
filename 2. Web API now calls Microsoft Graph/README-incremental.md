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
  - [Step 3:  Configure the sample code to use your Azure AD tenant](#step-3--configure-the-sample-code-to-use-your-azure-ad-tenant)
  - [Step 4:  Run the sample](#step-4-run-the-sample)
  - [Troubleshooting](#Troubleshooting)
  - [Current limitations](#Current-limitations)
- [How was the code created](#How-was-the-code-created)
- [Community Help and Support](#Community-Help-and-Support)
- [Contributing](#Contributing)
- [More information](#More-information)

### Scenario

In this scenario, you expose a Web API and protect it so that only authenticated users can access it. You want to enable authenticated users with Work and School accounts to use your Web API. Your API then also calls a downstream API (Microsoft Graph) to provide additional value to its client apps.									
### Overview

With respect to the previous chapter of the tutorial, this chapter adds the following steps:
3. When the client calls the Web API, the Web API acquires another token to call the Microsoft Graph (3)
4. then the Web API calls the graph

![Topology](./ReadmeFiles/topology.png)

### User experience when using this sample

The user experience on the client application is similar to the one in the first chapter, except that, when the signed-in user adds todo list items, the Web API appends the name of the user to the todo item (between parenthesis). This is done by calling Microsoft Graph (even if in this particular case, this would not be strictly necessary)

![TodoList Client](./ReadmeFiles/todolist-client.png)

## How to run this sample

### Step 1:  Clone or download this repository

From your shell or command line:

```Shell
cd "aspnetcore-webapi\2. Web API now calls Microsoft Graph"
```

### Step 2:  Register the sample with your Azure Active Directory tenant

There are two projects in this sample. Each needs to be separately registered in your Azure AD tenant. To register these projects, you can:

- either follow the steps [Step 2: Register the sample with your Azure Active Directory tenant](#step-2-register-the-sample-with-your-azure-active-directory-tenant) and [Step 3:  Configure the sample to use your Azure AD tenant](#choose-the-azure-ad-tenant-where-you-want-to-create-your-applications)
- or use PowerShell scripts that:
  - **automatically** creates the Azure AD applications and related objects (passwords, permissions, dependencies) for you
  - modify the Visual Studio projects' configuration files.

  > Note however that the automation will not, at this point, allow you to sign-in with a personal Microsoft account. If you want to allow sign in with personal Microsoft accounts, use the manual instructions. 

#### Using scripts

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

1. Once you've run the script, be sure to follow the manual steps. Indeed Azure AD PowerShell does not yet provide full control on applications consuming v2.0 tokens, even if this registration is already possible from the Azure portal:
   1. In the list of pages for the application registration of the *TodoListService-v2* application, select **Manifest**
      - in the manifest, search for **"accessTokenAcceptedVersion"**, and replace **null** by **2**. This property lets Azure AD know that the Web API accepts v2.0 tokens
      - search for **signInAudience** and make sure it's set to **AzureADandPersonalMicrosoftAccount**
      - Select **Save**
   1. In the application registration page for the *TodoListClient-v2* application, select the **Manifest** section:
      - search for **signInAudience** and make sure it's set to **AzureADandPersonalMicrosoftAccount**
      - Select **Save**

   > Tip: Get directly to the app registration portal page for a give app, you can navigate to the links provided in the [AppCreationScripts\createdApps.html](AppCreationScripts\createdApps.html). This file is generated by the scripts during the app registration and configuration.

5. Open the Visual Studio solution and click start

If you don't want to use this automation, follow the steps below

#### Choose the Azure AD tenant where you want to create your applications

These instructions only show the differences with the first part.

#### Register the service app (TodoListService)

1. In **App registrations** page, find the *TodoListService-2* app
1. In the app's registration screen, click on the **Certificates & secrets** blade in the left to open the page where we can generate secrets and upload certificates.
1. In the **Client secrets** section, click on **New client secret**:
   - Type a key description (for instance `app secret`),
   - Select one of the available key durations (**In 1 year**, **In 2 years**, or **Never Expires**) as per your security concerns.
   - The generated key value will be displayed when you click the **Add** button. Copy the generated value for use in the steps later.
   - You'll need this key later in your code's configuration files. This key value will not be displayed again, and is not retrievable by any other means, so make sure to note it from the Azure portal before navigating to any other screen or blade.
																 
1. In the app's registration screen, click on the **API permissions** blade in the left to open the page where we add access to the Apis that your application needs.
   - Click the **Add a permission** button and then,
   - Ensure that the **Microsoft APIs** tab is selected.
   - In the *Commonly used Microsoft APIs* section, click on **Microsoft Graph**
   - In the **Delegated permissions** section, select the **User.Read** in the list. Use the search box if necessary.
   - Click on the **Add permissions** button at the bottom.
   - [Optional] if you are a tenant admin, and agree to grant the admin consent to the web api, select **Grant admin consent for {your tenant domain}**. If you don't do
    it, users will be presented a consent screen enabling them to consent to using the web api. The consent screen will also mention the permissions required by the web api itself.
1. [Optional] Select the **Manifest** section and:
   - in the manifest, search for **"accessTokenAcceptedVersion"**, and see that its value is **2**. This property lets Azure AD know that the Web API accepts v2.0 tokens
   - Select **Save**

   > Important: it's up to the Web API to decide which version of token (v1.0 or v2.0) it accepts. Then when clients request a token for your Web API using the Microsoft identity platform endpoint, they'll get a token which version is accepted by the Web API. The code validating the tokens in this sample was written to accept both versions.

#### Register the client app (TodoListClient)

Nothing more to do more here. All was done in the first part

### Step 3: Configure the sample code to use your Azure AD tenant

By default the sample is configured to enable users to sign in with any work and school accounts (AAD) or personal Microsoft accounts.
This constraint is ensured by `ida:Tenant` in `TodoListClient\App.Config` having the value `common`.

#### Configure the TodoListService C# project

1. Open the solution in Visual Studio.
1. In the *TodoListService-v2* project, open the `appsettings.json` file.
1. Find the `ClientSecret` property and replace the existing value with the key you saved during the creation of the `TodoListService-v2` app, in the Azure portal.
   > Note
   > In chapter 1, the protected Web API did not call any downstrream API. In this chapter it does, and thus
   > it needs to acquire an access token, and becomes a confidential client. Therefore it needs to prove its identity to
   > Azure AD through a client secret (or a certificate)

#### Configure the TodoListClient C# project

Nothing more to do more here. All was done in the first part

### Step 4: Run the sample

Clean the solution, rebuild the solution, and run it

### Alternative architecture

This part of the sample uses different client ID for the client and the service and uses the on-behalf-of flow. If you are the author of both the client and the service, you might alternatively want to use the same client ID in the Client and the Service. This approach is described in the third part of the tutorial [3.-Web-api-call-Microsoft-graph-for-personal-accounts](../3.-Web-api-call-Microsoft-graph-for-personal-accounts)

## How was the code created

For details about the way the code to protect the Web API was created, see [How was the code created](../1.%20Desktop%20app%20calls%20Web%20API/README.md#How-was-the-code-created) section, of the README.md file located in the sibling folder named **1. Desktop app calls Web API**.

This section, here, is only about the additional code added to let the Web API call the Microsoft Graph

### Reference MSAL.NET

Calling a downstream API involves getting a token for this Web API. Acquiring a token is achieved by using MSAL.NET.

Reference the `Microsoft.Identity.Client` NuGet package from the TodoListService project.

Add a reference to the `Microsoft.Identity.Web` library. It contains reusable code that you can use in your Web APIs (and web apps)

### Modify the startup.cs file to add a token received by the Web API to the MSAL.NET cache

Update `Startup.cs` file:

- Add a using for `Microsoft.Identity.Client`

- In the `ConfigureServices` method, replace:

  ```CSharp
  services.AddAuthentication(AzureADDefaults.BearerAuthenticationScheme)
          .AddAzureADBearer(options => Configuration.Bind("AzureAd", options));
   ```

  by

  ```csharp
  services.AddMicrosoftWebApiAuthentication(Configuration)
          .AddMicrosoftWebApiCallsWebApi(Configuration)
          .AddInMemoryTokenCaches();
  ```

  `AddMicrosoftWebApiAuthentication` does the following:
  - add the **Jwt**BearerAuthenticationScheme (Note the replacement of BearerAuthenticationScheme by **Jwt**BearerAuthenticationScheme)
  - set the authority to be the Microsoft identity platform identity
  - sets the audiences to validate
  - register an issuer validator that accepts issuers to be in the Microsoft identity platform clouds.

  Here is an idea of the code (in the `WebApiServiceCollectionExtensions.cs` file)

  ```csharp
  services.AddAuthentication(AzureADDefaults.JwtBearerAuthenticationScheme)
          .AddAzureADBearer(options => configuration.Bind("AzureAd", options));

  // Added
  services.Configure<JwtBearerOptions>(AzureADDefaults.JwtBearerAuthenticationScheme, options =>
  {
    // This is an Microsoft identity platform Web API
    options.Authority += "/v2.0";

    // The valid audiences are both the Client ID (options.Audience) and api://{ClientID}
    options.TokenValidationParameters.ValidAudiences = new string[]
    {
          options.Audience,  $"api://{options.Audience}"
    };

    // Instead of using the default validation (validating against a single tenant
    // as we do in line of business apps),
    // we inject our own multi-tenant issuer validation logic
    // (which even accepts both V1 and V2 tokens)
    options.TokenValidationParameters.IssuerValidator = AadIssuerValidator.ForAadInstance(options.Authority).ValidateAadIssuer;
  });
  ```

  The .NET Core "services" that are added are:

  - a token acquisition service leveraging MSAL.NET
  - an in memory token cache

  The implementations of these classes are in the Microsoft.Identity.Web library (and folder), and they are designed to be reusable in your applications (Web apps and Web apis)

  `AddMicrosoftWebApiCallsWebApi` subscribes to the `OnTokenValidated` JwtBearerAuthentication event, and, in this event, adds the user account into MSAL.NET's user token cache by using the AcquireTokenOnBehalfOfUser method. This is done by the `AddAccountToCacheFromJwt` method of the `ITokenAcquisition` micro-service, which wraps MSAL.NET

  ```CSharp
  services.Configure<JwtBearerOptions>(AzureADDefaults.JwtBearerAuthenticationScheme, options =>
  {
    // When an access token for our own Web API is validated, we add it to MSAL.NET's cache
    // so that it can be used from the controllers.
    options.Events = new JwtBearerEvents();

    // Subscribing to OnTokenValidated to verify that the token has at least the Scope, scp or roles claims
    options.Events.OnTokenValidated = async context =>
    {
        // This check is required to ensure that the Web API only accepts tokens from tenants where it has been consented and provisioned.
        if (!context.Principal.Claims.Any(x => x.Type == ClaimConstants.Scope)
        && !context.Principal.Claims.Any(y => y.Type == ClaimConstants.Scp)
        && !context.Principal.Claims.Any(y => y.Type == ClaimConstants.Roles))
        {
            throw new UnauthorizedAccessException("Neither scope or roles claim was found in the bearer token.");
        }

        await Task.FromResult(0);
    };
  });
  ```

### Modify the TodoListController.cs file to add information to the todo item about its owner

In the `TodoListController.cs` file, the Post() action was modified

```CSharp
todoStore.Add(new TodoItem { Owner = owner, Title = Todo.Title });
```

is replaced by:

```CSharp
ownerName = await CallGraphAPIOnBehalfOfUser();
string title = string.IsNullOrWhiteSpace(ownerName) ? Todo.Title : $"{Todo.Title} ({ownerName})";
todoStore.Add(new TodoItem { Owner = owner, Title = title });
```

The work of calling the Microsoft Graph to get the owner name is done in `CallGraphAPIOnBehalfOfUser()`.

This method is the following. It:

- gets a token for the Microsoft Graph on behalf of the user (leveraging the token, which was added in the cache on the `TokenValidated` event in `startup.cs`)
- Calls the graph and retrieves the name of the user.

    ```CSharp
    public async Task<string> CallGraphAPIOnBehalfOfUser()
    {
        string[] scopes = new string[] { "user.read" };

        // we use MSAL.NET to get a token to call the API On Behalf Of the current user
        try
        {
            string accessToken = await tokenAcquisition.GetAccessTokenOnBehalfOfUser(HttpContext, scopes);
            dynamic me = await CallGraphApiOnBehalfOfUser(accessToken);
            return me.userPrincipalName;
        }
        catch (MsalUiRequiredException ex)
        {
            await tokenAcquisition.ReplyForbiddenWithWwwAuthenticateHeaderAsync(scopes, ex);
            return string.Empty;
        }
    }
    ```

### Handling required interactions with the user (dynamic consent, MFA, etc ...)

#### On the Web API side

An interesting piece is how `MsalUiRequiredException` are handled. These exceptions are typically sent by Azure AD when there is a need for a user interaction. This can be the case when the user needs to re-sign-in, or needs to grant some additional consent, or to obtain additional claims. For instance, the user might need to do multi-factor authentication required specifically by a specific downstream API. When these exceptions happen, given that the Web API does not have any UI, it needs to challenge the client passing all the information enabling this client to handle the interaction with the user.

This sample uses the `ReplyForbiddenWithWwwAuthenticateHeaderAsync` method of the `TokenAcquisition` service. This method uses the HttpResponse to:

- Send an HTTP 404 (Forbidden) to the client
- Set information in the www-Authenticate header of the HttpResponse with information that would enable a client to get more consent from the user that is:
  - the client ID of our Web API
  - the scopes to request
  - the claims (for conditional access, MFA etc ...)

The code for this method is available in [Microsoft.Identity.Web\Client\TokenAcquisition.cs L457-L493](https://github.com/Azure-Samples/active-directory-dotnet-native-aspnetcore-v2/blob/4f9a9bc7f08e79f1a3e908cb513c59f1976470da/Microsoft.Identity.Web/TokenAcquisition.cs#L457-L493)

#### On the client side

On the client side, when it calls the Web API and receives a 403 with a www-Authenticate header, the client will call the `HandleChallengeFromWebApi` method, which will

- extract from the www-Authenticate header
  - the consent URi
- Navigate to the consent URI provided by the Web API.

The code for `HandleChallengeFromWebApi` method is available from [TodoListClient\MainWindow.xaml.cs L162-197](https://github.com/Azure-Samples/active-directory-dotnet-native-aspnetcore-v2/blob/4f9a9bc7f08e79f1a3e908cb513c59f1976470da/2.%20Web%20API%20now%20calls%20Microsoft%20Graph/TodoListClient/MainWindow.xaml.cs#L162-L197)

## How to deploy this sample to Azure

See section [How to deploy this sample to Azure](../1.%20Desktop%20app%20calls%20Web%20API/README.md#How-to-deploy-this-sample-to-Azure) in the first part of this tutorial, as the deployment is the same.

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
  - [Azure Active Directory with ASP.NET Core](https://docs.microsoft.com/en-us/aspnet/core/security/authentication/azure-active-directory/?view=aspnetcore-2.2)
