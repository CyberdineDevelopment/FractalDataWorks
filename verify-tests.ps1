$srcProjects = Get-ChildItem src -Recurse -Filter "*.csproj" | Where-Object { 
    $_.Name -notmatch "(Analyzers|CodeFixes|SourceGenerators)" 
} | Select-Object -ExpandProperty Name | ForEach-Object { $_ -replace '\.csproj$','' }

$testProjects = Get-ChildItem tests -Recurse -Filter "*.csproj" | Select-Object -ExpandProperty Name | ForEach-Object { $_ -replace '\.Tests\.csproj$','' }

Write-Host "Source projects without tests:" -ForegroundColor Yellow
$srcProjects | Where-Object { $testProjects -notcontains $_ } | ForEach-Object {
    Write-Host "  - $_" -ForegroundColor Red
}

Write-Host "`nTest projects without source:" -ForegroundColor Yellow  
$testProjects | Where-Object { $srcProjects -notcontains $_ } | ForEach-Object {
    Write-Host "  - $_" -ForegroundColor Cyan
}

$missing = @($srcProjects | Where-Object { $testProjects -notcontains $_ })
$orphaned = @($testProjects | Where-Object { $srcProjects -notcontains $_ })

Write-Host "`nSummary:" -ForegroundColor Green
Write-Host "  Source projects: $($srcProjects.Count)"
Write-Host "  Test projects: $($testProjects.Count)"
Write-Host "  Missing tests: $($missing.Count)" -ForegroundColor $(if($missing.Count -gt 0){'Red'}else{'Green'})
Write-Host "  Orphaned tests: $($orphaned.Count)" -ForegroundColor $(if($orphaned.Count -gt 0){'Yellow'}else{'Green'})
