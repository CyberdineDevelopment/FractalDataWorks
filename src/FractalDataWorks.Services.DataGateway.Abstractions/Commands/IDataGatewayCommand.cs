using FractalDataWorks.Services.Abstractions.Commands;

namespace FractalDataWorks.Services.DataGateway.Abstractions;

/// <summary>
/// Represents a command that can be executed by data gateway services.
/// </summary>
/// <remarks>
/// Data gateway commands encapsulate operations that interact with external data sources
/// through the data gateway abstraction layer. These commands provide a standardized
/// interface for data operations across different data gateway implementations.
/// </remarks>
public interface IDataGatewayCommand : ICommand
{
    // Marker interface for data gateway commands
    // Additional properties specific to data gateway operations can be added here
}