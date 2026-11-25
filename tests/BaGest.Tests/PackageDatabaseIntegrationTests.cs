using System;
using System.Threading.Tasks;
using BaGest.Core;
using Microsoft.Extensions.DependencyInjection;
using NuGet.Packaging;
using NuGet.Versioning;
using Xunit;
using Xunit.Abstractions;

namespace BaGest.Tests;

public class PackageDatabaseIntegrationTests : IDisposable
{
    private readonly BaGestApplication _app;

    public PackageDatabaseIntegrationTests(ITestOutputHelper output)
    {
        _app = new BaGestApplication(output);
    }

    [Fact]
    public async Task AddingAnExistingPackageLeavesTheDatabaseUsable()
    {
        await _app.AddPackageAsync(TestResources.GetResourceStream(TestResources.Package));

        using var scope = _app.Services.CreateScope();
        var database = scope.ServiceProvider.GetRequiredService<IPackageDatabase>();
        var version = new NuGetVersion("1.2.3");

        var duplicate = ReadPackage();
        var result = await database.AddAsync(duplicate, default);

        // A download is the next write on the same database context when a mirrored package
        // turns out to exist already. It must not retry the rejected insert.
        await database.AddDownloadAsync("TestData", version, default);
        var package = await database.FindOrNullAsync("TestData", version, includeUnlisted: true, default);

        Assert.Equal(PackageAddResult.PackageAlreadyExists, result);
        Assert.Equal(1, package.Downloads);

        // The rejected package is left as the caller built it.
        Assert.Single(duplicate.Dependencies);
        Assert.Single(duplicate.PackageTypes);
        Assert.Single(duplicate.TargetFrameworks);
    }

    public void Dispose()
    {
        _app.Dispose();
    }

    private static Package ReadPackage()
    {
        using var stream = TestResources.GetResourceStream(TestResources.Package);
        using var reader = new PackageArchiveReader(stream);

        var package = reader.GetPackageMetadata();
        package.Published = DateTime.UtcNow;

        return package;
    }
}
