using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using BaGest.Core;
using Microsoft.EntityFrameworkCore;

namespace BaGest.Database.MySql;

/// <summary>
/// Provides the server version the MySQL provider needs to configure every <see cref="MySqlContext"/>.
/// </summary>
/// <remarks>
/// Detecting the version opens a connection to the server. Doing that for every context, as the provider would
/// otherwise do once per request, exhausted the server's connections. The version is therefore taken from
/// <see cref="DatabaseOptions.ServerVersion"/> or detected once per connection string, also when many requests
/// need it at the same time. A failed detection is not cached, so a server that is not reachable yet does not
/// break the application for good.
/// </remarks>
public sealed class MySqlServerVersionProvider
{
    private readonly Func<string, ServerVersion> _detectServerVersion;
    private readonly ConcurrentDictionary<string, Lazy<ServerVersion>> _detectedServerVersions = new(StringComparer.Ordinal);

    public MySqlServerVersionProvider()
        : this(connectionString => ServerVersion.AutoDetect(connectionString))
    {
    }

    internal MySqlServerVersionProvider(Func<string, ServerVersion> detectServerVersion)
    {
        _detectServerVersion = detectServerVersion ?? throw new ArgumentNullException(nameof(detectServerVersion));
    }

    public ServerVersion GetServerVersion(DatabaseOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        if (!string.IsNullOrWhiteSpace(options.ServerVersion))
        {
            return ServerVersion.Parse(options.ServerVersion);
        }

        var connectionString = options.ConnectionString;
        var detection = _detectedServerVersions.GetOrAdd(
            connectionString,
            key => new Lazy<ServerVersion>(() => _detectServerVersion(key), LazyThreadSafetyMode.ExecutionAndPublication));

        try
        {
            return detection.Value;
        }
        catch
        {
            // Lazy keeps the exception; drop this attempt so the next use detects the version again.
            _detectedServerVersions.TryRemove(new KeyValuePair<string, Lazy<ServerVersion>>(connectionString, detection));
            throw;
        }
    }
}
