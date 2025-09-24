using FractalDataWorks.ServiceTypes;
using FractalDataWorks.ServiceTypes.Attributes;

namespace FractalDataWorks.Services.SecretManagement.Abstractions;

/// <summary>
/// Concrete collection of all secret management service types in the system.
/// This partial class will be extended by the source generator to include
/// all discovered secret management types with high-performance lookup capabilities.
/// </summary>
[ServiceTypeCollection(typeof(SecretManagementTypeBase<,,>), typeof(ISecretManagementType), typeof(SecretManagementTypesBase))]
public partial class SecretManagementTypesBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SecretManagementTypesBase"/> class.
    /// The source generator will populate all discovered secret management types.
    /// </summary>
    public SecretManagementTypesBase()
    {
        // Source generator will add:
        // - Static fields for each secret management type (e.g., AzureKeyVault, HashiCorpVault, etc.)
        // - FrozenDictionary for O(1) lookups by Id/Name
        // - Factory methods for each constructor overload
        // - Empty() method returning default instance
        // - All() method returning all secret management types
        // - Lookup methods by SupportedSecretStores, capabilities, etc.
    }
}