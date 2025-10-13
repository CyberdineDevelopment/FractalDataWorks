# PowerShell script to setup all Group 4 test projects

$ErrorActionPreference = "Stop"

$testProjects = @(
    @{Test="FractalDataWorks.Services.Scheduling.Abstractions.Tests"; Src="FractalDataWorks.Services.Scheduling.Abstractions"}
    @{Test="FractalDataWorks.Services.Scheduling.Tests"; Src="FractalDataWorks.Services.Scheduling"}
    @{Test="FractalDataWorks.Services.SecretManagers.Abstractions.Tests"; Src="FractalDataWorks.Services.SecretManagers.Abstractions"}
    @{Test="FractalDataWorks.Services.SecretManagers.AzureKeyVault.Tests"; Src="FractalDataWorks.Services.SecretManagers.AzureKeyVault"}
    @{Test="FractalDataWorks.Services.SecretManagers.Tests"; Src="FractalDataWorks.Services.SecretManagers"}
    @{Test="FractalDataWorks.Services.Tests"; Src="FractalDataWorks.Services"}
    @{Test="FractalDataWorks.Services.Transformations.Abstractions.Tests"; Src="FractalDataWorks.Services.Transformations.Abstractions"}
    @{Test="FractalDataWorks.Services.Transformations.Tests"; Src="FractalDataWorks.Services.Transformations"}
    @{Test="FractalDataWorks.ServiceTypes.Tests"; Src="FractalDataWorks.ServiceTypes"}
    @{Test="FractalDataWorks.Web.Http.Abstractions.Tests"; Src="FractalDataWorks.Web.Http.Abstractions"}
    @{Test="FractalDataWorks.Web.RestEndpoints.Tests"; Src="FractalDataWorks.Web.RestEndpoints"}
)

$csprojTemplate = @'
  <ItemGroup>
    <PackageReference Include="Microsoft.NET.Test.Sdk" />
    <PackageReference Include="xunit.v3" />
    <PackageReference Include="xunit.runner.visualstudio" />
    <PackageReference Include="Shouldly" />
    <PackageReference Include="Moq" />
    <PackageReference Include="coverlet.collector" />
  </ItemGroup>

  <ItemGroup>
    <Content Include="xunit.runner.json" CopyToOutputDirectory="PreserveNewest" />
  </ItemGroup>

  <ItemGroup>
    <Using Include="Xunit" />
    <Using Include="Shouldly" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\..\src\{SOURCE}\{SOURCE}.csproj" />
  </ItemGroup>

</Project>
'@

$oldPattern = @'
  <ItemGroup>
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.14.1" />
    <PackageReference Include="xunit.v3" Version="3.0.0" />
    <PackageReference Include="xunit.runner.visualstudio" Version="3.1.3" />
  </ItemGroup>

  <ItemGroup>
    <Content Include="xunit.runner.json" CopyToOutputDirectory="PreserveNewest" />
  </ItemGroup>

  <ItemGroup>
    <Using Include="Xunit" />
  </ItemGroup>

</Project>
'@

Write-Host "Starting Group 4 test project setup..." -ForegroundColor Cyan

foreach ($proj in $testProjects) {
    Write-Host "`nProcessing $($proj.Test)..." -ForegroundColor Yellow

    $testDir = "D:\Development\DK-Tests-G4\tests\$($proj.Test)"
    $csprojPath = "$testDir\$($proj.Test).csproj"
    $unitTestPath = "$testDir\UnitTest1.cs"

    # Remove Unit Test1.cs
    if (Test-Path $unitTestPath) {
        Remove-Item $unitTestPath -Force
        Write-Host "  - Removed UnitTest1.cs" -ForegroundColor Green
    }

    # Update csproj
    if (Test-Path $csprojPath) {
        $content = Get-Content $csprojPath -Raw
        $newContent = $content -replace [regex]::Escape($oldPattern), ($csprojTemplate -replace '\{SOURCE\}', $proj.Src)
        Set-Content $csprojPath $newContent -NoNewline
        Write-Host "  - Updated project file with packages and reference" -ForegroundColor Green
    }
}

Write-Host "`n✓ All Group 4 test projects setup complete!" -ForegroundColor Green
Write-Host "`nNext: Run 'dotnet restore' to restore packages" -ForegroundColor Cyan
