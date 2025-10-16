# Remove all projects from solution first
Write-Host "Removing all projects from solution..."
$projects = dotnet sln list | Where-Object { $_ -match '.csproj' }
foreach ($proj in $projects) {
    dotnet sln remove $proj 2>$null
}

# Add all projects from private-repo
Write-Host "`nAdding private-repo projects..."
Get-ChildItem -Path "private-repo/src" -Filter "*.csproj" -Recurse -File | ForEach-Object {
    Write-Host "  Adding: $($_.Name)"
    dotnet sln add $_.FullName
}

Get-ChildItem -Path "private-repo/tests" -Filter "*.csproj" -Recurse -File | ForEach-Object {
    Write-Host "  Adding: $($_.Name)"
    dotnet sln add $_.FullName
}

# Add all projects from public-repo
Write-Host "`nAdding public-repo projects..."
Get-ChildItem -Path "public-repo/src" -Filter "*.csproj" -Recurse -File | ForEach-Object {
    Write-Host "  Adding: $($_.Name)"
    dotnet sln add $_.FullName
}

Get-ChildItem -Path "public-repo/tests" -Filter "*.csproj" -Recurse -File | ForEach-Object {
    Write-Host "  Adding: $($_.Name)"
    dotnet sln add $_.FullName
}

Write-Host "`nDone updating solution file"
