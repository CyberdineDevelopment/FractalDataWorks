# PowerShell script to rebuild the TypeCollection sample

Write-Host "TypeCollection Sample - Complete Rebuild" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green

# Get the root directory (three levels up from this script)
$rootDir = Join-Path $PSScriptRoot "..\..\..\"
$rootLocalPackages = Join-Path $rootDir "localpackages"

# First, run the root pack-local.ps1 to build framework packages
Write-Host ""
Write-Host "Step 1: Building framework packages..." -ForegroundColor Cyan
$packLocalScript = Join-Path $rootDir "pack-local.ps1"

if (Test-Path $packLocalScript) {
    Write-Host "Running root pack-local.ps1 script..." -ForegroundColor Yellow
    & $packLocalScript -SkipPrompt
    if ($LASTEXITCODE -ne 0) {
        Write-Host "Framework package build failed!" -ForegroundColor Red
        exit 1
    }
} else {
    Write-Host "Warning: pack-local.ps1 not found at $packLocalScript" -ForegroundColor Yellow
    Write-Host "Please ensure framework packages are built first." -ForegroundColor Yellow
}

Write-Host ""
Write-Host "Step 2: Clearing local bin/obj directories..." -ForegroundColor Cyan

# Clear all bin/obj folders in this sample
Get-ChildItem -Path $PSScriptRoot -Name "bin" -Directory -Recurse | Where-Object { $_ -ne $null } | ForEach-Object { 
    $fullPath = Join-Path $PSScriptRoot $_
    Remove-Item -Path $fullPath -Recurse -Force -ErrorAction SilentlyContinue 
}
Get-ChildItem -Path $PSScriptRoot -Name "obj" -Directory -Recurse | Where-Object { $_ -ne $null } | ForEach-Object { 
    $fullPath = Join-Path $PSScriptRoot $_
    Remove-Item -Path $fullPath -Recurse -Force -ErrorAction SilentlyContinue 
}

Write-Host ""
Write-Host "Step 3: Building and packing DataStore packages..." -ForegroundColor Cyan

# Build and pack DataStores.Abstractions
Write-Host "  Building DataStores.Abstractions..." -ForegroundColor DarkCyan
dotnet pack "DataStores.Abstractions\DataStores.Abstractions.csproj" --configuration Debug -p:PackageVersion=1.1.0 -o $rootLocalPackages
if ($LASTEXITCODE -ne 0) { 
    Write-Host "DataStores.Abstractions pack failed!" -ForegroundColor Red
    exit 1
}

# Build and pack DataStore type packages
@("DataStoreTypes.Database", "DataStoreTypes.File", "DataStoreTypes.Web") | ForEach-Object {
    Write-Host "  Building $_..." -ForegroundColor DarkCyan
    dotnet pack "$_\$_.csproj" --configuration Debug -p:PackageVersion=1.1.0 -o $rootLocalPackages
    if ($LASTEXITCODE -ne 0) { 
        Write-Host "$_ pack failed!" -ForegroundColor Red
        exit 1
    }
}

Write-Host ""
Write-Host "Step 4: Building TestApp..." -ForegroundColor Cyan

# Clear NuGet cache for our packages to ensure latest versions are used
Write-Host "  Clearing NuGet cache for DataStore packages..." -ForegroundColor Yellow
dotnet nuget locals all --clear | Out-Null

# Restore and build TestApp
Write-Host "  Restoring TestApp dependencies..." -ForegroundColor Yellow
dotnet restore "TestApp\TestApp.csproj"
if ($LASTEXITCODE -ne 0) {
    Write-Host "TestApp restore failed!" -ForegroundColor Red
    exit 1
}

Write-Host "  Building TestApp..." -ForegroundColor Yellow
dotnet build "TestApp\TestApp.csproj" --no-restore
if ($LASTEXITCODE -ne 0) { 
    Write-Host "TestApp build failed!" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Green
Write-Host "Rebuild complete!" -ForegroundColor Green
Write-Host ""
Write-Host "Packages created in: $rootLocalPackages" -ForegroundColor Cyan

# List created DataStore packages
$dataStorePackages = Get-ChildItem -Path $rootLocalPackages -Filter "DataStore*.nupkg" -ErrorAction SilentlyContinue
if ($dataStorePackages.Count -gt 0) {
    Write-Host ""
    Write-Host "DataStore packages:" -ForegroundColor Yellow
    foreach ($package in $dataStorePackages) {
        Write-Host "  - $($package.Name)" -ForegroundColor Gray
    }
}

Write-Host ""
Write-Host "To run the TestApp:" -ForegroundColor Yellow
Write-Host "  dotnet run --project TestApp\TestApp.csproj" -ForegroundColor Cyan
Write-Host ""