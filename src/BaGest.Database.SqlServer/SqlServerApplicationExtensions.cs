using System;
using BaGest.Core;
using BaGest.Database.SqlServer;
using Microsoft.Extensions.DependencyInjection;

namespace BaGest;

public static class SqlServerApplicationExtensions
{
    public static BaGestApplication AddSqlServerDatabase(this BaGestApplication app)
    {
        app.Services.AddBaGetDbContextProvider<SqlServerContext>("SqlServer");

        return app;
    }

    public static BaGestApplication AddSqlServerDatabase(
        this BaGestApplication app,
        Action<DatabaseOptions> configure)
    {
        app.AddSqlServerDatabase();
        app.Services.Configure(configure);
        return app;
    }
}
