using System;
using System.Threading;
using System.Threading.Tasks;
using Azure.Data.Tables;
using BaGest.Core;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NuGet.Versioning;
using Xunit;

namespace BaGest.Azure.Tests;

/// <summary>
/// Runs against a real Table service (for example Azurite). Set the
/// <c>BAGEST_AZURE_TABLE_CONNECTION_STRING</c> environment variable to enable.
/// </summary>
public sealed class RequiresTableServiceFactAttribute : FactAttribute
{
    public const string EnvironmentVariable = "BAGEST_AZURE_TABLE_CONNECTION_STRING";

    public RequiresTableServiceFactAttribute()
    {
        if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable(EnvironmentVariable)))
        {
            Skip = $"Set {EnvironmentVariable} (e.g. to an Azurite connection string) to run.";
        }
    }
}

public class TablePackageDatabaseTests
{
    private static readonly NuGetVersion Version = new("1.0.0-Beta");

    [RequiresTableServiceFact]
    public async Task QueriesAreTranslatedByTheTableServiceAndMatchCaseInsensitively()
    {
        var connectionString = Environment.GetEnvironmentVariable(RequiresTableServiceFactAttribute.EnvironmentVariable);
        var tableName = "T" + Guid.NewGuid().ToString("N");
        var options = new AzureTableOptions { ConnectionString = connectionString, TableName = tableName };
        var serviceClient = new TableServiceClient(connectionString);
        await serviceClient.CreateTableIfNotExistsAsync(tableName);

        try
        {
            var database = new TablePackageDatabase(
                serviceClient,
                NullLogger<TablePackageDatabase>.Instance,
                Options.Create(options));

            var added = await database.AddAsync(
                new Package
                {
                    Id = "Some.Package",
                    Version = Version,
                    Listed = true,
                    Published = DateTime.UtcNow,
                    Authors = [],
                    Tags = [],
                    Dependencies = [],
                    PackageTypes = [],
                    TargetFrameworks = [],
                },
                CancellationToken.None);
            Assert.Equal(PackageAddResult.Success, added);

            Assert.True(await database.ExistsAsync("SOME.package", CancellationToken.None));
            Assert.False(await database.ExistsAsync("Other.Package", CancellationToken.None));

            Assert.True(await database.ExistsAsync("SOME.package", new NuGetVersion("1.0.0-BETA"), CancellationToken.None));
            Assert.False(await database.ExistsAsync("SOME.package", new NuGetVersion("2.0.0"), CancellationToken.None));

            Assert.Single(await database.FindAsync("SOME.package", includeUnlisted: true, CancellationToken.None));
            Assert.Single(await database.FindAsync("SOME.package", includeUnlisted: false, CancellationToken.None));
            Assert.Empty(await database.FindAsync("Other.Package", includeUnlisted: true, CancellationToken.None));
        }
        finally
        {
            await serviceClient.DeleteTableAsync(tableName);
        }
    }
}
