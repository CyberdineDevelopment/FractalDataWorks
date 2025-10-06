using FractalDataWorks.Services.Abstractions;

namespace FractalDataWorks.Services.DataGateway.Abstractions;

/// <summary>
/// Service that routes data commands to the appropriate connection.
/// The DataGateway selects the correct connection based on the command's ConnectionName
/// and delegates execution to that connection.
/// </summary>
public interface IDataGateway : IGenericService<IDataGatewayCommand, IDataGatewayConfiguration>, IGenericService
{
}
