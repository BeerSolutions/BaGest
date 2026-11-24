using System;

namespace BaGest.Core;

/// <summary>
/// Thrown when a package file cannot be stored because a different file already exists at the same path.
/// This happens when the storage and the database are out of sync (for example, after switching the
/// database but keeping the storage) or when the same package is pushed concurrently.
/// </summary>
public class PackageStorageConflictException : InvalidOperationException
{
    public PackageStorageConflictException(string packageId, string normalizedVersion, string path)
        : base($"Failed to store package {packageId} {normalizedVersion} due to conflict: a file already exists at '{path}'.")
    {
        PackageId = packageId;
        NormalizedVersion = normalizedVersion;
        Path = path;
    }

    public string PackageId { get; }

    public string NormalizedVersion { get; }

    public string Path { get; }
}
