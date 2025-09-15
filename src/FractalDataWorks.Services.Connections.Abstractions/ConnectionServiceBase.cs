using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FractalDataWorks.Results;
using FractalDataWorks.Services.Abstractions;

namespace FractalDataWorks.Services.Connections.Abstractions;

/// <summary>
/// Abstract base class for all connection service implementations.
/// Provides common connection service functionality following the standard service pattern.
/// </summary>
/// <typeparam name="TCommand">The command type this connection service executes.</typeparam>
/// <typeparam name="TConfiguration">The configuration type for the connection service.</typeparam>
/// <typeparam name="TService">The concrete service type for logging and identification purposes.</typeparam>
/// <remarks>
/// This class provides a foundation for building connection services that integrate
/// with the FractalDataWorks framework's service management and connection abstractions.
/// All connection services should inherit from this class to ensure consistent
/// behavior across different connection types (HTTP, SQL, REST, etc.).
/// </remarks>
public abstract class ConnectionServiceBase<TCommand, TConfiguration, TService> : IFdwService<TCommand, TConfiguration, TService>, IFdwConnection
    where TCommand : IConnectionCommand
    where TConfiguration : class, IConnectionConfiguration
    where TService : class
{
    private readonly string _serviceId;
    private IConnectionState _state;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConnectionServiceBase{TCommand, TConfiguration, TService}"/> class.
    /// </summary>
    /// <param name="configuration">The configuration for this connection service.</param>
    protected ConnectionServiceBase(TConfiguration configuration)
    {
        Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _serviceId = Guid.NewGuid().ToString("N");
        _state = ConnectionStates.Created;
    }

    #region IFractalService Implementation

    /// <inheritdoc/>
    public virtual string Id => _serviceId;

    /// <inheritdoc/>
    public virtual string ServiceType => typeof(TService).Name;

    /// <inheritdoc/>
    public virtual bool IsAvailable => _state == ConnectionStates.Open;

    /// <inheritdoc/>
    public virtual string Name => typeof(TService).Name;

    /// <summary>
    /// Gets the configuration instance for this connection service.
    /// </summary>
    public TConfiguration Configuration { get; }

    /// <inheritdoc/>
    public abstract Task<IFdwResult<T>> Execute<T>(TCommand command);

    /// <inheritdoc/>
    public abstract Task<IFdwResult<TOut>> Execute<TOut>(TCommand command, CancellationToken cancellationToken);

    /// <inheritdoc/>
    public abstract Task<IFdwResult> Execute(TCommand command, CancellationToken cancellationToken);

    /// <inheritdoc/>
    public virtual async Task<IFdwResult> Execute(TCommand command)
    {
        return await Execute(command, CancellationToken.None).ConfigureAwait(false);
    }

    #endregion

    #region IFdwConnection Implementation

    /// <inheritdoc/>
    public string ConnectionId => _serviceId;

    /// <inheritdoc/>
    public virtual string ProviderName => ServiceType;

    /// <inheritdoc/>
    public IConnectionState State => _state;

    /// <inheritdoc/>
    public virtual string ConnectionString => Configuration?.ToString() ?? "Connection";

    /// <inheritdoc/>
    public virtual async Task<IFdwResult> OpenAsync()
    {
        try
        {
            _state = ConnectionStates.Opening;
            var result = await OpenCoreAsync().ConfigureAwait(false);
            if (result.IsSuccess)
            {
                _state = ConnectionStates.Open;
            }
            else
            {
                _state = ConnectionStates.Broken;
            }
            return result;
        }
        catch (Exception ex)
        {
            _state = ConnectionStates.Broken;
            return FdwResult.Failure($"Failed to open connection: {ex.Message}");
        }
    }

    /// <inheritdoc/>
    public virtual async Task<IFdwResult> CloseAsync()
    {
        try
        {
            _state = ConnectionStates.Closing;
            var result = await CloseCoreAsync().ConfigureAwait(false);
            _state = result.IsSuccess ? ConnectionStates.Closed : ConnectionStates.Broken;
            return result;
        }
        catch (Exception ex)
        {
            _state = ConnectionStates.Broken;
            return FdwResult.Failure($"Failed to close connection: {ex.Message}");
        }
    }

    /// <inheritdoc/>
    public virtual async Task<IFdwResult> TestConnectionAsync()
    {
        try
        {
            return await TestConnectionCoreAsync().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            return FdwResult.Failure($"Connection test failed: {ex.Message}");
        }
    }

    /// <inheritdoc/>
    public virtual async Task<IFdwResult<IConnectionMetadata>> GetMetadataAsync()
    {
        try
        {
            return await GetMetadataCoreAsync().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            return FdwResult<IConnectionMetadata>.Failure($"Failed to retrieve metadata: {ex.Message}");
        }
    }

    /// <inheritdoc/>
    public virtual void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    #endregion

    #region Protected Virtual Methods for Override

    /// <summary>
    /// Core implementation of connection opening logic.
    /// </summary>
    /// <returns>A task containing the operation result.</returns>
    protected virtual Task<IFdwResult> OpenCoreAsync()
    {
        return Task.FromResult(FdwResult.Success());
    }

    /// <summary>
    /// Core implementation of connection closing logic.
    /// </summary>
    /// <returns>A task containing the operation result.</returns>
    protected virtual Task<IFdwResult> CloseCoreAsync()
    {
        return Task.FromResult(FdwResult.Success());
    }

    /// <summary>
    /// Core implementation of connection testing logic.
    /// </summary>
    /// <returns>A task containing the test result.</returns>
    protected virtual Task<IFdwResult> TestConnectionCoreAsync()
    {
        return Task.FromResult(FdwResult.Success());
    }

    /// <summary>
    /// Core implementation of metadata retrieval logic.
    /// </summary>
    /// <returns>A task containing the metadata retrieval result.</returns>
    protected virtual Task<IFdwResult<IConnectionMetadata>> GetMetadataCoreAsync()
    {
        var metadata = new BasicConnectionMetadata(ServiceType);
        return Task.FromResult<IFdwResult<IConnectionMetadata>>(FdwResult<IConnectionMetadata>.Success(metadata));
    }

    /// <summary>
    /// Disposes the connection service resources.
    /// </summary>
    /// <param name="disposing">true if disposing managed resources.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (disposing && _state != ConnectionStates.Disposed)
        {
            _state = ConnectionStates.Disposed;
        }
    }

    #endregion

    /// <summary>
    /// Basic connection metadata implementation.
    /// </summary>
    private sealed class BasicConnectionMetadata : IConnectionMetadata
    {
        public BasicConnectionMetadata(string systemName)
        {
            SystemName = systemName;
            CollectedAt = DateTimeOffset.UtcNow;
            Capabilities = new Dictionary<string, object>(StringComparer.Ordinal);
            CustomProperties = new Dictionary<string, object>(StringComparer.Ordinal);
        }

        public string SystemName { get; }
        public string? Version => null;
        public string? ServerInfo => null;
        public string? DatabaseName => null;
        public IReadOnlyDictionary<string, object> Capabilities { get; }
        public DateTimeOffset CollectedAt { get; }
        public IReadOnlyDictionary<string, object> CustomProperties { get; }
    }
}