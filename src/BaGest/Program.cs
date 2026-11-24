using System;
using System.IO;
using System.Threading.Tasks;
using BaGest.Core;
using BaGest.Web;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.WindowsServices;

namespace BaGest;

public class Program
{
    public static async Task Main(string[] args)
    {
        if (WindowsServiceHelpers.IsWindowsService())
        {
            // Make sure appsettings.json can be found when running as a Windows service,
            // which starts with C:\Windows\System32 as the working directory.
            Directory.SetCurrentDirectory(AppContext.BaseDirectory);
        }

        var host = CreateHostBuilder(args).Build();
        if (!host.ValidateStartupOptions())
        {
            return;
        }

        var app = new CommandLineApplication
        {
            Name = "baget",
            Description = "A light-weight NuGet service",
        };

        app.HelpOption(inherited: true);

        app.Command("import", import =>
        {
            import.Command("downloads", downloads =>
            {
                downloads.OnExecuteAsync(async cancellationToken =>
                {
                    using var scope = host.Services.CreateScope();
                    var importer = scope.ServiceProvider.GetRequiredService<DownloadsImporter>();

                    await importer.ImportAsync(cancellationToken);
                });
            });
        });

        app.Option("--urls", "The URLs that BaGest should bind to.", CommandOptionType.SingleValue);

        app.OnExecuteAsync(async cancellationToken =>
        {
            await host.RunMigrationsAsync(cancellationToken);
            await host.RunAsync(cancellationToken);
        });

        await app.ExecuteAsync(args);
    }

    public static IHostBuilder CreateHostBuilder(string[] args)
    {
        return Host
            .CreateDefaultBuilder(args)
            // Runs as a Windows service when started by the service control manager; no-op elsewhere.
            .UseWindowsService()
            .ConfigureAppConfiguration((ctx, config) =>
            {
                var root = Environment.GetEnvironmentVariable("BAGET_CONFIG_ROOT");

                if (!string.IsNullOrEmpty(root))
                {
                    config.SetBasePath(root);
                }

                // Optionally load secrets from files in the conventional path
                config.AddKeyPerFile("/run/secrets", optional: true);
            })
            .ConfigureWebHostDefaults(web =>
            {
                web.ConfigureKestrel(options =>
                {
                    // Remove the upload limit from Kestrel. If needed, an upload limit can
                    // be enforced by a reverse proxy server, like IIS.
                    options.Limits.MaxRequestBodySize = null;
                });

                web.UseStartup<Startup>();
            });
    }
}
