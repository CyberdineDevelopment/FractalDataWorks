# Local Package Build and Pack Script
# Builds and packs FractalDataWorks packages to local NuGet folder

param(
    [string]$Configuration = "Release"
)

$ErrorActionPreference = "Stop"

# Get the solution directory
$SolutionDir = Split-Path -Parent $PSScriptRoot

# Define the local packages directory
$LocalPackagesPath = Join-Path $SolutionDir "localpackages"

# Ensure the local packages directory exists
if (-not (Test-Path $LocalPackagesPath)) {
    New-Item -Path $LocalPackagesPath -ItemType Directory -Force | Out-Null
}

Write-Host "FractalDataWorks Local Package Builder" -ForegroundColor Cyan
Write-Host "Configuration: $Configuration" -ForegroundColor Yellow
Write-Host "Output Path: $LocalPackagesPath" -ForegroundColor Yellow

# Solution file to build
$SolutionFile = Join-Path $SolutionDir "FractalDataWorks.DeveloperKit.sln"

$PackedCount = 0

Write-Host ""
Write-Host "Building solution..." -ForegroundColor Green

try {
    # Build the entire solution first
    & dotnet build $SolutionFile -c $Configuration

    if ($LASTEXITCODE -ne 0) {
        throw "Solution build failed with exit code $LASTEXITCODE"
    }

    Write-Host "Solution built successfully" -ForegroundColor Green
    Write-Host "Packing all packable projects..." -ForegroundColor Green

    # Pack all projects marked as IsPackable=true
    & dotnet pack $SolutionFile -c $Configuration -o $LocalPackagesPath --no-build

    if ($LASTEXITCODE -eq 0) {
        Write-Host "All packages packed successfully" -ForegroundColor Green

        # Count the packed packages
        $PackedFiles = Get-ChildItem -Path $LocalPackagesPath -Name "FractalDataWorks.*.nupkg"
        $PackedCount = $PackedFiles.Count
    } else {
        throw "Solution pack failed with exit code $LASTEXITCODE"
    }
}
catch {
    Write-Error "Failed to build/pack solution: $($_.Exception.Message)"
    exit 1
}

Write-Host ""
Write-Host "Packing completed!" -ForegroundColor Green
Write-Host "Packed $PackedCount packages to: $LocalPackagesPath" -ForegroundColor Green

# List the packed packages
Write-Host ""
Write-Host "Packed packages:" -ForegroundColor Yellow
$packages = Get-ChildItem -Path $LocalPackagesPath -Name "FractalDataWorks.*.nupkg" | Sort-Object
foreach ($package in $packages) {
    Write-Host "  $package" -ForegroundColor White
}