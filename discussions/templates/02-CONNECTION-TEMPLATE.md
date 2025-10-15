# Connection Project Template Documentation

## Document Purpose

This document provides comprehensive guidance for creating FractalDataWorks Connection implementations. Connections represent the protocol/transport layer in the FractalDataWorks architecture and are responsible for executing commands against external systems.

**Related Documentation**:
- `discussions/ARCHITECTURE_SUMMARY.md` - Overall architecture
- `discussions/commands/INVERTED_TRANSLATOR_ARCHITECTURE.md` - Translator ownership pattern

## 1. What is a Connection?

### Purpose and Responsibilities

A **Connection** in FractalDataWorks is a service that:
- Manages the physical connection to an external system (database, API, file system, etc.)
- Executes protocol-specific commands (SQL, HTTP, file operations)
- Owns and manages translator instances for command translation
- Handles connection lifecycle (open, execute, close, dispose)
- Manages connection state and transactions (if applicable)

### Connection vs Service Distinction

```
Connection = Protocol/Transport Layer
    - Knows HOW to communicate (TCP, HTTP, file I/O)
    - Executes low-level commands (SqlConnectionCommand, HttpConnectionCommand)
    - Manages physical resources (network connections, file handles)

Service = Business/Application Layer
    - Knows WHAT to do (data operations, business logic)
    - Uses connections to perform operations
    - Orchestrates multiple connection calls
```

### Connection<TTranslator> Pattern (Inverted Architecture)

**CRITICAL**: Connections OWN their translators. This is the inverted architecture pattern.

```csharp
// CORRECT: Connection owns translator
public class MsSqlConnection<TTranslator> : ConnectionBase<MsSqlConfiguration>
    where TTranslator : IDataCommandTranslator
{
    private readonly TTranslator _translator;

    public MsSqlConnection(TTranslator translator, MsSqlConfiguration config)
    {
        _translator = translator; // Injected by DI
    }
}

// WRONG: Connection fetches translator from service locator
public class MsSqlConnection : ConnectionBase<MsSqlConfiguration>
{
    public async Task Execute(IDataCommand command)
    {
        var translator = DataCommandTranslators.GetTranslator("TSql"); // Anti-pattern!
    }
}
```

**Benefits**:
1. One connection type can support multiple command languages (HTTP: REST/GraphQL/gRPC)
2. Translator selection happens at registration time (type-safe)
3. Easy to swap implementations via DI configuration
4. Shared infrastructure (HttpClientFactory for all HTTP translators)

### IConnectionCommand Pattern

Each connection defines its own domain-specific command type:

```csharp
// SQL Connection Command
public sealed class SqlConnectionCommand : IConnectionCommand
{
    public string SqlText { get; }
    public IReadOnlyDictionary<string, object> Parameters { get; }
}

// HTTP Connection Command
public sealed class HttpConnectionCommand : IConnectionCommand
{
    public string Url { get; }
    public HttpMethod Method { get; }
    public string Body { get; }
    public Dictionary<string, string> Headers { get; }
}

// File Connection Command
public sealed class FileConnectionCommand : IConnectionCommand
{
    public FileOperation Operation { get; } // Read, Write, Delete
    public string FilePath { get; }
    public byte[] Content { get; }
}
```

## 2. Connection Components

### Project Structure

A complete connection implementation consists of two projects:

```
FractalDataWorks.Services.Connections.{Type}.Abstractions/
    ├── I{Type}Connection.cs           (interface, if needed for extensibility)
    ├── I{Type}ConnectionFactory.cs    (factory interface)
    └── {Type}ConnectionMetadata.cs    (metadata, optional)

FractalDataWorks.Services.Connections.{Type}/
    ├── {Type}Connection.cs            (connection implementation)
    ├── {Type}Connection{TTranslator}.cs (generic version)
    ├── {Type}Configuration.cs         (configuration)
    ├── {Type}ConnectionType.cs        (service type registration)
    ├── {Type}ConnectionFactory.cs     (factory implementation)
    ├── Commands/
    │   └── {Type}ConnectionCommand.cs (domain-specific command)
    ├── Translators/                   (IMPORTANT: Translators live HERE!)
    │   ├── {Translator1}.cs
    │   ├── {Translator2}.cs
    │   └── ...
    └── States/                        (if stateful connection)
        └── Custom connection states (optional)
```

### Required Files

#### 1. IConnectionCommand Definition

```csharp
namespace FractalDataWorks.Services.Connections.{Type}.Commands;

/// <summary>
/// Command for {Type} protocol operations.
/// </summary>
public sealed class {Type}ConnectionCommand : IConnectionCommand
{
    // Protocol-specific command properties
    public string ProtocolSpecificProperty { get; init; }

    // Required IConnectionCommand members
    public Guid CommandId { get; }
    public DateTime CreatedAt { get; }
    public string CommandType => "{Type}Connection";
    public string ConnectionName { get; }
    public string ProviderType => "{Type}";

    public IGenericResult Validate()
    {
        // Validate protocol-specific requirements
    }
}
```

#### 2. Connection Configuration

```csharp
namespace FractalDataWorks.Services.Connections.{Type};

/// <summary>
/// Configuration for {Type} connections.
/// </summary>
public sealed class {Type}Configuration : ConfigurationBase<{Type}Configuration>, IConnectionConfiguration
{
    /// <summary>
    /// Protocol-specific configuration properties.
    /// </summary>
    public string ProtocolSpecificSetting { get; init; } = string.Empty;

    /// <summary>
    /// Connection timeout in seconds.
    /// </summary>
    public int TimeoutSeconds { get; init; } = 30;

    /// <inheritdoc/>
    public override string SectionName => "Connections:{Type}";

    /// <inheritdoc/>
    public string ConnectionType { get; init; } = "{Type}";

    /// <inheritdoc/>
    public IServiceLifetime Lifetime { get; init; } = ServiceLifetimes.Scoped;

    protected override IValidator<{Type}Configuration>? GetValidator()
    {
        return new {Type}ConfigurationValidator();
    }
}
```

#### 3. Connection Implementation

```csharp
namespace FractalDataWorks.Services.Connections.{Type};

/// <summary>
/// {Type} connection implementation.
/// </summary>
public sealed class {Type}Connection<TTranslator> :
    ConnectionBase<IConnectionCommand, {Type}Configuration, {Type}Connection<TTranslator>>,
    IGenericConnection
    where TTranslator : IDataCommandTranslator
{
    private readonly TTranslator _translator;
    private readonly {Type}Configuration _configuration;
    private IConnectionState _state;

    public {Type}Connection(
        ILogger<{Type}Connection<TTranslator>> logger,
        {Type}Configuration configuration,
        TTranslator translator) // Translator is INJECTED
        : base(logger, configuration)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _translator = translator ?? throw new ArgumentNullException(nameof(translator));
        _state = ConnectionStates.Created;
    }

    public override async Task<IGenericResult<TOut>> Execute<TOut>(
        IConnectionCommand command,
        CancellationToken cancellationToken)
    {
        // Execute the protocol-specific command
        if (command is {Type}ConnectionCommand typedCommand)
        {
            return await ExecuteTypedCommand<TOut>(typedCommand, cancellationToken);
        }

        return GenericResult<TOut>.Failure($"Unsupported command type: {command.GetType().Name}");
    }

    private async Task<IGenericResult<TOut>> ExecuteTypedCommand<TOut>(
        {Type}ConnectionCommand command,
        CancellationToken cancellationToken)
    {
        // Implementation depends on protocol
        // Example: Execute SQL, make HTTP request, read/write file
    }
}
```

#### 4. Connection Type (Registration)

```csharp
namespace FractalDataWorks.Services.Connections.{Type};

[ServiceTypeOption(typeof(ConnectionTypes), "{Type}")]
public sealed class {Type}ConnectionType :
    ConnectionTypeBase<IGenericConnection, {Type}Configuration, I{Type}ConnectionFactory>,
    IConnectionType
{
    public static {Type}ConnectionType Instance { get; } = new();

    private {Type}ConnectionType()
        : base(id: 5, name: "{Type}", category: "Database") // or "Web", "File", etc.
    {
    }

    public override void Register(IServiceCollection services)
    {
        // Register connection factory
        services.AddScoped<I{Type}ConnectionFactory, {Type}ConnectionFactory>();

        // Register connection with specific translator
        services.AddScoped<{Type}Connection<{Default}Translator>>();

        // Register translators that this connection brings
        // NOTE: Translators are part of THIS project, not separate
        services.AddScoped<{Translator1}>();
        services.AddScoped<{Translator2}>();

        // Register protocol-specific infrastructure
        // Example: HttpClientFactory for HTTP connections
        services.AddHttpClient("FractalDataWorks{Type}");
    }

    public override IReadOnlyList<string> SupportedTranslators =>
        ["{Translator1}", "{Translator2}"];
}
```

## 3. Dependencies

### Project References

#### Abstractions Project Dependencies
```xml
<ItemGroup>
    <PackageReference Include="FractalDataWorks.Services.Connections.Abstractions" />
    <PackageReference Include="FractalDataWorks.Results" />
</ItemGroup>
```

#### Implementation Project Dependencies
```xml
<ItemGroup>
    <ProjectReference Include="..\FractalDataWorks.Services.Connections.{Type}.Abstractions" />
    <PackageReference Include="FractalDataWorks.Services.Connections.Abstractions" />
    <PackageReference Include="FractalDataWorks.Services.Connections" />
    <PackageReference Include="FractalDataWorks.Results" />
    <PackageReference Include="FractalDataWorks.Commands.Data.Abstractions" />

    <!-- Domain-specific libraries -->
    <!-- SQL: Microsoft.Data.SqlClient or Npgsql -->
    <!-- HTTP: Microsoft.Extensions.Http -->
    <!-- File: System.IO.Abstractions -->
</ItemGroup>
```

## 4. Translator Integration Pattern

### Translators Live WITH Connections

**CRITICAL**: Translators are NOT separate projects. They are part of the connection implementation.

```
Project Structure:
FractalDataWorks.Services.Connections.MsSql/
    ├── MsSqlConnection.cs
    └── Translators/              ← Translators HERE!
        ├── TSqlTranslator.cs
        └── SqlKataTranslator.cs

FractalDataWorks.Services.Connections.Http/
    ├── HttpConnection.cs
    └── Translators/              ← Translators HERE!
        ├── RestTranslator.cs
        ├── GraphQLTranslator.cs
        └── GrpcTranslator.cs
```

### Translator Implementation Example

```csharp
namespace FractalDataWorks.Services.Connections.MsSql.Translators;

/// <summary>
/// Translates DataCommands to T-SQL.
/// </summary>
public class TSqlTranslator : DataCommandTranslatorBase
{
    public override async Task<IGenericResult<IConnectionCommand>> TranslateAsync(
        IDataCommand command,
        IContainerContext context,
        CancellationToken ct)
    {
        if (command is QueryCommand queryCmd)
        {
            var sqlBuilder = new StringBuilder();
            sqlBuilder.Append($"SELECT * FROM {context.ContainerName}");

            if (queryCmd.Filter != null)
            {
                sqlBuilder.Append(" WHERE ");
                // Build WHERE clause from filter expression
            }

            return GenericResult<IConnectionCommand>.Success(
                new SqlConnectionCommand(sqlBuilder.ToString())
            );
        }

        return GenericResult<IConnectionCommand>.Failure("Unsupported command type");
    }
}
```

### Registration Pattern

```csharp
public override void Register(IServiceCollection services)
{
    // Register connection with explicit translator type
    services.AddScoped<MsSqlConnection<TSqlTranslator>>();

    // OR: Support multiple translators via factory
    services.AddScoped<Func<string, MsSqlConnection>>(sp => translatorName =>
    {
        return translatorName switch
        {
            "TSql" => new MsSqlConnection<TSqlTranslator>(/*...*/),
            "SqlKata" => new MsSqlConnection<SqlKataTranslator>(/*...*/),
            _ => throw new NotSupportedException()
        };
    });
}
```

## 5. Connection State Management

### State Machine (for Stateful Connections)

```csharp
public interface IConnectionState : IEnumOption<ConnectionStateBase>
{
}

// Built-in states from ConnectionStates:
// - Created
// - Opening
// - Open
// - Executing
// - Closing
// - Closed
// - Broken
// - Disposed

// Connection tracks current state
private IConnectionState _state = ConnectionStates.Created;

public async Task<IGenericResult> OpenAsync()
{
    if (_state != ConnectionStates.Created && _state != ConnectionStates.Closed)
    {
        return GenericResult.Failure("Connection must be in Created or Closed state");
    }

    _state = ConnectionStates.Opening;

    try
    {
        // Open physical connection
        _state = ConnectionStates.Open;
        return GenericResult.Success();
    }
    catch (Exception ex)
    {
        _state = ConnectionStates.Broken;
        return GenericResult.Failure($"Failed to open: {ex.Message}");
    }
}
```

### Transaction Support (if applicable)

```csharp
public async Task<IGenericResult> BeginTransactionAsync(
    IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
{
    if (_state != ConnectionStates.Open)
    {
        return GenericResult.Failure("Connection must be open to start transaction");
    }

    // Begin protocol-specific transaction
    // SQL: _dbConnection.BeginTransaction()
    // HTTP: Not applicable
    // File: Create transaction log
}

public async Task<IGenericResult> CommitTransactionAsync()
{
    // Commit protocol-specific transaction
}

public async Task<IGenericResult> RollbackTransactionAsync()
{
    // Rollback protocol-specific transaction
}
```

## 6. Complete Implementation Example

### Example: REST API Connection

```csharp
// 1. Connection Command
public sealed class HttpConnectionCommand : IConnectionCommand
{
    public string Url { get; init; } = string.Empty;
    public HttpMethod Method { get; init; } = HttpMethod.Get;
    public string? Body { get; init; }
    public Dictionary<string, string> Headers { get; init; } = new();

    public Guid CommandId { get; } = Guid.NewGuid();
    public DateTime CreatedAt { get; } = DateTime.UtcNow;
    public string CommandType => "HttpConnection";
    public string ConnectionName { get; init; } = "Default";
    public string ProviderType => "Http";

    public IGenericResult Validate()
    {
        if (string.IsNullOrWhiteSpace(Url))
            return GenericResult.Failure("URL cannot be empty");

        return GenericResult.Success();
    }
}

// 2. Connection Configuration
public sealed class HttpConfiguration : ConfigurationBase<HttpConfiguration>, IConnectionConfiguration
{
    public string BaseUrl { get; init; } = string.Empty;
    public TimeSpan Timeout { get; init; } = TimeSpan.FromSeconds(30);
    public Dictionary<string, string> DefaultHeaders { get; init; } = new();

    public override string SectionName => "Connections:Http";
    public string ConnectionType { get; init; } = "Http";
    public IServiceLifetime Lifetime { get; init; } = ServiceLifetimes.Scoped;
}

// 3. Connection Implementation
public sealed class HttpConnection<TTranslator> :
    ConnectionBase<IConnectionCommand, HttpConfiguration, HttpConnection<TTranslator>>
    where TTranslator : IDataCommandTranslator
{
    private readonly TTranslator _translator;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly HttpConfiguration _configuration;

    public HttpConnection(
        ILogger<HttpConnection<TTranslator>> logger,
        HttpConfiguration configuration,
        TTranslator translator,
        IHttpClientFactory httpClientFactory)
        : base(logger, configuration)
    {
        _configuration = configuration;
        _translator = translator;
        _httpClientFactory = httpClientFactory;
    }

    public override async Task<IGenericResult<TOut>> Execute<TOut>(
        IConnectionCommand command,
        CancellationToken ct)
    {
        if (command is HttpConnectionCommand httpCommand)
        {
            var client = _httpClientFactory.CreateClient("FractalDataWorksHttp");

            var request = new HttpRequestMessage(httpCommand.Method, httpCommand.Url);

            if (!string.IsNullOrEmpty(httpCommand.Body))
            {
                request.Content = new StringContent(httpCommand.Body, Encoding.UTF8, "application/json");
            }

            foreach (var header in httpCommand.Headers)
            {
                request.Headers.Add(header.Key, header.Value);
            }

            var response = await client.SendAsync(request, ct);
            var content = await response.Content.ReadAsStringAsync(ct);

            var result = JsonSerializer.Deserialize<TOut>(content);
            return GenericResult<TOut>.Success(result);
        }

        return GenericResult<TOut>.Failure("Unsupported command type");
    }
}

// 4. REST Translator (lives in Translators/ folder)
namespace FractalDataWorks.Services.Connections.Http.Translators;

public class RestTranslator : DataCommandTranslatorBase
{
    public override async Task<IGenericResult<IConnectionCommand>> TranslateAsync(
        IDataCommand command,
        IContainerContext context,
        CancellationToken ct)
    {
        if (command is QueryCommand queryCmd)
        {
            // Build OData-style query
            var url = $"{context.ContainerName}";

            if (queryCmd.Filter != null)
            {
                url += $"?$filter={BuildODataFilter(queryCmd.Filter)}";
            }

            return GenericResult<IConnectionCommand>.Success(
                new HttpConnectionCommand
                {
                    Url = url,
                    Method = HttpMethod.Get
                }
            );
        }

        return GenericResult<IConnectionCommand>.Failure("Unsupported command");
    }

    private string BuildODataFilter(IFilterExpression filter)
    {
        // Convert filter to OData syntax
        // Example: "isActive eq true and price gt 10.00"
    }
}

// 5. GraphQL Translator (same connection, different language!)
namespace FractalDataWorks.Services.Connections.Http.Translators;

public class GraphQLTranslator : DataCommandTranslatorBase
{
    public override async Task<IGenericResult<IConnectionCommand>> TranslateAsync(
        IDataCommand command,
        IContainerContext context,
        CancellationToken ct)
    {
        if (command is QueryCommand queryCmd)
        {
            // Build GraphQL query
            var query = new StringBuilder();
            query.AppendLine("query {");
            query.AppendLine($"  {context.ContainerName}(");

            if (queryCmd.Filter != null)
            {
                query.AppendLine($"    where: {{ {BuildGraphQLFilter(queryCmd.Filter)} }}");
            }

            query.AppendLine("  ) {");
            query.AppendLine("    id name /* fields */");
            query.AppendLine("  }");
            query.AppendLine("}");

            return GenericResult<IConnectionCommand>.Success(
                new HttpConnectionCommand
                {
                    Url = "/graphql",
                    Method = HttpMethod.Post,
                    Body = JsonSerializer.Serialize(new { query = query.ToString() })
                }
            );
        }

        return GenericResult<IConnectionCommand>.Failure("Unsupported command");
    }
}

// 6. Connection Type Registration
[ServiceTypeOption(typeof(ConnectionTypes), "Http")]
public sealed class HttpConnectionType :
    ConnectionTypeBase<IGenericConnection, HttpConfiguration, IHttpConnectionFactory>
{
    public static HttpConnectionType Instance { get; } = new();

    private HttpConnectionType() : base(5, "Http", "Web") { }

    public override void Register(IServiceCollection services)
    {
        // Register HttpClientFactory (shared by ALL translators!)
        services.AddHttpClient("FractalDataWorksHttp");

        // Register connection with specific translator
        services.AddScoped<HttpConnection<RestTranslator>>();

        // Register translators
        services.AddScoped<RestTranslator>();
        services.AddScoped<GraphQLTranslator>();
    }

    public override IReadOnlyList<string> SupportedTranslators =>
        ["Rest", "GraphQL"];
}
```

## 7. Common Patterns

### Connection Pooling

```csharp
// For database connections
public class MsSqlConnection : ConnectionBase<MsSqlConfiguration>
{
    // Use ADO.NET's built-in connection pooling
    private readonly string _connectionString;

    // Connection pool configured in connection string:
    // "...;Min Pool Size=5;Max Pool Size=100;Pooling=true"
}
```

### Retry Logic

```csharp
public async Task<IGenericResult<TOut>> Execute<TOut>(
    IConnectionCommand command,
    CancellationToken ct)
{
    if (!_configuration.EnableRetryLogic)
    {
        return await ExecuteInternal<TOut>(command, ct);
    }

    var retryPolicy = Policy
        .Handle<TransientException>()
        .WaitAndRetryAsync(
            _configuration.MaxRetryAttempts,
            retryAttempt => TimeSpan.FromMilliseconds(
                _configuration.RetryDelayMilliseconds * Math.Pow(2, retryAttempt)
            )
        );

    return await retryPolicy.ExecuteAsync(
        async () => await ExecuteInternal<TOut>(command, ct)
    );
}
```

### Circuit Breaker

```csharp
public class HttpConnection : ConnectionBase<HttpConfiguration>
{
    private readonly ICircuitBreakerPolicy _circuitBreaker;

    public HttpConnection(/*...*/)
    {
        _circuitBreaker = Policy
            .Handle<HttpRequestException>()
            .CircuitBreakerAsync(
                handledEventsAllowedBeforeBreaking: 5,
                durationOfBreak: TimeSpan.FromMinutes(1)
            );
    }
}
```

### Authentication/Authorization

```csharp
public sealed class HttpConfiguration : ConfigurationBase<HttpConfiguration>
{
    public AuthenticationType AuthType { get; init; } = AuthenticationType.None;
    public string? ApiKey { get; init; }
    public string? BearerToken { get; init; }
    public OAuth2Config? OAuth2 { get; init; }
}

public class HttpConnection : ConnectionBase<HttpConfiguration>
{
    private async Task AddAuthentication(HttpRequestMessage request)
    {
        switch (_configuration.AuthType)
        {
            case AuthenticationType.ApiKey:
                request.Headers.Add("X-API-Key", _configuration.ApiKey);
                break;

            case AuthenticationType.Bearer:
                request.Headers.Authorization =
                    new AuthenticationHeaderValue("Bearer", _configuration.BearerToken);
                break;

            case AuthenticationType.OAuth2:
                var token = await GetOAuth2Token();
                request.Headers.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
                break;
        }
    }
}
```

## 8. Common Mistakes

### MISTAKE 1: Connection Owning Translator (Old Pattern - WRONG!)

```csharp
// WRONG: Connection creates/owns translator
public class MsSqlConnection : ConnectionBase<MsSqlConfiguration>
{
    private readonly TSqlTranslator _translator = new TSqlTranslator(); // Wrong!
}

// CORRECT: Connection receives translator via DI
public class MsSqlConnection<TTranslator> : ConnectionBase<MsSqlConfiguration>
    where TTranslator : IDataCommandTranslator
{
    private readonly TTranslator _translator;

    public MsSqlConnection(TTranslator translator) // Injected!
    {
        _translator = translator;
    }
}
```

### MISTAKE 2: Not Using IConnectionCommand

```csharp
// WRONG: Execute method accepts raw SQL string
public async Task<IGenericResult<T>> Execute<T>(string sql, Dictionary<string, object> parameters)
{
    // This bypasses the command pattern and translator architecture
}

// CORRECT: Execute method accepts IConnectionCommand
public async Task<IGenericResult<T>> Execute<T>(IConnectionCommand command)
{
    if (command is SqlConnectionCommand sqlCommand)
    {
        // Execute sqlCommand.SqlText with sqlCommand.Parameters
    }
}
```

### MISTAKE 3: Missing State Management

```csharp
// WRONG: No state tracking
public class MsSqlConnection
{
    public async Task Execute(IConnectionCommand command)
    {
        // Opens connection every time, no state validation
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        // ...
    }
}

// CORRECT: Proper state management
public class MsSqlConnection
{
    private IConnectionState _state = ConnectionStates.Created;

    public async Task<IGenericResult> OpenAsync()
    {
        if (_state != ConnectionStates.Created && _state != ConnectionStates.Closed)
        {
            return GenericResult.Failure("Invalid state for Open operation");
        }

        _state = ConnectionStates.Opening;
        // ... open logic
        _state = ConnectionStates.Open;
    }
}
```

### MISTAKE 4: Improper Disposal

```csharp
// WRONG: Resources not disposed
public class MsSqlConnection : ConnectionBase<MsSqlConfiguration>
{
    private SqlConnection _dbConnection;

    // No IDisposable implementation, resources leak!
}

// CORRECT: Proper disposal pattern
public class MsSqlConnection : ConnectionBase<MsSqlConfiguration>, IDisposable
{
    private SqlConnection _dbConnection;
    private bool _disposed;

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _dbConnection?.Dispose();
            }
            _disposed = true;
        }
    }
}
```

### MISTAKE 5: Translators in Separate Project

```csharp
// WRONG: Translators as separate project
FractalDataWorks.Services.Translators.Sql/
    └── TSqlTranslator.cs  // Wrong location!

// CORRECT: Translators with connection implementation
FractalDataWorks.Services.Connections.MsSql/
    └── Translators/
        └── TSqlTranslator.cs  // Correct location!
```

## 9. Testing Connections

### Unit Tests

```csharp
public class MsSqlConnectionTests
{
    [Fact]
    public async Task Execute_ValidSqlCommand_ReturnsSuccess()
    {
        // Arrange
        var mockTranslator = new Mock<IDataCommandTranslator>();
        var config = new MsSqlConfiguration { ConnectionString = "..." };
        var connection = new MsSqlConnection<TSqlTranslator>(
            Mock.Of<ILogger>(),
            config,
            mockTranslator.Object
        );

        var command = new SqlConnectionCommand("SELECT * FROM Users", null);

        // Act
        var result = await connection.Execute<User[]>(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }
}
```

### Integration Tests

```csharp
public class MsSqlConnectionIntegrationTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _fixture;

    [Fact]
    public async Task Execute_RealDatabase_ReturnsData()
    {
        // Arrange
        var config = new MsSqlConfiguration
        {
            ConnectionString = _fixture.ConnectionString
        };

        var connection = new MsSqlConnection<TSqlTranslator>(
            Mock.Of<ILogger>(),
            config,
            new TSqlTranslator()
        );

        // Act
        var result = await connection.Execute<User[]>(
            new SqlConnectionCommand("SELECT * FROM Users WHERE IsActive = @active",
                new Dictionary<string, object> { ["active"] = true }
            ),
            CancellationToken.None
        );

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();
    }
}
```

## 10. Checklist for New Connection Implementation

- [ ] Create Abstractions project with interfaces
- [ ] Create Implementation project structure
- [ ] Define IConnectionCommand for the protocol
- [ ] Implement Configuration class with validation
- [ ] Implement Connection class with state management
- [ ] Implement Connection<TTranslator> generic version
- [ ] Create Translators/ folder in implementation project
- [ ] Implement at least one translator
- [ ] Implement ConnectionType with registration logic
- [ ] Implement ConnectionFactory
- [ ] Add proper disposal pattern
- [ ] Add transaction support (if applicable)
- [ ] Add retry logic (if applicable)
- [ ] Add circuit breaker (if applicable)
- [ ] Add authentication/authorization (if applicable)
- [ ] Write unit tests
- [ ] Write integration tests
- [ ] Document translator capabilities
- [ ] Update ConnectionTypes enum/collection

## Summary

Creating a FractalDataWorks Connection requires:

1. **Understanding the inverted architecture**: Connections OWN translators
2. **Defining protocol-specific commands**: IConnectionCommand implementations
3. **Implementing state management**: Track connection lifecycle
4. **Supporting multiple translators**: Via generic Connection<TTranslator> pattern
5. **Proper resource management**: IDisposable, connection pooling
6. **Testing thoroughly**: Unit and integration tests

**Key Principle**: A connection is a thin protocol layer that executes commands. The translator handles command language translation. The connection handles protocol execution.
