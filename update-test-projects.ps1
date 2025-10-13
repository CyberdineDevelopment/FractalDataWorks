$testProjects = @(
    "FractalDataWorks.Commands.Abstractions.Tests",
    "FractalDataWorks.Configuration.Abstractions.Tests",
    "FractalDataWorks.Configuration.Tests",
    "FractalDataWorks.Data.Abstractions.Tests",
    "FractalDataWorks.Data.DataContainers.Abstractions.Tests",
    "FractalDataWorks.Data.DataSets.Abstractions.Tests",
    "FractalDataWorks.Data.DataStores.Abstractions.Tests",
    "FractalDataWorks.Data.DataStores.FileSystem.Tests"
)

$csprojTemplate = @"
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <OutputType>Exe</OutputType>
    <IsPackable>false</IsPackable>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.NET.Test.Sdk" />
    <PackageReference Include="xunit.v3" />
    <PackageReference Include="xunit.runner.visualstudio" />
    <PackageReference Include="Shouldly" />
    <PackageReference Include="Moq" />
    <PackageReference Include="coverlet.collector" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\..\src\{0}\{0}.csproj" />
  </ItemGroup>

  <ItemGroup>
    <Content Include="xunit.runner.json" CopyToOutputDirectory="PreserveNewest" />
  </ItemGroup>

  <ItemGroup>
    <Using Include="Xunit" />
    <Using Include="Shouldly" />
  </ItemGroup>

</Project>
"@

foreach ($project in $testProjects) {
    $srcProject = $project -replace "\.Tests$", ""
    $csprojPath = "tests\$project\$project.csproj"
    $content = $csprojTemplate -f $srcProject
    Set-Content -Path $csprojPath -Value $content
    Write-Host "Updated $csprojPath"
}

Write-Host "All test projects updated successfully!"
