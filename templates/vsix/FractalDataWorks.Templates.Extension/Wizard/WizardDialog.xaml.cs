using System;
using System.Windows;
using System.Windows.Controls;

namespace FractalDataWorks.Templates.Extension.Wizard;

public partial class WizardDialog : Window
{
    public WizardConfiguration Configuration { get; private set; }

    public WizardDialog()
    {
        InitializeComponent();
        InitializeEventHandlers();
        Configuration = new WizardConfiguration();
    }

    private void InitializeEventHandlers()
    {
        // Solution type radio button events
        ApiSolutionRadio.Checked += SolutionTypeRadio_Checked;
        EtlSolutionRadio.Checked += SolutionTypeRadio_Checked;
        SchedulerSolutionRadio.Checked += SolutionTypeRadio_Checked;
        FullStackSolutionRadio.Checked += SolutionTypeRadio_Checked;

        // Authentication type combo box event
        AuthTypeCombo.SelectionChanged += AuthTypeCombo_SelectionChanged;

        // Database provider combo box event
        DatabaseProviderCombo.SelectionChanged += DatabaseProviderCombo_SelectionChanged;

        // Secret management combo box event
        SecretManagementCombo.SelectionChanged += SecretManagementCombo_SelectionChanged;

        // CI/CD checkbox event
        IncludeCiCdCheckBox.Checked += IncludeCiCdCheckBox_CheckedChanged;
        IncludeCiCdCheckBox.Unchecked += IncludeCiCdCheckBox_CheckedChanged;
    }

    private void SolutionTypeRadio_Checked(object sender, RoutedEventArgs e)
    {
        if (!(sender is RadioButton radioButton)) return;

        // Update checkboxes based on solution type
        switch (radioButton.Name)
        {
            case nameof(ApiSolutionRadio):
                IncludeApiCheckBox.IsChecked = true;
                IncludeEtlCheckBox.IsChecked = false;
                IncludeSchedulerCheckBox.IsChecked = false;
                break;
            case nameof(EtlSolutionRadio):
                IncludeApiCheckBox.IsChecked = false;
                IncludeEtlCheckBox.IsChecked = true;
                IncludeSchedulerCheckBox.IsChecked = false;
                break;
            case nameof(SchedulerSolutionRadio):
                IncludeApiCheckBox.IsChecked = false;
                IncludeEtlCheckBox.IsChecked = false;
                IncludeSchedulerCheckBox.IsChecked = true;
                break;
            case nameof(FullStackSolutionRadio):
                IncludeApiCheckBox.IsChecked = true;
                IncludeEtlCheckBox.IsChecked = true;
                IncludeSchedulerCheckBox.IsChecked = true;
                break;
        }
    }

    private void AuthTypeCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!(sender is ComboBox comboBox)) return;

        var selectedItem = comboBox.SelectedItem as ComboBoxItem;
        var authType = selectedItem?.Tag?.ToString();

        // Show/hide configuration panels based on authentication type
        AzureAdPanel.Visibility = authType == "AzureAD" ? Visibility.Visible : Visibility.Collapsed;
        JwtPanel.Visibility = authType == "JWT" ? Visibility.Visible : Visibility.Collapsed;
    }

    private void DatabaseProviderCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!(sender is ComboBox comboBox)) return;

        var selectedItem = comboBox.SelectedItem as ComboBoxItem;
        var provider = selectedItem?.Tag?.ToString();

        // Show/hide database configuration panel
        DatabaseConfigPanel.Visibility = provider != "None" ? Visibility.Visible : Visibility.Collapsed;

        // Update connection string placeholder based on provider
        if (provider != "None" && ConnectionStringText != null)
        {
            ConnectionStringText.Text = GetConnectionStringTemplate(provider);
        }
    }

    private void SecretManagementCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!(sender is ComboBox comboBox)) return;

        var selectedItem = comboBox.SelectedItem as ComboBoxItem;
        var secretType = selectedItem?.Tag?.ToString();

        // Show/hide Key Vault configuration panel
        KeyVaultPanel.Visibility = secretType == "AzureKeyVault" ? Visibility.Visible : Visibility.Collapsed;
    }

    private void IncludeCiCdCheckBox_CheckedChanged(object sender, RoutedEventArgs e)
    {
        if (!(sender is CheckBox checkBox)) return;

        // Show/hide CI/CD configuration panel
        CiCdConfigPanel.Visibility = checkBox.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
    }

    private void CreateButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            // Validate inputs
            if (!ValidateInputs())
            {
                return;
            }

            // Populate configuration
            PopulateConfiguration();

            DialogResult = true;
            Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error creating configuration: {ex.Message}", 
                "Configuration Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }

    private bool ValidateInputs()
    {
        // Validate that at least one project type is selected
        if (IncludeApiCheckBox.IsChecked != true && 
            IncludeEtlCheckBox.IsChecked != true && 
            IncludeSchedulerCheckBox.IsChecked != true)
        {
            MessageBox.Show("Please select at least one project type to include.", 
                "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            return false;
        }

        // Validate Azure AD configuration if selected
        var selectedAuthItem = AuthTypeCombo.SelectedItem as ComboBoxItem;
        if (selectedAuthItem?.Tag?.ToString() == "AzureAD")
        {
            if (string.IsNullOrWhiteSpace(AzureTenantIdText.Text) || 
                string.IsNullOrWhiteSpace(AzureClientIdText.Text))
            {
                MessageBox.Show("Azure AD Tenant ID and Client ID are required when using Azure AD authentication.", 
                    "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
        }

        // Validate JWT configuration if selected
        if (selectedAuthItem?.Tag?.ToString() == "JWT")
        {
            if (string.IsNullOrWhiteSpace(JwtIssuerText.Text) || 
                string.IsNullOrWhiteSpace(JwtAudienceText.Text))
            {
                MessageBox.Show("JWT Issuer and Audience are required when using JWT authentication.", 
                    "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
        }

        // Validate Key Vault configuration if selected
        var selectedSecretItem = SecretManagementCombo.SelectedItem as ComboBoxItem;
        if (selectedSecretItem?.Tag?.ToString() == "AzureKeyVault")
        {
            if (string.IsNullOrWhiteSpace(KeyVaultNameText.Text))
            {
                MessageBox.Show("Key Vault name is required when using Azure Key Vault.", 
                    "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
        }

        return true;
    }

    private void PopulateConfiguration()
    {
        Configuration = new WizardConfiguration
        {
            // Solution type
            SolutionType = GetSelectedSolutionType(),
            
            // Project inclusion
            IncludeApiHost = IncludeApiCheckBox.IsChecked == true,
            IncludeEtlHost = IncludeEtlCheckBox.IsChecked == true,
            IncludeSchedulerHost = IncludeSchedulerCheckBox.IsChecked == true,
            IncludeTests = IncludeTestsCheckBox.IsChecked == true,
            IncludeDocker = IncludeDockerCheckBox.IsChecked == true,
            
            // Authentication
            AuthenticationType = (AuthTypeCombo.SelectedItem as ComboBoxItem)?.Tag?.ToString() ?? "None",
            AzureAdTenantId = AzureTenantIdText.Text,
            AzureAdClientId = AzureClientIdText.Text,
            JwtIssuer = JwtIssuerText.Text,
            JwtAudience = JwtAudienceText.Text,
            
            // Database
            DatabaseProvider = (DatabaseProviderCombo.SelectedItem as ComboBoxItem)?.Tag?.ToString() ?? "None",
            ConnectionString = ConnectionStringText.Text,
            UseEntityFramework = UseEntityFrameworkCheckBox.IsChecked == true,
            IncludeMigrations = IncludeMigrationsCheckBox.IsChecked == true,
            
            // Security
            SecretManagement = (SecretManagementCombo.SelectedItem as ComboBoxItem)?.Tag?.ToString() ?? "Configuration",
            KeyVaultName = KeyVaultNameText.Text,
            IncludeRateLimiting = IncludeRateLimitingCheckBox.IsChecked == true,
            IncludeCors = IncludeCorsCheckBox.IsChecked == true,
            IncludeHealthChecks = IncludeHealthChecksCheckBox.IsChecked == true,
            
            // CI/CD
            IncludeCiCd = IncludeCiCdCheckBox.IsChecked == true,
            CiCdProvider = (CiCdProviderCombo.SelectedItem as ComboBoxItem)?.Tag?.ToString() ?? "GitHubActions",
            IncludeDockerBuild = IncludeDockerBuildCheckBox.IsChecked == true,
            IncludeTestsInPipeline = IncludeTestsInPipelineCheckBox.IsChecked == true,
            IncludeCodeCoverage = IncludeCodeCoverageCheckBox.IsChecked == true
        };
    }

    private string GetSelectedSolutionType()
    {
        if (ApiSolutionRadio.IsChecked == true) return "API";
        if (EtlSolutionRadio.IsChecked == true) return "ETL";
        if (SchedulerSolutionRadio.IsChecked == true) return "Scheduler";
        if (FullStackSolutionRadio.IsChecked == true) return "FullStack";
        return "FullStack";
    }

    private static string GetConnectionStringTemplate(string provider)
    {
        return provider switch
        {
            "SqlServer" => "Server=localhost;Database=MyDatabase;Trusted_Connection=true;",
            "PostgreSQL" => "Host=localhost;Database=mydatabase;Username=myuser;Password=mypass;",
            "MySQL" => "Server=localhost;Database=mydatabase;Uid=myuser;Pwd=mypass;",
            "SQLite" => "Data Source=database.db;",
            "CosmosDB" => "AccountEndpoint=https://localhost:8081/;AccountKey=C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==;",
            _ => string.Empty
        };
    }
}
