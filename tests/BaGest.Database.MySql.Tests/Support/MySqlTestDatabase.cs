using System;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;
using Xunit;

namespace BaGest.Database.MySql.Tests.Support;

/// <summary>
/// A throwaway database on a real MySQL or MariaDB server. Set <c>BAGEST_TEST_MYSQL</c> to a connection string
/// whose user may create databases (for example <c>Server=localhost;Port=3306;User=root;Password=secret</c>);
/// otherwise the tests using it are skipped. Use a server no other client connects to while the tests run:
/// <see cref="MySqlConnectionUsageTests"/> counts the connections the whole server accepts.
/// </summary>
public sealed class MySqlTestDatabase : IAsyncLifetime
{
    private const string ConnectionStringVariable = "BAGEST_TEST_MYSQL";

    private readonly string _serverConnectionString = Environment.GetEnvironmentVariable(ConnectionStringVariable);
    private readonly string _databaseName = "bagest_" + Guid.NewGuid().ToString("N");
    private readonly MySqlServerVersionProvider _serverVersionProvider = new();

    public bool IsAvailable => _serverConnectionString != null;

    public string ConnectionString =>
        new MySqlConnectionStringBuilder(_serverConnectionString) { Database = _databaseName, Pooling = false }.ConnectionString;

    public string ServerConnectionString => _serverConnectionString;

    public async Task InitializeAsync()
    {
        if (!IsAvailable) return;
        await ExecuteAsync(_serverConnectionString, $"CREATE DATABASE `{_databaseName}`");
    }

    public async Task DisposeAsync()
    {
        if (!IsAvailable) return;
        await ExecuteAsync(_serverConnectionString, $"DROP DATABASE IF EXISTS `{_databaseName}`");
    }

    /// <summary>
    /// Creates a context that configures itself from the connection string, as it does in the application.
    /// </summary>
    public MySqlContext CreateContext() => new(
        new DbContextOptions<MySqlContext>(),
        StaticOptionsSnapshot.ForConnectionString(ConnectionString),
        _serverVersionProvider);

    /// <summary>
    /// The number of connection attempts the server has accepted since it started, including this query's own.
    /// </summary>
    public async Task<long> GetServerConnectionCountAsync()
    {
        await using var connection = new MySqlConnection(ConnectionString);
        await connection.OpenAsync();
        await using var command = new MySqlCommand("SHOW GLOBAL STATUS LIKE 'Connections'", connection);
        await using var reader = await command.ExecuteReaderAsync();
        await reader.ReadAsync();
        return long.Parse(reader.GetString(1), CultureInfo.InvariantCulture);
    }

    public async Task<T> QueryScalarAsync<T>(string sql)
    {
        await using var connection = new MySqlConnection(ConnectionString);
        await connection.OpenAsync();
        await using var command = new MySqlCommand(sql, connection);
        return (T)await command.ExecuteScalarAsync();
    }

    public Task ExecuteAsync(string sql) => ExecuteAsync(ConnectionString, sql);

    private static async Task ExecuteAsync(string connectionString, string sql)
    {
        await using var connection = new MySqlConnection(connectionString);
        await connection.OpenAsync();
        await using var command = new MySqlCommand(sql, connection);
        await command.ExecuteNonQueryAsync();
    }
}
