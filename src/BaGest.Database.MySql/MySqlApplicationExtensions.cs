using System;
using BaGest.Core;
using BaGest.Database.MySql;
using Microsoft.Extensions.DependencyInjection;

namespace BaGest;

public static class MySqlApplicationExtensions
{
    public static BaGestApplication AddMySqlDatabase(this BaGestApplication app)
    {
        app.Services.AddBaGetDbContextProvider<MySqlContext>("MySql");

        return app;
    }

    public static BaGestApplication AddMySqlDatabase(
        this BaGestApplication app,
        Action<DatabaseOptions> configure)
    {
        app.AddMySqlDatabase();
        app.Services.Configure(configure);
        return app;
    }
}
