MvcOdata [![Build status](https://ci.appveyor.com/api/projects/status/uw1t01dfyafu42fe?svg=true)](https://ci.appveyor.com/project/ImaginaryDevelopment/mvcodata)
========

a repo for a PoC and experimentation around using [OData Controllers](http://www.asp.net/web-api/overview/odata-support-in-aspnet-web-api) and [WCF Data services](http://msdn.microsoft.com/en-us/data/odata.aspx)

[OData.csproj](https://github.com/ImaginaryDevelopment/MvcOdata/tree/master/OData) - the front end - enables ajax calls through the web layer to be made via [OData url conventions](http://www.odata.org/documentation/uri-conventions/).

[Webby.csproj](https://github.com/ImaginaryDevelopment/MvcOdata/tree/master/Webby) - Jason Gerard's auto-hosting WCF services code

[Contracts.csproj](https://github.com/ImaginaryDevelopment/MvcOdata/tree/master/Contracts) - the shared contracts assembly describes the domain objects and repositories

[WcfData.csproj](https://github.com/ImaginaryDevelopment/MvcOdata/tree/master/WcfData) - Peter Lacomb's WCF Data services layer
