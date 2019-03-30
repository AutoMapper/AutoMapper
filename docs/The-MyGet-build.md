# The MyGet Build

AutoMapper uses MyGet to publish development builds based on the master branch. This means that the MyGet build sometimes contains fixes that are not available in the current NuGet package. Please try the latest MyGet build before reporting issues, in case your issue has already been fixed but not released.

The AutoMapper MyGet gallery is available [here](https://myget.org/feed/automapperdev/package/nuget/AutoMapper). Be sure to include prereleases.

## Installing the Package

If you want to install the latest MyGet package into a project, you can use the following command:

```
Install-Package AutoMapper -Source https://www.myget.org/F/automapperdev/api/v3/index.json -IncludePrerelease
```
