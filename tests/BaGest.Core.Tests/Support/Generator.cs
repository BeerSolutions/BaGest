using System;
using System.Collections.Generic;
using NuGet.Versioning;

namespace BaGest.Core.Tests.Support;

internal class Generator
{
    /// <summary>
    /// Create a valid <see cref="PackageDeprecationDetails"/>, failing if the input is invalid.
    /// </summary>
    internal static PackageDeprecationDetails GetDeprecation(
        string[] reasons,
        string message = null,
        string alternatePackageId = null,
        string alternatePackageVersionRange = null)
    {
        if (!PackageDeprecationDetails.TryCreate(reasons, message, alternatePackageId, alternatePackageVersionRange, out var details, out var error))
        {
            throw new ArgumentException($"Invalid test deprecation: {error}");
        }

        return details;
    }

    /// <summary>
    /// Create a fake <see cref="Package"></see> with the minimum metadata needed by the <see cref="RegistrationBuilder"></see>.
    /// </summary>
    internal static Package GetPackage(string packageId, string version, int downloads = 1)
    {
        return new Package
        {
            Id = packageId,
            Authors = new string[] { "test" },
            PackageTypes = new List<PackageType> { new PackageType { Name = "test" } },
            Dependencies = new List<PackageDependency> { },
            Version = new NuGetVersion(version),
            //Use current date for each packages publish date, because later a date offset will be
            //calculated and leads to an overflow error of the offset because the default is year 0001.
            Published = DateTime.UtcNow,
            Downloads = downloads
        };
    }
}
