# FractalDataWorks REST Connection Sample

Complete sample application demonstrating the FractalDataWorks REST API connection implementation with real-world examples.

## Overview

This sample demonstrates:
- ✅ REST configuration with HttpClient integration
- ✅ GET requests with and without query parameters
- ✅ POST requests with JSON body
- ✅ PUT and PATCH requests for updates
- ✅ DELETE requests
- ✅ Custom headers and authentication
- ✅ Automatic retry logic for transient failures
- ✅ Type-safe JSON serialization/deserialization
- ✅ Error handling and timeout management

## Prerequisites

- **.NET 10.0 SDK or later**
  - Download: https://dotnet.microsoft.com/download
- **Internet connection** for accessing JSONPlaceholder API

## Quick Start

### Run the Sample

```bash
dotnet run
```

The sample uses the public JSONPlaceholder API (https://jsonplaceholder.typicode.com) which is a free fake REST API for testing.

## Configuration

### appsettings.json

```json
{
  "RestConnection": {
    "BaseUrl": "https://jsonplaceholder.typicode.com",
    "TimeoutSeconds": 30,
    "ContentType": "application/json",
    "AcceptHeader": "application/json",
    "RetryCount": 3,
    "RetryDelayMilliseconds": 1000,
    "UseCompression": true,
    "ValidateSslCertificate": true
  }
}
```

### RestConnectionConfiguration Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| BaseUrl | string | Required | Base URL for the REST API |
| TimeoutSeconds | int | 30 | Request timeout in seconds |
| ContentType | string | "application/json" | Content-Type header |
| AcceptHeader | string | "application/json" | Accept header |
| RetryCount | int | 3 | Number of retry attempts |
| RetryDelayMilliseconds | int | 1000 | Delay between retries |
| UseCompression | bool | true | Enable HTTP compression |
| AllowAutoRedirect | bool | true | Follow redirects |
| MaxAutomaticRedirections | int | 5 | Max redirects to follow |
| ValidateSslCertificate | bool | true | Validate SSL certificates |

## Code Examples

### Example 1: GET Request

```csharp
var command = new RestConnectionCommand(
    HttpMethod.Get,
    "/posts"
);

var result = await service.Execute<List<Post>>(command, CancellationToken.None);

if (result.IsSuccess)
{
    foreach (var post in result.Value)
    {
        Console.WriteLine($"{post.Id}: {post.Title}");
    }
}
```

### Example 2: GET with Query Parameters

```csharp
var queryParams = new Dictionary<string, string>
{
    ["userId"] = "1",
    ["_limit"] = "5"
};

var command = new RestConnectionCommand(
    HttpMethod.Get,
    "/posts",
    queryParameters: queryParams
);

var result = await service.Execute<List<Post>>(command, CancellationToken.None);
```

### Example 3: POST Request

```csharp
var newPost = new Post
{
    UserId = 1,
    Title = "New Post",
    Body = "This is the post content."
};

var command = new RestConnectionCommand(
    HttpMethod.Post,
    "/posts",
    body: newPost
);

var result = await service.Execute<Post>(command, CancellationToken.None);
```

### Example 4: PUT Request

```csharp
var updatedPost = new Post
{
    Id = 1,
    UserId = 1,
    Title = "Updated Title",
    Body = "Updated content."
};

var command = new RestConnectionCommand(
    HttpMethod.Put,
    "/posts/1",
    body: updatedPost
);

var result = await service.Execute<Post>(command, CancellationToken.None);
```

### Example 5: PATCH Request

```csharp
var partialUpdate = new { Title = "Partially Updated" };

var command = new RestConnectionCommand(
    HttpMethod.Patch,
    "/posts/1",
    body: partialUpdate
);

var result = await service.Execute<Post>(command, CancellationToken.None);
```

### Example 6: DELETE Request

```csharp
var command = new RestConnectionCommand(
    HttpMethod.Delete,
    "/posts/1"
);

var result = await service.Execute<object>(command, CancellationToken.None);
```

### Example 7: Custom Headers

```csharp
var headers = new Dictionary<string, string>
{
    ["Authorization"] = "Bearer your-token-here",
    ["X-Custom-Header"] = "CustomValue"
};

var command = new RestConnectionCommand(
    HttpMethod.Get,
    "/posts/1",
    headers: headers
);

var result = await service.Execute<Post>(command, CancellationToken.None);
```

## Authentication

### Bearer Token Authentication

```csharp
var config = new RestConnectionConfiguration
{
    BaseUrl = "https://api.example.com",
    Headers = new Dictionary<string, string>
    {
        ["Authorization"] = "Bearer your-access-token"
    }
};
```

### API Key Authentication

```csharp
var config = new RestConnectionConfiguration
{
    BaseUrl = "https://api.example.com",
    Headers = new Dictionary<string, string>
    {
        ["X-API-Key"] = "your-api-key"
    }
};
```

## Error Handling

The REST service uses Railway-Oriented Programming with `IGenericResult<T>`:

```csharp
var result = await service.Execute<Post>(command, cancellationToken);

if (result.IsSuccess)
{
    var post = result.Value;
    // Process successful result
}
else
{
    // Handle error
    Console.WriteLine($"Error: {result.CurrentMessage}");
}
```

### Automatic Retry Logic

The service automatically retries transient HTTP failures:
- Connection timeouts
- HTTP 408 Request Timeout
- HTTP 429 Too Many Requests
- HTTP 500+ Server Errors

## Performance Tips

1. **Reuse HttpClient** - The service is designed to work with IHttpClientFactory
2. **Enable Compression** - Set `UseCompression = true`
3. **Use Async/Await** - Always use async methods
4. **Configure Timeouts** - Set appropriate `TimeoutSeconds`
5. **Implement Caching** - Cache frequently accessed data

## Common REST Patterns

### Pagination

```csharp
var queryParams = new Dictionary<string, string>
{
    ["_page"] = "1",
    ["_limit"] = "10"
};
```

### Filtering

```csharp
var queryParams = new Dictionary<string, string>
{
    ["userId"] = "1",
    ["_sort"] = "id",
    ["_order"] = "desc"
};
```

### Search

```csharp
var queryParams = new Dictionary<string, string>
{
    ["q"] = "search term"
};
```

## Dependency Injection

```csharp
services.AddHttpClient<RestService>();
services.AddSingleton(restConfig);
services.AddScoped<RestService>();
```

## Additional Resources

- [JSONPlaceholder API Guide](https://jsonplaceholder.typicode.com/guide/)
- [REST API Best Practices](https://restfulapi.net/)
- [HttpClient Best Practices](https://docs.microsoft.com/en-us/dotnet/fundamentals/networking/http/httpclient-guidelines)

## License

This sample is part of the FractalDataWorks project.
