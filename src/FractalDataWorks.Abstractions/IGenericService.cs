namespace FractalDataWorks.Abstractions;

/// <summary>
/// Base interface for all services in the FractalDataWorks framework.
/// </summary>
/// <remarks>
/// This interface is in FractalDataWorks.Abstractions to avoid circular dependencies
/// with source generators, following the same pattern as IGenericCommand.
/// Extended versions with command support are in FractalDataWorks.Services.Abstractions.
/// </remarks>
public interface IGenericService
{
    /// <summary>
    /// Gets the unique identifier for this service instance.
    /// </summary>
    string Id { get; }

    /// <summary>
    /// Gets the display name of the service.
    /// </summary>
    string ServiceType { get; }

    /// <summary>
    /// Gets a value indicating whether the service is currently available for use.
    /// </summary>
    bool IsAvailable { get; }
}
