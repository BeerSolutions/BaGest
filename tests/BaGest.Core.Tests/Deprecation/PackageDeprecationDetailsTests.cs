using Xunit;

namespace BaGest.Core.Tests.Deprecation;

public class PackageDeprecationDetailsTests
{
    [Theory]
    [InlineData(new[] { "Legacy" }, PackageDeprecationReasons.Legacy)]
    [InlineData(new[] { "criticalbugs" }, PackageDeprecationReasons.CriticalBugs)]
    [InlineData(new[] { " OTHER " }, PackageDeprecationReasons.Other)]
    [InlineData(new[] { "Legacy", "CriticalBugs", "Other" }, PackageDeprecationReasons.Legacy | PackageDeprecationReasons.CriticalBugs | PackageDeprecationReasons.Other)]
    [InlineData(new[] { "Legacy", "legacy" }, PackageDeprecationReasons.Legacy)]
    public void TryCreate_KnownReasons_CombinesThem(string[] reasons, PackageDeprecationReasons expected)
    {
        // Act
        var success = PackageDeprecationDetails.TryCreate(reasons, null, null, null, out var details, out var error);

        // Assert
        Assert.True(success);
        Assert.Null(error);
        Assert.Equal(expected, details.Reasons);
    }

    public static TheoryData<string[]> InvalidReasons => new()
    {
        null,
        new string[0],
        new[] { "Deprecated" },
        new[] { "Legacy", "" },
        new[] { "None" },
    };

    [Theory]
    [MemberData(nameof(InvalidReasons))]
    public void TryCreate_MissingOrUnknownReasons_Fails(string[] reasons)
    {
        // Act
        var success = PackageDeprecationDetails.TryCreate(reasons, null, null, null, out var details, out var error);

        // Assert
        Assert.False(success);
        Assert.Null(details);
        Assert.NotNull(error);
    }

    [Theory]
    [InlineData(null, null)]
    [InlineData("   ", null)]
    [InlineData(" Use the new package. ", "Use the new package.")]
    public void TryCreate_Message_IsTrimmedOrNull(string message, string expected)
    {
        // Act
        PackageDeprecationDetails.TryCreate(["Legacy"], message, null, null, out var details, out _);

        // Assert
        Assert.Equal(expected, details.Message);
    }

    [Fact]
    public void TryCreate_MessageTooLong_Fails()
    {
        // Arrange
        var message = new string('a', PackageDeprecationDetails.MaxMessageLength + 1);

        // Act
        var success = PackageDeprecationDetails.TryCreate(["Legacy"], message, null, null, out _, out var error);

        // Assert
        Assert.False(success);
        Assert.Contains("message", error);
    }

    [Theory]
    [InlineData(null, null)]
    [InlineData("", null)]
    [InlineData("*", null)]
    [InlineData("(, )", null)]
    [InlineData("3.0.0", "[3.0.0, )")]
    [InlineData("[1.0.0, 2.0.0)", "[1.0.0, 2.0.0)")]
    public void TryCreate_AlternatePackageVersionRange_IsNormalized(string range, string expected)
    {
        // Act
        var success = PackageDeprecationDetails.TryCreate(["Legacy"], null, "New.Package", range, out var details, out _);

        // Assert
        Assert.True(success);
        Assert.Equal("New.Package", details.AlternatePackageId);
        Assert.Equal(expected, details.AlternatePackageVersionRange);
    }

    [Theory]
    [InlineData("not a range")]
    [InlineData("1.0.*")]
    [InlineData("[2.0.0, 1.0.0]")]
    [InlineData("(1.0.0, 1.0.0)")]
    [InlineData("[1.0.0, 1.0.0)")]
    public void TryCreate_InvalidAlternatePackageVersionRange_Fails(string range)
    {
        // Act
        var success = PackageDeprecationDetails.TryCreate(["Legacy"], null, "New.Package", range, out _, out var error);

        // Assert
        Assert.False(success);
        Assert.Contains("version range", error);
    }

    [Theory]
    [InlineData("Not a package id")]
    [InlineData("../Package")]
    public void TryCreate_InvalidAlternatePackageId_Fails(string alternatePackageId)
    {
        // Act
        var success = PackageDeprecationDetails.TryCreate(["Legacy"], null, alternatePackageId, null, out _, out var error);

        // Assert
        Assert.False(success);
        Assert.Contains("package id", error);
    }

    [Fact]
    public void TryCreate_AlternatePackageIdTooLong_Fails()
    {
        // Arrange
        var alternatePackageId = new string('a', PackageDeprecationDetails.MaxAlternatePackageIdLength + 1);

        // Act
        var success = PackageDeprecationDetails.TryCreate(["Legacy"], null, alternatePackageId, null, out _, out var error);

        // Assert
        Assert.False(success);
        Assert.Contains("package id", error);
    }

    [Fact]
    public void TryCreate_ExactAlternatePackageVersion_IsAllowed()
    {
        // Act
        var success = PackageDeprecationDetails.TryCreate(["Legacy"], null, "New.Package", "[1.0.0]", out var details, out _);

        // Assert
        Assert.True(success);
        Assert.Equal("[1.0.0, 1.0.0]", details.AlternatePackageVersionRange);
    }

    [Fact]
    public void TryCreate_AlternatePackageVersionRangeTooLong_Fails()
    {
        // Arrange
        var range = $"[1.0.0-{new string('a', PackageDeprecationDetails.MaxAlternatePackageVersionRangeLength)}, )";

        // Act
        var success = PackageDeprecationDetails.TryCreate(["Legacy"], null, "New.Package", range, out _, out var error);

        // Assert
        Assert.False(success);
        Assert.Contains("version range", error);
    }

    [Fact]
    public void TryCreate_VersionRangeWithoutAlternatePackageId_Fails()
    {
        // Act
        var success = PackageDeprecationDetails.TryCreate(["Legacy"], null, null, "[1.0.0, )", out _, out var error);

        // Assert
        Assert.False(success);
        Assert.Contains("alternate package id", error);
    }
}
