using FractalDataWorks.ServiceTypes;
using FractalDataWorks.ServiceTypes.Attributes;
using FractalDataWorks.Services.DataGateway.Abstractions;

namespace FractalDataWorks.Services.DataGateway;

/// <summary>
/// ServiceType collection for all data gateway types.
/// The source generator will discover all DataGatewayTypeBase implementations.
/// </summary>
[ServiceTypeCollection(typeof(DataGatewayTypeBase<,,>), typeof(IDataGatewayServiceType), typeof(DataGatewayServiceTypesCollection))]
public abstract partial class DataGatewayServiceTypesCollection
{
    // DO NOT IMPLEMENT BY HAND!
    // Source generator automatically creates static DataGatewayServiceTypes class with:
    // - DataGatewayServiceTypes.SqlServer (returns IServiceType)
    // - DataGatewayServiceTypes.All (collection)
    // - DataGatewayServiceTypes.GetById(int id)
    // - DataGatewayServiceTypes.GetByName(string name)
}