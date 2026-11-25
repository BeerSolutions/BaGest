using System;

namespace BaGest.Core;

/// <summary>
/// The reasons why a package version was deprecated.
/// </summary>
/// <remarks>
/// Values match NuGetGallery's <c>PackageDeprecationStatus</c>.
/// See: https://github.com/NuGet/NuGetGallery/blob/main/src/NuGet.Services.Entities/PackageDeprecationStatus.cs
/// </remarks>
[Flags]
public enum PackageDeprecationReasons
{
    /// <summary>
    /// The package is not deprecated.
    /// </summary>
    None = 0,

    /// <summary>
    /// The package is deprecated due to a reason not covered by the other values.
    /// </summary>
    Other = 1,

    /// <summary>
    /// The package is no longer maintained.
    /// </summary>
    Legacy = 2,

    /// <summary>
    /// The package has bugs which make it unsuitable for usage.
    /// </summary>
    CriticalBugs = 4,
}
