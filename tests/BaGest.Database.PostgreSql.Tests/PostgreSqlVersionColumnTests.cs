using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BaGest.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Options;
using Npgsql;
using Xunit;

namespace BaGest.Database.PostgreSql.Tests;

/// <summary>
/// Runs against a real PostgreSQL server. Set <c>BAGEST_TEST_POSTGRES</c> to a connection string
/// whose user may create databases (for example
/// <c>Host=localhost;Port=5432;Username=postgres;Password=secret</c>); otherwise the tests are skipped.
/// </summary>
public class PostgreSqlVersionColumnTests : IAsyncLifetime
{
    private const string ConnectionStringVariable = "BAGEST_TEST_POSTGRES";
    private const string LastMigrationBeforeFix = "20210919191649_RemoveReleaseNotesMaxLength";

    private readonly string _serverConnectionString = Environment.GetEnvironmentVariable(ConnectionStringVariable);
    private readonly string _databaseName = "bagest_" + Guid.NewGuid().ToString("N");

    private string DatabaseConnectionString =>
        new NpgsqlConnectionStringBuilder(_serverConnectionString) { Database = _databaseName, Pooling = false }.ConnectionString;

    public async Task InitializeAsync()
    {
        if (_serverConnectionString == null) return;
        await ExecuteOnServerAsync($"CREATE DATABASE \"{_databaseName}\"");
    }

    public async Task DisposeAsync()
    {
        if (_serverConnectionString == null) return;
        await ExecuteOnServerAsync($"DROP DATABASE IF EXISTS \"{_databaseName}\" WITH (FORCE)");
    }

    [SkippableFact]
    public async Task FreshDatabase_VersionColumnIsCaseInsensitive()
    {
        Skip.If(_serverConnectionString == null);

        await using var context = CreateContext();
        await context.RunMigrationsAsync(CancellationToken.None);

        Assert.Equal("citext", await GetVersionColumnTypeAsync());
        await AssertVersionLookupIsCaseInsensitiveAsync(context);
    }

    [SkippableFact]
    public async Task DatabaseWithCaseSensitiveVersionColumn_IsRepairedByMigrations()
    {
        Skip.If(_serverConnectionString == null);

        await using var context = CreateContext();

        // Reproduce an installation that went through the migrations but ended up with a varchar column.
        await context.GetService<IMigrator>().MigrateAsync(LastMigrationBeforeFix);
        await ExecuteOnDatabaseAsync("ALTER TABLE \"Packages\" ALTER COLUMN \"Version\" TYPE character varying(64)");
        Assert.Equal("varchar", await GetVersionColumnTypeAsync());

        await context.RunMigrationsAsync(CancellationToken.None);

        Assert.Equal("citext", await GetVersionColumnTypeAsync());
        await AssertVersionLookupIsCaseInsensitiveAsync(context);
    }

    private async Task AssertVersionLookupIsCaseInsensitiveAsync(PostgreSqlContext context)
    {
        context.Packages.Add(new Package
        {
            Id = "Test.Package",
            NormalizedVersionString = "1.0.0-Beta",
            OriginalVersionString = "1.0.0-Beta",
            Published = DateTime.UtcNow,
        });
        await context.SaveChangesAsync();

        var found = await context.Packages
            .Where(p => p.Id == "test.package" && p.NormalizedVersionString == "1.0.0-beta")
            .CountAsync();

        Assert.Equal(1, found);
    }

    private PostgreSqlContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<PostgreSqlContext>()
            .UseNpgsql(DatabaseConnectionString)
            .Options;

        var bagestOptions = new BaGestOptions { Database = new DatabaseOptions { ConnectionString = DatabaseConnectionString } };

        return new PostgreSqlContext(options, new StaticOptionsSnapshot(bagestOptions));
    }

    private async Task<string> GetVersionColumnTypeAsync()
    {
        await using var connection = new NpgsqlConnection(DatabaseConnectionString);
        await connection.OpenAsync();
        await using var command = new NpgsqlCommand(
            "SELECT udt_name FROM information_schema.columns WHERE table_name = 'Packages' AND column_name = 'Version'",
            connection);
        return (string)await command.ExecuteScalarAsync();
    }

    private async Task ExecuteOnServerAsync(string sql) => await ExecuteAsync(_serverConnectionString, sql);

    private async Task ExecuteOnDatabaseAsync(string sql) => await ExecuteAsync(DatabaseConnectionString, sql);

    private static async Task ExecuteAsync(string connectionString, string sql)
    {
        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();
        await using var command = new NpgsqlCommand(sql, connection);
        await command.ExecuteNonQueryAsync();
    }

    private sealed class StaticOptionsSnapshot(BaGestOptions value) : IOptionsSnapshot<BaGestOptions>
    {
        public BaGestOptions Value => value;
        public BaGestOptions Get(string name) => value;
    }
}
