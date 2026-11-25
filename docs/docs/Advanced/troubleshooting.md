# Troubleshooting

## Pushing a package fails with `409 Conflict`

A push can be rejected with `409 Conflict` for two different reasons:

- **The package already exists.** The version is in the database and package overwrites are not allowed. See [Enable package overwrites](../configuration.md#enable-package-overwrites).
- **The storage and the database are out of sync.** The response body explains that different files already exist in the storage at the location of the package, although the package is not in the database. BaGest logs a warning with the path of the conflicting file.

The second case typically happens after the database was replaced or recreated while the package storage was kept, for example when moving from SQLite to PostgreSQL and pointing the new database at the old storage directory. BaGest does not overwrite such files on its own, because they may belong to a package that another request is registering at the same moment.

To fix it, either:

- restore the matching database, or
- delete the orphaned files of that package version from the storage (the folder `packages/<lowercased id>/<normalized version>/`) and push the package again.

## Search shows no results in Visual Studio or the NuGet CLI

Packages whose version uses [SemVer 2.0.0](https://semver.org/) features, such as build metadata (`1.0.0+build.5`) or dot-separated prerelease labels (`1.0.0-beta.1`), are only returned by the search and autocomplete endpoints when the client sends `semVerLevel=2.0.0`. Without that parameter only SemVer 1.0.0 compatible versions are returned. This follows the [NuGet Search API](https://learn.microsoft.com/en-us/nuget/api/search-query-service-resource#search-for-packages) and matches nuget.org.

If the search of your client stays empty although the feed contains packages:

- Make sure the client is recent enough to send `semVerLevel=2.0.0`. Older clients do not, and therefore never see SemVer 2.0.0 packages.
- Include prerelease packages in the client (the *Include prerelease* checkbox in Visual Studio, `-Prerelease` in the CLI) if the packages only have prerelease versions.
- Querying the feed directly shows what a client receives, for example `https://<your server>/v3/search?q=<name>&prerelease=true&semVerLevel=2.0.0`.
