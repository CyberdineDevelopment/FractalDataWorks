# Script to create test projects for all source projects
# Usage: pwsh create-test-projects.ps1

$sourceProjects = @(
    "FractalDataWorks.Abstractions",
    "FractalDataWorks.Collections",
    "FractalDataWorks.Commands.Abstractions",
    "FractalDataWorks.Configuration",
    "FractalDataWorks.Configuration.Abstractions",
    "FractalDataWorks.Data.Abstractions",
    "FractalDataWorks.Data.DataContainers.Abstractions",
    "FractalDataWorks.Data.DataSets.Abstractions",
    "FractalDataWorks.Data.DataStores",
    "FractalDataWorks.Data.DataStores.Abstractions",
    "FractalDataWorks.Data.DataStores.FileSystem",
    "FractalDataWorks.Data.DataStores.Rest",
    "FractalDataWorks.Data.DataStores.SqlServer",
    "FractalDataWorks.DependencyInjection",
    "FractalDataWorks.EnhancedEnums",
    "FractalDataWorks.Messages",
    "FractalDataWorks.Results",
    "FractalDataWorks.Results.Abstractions",
    "FractalDataWorks.Services",
    "FractalDataWorks.Services.Abstractions",
    "FractalDataWorks.Services.Authentication",
    "FractalDataWorks.Services.Authentication.Abstractions",
    "FractalDataWorks.Services.Authentication.Entra",
    "FractalDataWorks.Services.Connections",
    "FractalDataWorks.Services.Connections.Abstractions",
    "FractalDataWorks.Services.Connections.Http.Abstractions",
    "FractalDataWorks.Services.Connections.Rest",
    "FractalDataWorks.Services.DataGateway",
    "FractalDataWorks.Services.DataGateway.Abstractions",
    "FractalDataWorks.Services.Execution",
    "FractalDataWorks.Services.Execution.Abstractions",
    "FractalDataWorks.Services.Scheduling",
    "FractalDataWorks.Services.Scheduling.Abstractions",
    "FractalDataWorks.Services.SecretManagers",
    "FractalDataWorks.Services.SecretManagers.Abstractions",
    "FractalDataWorks.Services.SecretManagers.AzureKeyVault",
    "FractalDataWorks.Services.Transformations",
    "FractalDataWorks.Services.Transformations.Abstractions",
    "FractalDataWorks.ServiceTypes",
    "FractalDataWorks.Web.Http.Abstractions",
    "FractalDataWorks.Web.RestEndpoints"
)

# Excluded projects (source generators, analyzers, code fixes - not testable in same way)
# Also excluded: ClaudeCode and MCP projects (separate testing strategy)

foreach ($project in $sourceProjects) {
    $testProjectName = "$project.Tests"
    $testProjectPath = "tests/$testProjectName"

    Write-Host "Creating test project: $testProjectName" -ForegroundColor Cyan

    # Create test project
    dotnet new xunit -n $testProjectName -o $testProjectPath --force

    # Add reference to source project
    dotnet add "$testProjectPath/$testProjectName.csproj" reference "src/$project/$project.csproj"

    # Add test project to solution
    dotnet sln add "$testProjectPath/$testProjectName.csproj"

    # Create GlobalUsings.cs
    $globalUsings = @"
global using Xunit;
global using Shouldly;
global using Moq;
global using System.Diagnostics.CodeAnalysis;
"@
    Set-Content -Path "$testProjectPath/GlobalUsings.cs" -Value $globalUsings

    # Create README
    $readme = @"
# $testProjectName

Unit tests for the $project project.

## Test Strategy

- **Framework**: xUnit v3
- **Assertions**: Shouldly
- **Mocking**: Moq
- **Coverage Target**: 100% path coverage (measured by Coverlet)

## Coverage

All untestable code (external dependencies, infrastructure, etc.) should be marked with `[ExcludeFromCodeCoverage]` attribute.

## Running Tests

``````bash
# Run tests
dotnet test

# Run tests with coverage
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura

# View coverage report
reportgenerator -reports:coverage.cobertura.xml -targetdir:coverage-report
``````
"@
    Set-Content -Path "$testProjectPath/README.md" -Value $readme

    Write-Host "  ✓ Created $testProjectName" -ForegroundColor Green
}

Write-Host "`nAll test projects created successfully!" -ForegroundColor Green
Write-Host "Total projects: $($sourceProjects.Count)" -ForegroundColor Cyan
