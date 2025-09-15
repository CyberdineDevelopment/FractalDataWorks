using FractalDataWorks.Configuration.Abstractions;
using FractalDataWorks.EnhancedEnums;
using FractalDataWorks.EnhancedEnums.Attributes;
using FractalDataWorks.Services;
using FractalDataWorks.Services.Abstractions;
using FractalDataWorks.Services.SecretManagement.Abstractions;

namespace FractalDataWorks.Services.SecretManagement;

/// <summary>
/// Base class for secret management service type collections.
/// Concrete implementations should define their service types as static readonly fields.
/// </summary>
/// <typeparam name="TSelf">The concrete secret management service type.</typeparam>
/// <typeparam name="TFactory">The factory type for creating service instances.</typeparam>
[StaticEnumCollection(CollectionName = "SecretManagementServiceTypes", DefaultGenericReturnType = typeof(IServiceType))]
public abstract class SecretManagementServiceTypes<TSelf, TFactory> : 
    ServiceTypeCollection<TSelf, ISecretService, ISecretManagementConfiguration, TFactory>
    where TSelf : SecretManagementServiceType<TSelf, ISecretService, ISecretManagementConfiguration, TFactory>, 
                  IEnumOption<TSelf>
    where TFactory : class, IServiceFactory<ISecretService, ISecretManagementConfiguration>
{
    // Concrete implementations will be added by each secret management provider project
    // Example: public static readonly AzureKeyVaultType AzureKeyVault = new();
}