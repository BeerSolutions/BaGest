# BaGest 🥖🛒

BaGest is a lightweight [NuGet] and [symbol] server, written in C#.

<p align="center">
  <img width="100%" src="https://user-images.githubusercontent.com/737941/50140219-d8409700-0258-11e9-94c9-dad24d2b48bb.png">
</p>

![Build status] [![Release]][Release link] [![Container image]][Container link]

## 🚀 Getting Started

With Docker:

1. `docker run -p 5000:8080 -v ./bagest-data:/data ghcr.io/beersolutions/bagest:latest`
2. Browse `http://localhost:5000/` in your browser

With .NET:

1. Install the [.NET SDK]
2. Download and extract [BaGest's latest release]
3. Start the service with `dotnet BaGest.dll`
4. Browse `http://localhost:5000/` in your browser

With IIS ([official microsoft documentation](https://learn.microsoft.com/aspnet/core/host-and-deploy/iis)):

1. Install the [hosting bundle](https://dotnet.microsoft.com/permalink/dotnetcore-current-windows-runtime-bundle-installer)
2. Download the [zip release](https://github.com/BeerSolutions/BaGest/releases) of BaGest
3. Unpack the zip file contents to a folder of your choice
4. Create a new or configure an existing IIS site to point its physical path to the folder where you unpacked the zip file

For more information, please refer to the [documentation].

## 📦 Features

* **Cross-platform**: runs on Windows, macOS, and Linux!
* **ARM** (64bit) **support**. You can host your NuGets on a device like Raspberry Pi!
* **Cloud native**: supports [Docker][Docker doc link], [AWS][AWS doc link], [Google Cloud][GCP doc link], [Alibaba Cloud][Aliyun doc link]，[Tencent Cloud][Tencent doc link]
* **Offline support**: [Mirror a NuGet server][Read through caching] to speed up builds and enable offline downloads

## 🤝 Contributing

We welcome contributions! Check out the [Contributing Guide](CONTRIBUTING.md) to get started.

## 🔄 Migrating from BaGetter

* Configuration keys and environment variables (`Database__*`, `Storage__*`, `ApiKey`, ...) are unchanged.
* The default SQLite file is now `bagest.db` (was `bagetter.db`). If you relied on the default connection string, rename the file or set `Database__ConnectionString` explicitly; the Docker image uses `/data/db/bagest.db`.
* The image is published as `ghcr.io/beersolutions/bagest`, the release archive is `bagest-<version>.zip` with `BaGest.dll` as entry point, and the NuGet packages are named `BaGest.*`.
* The Helm chart is now named `bagest`, so resource names change on upgrade.
* Database schema and migrations are unchanged.

## 📄 License

This project is licensed under the [MIT License](LICENSE).

## 📞 Contact

If you have questions, feel free to open an [issue].

## 🤝🏼 Contributors

Thanks to everyone who helps to make BaGest better!

<a href="https://github.com/BeerSolutions/BaGest/graphs/contributors">
  <img src="https://contrib.rocks/image?repo=BeerSolutions/BaGest" />
</a>

## 🥖 Origin

BaGest continues [BaGetter], a community-driven fork of [BaGet] by Loic Sharma. BaGet laid the foundation of this server, and BaGetter carried it forward with new features and a lively community. Many thanks to Loic Sharma, the BaGetter maintainers and everyone who contributed to either project; their work lives on in BaGest and their copyright notices are preserved in the [LICENSE](LICENSE).


[Build status]: https://img.shields.io/github/actions/workflow/status/BeerSolutions/BaGest/main.yml?logo=github&logoColor=fff

[Release]: https://img.shields.io/github/v/release/BeerSolutions/BaGest?include_prereleases&logo=github&logoColor=fff
[Release link]: https://github.com/BeerSolutions/BaGest/releases
[Container image]: https://img.shields.io/badge/ghcr.io-beersolutions%2Fbagest-blue?logo=docker&logoColor=fff
[Container link]: https://github.com/BeerSolutions/BaGest/pkgs/container/bagest
[Docker doc link]: https://beersolutions.github.io/BaGest/docs/Installation/docker

[NuGet]: https://learn.microsoft.com/nuget/what-is-nuget
[symbol]: https://docs.microsoft.com/en-us/windows/desktop/debug/symbol-servers-and-symbol-stores
[.NET SDK]: https://www.microsoft.com/net/download
[Issue]: https://github.com/BeerSolutions/BaGest/issues
[BaGet]: https://github.com/loic-sharma/BaGet
[BaGetter]: https://github.com/bagetter/BaGetter

[BaGest's latest release]: https://github.com/BeerSolutions/BaGest/releases

[Documentation]: https://beersolutions.github.io/BaGest/
[Read through caching]: https://beersolutions.github.io/BaGest/docs/configuration#enable-read-through-caching
[AWS doc link]: https://beersolutions.github.io/BaGest/docs/Installation/aws
[GCP doc link]: https://beersolutions.github.io/BaGest/docs/Installation/gcp
[Aliyun doc link]: https://beersolutions.github.io/BaGest/docs/Installation/aliyun
[Tencent doc link]: https://beersolutions.github.io/BaGest/docs/Installation/tencent
