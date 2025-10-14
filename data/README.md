# DataCommands - Universal Data Query Representation

## What is a DataCommand?

A **DataCommand** is a universal, domain-agnostic representation of data operations (CRUD) that can be translated to any data access protocol or query language.

Think of it as a "universal translator" pattern for data operations - write your query once in DataCommand format, and it can be executed against:
- SQL databases (SQL Server, PostgreSQL, MySQL, SQLite)
- REST APIs (with OData or custom protocols)
- GraphQL endpoints
- File systems
- NoSQL databases
- Any other data source

## Key Characteristics

### 1. **Domain-Agnostic**
DataCommands don't know about SQL, REST, or any specific protocol. They describe WHAT you want to do, not HOW to do it.

```csharp
// This is a DataCommand - notice no SQL, no HTTP, no specific protocol
var query = new QueryCommand<Customer>("Customers")
{
    Filter = new FilterExpression
    {
        Conditions = new[]
        {
            new FilterCondition
            {
                PropertyName = "IsActive",
                Operator = FilterOperators.Equal,
                Value = true
            }
        }
    }
};
```

### 2. **Translator Pattern**
Translators convert DataCommands to domain-specific representations:

```csharp
// SQL Translator produces:
SELECT * FROM Customers WHERE IsActive = @IsActive

// REST Translator produces:
GET /Customers?$filter=IsActive eq true

// GraphQL Translator produces:
query { customers(where: { isActive: { eq: true } }) { ... } }
```

### 3. **Zero Boxing with Generics**
Three-level generic hierarchy ensures compile-time type safety with no runtime casting:

- `IDataCommand` - Non-generic marker interface
- `IDataCommand<TResult>` - Knows the result type
- `IDataCommand<TResult, TInput>` - Knows both input and result types

```csharp
// Compiler knows this returns IEnumerable<Customer>
QueryCommand<Customer> query = new QueryCommand<Customer>("Customers");

// No casting needed!
IEnumerable<Customer> results = await connection.ExecuteAsync(query);
```

### 4. **Serializable**
DataCommands can be serialized to JSON and transmitted over networks:

```json
{
  "CommandType": "Query",
  "ContainerName": "Customers",
  "Filter": {
    "LogicalOperator": "And",
    "Conditions": [
      {
        "PropertyName": "IsActive",
        "Operator": "Equal",
        "Value": true
      }
    ]
  },
  "Ordering": {
    "OrderedFields": [
      {
        "PropertyName": "Name",
        "Direction": "Ascending"
      }
    ]
  }
}
```

This allows:
- Client-side query building
- Query caching and replay
- Audit logging
- Cross-service communication
- Query optimization analysis

## Architecture

```
┌─────────────────────────────────────────────────┐
│ Application Layer                                │
│ Creates DataCommands (universal representation) │
└────────────────┬────────────────────────────────┘
                 │
                 ▼
┌─────────────────────────────────────────────────┐
│ Translator Layer                                 │
│ Converts DataCommand to domain-specific format  │
├─────────────┬───────────┬──────────┬────────────┤
│ SqlTranslator RestTrans GraphQLTr FileTrans    │
│     │           │           │          │        │
│     ▼           ▼           ▼          ▼        │
│   SQL         HTTP       GraphQL     File I/O   │
└─────────────────────────────────────────────────┘
```

## Command Types

### QueryCommand&lt;T&gt;
Represents a SELECT/GET/READ operation:
- **Filter**: WHERE conditions
- **Projection**: SELECT fields
- **Ordering**: ORDER BY
- **Paging**: SKIP/TAKE, OFFSET/LIMIT
- **Aggregation**: GROUP BY, COUNT, SUM, etc.
- **Joins**: Related data

### InsertCommand&lt;T&gt;
Represents a CREATE/POST operation:
- **Data**: The entity to insert
- Always requires a transaction
- Is a mutation (not cacheable)

### UpdateCommand&lt;T&gt;
Represents an UPDATE/PUT/PATCH operation:
- **Data**: The updated entity
- **Filter**: Which records to update (WHERE clause)
- Always requires a transaction
- Is a mutation

### DeleteCommand
Represents a DELETE operation:
- **Filter**: Which records to delete (WHERE clause)
- Always requires a transaction
- Is a mutation

## Benefits

### 1. **Write Once, Run Anywhere**
```csharp
var query = new QueryCommand<Customer>("Customers") { /* ... */ };

// Same query works with:
await sqlConnection.ExecuteAsync(query);      // → SQL
await restConnection.ExecuteAsync(query);     // → HTTP/OData
await graphqlConnection.ExecuteAsync(query);  // → GraphQL
await fileConnection.ExecuteAsync(query);     // → File I/O
```

### 2. **No Switch Statements**
Operators know their own translations:
```csharp
// EqualOperator knows:
SqlOperator = "="
ODataOperator = "eq"

// ContainsOperator knows:
SqlOperator = "LIKE"
ODataOperator = "contains"
```

### 3. **Extensible**
- Add new operators without modifying existing code
- Add new translators for new protocols
- Add new command types for specialized operations

### 4. **Testable**
Test your query logic without a database:
```csharp
var query = BuildCustomerQuery(isActive: true, minRevenue: 1000000);

// Assert the query is correct (no DB needed)
Assert.Equal("Customers", query.ContainerName);
Assert.Single(query.Filter.Conditions);
```

### 5. **Auditable**
Every operation is represented as a serializable object:
```csharp
// Log every query
logger.LogInformation("Executing query: {Query}",
    JsonSerializer.Serialize(query));
```

## Examples

See `DataCommands.Demo` project for comprehensive examples:
- Complex filtering with multiple conditions
- Sorting and paging
- Projection (selecting specific fields)
- All 12 filter operators
- CRUD operations
- JSON serialization
- SQL and OData translation examples

## Future Enhancements

- Query builder fluent API
- Query optimization hints
- Batch operations
- Stored procedure representation
- Full-text search commands
- Geospatial query commands
