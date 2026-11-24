using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BaGest.Core;
using BaGest.Database.MySql.Tests.Support;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace BaGest.Database.MySql.Tests;

public class MySqlServerVersionProviderTests
{
    // Nothing listens on port 1, so any attempt to reach the server fails.
    private const string UnreachableServer = "Server=127.0.0.1;Port=1;Database=bagest;User=bagest;Connection Timeout=1";

    private static readonly ServerVersion DetectedVersion = ServerVersion.Parse("8.4.0-mysql");

    private int _detections;

    [Fact]
    public void ConfiguredServerVersion_IsUsedWithoutDetection()
    {
        var provider = new MySqlServerVersionProvider(CountingDetection);

        var version = provider.GetServerVersion(Options(UnreachableServer, "11.4.0-mariadb"));

        Assert.Equal("11.4.0-mariadb", version.ToString());
        Assert.Equal(0, _detections);
    }

    [Fact]
    public void WithoutConfiguredServerVersion_DetectsOncePerConnectionString()
    {
        var provider = new MySqlServerVersionProvider(CountingDetection);

        for (var i = 0; i < 5; i++)
        {
            Assert.Same(DetectedVersion, provider.GetServerVersion(Options("Server=a")));
        }

        provider.GetServerVersion(Options("Server=b"));

        Assert.Equal(2, _detections);
    }

    [Fact]
    public async Task ConcurrentFirstUses_DetectOnlyOnce()
    {
        using var release = new ManualResetEventSlim();
        var provider = new MySqlServerVersionProvider(connectionString =>
        {
            Interlocked.Increment(ref _detections);
            release.Wait(TimeSpan.FromSeconds(5));
            return DetectedVersion;
        });

        var requests = Enumerable.Range(0, 8)
            .Select(_ => Task.Run(() => provider.GetServerVersion(Options("Server=a"))))
            .ToArray();
        // Let the requests reach the detection before it completes.
        await Task.Delay(200);
        release.Set();

        Assert.All(await Task.WhenAll(requests), version => Assert.Same(DetectedVersion, version));
        Assert.Equal(1, _detections);
    }

    [Fact]
    public void FailedDetection_IsRetriedOnNextUse()
    {
        var provider = new MySqlServerVersionProvider(connectionString =>
            ++_detections == 1 ? throw new InvalidOperationException("Server not reachable yet") : DetectedVersion);

        Assert.Throws<InvalidOperationException>(() => provider.GetServerVersion(Options("Server=a")));

        Assert.Same(DetectedVersion, provider.GetServerVersion(Options("Server=a")));
        Assert.Same(DetectedVersion, provider.GetServerVersion(Options("Server=a")));
        Assert.Equal(2, _detections);
    }

    [Fact]
    public void ContextsWithConfiguredServerVersion_DoNotConnectToTheServer()
    {
        using var services = MySqlServices.Create(UnreachableServer, "8.4.0-mysql");

        // Building the model configures the context; detecting the version would fail on the unreachable server.
        for (var i = 0; i < 3; i++)
        {
            using var scope = services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<MySqlContext>();
            Assert.NotNull(context.Model);
        }
    }

    private ServerVersion CountingDetection(string connectionString)
    {
        _detections++;
        return DetectedVersion;
    }

    private static DatabaseOptions Options(string connectionString, string serverVersion = null) =>
        new() { ConnectionString = connectionString, ServerVersion = serverVersion };
}
