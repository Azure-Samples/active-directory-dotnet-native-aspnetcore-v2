---
page_type: sample
urlFragment: 4-console-app-calls-web-api-with-pop
languages:
  - csharp  
products:
  - azure
  - azure-active-directory  
  - dotnet
description: "Sign-in a user with the Microsoft Identity Platform in a console application and call an ASP.NET Core web API using Proof of Possession token"
---
# Sign-in a user with the Microsoft Identity Platform in a console application and call an ASP.NET Core web API using Proof of Possession token

Proof of Possession, as of MSAL .NET 4.23, is no longer exposed in public client applications. Also, it is available under the Experimental Features flag.

See the dot net core [daemon sample](https://github.com/Azure-Samples/active-directory-dotnetcore-daemon-v2/tree/master/4-Call-OwnApi-Pop) showing the usage of Proof of Possession (Pop).