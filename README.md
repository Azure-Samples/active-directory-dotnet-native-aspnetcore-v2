---
services: active-directory
platforms: dotnet
author: jmprieur
level: 200
client: .NET Desktop (WPF)
service: ASP.NET Core Web API
endpoint: AAD v2.0
---
# Calling an ASP.NET Core Web API from a WPF application using Azure AD V2

![Build badge](https://identitydivision.visualstudio.com/_apis/public/build/definitions/a7934fdd-dcde-4492-a406-7fad6ac00e17/497/badge)

## About this sample

### Scenario

You expose a Web API and you want to protect it so that only authenticated users can access it. You want to enable apps authenticating users with both work and school accounts or Microsoft personal accounts (formerly live account) to use your Web API.

Later you enrich your Web API by enabling it to call another Web API such as the Microsoft Graph

### Structure of the repository

This repository contains a progressive tutorial made of two parts:

Sub folder                    | Description
----------------------------- | -----------
[1. Desktop app calls Web API](1.%20Desktop%20app%20calls%20Web%20API/README.md) | This first part, presents an ASP.NET Core 2.1 Web API protected by Azure Active Directory OAuth Bearer Authentication. This Web API is  exercised by a .NET Desktop WPF application. This subfolder contains a Visual Studio solution made of two applications: the desktop application (TodoListClient), and the Web API (TodoListService) </p> ![Topology](1.%20Desktop%20app%20calls%20Web%20API/ReadmeFiles/topology.png)
[2. Web API now calls Microsoft Graph](2.%20Web%20API%20now%20calls%20Microsoft%20Graph/README.md)  | This second part presents an increment where the Web API now calls Microsoft Graph on-behalf of the user signed-in in the desktop application. In this part, the Web API uses the Microsoft Authentication Library for .NET (MSAL.NET) to acquire a token for Microsoft Graph using the [on-behalf-of](https://aka.ms/msal-net-on-behalf-of) flow </p>  ![Topology](2.%20Web%20API%20now%20calls%20Microsoft%20Graph/ReadmeFiles/topology.png)

> Note: Even if you'll probably get the most of this tutorial by going through the part in the proposed order, it's also possible to jump directly to the second part.

### User experience with this sample

#### In the first part of the tutorial

The Web API (TodoListService) maintains an in-memory collection of to-do items per authenticated user. Several applications signed-in under the same identities share the same to-do list.

The WPF application (TodoListClient) enables a user to:

- Sign in. The first time a user signs in, a consent screen is presented letting the user consent for the application accessing the TodoList Service.
- When the user has signed-in, the user sees the list of to-do items exposed by Web API for the signed-in identity
- The user can add more to-do items by clicking on *Add item* button.

Next time a user runs the application, the user is signed-in with the same identity as the application maintains a cache on disk. Users can clear the cache (which will also have the effect of signing them out)

<img src="1.%20Desktop%20app%20calls%20Web%20API/ReadmeFiles/todolist-client.png" alt="TodoList client" width="320px" />

#### In the second part of the tutorial

The second phase of the tutorials modifies the Web API so that the todo-items also mention the identity of the user adding them.

<img src="2.%20Web%20API%20now%20calls%20Microsoft%20Graph/ReadmeFiles/todolist-client.png" alt="TodoList Client with user name" width="320" />

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
cd aspnetcore-webapi
```

> Given that the name of the sample is pretty long, that it has sub-folders and so are the name of the referenced NuGet pacakges, you might want to clone it in a folder close to the root of your hard drive, to avoid file size limitations on Windows.

- Start by the first part [1. Desktop app calls Web API](1.%20Desktop%20app%20calls%20Web%20API/README.md) where you will learn how to protect a Web API with the Azure AD v2.0 endpoint.
- or if you are interested in the Web API calling another downstream Web API using the on-behalf-of flow, go directly to [2. Web API now calls Microsoft Graph](2.%20Web%20API%20now%20calls%20Microsoft%20Graph/README.md)

## Community Help and Support

Use [Stack Overflow](http://stackoverflow.com/questions/tagged/msal) to get support from the community.
Ask your questions on Stack Overflow first and browse existing issues to see if someone has asked your question before.
Make sure that your questions or comments are tagged with [`msal` `dotnet`].

If you find a bug in the sample, please raise the issue on [GitHub Issues](../../issues).

To provide a recommendation, visit the following [User Voice page](https://feedback.azure.com/forums/169401-azure-active-directory).

## Contributing

If you'd like to contribute to this sample, see [CONTRIBUTING.MD](/CONTRIBUTING.md).

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/). For more information, see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.

## Other samples and documentation

- Other samples for Azure AD v2.0 are available from [https://aka.ms/aaddevsamplesv2](https://aka.ms/aaddevsamplesv2)
- The conceptual documentation for MSAL.NET is available from [https://aka.ms/msalnet](https://aka.ms/msalnet)
- the documentation for Azure AD v2.0 is available from [https://aka.ms/aadv2](https://aka.ms/aadv2)
