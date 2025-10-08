#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Packs all FractalDataWorks framework projects to local NuGet feed for template development.

.DESCRIPTION
    This script builds and packs all required FractalDataWorks framework projects into the
    templates/local-packages directory for use with dotnet templates.

.PARAMETER Configuration
    Build configuration (Debug, Release, etc.). Default: Release

.PARAMETER OutputPath
    Output path for NuGet packages. Default: templates/local-packages

.EXAMPLE
    .\Pack-FractalDataWorksPackages.ps1
    Packs all projects in Release configuration

.EXAMPLE
    .\Pack-FractalDataWorksPackages.ps1 -Configuration Debug
    Packs all projects in Debug configuration
#>

param(
    [Parameter()]
    [ValidateSet('Debug', 'Release', 'Alpha', 'Beta', 'Preview')]
    [string]$Configuration = 'Release',

    [Parameter()]
    [string]$OutputPath = './local-packages'
)

$ErrorActionPreference = 'Stop'

Write-Host "======================================" -ForegroundColor Cyan
Write-Host "Packing FractalDataWorks Framework Packages" -ForegroundColor Cyan
Write-Host "======================================" -ForegroundColor Cyan
Write-Host ""

# Ensure output directory exists
$fullOutputPath = Join-Path $PSScriptRoot $OutputPath
if (-not (Test-Path $fullOutputPath)) {
    New-Item -ItemType Directory -Path $fullOutputPath -Force | Out-Null
    Write-Host "Created output directory: $fullOutputPath" -ForegroundColor Green
}

# Navigate to solution root
$solutionRoot = Split-Path $PSScriptRoot -Parent
Set-Location $solutionRoot

Write-Host "Solution Root: $solutionRoot" -ForegroundColor Gray
Write-Host "Configuration: $Configuration" -ForegroundColor Gray
Write-Host "Output Path: $fullOutputPath" -ForegroundColor Gray
Write-Host ""

# Define projects to pack in dependency order
$projects = @(
    # Core abstractions (no dependencies)
    'src/FractalDataWorks.Abstractions/FractalDataWorks.Abstractions.csproj'
    'src/FractalDataWorks.Results.Abstractions/FractalDataWorks.Results.Abstractions.csproj'
    'src/FractalDataWorks.Configuration.Abstractions/FractalDataWorks.Configuration.Abstractions.csproj'

    # Core implementations
    'src/FractalDataWorks.Results/FractalDataWorks.Results.csproj'
    'src/FractalDataWorks.Configuration/FractalDataWorks.Configuration.csproj'
    'src/FractalDataWorks.Messages/FractalDataWorks.Messages.csproj'
    'src/FractalDataWorks.Collections/FractalDataWorks.Collections.csproj'
    'src/FractalDataWorks.EnhancedEnums/FractalDataWorks.EnhancedEnums.csproj'
    'src/FractalDataWorks.ServiceTypes/FractalDataWorks.ServiceTypes.csproj'

    # Source Generators (must be packed separately)
    'src/FractalDataWorks.Collections.SourceGenerators/FractalDataWorks.Collections.SourceGenerators.csproj'
    'src/FractalDataWorks.ServiceTypes.SourceGenerators/FractalDataWorks.ServiceTypes.SourceGenerators.csproj'
    'src/FractalDataWorks.Messages.SourceGenerators/FractalDataWorks.Messages.SourceGenerators.csproj'
    'src/FractalDataWorks.EnhancedEnums.SourceGenerators/FractalDataWorks.EnhancedEnums.SourceGenerators.csproj'

    # Services framework
    'src/FractalDataWorks.Services.Abstractions/FractalDataWorks.Services.Abstractions.csproj'
    'src/FractalDataWorks.Services/FractalDataWorks.Services.csproj'

    # Connection services
    'src/FractalDataWorks.Services.Connections.Abstractions/FractalDataWorks.Services.Connections.Abstractions.csproj'
    'src/FractalDataWorks.Services.Connections/FractalDataWorks.Services.Connections.csproj'

    # Data services (if needed)
    'src/FractalDataWorks.DataStores.Abstractions/FractalDataWorks.DataStores.Abstractions.csproj'
    'src/FractalDataWorks.Data/FractalDataWorks.Data.csproj'
)

$successCount = 0
$failCount = 0
$skippedCount = 0

foreach ($project in $projects) {
    $projectPath = Join-Path $solutionRoot $project
    $projectName = [System.IO.Path]::GetFileNameWithoutExtension($project)

    if (-not (Test-Path $projectPath)) {
        Write-Host "⚠️  SKIPPED: $projectName (not found)" -ForegroundColor Yellow
        $skippedCount++
        continue
    }

    Write-Host "📦 Packing: $projectName..." -ForegroundColor Cyan

    try {
        # Pack the project
        dotnet pack $projectPath `
            --configuration $Configuration `
            --output $fullOutputPath `
            --no-build `
            /p:IncludeSymbols=true `
            /p:SymbolPackageFormat=snupkg `
            2>&1 | Out-Null

        if ($LASTEXITCODE -eq 0) {
            Write-Host "✅ SUCCESS: $projectName" -ForegroundColor Green
            $successCount++
        } else {
            Write-Host "❌ FAILED: $projectName (exit code $LASTEXITCODE)" -ForegroundColor Red
            $failCount++
        }
    }
    catch {
        Write-Host "❌ ERROR: $projectName - $_" -ForegroundColor Red
        $failCount++
    }
}

Write-Host ""
Write-Host "======================================" -ForegroundColor Cyan
Write-Host "Summary" -ForegroundColor Cyan
Write-Host "======================================" -ForegroundColor Cyan
Write-Host "✅ Success: $successCount" -ForegroundColor Green
Write-Host "❌ Failed:  $failCount" -ForegroundColor Red
Write-Host "⚠️  Skipped: $skippedCount" -ForegroundColor Yellow
Write-Host ""

# List generated packages
Write-Host "Generated Packages:" -ForegroundColor Cyan
Get-ChildItem $fullOutputPath -Filter "*.nupkg" |
    Where-Object { $_.Name -notlike "*.symbols.nupkg" } |
    ForEach-Object {
        $size = "{0:N2} MB" -f ($_.Length / 1MB)
        Write-Host "  📦 $($_.Name) ($size)" -ForegroundColor Gray
    }

Write-Host ""
Write-Host "Packages location: $fullOutputPath" -ForegroundColor Cyan
Write-Host ""

if ($failCount -gt 0) {
    Write-Host "⚠️  Some packages failed to build. Check errors above." -ForegroundColor Yellow
    exit 1
}

Write-Host "✅ All packages built successfully!" -ForegroundColor Green
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Cyan
Write-Host "  1. Templates will use these packages automatically via NuGet.Config" -ForegroundColor Gray
Write-Host "  2. Test templates: cd temp-test && dotnet new fractaldataworks-domain -n TestDomain" -ForegroundColor Gray
Write-Host "  3. Push to Azure Artifacts (optional): dotnet nuget push *.nupkg --source YourFeed" -ForegroundColor Gray
