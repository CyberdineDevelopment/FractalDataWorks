$mcpProjects = Get-ChildItem -Path "private-repo/src" -Filter "*.csproj" -Recurse -File | Where-Object { $_.Directory.Name -match '[Mm][Cc][Pp]' }

foreach ($proj in $mcpProjects) {
    Write-Host "Processing: $($proj.Name)"

    # Read the project file
    $xml = [xml](Get-Content $proj.FullName)

    # Find all ProjectReference elements
    $projectRefs = $xml.SelectNodes("//ProjectReference")

    $updated = $false
    foreach ($ref in $projectRefs) {
        $oldPath = $ref.Include

        # If the reference doesn't contain "mcp" (case insensitive), update it to point to public-repo
        if ($oldPath -notmatch '[Mm][Cc][Pp]') {
            # Extract project name from path (get the .csproj filename without extension)
            if ($oldPath -match '([^\\]+)\.csproj$') {
                $projectName = $matches[1]

                # Determine if it's in src or tests based on original path
                if ($oldPath -match '\\tests\\') {
                    $newPath = "..\..\public-repo\tests\$projectName\$projectName.csproj"
                } else {
                    $newPath = "..\..\public-repo\src\$projectName\$projectName.csproj"
                }

                Write-Host "  Updating: $oldPath -> $newPath"
                $ref.Include = $newPath
                $updated = $true
            }
        }
    }

    if ($updated) {
        $xml.Save($proj.FullName)
    }
}

Write-Host "Done updating MCP project references"
