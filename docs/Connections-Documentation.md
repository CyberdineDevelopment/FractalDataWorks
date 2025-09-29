# FractalDataWorks Connections Framework Documentation

## Executive Summary

The FractalDataWorks Connections Framework provides a unified, pluggable architecture for managing external system connections. Built on the Services framework, it offers type-safe connection management, automatic pooling, retry logic, and comprehensive monitoring for database connections, REST APIs, message queues, and any external system integration. The framework ensures consistent connection handling across distributed applications with minimal boilerplate code.

## Architecture Overview

### Design Philosophy

The Connections framework extends the Services architecture specifically for connection management:

1. **Unified Connection Interface**: All connections implement `IGenericConnection` regardless of underlying technology
2. **Factory-Based Creation**: Connection factories ensure proper initialization and resource management
3. **Configuration-Driven**: Strongly-typed configuration with validation for each connection type
4. **Provider Pattern**: Central connection provider manages all connection types
5. **Lifecycle Management**: Automatic connection pooling, health checks, and disposal

### Framework Layers

```
┌─────────────────────────────────────────────────────────────┐
│                 Application Services                        │
├─────────────────────────────────────────────────────────────┤
│              Connection Provider (Central)                  │
├─────────────────────────────────────────────────────────────┤
│         Connection Factories (Type-Specific)                │
├─────────────────────────────────────────────────────────────┤
│     Connection Implementations (SQL, REST, MQ, etc.)        │
├─────────────────────────────────────────────────────────────┤
│         Connection Base Classes & Abstractions              │
├─────────────────────────────────────────────────────────────┤
│      Connection Types Registry (Source Generated)           │
└─────────────────────────────────────────────────────────────┘
```

## Project Structure

### FractalDataWorks.Services.Connections.Abstractions

**Purpose**: Defines the core contracts for all connection types and their lifecycle management.

#### Core Interfaces

##### IGenericConnection
The base interface for all connections:
```csharp
public interface IGenericConnection : IAsyncDisposable, IDisposable
{
    string ConnectionId { get; }           // Unique identifier
    string ConnectionName { get; }         // Friendly name
    ConnectionState State { get; }         // Current state
    DateTime CreatedAt { get; }            // Creation timestamp
    DateTime? LastUsedAt { get; }          // Last activity

    Task<bool> OpenAsync(CancellationToken cancellationToken = default);
    Task<bool> CloseAsync(CancellationToken cancellationToken = default);
    Task<bool> ValidateAsync(CancellationToken cancellationToken = default);
    Task<HealthCheckResult> CheckHealthAsync(CancellationToken cancellationToken = default);
}
```

##### IGenericConnection<TResource>
Generic connection with typed resource access:
```csharp
public interface IGenericConnection<TResource> : IGenericConnection
{
    TResource Resource { get; }            // Underlying resource (DbConnection, HttpClient, etc.)
    Task<TResource> GetResourceAsync();    // Async resource acquisition
}
```

##### IConnectionConfiguration
Base configuration for all connections:
```csharp
public interface IConnectionConfiguration : IGenericConfiguration
{
    string ConnectionType { get; set; }    // Type identifier for factory lookup
    string Name { get; set; }              // Configuration name
    int? PoolSize { get; set; }            // Connection pool size
    int? Timeout { get; set; }             // Connection timeout in seconds
    RetryPolicy RetryPolicy { get; set; }  // Retry configuration
    bool EnableHealthChecks { get; set; }  // Health monitoring
}
```

##### IConnectionFactory
Factory pattern for connection creation:
```csharp
public interface IConnectionFactory
{
    Task<IGenericResult<IGenericConnection>> CreateConnectionAsync(
        IConnectionConfiguration configuration);
}

public interface IConnectionFactory<TConnection, TConfiguration> : IConnectionFactory
    where TConnection : IGenericConnection
    where TConfiguration : IConnectionConfiguration
{
    Task<IGenericResult<TConnection>> CreateConnectionAsync(
        TConfiguration configuration);
}
```

##### IGenericConnectionProvider
Central provider for all connection types:
```csharp
public interface IGenericConnectionProvider
{
    Task<IGenericResult<IGenericConnection>> GetConnection(IConnectionConfiguration configuration);
    Task<IGenericResult<IGenericConnection>> GetConnection(int configurationId);
    Task<IGenericResult<IGenericConnection>> GetConnection(string configurationName);
    Task<IGenericResult<T>> GetConnection<T>(IConnectionConfiguration configuration) where T : IGenericConnection;
}
```

#### Connection State Management

##### ConnectionState Enum
```csharp
public enum ConnectionState
{
    Disconnected,       // Not connected
    Connecting,         // Connection in progress
    Connected,          // Active connection
    Executing,          // Executing operation
    Broken,            // Connection failed
    Closed             // Explicitly closed
}
```

##### Connection Events
```csharp
public interface IConnectionEvents
{
    event EventHandler<ConnectionOpenedEventArgs> ConnectionOpened;
    event EventHandler<ConnectionClosedEventArgs> ConnectionClosed;
    event EventHandler<ConnectionErrorEventArgs> ConnectionError;
    event EventHandler<ConnectionStateChangedEventArgs> StateChanged;
}
```

### FractalDataWorks.Services.Connections

**Purpose**: Core implementation of the connections framework with base classes and provider.

#### GenericConnectionProvider
Central connection management:
```csharp
public sealed class GenericConnectionProvider : IGenericConnectionProvider
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IConfiguration _configuration;
    private readonly ILogger<GenericConnectionProvider> _logger;

    public async Task<IGenericResult<IGenericConnection>> GetConnection(
        IConnectionConfiguration configuration)
    {
        // 1. Validate configuration
        // 2. Look up connection type in registry
        // 3. Get appropriate factory from DI
        // 4. Create connection via factory
        // 5. Initialize and validate connection
        // 6. Return wrapped in Result type
    }
}
```

#### ConnectionTypes Registry
Source-generated static registry:
```csharp
public static partial class ConnectionTypes
{
    // Generated methods
    public static ConnectionType GetById(int id);
    public static ConnectionType GetByName(string name);
    public static IEnumerable<ConnectionType> GetByCategory(string category);

    // Registration
    public static void RegisterAll(IServiceCollection services);

    // Known types (generated)
    public static readonly SqlConnectionType Sql;
    public static readonly RestConnectionType Rest;
    public static readonly RedisConnectionType Redis;
    // ... more types
}
```

#### Connection Base Classes

##### ConnectionBase<TResource, TConfiguration>
```csharp
public abstract class ConnectionBase<TResource, TConfiguration>
    : IGenericConnection<TResource>
    where TConfiguration : IConnectionConfiguration
{
    protected readonly ILogger Logger;
    protected readonly TConfiguration Configuration;
    protected ConnectionState _state;
    protected TResource? _resource;

    public string ConnectionId { get; }
    public string ConnectionName { get; }
    public ConnectionState State => _state;
    public DateTime CreatedAt { get; }
    public DateTime? LastUsedAt { get; protected set; }

    protected ConnectionBase(
        ILogger logger,
        TConfiguration configuration)
    {
        Logger = logger;
        Configuration = configuration;
        ConnectionId = Guid.NewGuid().ToString();
        ConnectionName = configuration.Name;
        CreatedAt = DateTime.UtcNow;
        _state = ConnectionState.Disconnected;
    }

    public virtual async Task<bool> OpenAsync(CancellationToken cancellationToken)
    {
        if (_state == ConnectionState.Connected)
            return true;

        try
        {
            SetState(ConnectionState.Connecting);
            _resource = await CreateResourceAsync(cancellationToken);
            await OnOpeningAsync(cancellationToken);
            SetState(ConnectionState.Connected);
            return true;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to open connection {ConnectionId}", ConnectionId);
            SetState(ConnectionState.Broken);
            return false;
        }
    }

    protected abstract Task<TResource> CreateResourceAsync(CancellationToken cancellationToken);
    protected virtual Task OnOpeningAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
```

### FractalDataWorks.Services.Connections.MsSql

**Purpose**: SQL Server connection implementation with advanced features.

#### SqlConnection Implementation

##### MsSqlConnection
```csharp
public class MsSqlConnection : ConnectionBase<SqlConnection, MsSqlConfiguration>
{
    private readonly ISqlConnectionPool _pool;
    private readonly IRetryPolicy _retryPolicy;

    public MsSqlConnection(
        ILogger<MsSqlConnection> logger,
        MsSqlConfiguration configuration,
        ISqlConnectionPool pool,
        IRetryPolicy retryPolicy)
        : base(logger, configuration)
    {
        _pool = pool;
        _retryPolicy = retryPolicy;
    }

    protected override async Task<SqlConnection> CreateResourceAsync(
        CancellationToken cancellationToken)
    {
        var connection = _pool.Rent() ?? new SqlConnection(Configuration.ConnectionString);

        await _retryPolicy.ExecuteAsync(async () =>
        {
            await connection.OpenAsync(cancellationToken);
        });

        return connection;
    }

    public async Task<IGenericResult<T>> ExecuteAsync<T>(
        Func<SqlConnection, Task<T>> operation)
    {
        if (State != ConnectionState.Connected)
            return GenericResult<T>.Failure("Connection not open");

        SetState(ConnectionState.Executing);

        try
        {
            var result = await _retryPolicy.ExecuteAsync(() => operation(_resource!));
            LastUsedAt = DateTime.UtcNow;
            return GenericResult<T>.Success(result);
        }
        finally
        {
            SetState(ConnectionState.Connected);
        }
    }
}
```

##### MsSqlConfiguration
```csharp
public class MsSqlConfiguration : IConnectionConfiguration
{
    public string ConnectionType => "MsSql";
    public string Name { get; set; }
    public string ConnectionString { get; set; }
    public int? CommandTimeout { get; set; } = 30;
    public int? PoolSize { get; set; } = 10;
    public int? Timeout { get; set; } = 15;
    public RetryPolicy RetryPolicy { get; set; } = RetryPolicy.Default;
    public bool EnableHealthChecks { get; set; } = true;
    public bool MultipleActiveResultSets { get; set; } = false;
    public string ApplicationName { get; set; }
    public bool Encrypt { get; set; } = true;
    public bool TrustServerCertificate { get; set; } = false;

    public IGenericResult<ValidationResult> Validate()
    {
        var validator = new MsSqlConfigurationValidator();
        return GenericResult<ValidationResult>.From(validator.Validate(this));
    }
}
```

##### MsSqlConnectionFactory
```csharp
public class MsSqlConnectionFactory : IConnectionFactory<MsSqlConnection, MsSqlConfiguration>
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<MsSqlConnectionFactory> _logger;

    public async Task<IGenericResult<MsSqlConnection>> CreateConnectionAsync(
        MsSqlConfiguration configuration)
    {
        try
        {
            var validationResult = configuration.Validate();
            if (!validationResult.IsSuccess)
                return GenericResult<MsSqlConnection>.Failure(validationResult.Error);

            var connection = new MsSqlConnection(
                _serviceProvider.GetRequiredService<ILogger<MsSqlConnection>>(),
                configuration,
                _serviceProvider.GetRequiredService<ISqlConnectionPool>(),
                _serviceProvider.GetRequiredService<IRetryPolicy>());

            await connection.OpenAsync();

            return GenericResult<MsSqlConnection>.Success(connection);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create SQL connection");
            return GenericResult<MsSqlConnection>.Failure(ex.Message);
        }
    }
}
```

#### Advanced SQL Features

##### Connection Pooling
```csharp
public class SqlConnectionPool : ISqlConnectionPool
{
    private readonly ConcurrentBag<SqlConnection> _available;
    private readonly HashSet<SqlConnection> _inUse;
    private readonly SemaphoreSlim _semaphore;
    private readonly MsSqlConfiguration _configuration;

    public SqlConnection? Rent()
    {
        if (_available.TryTake(out var connection))
        {
            _inUse.Add(connection);
            return connection;
        }
        return null;
    }

    public void Return(SqlConnection connection)
    {
        if (_inUse.Remove(connection))
        {
            if (connection.State == ConnectionState.Open)
                _available.Add(connection);
            else
                connection.Dispose();
        }
    }
}
```

##### Transaction Management
```csharp
public class SqlTransactionScope : IAsyncDisposable
{
    private readonly MsSqlConnection _connection;
    private readonly SqlTransaction _transaction;

    public static async Task<SqlTransactionScope> BeginAsync(
        MsSqlConnection connection,
        IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
    {
        var transaction = await connection.Resource.BeginTransactionAsync(isolationLevel);
        return new SqlTransactionScope(connection, transaction);
    }

    public async Task CommitAsync()
    {
        await _transaction.CommitAsync();
    }

    public async Task RollbackAsync()
    {
        await _transaction.RollbackAsync();
    }
}
```

### FractalDataWorks.Services.Connections.Rest

**Purpose**: RESTful API connection implementation with HTTP client management.

#### RestConnection Implementation

##### RestConnection
```csharp
public class RestConnection : ConnectionBase<HttpClient, RestConfiguration>
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ITokenProvider _tokenProvider;

    protected override async Task<HttpClient> CreateResourceAsync(
        CancellationToken cancellationToken)
    {
        var client = _httpClientFactory.CreateClient(Configuration.Name);

        client.BaseAddress = new Uri(Configuration.BaseUrl);
        client.Timeout = TimeSpan.FromSeconds(Configuration.Timeout ?? 30);

        // Add default headers
        foreach (var header in Configuration.DefaultHeaders)
        {
            client.DefaultRequestHeaders.Add(header.Key, header.Value);
        }

        // Add authentication
        if (Configuration.RequiresAuthentication)
        {
            var token = await _tokenProvider.GetTokenAsync(cancellationToken);
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
        }

        return client;
    }

    public async Task<IGenericResult<T>> GetAsync<T>(
        string endpoint,
        CancellationToken cancellationToken = default)
    {
        return await ExecuteAsync<T>(async client =>
        {
            var response = await client.GetAsync(endpoint, cancellationToken);
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(content);
        });
    }

    public async Task<IGenericResult<TResponse>> PostAsync<TRequest, TResponse>(
        string endpoint,
        TRequest data,
        CancellationToken cancellationToken = default)
    {
        return await ExecuteAsync<TResponse>(async client =>
        {
            var json = JsonSerializer.Serialize(data);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await client.PostAsync(endpoint, content, cancellationToken);
            response.EnsureSuccessStatusCode();
            var responseContent = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<TResponse>(responseContent);
        });
    }
}
```

##### RestConfiguration
```csharp
public class RestConfiguration : IConnectionConfiguration
{
    public string ConnectionType => "Rest";
    public string Name { get; set; }
    public string BaseUrl { get; set; }
    public Dictionary<string, string> DefaultHeaders { get; set; } = new();
    public bool RequiresAuthentication { get; set; }
    public string AuthenticationEndpoint { get; set; }
    public string ClientId { get; set; }
    public string ClientSecret { get; set; }
    public int? Timeout { get; set; } = 30;
    public RetryPolicy RetryPolicy { get; set; }
    public bool EnableHealthChecks { get; set; } = true;
    public string HealthCheckEndpoint { get; set; } = "/health";
}
```

### FractalDataWorks.Services.Connections.Http.Abstractions

**Purpose**: HTTP-specific abstractions and utilities.

#### HTTP-Specific Features

##### Request Builders
```csharp
public interface IHttpRequestBuilder
{
    IHttpRequestBuilder WithEndpoint(string endpoint);
    IHttpRequestBuilder WithMethod(HttpMethod method);
    IHttpRequestBuilder WithHeader(string name, string value);
    IHttpRequestBuilder WithQueryParameter(string name, string value);
    IHttpRequestBuilder WithBody<T>(T body);
    IHttpRequestBuilder WithTimeout(TimeSpan timeout);
    HttpRequestMessage Build();
}
```

##### Response Handlers
```csharp
public interface IHttpResponseHandler<T>
{
    Task<IGenericResult<T>> HandleAsync(HttpResponseMessage response);
    Task<IGenericResult<T>> HandleErrorAsync(HttpResponseMessage response);
}
```

##### Interceptors
```csharp
public interface IHttpInterceptor
{
    Task<HttpRequestMessage> OnRequestAsync(HttpRequestMessage request);
    Task<HttpResponseMessage> OnResponseAsync(HttpResponseMessage response);
    Task OnErrorAsync(Exception exception, HttpRequestMessage request);
}
```

## Connection Lifecycle Management

### Connection States

```
Disconnected → Connecting → Connected → Executing → Connected
                    ↓           ↓           ↓           ↓
                  Broken     Broken      Broken      Closed
```

### Connection Pooling

#### Pool Configuration
```csharp
public class PoolConfiguration
{
    public int MinSize { get; set; } = 0;
    public int MaxSize { get; set; } = 10;
    public TimeSpan ConnectionLifetime { get; set; } = TimeSpan.FromMinutes(5);
    public TimeSpan IdleTimeout { get; set; } = TimeSpan.FromMinutes(1);
    public bool TestOnBorrow { get; set; } = true;
    public bool TestWhileIdle { get; set; } = true;
}
```

#### Pool Statistics
```csharp
public class PoolStatistics
{
    public int TotalConnections { get; set; }
    public int ActiveConnections { get; set; }
    public int IdleConnections { get; set; }
    public int WaitingRequests { get; set; }
    public TimeSpan AverageWaitTime { get; set; }
    public long TotalRequests { get; set; }
    public long TotalTimeouts { get; set; }
}
```

### Health Monitoring

#### Health Check Implementation
```csharp
public class ConnectionHealthCheck : IHealthCheck
{
    private readonly IGenericConnectionProvider _provider;
    private readonly string _connectionName;

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _provider.GetConnection(_connectionName);
            if (result.IsSuccess)
            {
                var healthResult = await result.Value.CheckHealthAsync(cancellationToken);
                return healthResult;
            }
            return HealthCheckResult.Unhealthy(result.Error);
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Health check failed", ex);
        }
    }
}
```

#### Monitoring Metrics
```csharp
public class ConnectionMetrics
{
    public string ConnectionId { get; set; }
    public string ConnectionType { get; set; }
    public long TotalOperations { get; set; }
    public long SuccessfulOperations { get; set; }
    public long FailedOperations { get; set; }
    public TimeSpan TotalExecutionTime { get; set; }
    public TimeSpan AverageExecutionTime { get; set; }
    public DateTime LastOperationTime { get; set; }
    public Dictionary<string, long> ErrorCounts { get; set; }
}
```

## Retry and Resilience

### Retry Policies

#### Exponential Backoff
```csharp
public class ExponentialBackoffRetryPolicy : IRetryPolicy
{
    private readonly int _maxRetries;
    private readonly TimeSpan _baseDelay;

    public async Task<T> ExecuteAsync<T>(Func<Task<T>> operation)
    {
        for (int attempt = 0; attempt < _maxRetries; attempt++)
        {
            try
            {
                return await operation();
            }
            catch (Exception) when (attempt < _maxRetries - 1)
            {
                var delay = TimeSpan.FromMilliseconds(
                    _baseDelay.TotalMilliseconds * Math.Pow(2, attempt));
                await Task.Delay(delay);
            }
        }
        return await operation(); // Last attempt, let it throw
    }
}
```

#### Circuit Breaker
```csharp
public class CircuitBreaker
{
    private CircuitState _state = CircuitState.Closed;
    private int _failureCount = 0;
    private DateTime _lastFailureTime;
    private readonly int _threshold;
    private readonly TimeSpan _timeout;

    public async Task<T> ExecuteAsync<T>(Func<Task<T>> operation)
    {
        if (_state == CircuitState.Open)
        {
            if (DateTime.UtcNow - _lastFailureTime > _timeout)
                _state = CircuitState.HalfOpen;
            else
                throw new CircuitOpenException();
        }

        try
        {
            var result = await operation();
            OnSuccess();
            return result;
        }
        catch
        {
            OnFailure();
            throw;
        }
    }

    private void OnSuccess()
    {
        _failureCount = 0;
        _state = CircuitState.Closed;
    }

    private void OnFailure()
    {
        _failureCount++;
        _lastFailureTime = DateTime.UtcNow;

        if (_failureCount >= _threshold)
            _state = CircuitState.Open;
    }
}
```

## Usage Examples

### Basic Connection Usage
```csharp
public class DataService
{
    private readonly IGenericConnectionProvider _connectionProvider;

    public async Task<IEnumerable<Customer>> GetCustomersAsync()
    {
        // Get connection by configuration name
        var connectionResult = await _connectionProvider.GetConnection("CustomerDB");

        if (!connectionResult.IsSuccess)
            return Enumerable.Empty<Customer>();

        var sqlConnection = connectionResult.Value as MsSqlConnection;

        return await sqlConnection.ExecuteAsync(async conn =>
        {
            using var command = new SqlCommand("SELECT * FROM Customers", conn);
            using var reader = await command.ExecuteReaderAsync();

            var customers = new List<Customer>();
            while (await reader.ReadAsync())
            {
                customers.Add(new Customer
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1)
                });
            }
            return customers;
        });
    }
}
```

### REST API Connection
```csharp
public class ApiService
{
    private readonly IGenericConnectionProvider _connectionProvider;

    public async Task<WeatherData> GetWeatherAsync(string city)
    {
        var config = new RestConfiguration
        {
            Name = "WeatherAPI",
            BaseUrl = "https://api.weather.com",
            DefaultHeaders = new Dictionary<string, string>
            {
                ["X-API-Key"] = "your-api-key"
            }
        };

        var connectionResult = await _connectionProvider.GetConnection<RestConnection>(config);

        if (!connectionResult.IsSuccess)
            throw new Exception(connectionResult.Error);

        var connection = connectionResult.Value;

        var result = await connection.GetAsync<WeatherData>($"/weather/{city}");

        return result.Value;
    }
}
```

### Transaction Management
```csharp
public class TransactionService
{
    private readonly IGenericConnectionProvider _connectionProvider;

    public async Task<IGenericResult> TransferFundsAsync(
        int fromAccount,
        int toAccount,
        decimal amount)
    {
        var connectionResult = await _connectionProvider.GetConnection("BankingDB");

        if (!connectionResult.IsSuccess)
            return GenericResult.Failure(connectionResult.Error);

        var sqlConnection = connectionResult.Value as MsSqlConnection;

        await using var transaction = await SqlTransactionScope.BeginAsync(
            sqlConnection,
            IsolationLevel.Serializable);

        try
        {
            // Debit source account
            await sqlConnection.ExecuteAsync(async conn =>
            {
                using var cmd = new SqlCommand(
                    "UPDATE Accounts SET Balance = Balance - @Amount WHERE Id = @Id",
                    conn,
                    transaction.Transaction);
                cmd.Parameters.AddWithValue("@Amount", amount);
                cmd.Parameters.AddWithValue("@Id", fromAccount);
                return await cmd.ExecuteNonQueryAsync();
            });

            // Credit destination account
            await sqlConnection.ExecuteAsync(async conn =>
            {
                using var cmd = new SqlCommand(
                    "UPDATE Accounts SET Balance = Balance + @Amount WHERE Id = @Id",
                    conn,
                    transaction.Transaction);
                cmd.Parameters.AddWithValue("@Amount", amount);
                cmd.Parameters.AddWithValue("@Id", toAccount);
                return await cmd.ExecuteNonQueryAsync();
            });

            await transaction.CommitAsync();
            return GenericResult.Success("Transfer completed");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return GenericResult.Failure($"Transfer failed: {ex.Message}");
        }
    }
}
```

## Configuration

### appsettings.json Configuration
```json
{
  "Connections": {
    "CustomerDB": {
      "ConnectionType": "MsSql",
      "ConnectionString": "Server=localhost;Database=CustomerDB;...",
      "CommandTimeout": 30,
      "PoolSize": 10,
      "RetryPolicy": {
        "MaxRetries": 3,
        "InitialDelay": "00:00:01"
      }
    },
    "WeatherAPI": {
      "ConnectionType": "Rest",
      "BaseUrl": "https://api.weather.com",
      "Timeout": 30,
      "DefaultHeaders": {
        "X-API-Key": "#{WeatherApiKey}#"
      },
      "RetryPolicy": {
        "Type": "ExponentialBackoff",
        "MaxRetries": 5
      }
    },
    "CacheServer": {
      "ConnectionType": "Redis",
      "ConnectionString": "localhost:6379",
      "Database": 0,
      "PoolSize": 20
    }
  }
}
```

### Dependency Injection Setup
```csharp
public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        // Register connection types
        ConnectionTypes.RegisterAll(services);

        // Register connection provider
        services.AddSingleton<IGenericConnectionProvider, GenericConnectionProvider>();

        // Register specific factories
        services.AddTransient<IConnectionFactory, MsSqlConnectionFactory>();
        services.AddTransient<IConnectionFactory, RestConnectionFactory>();

        // Configure HttpClient factory
        services.AddHttpClient();

        // Add health checks
        services.AddHealthChecks()
            .AddCheck<ConnectionHealthCheck>("sql_connection")
            .AddCheck<ConnectionHealthCheck>("api_connection");

        // Configure connection pooling
        services.Configure<PoolConfiguration>(
            Configuration.GetSection("ConnectionPooling"));
    }
}
```

## Best Practices

### Connection Management
1. **Always use the provider**: Never create connections directly
2. **Dispose properly**: Use `await using` for automatic disposal
3. **Handle failures gracefully**: Check Result.IsSuccess before using connections
4. **Configure timeouts**: Set appropriate timeouts for your use case
5. **Monitor health**: Enable health checks for production

### Configuration
1. **Use strong typing**: Always use typed configuration classes
2. **Validate early**: Validate configuration at startup
3. **Secure credentials**: Use Key Vault or secure storage for secrets
4. **Environment-specific**: Use configuration transforms for different environments
5. **Document settings**: Provide XML documentation for all configuration properties

### Performance
1. **Enable pooling**: Use connection pooling for frequently accessed resources
2. **Set pool limits**: Configure min/max pool sizes appropriately
3. **Monitor metrics**: Track connection usage and performance
4. **Use async**: Always use async methods for I/O operations
5. **Cache connections**: Reuse connections when possible

### Error Handling
1. **Use retry policies**: Configure appropriate retry strategies
2. **Implement circuit breakers**: Prevent cascading failures
3. **Log comprehensively**: Log all connection events and errors
4. **Handle transient failures**: Distinguish between transient and permanent failures
5. **Provide fallbacks**: Have fallback strategies for critical connections

## Troubleshooting

### Common Issues

#### Connection Timeout
- Check network connectivity
- Verify firewall rules
- Review timeout configuration
- Check server load

#### Pool Exhaustion
- Increase pool size
- Review connection usage patterns
- Check for connection leaks
- Monitor pool statistics

#### Authentication Failures
- Verify credentials
- Check token expiration
- Review authentication configuration
- Monitor authentication events

#### Performance Issues
- Enable connection pooling
- Review query performance
- Check network latency
- Profile connection usage

## Extending the Framework

### Creating Custom Connection Types

1. **Define Configuration**:
```csharp
public class CustomConfiguration : IConnectionConfiguration
{
    public string ConnectionType => "Custom";
    // Add custom properties
}
```

2. **Implement Connection**:
```csharp
public class CustomConnection : ConnectionBase<CustomResource, CustomConfiguration>
{
    protected override async Task<CustomResource> CreateResourceAsync(
        CancellationToken cancellationToken)
    {
        // Create and return resource
    }
}
```

3. **Create Factory**:
```csharp
public class CustomConnectionFactory : IConnectionFactory<CustomConnection, CustomConfiguration>
{
    public async Task<IGenericResult<CustomConnection>> CreateConnectionAsync(
        CustomConfiguration configuration)
    {
        // Create and return connection
    }
}
```

4. **Register Type**:
```csharp
public class CustomConnectionType : ConnectionTypeBase<CustomConnection, CustomConfiguration, CustomConnectionFactory>
{
    public CustomConnectionType() : base(100, "Custom", "CustomCategory") { }
}
```

## License and Credits

Developed by FractalDataWorks Engineering Team
Lead Architect: Mike Blair
Copyright © 2024 FractalDataWorks Electric Cooperative