using FractalDataWorks.ServiceTypes;
using FractalDataWorks.ServiceTypes.Attributes;
using FractalDataWorks.Services.Abstractions;
using FractalDataWorks.Services.DataGateway.Abstractions;

namespace FractalDataWorks.Services.DataGateway;

/// <summary>
/// Collection of DataGateway service types.
/// </summary>
[ServiceTypeCollection(typeof(DataGatewayType), typeof(IServiceType<,,>), typeof(DataGatewayTypes))]
public partial class DataGatewayTypes : ServiceTypeCollectionBase<DataGatewayType, IServiceType<IDataGateway, IServiceFactory<IDataGateway, IDataGatewayConfiguration>, IDataGatewayConfiguration>, IDataGateway, IDataGatewayConfiguration, IServiceFactory<IDataGateway, IDataGatewayConfiguration>>
{
}
