using System;
using BaGest.Core;
using BaGest.Database.PostgreSql;
using Microsoft.Extensions.DependencyInjection;

namespace BaGest;

public static class PostgreSqlApplicationExtensions
{
    public static BaGestApplication AddPostgreSqlDatabase(this BaGestApplication app)
    {
        app.Services.AddBaGetDbContextProvider<PostgreSqlContext>("PostgreSql");

        return app;
    }

    public static BaGestApplication AddPostgreSqlDatabase(
        this BaGestApplication app,
        Action<DatabaseOptions> configure)
    {
        app.AddPostgreSqlDatabase();
        app.Services.Configure(configure);
        return app;
    }
}
