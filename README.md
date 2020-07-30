---
languages:
  - csharp
products:
  - aspnet
  - azure
  - azure-active-directory
page_type: sample
description: "A sample that shows how to protect an ASP.NET Core Web API using Microsoft Identity Platform."
---

# Protecting an ASP.NET Core Web API using Microsoft Identity Platform 

[![Build status](https://identitydivision.visualstudio.com/IDDP/_apis/build/status/AAD%20Samples/.NET%20client%20samples/active-directory-dotnet-native-aspnetcore-v2)](https://identitydivision.visualstudio.com/IDDP/_build/latest?definitionId=516)

## About this sample

### Scenario

In this scenario, we would be protecting a Web API using the Microsoft Identity Platform. This will ensure that the Web API is only accessible to authenticated users. In these samples we would work with Apps who authenticate users using both **Work and School accounts** or **Microsoft Personal accounts (formerly live account)**.

We will also enrich the Web API to use the [on-behalf of flow](https://docs.microsoft.com/azure/active-directory/develop/v2-oauth2-on-behalf-of-flow) to call other Web APIs protected by the Microsoft Identity Platform.

### Pre-requisites

- Install .NET Core for Windows by following the instructions at [dot.net/core](https://dot.net/core), which will include [Visual Studio 2019](https://aka.ms/vsdownload).
- An Internet connection
- An Azure Active Directory (Azure AD) tenant. For more information on how to get an Azure AD tenant, see [How to get an Azure AD tenant](https://azure.microsoft.com/en-us/documentation/articles/active-directory-howto-tenant/)
- A user account in your Azure AD tenant, or a Microsoft personal account

### Step 1:  Clone or download this repository

From your shell or command line:

```Shell
git clone https://github.com/Azure-Samples/active-directory-dotnet-native-aspnetcore-v2.git
```

> Given that the name of the sample is pretty long, that it has sub-folders and so are the name of the referenced NuGet packages, you might want to clone it in a folder close to the root of your hard drive, to avoid file size limitations on Windows.

### Structure of the repository

This repository contains a progressive tutorial made up of the following chapters:

Sub folder                    | Description
----------------------------- | -----------
[1. Desktop app calls a protected Web API](1.%20Desktop%20app%20calls%20Web%20API/README-incremental.md) | In the first chapter, we would protect an ASP.Net Core Web API using the Microsoft Identity Platform. The Web API will be protected using Azure Active Directory OAuth Bearer Authorization. The Web API is called by a .NET Desktop WPF application. In this chapter, the desktop application uses the [Microsoft Authentication Library for .NET (MSAL.NET)](https://aka.ms/msal-net) to sign-in the user to acquire an [Access Token](https://docs.microsoft.com/azure/active-directory/develop/access-tokens) for the protected Web API. </p> ![Topology](1.%20Desktop%20app%20calls%20Web%20API/ReadmeFiles/topology.png)
[2. Web API now calls Microsoft Graph](2.%20Web%20API%20now%20calls%20Microsoft%20Graph/README-incremental.md)  | In the second chapter we enhance the Web API to call Microsoft Graph using the on-behalf flow to represent the user signed-in in the desktop application to Microsoft Graph. In this chapter, the Web API uses the [MSAL.NET](https://aka.ms/msal-net) to acquire an [Access Token](https://docs.microsoft.com/azure/active-directory/develop/access-tokens) for Microsoft Graph using the [on-behalf-of](https://aka.ms/msal-net-on-behalf-of) flow </p>  ![Topology](2.%20Web%20API%20now%20calls%20Microsoft%20Graph/ReadmeFiles/topology.png)
[3.-Web API and client share the same app id and signs-in MSA users](3.-Web-api-call-Microsoft-graph-for-personal-accounts/README-incremental.md)  | In the third chapter, we present another pattern where a tightly-knit client and Web API share the same client id (app id). In this one we will  sign-in users with Microsoft Personal Accounts. The sign-in flow and the call to Web API uses the same flow as chapter 2. </p>  ![Topology](3.-Web-api-call-Microsoft-graph-for-personal-accounts/ReadmeFiles/topology.png)
[4. Client app calls a Web API with Proof of Possession(PoP)](4.-Console-app-calls-web-API-with-PoP/README-incremental.md) | In this chapter, the ASP.NET Core Web API is expecting an [Access Token](https://docs.microsoft.com/azure/active-directory/develop/access-tokens) with a Proof of Possession key. </p> ![Topology](1.%20Desktop%20app%20calls%20Web%20API/ReadmeFiles/topology.png)

> Note: We advise you to follow the tutorial in the order presented, but you can still try out individual chapters if you so wish.

- Start with the chapter [1. Desktop app calls Web API](1.%20Desktop%20app%20calls%20Web%20API/README-incremental.md) where you will learn how to protect a Web API with the Azure AD.

## Community Help and Support

Use [Stack Overflow](http://stackoverflow.com/questions/tagged/msal) to get support from the community.
Ask your questions on Stack Overflow first and browse existing issues to see if someone has asked your question before.
Make sure that your questions or comments are tagged with [`msal` `dotnet`].

If you find a bug in the sample, please open an issue on [GitHub Issues](https://github.com/Azure-Samples/active-directory-dotnet-native-aspnetcore-v2/issues).

To provide a recommendation, visit the following [User Voice page](https://feedback.azure.com/forums/169401-azure-active-directory).

## Contributing

If you'd like to contribute to this sample, see [CONTRIBUTING.MD](/CONTRIBUTING.md).

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/). For more information, see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.

## Other samples and documentation

- Other samples for Microsoft identity platform are available from [https://aka.ms/aaddevsamplesv2](https://aka.ms/aaddevsamplesv2).
- The conceptual documentation for MSAL.NET is available from [https://aka.ms/msalnet](https://aka.ms/msalnet).
- The documentation for identity platform is available from [https://aka.ms/aadv2](https://aka.ms/aadv2).
