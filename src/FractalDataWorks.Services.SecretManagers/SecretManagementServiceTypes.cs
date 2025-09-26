using FractalDataWorks.Configuration.Abstractions;
using FractalDataWorks.EnhancedEnums;
using FractalDataWorks.ServiceTypes;
using FractalDataWorks.ServiceTypes.Attributes;
using FractalDataWorks.Services;
using FractalDataWorks.Services.Abstractions;
using FractalDataWorks.Services.SecretManagers.Abstractions;

namespace FractalDataWorks.Services.SecretManager;

/// <summary>
/// Collection of secret management service types.
/// The source generator will discover all SecretManagerServiceType implementations.
/// </summary>
[ServiceTypeCollection("ISecretManagerServiceType", "SecretManagerServiceTypes")]
public static partial class SecretManagerServiceTypes
{
    // Concrete implementations will be added by each secret management provider project
    // Example: public static readonly AzureKeyVaultType AzureKeyVault = new();
}