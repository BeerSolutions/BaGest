using Xunit;

namespace BaGest.Core.Tests.Deprecation;

public class PackageDeprecationReasonsExtensionsTests
{
    [Fact]
    public void ToProtocolValues_None_IsEmpty()
    {
        Assert.Empty(PackageDeprecationReasons.None.ToProtocolValues());
    }

    [Fact]
    public void ToProtocolValues_AllReasons_UsesProtocolNamesInStableOrder()
    {
        // Arrange
        var reasons = PackageDeprecationReasons.Other | PackageDeprecationReasons.CriticalBugs | PackageDeprecationReasons.Legacy;

        // Act
        var values = reasons.ToProtocolValues();

        // Assert
        Assert.Equal(["Legacy", "CriticalBugs", "Other"], values);
    }

    [Theory]
    [InlineData("Legacy", PackageDeprecationReasons.Legacy)]
    [InlineData("CRITICALBUGS", PackageDeprecationReasons.CriticalBugs)]
    [InlineData("other", PackageDeprecationReasons.Other)]
    public void TryParseProtocolValue_KnownValue_Succeeds(string value, PackageDeprecationReasons expected)
    {
        // Act
        var success = PackageDeprecationReasonsExtensions.TryParseProtocolValue(value, out var reason);

        // Assert
        Assert.True(success);
        Assert.Equal(expected, reason);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("None")]
    [InlineData("1")]
    [InlineData("Obsolete")]
    public void TryParseProtocolValue_UnknownValue_Fails(string value)
    {
        Assert.False(PackageDeprecationReasonsExtensions.TryParseProtocolValue(value, out _));
    }
}
