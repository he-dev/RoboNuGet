# RoboNuGet v2.0.0

RoboNuGet is a tool that automates multiple NuGet package cretion.

## How does it work?

You place RoboNuGet in a solution subfolder. When started it searches for the solution file and all `*.nuspec` files in solution subfolders (one level). Now you are ready to go. There are several commands available:

- `build` - to build the solution
- `pack` - packs each package. When run it resolves all dependencies for each package from the respective `*.csproj` and `packages.config` files and updates the `*.nuspec`.
- `push` - pushes each package
- `version` - allows setting the version for all packages
- `list` - lists all packages and their dependencies
- `patch` - increases the last part of the version number

## Configuration

You can adjust a few settings by editing the `RoboNuGet.json` file. The most important are:

- `PackageDirectoryName` - this is the place where you'll find your packages.
- `NuGetConfigName` - this is the main `NuGet.config`.
- `SolutionFileName` - this allows you to override the automatic solution file.
- `PackageVersion` - this is the current package version. You set with the `version` command.
- `IsPrerelease` - this indicates that the package is prerelease.
- `MsBuild` - this allows you to specify some of the `msbuild` options.
- `NuGet` - this allows you to adjust NuGet commands.
