$content = Get-Content FractalDataWorks.DeveloperKit.sln -Raw

# First pass: Update MCP projects to private-repo
$content = $content -creplace '"(src|tests)\\(FractalDataWorks\.[Mm][Cc][Pp][^"]+)"', '"private-repo\$1\$2"'

# Second pass: Update non-MCP projects to public-repo
$content = $content -creplace '"(src|tests)\\(FractalDataWorks\.(?![Mm][Cc][Pp])[^"]+)"', '"public-repo\$1\$2"'

# Update props files
$content = $content -replace 'Directory\.Build\.props = Directory\.Build\.props', 'Directory.Build.props = private-repo\Directory.Build.props'
$content = $content -replace 'Directory\.Packages\.props = Directory\.Packages\.props', 'Directory.Packages.props = private-repo\Directory.Packages.props'

Set-Content FractalDataWorks.DeveloperKit.sln $content
Write-Host "Done"
