# FractalDataWorks.Services.DataGateway

## Overview

The DataGateway framework provides ServiceType auto-discovery for data access gateways with unified interfaces that work across different data sources and storage systems.

## Features

- **ServiceType Auto-Discovery**: Add data gateway packages and they're automatically registered
- **Universal Data Access Interface**: Same API works with all data sources
- **Dynamic Gateway Creation**: Data gateways created via factories
- **Source-Generated Collections**: High-performance gateway lookup

## Quick Start

### 1. Install Packages

```xml
<ProjectReference Include="..\FractalDataWorks.Services.DataGateway\FractalDataWorks.Services.DataGateway.csproj" />
<ProjectReference Include="..\FractalDataWorks.Services.DataGateway.Entity\FractalDataWorks.Services.DataGateway.Entity.csproj" />
```

### 2. Register Services

```csharp
// Program.cs - Zero-configuration registration
builder.Services.AddScoped<IGenericDataGatewayProvider, GenericDataGatewayProvider>();

// Single line registers ALL discovered data gateway types
DataGatewayTypes.Register(builder.Services);
```

### 3. Configure Data Gateways

```json
{
  "DataGateways": {
    "EntityFramework": {
      "GatewayType": "Entity",
      "ConnectionString": "Server=localhost;Database=MyApp;Integrated Security=true;",
      "EnableChangeTracking": true,
      "QueryTimeout": 30
    }
  }
}
```

### 4. Use Universal Data Access

```csharp
public class UserRepository
{
    private readonly IGenericDataGatewayProvider _gatewayProvider;

    public UserRepository(IGenericDataGatewayProvider gatewayProvider)
    {
        _gatewayProvider = gatewayProvider;
    }

    public async Task<IGenericResult<List<User>>> GetActiveUsersAsync()
    {
        var gatewayResult = await _gatewayProvider.GetDataGateway("EntityFramework");
        if (!gatewayResult.IsSuccess)
            return GenericResult<List<User>>.Failure(gatewayResult.Error);

        using var gateway = gatewayResult.Value;

        // Universal data access - works with any gateway
        var query = new DataQuery<User>()
            .Where(u => u.IsActive)
            .OrderBy(u => u.LastName);

        var result = await gateway.QueryAsync(query);
        return result;
    }

    public async Task<IGenericResult<User>> CreateUserAsync(User user)
    {
        var gatewayResult = await _gatewayProvider.GetDataGateway("EntityFramework");
        if (!gatewayResult.IsSuccess)
            return GenericResult<User>.Failure(gatewayResult.Error);

        using var gateway = gatewayResult.Value;

        var result = await gateway.CreateAsync(user);
        return result;
    }
}
```

## Available Data Gateway Types

| Package | Gateway Type | Purpose |
|---------|-------------|---------|
| `FractalDataWorks.Services.DataGateway.Entity` | Entity | Entity Framework Core integration |
| `FractalDataWorks.Services.DataGateway.Dapper` | Dapper | Dapper micro-ORM integration |
| `FractalDataWorks.Services.DataGateway.MongoDb` | MongoDb | MongoDB document database |

## How Auto-Discovery Works

1. **Source Generator Scans**: `[ServiceTypeCollection]` attribute triggers compile-time discovery
2. **Finds Implementations**: Scans referenced assemblies for types inheriting from `DataGatewayTypeBase`
3. **Generates Collections**: Creates `DataGatewayTypes.All`, `DataGatewayTypes.Name()`, etc.
4. **Self-Registration**: Each data gateway type handles its own DI registration

## Adding Custom Data Gateway Types

```csharp
// 1. Create your data gateway type (singleton pattern)
public sealed class CustomDataGatewayType : DataGatewayTypeBase<IGenericDataGateway, CustomDataGatewayConfiguration, ICustomDataGatewayFactory>
{
    public static CustomDataGatewayType Instance { get; } = new();

    private CustomDataGatewayType() : base(4, "Custom", "Data Gateways") { }

    public override Type FactoryType => typeof(ICustomDataGatewayFactory);

    public override void Register(IServiceCollection services)
    {
        services.AddScoped<ICustomDataGatewayFactory, CustomDataGatewayFactory>();
        services.AddScoped<CustomQueryBuilder>();
        services.AddScoped<CustomDataMapper>();
    }
}

// 2. Add package reference - source generator automatically discovers it
// 3. DataGatewayTypes.Register(services) will include it automatically
```

## Common Data Access Patterns

### Repository Pattern

```csharp
public class ProductRepository : IProductRepository
{
    private readonly IGenericDataGatewayProvider _gatewayProvider;

    public ProductRepository(IGenericDataGatewayProvider gatewayProvider)
    {
        _gatewayProvider = gatewayProvider;
    }

    public async Task<IGenericResult<Product>> GetByIdAsync(int id)
    {
        var gatewayResult = await _gatewayProvider.GetDataGateway("EntityFramework");
        if (!gatewayResult.IsSuccess)
            return GenericResult<Product>.Failure(gatewayResult.Error);

        using var gateway = gatewayResult.Value;
        return await gateway.GetByIdAsync<Product>(id);
    }

    public async Task<IGenericResult<List<Product>>> SearchAsync(string searchTerm)
    {
        var gatewayResult = await _gatewayProvider.GetDataGateway("EntityFramework");
        if (!gatewayResult.IsSuccess)
            return GenericResult<List<Product>>.Failure(gatewayResult.Error);

        using var gateway = gatewayResult.Value;

        var query = new DataQuery<Product>()
            .Where(p => p.Name.Contains(searchTerm) || p.Description.Contains(searchTerm))
            .OrderBy(p => p.Name);

        return await gateway.QueryAsync(query);
    }
}
```

### Unit of Work Pattern

```csharp
public class OrderService
{
    private readonly IGenericDataGatewayProvider _gatewayProvider;

    public OrderService(IGenericDataGatewayProvider gatewayProvider)
    {
        _gatewayProvider = gatewayProvider;
    }

    public async Task<IGenericResult<Order>> CreateOrderAsync(CreateOrderRequest request)
    {
        var gatewayResult = await _gatewayProvider.GetDataGateway("EntityFramework");
        if (!gatewayResult.IsSuccess)
            return GenericResult<Order>.Failure(gatewayResult.Error);

        using var gateway = gatewayResult.Value;

        // Begin transaction
        using var transaction = await gateway.BeginTransactionAsync();

        try
        {
            // Create order
            var order = new Order { CustomerId = request.CustomerId, OrderDate = DateTimeOffset.UtcNow };
            var orderResult = await gateway.CreateAsync(order);
            if (!orderResult.IsSuccess)
                return GenericResult<Order>.Failure(orderResult.Error);

            // Create order items
            foreach (var item in request.Items)
            {
                var orderItem = new OrderItem { OrderId = orderResult.Value.Id, ProductId = item.ProductId, Quantity = item.Quantity };
                var itemResult = await gateway.CreateAsync(orderItem);
                if (!itemResult.IsSuccess)
                {
                    await transaction.RollbackAsync();
                    return GenericResult<Order>.Failure(itemResult.Error);
                }
            }

            await transaction.CommitAsync();
            return orderResult;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return GenericResult<Order>.Failure($"Order creation failed: {ex.Message}");
        }
    }
}
```

## Architecture Benefits

- **Gateway Agnostic**: Switch data access strategies without code changes
- **Zero Configuration**: Add package reference, get functionality
- **Type Safety**: Compile-time validation of gateway types
- **Performance**: Source-generated collections use FrozenDictionary
- **Transactional**: Built-in transaction and unit of work support

For complete architecture details, see [Services.Abstractions README](../FractalDataWorks.Services.Abstractions/README.md).