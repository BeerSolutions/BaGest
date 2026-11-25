using System;
using System.Threading;
using System.Threading.Tasks;
using BaGest.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace BaGest.Web;

public static class IHostExtensions
{
    public static IHostBuilder UseBaGest(this IHostBuilder host, Action<BaGestApplication> configure)
    {
        return host.ConfigureServices(services =>
        {
            services.AddBaGestWebApplication(configure);
        });
    }

    public static async Task RunMigrationsAsync(
        this IHost host,
        CancellationToken cancellationToken = default)
    {
        // Run migrations if necessary.
        var options = host.Services.GetRequiredService<IOptions<BaGestOptions>>();

        if (options.Value.RunMigrationsAtStartup)
        {
            using var scope = host.Services.CreateScope();
            var ctx = scope.ServiceProvider.GetService<IContext>();
            if (ctx != null)
            {
                await ctx.RunMigrationsAsync(cancellationToken);
            }
        }
    }

    public static bool ValidateStartupOptions(this IHost host)
    {
        return host
            .Services
            .GetRequiredService<ValidateStartupOptions>()
            .Validate();
    }
}
