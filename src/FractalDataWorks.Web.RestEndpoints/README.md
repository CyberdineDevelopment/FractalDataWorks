# FractalDataWorks.Web.RestEndpoints

This package provides concrete implementations of REST endpoint patterns using FastEndpoints, building on the abstractions defined in `FractalDataWorks.Web.Http.Abstractions`.

## Features

- **FastEndpoints Integration**: Built on the high-performance FastEndpoints framework
- **FractalDataWorks Pattern Compliance**: Implements FractalDataWorks web service patterns and conventions
- **Configuration-Driven**: Extensive configuration options for endpoint behavior
- **Security Integration**: Built-in security method implementations
- **Pagination Support**: Comprehensive pagination with streaming and paged responses

## Core Components

### Base Endpoint Classes
- **FractalEndpoint**: Base class for endpoints returning `FdwResult<T>` responses
- **QueryEndpoint**: Specialized base for read-only query operations
- **CrudEndpoint**: Base for Create, Read, Update, Delete operations
- **FileEndpoint**: Base for file upload/download operations

### Configuration System
- **WebConfiguration**: Main configuration class for web services
- **SecurityConfiguration**: Security-related configuration options
- **EndpointDefaults**: Default settings for different endpoint types
- **ApiKeySecurityConfiguration**: API key authentication configuration

### Pagination
- **PagedRequest**: Request model for paginated data
- **PagedResponse<T>**: Response model for paginated results
- **StreamingRequest**: Request model for streaming data
- **StreamingResponse<T>**: Response model for streaming results

## Installation

```xml
<PackageReference Include="FractalDataWorks.Web.RestEndpoints" Version="1.0.0" />
```

## Usage

### Basic Endpoint Implementation

```csharp
public class GetUsersEndpoint : QueryEndpoint<GetUsersRequest, PagedResponse<User>>
{
    public override void Configure()
    {
        Get("/users");
        Summary(s => s.Summary = "Get paginated list of users");
    }

    public override async Task<PagedResponse<User>> ExecuteAsync(GetUsersRequest req, CancellationToken ct)
    {
        // Implementation here
        return new PagedResponse<User>();
    }
}
```

### Configuration Setup

```csharp
public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddFractalDataWorksWeb(configuration =>
        {
            configuration.EnableApiKeyAuthentication();
            configuration.ConfigureRateLimiting();
            configuration.EnablePagination();
        });
    }

    public void Configure(IApplicationBuilder app)
    {
        app.UseFractalDataWorksWeb();
        app.UseFractalDataWorksEndpoints();
    }
}
```

## Extension Methods

- **UseFractalDataWorksWeb()**: Core FractalDataWorks web middleware setup
- **UseFractalDataWorksEndpoints()**: Endpoint discovery and registration  
- **UseFractalDataWorksAuthentication()**: Authentication middleware setup
- **UseFractalDataWorksRateLimiting()**: Rate limiting middleware setup

## Integration

- Builds on `FractalDataWorks.Web.Http.Abstractions` for type definitions
- Integrates with `FractalDataWorks.Configuration` for configuration management
- Uses `FractalDataWorks.Services` for service layer patterns
- Supports `FractalDataWorks.Results` for consistent response patterns

## Build Status

This project builds successfully with warnings related to TODO items and string comparisons.

## Current Implementation Status

- **Core Infrastructure**: ✅ Implemented
- **Base Endpoint Classes**: ✅ Implemented  
- **Configuration System**: ⚠️ Partially implemented (many TODOs)
- **Security Integration**: ⚠️ TODO items remain
- **Middleware Pipeline**: ⚠️ TODO items remain
- **Pagination**: ✅ Implemented

## Test Coverage

Current test status: Basic functionality tested
- Configuration validation tests
- Pagination request/response tests  
- Base endpoint functionality tests