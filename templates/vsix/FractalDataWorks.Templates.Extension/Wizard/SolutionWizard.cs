using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using EnvDTE;
using Microsoft.VisualStudio.TemplateWizard;

namespace FractalDataWorks.Templates.Extension.Wizard;

public sealed class SolutionWizard : IWizard
{
    private WizardDialog _dialog;
    private DTE _dte;
    private Dictionary<string, string> _replacementsDictionary;
    private WizardRunKind _runKind;
    private object[] _customParams;

    public void RunStarted(object automationObject, Dictionary<string, string> replacementsDictionary, 
        WizardRunKind runKind, object[] customParams)
    {
        try
        {
            _dte = automationObject as DTE;
            _replacementsDictionary = replacementsDictionary ?? new Dictionary<string, string>(StringComparer.Ordinal);
            _runKind = runKind;
            _customParams = customParams;

            _dialog = new WizardDialog();
            
            if (_dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                ApplyWizardSettings();
            }
            else
            {
                throw new WizardCancelledException("User cancelled the wizard");
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error initializing solution wizard: {ex.Message}", 
                "FractalDataWorks Template Wizard", MessageBoxButtons.OK, MessageBoxIcon.Error);
            throw;
        }
    }

    public void ProjectFinishedGenerating(Project project)
    {
        // Project-specific post-processing can be added here
        if (project != null)
        {
            ConfigureProject(project);
        }
    }

    public void ProjectItemFinishedGenerating(ProjectItem projectItem)
    {
        // Item-specific post-processing can be added here
    }

    public bool ShouldAddProjectItem(string filePath) => true;

    public void BeforeOpeningFile(ProjectItem projectItem)
    {
        // Pre-file opening logic can be added here
    }

    public void RunFinished()
    {
        try
        {
            _dialog?.Dispose();
            
            // Perform final solution-level configuration
            if (_dte?.Solution != null)
            {
                ConfigureSolution();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Warning: Some post-generation tasks failed: {ex.Message}", 
                "FractalDataWorks Template Wizard", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private void ApplyWizardSettings()
    {
        if (_dialog?.Configuration == null) return;

        var config = _dialog.Configuration;

        // Apply basic settings
        _replacementsDictionary["$solutiontype$"] = config.SolutionType;
        _replacementsDictionary["$includeapi$"] = config.IncludeApiHost.ToString().ToLowerInvariant();
        _replacementsDictionary["$includeetl$"] = config.IncludeEtlHost.ToString().ToLowerInvariant();
        _replacementsDictionary["$includescheduler$"] = config.IncludeSchedulerHost.ToString().ToLowerInvariant();
        _replacementsDictionary["$includetests$"] = config.IncludeTests.ToString().ToLowerInvariant();
        _replacementsDictionary["$includedocker$"] = config.IncludeDocker.ToString().ToLowerInvariant();

        // Authentication settings
        _replacementsDictionary["$authtype$"] = config.AuthenticationType;
        _replacementsDictionary["$azureadtenantid$"] = config.AzureAdTenantId ?? string.Empty;
        _replacementsDictionary["$azureadclientid$"] = config.AzureAdClientId ?? string.Empty;

        // Database settings
        _replacementsDictionary["$databaseprovider$"] = config.DatabaseProvider;
        _replacementsDictionary["$connectionstring$"] = config.ConnectionString ?? string.Empty;

        // CI/CD settings
        _replacementsDictionary["$includecicd$"] = config.IncludeCiCd.ToString().ToLowerInvariant();
        _replacementsDictionary["$cicdprovider$"] = config.CiCdProvider;

        // Security settings
        _replacementsDictionary["$secretmanagement$"] = config.SecretManagement;
        _replacementsDictionary["$keyvaultname$"] = config.KeyVaultName ?? string.Empty;
    }

    private void ConfigureProject(Project project)
    {
        try
        {
            // Add NuGet packages based on configuration
            var config = _dialog?.Configuration;
            if (config == null) return;

            var packageManager = GetPackageManager(project);
            if (packageManager == null) return;

            // Install core FractalDataWorks packages
            InstallCorePackages(packageManager, config);

            // Install provider-specific packages
            InstallProviderPackages(packageManager, config);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Warning: Failed to configure project packages: {ex.Message}", 
                "FractalDataWorks Template Wizard", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private void ConfigureSolution()
    {
        try
        {
            // Add solution-level configurations
            var config = _dialog?.Configuration;
            if (config == null) return;

            // Create solution folders if needed
            CreateSolutionFolders(config);

            // Configure solution properties
            ConfigureSolutionProperties(config);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Warning: Failed to configure solution: {ex.Message}", 
                "FractalDataWorks Template Wizard", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private object GetPackageManager(Project project)
    {
        // This would integrate with NuGet package manager
        // Implementation depends on VS SDK version
        return null; // Placeholder for package manager integration
    }

    private void InstallCorePackages(object packageManager, WizardConfiguration config)
    {
        var corePackages = new[]
        {
            "FractalDataWorks.Configuration",
            "FractalDataWorks.DependencyInjection",
            "FractalDataWorks.Results",
            "FractalDataWorks.Messages"
        };

        foreach (var package in corePackages)
        {
            // Install package logic would go here
            System.Diagnostics.Debug.WriteLine($"Would install package: {package}");
        }
    }

    private void InstallProviderPackages(object packageManager, WizardConfiguration config)
    {
        var packages = new List<string>();

        // Authentication packages
        if (config.AuthenticationType == "AzureAD")
        {
            packages.Add("FractalDataWorks.Services.Authentication.AzureEntra");
        }

        // Database packages
        if (config.DatabaseProvider == "SqlServer")
        {
            packages.Add("FractalDataWorks.Services.Connections.MsSql");
        }

        // Secret management packages
        if (config.SecretManagement == "AzureKeyVault")
        {
            packages.Add("FractalDataWorks.Services.SecretManagement.AzureKeyVault");
        }

        foreach (var package in packages)
        {
            // Install package logic would go here
            System.Diagnostics.Debug.WriteLine($"Would install package: {package}");
        }
    }

    private void CreateSolutionFolders(WizardConfiguration config)
    {
        if (_dte?.Solution?.Projects == null) return;

        var folders = new List<string> { "src", "tests" };

        if (config.IncludeDocker)
        {
            folders.Add("docker");
        }

        if (config.IncludeCiCd)
        {
            folders.Add("build");
        }

        foreach (var folder in folders)
        {
            try
            {
                _dte.Solution.AddSolutionFolder(folder);
            }
            catch
            {
                // Folder might already exist
            }
        }
    }

    private void ConfigureSolutionProperties(WizardConfiguration config)
    {
        // Configure solution-wide properties based on wizard settings
        // This could include setting up solution-wide build configurations,
        // solution filters, etc.
    }
}

public sealed class WizardCancelledException : Exception
{
    public WizardCancelledException(string message) : base(message) { }
    public WizardCancelledException(string message, Exception innerException) : base(message, innerException) { }
}
