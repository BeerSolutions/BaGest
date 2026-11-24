# BaGest Source Code

These folders contain the core components of BaGest:

* `BaGest` - The app's entry point that glues everything together.
* `BaGest.Core` - BaGest's core logic and services.
* `BaGest.Web` - The [NuGet server APIs](https://docs.microsoft.com/en-us/nuget/api/overview) and web UI.
* `BaGest.Protocol` - Libraries to interact with [NuGet servers' APIs](https://docs.microsoft.com/en-us/nuget/api/overview).

These folders contain database-specific components of BaGest:

* `BaGest.Database.MySql` - BaGest's MySQL database provider.
* `BaGest.Database.PostgreSql` - BaGest's PostgreSql database provider.
* `BaGest.Database.Sqlite` - BaGest's SQLite database provider.
* `BaGest.Database.SqlServer` - BaGest's Microsoft SQL Server database provider.

These folders contain cloud-specific components of BaGest:

* `BaGest.Aliyun` - BaGest's Alibaba Cloud(Aliyun) provider.
* `BaGest.Aws` - BaGest's Amazon Web Services provider.
* `BaGest.Azure` - BaGest's Azure provider.
* `BaGest.Gcp` - BaGest's Google Cloud Platform provider.
* `BaGest.Tencent` - BaGest's Tencent Cloud Platform provider.

