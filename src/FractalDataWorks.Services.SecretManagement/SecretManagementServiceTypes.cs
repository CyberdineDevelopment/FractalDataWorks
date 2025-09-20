using FractalDataWorks.Configuration.Abstractions;
using FractalDataWorks.EnhancedEnums;
using FractalDataWorks.ServiceTypes;
using FractalDataWorks.ServiceTypes.Attributes;
using FractalDataWorks.Services;
using FractalDataWorks.Services.Abstractions;
using FractalDataWorks.Services.SecretManagement.Abstractions;

namespace FractalDataWorks.Services.SecretManagement;

/// <summary>
/// Collection of secret management service types.
/// The source generator will discover all SecretManagementServiceType implementations.
/// </summary>
[ServiceTypeCollection("ISecretManagementServiceType", "SecretManagementServiceTypes")]
public static partial class SecretManagementServiceTypes
{
    // Concrete implementations will be added by each secret management provider project
    // Example: public static readonly AzureKeyVaultType AzureKeyVault = new();
}