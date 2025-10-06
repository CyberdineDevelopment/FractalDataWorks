using FractalDataWorks.Services.Abstractions.Commands;

namespace FractalDataWorks.Services.DataGateway.Abstractions;

/// <summary>
/// Base interface for all data gateway commands.
/// Commands specify which connection to use and what operation to perform.
/// </summary>
public interface IDataGatewayCommand : ICommand
{
    /// <summary>
    /// Gets the name of the connection to route this command to.
    /// </summary>
    string ConnectionName { get; }
}
