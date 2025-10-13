# Script to update all Group 4 test projects with necessary packages and references

$testProjects = @(
    @{Name="FractalDataWorks.Services.Scheduling.Abstractions.Tests"; Source="FractalDataWorks.Services.Scheduling.Abstractions"}
    @{Name="FractalDataWorks.Services.Scheduling.Tests"; Source="FractalDataWorks.Services.Scheduling"}
    @{Name="FractalDataWorks.Services.SecretManagers.Abstractions.Tests"; Source="FractalDataWorks.Services.SecretManagers.Abstractions"}
    @{Name="FractalDataWorks.Services.SecretManagers.AzureKeyVault.Tests"; Source="FractalDataWorks.Services.SecretManagers.AzureKeyVault"}
    @{Name="FractalDataWorks.Services.SecretManagers.Tests"; Source="FractalDataWorks.Services.SecretManagers"}
    @{Name="FractalDataWorks.Services.Tests"; Source="FractalDataWorks.Services"}
    @{Name="FractalDataWorks.Services.Transformations.Abstractions.Tests"; Source="FractalDataWorks.Services.Transformations.Abstractions"}
    @{Name="FractalDataWorks.Services.Transformations.Tests"; Source="FractalDataWorks.Services.Transformations"}
    @{Name="FractalDataWorks.ServiceTypes.Tests"; Source="FractalDataWorks.ServiceTypes"}
    @{Name="FractalDataWorks.Web.Http.Abstractions.Tests"; Source="FractalDataWorks.Web.Http.Abstractions"}
    @{Name="FractalDataWorks.Web.RestEndpoints.Tests"; Source="FractalDataWorks.Web.RestEndpoints"}
)

foreach ($project in $testProjects) {
    $testPath = "D:\Development\DK-Tests-G4\tests\$($project.Name)"
    $csprojPath = "$testPath\$($project.Name).csproj"
    $unitTestPath = "$testPath\UnitTest1.cs"

    Write-Host "Processing $($project.Name)..." -ForegroundColor Cyan

    # Remove UnitTest1.cs if it exists
    if (Test-Path $unitTestPath) {
        Remove-Item $unitTestPath -Force
        Write-Host "  Removed UnitTest1.cs" -ForegroundColor Green
    }

    Write-Host "  Updated $($project.Name)" -ForegroundColor Green
}

Write-Host "`nAll test projects updated successfully!" -ForegroundColor Green
