using System;
using System.Threading;
using System.Threading.Tasks;
using NuGet.Versioning;
using Xunit;

namespace BaGest.Core.Tests.Upstream;

public class PackageMirrorLockTests
{
    private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(10);

    private readonly PackageMirrorLock _target = new();

    [Fact]
    public async Task SamePackageWaitsUntilReleased()
    {
        var first = await _target.AcquireAsync("Package", new NuGetVersion("1.0.0"), CancellationToken.None);

        // The ID is case insensitive and the version is normalized, like package lookups.
        var second = _target.AcquireAsync("PACKAGE", new NuGetVersion("1.0.0.0"), CancellationToken.None);
        await Task.Delay(50);
        Assert.False(second.IsCompleted);

        first.Dispose();

        using var acquired = await second.WaitAsync(Timeout);
    }

    [Fact]
    public async Task DifferentPackagesDoNotWaitForEachOther()
    {
        using var first = await _target.AcquireAsync("Package", new NuGetVersion("1.0.0"), CancellationToken.None);

        using var otherVersion = await _target
            .AcquireAsync("Package", new NuGetVersion("2.0.0"), CancellationToken.None)
            .WaitAsync(Timeout);
        using var otherPackage = await _target
            .AcquireAsync("Other", new NuGetVersion("1.0.0"), CancellationToken.None)
            .WaitAsync(Timeout);
    }

    [Fact]
    public async Task CancelledWaitDoesNotKeepTheLock()
    {
        var version = new NuGetVersion("1.0.0");
        var first = await _target.AcquireAsync("Package", version, CancellationToken.None);

        using var cancellation = new CancellationTokenSource();
        var cancelled = _target.AcquireAsync("Package", version, cancellation.Token);
        await cancellation.CancelAsync();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => cancelled);

        first.Dispose();

        using var acquired = await _target
            .AcquireAsync("Package", version, CancellationToken.None)
            .WaitAsync(Timeout);
    }

    [Fact]
    public async Task DisposingTwiceReleasesOnce()
    {
        var version = new NuGetVersion("1.0.0");
        var first = await _target.AcquireAsync("Package", version, CancellationToken.None);
        first.Dispose();

        using var second = await _target.AcquireAsync("Package", version, CancellationToken.None);
        first.Dispose();

        var third = _target.AcquireAsync("Package", version, CancellationToken.None);
        await Task.Delay(50);
        Assert.False(third.IsCompleted);
    }
}
