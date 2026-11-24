using BaGest.Core;
using Microsoft.Extensions.DependencyInjection;

namespace BaGest.Database.MySql.Tests.Support;

internal static class MySqlServices
{
    /// <summary>
    /// Registers the MySQL database the way the application does, so contexts are configured by
    /// <see cref="MySqlContext"/> itself rather than by the test.
    /// </summary>
    public static ServiceProvider Create(string connectionString, string serverVersion = null)
    {
        var services = new ServiceCollection();
        services.Configure<BaGestOptions>(options => options.Database = new DatabaseOptions
        {
            Type = "MySql",
            ConnectionString = connectionString,
            ServerVersion = serverVersion,
        });
        new BaGestApplication(services).AddMySqlDatabase();

        return services.BuildServiceProvider();
    }
}
