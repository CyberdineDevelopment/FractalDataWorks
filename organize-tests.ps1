# Remove all test projects from solution first
Get-ChildItem tests -Recurse -Filter "*.csproj" | ForEach-Object {
    dotnet sln remove $_.FullName 2>$null
}

# Add test projects with correct folder structure matching src
# tests/src folder structure

# CodeBuilder folder
dotnet sln add tests/FractalDataWorks.CodeBuilder.Abstractions.Tests/FractalDataWorks.CodeBuilder.Abstractions.Tests.csproj --solution-folder "tests/src/CodeBuilder"

# Configuration folder
dotnet sln add tests/FractalDataWorks.Configuration.Tests/FractalDataWorks.Configuration.Tests.csproj --solution-folder "tests/src/Configuration"
dotnet sln add tests/FractalDataWorks.Configuration.Abstractions.Tests/FractalDataWorks.Configuration.Abstractions.Tests.csproj --solution-folder "tests/src/Configuration"

# Enums folder
dotnet sln add tests/FractalDataWorks.EnhancedEnums.Tests/FractalDataWorks.EnhancedEnums.Tests.csproj --solution-folder "tests/src/Enums"
dotnet sln add tests/FractalDataWorks.Collections.Tests/FractalDataWorks.Collections.Tests.csproj --solution-folder "tests/src/Enums"

# Messages folder
dotnet sln add tests/FractalDataWorks.Messages.Tests/FractalDataWorks.Messages.Tests.csproj --solution-folder "tests/src/Messages"

# ServiceTypes folder
dotnet sln add tests/FractalDataWorks.ServiceTypes.Tests/FractalDataWorks.ServiceTypes.Tests.csproj --solution-folder "tests/src/ServiceTypes"

# Root src (top-level projects)
dotnet sln add tests/FractalDataWorks.Abstractions.Tests/FractalDataWorks.Abstractions.Tests.csproj --solution-folder "tests/src"
dotnet sln add tests/FractalDataWorks.DependencyInjection.Tests/FractalDataWorks.DependencyInjection.Tests.csproj --solution-folder "tests/src"
dotnet sln add tests/FractalDataWorks.Results.Tests/FractalDataWorks.Results.Tests.csproj --solution-folder "tests/src"
dotnet sln add tests/FractalDataWorks.Results.Abstractions.Tests/FractalDataWorks.Results.Abstractions.Tests.csproj --solution-folder "tests/src"
dotnet sln add tests/FractalDataWorks.Commands.Abstractions.Tests/FractalDataWorks.Commands.Abstractions.Tests.csproj --solution-folder "tests/src"

# DataContainers folder
dotnet sln add tests/FractalDataWorks.Data.Abstractions.Tests/FractalDataWorks.Data.Abstractions.Tests.csproj --solution-folder "tests/src/DataContainers"
dotnet sln add tests/FractalDataWorks.Data.DataContainers.Abstractions.Tests/FractalDataWorks.Data.DataContainers.Abstractions.Tests.csproj --solution-folder "tests/src/DataContainers"
dotnet sln add tests/FractalDataWorks.Data.DataSets.Abstractions.Tests/FractalDataWorks.Data.DataSets.Abstractions.Tests.csproj --solution-folder "tests/src/DataContainers"
dotnet sln add tests/FractalDataWorks.Data.DataStores.Abstractions.Tests/FractalDataWorks.Data.DataStores.Abstractions.Tests.csproj --solution-folder "tests/src/DataContainers"
dotnet sln add tests/FractalDataWorks.Data.DataStores.FileSystem.Tests/FractalDataWorks.Data.DataStores.FileSystem.Tests.csproj --solution-folder "tests/src/DataContainers"
dotnet sln add tests/FractalDataWorks.Data.DataStores.Rest.Tests/FractalDataWorks.Data.DataStores.Rest.Tests.csproj --solution-folder "tests/src/DataContainers"
dotnet sln add tests/FractalDataWorks.Data.DataStores.SqlServer.Tests/FractalDataWorks.Data.DataStores.SqlServer.Tests.csproj --solution-folder "tests/src/DataContainers"
dotnet sln add tests/FractalDataWorks.Data.DataStores.Tests/FractalDataWorks.Data.DataStores.Tests.csproj --solution-folder "tests/src/DataContainers"

# Services folder and subfolders
dotnet sln add tests/FractalDataWorks.Services.Tests/FractalDataWorks.Services.Tests.csproj --solution-folder "tests/src/Services"
dotnet sln add tests/FractalDataWorks.Services.Abstractions.Tests/FractalDataWorks.Services.Abstractions.Tests.csproj --solution-folder "tests/src/Services"

# Services/Auth
dotnet sln add tests/FractalDataWorks.Services.Authentication.Tests/FractalDataWorks.Services.Authentication.Tests.csproj --solution-folder "tests/src/Services/Auth"
dotnet sln add tests/FractalDataWorks.Services.Authentication.Abstractions.Tests/FractalDataWorks.Services.Authentication.Abstractions.Tests.csproj --solution-folder "tests/src/Services/Auth"
dotnet sln add tests/FractalDataWorks.Services.Authentication.Entra.Tests/FractalDataWorks.Services.Authentication.Entra.Tests.csproj --solution-folder "tests/src/Services/Auth"

# Services/Connections
dotnet sln add tests/FractalDataWorks.Services.Connections.Tests/FractalDataWorks.Services.Connections.Tests.csproj --solution-folder "tests/src/Services/Connections"
dotnet sln add tests/FractalDataWorks.Services.Connections.Abstractions.Tests/FractalDataWorks.Services.Connections.Abstractions.Tests.csproj --solution-folder "tests/src/Services/Connections"
dotnet sln add tests/FractalDataWorks.Services.Connections.Http.Abstractions.Tests/FractalDataWorks.Services.Connections.Http.Abstractions.Tests.csproj --solution-folder "tests/src/Services/Connections"
dotnet sln add tests/FractalDataWorks.Services.Connections.Rest.Tests/FractalDataWorks.Services.Connections.Rest.Tests.csproj --solution-folder "tests/src/Services/Connections"

# Services/Data (DataGateway)
dotnet sln add tests/FractalDataWorks.Services.DataGateway.Tests/FractalDataWorks.Services.DataGateway.Tests.csproj --solution-folder "tests/src/Services"
dotnet sln add tests/FractalDataWorks.Services.DataGateway.Abstractions.Tests/FractalDataWorks.Services.DataGateway.Abstractions.Tests.csproj --solution-folder "tests/src/Services"

# Services/Execution  
dotnet sln add tests/FractalDataWorks.Services.Execution.Tests/FractalDataWorks.Services.Execution.Tests.csproj --solution-folder "tests/src/Services"
dotnet sln add tests/FractalDataWorks.Services.Execution.Abstractions.Tests/FractalDataWorks.Services.Execution.Abstractions.Tests.csproj --solution-folder "tests/src/Services"

# Services/Scheduling
dotnet sln add tests/FractalDataWorks.Services.Scheduling.Tests/FractalDataWorks.Services.Scheduling.Tests.csproj --solution-folder "tests/src/Services/Scheduling"
dotnet sln add tests/FractalDataWorks.Services.Scheduling.Abstractions.Tests/FractalDataWorks.Services.Scheduling.Abstractions.Tests.csproj --solution-folder "tests/src/Services/Scheduling"

# Services/Secrets
dotnet sln add tests/FractalDataWorks.Services.SecretManagers.Tests/FractalDataWorks.Services.SecretManagers.Tests.csproj --solution-folder "tests/src/Services/Secrets"
dotnet sln add tests/FractalDataWorks.Services.SecretManagers.Abstractions.Tests/FractalDataWorks.Services.SecretManagers.Abstractions.Tests.csproj --solution-folder "tests/src/Services/Secrets"
dotnet sln add tests/FractalDataWorks.Services.SecretManagers.AzureKeyVault.Tests/FractalDataWorks.Services.SecretManagers.AzureKeyVault.Tests.csproj --solution-folder "tests/src/Services/Secrets"

# Services/Transformations
dotnet sln add tests/FractalDataWorks.Services.Transformations.Tests/FractalDataWorks.Services.Transformations.Tests.csproj --solution-folder "tests/src/Services/Transformations"
dotnet sln add tests/FractalDataWorks.Services.Transformations.Abstractions.Tests/FractalDataWorks.Services.Transformations.Abstractions.Tests.csproj --solution-folder "tests/src/Services/Transformations"

# UI folder
dotnet sln add tests/FractalDataWorks.Web.Http.Abstractions.Tests/FractalDataWorks.Web.Http.Abstractions.Tests.csproj --solution-folder "tests/src/UI"
dotnet sln add tests/FractalDataWorks.Web.RestEndpoints.Tests/FractalDataWorks.Web.RestEndpoints.Tests.csproj --solution-folder "tests/src/UI"

Write-Host "Test projects organized to mirror src folder structure"
