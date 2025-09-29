# PowerShell script to clean build artifacts and rename FractalDataWorks to FractalDataWorks
param(
    [string]$RootPath = "D:\FractalDataWorks\FractalDataWorks.DeveloperKit"
)

Write-Host "Starting cleanup and rename operation..." -ForegroundColor Green
Write-Host "Root Path: $RootPath" -ForegroundColor Yellow

# Step 1: Clean up build artifacts and IDE files
Write-Host "`nStep 1: Cleaning up build artifacts and IDE files..." -ForegroundColor Cyan

$FoldersToDelete = @(
    "bin", "obj", ".vs", ".vscode", "packages", "TestResults",
    "artifacts", "publish", "out", "dist", "build", "target"
)

$FilesToDelete = @(
    "*.suo", "*.user", "*.userosscache", "*.sln.docstates",
    "*.userprefs", "*.pidb", "*.booproj", "*.svd", "*.pdb",
    "*.log", "*.tmp", "*.temp", "*.cache"
)

# Delete folders
foreach ($FolderPattern in $FoldersToDelete) {
    $Folders = Get-ChildItem -Path $RootPath -Recurse -Directory -Name $FolderPattern -ErrorAction SilentlyContinue
    foreach ($Folder in $Folders) {
        $FullPath = Join-Path $RootPath $Folder
        if (Test-Path $FullPath) {
            try {
                Remove-Item -Path $FullPath -Recurse -Force -ErrorAction Stop
                Write-Host "Deleted folder: $FullPath" -ForegroundColor Red
            }
            catch {
                Write-Warning "Failed to delete folder: $FullPath - $($_.Exception.Message)"
            }
        }
    }
}

# Delete files
foreach ($FilePattern in $FilesToDelete) {
    $Files = Get-ChildItem -Path $RootPath -Recurse -File -Name $FilePattern -ErrorAction SilentlyContinue
    foreach ($File in $Files) {
        $FullPath = Join-Path $RootPath $File
        if (Test-Path $FullPath) {
            try {
                Remove-Item -Path $FullPath -Force -ErrorAction Stop
                Write-Host "Deleted file: $FullPath" -ForegroundColor Red
            }
            catch {
                Write-Warning "Failed to delete file: $FullPath - $($_.Exception.Message)"
            }
        }
    }
}

Write-Host "Cleanup completed!" -ForegroundColor Green

# Step 2: Rename file and folder contents
Write-Host "`nStep 2: Updating file contents..." -ForegroundColor Cyan

# File extensions to process
$FileExtensions = @("*.cs", "*.csproj", "*.sln", "*.md", "*.json", "*.xml", "*.props", "*.targets", "*.config", "*.yml", "*.yaml", "*.txt", "*.ps1")

# Get all files to process
Write-Host "Finding files to process..." -ForegroundColor Yellow
$FilesToProcess = @()
foreach ($Extension in $FileExtensions) {
    $Files = Get-ChildItem -Path $RootPath -Recurse -Include $Extension -File -ErrorAction SilentlyContinue
    $FilesToProcess += $Files
}

Write-Host "Found $($FilesToProcess.Count) files to process" -ForegroundColor Yellow

# Process file contents
$FileCount = 0
$TotalFiles = $FilesToProcess.Count

foreach ($File in $FilesToProcess) {
    $FileCount++
    Write-Progress -Activity "Processing file contents" -Status "File $FileCount of $TotalFiles" -PercentComplete (($FileCount / $TotalFiles) * 100)

    try {
        $Content = Get-Content $File.FullName -Raw -ErrorAction Stop
        $OriginalContent = $Content

        # Perform replacements (order matters for proper replacement)
        $Content = $Content -replace 'FractalDataWorks', 'FractalDataWorks'
        $Content = $Content -replace 'FractalDataWorks', 'FractalDataWorks'  # Handle lowercase variant
        $Content = $Content -replace 'FractalDataWorks', 'FRACTALDATAWORKS'  # Handle uppercase variant
        $Content = $Content -replace 'Generic', 'Generic'
        $Content = $Content -replace 'Generic', 'generic'  # Handle lowercase variant
        $Content = $Content -replace 'Generic', 'GENERIC'  # Handle uppercase variant

        # Only write if content changed
        if ($Content -ne $OriginalContent) {
            Set-Content -Path $File.FullName -Value $Content -NoNewline -ErrorAction Stop
            Write-Host "Updated content: $($File.FullName)" -ForegroundColor Green
        }
    }
    catch {
        Write-Warning "Failed to process file: $($File.FullName) - $($_.Exception.Message)"
    }
}

Write-Progress -Activity "Processing file contents" -Completed

# Step 3: Rename directories (process deepest first to avoid path conflicts)
Write-Host "`nStep 3: Renaming directories..." -ForegroundColor Cyan

$DirsToRename = Get-ChildItem -Path $RootPath -Recurse -Directory -ErrorAction SilentlyContinue |
    Where-Object { $_.Name -match 'FractalDataWorks|FractalDataWorks|FractalDataWorks|Generic|Generic|Generic' } |
    Sort-Object FullName -Descending

foreach ($Dir in $DirsToRename) {
    $NewName = $Dir.Name
    $NewName = $NewName -replace 'FractalDataWorks', 'FractalDataWorks'
    $NewName = $NewName -replace 'FractalDataWorks', 'FractalDataWorks'
    $NewName = $NewName -replace 'FractalDataWorks', 'FRACTALDATAWORKS'
    $NewName = $NewName -replace 'Generic', 'Generic'
    $NewName = $NewName -replace 'Generic', 'generic'
    $NewName = $NewName -replace 'Generic', 'GENERIC'

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

# Step 4: Rename files
Write-Host "`nStep 4: Renaming files..." -ForegroundColor Cyan

$FilesToRename = Get-ChildItem -Path $RootPath -Recurse -File -ErrorAction SilentlyContinue |
    Where-Object { $_.Name -match 'FractalDataWorks|FractalDataWorks|FractalDataWorks|Generic|Generic|Generic' }

foreach ($File in $FilesToRename) {
    $NewName = $File.Name
    $NewName = $NewName -replace 'FractalDataWorks', 'FractalDataWorks'
    $NewName = $NewName -replace 'FractalDataWorks', 'FractalDataWorks'
    $NewName = $NewName -replace 'FractalDataWorks', 'FRACTALDATAWORKS'
    $NewName = $NewName -replace 'Generic', 'Generic'
    $NewName = $NewName -replace 'Generic', 'generic'
    $NewName = $NewName -replace 'Generic', 'GENERIC'

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

Write-Host "`nCleanup and rename operation completed!" -ForegroundColor Green
Write-Host "Summary:" -ForegroundColor Yellow
Write-Host "- Cleaned up build artifacts (bin, obj, .vs, etc.)" -ForegroundColor White
Write-Host "- Processed $TotalFiles files for content replacement" -ForegroundColor White
Write-Host "- Renamed directories and files containing old names" -ForegroundColor White
Write-Host "`nReplacements made:" -ForegroundColor Yellow
Write-Host "- FractalDataWorks -> FractalDataWorks" -ForegroundColor White
Write-Host "- Generic -> Generic" -ForegroundColor White
Write-Host "- All case variations handled" -ForegroundColor White
Write-Host "`nNext steps:" -ForegroundColor Yellow
Write-Host "1. Rebuild the solution to ensure everything works" -ForegroundColor White
Write-Host "2. Update any remaining Git configuration if needed" -ForegroundColor White
Write-Host "3. Test the renamed solution" -ForegroundColor White