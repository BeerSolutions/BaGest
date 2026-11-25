using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NuGet.Packaging;
using NuGet.Versioning;
using Xunit;

namespace BaGest.Core.Tests.Services;

public class PackageIndexingServiceTests
{
    private readonly Mock<IPackageDatabase> _packages;
    private readonly Mock<IPackageStorageService> _storage;
    private readonly Mock<ISearchIndexer> _search;
    private readonly Mock<SystemTime> _time;
    private readonly Mock<IPackageDeletionService> _deleter;
    private readonly PackageIndexingService _target;
    private readonly BaGestOptions _mockOptions;
    private RetentionOptions _retentionOptions;

    public PackageIndexingServiceTests()
    {
        _packages = new Mock<IPackageDatabase>(MockBehavior.Strict);
        _storage = new Mock<IPackageStorageService>(MockBehavior.Strict);
        _search = new Mock<ISearchIndexer>(MockBehavior.Strict);
        _time = new Mock<SystemTime>(MockBehavior.Loose);
        _deleter = new Mock<IPackageDeletionService>(MockBehavior.Strict);
        _mockOptions = new();
        _retentionOptions = new();
        var options = new Mock<IOptionsSnapshot<BaGestOptions>>(MockBehavior.Strict);
        options.Setup(o => o.Value).Returns(_mockOptions);
        var retentionOptions = new Mock<IOptionsSnapshot<RetentionOptions>>(MockBehavior.Strict);
        retentionOptions.Setup(o => o.Value).Returns(_retentionOptions);

        _target = new PackageIndexingService(
            _packages.Object,
            _storage.Object,
            _deleter.Object,
            _search.Object,
            _time.Object,
            options.Object,
            retentionOptions.Object,
            Mock.Of<ILogger<PackageIndexingService>>());
    }

    // TODO: Add malformed package tests

    [Fact]
    public async Task IndexAsync_WhenPackageAlreadyExists_AndOverwriteForbidden_ReturnsPackageAlreadyExists()
    {
        // Arrange
        _mockOptions.AllowPackageOverwrites = PackageOverwriteAllowed.False;

        var builder = new PackageBuilder
        {
            Id = "bagest-test",
            Version = NuGetVersion.Parse("1.0.0"),
            Description = "Test Description",
        };
        builder.Authors.Add("Test Author");
        var assemblyFile = GetType().Assembly.Location;
        builder.Files.Add(new PhysicalPackageFile
        {
            SourcePath = assemblyFile,
            TargetPath = "lib/Test.dll"
        });
        var stream = new MemoryStream();
        builder.Save(stream);
        _packages.Setup(p => p.ExistsAsync(builder.Id, builder.Version, default)).ReturnsAsync(true);

        // Act
        var result = await _target.IndexAsync(stream, default);

        // Assert
        Assert.Equal(PackageIndexingResult.PackageAlreadyExists, result);
    }

    [Fact]
    public async Task IndexAsync_WhenPackageAlreadyExists_AndOverwriteAllowed_IndexesPackage()
    {
        // Arrange
        _mockOptions.AllowPackageOverwrites = PackageOverwriteAllowed.True;

        var builder = new PackageBuilder
        {
            Id = "bagest-test",
            Version = NuGetVersion.Parse("1.0.0"),
            Description = "Test Description",
        };
        builder.Authors.Add("Test Author");
        var assemblyFile = GetType().Assembly.Location;
        builder.Files.Add(new PhysicalPackageFile
        {
            SourcePath = assemblyFile,
            TargetPath = "lib/Test.dll"
        });
        var stream = new MemoryStream();
        builder.Save(stream);
        _packages.Setup(p => p.ExistsAsync(builder.Id, builder.Version, default)).ReturnsAsync(true);
        _packages.Setup(p => p.HardDeletePackageAsync(builder.Id, builder.Version, default)).ReturnsAsync(true);
        _packages.Setup(p => p.AddAsync(It.Is<Package>(p1 => p1.Id == builder.Id && p1.Version.ToString() == builder.Version.ToString()), default)).ReturnsAsync(PackageAddResult.Success);

        _storage.Setup(s => s.DeleteAsync(builder.Id, builder.Version, default)).Returns(Task.CompletedTask);
        _storage.Setup(s => s.SavePackageContentAsync(It.Is<Package>(p => p.Id == builder.Id && p.Version.ToString() == builder.Version.ToString()), stream, It.IsAny<FileStream>(), default, default, default)).Returns(Task.CompletedTask);

        _search.Setup(s => s.IndexAsync(It.Is<Package>(p => p.Id == builder.Id && p.Version.ToString() == builder.Version.ToString()), default)).Returns(Task.CompletedTask);

        // Act
        var result = await _target.IndexAsync(stream, default);

        // Assert
        Assert.Equal(PackageIndexingResult.Success, result);
    }

    [Fact]
    public async Task IndexAsync_WhenPrereleasePackageAlreadyExists_AndOverwritePrereleaseAllowed_IndexesPackage()
    {
        // Arrange
        _mockOptions.AllowPackageOverwrites = PackageOverwriteAllowed.PrereleaseOnly;

        var builder = new PackageBuilder
        {
            Id = "bagest-test",
            Version = NuGetVersion.Parse("1.0.0-beta"),
            Description = "Test Description",
        };
        builder.Authors.Add("Test Author");
        var assemblyFile = GetType().Assembly.Location;
        builder.Files.Add(new PhysicalPackageFile
        {
            SourcePath = assemblyFile,
            TargetPath = "lib/Test.dll"
        });
        var stream = new MemoryStream();
        builder.Save(stream);
        _packages.Setup(p => p.ExistsAsync(builder.Id, builder.Version, default)).ReturnsAsync(true);
        _packages.Setup(p => p.HardDeletePackageAsync(builder.Id, builder.Version, default)).ReturnsAsync(true);
        _packages.Setup(p => p.AddAsync(It.Is<Package>(p1 => p1.Id == builder.Id && p1.Version.ToString() == builder.Version.ToString()), default)).ReturnsAsync(PackageAddResult.Success);

        _storage.Setup(s => s.DeleteAsync(builder.Id, builder.Version, default)).Returns(Task.CompletedTask);
        _storage.Setup(s => s.SavePackageContentAsync(It.Is<Package>(p => p.Id == builder.Id && p.Version.ToString() == builder.Version.ToString()), stream, It.IsAny<FileStream>(), default, default, default)).Returns(Task.CompletedTask);

        _search.Setup(s => s.IndexAsync(It.Is<Package>(p => p.Id == builder.Id && p.Version.ToString() == builder.Version.ToString()), default)).Returns(Task.CompletedTask);

        // Act
        var result = await _target.IndexAsync(stream, default);

        // Assert
        Assert.Equal(PackageIndexingResult.Success, result);
    }

    [Fact]
    public async Task IndexAsync_WhenPrereleasePackageAlreadyExists_AndOverwriteForbidden_ReturnsPackageAlreadyExists()
    {
        // Arrange
        _mockOptions.AllowPackageOverwrites = PackageOverwriteAllowed.False;

        var builder = new PackageBuilder
        {
            Id = "bagest-test",
            Version = NuGetVersion.Parse("1.0.0-beta"),
            Description = "Test Description",
        };
        builder.Authors.Add("Test Author");
        var assemblyFile = GetType().Assembly.Location;
        builder.Files.Add(new PhysicalPackageFile
        {
            SourcePath = assemblyFile,
            TargetPath = "lib/Test.dll"
        });
        var stream = new MemoryStream();
        builder.Save(stream);
        _packages.Setup(p => p.ExistsAsync(builder.Id, builder.Version, default)).ReturnsAsync(true);

        // Act
        var result = await _target.IndexAsync(stream, default);

        // Assert
        Assert.Equal(PackageIndexingResult.PackageAlreadyExists, result);
    }

    [Fact]
    public async Task IndexAsync_WithValidPackage_ReturnsSuccess()
    {
        // Arrange
        _mockOptions.AllowPackageOverwrites = PackageOverwriteAllowed.False;
        var builder = new PackageBuilder
        {
            Id = "bagest-test",
            Version = NuGetVersion.Parse("1.0.0"),
            Description = "Test Description",
        };
        builder.Authors.Add("Test Author");
        var assemblyFile = GetType().Assembly.Location;
        builder.Files.Add(new PhysicalPackageFile
        {
            SourcePath = assemblyFile,
            TargetPath = "lib/Test.dll"
        });
        var stream = new MemoryStream();
        builder.Save(stream);
        _packages.Setup(p => p.ExistsAsync(builder.Id, builder.Version, default)).ReturnsAsync(false);
        _packages.Setup(p => p.AddAsync(It.Is<Package>(p1 => p1.Id == builder.Id && p1.Version.ToString() == builder.Version.ToString()), default)).ReturnsAsync(PackageAddResult.Success);

        _storage.Setup(s => s.SavePackageContentAsync(It.Is<Package>(p => p.Id == builder.Id && p.Version.ToString() == builder.Version.ToString()), stream, It.IsAny<FileStream>(), default, default, default)).Returns(Task.CompletedTask);

        _search.Setup(s => s.IndexAsync(It.Is<Package>(p => p.Id == builder.Id && p.Version.ToString() == builder.Version.ToString()), default)).Returns(Task.CompletedTask);

        // Act
        var result = await _target.IndexAsync(stream, default);

        // Assert
        Assert.Equal(PackageIndexingResult.Success, result);
    }

    [Theory]
    [InlineData(null, null, null, null, false, false)]
    [InlineData(1u, null, null, null, false, true)]
    [InlineData(null, 1u, null, null, false, true)]
    [InlineData(null, null, 1u, null, false, true)]
    [InlineData(null, null, null, 1u, false, true)]
    [InlineData(null, null, null, null, true, true)]
    public async Task IndexAsync_RunsRetention_WhenAnyLimitIsConfigured(
        uint? maxMajor, uint? maxMinor, uint? maxPatch, uint? maxPrerelease, bool deletePrereleasesOfOlderMajors, bool retentionRuns)
    {
        // Arrange
        _mockOptions.AllowPackageOverwrites = PackageOverwriteAllowed.False;
        _retentionOptions.MaxMajorVersions = maxMajor;
        _retentionOptions.MaxMinorVersions = maxMinor;
        _retentionOptions.MaxPatchVersions = maxPatch;
        _retentionOptions.MaxPrereleaseVersions = maxPrerelease;
        _retentionOptions.DeletePrereleasesOfOlderMajors = deletePrereleasesOfOlderMajors;

        var builder = new PackageBuilder
        {
            Id = "bagest-test",
            Version = NuGetVersion.Parse("1.0.0"),
            Description = "Test Description",
        };
        builder.Authors.Add("Test Author");
        builder.Files.Add(new PhysicalPackageFile
        {
            SourcePath = GetType().Assembly.Location,
            TargetPath = "lib/Test.dll"
        });
        var stream = new MemoryStream();
        builder.Save(stream);
        _packages.Setup(p => p.ExistsAsync(builder.Id, builder.Version, default)).ReturnsAsync(false);
        _packages.Setup(p => p.AddAsync(It.IsAny<Package>(), default)).ReturnsAsync(PackageAddResult.Success);
        _storage.Setup(s => s.SavePackageContentAsync(It.IsAny<Package>(), stream, It.IsAny<FileStream>(), default, default, default)).Returns(Task.CompletedTask);
        _search.Setup(s => s.IndexAsync(It.IsAny<Package>(), default)).Returns(Task.CompletedTask);
        _deleter
            .Setup(d => d.DeleteOldVersionsAsync(It.IsAny<Package>(), maxMajor, maxMinor, maxPatch, maxPrerelease, deletePrereleasesOfOlderMajors, default))
            .ReturnsAsync(0);

        // Act
        var result = await _target.IndexAsync(stream, default);

        // Assert
        Assert.Equal(PackageIndexingResult.Success, result);
        _deleter.Verify(
            d => d.DeleteOldVersionsAsync(It.IsAny<Package>(), It.IsAny<uint?>(), It.IsAny<uint?>(), It.IsAny<uint?>(), It.IsAny<uint?>(), It.IsAny<bool>(), default),
            retentionRuns ? Times.Once : Times.Never);
    }

    [Fact]
    public async Task WhenDatabaseAddFailsBecausePackageAlreadyExists_ReturnsPackageAlreadyExists()
    {
        await Task.Yield();
    }

    [Fact]
    public async Task ThrowsWhenStorageSaveThrows()
    {
        await Task.Yield();
    }
}
