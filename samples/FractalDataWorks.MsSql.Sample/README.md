# FractalDataWorks MsSql Connection Sample

Complete sample application demonstrating the FractalDataWorks SQL Server connection implementation with real-world examples.

## Overview

This sample demonstrates:
- ✅ Database setup with schemas, tables, and seed data
- ✅ Connection configuration and dependency injection
- ✅ Query execution (SELECT, JOIN, aggregates)
- ✅ Data manipulation (INSERT, UPDATE, DELETE)
- ✅ Type-safe result mapping
- ✅ Error handling and retry logic
- ✅ Transaction support
- ✅ Paging and sorting
- ✅ Parameterized queries (SQL injection prevention)

## Prerequisites

- **SQL Server** (LocalDB, Express, Developer, or Enterprise)
  - Download: https://www.microsoft.com/en-us/sql-server/sql-server-downloads
- **.NET 10.0 SDK or later**
  - Download: https://dotnet.microsoft.com/download
- **Optional**: SQL Server Management Studio (SSMS) for database management
  - Download: https://aka.ms/ssmsfullsetup

## Quick Start

### 1. Set Up the Database

Run the database setup scripts in order:

**Option A: Run all scripts at once (Recommended)**
```bash
cd Scripts
sqlcmd -S localhost -E -i 04-RunAll.sql
```

**Option B: Run scripts individually**
```bash
sqlcmd -S localhost -E -i 01-CreateDatabase.sql
sqlcmd -S localhost -E -i 02-CreateTables.sql
sqlcmd -S localhost -E -i 03-SeedData.sql
```

**Option C: Using SQL Server Management Studio**
1. Open SSMS and connect to your server
2. Open each .sql file (01 through 03)
3. Execute them in order (F5)

### 2. Configure Connection String

Edit `appsettings.json` to match your SQL Server instance:

```json
{
  "ConnectionStrings": {
    "FractalSample": "Server=localhost;Database=FractalSample;Integrated Security=true;TrustServerCertificate=true;Encrypt=false"
  }
}
```

**Common connection string variations:**

**LocalDB:**
```
Server=(localdb)\\mssqllocaldb;Database=FractalSample;Integrated Security=true;TrustServerCertificate=true
```

**Named instance:**
```
Server=localhost\\SQLEXPRESS;Database=FractalSample;Integrated Security=true;TrustServerCertificate=true
```

**SQL Authentication:**
```
Server=localhost;Database=FractalSample;User Id=sa;Password=YourPassword;TrustServerCertificate=true;Encrypt=false
```

**Azure SQL:**
```
Server=your-server.database.windows.net;Database=FractalSample;User Id=yourusername;Password=yourpassword;Encrypt=true
```

### 3. Run the Sample

```bash
dotnet run
```

## Database Schema

The sample database includes:

### Schemas

- **crm** - Customer Relationship Management
- **sales** - Sales and Orders
- **inventory** - Product Catalog

### Tables

#### crm.Customers
Customer information with loyalty tracking.

| Column | Type | Description |
|--------|------|-------------|
| CustomerId | INT | Primary key (identity) |
| FirstName | NVARCHAR(100) | Customer first name |
| LastName | NVARCHAR(100) | Customer last name |
| Email | NVARCHAR(255) | Email (unique) |
| CustomerType | NVARCHAR(20) | Standard, Premium, or VIP |
| TotalSpent | DECIMAL(18,2) | Lifetime spending |
| LoyaltyPoints | INT | Reward points |
| IsActive | BIT | Active status |

#### inventory.Products
Product catalog with pricing and inventory.

| Column | Type | Description |
|--------|------|-------------|
| ProductId | INT | Primary key (identity) |
| CategoryId | INT | Foreign key to Categories |
| ProductName | NVARCHAR(200) | Product name |
| SKU | NVARCHAR(50) | Stock keeping unit (unique) |
| Price | DECIMAL(18,2) | Selling price |
| Cost | DECIMAL(18,2) | Cost of goods |
| QuantityInStock | INT | Current inventory |
| ReorderLevel | INT | Reorder threshold |

#### sales.Orders
Sales orders with payment tracking.

| Column | Type | Description |
|--------|------|-------------|
| OrderId | INT | Primary key (identity) |
| CustomerId | INT | Foreign key to Customers |
| OrderDate | DATETIME2 | Order placement date |
| Status | NVARCHAR(20) | Pending, Processing, Shipped, Delivered, Cancelled |
| TotalAmount | DECIMAL(18,2) | Order total |
| PaymentStatus | NVARCHAR(20) | Pending, Authorized, Paid, Refunded |

#### sales.OrderItems
Line items for orders.

| Column | Type | Description |
|--------|------|-------------|
| OrderItemId | INT | Primary key (identity) |
| OrderId | INT | Foreign key to Orders |
| ProductId | INT | Foreign key to Products |
| Quantity | INT | Quantity ordered |
| UnitPrice | DECIMAL(18,2) | Price per unit |
| Discount | DECIMAL(5,2) | Discount percentage |
| LineTotal | DECIMAL(18,2) | Line total after discount |

## Sample Data

The seed script creates:
- **6 product categories** (Electronics, Computers, Peripherals, Software, Office Supplies, Furniture)
- **20 products** with realistic pricing and inventory
- **10 customers** with various types (Standard, Premium, VIP)
- **5 orders** with 23 line items

## Code Examples

### Example 1: Simple Query

```csharp
var sql = "SELECT * FROM [crm].[Customers] ORDER BY CustomerId";
var command = new SqlConnectionCommand(sql);
var result = await service.Execute<IEnumerable<Customer>>(command, cancellationToken);

if (result.IsSuccess)
{
    foreach (var customer in result.Value)
    {
        Console.WriteLine($"{customer.FullName} - {customer.Email}");
    }
}
```

### Example 2: Parameterized Query

```csharp
var sql = @"
    SELECT * FROM [crm].[Customers]
    WHERE CustomerType = @type AND IsActive = @isActive";

var parameters = new Dictionary<string, object>
{
    ["type"] = "VIP",
    ["isActive"] = true
};

var command = new SqlConnectionCommand(sql, parameters);
var result = await service.Execute<IEnumerable<Customer>>(command, cancellationToken);
```

### Example 3: Scalar Query (COUNT, SUM, AVG)

```csharp
var sql = "SELECT COUNT(*) FROM [sales].[Orders] WHERE Status = 'Delivered'";
var command = new SqlConnectionCommand(sql);
var result = await service.Execute<int>(command, cancellationToken);

Console.WriteLine($"Delivered orders: {result.Value}");
```

### Example 4: JOIN Query

```csharp
var sql = @"
    SELECT
        o.OrderId,
        o.OrderDate,
        (c.FirstName + ' ' + c.LastName) AS CustomerName,
        c.Email AS CustomerEmail,
        o.TotalAmount,
        o.Status
    FROM [sales].[Orders] o
    INNER JOIN [crm].[Customers] c ON o.CustomerId = c.CustomerId";

var command = new SqlConnectionCommand(sql);
var result = await service.Execute<IEnumerable<OrderSummary>>(command, cancellationToken);
```

### Example 5: Paging with OFFSET/FETCH

```csharp
var sql = @"
    SELECT * FROM [inventory].[Products]
    ORDER BY Price DESC
    OFFSET @offset ROWS
    FETCH NEXT @pageSize ROWS ONLY";

var parameters = new Dictionary<string, object>
{
    ["offset"] = 10,     // Skip 10 records
    ["pageSize"] = 10    // Take 10 records
};

var command = new SqlConnectionCommand(sql, parameters);
var result = await service.Execute<IEnumerable<Product>>(command, cancellationToken);
```

### Example 6: INSERT with SCOPE_IDENTITY

```csharp
var sql = @"
    INSERT INTO [inventory].[Categories] (CategoryName, Description, IsActive)
    VALUES (@name, @description, @isActive);
    SELECT SCOPE_IDENTITY();";

var parameters = new Dictionary<string, object>
{
    ["name"] = "New Category",
    ["description"] = "Category description",
    ["isActive"] = true
};

var command = new SqlConnectionCommand(sql, parameters);
var result = await service.Execute<decimal>(command, cancellationToken);

Console.WriteLine($"New category ID: {result.Value}");
```

### Example 7: UPDATE

```csharp
var sql = @"
    UPDATE [inventory].[Products]
    SET Price = @newPrice, LastModifiedDate = GETUTCDATE()
    WHERE ProductId = @productId";

var parameters = new Dictionary<string, object>
{
    ["newPrice"] = 99.99m,
    ["productId"] = 5
};

var command = new SqlConnectionCommand(sql, parameters);
var result = await service.Execute<int>(command, cancellationToken);

Console.WriteLine($"Updated {result.Value} rows");
```

### Example 8: DELETE

```csharp
var sql = "DELETE FROM [inventory].[Categories] WHERE CategoryName = @name";

var parameters = new Dictionary<string, object>
{
    ["name"] = "Test Category"
};

var command = new SqlConnectionCommand(sql, parameters);
var result = await service.Execute<int>(command, cancellationToken);
```

## Configuration Reference

### MsSqlConfiguration Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| ConnectionString | string | Required | SQL Server connection string |
| CommandTimeoutSeconds | int | 30 | Command execution timeout |
| ConnectionTimeoutSeconds | int | 15 | Connection establishment timeout |
| DefaultSchema | string | "dbo" | Default schema for unqualified tables |
| SchemaMappings | Dictionary | {} | Map container names to schema.table |
| EnableConnectionPooling | bool | true | Use connection pooling |
| MinPoolSize | int | 0 | Minimum pool size |
| MaxPoolSize | int | 100 | Maximum pool size |
| EnableRetryLogic | bool | true | Retry transient errors |
| MaxRetryAttempts | int | 3 | Maximum retry attempts |
| RetryDelayMilliseconds | int | 1000 | Delay between retries |
| EnableSqlLogging | bool | false | Log SQL commands |
| MaxSqlLogLength | int | 1000 | Truncate logged SQL |
| UseTransactions | bool | false | Wrap operations in transactions |
| TransactionIsolationLevel | IsolationLevel | ReadCommitted | Transaction isolation level |

### Logging Configuration

The sample uses Microsoft.Extensions.Logging with console output:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "FractalDataWorks": "Debug"
    }
  }
}
```

**Log Levels:**
- **Debug**: SQL commands, connection events
- **Information**: Operation results
- **Warning**: Retry attempts
- **Error**: Failures and exceptions

## Error Handling

The MsSql service uses Railway-Oriented Programming with `IGenericResult<T>`:

```csharp
var result = await service.Execute<IEnumerable<Customer>>(command, cancellationToken);

if (result.IsSuccess)
{
    // Use result.Value
    var customers = result.Value;
}
else
{
    // Handle error
    Console.WriteLine($"Error: {result.CurrentMessage}");
}
```

### Automatic Retry Logic

The service automatically retries transient SQL Server errors:

**Transient Error Codes:**
- `-2` - Timeout
- `1205` - Deadlock victim
- `2, 20, 64, 233, 10053, 10054, 10060` - Connection errors
- `40197, 40501, 40613` - Azure SQL transient errors

**Configuration:**
```json
{
  "EnableRetryLogic": true,
  "MaxRetryAttempts": 3,
  "RetryDelayMilliseconds": 1000
}
```

## Transaction Support

Enable transactions via configuration:

```json
{
  "UseTransactions": true,
  "TransactionIsolationLevel": "ReadCommitted"
}
```

**Supported Isolation Levels:**
- ReadUncommitted
- ReadCommitted (default)
- RepeatableRead
- Serializable
- Snapshot

**Behavior:**
- ✅ Automatic commit on success
- ✅ Automatic rollback on exception
- ✅ Configurable isolation level
- ✅ Works with retry logic

## Performance Tips

1. **Use Connection Pooling** (enabled by default)
   ```json
   "EnableConnectionPooling": true,
   "MaxPoolSize": 100
   ```

2. **Disable SQL Logging in Production**
   ```json
   "EnableSqlLogging": false
   ```

3. **Use Paging for Large Result Sets**
   ```sql
   OFFSET @offset ROWS FETCH NEXT @pageSize ROWS ONLY
   ```

4. **Use Indexes** - The sample creates indexes on:
   - Customer email and type
   - Product SKU and category
   - Order customer ID, date, and status
   - OrderItems order ID and product ID

5. **Batch Operations** - For multiple inserts, consider table-valued parameters or bulk insert

## Security

### SQL Injection Prevention

All queries use **parameterized commands**:

```csharp
// ✅ SAFE - Parameterized
var parameters = new Dictionary<string, object>
{
    ["email"] = userInput
};
var sql = "SELECT * FROM Customers WHERE Email = @email";

// ❌ UNSAFE - String concatenation
var sql = $"SELECT * FROM Customers WHERE Email = '{userInput}'";
```

### Connection String Security

- ✅ Store connection strings in `appsettings.json`
- ✅ Use User Secrets for development
- ✅ Use Azure Key Vault or similar for production
- ✅ Connection string sanitization for logging (passwords removed)

## Troubleshooting

### Connection Errors

**Error: "A network-related or instance-specific error"**
- Verify SQL Server is running
- Check server name and instance
- Ensure TCP/IP is enabled
- Check firewall settings

**Error: "Login failed for user"**
- Verify SQL Authentication is enabled (if using SQL auth)
- Check username and password
- Verify user has access to the database

### Database Not Found

```bash
# Verify database exists
sqlcmd -S localhost -E -Q "SELECT name FROM sys.databases"

# Recreate if needed
sqlcmd -S localhost -E -i Scripts/01-CreateDatabase.sql
```

### Performance Issues

- Enable SQL logging to see actual queries:
  ```json
  "EnableSqlLogging": true
  ```
- Check execution plans in SSMS
- Verify indexes are created (run 02-CreateTables.sql)

## Next Steps

1. **Modify the Models** - Add properties or create new entities
2. **Create Custom Queries** - Write domain-specific queries
3. **Add Validation** - Implement FluentValidation
4. **Build an API** - Use with ASP.NET Core / FastEndpoints
5. **Add Testing** - Write integration tests

## Additional Resources

- [MsSql Service Documentation](../../public-repo/src/FractalDataWorks.Services.Connections.MsSql/README.md)
- [SQL Server Documentation](https://docs.microsoft.com/en-us/sql/sql-server/)
- [Railway-Oriented Programming](https://fsharpforfunandprofit.com/rop/)

## License

This sample is part of the FractalDataWorks project.
