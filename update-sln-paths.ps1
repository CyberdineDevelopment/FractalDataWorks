$slnPath = "FractalDataWorks.DeveloperKit.sln"
$content = Get-Content $slnPath -Raw

# Update non-MCP projects: src\ -> public-repo\src\
# But NOT for MCP projects
$lines = $content -split "`r?`n"
$updatedLines = @()

foreach ($line in $lines) {
    if ($line -match 'Project\("{[^}]+}"\) = "([^"]+)", "([^"]+)", "{[^}]+}"') {
        $projectName = $matches[1]
        $projectPath = $matches[2]

        # If path starts with src\ or tests\ and doesn't contain Mcp/MCP
        if (($projectPath -match '^(src|tests)\\' ) -and ($projectPath -notmatch '[Mm][Cc][Pp]')) {
            $newPath = $projectPath -replace '^src\\', 'public-repo\src\' -replace '^tests\\', 'public-repo\tests\'
            $line = $line -replace [regex]::Escape($projectPath), $newPath
            Write-Host "Updated: $projectPath -> $newPath"
        }
        # If path starts with src\ or tests\ and contains Mcp/MCP
        elseif (($projectPath -match '^(src|tests)\\' ) -and ($projectPath -match '[Mm][Cc][Pp]')) {
            $newPath = $projectPath -replace '^src\\', 'private-repo\src\' -replace '^tests\\', 'private-repo\tests\'
            $line = $line -replace [regex]::Escape($projectPath), $newPath
            Write-Host "Updated: $projectPath -> $newPath"
        }
    }
    # Also update Solution Items paths for .props files
    elseif ($line -match '(Directory\.Build\.props|Directory\.Packages\.props) =') {
        $line = $line -replace 'Directory\.Build\.props = Directory\.Build\.props', 'Directory.Build.props = private-repo\Directory.Build.props'
        $line = $line -replace 'Directory\.Packages\.props = Directory\.Packages\.props', 'Directory.Packages.props = private-repo\Directory.Packages.props'
    }

    $updatedLines += $line
}

$updatedContent = $updatedLines -join "`r`n"
Set-Content -Path $slnPath -Value $updatedContent -NoNewline

Write-Host "`nDone updating solution file paths"
