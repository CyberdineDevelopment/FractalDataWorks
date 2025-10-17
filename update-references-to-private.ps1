$mcpProjects = Get-ChildItem -Path "src" -Filter "*.csproj" -Recurse -File | Where-Object { $_.Directory.Name -match '[Mm][Cc][Pp]' }

foreach ($proj in $mcpProjects) {
    Write-Host "Processing: $($proj.Name)"

    # Read the project file
    $xml = [xml](Get-Content $proj.FullName)

    # Find all ProjectReference elements
    $projectRefs = $xml.SelectNodes("//ProjectReference")

    foreach ($ref in $projectRefs) {
        $oldPath = $ref.Include
        if ($oldPath -match '^\.\.[/\\]([^/\\]+)[/\\]') {
            # Only update if it starts with ../ and doesn't already point to private-repo
            if ($oldPath -notmatch 'private-repo') {
                $newPath = $oldPath -replace '^\.\.[/\\]', '..\..\private-repo\src\'
                Write-Host "  Updating: $oldPath -> $newPath"
                $ref.Include = $newPath
            }
        }
    }

    # Save the file
    $xml.Save($proj.FullName)
}

Write-Host "Done updating project references"
