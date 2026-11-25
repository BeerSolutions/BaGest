import Tabs from '@theme/Tabs';
import TabItem from '@theme/TabItem';

# Package deprecation

A package version can be marked as deprecated when it should no longer be used, for example
because it is no longer maintained or has critical bugs. Deprecating a package does not remove it:
existing projects still restore it, but NuGet clients warn about it.

BaGest returns deprecations in its [registration resource](https://learn.microsoft.com/nuget/api/registration-base-url-resource#package-deprecation),
which means that:

* `dotnet list package --deprecated` lists the deprecated packages a project uses,
* Visual Studio's package manager shows a deprecation warning,
* the package page in BaGest's UI shows a deprecation banner, and marks deprecated versions in the version list.

## Deprecation details

A deprecation applies to a single package version and has:

| Field | Required | Description |
| --- | --- | --- |
| `reasons` | Yes | One or more of `Legacy` (no longer maintained), `CriticalBugs` (has bugs which make it unsuitable for usage) and `Other`. Case-insensitive. |
| `message` | No | Additional details, up to 4000 characters. |
| `alternatePackageId` | No | The id of the package that should be used instead. Must be a valid package id of up to 128 characters. The package does not have to exist on this feed. |
| `alternatePackageVersionRange` | No | The [version range](https://learn.microsoft.com/nuget/concepts/package-versioning#version-ranges) of the alternate package, for example `[2.0.0, )` or `2.0.0` (which means "2.0.0 or higher"). Omit it, or use `*`, to allow any version. Floating ranges such as `1.0.*` and ranges that no version can satisfy are rejected. Requires `alternatePackageId`. |

## Deprecate a package version

Send a `PUT` request to `/api/v2/package/{id}/{version}/deprecation` with the deprecation as a JSON body.
If your feed [requires an API key](configuration.md#require-an-api-key), pass it in the `X-NuGet-ApiKey` header.
Deprecating a version that is already deprecated replaces its previous deprecation.

<Tabs>
  <TabItem value="curl" label="curl" default>

```shell
curl -X PUT "http://localhost:5000/api/v2/package/My.Package/1.0.0/deprecation" \
  -H "X-NuGet-ApiKey: NUGET-SERVER-API-KEY" \
  -H "Content-Type: application/json" \
  -d '{
    "reasons": ["Legacy"],
    "message": "My.Package is no longer maintained, use My.Package.Next instead.",
    "alternatePackageId": "My.Package.Next",
    "alternatePackageVersionRange": "2.0.0"
  }'
```

  </TabItem>
  <TabItem value="powershell" label="PowerShell">

```powershell
$body = @{
    reasons = @("Legacy")
    message = "My.Package is no longer maintained, use My.Package.Next instead."
    alternatePackageId = "My.Package.Next"
    alternatePackageVersionRange = "2.0.0"
} | ConvertTo-Json

Invoke-RestMethod -Method Put `
    -Uri "http://localhost:5000/api/v2/package/My.Package/1.0.0/deprecation" `
    -Headers @{ "X-NuGet-ApiKey" = "NUGET-SERVER-API-KEY" } `
    -ContentType "application/json" `
    -Body $body
```

  </TabItem>
</Tabs>

The endpoint responds with:

| Status | Meaning |
| --- | --- |
| `204 No Content` | The package version is deprecated. |
| `400 Bad Request` | The body is not a valid deprecation. The response's `detail` explains why. |
| `401 Unauthorized` | The API key is missing or invalid, or BaGest runs in read-only mode. |
| `404 Not Found` | The package version does not exist. |

Unlisted package versions can be deprecated too.

To deprecate every version of a package, deprecate each version. For example, with the
[package versions resource](https://learn.microsoft.com/nuget/api/package-base-address-resource#enumerate-package-versions)
(with [read-through caching](configuration.md#enable-read-through-caching) enabled, this resource also lists
upstream versions that are not on this feed yet, and deprecating them returns `404 Not Found`):

```shell
for version in $(curl -s "http://localhost:5000/v3/package/my.package/index.json" | jq -r '.versions[]'); do
  curl -X PUT "http://localhost:5000/api/v2/package/My.Package/$version/deprecation" \
    -H "X-NuGet-ApiKey: NUGET-SERVER-API-KEY" \
    -H "Content-Type: application/json" \
    -d '{ "reasons": ["Legacy"], "alternatePackageId": "My.Package.Next" }'
done
```

## Remove a deprecation

Send a `DELETE` request to the same URL. It responds with `204 No Content`, or `404 Not Found` if the package version does not exist.

```shell
curl -X DELETE "http://localhost:5000/api/v2/package/My.Package/1.0.0/deprecation" \
  -H "X-NuGet-ApiKey: NUGET-SERVER-API-KEY"
```

## Check the deprecated packages of a project

```shell
dotnet list package --deprecated
```

```
Project `MyApp` has the following deprecated packages
   [net10.0]:
   Top-level Package      Requested   Resolved   Reason(s)   Alternative
   > My.Package           1.0.0       1.0.0      Legacy      My.Package.Next >= 2.0.0
```

NuGet caches HTTP responses for 30 minutes, so a project that was restored recently may need
`dotnet nuget locals http-cache --clear` before it shows a new deprecation.

:::note

Deprecations are stored in BaGest's database. Packages mirrored from an upstream feed with
[read-through caching](configuration.md#enable-read-through-caching) do not keep the upstream feed's
deprecations; deprecate them in BaGest if needed.

:::
