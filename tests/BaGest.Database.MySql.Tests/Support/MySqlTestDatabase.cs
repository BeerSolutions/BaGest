using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;
using Xunit;

namespace BaGest.Database.MySql.Tests.Support;

/// <summary>
/// A throwaway database on a real MySQL or MariaDB server. Set <c>BAGEST_TEST_MYSQL</c> to a connection string
/// whose user may create databases (for example <c>Server=localhost;Port=3306;User=root;Password=secret</c>);
/// otherwise the tests using it are skipped.
/// </summary>
public sealed class MySqlTestDatabase : IAsyncLifetime
{
    private const string ConnectionStringVariable = "BAGEST_TEST_MYSQL";

    private readonly string _serverConnectionString = Environment.GetEnvironmentVariable(ConnectionStringVariable);
    private readonly string _databaseName = "bagest_" + Guid.NewGuid().ToString("N");

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

    public MySqlContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<MySqlContext>()
            .UseMySql(ConnectionString, ServerVersion.AutoDetect(ConnectionString))
            .Options;

        return new MySqlContext(options, StaticOptionsSnapshot.ForConnectionString(ConnectionString));
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
