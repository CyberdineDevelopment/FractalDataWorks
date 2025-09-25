using FractalDataWorks.EnhancedEnums.Attributes;
using FractalDataWorks.Services.Abstractions;
using FractalDataWorks.Services.SecretManagement.Abstractions;
using FractalDataWorks.Services.SecretManagement;
using FractalDataWorks.Services.SecretManagement.AzureKeyVault.EnhancedEnums;
using FractalDataWorks.ServiceTypes;

namespace FractalDataWorks.Services.SecretManagement.AzureKeyVault;

/// <summary>
/// Concrete collection of secret management service types available in the system.
/// </summary>
[StaticEnumCollection(CollectionName = "SecretManagementServiceTypes", DefaultGenericReturnType = typeof(IServiceType))]
public sealed class SecretManagementServiceTypesCollection : 
    SecretManagementServiceTypes<AzureKeyVaultType, IServiceFactory<ISecretService, ISecretManagementConfiguration>>
{
    // DO NOT IMPLEMENT BY HAND!
    // Source generator automatically creates static SecretManagementServiceTypes class with:
    // - SecretManagementServiceTypes.AzureKeyVault (returns IServiceType)
    // - SecretManagementServiceTypes.All (collection)
    // - SecretManagementServiceTypes.GetById(int id)
    // - SecretManagementServiceTypes.GetByName(string name)
}