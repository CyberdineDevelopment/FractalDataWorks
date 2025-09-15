using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using FractalDataWorks.Results;
using FractalDataWorks.Services.Connections.Abstractions;
using Microsoft.Extensions.Logging;
using FractalDataWorks.Services.Connections.Http.Abstractions.Logging;

namespace FractalDataWorks.Services.Connections.Http.Abstractions;

/// <summary>
/// Abstract base class for HTTP external connection service implementations.
/// Concrete implementations should inherit from this class and provide specific HTTP functionality.
/// </summary>
public abstract partial class HttpServiceBase<TConfiguration> : ConnectionServiceBase<IConnectionCommand, TConfiguration, HttpServiceBase<TConfiguration>>
    where TConfiguration : class, IConnectionConfiguration
{
    private readonly ILogger<HttpServiceBase<TConfiguration>> _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly Dictionary<string, IFdwConnection> _connections;

    /// <summary>
    /// Initializes a new instance of the <see cref="HttpServiceBase{TConfiguration}"/> class.
    /// </summary>
    /// <param name="logger">The logger instance for the concrete service type.</param>
    /// <param name="httpClientFactory">The HTTP client factory for creating managed HTTP clients.</param>
    /// <param name="configuration">The HTTP service configuration.</param>
    protected HttpServiceBase(
        ILogger<HttpServiceBase<TConfiguration>>? logger,
        IHttpClientFactory httpClientFactory,
        TConfiguration configuration)
        : base(configuration)
    {
        _logger = logger ?? Microsoft.Extensions.Logging.Abstractions.NullLogger<HttpServiceBase<TConfiguration>>.Instance;
        _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        _connections = new Dictionary<string, IFdwConnection>(StringComparer.Ordinal);
    }

    /// <inheritdoc/>
    public override string ServiceType => "HTTP Connection Service";

    /// <inheritdoc/>
    public override async Task<IFdwResult<T>> Execute<T>(IConnectionCommand command)
    {
        return await Execute<T>(command, CancellationToken.None).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public abstract override Task<IFdwResult<TOut>> Execute<TOut>(IConnectionCommand command, CancellationToken cancellationToken);

    /// <inheritdoc/>
    public override async Task<IFdwResult> Execute(IConnectionCommand command, CancellationToken cancellationToken)
    {
        HttpServiceBaseLog.ExecutingCommand(_logger, command.GetType().Name);
        
        try
        {
            var result = await ExecuteCore(command, cancellationToken).ConfigureAwait(false);
            
            if (result.IsSuccess)
            {
                HttpServiceBaseLog.CommandCompleted(_logger, command.GetType().Name);
            }
            else
            {
                HttpServiceBaseLog.CommandFailed(_logger, command.GetType().Name, result.Message ?? "");
            }
            
            return result;
        }
        catch (Exception ex)
        {
            HttpServiceBaseLog.CommandException(_logger, command.GetType().Name, ex);
            return FdwResult.Failure($"Command execution failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Core implementation for executing commands.
    /// Derived classes should override this method to provide specific HTTP functionality.
    /// </summary>
    /// <param name="command">The command to execute.</param>
    /// <param name="cancellationToken">The cancellation token for the operation.</param>
    /// <returns>A task containing the execution result.</returns>
    protected abstract Task<IFdwResult> ExecuteCore(IConnectionCommand command, CancellationToken cancellationToken);

    /// <summary>
    /// Gets the HTTP client factory for creating managed HTTP clients.
    /// </summary>
    protected IHttpClientFactory HttpClientFactory => _httpClientFactory;

    /// <summary>
    /// Gets the logger instance for this service.
    /// </summary>
    protected ILogger<HttpServiceBase<TConfiguration>> Logger => _logger;

    /// <summary>
    /// Gets the connections dictionary.
    /// </summary>
    protected IReadOnlyDictionary<string, IFdwConnection> Connections => _connections;

    #region IFdwConnection Implementation

    /// <inheritdoc/>
    public string ConnectionId => Id;

    /// <inheritdoc/>
    public virtual string ProviderName => ServiceType;

    /// <inheritdoc/>
    public FdwConnectionState State => IsAvailable ? FdwConnectionState.Open : FdwConnectionState.Closed;

    /// <inheritdoc/>
    public virtual string ConnectionString => Configuration?.ToString() ?? "HTTP Connection";

    /// <inheritdoc/>
    public virtual async Task<IFdwResult> OpenAsync()
    {
        try
        {
            HttpServiceBaseLog.OpeningConnection(_logger);
            // HTTP connections are typically stateless, so just validate configuration
            var result = ValidateConfiguration();
            if (result.IsSuccess)
            {
                HttpServiceBaseLog.ConnectionOpened(_logger);
            }
            return result;
        }
        catch (Exception ex)
        {
            HttpServiceBaseLog.ConnectionFailed(_logger, ServiceType, ex);
            return FdwResult.Failure($"Failed to open HTTP connection: {ex.Message}");
        }
    }

    /// <inheritdoc/>
    public virtual async Task<IFdwResult> CloseAsync()
    {
        try
        {
            HttpServiceBaseLog.ClosingConnection(_logger);
            // HTTP connections are typically stateless, so just clean up any cached connections
            _connections.Clear();
            HttpServiceBaseLog.ConnectionClosed(_logger);
            return FdwResult.Success();
        }
        catch (Exception ex)
        {
            HttpServiceBaseLog.ConnectionFailed(_logger, ServiceType, ex);
            return FdwResult.Failure($"Failed to close HTTP connection: {ex.Message}");
        }
    }

    /// <inheritdoc/>
    public virtual async Task<IFdwResult> TestConnectionAsync()
    {
        try
        {
            HttpServiceBaseLog.TestingConnection(_logger);
            var result = await TestConnectionCore().ConfigureAwait(false);
            return result;
        }
        catch (Exception ex)
        {
            HttpServiceBaseLog.ConnectionFailed(_logger, ServiceType, ex);
            return FdwResult.Failure($"Connection test failed: {ex.Message}");
        }
    }

    /// <inheritdoc/>
    public virtual async Task<IFdwResult<IConnectionMetadata>> GetMetadataAsync()
    {
        try
        {
            var metadata = new HttpConnectionMetadata(ServiceType);
            return FdwResult<IConnectionMetadata>.Success(metadata);
        }
        catch (Exception ex)
        {
            HttpServiceBaseLog.ConnectionFailed(_logger, ServiceType, ex);
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

    /// <summary>
    /// Validates the current configuration.
    /// </summary>
    /// <returns>A result indicating whether the configuration is valid.</returns>
    protected virtual IFdwResult ValidateConfiguration()
    {
        if (Configuration == null)
        {
            return FdwResult.Failure("Configuration cannot be null");
        }
        
        return FdwResult.Success();
    }

    /// <summary>
    /// Tests the connection by performing a basic connectivity check.
    /// Derived classes can override this to provide more specific tests.
    /// </summary>
    /// <returns>A result indicating whether the connection test passed.</returns>
    protected virtual Task<IFdwResult> TestConnectionCore()
    {
        // Default implementation just validates configuration
        return Task.FromResult(ValidateConfiguration());
    }

    /// <summary>
    /// Disposes resources used by this HTTP service.
    /// </summary>
    /// <param name="disposing">True if disposing managed resources.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            _connections.Clear();
        }
    }

    /// <summary>
    /// Basic HTTP connection metadata implementation.
    /// </summary>
    private sealed class HttpConnectionMetadata : IConnectionMetadata
    {
        public HttpConnectionMetadata(string systemName)
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