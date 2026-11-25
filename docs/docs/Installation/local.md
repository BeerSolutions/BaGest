# Run BaGest on your Computer

## Run BaGest

1. Install the [.NET Core SDK](https://www.microsoft.com/net/download)
2. Download and extract [BaGest's latest release](https://github.com/BeerSolutions/BaGest/releases)
3. Start the service with `dotnet BaGest.dll`
4. Browse `http://localhost:5000/` in your browser

## Run BaGest as a Windows service

BaGest can run as a Windows service. When started by the Windows service control manager it detects this automatically; on other platforms, and when started from a console, nothing changes.

1. Publish or extract BaGest to a permanent folder, for example `C:\BaGest`
2. Open an elevated command prompt and create the service (note the space after `binPath=`):

    ```shell
    sc.exe create BaGest binPath= "C:\BaGest\BaGest.exe" start= auto
    ```

3. Start the service:

    ```shell
    sc.exe start BaGest
    ```

The service uses the application folder as its working directory, so `appsettings.json` is read from the folder that contains `BaGest.exe`. To change the URLs BaGest listens on, set `Kestrel:Endpoints` or `urls` in `appsettings.json`, or the `ASPNETCORE_URLS` environment variable for the service.

To remove the service, stop it and delete it:

```shell
sc.exe stop BaGest
sc.exe delete BaGest
```

## Configure BaGest

You can modify BaGest's configurations by editing the `appsettings.json` file. For the full list of configurations, please refer to [BaGest's configuration](../configuration.md) guide.

## Publish packages

Publish your first package with:

```shell
dotnet nuget push -s http://localhost:5000/v3/index.json package.1.0.0.nupkg
```

Publish your first [symbol package](https://docs.microsoft.com/en-us/nuget/create-packages/symbol-packages-snupkg) with:

```shell
dotnet nuget push -s http://localhost:5000/v3/index.json symbol.package.1.0.0.snupkg
```

:::warning

You should secure your server by requiring an API Key to publish packages. For more information, please refer to the [Require an API Key](../configuration.md#require-an-api-key) guide.

:::

## Restore packages

You can restore packages by using the following package source:

`http://localhost:5000/v3/index.json`

Some helpful guides:

- [Visual Studio](https://docs.microsoft.com/en-us/nuget/consume-packages/install-use-packages-visual-studio#package-sources)
- [NuGet.config](https://docs.microsoft.com/en-us/nuget/reference/nuget-config-file#package-source-sections)

## Symbol server

You can load symbols by using the following symbol location:

`http://localhost:5000/api/download/symbols`

For Visual Studio, please refer to the [Configure Debugging](https://docs.microsoft.com/en-us/visualstudio/debugger/specify-symbol-dot-pdb-and-source-files-in-the-visual-studio-debugger?view=vs-2017#configure-symbol-locations-and-loading-options) guide.
