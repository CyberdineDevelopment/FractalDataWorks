# Fix-TestProjectDuplicates.ps1
# Removes duplicate PackageReference entries from test projects
# (they're already defined in tests/Directory.Build.props)

param(
    [switch]$WhatIf
)

$ErrorActionPreference = "Stop"

Write-Host "Fixing duplicate PackageReference entries in test projects..." -ForegroundColor Cyan
Write-Host ""

if ($WhatIf) {
    Write-Host "WHAT-IF MODE - No files will be modified" -ForegroundColor Yellow
    Write-Host ""
}

# Packages that are already in tests/Directory.Build.props
$commonTestPackages = @(
    "xunit.v3",
    "xunit.runner.visualstudio",
    "Shouldly",
    "coverlet.collector",
    "coverlet.msbuild",
    "Microsoft.NET.Test.Sdk",
    "Moq"
)

# Common Using statements already in tests/Directory.Build.props
$commonUsings = @(
    "Xunit",
    "Shouldly",
    "Moq"
)

# Find all test projects
$testProjects = Get-ChildItem -Path "tests" -Filter "*.csproj" -Recurse -File |
    Where-Object { $_.FullName -notlike "*\bin\*" -and $_.FullName -notlike "*\obj\*" }

$fixedCount = 0
$alreadyCleanCount = 0

foreach ($projectFile in $testProjects) {
    $projectName = $projectFile.BaseName

    # Read the project file
    $content = Get-Content $projectFile.FullName -Raw
    [xml]$csproj = $content

    $hadDuplicates = $false

    # Check for duplicate PackageReferences
    foreach ($itemGroup in $csproj.Project.ItemGroup) {
        if ($itemGroup.PackageReference) {
            $packagesToRemove = @()

            foreach ($packageRef in $itemGroup.PackageReference) {
                $packageName = $packageRef.Include
                if ($commonTestPackages -contains $packageName) {
                    $packagesToRemove += $packageRef
                    $hadDuplicates = $true
                }
            }

            # Remove duplicate package references
            foreach ($package in $packagesToRemove) {
                $itemGroup.RemoveChild($package) | Out-Null
            }

            # Remove empty ItemGroup
            if ($itemGroup.PackageReference.Count -eq 0 -and $itemGroup.ChildNodes.Count -eq 0) {
                $csproj.Project.RemoveChild($itemGroup) | Out-Null
            }
        }

        # Check for duplicate Using statements
        if ($itemGroup.Using) {
            $usingsToRemove = @()

            foreach ($usingRef in $itemGroup.Using) {
                $usingName = $usingRef.Include
                if ($commonUsings -contains $usingName) {
                    $usingsToRemove += $usingRef
                    $hadDuplicates = $true
                }
            }

            # Remove duplicate using statements
            foreach ($using in $usingsToRemove) {
                $itemGroup.RemoveChild($using) | Out-Null
            }

            # Remove empty ItemGroup
            if ($itemGroup.Using.Count -eq 0 -and $itemGroup.ChildNodes.Count -eq 0) {
                $csproj.Project.RemoveChild($itemGroup) | Out-Null
            }
        }
    }

    if ($hadDuplicates) {
        if ($WhatIf) {
            Write-Host "  → Would fix: $projectName" -ForegroundColor Yellow
        } else {
            # Save with proper formatting
            $settings = New-Object System.Xml.XmlWriterSettings
            $settings.Indent = $true
            $settings.IndentChars = "  "
            $settings.NewLineChars = "`r`n"
            $settings.OmitXmlDeclaration = $true

            $writer = [System.Xml.XmlWriter]::Create($projectFile.FullName, $settings)
            $csproj.Save($writer)
            $writer.Close()

            Write-Host "  ✓ Fixed: $projectName" -ForegroundColor Green
            $fixedCount++
        }
    } else {
        Write-Host "  ✓ Already clean: $projectName" -ForegroundColor Gray
        $alreadyCleanCount++
    }
}

Write-Host ""
Write-Host "Summary:" -ForegroundColor Cyan
Write-Host "  Fixed:         $fixedCount" -ForegroundColor Green
Write-Host "  Already clean: $alreadyCleanCount" -ForegroundColor Gray
Write-Host "  Total:         $($testProjects.Count)" -ForegroundColor White

if ($WhatIf) {
    Write-Host ""
    Write-Host "Run without -WhatIf to apply changes" -ForegroundColor Yellow
} elseif ($fixedCount -gt 0) {
    Write-Host ""
    Write-Host "All duplicate references removed!" -ForegroundColor Green
    Write-Host "Test packages are now inherited from tests/Directory.Build.props" -ForegroundColor Gray
}
