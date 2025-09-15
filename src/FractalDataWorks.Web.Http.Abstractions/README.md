# FractalDataWorks.Web.Http.Abstractions

This package provides core abstractions and Enhanced Enum definitions for HTTP-based web services in the FractalDataWorks platform, offering structured approaches to endpoint types, security methods, and HTTP configurations.

## Features

- **Endpoint Type System**: Enhanced Enum-based endpoint categorization
- **Security Method Abstractions**: Comprehensive security method definitions
- **Rate Limiting Policies**: Enhanced Enum-based rate limiting configurations
- **HTTP Configuration Models**: Structured configuration for web services

## Core Components

### Endpoint Types
- **CRUD**: Create, Read, Update, Delete endpoints
- **File**: File upload/download endpoints  
- **Health**: Health check and monitoring endpoints
- **Event**: Event-driven and streaming endpoints
- **Query**: Read-only query endpoints

### Security Methods
- **API Key**: API key-based authentication
- **Bearer Token**: JWT and bearer token authentication
- **OAuth2**: OAuth 2.0 authentication flows
- **Basic Authentication**: HTTP basic authentication
- **Custom**: Extensible custom authentication methods

### Rate Limiting Policies
- **Fixed Window**: Fixed time window rate limiting
- **Sliding Window**: Sliding window rate limiting
- **Token Bucket**: Token bucket algorithm implementation
- **Concurrency**: Concurrency-based limiting

## Installation

```xml
<PackageReference Include="FractalDataWorks.Web.Http.Abstractions" Version="1.0.0" />
```

## Usage

```csharp
using FractalDataWorks.Web.Http.Abstractions.EndPoints;
using FractalDataWorks.Web.Http.Abstractions.Security;

// Configure endpoint types
var crudEndpoint = CRUD.Instance;
var fileEndpoint = File.Instance;

// Configure security methods
var apiKeyAuth = ApiKey.Instance;
var bearerAuth = BearerToken.Instance;

// Apply rate limiting
var slidingWindow = SlidingWindow.Instance;
```

## Enhanced Enum Collections

- **EndpointTypeCollection**: Complete collection of available endpoint types
- **SecurityMethodCollection**: All supported security methods
- **RateLimitPolicyCollection**: Available rate limiting policies

## Integration

- Used by `FractalDataWorks.Web.RestEndpoints` for concrete implementations
- Integrates with Enhanced Enum system for type safety
- Provides foundation for web service configuration

## Build Status

This project builds successfully with warnings related to dependency XML documentation.

## Test Coverage

Current test status: 156/171 tests passing (91.2%)
- Some tests failing due to workspace configuration issues
- Core functionality tests are passing
- Enhanced Enum functionality is well-covered