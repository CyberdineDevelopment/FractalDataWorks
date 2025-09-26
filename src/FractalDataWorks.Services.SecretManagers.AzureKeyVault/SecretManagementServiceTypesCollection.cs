using FractalDataWorks.EnhancedEnums.Attributes;
using FractalDataWorks.Services.Abstractions;
using FractalDataWorks.Services.SecretManagers.Abstractions;
using FractalDataWorks.Services.SecretManager;
using FractalDataWorks.Services.SecretManagers.AzureKeyVault.EnhancedEnums;
using FractalDataWorks.ServiceTypes;

namespace FractalDataWorks.Services.SecretManagers.AzureKeyVault;

/// <summary>
/// Concrete collection of secret management service types available in the system.
/// </summary>
[StaticEnumCollection(CollectionName = "SecretManagerServiceTypes", DefaultGenericReturnType = typeof(IServiceType))]
public sealed class SecretManagerServiceTypesCollection : 
    SecretManagerServiceTypes<AzureKeyVaultType, IServiceFactory<ISecretService, ISecretManagerConfiguration>>
{
    // DO NOT IMPLEMENT BY HAND!
    // Source generator automatically creates static SecretManagerServiceTypes class with:
    // - SecretManagerServiceTypes.AzureKeyVault (returns IServiceType)
    // - SecretManagerServiceTypes.All (collection)
    // - SecretManagerServiceTypes.GetById(int id)
    // - SecretManagerServiceTypes.GetByName(string name)
}