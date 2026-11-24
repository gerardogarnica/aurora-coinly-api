---
paths:
  - "**/*.csproj"
  - "*.slnx"
  - "**/Dockerfile"
  - "docker-compose*.yml"
  - "Directory.Build.props"
  - "Directory.Packages.props"
  - ".editorconfig"
  - ".github/workflows/**"
---

# Build and packaging

## No spaces in project names

All project folder/file names use the dot-separated `Aurora.Coinly.*` convention. Do not introduce a space in a project name: a space in a `ProjectReference`'s target breaks the .NET SDK's publish-time copy-local resolution for that project's *transitive* `PackageReference`s. `dotnet build` copies them fine, but `dotnet publish` silently drops them, which only surfaces as a `FileNotFoundException` at runtime in the published/container image.

The solution file `Aurora Coinly.slnx` does have a space. That is harmless (nothing references the solution file), but every command that names it must quote it: `dotnet build "Aurora Coinly.slnx"`.

## Central package management

`Directory.Packages.props` manages all NuGet versions centrally — never add a `Version` attribute to a `PackageReference` in a `.csproj`; add the `PackageVersion` to `Directory.Packages.props` under its comment group instead.

## Warnings are errors

`Directory.Build.props` sets `TreatWarningsAsErrors`, `CodeAnalysisTreatWarningsAsErrors`, `AnalysisMode=All`, `EnforceCodeStyleInBuild`, and adds SonarAnalyzer to every project except the `.dcproj`. Many `.editorconfig` rules are severity `error`. A style or analyzer warning fails the build locally and in CI, so fix it rather than suppress it. When a suppression is genuinely justified, scope it with `#pragma warning disable/restore` around the line and keep the rule id (see `Abstractions/Messaging/ICommand.cs`); never disable a rule project-wide to get a change through.

## CI and deployment

`.github/workflows/build.yml` behaves by trigger:

| Trigger | Jobs |
|---|---|
| Push to `main` / `staging` | `build-and-test` (restore → build Release → `dotnet test` on the solution) → `docker-build-push` (GHCR `:production` for `main`, `:staging` for `staging`, plus `:<sha>`) → `deploy` (Dokploy webhook) |
| Pull request to `main` / `staging` | `build-and-test` only, as the required status check |
| Push of a `vX.Y.Z` tag | `tag-release-image` only |

Pushes to feature branches don't run CI; run the `dotnet-test-runner` agent before pushing.

## Versioning and releases

- The version is `<Version>` in `Directory.Build.props` (SemVer, no prefix). Bump it in the PR that goes to `main` for the release.
- A release is a `vX.Y.Z` tag on the `main` commit that was deployed. `tag-release-image` doesn't rebuild: it checks that the tag is `vMAJOR.MINOR.PATCH` and matches `<Version>`, then re-tags the existing `:<sha>` image as `:X.Y.Z` and `:X.Y` with `docker buildx imagetools create`, so the release image is byte-for-byte the one in production.
- Tag the merge commit on `main`, not the `staging` commit it came from: their SHAs differ, and the `staging` image was built from different code if `main` and `staging` diverged. The job only fails when no `:<sha>` image exists, and `staging` pushes also publish `:<sha>`, so it can't catch this mistake.
- `docker-build-push` must keep publishing `:<sha>`; the release job depends on it.

The test step uses `dotnet test`, which works because the test projects are xUnit v2. Migrating them to xUnit v3 (Microsoft.Testing.Platform) means changing this step and the `dotnet-test-runner` agent in the same change.
