using System;
using System.Threading;
using System.Threading.Tasks;
using BaGest.Database.MySql.Tests.Support;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace BaGest.Database.MySql.Tests;

public class MySqlConnectionUsageTests : IAsyncLifetime
{
    private const int ContextCount = 10;

    private readonly MySqlTestDatabase _database = new();

    public Task InitializeAsync() => _database.InitializeAsync();

    public Task DisposeAsync() => _database.DisposeAsync();

    [SkippableFact]
    public async Task ContextsWithoutConfiguredServerVersion_DetectItOnlyOnce()
    {
        Skip.IfNot(_database.IsAvailable);

        // Pooling is disabled in the test connection string, so every opened connection reaches the server.
        await using var services = MySqlServices.Create(_database.ConnectionString);
        await RunAsync(services, context => context.RunMigrationsAsync(CancellationToken.None));

        var connectionsBefore = await _database.GetServerConnectionCountAsync();
        for (var i = 0; i < ContextCount; i++)
        {
            await RunAsync(services, context => context.Packages.CountAsync());
        }
        var connectionsAfter = await _database.GetServerConnectionCountAsync();

        // One connection per query and one for reading the counter; detecting the version would add one per context.
        Assert.Equal(ContextCount + 1, connectionsAfter - connectionsBefore);
    }

    private static async Task RunAsync(ServiceProvider services, Func<MySqlContext, Task> action)
    {
        await using var scope = services.CreateAsyncScope();
        await action(scope.ServiceProvider.GetRequiredService<MySqlContext>());
    }
}
