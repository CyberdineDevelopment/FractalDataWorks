$projects = dotnet sln list | Where-Object { $_ -match '.csproj' -and $_ -notmatch '[Mm][Cc][Pp]' }

foreach ($proj in $projects) {
    Write-Host "Removing: $proj"
    dotnet sln remove $proj
}
