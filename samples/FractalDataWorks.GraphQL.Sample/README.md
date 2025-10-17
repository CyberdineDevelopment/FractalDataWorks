# FractalDataWorks GraphQL Connection Sample

Complete sample application demonstrating the FractalDataWorks GraphQL connection implementation.

## Overview

This sample demonstrates:
- ✅ GraphQL queries with type-safe responses
- ✅ Variables and operation names
- ✅ Nested field selection
- ✅ Mutations (conceptual)
- ✅ GraphQL error handling
- ✅ Query complexity management
- ✅ Automatic retry logic
- ✅ JSON serialization/deserialization

## Prerequisites

- **.NET 10.0 SDK or later**
- **Internet connection** for accessing GraphQL APIs

## Quick Start

```bash
dotnet run
```

The sample uses the public Countries GraphQL API (https://countries.trevorblades.com).

## Configuration

### appsettings.json

```json
{
  "GraphQLConnection": {
    "EndpointUrl": "https://countries.trevorblades.com/graphql",
    "TimeoutSeconds": 30,
    "RetryCount": 3,
    "MaxQueryDepth": 10,
    "MaxQueryComplexity": 0,
    "UsePersistedQueries": false
  }
}
```

### GraphQLConnectionConfiguration Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| EndpointUrl | string | Required | GraphQL endpoint URL |
| SubscriptionEndpointUrl | string | null | WebSocket URL for subscriptions |
| TimeoutSeconds | int | 30 | Request timeout |
| RetryCount | int | 3 | Retry attempts |
| MaxQueryDepth | int | 10 | Max nested query depth |
| MaxQueryComplexity | int | 0 | Max query complexity (0 = unlimited) |
| UsePersistedQueries | bool | false | Use persisted queries |
| UseAutomaticPersistedQueries | bool | false | Use APQ |
| EnableIntrospection | bool | false | Allow introspection |
| UseBatching | bool | false | Batch multiple queries |

## Code Examples

### Example 1: Simple Query

```csharp
var query = @"
    query {
        countries {
            code
            name
            capital
        }
    }";

var command = new GraphQLConnectionCommand(query);
var result = await service.Execute<CountriesResponse>(command, cancellationToken);
```

### Example 2: Query with Variables

```csharp
var query = @"
    query GetCountry($code: ID!) {
        country(code: $code) {
            name
            capital
            currency
        }
    }";

var variables = new Dictionary<string, object>
{
    ["code"] = "US"
};

var command = new GraphQLConnectionCommand(query, variables, "GetCountry");
var result = await service.Execute<CountryResponse>(command, cancellationToken);
```

### Example 3: Nested Fields

```csharp
var query = @"
    query {
        continents {
            name
            countries {
                code
                name
                languages {
                    name
                }
            }
        }
    }";

var command = new GraphQLConnectionCommand(query);
var result = await service.Execute<ContinentsResponse>(command, cancellationToken);
```

### Example 4: Mutation

```csharp
var mutation = @"
    mutation CreatePost($title: String!, $content: String!) {
        createPost(title: $title, content: $content) {
            id
            title
            content
            createdAt
        }
    }";

var variables = new Dictionary<string, object>
{
    ["title"] = "New Post",
    ["content"] = "Post content here"
};

var command = new GraphQLConnectionCommand(mutation, variables);
var result = await service.Execute<CreatePostResponse>(command, cancellationToken);
```

### Example 5: Custom Headers

```csharp
var headers = new Dictionary<string, string>
{
    ["Authorization"] = "Bearer your-token",
    ["X-Request-ID"] = Guid.NewGuid().ToString()
};

var command = new GraphQLConnectionCommand(query, variables, null, headers);
```

## Response Models

Define C# models matching your GraphQL schema:

```csharp
public sealed class CountriesResponse
{
    public List<Country> Countries { get; set; } = new();
}

public sealed class Country
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Capital { get; set; }
}
```

## Error Handling

### GraphQL Errors

GraphQL can return data AND errors:

```csharp
var result = await service.Execute<Response>(command, cancellationToken);

if (!result.IsSuccess)
{
    // GraphQL errors are included in the error message
    Console.WriteLine($"GraphQL Error: {result.CurrentMessage}");
}
```

### Partial Results

GraphQL may return partial data with errors. The service treats any GraphQL errors as failures.

## Authentication

### Bearer Token

```csharp
var config = new GraphQLConnectionConfiguration
{
    Headers = new Dictionary<string, string>
    {
        ["Authorization"] = "Bearer your-access-token"
    }
};
```

### API Key

```csharp
var config = new GraphQLConnectionConfiguration
{
    Headers = new Dictionary<string, string>
    {
        ["X-API-Key"] = "your-api-key"
    }
};
```

## Advanced Features

### Persisted Queries

Enable persisted queries to reduce bandwidth:

```csharp
var config = new GraphQLConnectionConfiguration
{
    UsePersistedQueries = true
};
```

### Query Complexity

Limit query complexity to prevent abuse:

```csharp
var config = new GraphQLConnectionConfiguration
{
    MaxQueryDepth = 5,
    MaxQueryComplexity = 100
};
```

### Batching

Send multiple queries in one request:

```csharp
var config = new GraphQLConnectionConfiguration
{
    UseBatching = true
};
```

## Common Patterns

### Aliases

```graphql
query {
    usa: country(code: "US") { name }
    canada: country(code: "CA") { name }
}
```

### Fragments

```graphql
fragment countryFields on Country {
    code
    name
    capital
}

query {
    countries {
        ...countryFields
    }
}
```

### Inline Fragments

```graphql
query {
    search(term: "test") {
        ... on User { username }
        ... on Post { title }
    }
}
```

## Dependency Injection

```csharp
services.AddHttpClient<GraphQLService>();
services.AddSingleton(graphQLConfig);
services.AddScoped<GraphQLService>();
```

## Additional Resources

- [GraphQL Official Documentation](https://graphql.org/learn/)
- [Countries GraphQL API](https://github.com/trevorblades/countries)
- [GraphQL Best Practices](https://graphql.org/learn/best-practices/)

## License

This sample is part of the FractalDataWorks project.
