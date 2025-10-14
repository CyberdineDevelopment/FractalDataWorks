# FractalDataWorks DataCommands

Universal data command system that provides a single, type-safe API for working with data across any backend - SQL databases, REST APIs, file systems, GraphQL endpoints, and more.

## 🚀 Quick Start

```csharp
// Query customers
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
```

## 📚 Documentation

### User Documentation
- **[Overview](docs/DataCommands-Overview.md)** - Architecture, concepts, and comparisons
- **[Developer Guide](docs/DataCommands-Developer-Guide.md)** - Building commands and expressions
- **[Translator Guide](docs/DataCommands-Translator-Guide.md)** - Building translators
- **[Examples](docs/DataCommands-Examples.md)** - Practical examples and patterns

### Design Documentation
- **[Design History](docs/design/datacommands/)** - Architecture evolution and decisions
- **[Architecture Summary](docs/design/datacommands/ARCHITECTURE_SUMMARY.md)** - Final architecture
- **[Implementation Details](docs/design/datacommands/IMPLEMENTATION_DETAILS.md)** - Technical specs

## ✨ Key Features

✅ **Universal API** - One command works with SQL, REST, Files, GraphQL
✅ **Type-Safe** - Full compile-time type checking with generics
✅ **Zero Boxing** - No object casting or performance overhead
✅ **Extensible** - Add commands and translators without changing existing code
✅ **Railway-Oriented** - Explicit error handling with `IGenericResult<T>`
✅ **No Switch Statements** - Clean dispatch via TypeCollections and visitor pattern

## 🏗️ Architecture

```
Application Code
    ↓
DataCommands (Universal)
    ↓
Translators (Backend-Specific)
    ↓
Connections (Protocol)
    ↓
Data Sources
```

### Projects

```
FractalDataWorks.Commands.Data.Abstractions/  ← Interfaces and base classes
FractalDataWorks.Commands.Data/               ← Concrete implementations
FractalDataWorks.Commands.Data.Translators/   ← Translator implementations (TBD)
```

## 🎯 Core Concepts

### Commands
Operations on data:
- `QueryCommand<T>` - Retrieve data
- `InsertCommand<T>` - Add new data
- `UpdateCommand<T>` - Modify data
- `DeleteCommand` - Remove data

### Expressions
Query logic:
- `FilterExpression` - WHERE conditions
- `ProjectionExpression` - SELECT fields
- `OrderingExpression` - ORDER BY
- `PagingExpression` - SKIP/TAKE

### Translators
Backend conversion:
- `SqlTranslator` → SQL statements
- `RestTranslator` → HTTP + OData
- `FileTranslator` → File I/O

### Operators
TypeCollection of filter operators:
- `FilterOperators.Equal` → `=` / `eq`
- `FilterOperators.Contains` → `LIKE '%value%'` / `contains`
- `FilterOperators.GreaterThan` → `>` / `gt`
- And 9 more...

## 📝 Example Usage

### Query with Multiple Conditions

```csharp
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
                PropertyName = nameof(Customer.TotalSpent),
                Operator = FilterOperators.GreaterThanOrEqual,
                Value = 10000
            }
        ]
    },
    Paging = new PagingExpression { Skip = 0, Take = 50 }
};
```

### Multi-Backend Support

```csharp
// Same command works with any backend!
var sqlResult = await sqlConnection.ExecuteAsync(command);      // → SQL
var restResult = await httpConnection.ExecuteAsync(command);    // → REST API
var fileResult = await fileConnection.ExecuteAsync(command);    // → JSON file
```

### Insert with Type Safety

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
    var newId = result.Value; // Type-safe! No casting needed
    Console.WriteLine($"Created customer with ID: {newId}");
}
```

## 🔧 Building Custom Commands

```csharp
[TypeOption(typeof(DataCommands), "MyCustomCommand")]
public sealed class MyCustomCommand<T> : DataCommandBase<T>
{
    public MyCustomCommand(string containerName)
        : base(
            id: 10,
            name: "MyCustomCommand",
            containerName,
            DataCommandCategory.Query)
    {
    }

    // Add command-specific properties
    public string? CustomProperty { get; init; }
}
```

## 🧪 Testing

```csharp
[Fact]
public async Task ExecuteAsync_WithFilter_ReturnsFilteredResults()
{
    // Arrange
    var mockConnection = new Mock<IDataConnection>();
    var command = new QueryCommand<Customer>("Customers")
    {
        Filter = CustomerFilters.Active()
    };

    // Act
    var result = await mockConnection.Object.ExecuteAsync(command);

    // Assert
    result.IsSuccess.Should().BeTrue();
}
```

## 🎓 Learning Path

1. **Start Here**: [Overview](docs/DataCommands-Overview.md) - Understand the architecture
2. **Build Commands**: [Developer Guide](docs/DataCommands-Developer-Guide.md) - Create your first command
3. **Build Translators**: [Translator Guide](docs/DataCommands-Translator-Guide.md) - Support new backends
4. **See Examples**: [Examples](docs/DataCommands-Examples.md) - Real-world patterns

## 🆚 Comparisons

### vs. Entity Framework Core
- ✅ Works with non-SQL backends (REST, Files, GraphQL)
- ✅ Easier testing (mock connections, not databases)
- ⚠️ No change tracking or migrations

### vs. Dapper
- ✅ Type-safe query building (no string SQL)
- ✅ Works with non-SQL backends
- ✅ Protected from SQL injection by design

### vs. Repository Pattern
- ✅ No code duplication (shared commands)
- ✅ Easy to swap backends
- ✅ Consistent API across data sources

## 📦 Current Status

- ✅ **Architecture**: Complete
- ✅ **Commands.Data.Abstractions**: Implemented
- ✅ **Commands.Data**: Implemented (4 commands, 12 operators)
- ✅ **Documentation**: Complete
- ⏳ **Translators**: Not yet started (see guides for implementation)
- ⏳ **Samples**: Planned

## 🤝 Contributing

See design docs in `docs/design/datacommands/` for architectural decisions and patterns to follow.

### Key Conventions
- ✅ Use TypeCollections instead of enums
- ✅ Use Railway-Oriented Programming (IGenericResult)
- ✅ Use visitor pattern (no switch statements)
- ✅ Properties set in constructors (for TypeCollection compatibility)
- ✅ Full XML documentation

## 📄 License

Part of the FractalDataWorks Developer Kit.

---

## 🔗 Quick Links

- **API Reference**: `src/FractalDataWorks.Commands.Data.Abstractions/`
- **Implementation**: `src/FractalDataWorks.Commands.Data/`
- **Design History**: `docs/design/datacommands/`
- **Samples**: `samples/DataCommands/` (coming soon)

---

**Version**: 1.0.0
**Last Updated**: October 2024
**Branch**: `feature/datacommands-architecture`
