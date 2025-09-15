using FractalDataWorks.EnhancedEnums.Attributes;
using FractalDataWorks.Services;
using FractalDataWorks.Services.Abstractions;
using FractalDataWorks.Services.DataGateway.Abstractions;

namespace FractalDataWorks.Services.DataGateway;

/// <summary>
/// Concrete collection of data provider service types available in the system.
/// </summary>
[StaticEnumCollection(CollectionName = "DataGatewayServiceTypes", DefaultGenericReturnType = typeof(IServiceType))]
public sealed class DataGatewayServiceTypesCollection : 
    DataGatewayServiceTypes<SqlDataGatewayServiceType, IServiceFactory<IDataService, IDataGatewaysConfiguration>>
{
    // DO NOT IMPLEMENT BY HAND!
    // Source generator automatically creates static DataGatewayServiceTypes class with:
    // - DataGatewayServiceTypes.SqlServer (returns IServiceType)
    // - DataGatewayServiceTypes.All (collection)
    // - DataGatewayServiceTypes.GetById(int id)
    // - DataGatewayServiceTypes.GetByName(string name)
}