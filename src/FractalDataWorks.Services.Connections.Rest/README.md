# FractalDataWorks.Services.Connections.Rest

REST API external connection service implementation for the FractalDataWorks platform.

## Overview

This project provides a REST-specific implementation of HTTP-based external connections. It extends the HTTP abstractions to provide REST API connectivity with JSON support, automatic retries, SSL certificate validation options, and comprehensive HTTP client management.

## Key Components

### Core Classes

#### `RestService`
- **Type**: `sealed partial class`
- **Base Class**: `HttpServiceBase<RestConnectionConfiguration>`
- **Purpose**: Manages REST API connections and command execution
- **Key Features**:
  - Named HttpClient creation with factory pattern
  - Comprehensive HTTP configuration (headers, timeouts, redirects)
  - Exception handling for HTTP-specific errors (timeouts, request failures)
  - Built-in logging with high-performance LoggerMessage delegates
  - SSL certificate validation controls

#### `RestConnectionConfiguration`
- **Type**: `sealed class`
- **Base Class**: `HttpConnectionConfigurationBase<RestConnectionConfiguration>`
- **Purpose**: Configuration for REST API connections
- **Key Properties**:
  - `ContentType` - Default content type ("application/json")
  - `AcceptHeader` - Accept header value ("application/json")
  - `UserAgent` - User-Agent header ("FractalDataWorks-REST-Client/1.0")
  - `AllowAutoRedirect` - Follow HTTP redirects automatically (true)
  - `MaxAutomaticRedirections` - Maximum redirect count (5)
  - `UseCompression` - Enable compression for requests/responses (true)
  - `DefaultQueryParameters` - Query parameters for all requests
  - `RetryCount` - Number of retry attempts (3)
  - `RetryDelayMilliseconds` - Delay between retries (1000ms)
  - `ValidateSslCertificate` - SSL certificate validation (true)

#### `RestConnectionType`
- **Type**: `sealed class`
- **Base Class**: `HttpConnectionTypeBase<RestService, RestConnectionConfiguration, IServiceFactory<RestService, RestConnectionConfiguration>>`
- **Interfaces**: `IEnumOption<RestConnectionType>`
- **Purpose**: Enhanced enum defining REST connection type metadata
- **Attributes**: `[EnumOption("REST")]`
- **Properties**:
  - ID: 1
  - Name: "REST"
  - Protocol: "HTTP"
  - Supports Pooling: true
  - Supports Transactions: false
  - Default Port: 80

### Supporting Components

#### `RestServiceLog`
- **Type**: `static partial class`
- **Purpose**: High-performance logging using source generators
- **Exclusion**: `[ExcludeFromCodeCoverage]` - Source-generated logging class
- **Events**:
  - ExecutingCommand - Command execution start
  - CommandSuccess - Successful command completion
  - HttpRequestFailed - HTTP request failures
  - RequestTimeout - Request timeout errors
  - UnexpectedError - Unexpected exceptions
  - ServiceDisposed - Service disposal
  - CreatingService - Service instance creation
  - ConfiguringHttpClient - HttpClient configuration

#### `RestProtocol` (Enhanced Enum)
- **Location**: `EnhancedEnums/Protocols/`
- **Purpose**: Protocol definition for REST services

## Dependencies

### Project References
- `FractalDataWorks.Services.Connections.Abstractions` - External connection contracts
- `FractalDataWorks.Services.Connections.Http.Abstractions` - HTTP connection abstractions
- `FractalDataWorks.EnhancedEnums` - Enhanced enum support
- `FractalDataWorks.Services` - Base service implementations
- `FractalDataWorks.Services.Abstractions` - Service abstractions
- `FractalDataWorks.EnhancedEnums.SourceGenerators` - Build-time enum generation

### Framework Dependencies
- `Microsoft.Extensions.Http` - HttpClientFactory support
- `Microsoft.Extensions.DependencyInjection` - Dependency injection
- `Microsoft.Extensions.Logging` - Logging abstractions

## Usage Patterns

### Service Registration
```csharp
services.Configure<RestConnectionConfiguration>(config =>
{
    config.BaseUrl = "https://api.example.com";
    config.TimeoutSeconds = 30;
    config.ValidateSslCertificate = true;
    config.RetryCount = 3;
    config.Headers = new Dictionary<string, string>
    {
        ["Authorization"] = "Bearer token-here",
        ["Custom-Header"] = "custom-value"
    };
});

// Register named HttpClient with custom configuration
RestService.ConfigureHttpClient(services, "MyRestClient", restConfig);

services.AddSingleton<RestService>();
```

### Basic Usage
```csharp
var restService = serviceProvider.GetRequiredService<RestService>();

// Execute REST command
var command = new MyRestCommand { /* command properties */ };
var result = await restService.Execute<MyResponseType>(command, cancellationToken);

if (result.IsSuccess)
{
    var response = result.Value;
    // Handle successful response
}
else
{
    // Handle error: result.Message contains error details
}
```

### HttpClient Configuration
```csharp
var config = new RestConnectionConfiguration
{
    BaseUrl = "https://api.example.com",
    TimeoutSeconds = 60,
    ContentType = "application/json",
    AcceptHeader = "application/json",
    UserAgent = "MyApp/2.0",
    AllowAutoRedirect = true,
    MaxAutomaticRedirections = 3,
    ValidateSslCertificate = false, // For development only
    RetryCount = 5,
    RetryDelayMilliseconds = 2000,
    DefaultQueryParameters = new Dictionary<string, string>
    {
        ["api-version"] = "v2",
        ["format"] = "json"
    }
};
```

## Architecture Notes

### HTTP Client Management
- Uses IHttpClientFactory for managed HttpClient instances
- Named clients prevent socket exhaustion
- Automatic connection pooling and lifecycle management
- Configurable message handlers for advanced scenarios

### Error Handling
The service provides comprehensive error handling for:
- `HttpRequestException` - Network and HTTP protocol errors
- `TaskCanceledException` with `TimeoutException` - Request timeouts
- General exceptions - Unexpected errors during command execution

### SSL/TLS Configuration
- Configurable SSL certificate validation
- Custom certificate validation callbacks
- Support for self-signed certificates in development environments

### Retry Logic
- Configurable retry attempts and delays
- Exponential backoff can be implemented in derived classes
- HTTP-specific retry policies for transient failures

## Implementation Status

### Current Implementation
- Basic service structure and configuration
- HttpClient factory integration
- Error handling and logging infrastructure
- Configuration management with validation

### Placeholder Implementation
The `ProcessRestCommand<TOut>` method currently contains placeholder logic that:
- Simulates async work with `Task.Delay(1)`
- Returns hardcoded success results based on expected type
- Does not perform actual HTTP requests

### TODO: Full Implementation
A complete implementation would include:
1. **Command Parsing**: Extract HTTP method, endpoint, body, and headers from commands
2. **HTTP Request Execution**: Make actual HTTP requests using the configured HttpClient
3. **Response Handling**: Parse response content with appropriate deserialization
4. **Content Type Support**: Handle various content types (JSON, XML, form data, etc.)
5. **Retry Logic**: Implement configurable retry policies for transient failures
6. **Authentication**: Support for various authentication schemes (Bearer, Basic, etc.)
7. **Request/Response Middleware**: Pipeline for request/response transformation

## Code Coverage Exclusions

The following should be excluded from coverage testing:

### Source-Generated Code
- `RestServiceLog` class - Marked with `[ExcludeFromCodeCoverage]` as it contains only source-generated logging methods with no business logic

### Integration-Dependent Code
- HttpClient configuration and factory usage - Requires integration testing with actual HTTP endpoints
- SSL certificate validation callbacks - Requires certificate-specific test scenarios

## Security Features

- SSL/TLS certificate validation (configurable)
- Secure credential handling through headers
- User-Agent identification for API tracking
- Configurable timeout protections
- Request header sanitization capabilities

## Performance Features

- HttpClient reuse through factory pattern
- Connection pooling via HttpClientFactory
- High-performance logging with source generators
- Async/await throughout for non-blocking operations
- Configurable compression support

## Logging

Source-generated high-performance logging for:
- Command execution lifecycle
- HTTP request success/failure tracking
- Timeout and error conditions
- Service lifecycle events
- HttpClient configuration events

All logging uses structured logging with consistent event IDs for monitoring and troubleshooting.