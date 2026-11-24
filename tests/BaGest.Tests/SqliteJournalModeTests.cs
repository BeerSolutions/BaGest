using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using BaGest.Core;
using BaGest.Database.Sqlite;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Xunit;

namespace BaGest.Tests;

public class SqliteJournalModeTests : IDisposable
{
    private readonly string _directory = Path.Combine(Path.GetTempPath(), "bagest-journal-" + Guid.NewGuid().ToString("N"));

    public SqliteJournalModeTests()
    {
        Directory.CreateDirectory(_directory);
    }

    [Theory]
    [InlineData("WAL", "wal")]
    [InlineData("wal", "wal")]
    [InlineData("DELETE", "delete")]
    [InlineData("truncate", "truncate")]
    public async Task AppliesConfiguredJournalMode(string configured, string expected)
    {
        var connectionString = $"Data Source={Path.Combine(_directory, "bagest.db")}";

        await MigrateAsync(connectionString, configured);

        Assert.Equal(expected, await ReadJournalModeAsync(connectionString));
    }

    [Fact]
    public async Task SwitchesAnExistingDatabaseBackToDelete()
    {
        var connectionString = $"Data Source={Path.Combine(_directory, "bagest.db")}";

        await MigrateAsync(connectionString, "WAL");
        Assert.Equal("wal", await ReadJournalModeAsync(connectionString));

        await MigrateAsync(connectionString, "DELETE");
        Assert.Equal("delete", await ReadJournalModeAsync(connectionString));
    }

    [Fact]
    public async Task LeavesJournalModeUnchangedWhenNotConfigured()
    {
        var connectionString = $"Data Source={Path.Combine(_directory, "bagest.db")}";

        await MigrateAsync(connectionString, "DELETE");
        Assert.Equal("delete", await ReadJournalModeAsync(connectionString));

        await MigrateAsync(connectionString, journalMode: null);
        Assert.Equal("delete", await ReadJournalModeAsync(connectionString));
    }

    [Theory]
    [InlineData("FAST")]
    [InlineData("WAL; DROP TABLE Packages")]
    public async Task RejectsUnsupportedJournalMode(string configured)
    {
        var connectionString = $"Data Source={Path.Combine(_directory, "bagest.db")}";

        await Assert.ThrowsAsync<InvalidOperationException>(() => MigrateAsync(connectionString, configured));
    }

    public void Dispose()
    {
        SqliteConnection.ClearAllPools();
        Directory.Delete(_directory, recursive: true);
    }

    private static async Task MigrateAsync(string connectionString, string journalMode)
    {
        var options = new BaGestOptions
        {
            Database = new DatabaseOptions
            {
                Type = "Sqlite",
                ConnectionString = connectionString,
                JournalMode = journalMode,
            },
        };

        var efOptions = new DbContextOptionsBuilder<SqliteContext>().UseSqlite(connectionString).Options;

        await using var context = new SqliteContext(efOptions, new FixedOptionsSnapshot(options));
        await context.RunMigrationsAsync(CancellationToken.None);
    }

    private static async Task<string> ReadJournalModeAsync(string connectionString)
    {
        await using var connection = new SqliteConnection(connectionString);
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();
        command.CommandText = "PRAGMA journal_mode;";

        return (string)await command.ExecuteScalarAsync();
    }

    private sealed class FixedOptionsSnapshot(BaGestOptions value) : IOptionsSnapshot<BaGestOptions>
    {
        public BaGestOptions Value => value;

        public BaGestOptions Get(string name) => value;
    }
}
