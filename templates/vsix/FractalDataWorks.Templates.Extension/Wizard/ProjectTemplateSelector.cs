using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace FractalDataWorks.Templates.Extension.Wizard;

public sealed class ProjectTemplateSelector
{
    private readonly Dictionary<string, TemplateInfo> _templates;
    
    public ProjectTemplateSelector()
    {
        _templates = LoadTemplates();
    }
    
    public TemplateInfo GetTemplate(string templateId)
    {
        return _templates.TryGetValue(templateId, out var template) ? template : null;
    }
    
    public IEnumerable<TemplateInfo> GetTemplatesForSolution(WizardConfiguration config)
    {
        var templates = new List<TemplateInfo>();
        
        if (config.IncludeApiHost)
        {
            var apiTemplate = GetTemplate("Rec-api-host");
            if (apiTemplate != null)
            {
                templates.Add(ConfigureTemplate(apiTemplate, config));
            }
        }
        
        if (config.IncludeEtlHost)
        {
            var etlTemplate = GetTemplate("Rec-etl-host");
            if (etlTemplate != null)
            {
                templates.Add(ConfigureTemplate(etlTemplate, config));
            }
        }
        
        if (config.IncludeSchedulerHost)
        {
            var schedulerTemplate = GetTemplate("Rec-scheduler-host");
            if (schedulerTemplate != null)
            {
                templates.Add(ConfigureTemplate(schedulerTemplate, config));
            }
        }
        
        return templates;
    }
    
    public IEnumerable<string> GetRequiredPackages(WizardConfiguration config)
    {
        var packages = new HashSet<string>(StringComparer.Ordinal)
        {
            "FractalDataWorks.Configuration",
            "FractalDataWorks.DependencyInjection",
            "FractalDataWorks.Results",
            "FractalDataWorks.Messages"
        };
        
        // Authentication packages
        switch (config.AuthenticationType)
        {
            case "AzureAD":
                packages.Add("FractalDataWorks.Services.Authentication.AzureEntra");
                packages.Add("Microsoft.AspNetCore.Authentication.JwtBearer");
                break;
            case "JWT":
                packages.Add("Microsoft.AspNetCore.Authentication.JwtBearer");
                break;
        }
        
        // Database packages
        switch (config.DatabaseProvider)
        {
            case "SqlServer":
                packages.Add("FractalDataWorks.Services.Connections.MsSql");
                if (config.UseEntityFramework)
                {
                    packages.Add("Microsoft.EntityFrameworkCore.SqlServer");
                    packages.Add("Microsoft.EntityFrameworkCore.Tools");
                }
                break;
            case "PostgreSQL":
                if (config.UseEntityFramework)
                {
                    packages.Add("Microsoft.EntityFrameworkCore.PostgreSQL");
                }
                break;
        }
        
        // Secret management packages
        if (config.SecretManagement == "AzureKeyVault")
        {
            packages.Add("FractalDataWorks.Services.SecretManagement.AzureKeyVault");
        }
        
        // Host-specific packages
        if (config.IncludeApiHost)
        {
            packages.Add("FractalDataWorks.RestEndpoints");
        }
        
        if (config.IncludeEtlHost)
        {
            packages.Add("FractalDataWorks.Services.Transformations.Abstractions");
        }
        
        if (config.IncludeSchedulerHost)
        {
            packages.Add("FractalDataWorks.Services.Scheduling.Abstractions");
        }
        
        return packages;
    }
    
    private Dictionary<string, TemplateInfo> LoadTemplates()
    {
        try
        {
            var catalogPath = GetTemplateCatalogPath();
            if (!File.Exists(catalogPath))
            {
                return new Dictionary<string, TemplateInfo>(StringComparer.Ordinal);
            }
            
            var json = File.ReadAllText(catalogPath);
            var catalog = JsonSerializer.Deserialize<TemplateCatalog>(json);
            
            return catalog?.Templates?.ToDictionary(t => t.Id, StringComparer.Ordinal) 
                   ?? new Dictionary<string, TemplateInfo>(StringComparer.Ordinal);
        }
        catch
        {
            return new Dictionary<string, TemplateInfo>(StringComparer.Ordinal);
        }
    }
    
    private TemplateInfo ConfigureTemplate(TemplateInfo template, WizardConfiguration config)
    {
        // Create a configured copy of the template
        var configuredTemplate = new TemplateInfo
        {
            Id = template.Id,
            Name = template.Name,
            Description = template.Description,
            Category = template.Category,
            Tags = template.Tags,
            Parameters = new Dictionary<string, object>(StringComparer.Ordinal)
        };
        
        // Apply configuration to template parameters
        if (template.Id == "Rec-api-host")
        {
            configuredTemplate.Parameters["AuthenticationType"] = config.AuthenticationType;
            configuredTemplate.Parameters["DatabaseProvider"] = config.DatabaseProvider;
            configuredTemplate.Parameters["IncludeDocker"] = config.IncludeDocker;
            configuredTemplate.Parameters["IncludeHealthChecks"] = config.IncludeHealthChecks;
        }
        else if (template.Id == "Rec-etl-host")
        {
            configuredTemplate.Parameters["ProcessingType"] = "Both"; // Default for wizard
            configuredTemplate.Parameters["DataSource"] = "Database"; // Default for wizard
        }
        else if (template.Id == "Rec-scheduler-host")
        {
            configuredTemplate.Parameters["SchedulerType"] = "Quartz"; // Default for wizard
        }
        
        return configuredTemplate;
    }
    
    private static string GetTemplateCatalogPath()
    {
        // Try to find the catalog in the extension directory
        var extensionDir = Path.GetDirectoryName(typeof(ProjectTemplateSelector).Assembly.Location);
        var catalogPath = Path.Combine(extensionDir ?? string.Empty, "catalog", "templates.json");
        
        if (File.Exists(catalogPath))
        {
            return catalogPath;
        }
        
        // Fallback to embedded resource or default location
        return Path.Combine(Path.GetTempPath(), "templates.json");
    }
}

public sealed class TemplateInfo
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string[] Tags { get; set; } = Array.Empty<string>();
    public Dictionary<string, object> Parameters { get; set; } = new(StringComparer.Ordinal);
}

public sealed class TemplateCatalog
{
    public string Version { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public TemplateInfo[] Templates { get; set; } = Array.Empty<TemplateInfo>();
}
