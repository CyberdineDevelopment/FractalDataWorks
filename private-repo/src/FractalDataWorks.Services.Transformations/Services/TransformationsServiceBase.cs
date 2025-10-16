using Microsoft.Extensions.Logging;
using FractalDataWorks.Services;
using FractalDataWorks.Services.Transformations.Abstractions;

namespace FractalDataWorks.Services.Transformations;

/// <summary>
/// Base class for transformations service implementations.
/// Provides common transformations service functionality following the standard service pattern.
/// </summary>
/// <typeparam name="TCommand">The command type this transformations service executes.</typeparam>
/// <typeparam name="TConfiguration">The configuration type for the transformations service.</typeparam>
/// <typeparam name="TService">The concrete service type for logging and identification purposes.</typeparam>
/// <remarks>
/// This class provides a foundation for building transformations services that integrate
/// with the FractalDataWorks framework's service management and transformations abstractions.
/// All transformations services should inherit from this class to ensure consistent
/// behavior across different transformation providers.
/// </remarks>
public abstract class TransformationsServiceBase<TCommand, TConfiguration, TService>
    : ServiceBase<TCommand, TConfiguration, TService>
    where TCommand : ITransformationsCommand
    where TConfiguration : class, ITransformationsConfiguration
    where TService : class
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TransformationsServiceBase{TCommand, TConfiguration, TService}"/> class.
    /// </summary>
    /// <param name="logger">The logger for this transformations service.</param>
    /// <param name="configuration">The configuration for this transformations service.</param>
    protected TransformationsServiceBase(ILogger<TService> logger, TConfiguration configuration)
        : base(logger, configuration)
    {
    }
}