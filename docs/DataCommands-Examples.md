# DataCommands Examples

## Table of Contents
- [Quick Start](#quick-start)
- [Query Examples](#query-examples)
- [Mutation Examples](#mutation-examples)
- [Expression Building](#expression-building)
- [Multi-Backend Scenarios](#multi-backend-scenarios)
- [Advanced Patterns](#advanced-patterns)
- [Sample Applications](#sample-applications)

---

## Quick Start

### Basic Query

```csharp
using FractalDataWorks.Commands.Data;
using FractalDataWorks.Commands.Data.Abstractions;

// Simple query with filter
var command = new QueryCommand<Customer>("Customers")
{
    Filter = new FilterExpression
    {
        Conditions =
        [
            new FilterCondition
            {
                PropertyName = nameof(Customer.IsActive),
                Operator = FilterOperators.Equal,
                Value = true
            }
        ]
    }
};

var result = await connection.ExecuteAsync(command);

if (result.IsSuccess)
{
    foreach (var customer in result.Value)
    {
        Console.WriteLine($"{customer.Name} - {customer.Email}");
    }
}
```

### Basic Insert

```csharp
var customer = new Customer
{
    Name = "John Doe",
    Email = "john@example.com",
    IsActive = true
};

var command = new InsertCommand<Customer>("Customers", customer);
var result = await connection.ExecuteAsync(command);

if (result.IsSuccess)
{
    var newId = result.Value; // Returns generated ID
    Console.WriteLine($"Customer created with ID: {newId}");
}
```

---

## Query Examples

### Complex Filtering

```csharp
// Query with multiple conditions and logical operators
var command = new QueryCommand<Customer>("Customers")
{
    Filter = new FilterExpression
    {
        Logic = LogicalOperator.Or,
        Conditions =
        [
            // Premium customers
            new FilterCondition
            {
                PropertyName = nameof(Customer.Status),
                Operator = FilterOperators.Equal,
                Value = "Premium"
            }
        ],
        NestedFilters =
        [
            // OR high-value active customers
            new FilterExpression
            {
                Logic = LogicalOperator.And,
                Conditions =
                [
                    new FilterCondition
                    {
                        PropertyName = nameof(Customer.TotalSpent),
                        Operator = FilterOperators.GreaterThanOrEqual,
                        Value = 10000
                    },
                    new FilterCondition
                    {
                        PropertyName = nameof(Customer.IsActive),
                        Operator = FilterOperators.Equal,
                        Value = true
                    }
                ]
            }
        ]
    }
};

// Translates to:
// WHERE Status = 'Premium'
//    OR (TotalSpent >= 10000 AND IsActive = 1)
```

### String Operations

```csharp
// Search by email domain
var command = new QueryCommand<Customer>("Customers")
{
    Filter = new FilterExpression
    {
        Conditions =
        [
            new FilterCondition
            {
                PropertyName = nameof(Customer.Email),
                Operator = FilterOperators.EndsWith,
                Value = "@example.com"
            }
        ]
    }
};

// Translates to:
// WHERE Email LIKE '%@example.com'
```

### Null Checks

```csharp
// Find customers without phone numbers
var command = new QueryCommand<Customer>("Customers")
{
    Filter = new FilterExpression
    {
        Conditions =
        [
            new FilterCondition
            {
                PropertyName = nameof(Customer.PhoneNumber),
                Operator = FilterOperators.IsNull
            }
        ]
    }
};

// Translates to:
// WHERE PhoneNumber IS NULL
```

### IN Operator

```csharp
// Find customers in specific cities
var cities = new[] { "New York", "Los Angeles", "Chicago" };

var command = new QueryCommand<Customer>("Customers")
{
    Filter = new FilterExpression
    {
        Conditions =
        [
            new FilterCondition
            {
                PropertyName = nameof(Customer.City),
                Operator = FilterOperators.In,
                Value = cities
            }
        ]
    }
};

// Translates to:
// WHERE City IN (@p0_0, @p0_1, @p0_2)
```

### Projection (Select Specific Fields)

```csharp
// Query only specific fields
var command = new QueryCommand<Customer>("Customers")
{
    Projection = new ProjectionExpression
    {
        Fields =
        [
            new ProjectionField { PropertyName = nameof(Customer.Id) },
            new ProjectionField { PropertyName = nameof(Customer.Name) },
            new ProjectionField
            {
                PropertyName = nameof(Customer.Email),
                Alias = "EmailAddress"
            }
        ]
    }
};

// Translates to:
// SELECT Id, Name, Email AS EmailAddress FROM Customers
```

### Ordering

```csharp
// Order by multiple fields
var command = new QueryCommand<Customer>("Customers")
{
    Ordering = new OrderingExpression
    {
        OrderedFields =
        [
            new OrderedField
            {
                PropertyName = nameof(Customer.LastName),
                Direction = SortDirection.Ascending
            },
            new OrderedField
            {
                PropertyName = nameof(Customer.FirstName),
                Direction = SortDirection.Ascending
            },
            new OrderedField
            {
                PropertyName = nameof(Customer.CreatedDate),
                Direction = SortDirection.Descending
            }
        ]
    }
};

// Translates to:
// ORDER BY LastName ASC, FirstName ASC, CreatedDate DESC
```

### Paging

```csharp
// Get page 3 (records 21-30)
var command = new QueryCommand<Customer>("Customers")
{
    Paging = new PagingExpression
    {
        Skip = 20,  // Skip first 20 records
        Take = 10   // Take next 10 records
    },
    Ordering = new OrderingExpression
    {
        OrderedFields =
        [
            new OrderedField
            {
                PropertyName = nameof(Customer.CreatedDate),
                Direction = SortDirection.Descending
            }
        ]
    }
};

// SQL Server translates to:
// ORDER BY CreatedDate DESC
// OFFSET 20 ROWS FETCH NEXT 10 ROWS ONLY

// REST/OData translates to:
// ?$skip=20&$top=10&$orderby=CreatedDate desc
```

### Complete Query Example

```csharp
// Comprehensive query with all features
var command = new QueryCommand<Customer>("Customers")
{
    Filter = new FilterExpression
    {
        Logic = LogicalOperator.And,
        Conditions =
        [
            new FilterCondition
            {
                PropertyName = nameof(Customer.IsActive),
                Operator = FilterOperators.Equal,
                Value = true
            },
            new FilterCondition
            {
                PropertyName = nameof(Customer.CreatedDate),
                Operator = FilterOperators.GreaterThanOrEqual,
                Value = DateTime.UtcNow.AddMonths(-6)
            }
        ]
    },
    Projection = new ProjectionExpression
    {
        Fields =
        [
            new ProjectionField { PropertyName = nameof(Customer.Id) },
            new ProjectionField { PropertyName = nameof(Customer.Name) },
            new ProjectionField { PropertyName = nameof(Customer.Email) },
            new ProjectionField { PropertyName = nameof(Customer.TotalSpent) }
        ]
    },
    Ordering = new OrderingExpression
    {
        OrderedFields =
        [
            new OrderedField
            {
                PropertyName = nameof(Customer.TotalSpent),
                Direction = SortDirection.Descending
            }
        ]
    },
    Paging = new PagingExpression { Skip = 0, Take = 50 }
};
```

---

## Mutation Examples

### Insert

```csharp
var newCustomer = new Customer
{
    Name = "Jane Smith",
    Email = "jane@example.com",
    IsActive = true,
    CreatedDate = DateTime.UtcNow
};

var command = new InsertCommand<Customer>("Customers", newCustomer);
var result = await connection.ExecuteAsync(command);

if (result.IsSuccess)
{
    Console.WriteLine($"Inserted customer with ID: {result.Value}");
}
```

### Update

```csharp
var customer = await GetCustomerAsync(customerId);
customer.Email = "newemail@example.com";
customer.LastModified = DateTime.UtcNow;

var command = new UpdateCommand<Customer>("Customers", customer);
var result = await connection.ExecuteAsync(command);

if (result.IsSuccess)
{
    Console.WriteLine($"Updated customer: {result.Value.Name}");
}
```

### Delete

```csharp
// Delete inactive customers older than 5 years
var command = new DeleteCommand("Customers")
{
    Filter = new FilterExpression
    {
        Logic = LogicalOperator.And,
        Conditions =
        [
            new FilterCondition
            {
                PropertyName = nameof(Customer.IsActive),
                Operator = FilterOperators.Equal,
                Value = false
            },
            new FilterCondition
            {
                PropertyName = nameof(Customer.LastLoginDate),
                Operator = FilterOperators.LessThan,
                Value = DateTime.UtcNow.AddYears(-5)
            }
        ]
    }
};

var result = await connection.ExecuteAsync(command);

if (result.IsSuccess)
{
    Console.WriteLine($"Deleted {result.Value} customers");
}
```

### Bulk Insert

```csharp
var customers = new List<Customer>
{
    new() { Name = "Customer 1", Email = "c1@example.com" },
    new() { Name = "Customer 2", Email = "c2@example.com" },
    new() { Name = "Customer 3", Email = "c3@example.com" },
    // ... 1000 more
};

var command = new BulkInsertCommand<Customer>("Customers", customers)
{
    BatchSize = 500,
    ContinueOnError = false
};

var result = await connection.ExecuteAsync(command);

if (result.IsSuccess)
{
    var bulkResult = result.Value;
    Console.WriteLine($"Inserted {bulkResult.RowsAffected} customers");

    if (!bulkResult.IsFullSuccess)
    {
        Console.WriteLine("Errors:");
        foreach (var error in bulkResult.Errors)
        {
            Console.WriteLine($"  - {error}");
        }
    }
}
```

---

## Expression Building

### Reusable Filter Builders

```csharp
public static class CustomerFilters
{
    public static IFilterExpression Active()
    {
        return new FilterExpression
        {
            Conditions =
            [
                new FilterCondition
                {
                    PropertyName = nameof(Customer.IsActive),
                    Operator = FilterOperators.Equal,
                    Value = true
                }
            ]
        };
    }

    public static IFilterExpression RecentlyCreated(int days)
    {
        return new FilterExpression
        {
            Conditions =
            [
                new FilterCondition
                {
                    PropertyName = nameof(Customer.CreatedDate),
                    Operator = FilterOperators.GreaterThanOrEqual,
                    Value = DateTime.UtcNow.AddDays(-days)
                }
            ]
        };
    }

    public static IFilterExpression Premium()
    {
        return new FilterExpression
        {
            Logic = LogicalOperator.Or,
            Conditions =
            [
                new FilterCondition
                {
                    PropertyName = nameof(Customer.Status),
                    Operator = FilterOperators.Equal,
                    Value = "Premium"
                },
                new FilterCondition
                {
                    PropertyName = nameof(Customer.TotalSpent),
                    Operator = FilterOperators.GreaterThanOrEqual,
                    Value = 10000m
                }
            ]
        };
    }

    public static IFilterExpression CombineAnd(params IFilterExpression[] filters)
    {
        return new FilterExpression
        {
            Logic = LogicalOperator.And,
            NestedFilters = filters.ToList()
        };
    }
}

// Usage:
var command = new QueryCommand<Customer>("Customers")
{
    Filter = CustomerFilters.CombineAnd(
        CustomerFilters.Active(),
        CustomerFilters.Premium(),
        CustomerFilters.RecentlyCreated(30)
    )
};
```

### Fluent Filter Builder

```csharp
public class FilterBuilder
{
    private readonly FilterExpression _filter = new();

    public FilterBuilder Where(string propertyName, FilterOperatorBase op, object? value)
    {
        _filter.Conditions.Add(new FilterCondition
        {
            PropertyName = propertyName,
            Operator = op,
            Value = value
        });
        return this;
    }

    public FilterBuilder And() { _filter.Logic = LogicalOperator.And; return this; }
    public FilterBuilder Or() { _filter.Logic = LogicalOperator.Or; return this; }

    public IFilterExpression Build() => _filter;
}

// Usage:
var filter = new FilterBuilder()
    .Where(nameof(Customer.IsActive), FilterOperators.Equal, true)
    .And()
    .Where(nameof(Customer.TotalSpent), FilterOperators.GreaterThan, 5000)
    .Build();
```

---

## Multi-Backend Scenarios

### Same Command, Different Backends

```csharp
// Define command once
var command = new QueryCommand<Customer>("Customers")
{
    Filter = new FilterExpression
    {
        Conditions =
        [
            new FilterCondition
            {
                PropertyName = nameof(Customer.IsActive),
                Operator = FilterOperators.Equal,
                Value = true
            }
        ]
    }
};

// Execute on SQL
var sqlResult = await sqlConnection.ExecuteAsync(command);
// Translates to: SELECT * FROM Customers WHERE IsActive = @p0

// Execute on REST API
var restResult = await httpConnection.ExecuteAsync(command);
// Translates to: GET /api/Customers?$filter=IsActive eq true

// Execute on JSON file
var fileResult = await fileConnection.ExecuteAsync(command);
// Translates to: Read file, filter in-memory
```

### Fallback Pattern

```csharp
public async Task<IGenericResult<IEnumerable<Customer>>> GetCustomersAsync()
{
    // Try primary source (SQL)
    var result = await _sqlConnection.ExecuteAsync(_command);

    if (result.IsSuccess)
        return result;

    _logger.LogWarning("Primary source failed, trying cache...");

    // Fallback to cache (REST/Redis)
    result = await _cacheConnection.ExecuteAsync(_command);

    if (result.IsSuccess)
        return result;

    _logger.LogError("Both sources failed");

    // Fallback to file backup
    return await _fileConnection.ExecuteAsync(_command);
}
```

### Multi-Source Aggregation

```csharp
public async Task<IEnumerable<Customer>> GetAllCustomersAsync()
{
    var command = new QueryCommand<Customer>("Customers");

    // Query multiple sources in parallel
    var tasks = new[]
    {
        _sqlConnection.ExecuteAsync(command),
        _restConnection.ExecuteAsync(command),
        _fileConnection.ExecuteAsync(command)
    };

    var results = await Task.WhenAll(tasks);

    // Combine successful results
    return results
        .Where(r => r.IsSuccess)
        .SelectMany(r => r.Value)
        .DistinctBy(c => c.Id);
}
```

---

## Advanced Patterns

### Repository Pattern with DataCommands

```csharp
public interface ICustomerRepository
{
    Task<IGenericResult<IEnumerable<Customer>>> GetActiveCustomersAsync();
    Task<IGenericResult<Customer>> GetByIdAsync(int id);
    Task<IGenericResult<int>> CreateAsync(Customer customer);
}

public class CustomerRepository : ICustomerRepository
{
    private readonly IDataConnection _connection;
    private const string Container = "Customers";

    public CustomerRepository(IDataConnection connection)
    {
        _connection = connection;
    }

    public async Task<IGenericResult<IEnumerable<Customer>>> GetActiveCustomersAsync()
    {
        var command = new QueryCommand<Customer>(Container)
        {
            Filter = CustomerFilters.Active()
        };

        return await _connection.ExecuteAsync(command);
    }

    public async Task<IGenericResult<Customer>> GetByIdAsync(int id)
    {
        var command = new QueryCommand<Customer>(Container)
        {
            Filter = new FilterExpression
            {
                Conditions =
                [
                    new FilterCondition
                    {
                        PropertyName = nameof(Customer.Id),
                        Operator = FilterOperators.Equal,
                        Value = id
                    }
                ]
            },
            Paging = new PagingExpression { Skip = 0, Take = 1 }
        };

        var result = await _connection.ExecuteAsync(command);

        if (!result.IsSuccess)
            return GenericResult<Customer>.Failure(result.Message);

        var customer = result.Value.FirstOrDefault();

        return customer != null
            ? GenericResult<Customer>.Success(customer)
            : GenericResult<Customer>.Failure($"Customer {id} not found");
    }

    public async Task<IGenericResult<int>> CreateAsync(Customer customer)
    {
        var command = new InsertCommand<Customer>(Container, customer);
        return await _connection.ExecuteAsync(command);
    }
}
```

### Command Factory Pattern

```csharp
public static class CustomerCommands
{
    public static QueryCommand<Customer> GetActive()
    {
        return new QueryCommand<Customer>("Customers")
        {
            Filter = CustomerFilters.Active()
        };
    }

    public static QueryCommand<Customer> Search(string searchTerm)
    {
        return new QueryCommand<Customer>("Customers")
        {
            Filter = new FilterExpression
            {
                Logic = LogicalOperator.Or,
                Conditions =
                [
                    new FilterCondition
                    {
                        PropertyName = nameof(Customer.Name),
                        Operator = FilterOperators.Contains,
                        Value = searchTerm
                    },
                    new FilterCondition
                    {
                        PropertyName = nameof(Customer.Email),
                        Operator = FilterOperators.Contains,
                        Value = searchTerm
                    }
                ]
            }
        };
    }

    public static QueryCommand<Customer> GetPage(int pageNumber, int pageSize)
    {
        return new QueryCommand<Customer>("Customers")
        {
            Paging = new PagingExpression
            {
                Skip = (pageNumber - 1) * pageSize,
                Take = pageSize
            },
            Ordering = new OrderingExpression
            {
                OrderedFields =
                [
                    new OrderedField
                    {
                        PropertyName = nameof(Customer.CreatedDate),
                        Direction = SortDirection.Descending
                    }
                ]
            }
        };
    }
}

// Usage:
var activeCustomers = await connection.ExecuteAsync(CustomerCommands.GetActive());
var searchResults = await connection.ExecuteAsync(CustomerCommands.Search("john"));
var page2 = await connection.ExecuteAsync(CustomerCommands.GetPage(2, 20));
```

### Command Decorator Pattern

```csharp
public class LoggingCommandDecorator<T> : IDataCommand<T>
{
    private readonly IDataCommand<T> _innerCommand;
    private readonly ILogger _logger;

    public LoggingCommandDecorator(IDataCommand<T> innerCommand, ILogger logger)
    {
        _innerCommand = innerCommand;
        _logger = logger;
    }

    public string ContainerName => _innerCommand.ContainerName;
    public string Name => _innerCommand.Name;

    // Log before execution
    public void LogExecution()
    {
        _logger.LogInformation(
            "Executing {CommandType} on {Container}",
            _innerCommand.Name,
            _innerCommand.ContainerName);
    }
}

// Usage:
var command = new QueryCommand<Customer>("Customers") { /* ... */ };
var decoratedCommand = new LoggingCommandDecorator<IEnumerable<Customer>>(command, logger);

decoratedCommand.LogExecution();
var result = await connection.ExecuteAsync(command);
```

---

## Sample Applications

### Complete Customer Service Example

See: `samples/DataCommands/CustomerService/`

Features:
- CRUD operations using DataCommands
- Multi-backend support (SQL + REST fallback)
- Repository pattern implementation
- Comprehensive error handling
- Unit and integration tests

### E-Commerce Order System

See: `samples/DataCommands/ECommerce/`

Features:
- Complex queries with joins
- Bulk operations for inventory
- Transaction coordination
- Multi-tenant filtering
- Performance benchmarks

### Data Migration Tool

See: `samples/DataCommands/DataMigration/`

Features:
- Reading from SQL Server
- Writing to PostgreSQL
- Same command, different translators
- Progress reporting
- Error handling and retry logic

---

## Additional Resources

- **[Overview](DataCommands-Overview.md)** - Architecture and concepts
- **[Developer Guide](DataCommands-Developer-Guide.md)** - Building commands
- **[Translator Guide](DataCommands-Translator-Guide.md)** - Building translators
- **Sample Code**: `samples/DataCommands/`
- **API Reference**: `src/FractalDataWorks.Commands.Data.Abstractions/README.md`

---

**Last Updated**: October 2024
**Version**: 1.0.0 (Initial Release)
