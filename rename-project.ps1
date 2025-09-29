# PowerShell script to rename FractalDataWorks to FractalDataWorks and Generic to Generic
param(
    [string]$RootPath = "D:\FractalDataWorks\FractalDataWorks.DeveloperKit"
)

Write-Host "Starting large-scale rename operation..." -ForegroundColor Green
Write-Host "Root Path: $RootPath" -ForegroundColor Yellow

# File extensions to process
$FileExtensions = @("*.cs", "*.csproj", "*.sln", "*.md", "*.json", "*.xml", "*.props", "*.targets", "*.config", "*.yml", "*.yaml")

# Get all files to process
Write-Host "Finding files to process..." -ForegroundColor Yellow
$FilesToProcess = @()
foreach ($Extension in $FileExtensions) {
    $Files = Get-ChildItem -Path $RootPath -Recurse -Include $Extension -File
    $FilesToProcess += $Files
}

Write-Host "Found $($FilesToProcess.Count) files to process" -ForegroundColor Yellow

# Process file contents
$FileCount = 0
$TotalFiles = $FilesToProcess.Count

foreach ($File in $FilesToProcess) {
    $FileCount++
    Write-Progress -Activity "Processing files" -Status "File $FileCount of $TotalFiles" -PercentComplete (($FileCount / $TotalFiles) * 100)

    try {
        $Content = Get-Content $File.FullName -Raw -ErrorAction Stop
        $OriginalContent = $Content

        # Perform replacements
        $Content = $Content -replace 'FractalDataWorks', 'FractalDataWorks'
        $Content = $Content -replace 'FractalDataWorks', 'FractalDataWorks'  # Handle lowercase variant
        $Content = $Content -replace 'Generic', 'Generic'
        $Content = $Content -replace 'Generic', 'generic'  # Handle lowercase variant

        # Only write if content changed
        if ($Content -ne $OriginalContent) {
            Set-Content -Path $File.FullName -Value $Content -NoNewline -ErrorAction Stop
            Write-Host "Updated: $($File.FullName)" -ForegroundColor Green
        }
    }
    catch {
        Write-Warning "Failed to process file: $($File.FullName) - $($_.Exception.Message)"
    }
}

Write-Progress -Activity "Processing files" -Completed

# Now rename directories and files
Write-Host "`nRenaming directories and files..." -ForegroundColor Yellow

# Get all directories with FractalDataWorks or Generic in the name (process deepest first)
$DirsToRename = Get-ChildItem -Path $RootPath -Recurse -Directory |
    Where-Object { $_.Name -match 'FractalDataWorks|Generic' } |
    Sort-Object FullName -Descending

foreach ($Dir in $DirsToRename) {
    $NewName = $Dir.Name -replace 'FractalDataWorks', 'FractalDataWorks' -replace 'Generic', 'Generic'
    if ($NewName -ne $Dir.Name) {
        $NewPath = Join-Path $Dir.Parent.FullName $NewName
        try {
            Rename-Item -Path $Dir.FullName -NewName $NewName -ErrorAction Stop
            Write-Host "Renamed directory: $($Dir.FullName) -> $NewPath" -ForegroundColor Green
        }
        catch {
            Write-Warning "Failed to rename directory: $($Dir.FullName) - $($_.Exception.Message)"
        }
    }
}

# Get all files with FractalDataWorks or Generic in the name
$FilesToRename = Get-ChildItem -Path $RootPath -Recurse -File |
    Where-Object { $_.Name -match 'FractalDataWorks|Generic' }

foreach ($File in $FilesToRename) {
    $NewName = $File.Name -replace 'FractalDataWorks', 'FractalDataWorks' -replace 'Generic', 'Generic'
    if ($NewName -ne $File.Name) {
        $NewPath = Join-Path $File.Directory.FullName $NewName
        try {
            Rename-Item -Path $File.FullName -NewName $NewName -ErrorAction Stop
            Write-Host "Renamed file: $($File.FullName) -> $NewPath" -ForegroundColor Green
        }
        catch {
            Write-Warning "Failed to rename file: $($File.FullName) - $($_.Exception.Message)"
        }
    }
}

Write-Host "`nRename operation completed!" -ForegroundColor Green
Write-Host "Summary:" -ForegroundColor Yellow
Write-Host "- Processed $TotalFiles files for content replacement" -ForegroundColor White
Write-Host "- Renamed directories and files containing 'FractalDataWorks' or 'Generic'" -ForegroundColor White
Write-Host "`nNote: You may need to:" -ForegroundColor Yellow
Write-Host "1. Update any remaining references in .git files" -ForegroundColor White
Write-Host "2. Update project references in any IDE settings" -ForegroundColor White
Write-Host "3. Rebuild the solution to ensure everything works" -ForegroundColor White