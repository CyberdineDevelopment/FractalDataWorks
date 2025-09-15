using FractalDataWorks.ServiceTypes;
using FractalDataWorks.ServiceTypes.Attributes;

namespace FractalDataWorks.Services.DataGateway.Abstractions;

/// <summary>
/// Concrete collection of all data provider service types in the system.
/// This partial class will be extended by the source generator to include
/// all discovered data provider types with high-performance lookup capabilities.
/// </summary>
[ServiceTypeCollection("DataGatewayTypeBase", "DataGatewayTypes")]
public partial class DataGatewayTypesBase : 
    DataGatewayTypeCollectionBase<
        DataGatewayTypeBase<IDataService, IDataGatewaysConfiguration, IServiceFactory<IDataService, IDataGatewaysConfiguration>>,
        DataGatewayTypeBase<IDataService, IDataGatewaysConfiguration, IServiceFactory<IDataService, IDataGatewaysConfiguration>>,
        IDataService,
        IDataGatewaysConfiguration,
        IServiceFactory<IDataService, IDataGatewaysConfiguration>>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DataGatewayTypesBase"/> class.
    /// The source generator will populate all discovered data provider types.
    /// </summary>
    public DataGatewayTypesBase()
    {
        // Source generator will add:
        // - Static fields for each data provider type (e.g., EntityFramework, Dapper, MongoDB, etc.)
        // - FrozenDictionary for O(1) lookups by Id/Name
        // - Factory methods for each constructor overload
        // - Empty() method returning default instance
        // - All() method returning all data provider types
        // - Lookup methods by ProviderName, SupportedDataStores, capabilities, etc.
    }
}