. ".\common.ps1"

# Rebuild solution
Set-Location $rootFolder
& dotnet build -c Release

if (-Not $?) {
	Write-Host ("Building failed.")
	exit $LASTEXITCODE
}
    
# Copy nuget package
$projectPackPath = Join-Path $rootFolder ("../src/AbpHelper/bin/Release/EasyAbp.AbpHelper.*.nupkg")
Move-Item $projectPackPath $packFolder

# Go back to the pack folder
Set-Location $packFolder