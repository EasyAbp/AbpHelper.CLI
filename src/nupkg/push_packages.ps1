. ".\common.ps1"

# Get the version
[xml]$proj = Get-Content (Join-Path $rootFolder "AbpHelper/AbpHelper.csproj")
$version = $proj.Project.PropertyGroup.AssemblyVersion

# Publish all packages
& dotnet nuget push ("DosSEdo.AbpHelper." + "$version".Trim() + ".nupkg") -s https://nuget.pkg.github.com/DosSEdo/index.json

# Go back to the pack folder
Set-Location $packFolder
