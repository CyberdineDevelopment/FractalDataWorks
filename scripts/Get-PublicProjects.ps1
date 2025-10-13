# Get-PublicProjects.ps1
# Discovers all projects marked as public via <IsPublicProject>true</IsPublicProject>

param(
    [string]$SolutionRoot = (Get-Location),
    [switch]$ShowPaths,
    [switch]$ShowDirectories
)

$ErrorActionPreference = "Stop"

Write-Host "Discovering public projects..." -ForegroundColor Cyan
Write-Host "Solution root: $SolutionRoot" -ForegroundColor Gray
Write-Host ""

# Find all .csproj files
$projectFiles = Get-ChildItem -Path $SolutionRoot -Filter "*.csproj" -Recurse -File |
    Where-Object { $_.FullName -notlike "*\bin\*" -and $_.FullName -notlike "*\obj\*" }

$publicProjects = @()

foreach ($projectFile in $projectFiles) {
    # Read the csproj file
    [xml]$csproj = Get-Content $projectFile.FullName

    # Check if IsPublicProject is set to true
    $isPublic = $csproj.Project.PropertyGroup.IsPublicProject | Where-Object { $_ -eq 'true' }

    if ($isPublic) {
        $relativePath = [System.IO.Path]::GetRelativePath($SolutionRoot, $projectFile.DirectoryName)
        $projectName = $projectFile.BaseName

        $publicProjects += [PSCustomObject]@{
            Name = $projectName
            RelativePath = $relativePath
            FullPath = $projectFile.DirectoryName
            ProjectFile = $projectFile.FullName
        }
    }
}

if ($publicProjects.Count -eq 0) {
    Write-Host "No public projects found." -ForegroundColor Yellow
    Write-Host ""
    Write-Host "To mark a project as public, add this to its .csproj:" -ForegroundColor Gray
    Write-Host "  <IsPublicProject>true</IsPublicProject>" -ForegroundColor Gray
    exit 0
}

Write-Host "Found $($publicProjects.Count) public projects:" -ForegroundColor Green
Write-Host ""

if ($ShowDirectories) {
    # Output just the directory paths (for scripting)
    foreach ($project in $publicProjects | Sort-Object RelativePath) {
        Write-Output $project.RelativePath
    }
} elseif ($ShowPaths) {
    # Output full paths (for scripting)
    foreach ($project in $publicProjects | Sort-Object RelativePath) {
        Write-Output $project.FullPath
    }
} else {
    # Pretty output (for humans)
    foreach ($project in $publicProjects | Sort-Object RelativePath) {
        Write-Host "  ✓ " -ForegroundColor Green -NoNewline
        Write-Host "$($project.Name)" -ForegroundColor White -NoNewline
        Write-Host " ($($project.RelativePath))" -ForegroundColor Gray
    }
    Write-Host ""
}

# Export for GitHub Actions or other automation
if ($env:GITHUB_ACTIONS -eq "true") {
    $paths = $publicProjects | ForEach-Object { $_.RelativePath } | Sort-Object
    $pathsJson = $paths | ConvertTo-Json -Compress
    Write-Output "PUBLIC_PROJECTS=$pathsJson" >> $env:GITHUB_OUTPUT
    Write-Host "Exported to GitHub Actions: PUBLIC_PROJECTS" -ForegroundColor Cyan
}
