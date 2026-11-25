using System;
using BaGest.Core;
using BaGest.Core.Statistics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace BaGest;

public static class BaGestApplicationExtensions
{
    public static BaGestApplication AddFileStorage(this BaGestApplication app)
    {
        app.Services.TryAddTransient<IStorageService>(provider => provider.GetRequiredService<FileStorageService>());

        return app;
    }

    public static BaGestApplication AddFileStorage(
        this BaGestApplication app,
        Action<FileSystemStorageOptions> configure)
    {
        app.AddFileStorage();
        app.Services.Configure(configure);
        return app;
    }

    public static BaGestApplication AddNullStorage(this BaGestApplication app)
    {
        app.Services.TryAddTransient<IStorageService>(provider => provider.GetRequiredService<NullStorageService>());
        return app;
    }

    public static BaGestApplication AddNullSearch(this BaGestApplication app)
    {
        app.Services.TryAddTransient<ISearchIndexer>(provider => provider.GetRequiredService<NullSearchIndexer>());
        app.Services.TryAddTransient<ISearchService>(provider => provider.GetRequiredService<NullSearchService>());
        return app;
    }

    public static BaGestApplication AddStatistics(this BaGestApplication app)
    {
        app.Services.TryAddSingleton<IStatisticsService, StatisticsService>();
        return app;
    }
}
