using System;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using FractalDataWorks.Commands.Abstractions;
using FractalDataWorks.Data.Abstractions;
using FractalDataWorks.Data.Sql.Commands;
using FractalDataWorks.Data.Sql.Logging;
using FractalDataWorks.Data.Sql.Translators;
using FractalDataWorks.Results;

namespace FractalDataWorks.Data.Sql.Sample;

/// <summary>
/// Sample application demonstrating SQL command translation and execution
/// using the FractalDataWorks framework patterns.
/// </summary>
public class Program
{
    public static void Main(string[] args)
    {
        Console.WriteLine("========================================");
        Console.WriteLine("FractalDataWorks SQL Command Translation Sample");
        Console.WriteLine("========================================\n");

        // Set up services and logging
        var services = new ServiceCollection();

        // Add logging
        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Debug);
        });

        // Add Entity Framework with In-Memory database
        services.AddDbContext<SampleDbContext>(options =>
            options.UseInMemoryDatabase("SampleDb"));

        // Add SQL translator services
        services.AddSingleton<SqlTranslatorType>();
        services.AddSingleton<SqlCommandTranslator>();
        services.AddSingleton<SqlOptimizer>();
        services.AddSingleton<IDataSchema, InMemoryDataSchema>();

        var serviceProvider = services.BuildServiceProvider();

        // Initialize database with sample data
        InitializeDatabase(serviceProvider);

        // Run translation demonstrations
        DemoLinqToSqlTranslation(serviceProvider);
        DemoQueryExecution(serviceProvider);
        DemoComplexQueries(serviceProvider);
        DemoOptimization(serviceProvider);

        Console.WriteLine("\n========================================");
        Console.WriteLine("Sample completed successfully!");
        Console.WriteLine("========================================");
    }

    private static void InitializeDatabase(IServiceProvider serviceProvider)
    {
        Console.WriteLine("Initializing database with sample data...");

        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<SampleDbContext>();

        // Add sample products
        var products = new[]
        {
            new Product { Id = 1, Name = "Laptop", Category = "Electronics", Price = 1299.99m, Stock = 15 },
            new Product { Id = 2, Name = "Mouse", Category = "Electronics", Price = 29.99m, Stock = 150 },
            new Product { Id = 3, Name = "Keyboard", Category = "Electronics", Price = 79.99m, Stock = 85 },
            new Product { Id = 4, Name = "Desk", Category = "Furniture", Price = 349.99m, Stock = 20 },
            new Product { Id = 5, Name = "Chair", Category = "Furniture", Price = 199.99m, Stock = 35 },
            new Product { Id = 6, Name = "Monitor", Category = "Electronics", Price = 399.99m, Stock = 45 },
            new Product { Id = 7, Name = "Notebook", Category = "Stationery", Price = 4.99m, Stock = 200 },
            new Product { Id = 8, Name = "Pen Set", Category = "Stationery", Price = 19.99m, Stock = 100 }
        };

        context.Products.AddRange(products);

        // Add sample customers
        var customers = new[]
        {
            new Customer { Id = 1, Name = "John Doe", Email = "john@example.com", City = "New York" },
            new Customer { Id = 2, Name = "Jane Smith", Email = "jane@example.com", City = "Los Angeles" },
            new Customer { Id = 3, Name = "Bob Johnson", Email = "bob@example.com", City = "Chicago" }
        };

        context.Customers.AddRange(customers);

        // Add sample orders
        var orders = new[]
        {
            new Order { Id = 1, CustomerId = 1, ProductId = 1, Quantity = 1, OrderDate = DateTime.Now.AddDays(-5) },
            new Order { Id = 2, CustomerId = 1, ProductId = 2, Quantity = 2, OrderDate = DateTime.Now.AddDays(-4) },
            new Order { Id = 3, CustomerId = 2, ProductId = 4, Quantity = 1, OrderDate = DateTime.Now.AddDays(-3) },
            new Order { Id = 4, CustomerId = 3, ProductId = 6, Quantity = 2, OrderDate = DateTime.Now.AddDays(-2) }
        };

        context.Orders.AddRange(orders);
        context.SaveChanges();

        Console.WriteLine($"Added {products.Length} products, {customers.Length} customers, and {orders.Length} orders.\n");
    }

    private static void DemoLinqToSqlTranslation(IServiceProvider serviceProvider)
    {
        Console.WriteLine("=====================================");
        Console.WriteLine("Demo: LINQ to SQL Translation");
        Console.WriteLine("=====================================\n");

        var translator = serviceProvider.GetRequiredService<SqlCommandTranslator>();
        var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
        var schema = serviceProvider.GetRequiredService<IDataSchema>();

        // Example 1: Simple SELECT query
        Console.WriteLine("1. Simple SELECT query:");
        Expression<Func<Product, bool>> simpleWhere = p => p.Category == "Electronics";
        var simpleCommand = new SqlQueryCommand<Product>(
            simpleWhere,
            SqlText: null,
            Parameters: []
        );

        var translationResult = translator.Translate(simpleCommand, new TranslationContext(schema));

        if (translationResult.IsSuccess)
        {
            var translatedCommand = translationResult.Value as SqlQueryCommand<Product>;
            Console.WriteLine($"   Original LINQ: p => p.Category == \"Electronics\"");
            Console.WriteLine($"   Translated SQL: {translatedCommand?.SqlText ?? "N/A"}\n");
        }

        // Example 2: Complex query with multiple conditions
        Console.WriteLine("2. Complex query with multiple conditions:");
        Expression<Func<Product, bool>> complexWhere = p =>
            p.Price > 100 && p.Stock < 50 && (p.Category == "Electronics" || p.Category == "Furniture");

        var complexCommand = new SqlQueryCommand<Product>(
            complexWhere,
            SqlText: null,
            Parameters: []
        );

        translationResult = translator.Translate(complexCommand, new TranslationContext(schema));

        if (translationResult.IsSuccess)
        {
            var translatedCommand = translationResult.Value as SqlQueryCommand<Product>;
            Console.WriteLine($"   Original LINQ: Complex multi-condition expression");
            Console.WriteLine($"   Translated SQL: {translatedCommand?.SqlText ?? "N/A"}\n");
        }
    }

    private static void DemoQueryExecution(IServiceProvider serviceProvider)
    {
        Console.WriteLine("=====================================");
        Console.WriteLine("Demo: Query Execution");
        Console.WriteLine("=====================================\n");

        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<SampleDbContext>();

        // Query 1: Get all electronics
        Console.WriteLine("1. Get all electronics:");
        var electronics = context.Products
            .Where(p => p.Category == "Electronics")
            .OrderBy(p => p.Price)
            .ToList();

        foreach (var product in electronics)
        {
            Console.WriteLine($"   - {product.Name}: ${product.Price:F2} (Stock: {product.Stock})");
        }

        // Query 2: Get products with low stock
        Console.WriteLine("\n2. Products with low stock (< 30):");
        var lowStock = context.Products
            .Where(p => p.Stock < 30)
            .OrderBy(p => p.Stock)
            .ToList();

        foreach (var product in lowStock)
        {
            Console.WriteLine($"   - {product.Name}: Stock = {product.Stock}");
        }

        // Query 3: Get customer orders with product details
        Console.WriteLine("\n3. Customer orders:");
        var orderDetails = from o in context.Orders
                          join c in context.Customers on o.CustomerId equals c.Id
                          join p in context.Products on o.ProductId equals p.Id
                          select new { c.Name, ProductName = p.Name, o.Quantity, o.OrderDate };

        foreach (var order in orderDetails)
        {
            Console.WriteLine($"   - {order.Name} ordered {order.Quantity}x {order.ProductName} on {order.OrderDate:yyyy-MM-dd}");
        }
    }

    private static void DemoComplexQueries(IServiceProvider serviceProvider)
    {
        Console.WriteLine("\n=====================================");
        Console.WriteLine("Demo: Complex Query Patterns");
        Console.WriteLine("=====================================\n");

        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<SampleDbContext>();

        // Aggregation query
        Console.WriteLine("1. Category statistics:");
        var categoryStats = context.Products
            .GroupBy(p => p.Category)
            .Select(g => new
            {
                Category = g.Key,
                Count = g.Count(),
                AveragePrice = g.Average(p => p.Price),
                TotalStock = g.Sum(p => p.Stock)
            })
            .OrderByDescending(s => s.TotalStock)
            .ToList();

        foreach (var stat in categoryStats)
        {
            Console.WriteLine($"   - {stat.Category}: {stat.Count} products, " +
                            $"Avg Price: ${stat.AveragePrice:F2}, Total Stock: {stat.TotalStock}");
        }

        // Subquery pattern
        Console.WriteLine("\n2. Products above average price:");
        var avgPrice = context.Products.Average(p => p.Price);
        var aboveAverage = context.Products
            .Where(p => p.Price > avgPrice)
            .OrderByDescending(p => p.Price)
            .ToList();

        Console.WriteLine($"   Average price: ${avgPrice:F2}");
        foreach (var product in aboveAverage)
        {
            Console.WriteLine($"   - {product.Name}: ${product.Price:F2} " +
                            $"(+${(product.Price - avgPrice):F2} above average)");
        }
    }

    private static void DemoOptimization(IServiceProvider serviceProvider)
    {
        Console.WriteLine("\n=====================================");
        Console.WriteLine("Demo: SQL Query Optimization");
        Console.WriteLine("=====================================\n");

        var optimizer = serviceProvider.GetRequiredService<SqlOptimizer>();
        var logger = serviceProvider.GetRequiredService<ILogger<Program>>();

        // Create a sample SQL query for optimization
        string sampleSql = @"
            SELECT p.Name, p.Price, p.Stock
            FROM Products p
            WHERE p.Category = 'Electronics'
            AND p.Price > 100
            ORDER BY p.Price DESC";

        Console.WriteLine("Original SQL:");
        Console.WriteLine(sampleSql);

        // Parse the SQL using ScriptDom
        var parser = new TSql170Parser(true);
        var fragment = parser.Parse(new System.IO.StringReader(sampleSql), out var errors);

        if (!errors.Any())
        {
            // Apply optimization
            var optimizedFragment = optimizer.OptimizeAsync(fragment.Fragment, default).Result;

            // Generate optimized SQL
            var generator = new Sql170ScriptGenerator();
            generator.GenerateScript(optimizedFragment, out string optimizedSql);

            Console.WriteLine("\nOptimized SQL:");
            Console.WriteLine(optimizedSql);

            // Log optimization details
            SqlTranslatorLog.OptimizationCompleted(logger, optimizedSql);
        }
        else
        {
            Console.WriteLine("Failed to parse SQL for optimization.");
            foreach (var error in errors)
            {
                Console.WriteLine($"   Error: {error.Message}");
            }
        }
    }
}

/// <summary>
/// Sample Entity Framework DbContext for demonstration.
/// </summary>
public class SampleDbContext : DbContext
{
    public DbSet<Product> Products { get; set; }
    public DbSet<Customer> Customers { get; set; }
    public DbSet<Order> Orders { get; set; }

    public SampleDbContext(DbContextOptions<SampleDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configure entity relationships
        modelBuilder.Entity<Order>()
            .HasOne<Customer>()
            .WithMany()
            .HasForeignKey(o => o.CustomerId);

        modelBuilder.Entity<Order>()
            .HasOne<Product>()
            .WithMany()
            .HasForeignKey(o => o.ProductId);

        // Configure decimal precision
        modelBuilder.Entity<Product>()
            .Property(p => p.Price)
            .HasPrecision(18, 2);
    }
}

/// <summary>
/// Sample product entity.
/// </summary>
public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Stock { get; set; }
}

/// <summary>
/// Sample customer entity.
/// </summary>
public class Customer
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
}

/// <summary>
/// Sample order entity.
/// </summary>
public class Order
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public DateTime OrderDate { get; set; }
}

/// <summary>
/// Sample implementation of IDataSchema for in-memory database.
/// </summary>
public class InMemoryDataSchema : IDataSchema
{
    public IGenericResult<TableSchema> GetTableSchema(string tableName)
    {
        // Return simplified schema for demo purposes
        var schema = tableName.ToLower() switch
        {
            "products" => new TableSchema
            {
                TableName = "Products",
                Columns = new[]
                {
                    new ColumnSchema { Name = "Id", DataType = "int", IsPrimaryKey = true },
                    new ColumnSchema { Name = "Name", DataType = "nvarchar(255)" },
                    new ColumnSchema { Name = "Category", DataType = "nvarchar(100)" },
                    new ColumnSchema { Name = "Price", DataType = "decimal(18,2)" },
                    new ColumnSchema { Name = "Stock", DataType = "int" }
                }
            },
            "customers" => new TableSchema
            {
                TableName = "Customers",
                Columns = new[]
                {
                    new ColumnSchema { Name = "Id", DataType = "int", IsPrimaryKey = true },
                    new ColumnSchema { Name = "Name", DataType = "nvarchar(255)" },
                    new ColumnSchema { Name = "Email", DataType = "nvarchar(255)" },
                    new ColumnSchema { Name = "City", DataType = "nvarchar(100)" }
                }
            },
            "orders" => new TableSchema
            {
                TableName = "Orders",
                Columns = new[]
                {
                    new ColumnSchema { Name = "Id", DataType = "int", IsPrimaryKey = true },
                    new ColumnSchema { Name = "CustomerId", DataType = "int" },
                    new ColumnSchema { Name = "ProductId", DataType = "int" },
                    new ColumnSchema { Name = "Quantity", DataType = "int" },
                    new ColumnSchema { Name = "OrderDate", DataType = "datetime" }
                }
            },
            _ => null
        };

        return schema != null
            ? Result.Success(schema)
            : Result.Failure<TableSchema>($"Table '{tableName}' not found");
    }

    public IGenericResult<IEnumerable<string>> GetTableNames()
    {
        return Result.Success<IEnumerable<string>>(new[] { "Products", "Customers", "Orders" });
    }

    public IGenericResult<IEnumerable<RelationshipSchema>> GetRelationships()
    {
        var relationships = new[]
        {
            new RelationshipSchema
            {
                PrimaryTable = "Customers",
                PrimaryKey = "Id",
                ForeignTable = "Orders",
                ForeignKey = "CustomerId"
            },
            new RelationshipSchema
            {
                PrimaryTable = "Products",
                PrimaryKey = "Id",
                ForeignTable = "Orders",
                ForeignKey = "ProductId"
            }
        };

        return Result.Success<IEnumerable<RelationshipSchema>>(relationships);
    }
}

/// <summary>
/// Translation context implementation.
/// </summary>
public class TranslationContext : ITranslationContext
{
    public IDataSchema Schema { get; }
    public Dictionary<string, object> Parameters { get; }
    public TranslationCapabilities Capabilities { get; }

    public TranslationContext(IDataSchema schema)
    {
        Schema = schema;
        Parameters = new Dictionary<string, object>();
        Capabilities = new TranslationCapabilities
        {
            SupportsSubqueries = true,
            SupportsJoins = true,
            SupportsAggregates = true,
            SupportsWindowFunctions = false,
            MaxParameterCount = 2100
        };
    }
}

/// <summary>
/// Table schema for data schema implementation.
/// </summary>
public class TableSchema
{
    public string TableName { get; set; } = string.Empty;
    public ColumnSchema[] Columns { get; set; } = Array.Empty<ColumnSchema>();
}

/// <summary>
/// Column schema for data schema implementation.
/// </summary>
public class ColumnSchema
{
    public string Name { get; set; } = string.Empty;
    public string DataType { get; set; } = string.Empty;
    public bool IsPrimaryKey { get; set; }
    public bool IsNullable { get; set; }
}

/// <summary>
/// Relationship schema for data schema implementation.
/// </summary>
public class RelationshipSchema
{
    public string PrimaryTable { get; set; } = string.Empty;
    public string PrimaryKey { get; set; } = string.Empty;
    public string ForeignTable { get; set; } = string.Empty;
    public string ForeignKey { get; set; } = string.Empty;
}