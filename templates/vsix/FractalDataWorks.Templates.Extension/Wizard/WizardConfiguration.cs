using System;

namespace FractalDataWorks.Templates.Extension.Wizard;

public sealed class WizardConfiguration
{
    // Solution Type
    public string SolutionType { get; set; } = "FullStack";
    
    // Project Inclusion
    public bool IncludeApiHost { get; set; } = true;
    public bool IncludeEtlHost { get; set; } = true;
    public bool IncludeSchedulerHost { get; set; } = true;
    public bool IncludeTests { get; set; } = true;
    public bool IncludeDocker { get; set; } = true;
    
    // Authentication
    public string AuthenticationType { get; set; } = "None";
    public string AzureAdTenantId { get; set; }
    public string AzureAdClientId { get; set; }
    public string JwtIssuer { get; set; }
    public string JwtAudience { get; set; }
    
    // Database
    public string DatabaseProvider { get; set; } = "None";
    public string ConnectionString { get; set; }
    public bool UseEntityFramework { get; set; } = true;
    public bool IncludeMigrations { get; set; } = true;
    
    // Security
    public string SecretManagement { get; set; } = "Configuration";
    public string KeyVaultName { get; set; }
    public bool IncludeRateLimiting { get; set; } = true;
    public bool IncludeCors { get; set; } = true;
    public bool IncludeHealthChecks { get; set; } = true;
    
    // CI/CD
    public bool IncludeCiCd { get; set; } = false;
    public string CiCdProvider { get; set; } = "GitHubActions";
    public bool IncludeDockerBuild { get; set; } = true;
    public bool IncludeTestsInPipeline { get; set; } = true;
    public bool IncludeCodeCoverage { get; set; } = true;
    
    // Additional Settings
    public string ProjectName { get; set; } = "MyProject";
    public string CompanyName { get; set; } = "MyCompany";
    public string Description { get; set; } = "FractalDataWorks solution";
    
    public override string ToString()
    {
        return $"SolutionType: {SolutionType}, " +
               $"IncludeApi: {IncludeApiHost}, " +
               $"IncludeEtl: {IncludeEtlHost}, " +
               $"IncludeScheduler: {IncludeSchedulerHost}, " +
               $"AuthType: {AuthenticationType}, " +
               $"DatabaseProvider: {DatabaseProvider}";
    }
}
