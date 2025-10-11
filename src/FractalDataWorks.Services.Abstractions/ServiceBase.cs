using System;
using System.Threading;
using System.Threading.Tasks;
using FractalDataWorks.Commands.Abstractions;
using FractalDataWorks.Configuration.Abstractions;
using FractalDataWorks.Results;
using FractalDataWorks.Services.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace FractalDataWorks.Services;

/// <summary>
/// Base class for all services in the FractalDataWorks framework.
/// Provides common implementation for service lifecycle management.
/// </summary>
/// <typeparam name="TCommand">The type of command this service executes.</typeparam>
/// <typeparam name="TConfiguration">The type of configuration this service uses.</typeparam>
/// <typeparam name="TService">The concrete service type for identification purposes.</typeparam>
public abstract class ServiceBase<TCommand, TConfiguration, TService> : IGenericService<TCommand, TConfiguration, TService>
    where TCommand : ICommand
    where TConfiguration : IGenericConfiguration
    where TService : class
{
    private readonly ILogger<TService> _logger;

    /// <summary>
    /// Gets the unique identifier for this service instance.
    /// </summary>
    public string Id { get; }

    /// <summary>
    /// Gets the display name of the service.
    /// </summary>
    public string ServiceType { get; }

    /// <summary>
    /// Gets a value indicating whether the service is currently available for use.
    /// </summary>
    public virtual bool IsAvailable { get; protected set; } = true;

    /// <summary>
    /// Gets the service name for display purposes.
    /// </summary>
    public string Name => Configuration?.Name ?? typeof(TService).Name;

    /// <summary>
    /// Gets the configuration instance for this service.
    /// </summary>
    public TConfiguration Configuration { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ServiceBase{TCommand, TConfiguration, TService}"/> class.
    /// </summary>
    /// <param name="logger">The logger for this service.</param>
    /// <param name="configuration">The configuration for this service.</param>
    protected ServiceBase(ILogger<TService> logger, TConfiguration configuration)
    {
        _logger = logger ?? NullLogger<TService>.Instance;
        Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        Id = Guid.NewGuid().ToString();
        ServiceType = typeof(TService).Name;
    }

    /// <summary>
    /// Executes a command using the service.
    /// </summary>
    /// <param name="command">The command to execute.</param>
    /// <returns>A task containing the execution result.</returns>
    public abstract Task<IGenericResult> Execute(TCommand command);

    /// <summary>
    /// Executes a command with generic return type.
    /// </summary>
    /// <typeparam name="TOut">The expected return type.</typeparam>
    /// <param name="command">The command to execute.</param>
    /// <returns>A task containing the execution result.</returns>
    public abstract Task<IGenericResult<TOut>> Execute<TOut>(TCommand command);

    /// <summary>
    /// Executes a command with generic return type and cancellation support.
    /// </summary>
    /// <typeparam name="TOut">The expected return type.</typeparam>
    /// <param name="command">The command to execute.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>A task containing the execution result.</returns>
    public virtual Task<IGenericResult<TOut>> Execute<TOut>(TCommand command, CancellationToken cancellationToken)
    {
        return Execute<TOut>(command);
    }

    /// <summary>
    /// Executes a command with cancellation support.
    /// </summary>
    /// <param name="command">The command to execute.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>A task containing the execution result.</returns>
    public virtual Task<IGenericResult> Execute(TCommand command, CancellationToken cancellationToken)
    {
        return Execute(command);
    }

    /// <summary>
    /// Gets the logger for derived classes to use.
    /// </summary>
    protected ILogger<TService> Logger => _logger;
}