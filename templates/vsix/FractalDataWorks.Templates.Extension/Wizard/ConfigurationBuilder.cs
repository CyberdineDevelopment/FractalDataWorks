using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;

namespace FractalDataWorks.Templates.Extension.Wizard;

public sealed class ConfigurationBuilder
{
    public string BuildSolutionFile(WizardConfiguration config, string solutionName)
    {
        var sb = new StringBuilder();
        
        // Solution header
        sb.AppendLine("Microsoft Visual Studio Solution File, Format Version 12.00");
        sb.AppendLine("# Visual Studio Version 17");
        sb.AppendLine("VisualStudioVersion = 17.0.31903.59");
        sb.AppendLine("MinimumVisualStudioVersion = 10.0.40219.1");
        
        // Projects
        var projects = GetProjectsForConfiguration(config, solutionName);
        foreach (var project in projects)
        {
            sb.AppendLine($"Project(\"{project.TypeGuid}\") = \"{project.Name}\", \"{project.Path}\", \"{project.ProjectGuid}\"");
            sb.AppendLine("EndProject");
        }
        
        // Solution configuration
        sb.AppendLine("Global");
        sb.AppendLine("\tGlobalSection(SolutionConfigurationPlatforms) = preSolution");
        sb.AppendLine("\t\tDebug|Any CPU = Debug|Any CPU");
        sb.AppendLine("\t\tRelease|Any CPU = Release|Any CPU");
        sb.AppendLine("\tEndGlobalSection");
        
        // Project configuration
        sb.AppendLine("\tGlobalSection(ProjectConfigurationPlatforms) = postSolution");
        foreach (var project in projects)
        {
            sb.AppendLine($"\t\t{project.ProjectGuid}.Debug|Any CPU.ActiveCfg = Debug|Any CPU");
            sb.AppendLine($"\t\t{project.ProjectGuid}.Debug|Any CPU.Build.0 = Debug|Any CPU");
            sb.AppendLine($"\t\t{project.ProjectGuid}.Release|Any CPU.ActiveCfg = Release|Any CPU");
            sb.AppendLine($"\t\t{project.ProjectGuid}.Release|Any CPU.Build.0 = Release|Any CPU");
        }
        sb.AppendLine("\tEndGlobalSection");
        
        // Solution properties
        sb.AppendLine("\tGlobalSection(SolutionProperties) = preSolution");
        sb.AppendLine("\t\tHideSolutionNode = FALSE");
        sb.AppendLine("\tEndGlobalSection");
        
        // Nested projects (solution folders)
        var folders = GetSolutionFolders(config);
        if (folders.Count > 0)
        {
            sb.AppendLine("\tGlobalSection(NestedProjects) = preSolution");
            foreach (var folder in folders)
            {
                foreach (var projectGuid in folder.Value)
                {
                    sb.AppendLine($"\t\t{projectGuid} = {folder.Key}");
                }
            }
            sb.AppendLine("\tEndGlobalSection");
        }
        
        sb.AppendLine("EndGlobal");
        
        return sb.ToString();
    }
    
    public string BuildDockerComposeFile(WizardConfiguration config, string solutionName)
    {
        if (!config.IncludeDocker)
        {
            return string.Empty;
        }
        
        var sb = new StringBuilder();
        
        sb.AppendLine("version: '3.8'");
        sb.AppendLine();
        sb.AppendLine("services:");
        
        if (config.IncludeApiHost)
        {
            sb.AppendLine($"  {solutionName.ToLowerInvariant()}-api:");
            sb.AppendLine($"    build:");
            sb.AppendLine($"      context: .");
            sb.AppendLine($"      dockerfile: {solutionName}.Api/Dockerfile");
            sb.AppendLine($"    ports:");
            sb.AppendLine($"      - \"8080:8080\"");
            sb.AppendLine($"    environment:");
            sb.AppendLine($"      - ASPNETCORE_ENVIRONMENT=Development");
            if (config.DatabaseProvider != "None")
            {
                sb.AppendLine($"    depends_on:");
                sb.AppendLine($"      - database");
            }
            sb.AppendLine();
        }
        
        if (config.IncludeEtlHost)
        {
            sb.AppendLine($"  {solutionName.ToLowerInvariant()}-etl:");
            sb.AppendLine($"    build:");
            sb.AppendLine($"      context: .");
            sb.AppendLine($"      dockerfile: {solutionName}.Etl/Dockerfile");
            sb.AppendLine($"    environment:");
            sb.AppendLine($"      - DOTNET_ENVIRONMENT=Development");
            if (config.DatabaseProvider != "None")
            {
                sb.AppendLine($"    depends_on:");
                sb.AppendLine($"      - database");
            }
            sb.AppendLine();
        }
        
        if (config.IncludeSchedulerHost)
        {
            sb.AppendLine($"  {solutionName.ToLowerInvariant()}-scheduler:");
            sb.AppendLine($"    build:");
            sb.AppendLine($"      context: .");
            sb.AppendLine($"      dockerfile: {solutionName}.Scheduler/Dockerfile");
            sb.AppendLine($"    environment:");
            sb.AppendLine($"      - DOTNET_ENVIRONMENT=Development");
            if (config.DatabaseProvider != "None")
            {
                sb.AppendLine($"    depends_on:");
                sb.AppendLine($"      - database");
            }
            sb.AppendLine();
        }
        
        // Database service
        if (config.DatabaseProvider == "SqlServer")
        {
            sb.AppendLine($"  database:");
            sb.AppendLine($"    image: mcr.microsoft.com/mssql/server:2022-latest");
            sb.AppendLine($"    environment:");
            sb.AppendLine($"      - ACCEPT_EULA=Y");
            sb.AppendLine($"      - SA_PASSWORD=YourStrong@Passw0rd");
            sb.AppendLine($"    ports:");
            sb.AppendLine($"      - \"1433:1433\"");
            sb.AppendLine($"    volumes:");
            sb.AppendLine($"      - sqlserver_data:/var/opt/mssql");
            sb.AppendLine();
        }
        else if (config.DatabaseProvider == "PostgreSQL")
        {
            sb.AppendLine($"  database:");
            sb.AppendLine($"    image: postgres:15");
            sb.AppendLine($"    environment:");
            sb.AppendLine($"      - POSTGRES_DB={solutionName.ToLowerInvariant()}");
            sb.AppendLine($"      - POSTGRES_USER=postgres");
            sb.AppendLine($"      - POSTGRES_PASSWORD=postgres");
            sb.AppendLine($"    ports:");
            sb.AppendLine($"      - \"5432:5432\"");
            sb.AppendLine($"    volumes:");
            sb.AppendLine($"      - postgres_data:/var/lib/postgresql/data");
            sb.AppendLine();
        }
        
        // Volumes
        if (config.DatabaseProvider != "None")
        {
            sb.AppendLine("volumes:");
            if (config.DatabaseProvider == "SqlServer")
            {
                sb.AppendLine("  sqlserver_data:");
            }
            else if (config.DatabaseProvider == "PostgreSQL")
            {
                sb.AppendLine("  postgres_data:");
            }
        }
        
        return sb.ToString();
    }
    
    public string BuildGitHubActionsWorkflow(WizardConfiguration config, string solutionName)
    {
        if (!config.IncludeCiCd || config.CiCdProvider != "GitHubActions")
        {
            return string.Empty;
        }
        
        var workflow = new
        {
            name = $"{solutionName} CI/CD",
            on = new
            {
                push = new { branches = new[] { "master", "main", "develop" } },
                pull_request = new { branches = new[] { "master", "main" } }
            },
            jobs = new
            {
                build = new
                {
                    name = "Build and Test",
                    runs_on = "ubuntu-latest",
                    steps = new object[]
                    {
                        new
                        {
                            name = "Checkout code",
                            uses = "actions/checkout@v4"
                        },
                        new
                        {
                            name = "Setup .NET",
                            uses = "actions/setup-dotnet@v4",
                            with_ = new { dotnet_version = "8.0.x" }
                        },
                        new
                        {
                            name = "Restore dependencies",
                            run = "dotnet restore"
                        },
                        new
                        {
                            name = "Build solution",
                            run = "dotnet build --no-restore --configuration Release"
                        },
                        config.IncludeTestsInPipeline ? new
                        {
                            name = "Run tests",
                            run = "dotnet test --no-build --configuration Release --verbosity normal"
                        } : null,
                        config.IncludeDockerBuild ? new
                        {
                            name = "Build Docker images",
                            run = "docker-compose build"
                        } : null
                    }.Where(step => step != null).ToArray()
                }
            }
        };
        
        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.KebabCaseLower
        };
        
        return JsonSerializer.Serialize(workflow, options)
            .Replace("\"with_\"", "\"with\"");
    }
    
    private IEnumerable<SolutionProject> GetProjectsForConfiguration(WizardConfiguration config, string solutionName)
    {
        var projects = new List<SolutionProject>();
        
        if (config.IncludeApiHost)
        {
            projects.Add(new SolutionProject
            {
                Name = $"{solutionName}.Api",
                Path = $"{solutionName}.Api\\{solutionName}.Api.csproj",
                ProjectGuid = Guid.NewGuid().ToString("B").ToUpperInvariant(),
                TypeGuid = "{9A19103F-16F7-4668-BE54-9A1E7A4F7556}" // .NET SDK project
            });
        }
        
        if (config.IncludeEtlHost)
        {
            projects.Add(new SolutionProject
            {
                Name = $"{solutionName}.Etl",
                Path = $"{solutionName}.Etl\\{solutionName}.Etl.csproj",
                ProjectGuid = Guid.NewGuid().ToString("B").ToUpperInvariant(),
                TypeGuid = "{9A19103F-16F7-4668-BE54-9A1E7A4F7556}"
            });
        }
        
        if (config.IncludeSchedulerHost)
        {
            projects.Add(new SolutionProject
            {
                Name = $"{solutionName}.Scheduler",
                Path = $"{solutionName}.Scheduler\\{solutionName}.Scheduler.csproj",
                ProjectGuid = Guid.NewGuid().ToString("B").ToUpperInvariant(),
                TypeGuid = "{9A19103F-16F7-4668-BE54-9A1E7A4F7556}"
            });
        }
        
        if (config.IncludeTests)
        {
            projects.Add(new SolutionProject
            {
                Name = $"{solutionName}.Tests",
                Path = $"{solutionName}.Tests\\{solutionName}.Tests.csproj",
                ProjectGuid = Guid.NewGuid().ToString("B").ToUpperInvariant(),
                TypeGuid = "{9A19103F-16F7-4668-BE54-9A1E7A4F7556}"
            });
        }
        
        return projects;
    }
    
    private Dictionary<string, List<string>> GetSolutionFolders(WizardConfiguration config)
    {
        var folders = new Dictionary<string, List<string>>(StringComparer.Ordinal);
        
        // Create solution folders if needed
        if (config.IncludeCiCd || config.IncludeDocker)
        {
            folders["Build"] = new List<string>();
        }
        
        return folders;
    }
}

public sealed class SolutionProject
{
    public string Name { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public string ProjectGuid { get; set; } = string.Empty;
    public string TypeGuid { get; set; } = string.Empty;
}
