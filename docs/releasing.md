# Releasing Sandlada.RopSharp

## Versioning

The package follows [Semantic Versioning](https://semver.org/):

- **MAJOR** — breaking changes to the public API. The public surface is locked by
  `Sandlada.RopSharp/PublicAPI.Shipped.txt`; a removal or signature change is a breaking change.
- **MINOR** — additive API. Add new entries to `PublicAPI.Unshipped.txt`, ship the release, then move them to
  `PublicAPI.Shipped.txt` (the analyzer reports `RS0017` when a declared symbol disappears).
- **PATCH** — behaviour fixes with no API change.

`CHANGELOG.md` is updated in the same commit as the version bump.

The development default version lives in `Sandlada.RopSharp/Sandlada.RopSharp.csproj` (`<Version>`); a `vX.Y.Z`
tag or the manual `version` input overrides it at pack time (`-p:Version=...`), so the csproj value is the local
default and the fallback for `dotnet pack -c Release`.

## Pre-flight checklist

```bash
dotnet restore --locked-mode
dotnet format RopSharp.sln --verify-no-changes --no-restore
dotnet build -c Release --no-restore        # 0 warnings, 0 errors
dotnet test -c Release --no-build           # all tests pass on net10.0 and net11.0
dotnet pack -c Release --no-build
```

Verify the package before publishing:

- `lib/net10.0` and `lib/net11.0` are present.
- `Sandlada.RopSharp.xml` (XML documentation) is present for both targets.
- `README.md`, `LICENSE` and the release notes are embedded in the nuspec.
- Version, package id `Sandlada.RopSharp`, and MIT license match the release.

## Publishing

Publishing is automated by `.github/workflows/release.yml`:

- Push a tag `v<major>.<minor>.<patch>` to publish that version, or
- run the workflow manually with an explicit `version` input.

The workflow requires a repository secret named `NUGET_API_KEY` with push permission for the
`Sandlada` publisher on nuget.org. It runs the format check, build, tests, pack, and pushes both
`.nupkg` and `.snupkg` (`--skip-duplicate` makes retries idempotent).

## Manual publish (fallback)

```bash
dotnet pack -c Release -p:Version=0.0.1
dotnet nuget push Sandlada.RopSharp/bin/Release/Sandlada.RopSharp.0.0.1.nupkg \
  --api-key "$NUGET_API_KEY" --source https://api.nuget.org/v3/index.json
```

## Verified mechanics (local dry run)

The two mechanics `release.yml` relies on can be exercised without a NuGet key:

```bash
# Version override (the tag path strips the leading "v" before passing this).
dotnet pack Sandlada.RopSharp/Sandlada.RopSharp.csproj -c Release \
  -p:Version=0.0.1 -o artifacts/release-test
# Verified: package file names and nuspec <version> become 0.0.1, and README/LICENSE are still packed.

# Push to a folder source. The path must be absolute; relative paths are rejected as an invalid source.
dotnet nuget push artifacts/release-test/Sandlada.RopSharp.0.0.1.nupkg \
  --source "E:\path\to\local-feed"
dotnet nuget push artifacts/release-test/Sandlada.RopSharp.0.0.1.nupkg \
  --source "E:\path\to\local-feed" --skip-duplicate
# Verified: the command shape used by release.yml works. Folder sources warn that --skip-duplicate
# is not supported (they overwrite); nuget.org honours it and rejects duplicates otherwise.
```

`.snupkg` uploads target the server source (nuget.org); folder feeds do not store symbol packages.

## After publishing

1. Create a GitHub release with the changelog section for the version.
2. Verify the package page renders `README.md` and lists both target frameworks.
3. For a minor release, move `PublicAPI.Unshipped.txt` entries into `PublicAPI.Shipped.txt`.
