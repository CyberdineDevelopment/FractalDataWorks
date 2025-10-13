# Mark-PublicProjects.ps1
# Quickly mark multiple projects as public by adding <IsPublicProject>true</IsPublicProject>

param(
    [Parameter(Mandatory=$false)]
    [string[]]$ProjectPatterns,
    [switch]$WhatIf,
    [switch]$All
)

$ErrorActionPreference = "Stop"

# Default patterns for public projects
$defaultPatterns = @(
    # Core abstractions
    "*Abstractions",

    # CodeBuilder
    "*CodeBuilder*",

    # Collections
    "*Collections*",

    # Commands
    "*Commands*",

    # Configuration
    "*Configuration*",

    # Dependency Injection
    "*DependencyInjection*",

    # Data abstractions only
    "FractalDataWorks.Data.Abstractions",
    "FractalDataWorks.Data.DataContainers.Abstractions",
    "FractalDataWorks.Data.DataSets.Abstractions",
    "FractalDataWorks.Data.DataStores.Abstractions",

    # Enhanced Enums
    "*EnhancedEnums*",

    # Messages
    "*Messages*",

    # Results
    "*Results*",

    # Services core (not implementations)
    "FractalDataWorks.Services",
    "*Services.Abstractions*",
    "*Services.Execution*",

    # ServiceTypes
    "*ServiceTypes*",

    # Source Generators
    "*SourceGenerators*",

    # Web abstractions
    "*Web.Http.Abstractions*",
    "*Web.RestEndpoints*",

    # Test projects matching above patterns
    "*Tests"
)

if ($ProjectPatterns) {
    $patterns = $ProjectPatterns
} else {
    $patterns = $defaultPatterns
}

Write-Host "Marking projects as public..." -ForegroundColor Cyan
Write-Host ""

if ($WhatIf) {
    Write-Host "WHAT-IF MODE - No files will be modified" -ForegroundColor Yellow
    Write-Host ""
}

# Find all .csproj files
$projectFiles = Get-ChildItem -Path . -Filter "*.csproj" -Recurse -File |
    Where-Object {
        $_.FullName -notlike "*\bin\*" -and
        $_.FullName -notlike "*\obj\*"
    }

$matched = 0
$alreadyPublic = 0
$modified = 0

foreach ($projectFile in $projectFiles) {
    $projectName = $projectFile.BaseName

    # Check if matches any pattern
    $isMatch = $false
    foreach ($pattern in $patterns) {
        if ($projectName -like $pattern) {
            $isMatch = $true
            break
        }
    }

    if (-not $isMatch -and -not $All) {
        continue
    }

    $matched++

    # Read the csproj file
    $content = Get-Content $projectFile.FullName -Raw
    [xml]$csproj = $content

    # Check if already marked as public
    $existingPublic = $csproj.Project.PropertyGroup.IsPublicProject | Where-Object { $_ -eq 'true' }

    if ($existingPublic) {
        Write-Host "  ✓ Already public: $projectName" -ForegroundColor Gray
        $alreadyPublic++
        continue
    }

    if ($WhatIf) {
        Write-Host "  → Would mark as public: $projectName" -ForegroundColor Yellow
        continue
    }

    # Add <IsPublicProject>true</IsPublicProject> to first PropertyGroup
    $firstPropertyGroup = $csproj.Project.PropertyGroup[0]

    if ($firstPropertyGroup) {
        # Add comment
        $comment = $csproj.CreateComment(" Mark as public open-source project ")
        $firstPropertyGroup.AppendChild($comment) | Out-Null

        # Add IsPublicProject element
        $isPublicElement = $csproj.CreateElement("IsPublicProject")
        $isPublicElement.InnerText = "true"
        $firstPropertyGroup.AppendChild($isPublicElement) | Out-Null

        # Save with proper formatting
        $settings = New-Object System.Xml.XmlWriterSettings
        $settings.Indent = $true
        $settings.IndentChars = "  "
        $settings.NewLineChars = "`r`n"
        $settings.OmitXmlDeclaration = $true

        $writer = [System.Xml.XmlWriter]::Create($projectFile.FullName, $settings)
        $csproj.Save($writer)
        $writer.Close()

        Write-Host "  ✓ Marked as public: $projectName" -ForegroundColor Green
        $modified++
    } else {
        Write-Host "  ✗ Failed: $projectName (no PropertyGroup found)" -ForegroundColor Red
    }
}

Write-Host ""
Write-Host "Summary:" -ForegroundColor Cyan
Write-Host "  Matched projects: $matched" -ForegroundColor White
Write-Host "  Already public:   $alreadyPublic" -ForegroundColor Gray
Write-Host "  Modified:         $modified" -ForegroundColor Green

if ($WhatIf) {
    Write-Host ""
    Write-Host "Run without -WhatIf to apply changes" -ForegroundColor Yellow
}
