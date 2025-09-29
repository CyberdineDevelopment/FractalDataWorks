using FractalDataWorks.Services.Abstractions;
using FractalDataWorks.Services.DataGateway.Abstractions.Commands;

namespace FractalDataWorks.Services.DataGateway.Abstractions;

/// <summary>
/// Interface for data gateway services that handle data operations and queries.
/// </summary>
public interface IDataService : IGenericService<IDataCommand, IDataGatewaysConfiguration>
{
    // Data gateway specific methods can be added here if needed
}