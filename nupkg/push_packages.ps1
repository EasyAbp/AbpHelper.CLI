. ".\common.ps1"

# Get the version
[xml]$props = Get-Content (Join-Path $rootFolder "common.props")
$version = $props.Project.PropertyGroup.AssemblyVersion

# Publish all packages
& dotnet nuget push ("EasyAbp.AbpHelper." + "$version".Trim() + ".nupkg") -s https://api.nuget.org/v3/index.json

# Go back to the pack folder
Set-Location $packFolder
