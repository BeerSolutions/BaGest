using System;
using BaGest.Core;
using BaGest.Database.Sqlite;
using Microsoft.Extensions.DependencyInjection;

namespace BaGest;

public static class SqliteApplicationExtensions
{
    public static BaGestApplication AddSqliteDatabase(this BaGestApplication app)
    {
        app.Services.AddBaGetDbContextProvider<SqliteContext>("Sqlite");

        return app;
    }

    public static BaGestApplication AddSqliteDatabase(
        this BaGestApplication app,
        Action<DatabaseOptions> configure)
    {
        app.AddSqliteDatabase();
        app.Services.Configure(configure);
        return app;
    }
}
