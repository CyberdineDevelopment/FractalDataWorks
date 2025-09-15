# Enhanced Enums Framework

## Table of Contents
- [Overview](#overview)
- [Configuration Decision Matrix](#configuration-decision-matrix)
- [Basic Implementation Pattern](#basic-implementation-pattern)
- [Advanced Features](#advanced-features)
- [Performance Characteristics](#performance-characteristics)
- [Global Assembly Discovery](#global-assembly-discovery)
- [Best Practices](#best-practices)
- [Integration Examples](#integration-examples)
- [Creating New Enhanced Enums](#creating-new-enhanced-enums)

## Overview
Enhanced Enums provide type-safe, object-oriented alternatives to traditional C# enums with rich behavior, business logic, and efficient lookups.

## Enhanced Enum System Configuration

The Enhanced Enum system provides simple configuration through the EnumCollection attribute properties.

### Configuration Options

| Property | Type | Purpose | Values |
|----------|------|---------|---------|
| **CollectionName** | string | Name of generated collection | Any valid C# identifier |
| **DefaultGenericReturnType** | Type | Return type for methods | Any valid .NET type |
| **UseSingletonInstances** | bool | Instance management | true (singletons) / false (factory methods) |

## Basic Implementation Pattern

### Step 1: Define Base Class
```csharp
public abstract class Priority : EnumOptionBase<Priority>
{
    protected Priority(int id, string name) : base(id, name) { }
    public abstract int Level { get; }
}
```

### Step 2: Define Enum Options
```csharp
[EnumOption("High")]
public sealed class HighPriority : Priority
{
    public HighPriority() : base(1, "High") { }
    public override int Level => 100;
}

[EnumOption("Medium")]
public sealed class MediumPriority : Priority
{
    public MediumPriority() : base(2, "Medium") { }
    public override int Level => 50;
}
```

### Step 3: Configure Collection Generation
```csharp
[EnumCollection(CollectionName = "Priorities", UseSingletonInstances = true)]
public abstract class PriorityCollection : EnumCollectionBase<Priority>
{
    // Source generator populates this automatically
}
```

## Advanced Features

### Abstract Methods Pattern
```csharp
public abstract class PaymentMethod : EnumOptionBase<PaymentMethod>
{
    protected PaymentMethod(int id, string name) : base(id, name) { }
    
    public abstract decimal ProcessingFee { get; }
    public abstract bool RequiresVerification { get; }
    public abstract string ProcessorName { get; }
}
```

### Lookup Properties
```csharp
public abstract class HttpStatusCode : EnumOptionBase<HttpStatusCode>
{
    [EnumLookup("GetByCode")]
    public int Code { get; }
    
    [EnumLookup("GetByCategory")]  
    public string Category { get; }
    
    protected HttpStatusCode(int id, string name, int code, string category) : base(id, name)
    {
        Code = code;
        Category = category;
    }
}
```

### Custom Return Types
```csharp
[EnumCollection(CollectionName = "SecurityMethods", UseSingletonInstances = true)]
public abstract class SecurityMethodCollectionBase : EnumCollectionBase<SecurityMethodBase>
{
    // Can set ReturnType and ReturnTypeNamespace properties for custom return types
}
```

## Performance Characteristics

### Current Implementation
- **Initialization**: O(n) - All enum instances created during static constructor
- **All() Method Access**: O(1) - Returns cached `ImmutableArray`
- **Name Lookups**: O(1) - Uses `FrozenDictionary` (.NET 8+) for fast lookups
- **ID Lookups**: O(1) - Uses dedicated dictionary for ID-based lookups
- **Custom Property Lookups**: O(1) - Uses dedicated dictionaries for each [EnumLookup] property
- **Memory Usage**: All instances stored in memory permanently (singleton pattern)

### Performance Benefits
- **Fast Lookups**: Dictionary-based lookups provide O(1) performance
- **Memory Efficient**: Singleton pattern ensures only one instance per enum value
- **Compile-time Safety**: All lookup methods are generated and type-safe
- **Zero Runtime Overhead**: Everything is generated at compile time

## Global Assembly Discovery

### Package Requirements
Enhanced Enums with global discovery must be referenced as **packages**, not project references:

**✅ Required:**
```xml
<PackageReference Include="MyEnumLibrary" Version="1.0.0" />
```

**❌ Not Supported:**
```xml
<ProjectReference Include="..\MyEnumLibrary\MyEnumLibrary.csproj" />
```

### Why Package References Are Required
The source generator uses **ILRepack** for dependency bundling:
1. **Resolved Package Dependencies**: Package references provide fully resolved assembly paths
2. **Dependency Chain Resolution**: NuGet metadata enables complete dependency graph analysis  
3. **Build-Time Assembly Loading**: Source generators need compiled assemblies, not source projects

## Best Practices

### 1. Choose the Right Pattern
- Use **EnumCollectionBase** for source-generated collections
- Use **static readonly** for rich business logic scenarios

### 2. Use Descriptive Names
```csharp
[EnumOption("AwaitingPayment")]
public class AwaitingPayment : OrderStatus 
{ 
    public AwaitingPayment() : base(1, "AwaitingPayment") { }
}
```

### 3. Keep Enums Focused
- Single responsibility per enum type
- Separate concerns into different enum hierarchies
- Use composition over complex inheritance

## Integration Examples

### Integration with Services

```csharp
public class OrderService : ServiceBase<OrderExecutor, OrderConfiguration>
{
    public async Task<IFdwResult<Order>> CreateOrderAsync(CreateOrderRequest request)
    {
        // Use Enhanced Enums for validation
        if (!OrderStatuses.IsValid(request.InitialStatus))
        {
            return FdwResult<Order>.Failure(OrderMessages.InvalidStatus(request.InitialStatus));
        }
        
        // Use Enhanced Enums for business logic
        var priority = Priorities.GetByName(request.PriorityLevel);
        if (priority != null && priority.RequiresApproval)
        {
            await RequestApprovalAsync(request);
        }
        
        var order = await Executor.CreateOrderAsync(request);
        return FdwResult<Order>.Success(order);
    }
}
```

### Integration with ASP.NET Core

```csharp
[ApiController]
[Route("api/[controller]")]
public class OrderController : ControllerBase
{
    private readonly IOrderService _orderService;
    
    [HttpPost]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request)
    {
        // Validate Enhanced Enum values from request
        if (!Priorities.TryGetByName(request.Priority, out var priority))
        {
            return BadRequest(new { Error = "Invalid priority level", ValidPriorities = Priorities.All().Select(p => p.Name) });
        }
        
        var result = await _orderService.CreateOrderAsync(request);
        
        return result.IsSuccess 
            ? Ok(result.Value)
            : BadRequest(new { Error = result.Message });
    }
    
    [HttpGet("priorities")]
    public IActionResult GetPriorities()
    {
        var priorities = Priorities.All().Select(p => new
        {
            p.Name,
            p.Level,
            p.Color,
            IsUrgent = p.Level > 75
        });
        
        return Ok(priorities);
    }
}
```

### Integration with Entity Framework

```csharp
public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.HasKey(o => o.Id);
        
        // Store Enhanced Enum by Name for database compatibility
        builder.Property(o => o.Status)
            .HasConversion(
                status => status.Name,
                name => OrderStatuses.GetByName(name) ?? OrderStatuses.Pending());
                
        builder.Property(o => o.Priority)
            .HasConversion(
                priority => priority.Name,
                name => Priorities.GetByName(name) ?? Priorities.Medium());
    }
}

public class Order
{
    public Guid Id { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public OrderStatus Status { get; set; } = OrderStatuses.Pending();
    public Priority Priority { get; set; } = Priorities.Medium();
    public DateTime CreatedAt { get; set; }
    
    // Business methods using Enhanced Enum logic
    public bool CanBeCancelled() => Status.AllowsCancellation;
    public TimeSpan GetEstimatedProcessingTime() => Priority.EstimatedProcessingTime;
}
```

### Integration with Messaging

```csharp
public class OrderEventHandler
{
    private readonly IMessagePublisher _messagePublisher;
    
    public async Task HandleOrderStatusChanged(Order order, OrderStatus previousStatus)
    {
        // Use Enhanced Enum business logic
        if (order.Status.RequiresNotification && !previousStatus.RequiresNotification)
        {
            await _messagePublisher.PublishAsync(new OrderStatusNotificationEvent
            {
                OrderId = order.Id,
                StatusName = order.Status.Name,
                StatusDisplayName = order.Status.DisplayName,
                NotificationLevel = order.Status.NotificationLevel,
                RequiresCustomerNotification = order.Status.RequiresCustomerNotification
            });
        }
        
        // Trigger priority-based processing
        if (order.Priority.RequiresImmediateProcessing)
        {
            await _messagePublisher.PublishAsync(new HighPriorityOrderEvent
            {
                OrderId = order.Id,
                Priority = order.Priority.Name,
                MaxProcessingTime = order.Priority.MaxAllowedProcessingTime
            });
        }
    }
}
```

### Integration with Validation

```csharp
public class CreateOrderRequestValidator : AbstractValidator<CreateOrderRequest>
{
    public CreateOrderRequestValidator()
    {
        RuleFor(x => x.Priority)
            .NotEmpty()
            .Must(priority => Priorities.GetByName(priority) != null)
            .WithMessage("Priority must be one of: {ValidPriorities}")
            .WithState(_ => new { ValidPriorities = string.Join(", ", Priorities.All().Select(p => p.Name)) });
            
        RuleFor(x => x.InitialStatus)
            .Must(status => OrderStatuses.GetByName(status)?.AllowsInitialCreation == true)
            .WithMessage("Initial status must allow order creation");
            
        RuleFor(x => x.PaymentMethod)
            .Must(method => PaymentMethods.GetByName(method) != null)
            .WithMessage(x => $"Payment method '{x.PaymentMethod}' is not supported");
    }
}
```

### Integration with Background Services

```csharp
public class OrderProcessingService : BackgroundService
{
    private readonly IOrderRepository _orderRepository;
    private readonly ILogger<OrderProcessingService> _logger;
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            // Process orders by priority
            var highPriorityOrders = await _orderRepository.GetOrdersByPriorityAsync(Priorities.High());
            await ProcessOrdersByPriority(highPriorityOrders, Priorities.High());
            
            var mediumPriorityOrders = await _orderRepository.GetOrdersByPriorityAsync(Priorities.Medium());
            await ProcessOrdersByPriority(mediumPriorityOrders, Priorities.Medium());
            
            // Process orders by status
            var pendingOrders = await _orderRepository.GetOrdersByStatusAsync(OrderStatuses.Pending());
            await ProcessPendingOrders(pendingOrders);
            
            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
    }
    
    private async Task ProcessOrdersByPriority(IEnumerable<Order> orders, Priority priority)
    {
        _logger.LogInformation("Processing {Count} orders with {Priority} priority (max processing time: {MaxTime})",
            orders.Count(), priority.Name, priority.MaxAllowedProcessingTime);
            
        foreach (var order in orders.Take(priority.MaxConcurrentProcessing))
        {
            await ProcessOrder(order);
        }
    }
}
```

## Creating New Enhanced Enums

1. **Define base abstract class** inheriting from `EnumOptionBase<T>`
2. **Add abstract properties/methods** that concrete options must implement
3. **Create concrete options** with `[EnumOption]` attribute
4. **Define collection class** with `[EnumCollection]` attribute
5. **Choose appropriate GenerationMode and StorageMode** based on usage patterns
6. **Add lookup properties** with `[EnumLookup]` for custom queries