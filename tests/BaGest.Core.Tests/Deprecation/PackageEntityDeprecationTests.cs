using BaGest.Core.Tests.Support;
using Xunit;

namespace BaGest.Core.Tests.Deprecation;

public class PackageEntityDeprecationTests
{
    [Fact]
    public void Deprecate_SetsDeprecation()
    {
        // Arrange
        var package = Generator.GetPackage("Old.Package", "1.0.0");
        var details = Generator.GetDeprecation(["Legacy", "CriticalBugs"], message: "Use New.Package", alternatePackageId: "New.Package", alternatePackageVersionRange: "2.0.0");

        // Act
        package.Deprecate(details);

        // Assert
        Assert.True(package.IsDeprecated);
        Assert.Equal(PackageDeprecationReasons.Legacy | PackageDeprecationReasons.CriticalBugs, package.DeprecationReasons);
        Assert.Equal("Use New.Package", package.DeprecationMessage);
        Assert.Equal("New.Package", package.DeprecationAlternatePackageId);
        Assert.Equal("[2.0.0, )", package.DeprecationAlternatePackageVersionRange);
    }

    [Fact]
    public void Deprecate_ReplacesPreviousDeprecation()
    {
        // Arrange
        var package = Generator.GetPackage("Old.Package", "1.0.0");
        var first = Generator.GetDeprecation(["Legacy"], message: "Use New.Package", alternatePackageId: "New.Package");
        var second = Generator.GetDeprecation(["Other"]);
        package.Deprecate(first);

        // Act
        package.Deprecate(second);

        // Assert
        Assert.Equal(PackageDeprecationReasons.Other, package.DeprecationReasons);
        Assert.Null(package.DeprecationMessage);
        Assert.Null(package.DeprecationAlternatePackageId);
        Assert.Null(package.DeprecationAlternatePackageVersionRange);
    }

    [Fact]
    public void Undeprecate_ClearsDeprecation()
    {
        // Arrange
        var package = Generator.GetPackage("Old.Package", "1.0.0");
        var details = Generator.GetDeprecation(["Legacy"], message: "Use New.Package", alternatePackageId: "New.Package", alternatePackageVersionRange: "2.0.0");
        package.Deprecate(details);

        // Act
        package.Undeprecate();

        // Assert
        Assert.False(package.IsDeprecated);
        Assert.Equal(PackageDeprecationReasons.None, package.DeprecationReasons);
        Assert.Null(package.DeprecationMessage);
        Assert.Null(package.DeprecationAlternatePackageId);
        Assert.Null(package.DeprecationAlternatePackageVersionRange);
    }
}
