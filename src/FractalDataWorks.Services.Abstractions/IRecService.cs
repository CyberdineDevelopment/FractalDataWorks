using System.Threading;
using System.Threading.Tasks;
using FractalDataWorks.Configuration.Abstractions;
using FractalDataWorks.Results;
using FractalDataWorks.Services.Abstractions.Commands;

namespace FractalDataWorks.Services.Abstractions;

/// <summary>
/// Base interface for all services in the FractalDataWorks framework.
/// Provides common service lifecycle management and identification capabilities.
/// </summary>
/// <remarks>
/// All framework services should implement this interface to ensure consistent
/// behavior and integration with the service management infrastructure.
/// The "Rec" prefix avoids namespace collisions with common service interfaces.
/// </remarks>
public interface IFractalService
{
    /// <summary>
    /// Gets the unique identifier for this service instance.
    /// </summary>
    /// <value>A unique identifier for the service instance.</value>
    /// <remarks>
    /// This identifier is used for service tracking, logging, and debugging purposes.
    /// It should remain constant for the lifetime of the service instance.
    /// </remarks>
    string Id { get; }

    /// <summary>
    /// Gets the display name of the service.
    /// </summary>
    /// <value>A human-readable name for the service.</value>
    /// <remarks>
    /// This name is used in user interfaces, logging, and diagnostic outputs.
    /// It should be descriptive and help identify the service's purpose.
    /// </remarks>
    string ServiceType { get; }

    /// <summary>
    /// Gets a value indicating whether the service is currently available for use.
    /// </summary>
    /// <value><c>true</c> if the service is available; otherwise, <c>false</c>.</value>
    /// <remarks>
    /// Services may become unavailable due to configuration issues, network problems,
    /// or temporary failures. The framework can use this property to determine
    /// whether to route requests to this service instance.
    /// </remarks>
    bool IsAvailable { get; }

}

/// <summary>
/// Generic interface for services that provide specific functionality.
/// Extends the base service interface with typed configuration support.
/// </summary>
/// <typeparam name="TCommand">The type of command this service utilizes.</typeparam>
/// <remarks>
/// Use this interface for services that require specific configuration objects
/// to function properly. The configuration should be provided via constructor.
/// </remarks>
public interface IFractalService<TCommand>
    where TCommand : ICommand
{
    /// <summary>
    /// Gets the unique identifier for this service instance.
    /// </summary>
    /// <value>A unique identifier for the service instance.</value>
    /// <remarks>
    /// This identifier is used for service tracking, logging, and debugging purposes.
    /// It should remain constant for the lifetime of the service instance.
    /// </remarks>
    string Id { get; }

    /// <summary>
    /// Gets the display name of the service.
    /// </summary>
    /// <value>A human-readable name for the service.</value>
    /// <remarks>
    /// This name is used in user interfaces, logging, and diagnostic outputs.
    /// It should be descriptive and help identify the service's purpose.
    /// </remarks>
    string ServiceType { get; }

    /// <summary>
    /// Gets a value indicating whether the service is currently available for use.
    /// </summary>
    /// <value><c>true</c> if the service is available; otherwise, <c>false</c>.</value>
    /// <remarks>
    /// Services may become unavailable due to configuration issues, network problems,
    /// or temporary failures. The framework can use this property to determine
    /// whether to route requests to this service instance.
    /// </remarks>
    bool IsAvailable { get; }
    /// <summary>
    /// Executes a command using the service.
    /// </summary>
    /// <param name="command"></param>
    /// <returns></returns>
    Task<IFdwResult> Execute(TCommand command);
}

/// <summary>
/// Generic interface for services with command and configuration support.
/// Extends the command service interface with typed configuration access.
/// </summary>
/// <typeparam name="TCommand">The type of command this service executes.</typeparam>
/// <typeparam name="TConfiguration">The type of configuration this service uses.</typeparam>
/// <remarks>
/// This interface provides services with both command execution capabilities
/// and access to strongly-typed configuration objects. It represents the contract
/// that most service base classes implement.
/// </remarks>
public interface IFractalService<TCommand, TConfiguration> : IFractalService<TCommand>
    where TCommand : ICommand
    where TConfiguration : IFractalConfiguration
{
    /// <summary>
    /// Gets the service name for display purposes.
    /// </summary>
    /// <value>A human-readable name for the service.</value>
    /// <remarks>
    /// This name is used in user interfaces and logging to identify the service.
    /// It's typically the class name or a descriptive identifier.
    /// </remarks>
    string Name { get; }

    /// <summary>
    /// Gets the configuration instance for this service.
    /// </summary>
    /// <value>The strongly-typed configuration object.</value>
    /// <remarks>
    /// Provides access to the service's configuration for validation,
    /// logging, and runtime behavior modification.
    /// </remarks>
    TConfiguration Configuration { get; }

    /// <summary>
    /// Executes a command with generic return type.
    /// </summary>
    /// <typeparam name="T">The expected return type.</typeparam>
    /// <param name="command">The command to execute.</param>
    /// <returns>A task containing the execution result.</returns>
    Task<IFdwResult<T>> Execute<T>(TCommand command);

    /// <summary>
    /// Executes a command with generic return type and cancellation support.
    /// </summary>
    /// <typeparam name="TOut">The expected return type.</typeparam>
    /// <param name="command">The command to execute.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>A task containing the execution result.</returns>
    Task<IFdwResult<TOut>> Execute<TOut>(TCommand command, CancellationToken cancellationToken);

    /// <summary>
    /// Executes a command with cancellation support.
    /// </summary>
    /// <param name="command">The command to execute.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>A task containing the execution result.</returns>
    Task<IFdwResult> Execute(TCommand command, CancellationToken cancellationToken);
}

/// <summary>
/// Generic interface for services with command, configuration, and service type support.
/// Extends the configuration service interface with service type identification.
/// </summary>
/// <typeparam name="TCommand">The type of command this service executes.</typeparam>
/// <typeparam name="TConfiguration">The type of configuration this service uses.</typeparam>
/// <typeparam name="TService">The concrete service type for identification purposes.</typeparam>
/// <remarks>
/// This interface provides the full service contract with command execution,
/// configuration access, and service type identification. It matches the pattern
/// used by ServiceBase and ConnectionServiceBase classes.
/// </remarks>
public interface IFractalService<TCommand, TConfiguration, TService> : IFractalService<TCommand, TConfiguration>
    where TCommand : ICommand
    where TConfiguration : IFractalConfiguration
    where TService : class
{
    // No additional members - the TService is used for type identification and logging
    // The concrete type provides the implementation details
}
