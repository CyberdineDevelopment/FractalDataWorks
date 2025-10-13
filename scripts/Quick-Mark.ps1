$projects = @(
    'src\FractalDataWorks.Abstractions\FractalDataWorks.Abstractions.csproj',
    'src\FractalDataWorks.Collections\FractalDataWorks.Collections.csproj',
    'src\FractalDataWorks.Messages\FractalDataWorks.Messages.csproj',
    'src\FractalDataWorks.Services\FractalDataWorks.Services.csproj',
    'src\FractalDataWorks.ServiceTypes\FractalDataWorks.ServiceTypes.csproj',
    'src\FractalDataWorks.Services.Execution\FractalDataWorks.Services.Execution.csproj',
    'src\FractalDataWorks.EnhancedEnums\FractalDataWorks.EnhancedEnums.csproj',
    'src\FractalDataWorks.Results\FractalDataWorks.Results.csproj'
)

foreach ($proj in $projects) {
    if (Test-Path $proj) {
        $content = Get-Content $proj -Raw
        if ($content -notmatch '<IsPublicProject>true</IsPublicProject>') {
            $content = $content -replace '(<PropertyGroup>)', "`$1`n    <IsPublicProject>true</IsPublicProject>"
            Set-Content -Path $proj -Value $content -NoNewline
            Write-Host "Marked: $proj" -ForegroundColor Green
        } else {
            Write-Host "Already marked: $proj" -ForegroundColor Gray
        }
    }
}

Write-Host ""
Write-Host "Now run: pwsh -File scripts/Get-PublicProjects.ps1 -ShowDirectories" -ForegroundColor Cyan
