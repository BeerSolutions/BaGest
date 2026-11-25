namespace BaGest.Core;

public class RetentionOptions
{
    /// <summary>
    /// If this is set to a value, it will limit the number of versions that will be retained for a package.
    /// The limit is applied to all major version of the package, and if the limit is exceeded,
    /// the older versions will be deleted.
    /// For a limit of 5, if there are versions 1.*.* through 5.*.* and a package version 6.0.0 is pushed, versions 1.*.* will be deleted, including all minor, patch and prerelease versions from that major version.
    /// </summary>
    public uint? MaxMajorVersions { get; set; } = null;

    /// <summary>
    /// This corresponds to the maximum number of minor versions for each major version.
    /// If this is set to a value, it will limit the number of versions that will be retained for a package.
    /// The limit is applied within each major version of the package, and if the limit of minor versions within a major version is exceeded,
    /// the older versions will be deleted.
    /// For a limit of 5, if there are versions 1.0.* through 1.5.* and a package version 1.6.0 is pushed, versions 1.0.* will be deleted, including all patch and prerelease versions from that minor version.
    /// </summary>
    public uint? MaxMinorVersions { get; set; }

    /// <summary>
    /// If this is set to a value, it will limit the number of versions that will be retained for a package.
    /// The limit is applied within each minor version of the package, and if the limit of patches within a minor version is exceeded,
    /// the older versions will be deleted.
    /// For a limit of 5, if there are versions 1.0.0 through 1.0.5 and a package version 1.0.6 is pushed, version 1.0.0 will be deleted, including all prerelease versions from that patch version.
    /// </summary>
    public uint? MaxPatchVersions { get; set; }

    /// <summary>  
    /// If this is set to a value, it will limit the number of versions that will be retained for a package.  
    /// The limit is applied within each prerelease label of the package, and if the limit of prerelease builds within a label is exceeded,  
    /// the older versions will be deleted.  
    /// For a limit of 5, if there are versions 1.0.0-alpha.1 through 1.0.0-alpha.5 and a package version 1.0.0-alpha.6 is pushed, version 1.0.0-alpha.0 will be deleted.  
    /// </summary>  
    public uint? MaxPrereleaseVersions { get; set; }

    /// <summary>
    /// If this is set to <c>true</c>, every prerelease version whose major version is lower than the highest major version
    /// that has a stable release is deleted. Disabled by default.
    /// For example, once 3.0.0 exists, 2.1.0-beta.1 and 1.0.0-rc.1 are deleted, while 3.1.0-beta.1 and 4.0.0-alpha.1 are kept.
    /// Stable versions are never affected by this option, and neither is the version being pushed or mirrored.
    /// </summary>
    public bool DeletePrereleasesOfOlderMajors { get; set; }

    /// <summary>
    /// Whether at least one retention limit or rule is configured. When none is, retention never runs.
    /// </summary>
    public bool HasAnyLimit =>
        MaxMajorVersions.HasValue
        || MaxMinorVersions.HasValue
        || MaxPatchVersions.HasValue
        || MaxPrereleaseVersions.HasValue
        || DeletePrereleasesOfOlderMajors;
}
