using System.Globalization;
using FractalDataWorks.MsSql.Sample.Models;
using FractalDataWorks.Services.Connections.MsSql;
using FractalDataWorks.Services.Connections.MsSql.Commands;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FractalDataWorks.MsSql.Sample;

/// <summary>
/// Sample application demonstrating FractalDataWorks MsSql connection capabilities.
/// </summary>
public static class Program
{
    public static async Task Main(string[] args)
    {
        // Build configuration
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        // Setup dependency injection
        var services = new ServiceCollection();

        // Add logging
        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.AddConfiguration(configuration.GetSection("Logging"));
        });

        // Add MsSql configuration and service
        var msSqlConfig = new MsSqlConfiguration
        {
            ConnectionString = configuration.GetConnectionString("FractalSample")
                ?? throw new InvalidOperationException("Connection string not found"),
            CommandTimeoutSeconds = 30,
            EnableRetryLogic = true,
            MaxRetryAttempts = 3,
            EnableSqlLogging = true,
            MaxSqlLogLength = 1000,
            UseTransactions = false,
            SchemaMappings = new Dictionary<string, string>
            {
                ["Customers"] = "crm.Customers",
                ["Products"] = "inventory.Products",
                ["Categories"] = "inventory.Categories",
                ["Orders"] = "sales.Orders",
                ["OrderItems"] = "sales.OrderItems"
            }
        };

        services.AddSingleton(msSqlConfig);
        services.AddScoped<MsSqlService>();

        var serviceProvider = services.BuildServiceProvider();

        using var scope = serviceProvider.CreateScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<MsSqlService>>();
        var service = scope.ServiceProvider.GetRequiredService<MsSqlService>();

        Console.WriteLine("================================================================================");
        Console.WriteLine("FractalDataWorks MsSql Connection Sample");
        Console.WriteLine("================================================================================");
        Console.WriteLine();

        try
        {
            // Example 1: Query all customers
            await Example1_QueryAllCustomers(service);

            // Example 2: Query with filtering
            await Example2_QueryWithFilter(service);

            // Example 3: Scalar queries (COUNT, SUM)
            await Example3_ScalarQueries(service);

            // Example 4: Join queries
            await Example4_JoinQueries(service);

            // Example 5: Paging and sorting
            await Example5_PagingAndSorting(service);

            // Example 6: Insert operation
            await Example6_InsertOperation(service);

            // Example 7: Update operation
            await Example7_UpdateOperation(service);

            // Example 8: Delete operation
            await Example8_DeleteOperation(service);

            // Example 9: Transaction handling
            await Example9_TransactionHandling(service, msSqlConfig);

            // Example 10: Error handling and retry logic
            await Example10_ErrorHandling(service);

            Console.WriteLine();
            Console.WriteLine("================================================================================");
            Console.WriteLine("All examples completed successfully!");
            Console.WriteLine("================================================================================");
        }
        catch (Exception ex)
        {
            Console.WriteLine();
            Console.WriteLine($"ERROR: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
            return;
        }
    }

    private static async Task Example1_QueryAllCustomers(MsSqlService service)
    {
        Console.WriteLine("Example 1: Query All Customers");
        Console.WriteLine("----------------------------------------");

        var sql = "SELECT * FROM [crm].[Customers] ORDER BY CustomerId";
        var command = new SqlConnectionCommand(sql);

        var result = await service.Execute<IEnumerable<Customer>>(command, CancellationToken.None);

        if (result.IsSuccess)
        {
            Console.WriteLine($"Found {result.Value.Count()} customers:");
            foreach (var customer in result.Value.Take(5))
            {
                Console.WriteLine($"  {customer.CustomerId}: {customer.FullName} ({customer.Email}) - {customer.CustomerType}");
            }
            if (result.Value.Count() > 5)
            {
                Console.WriteLine($"  ... and {result.Value.Count() - 5} more");
            }
        }
        else
        {
            Console.WriteLine($"Query failed: {result.CurrentMessage}");
        }

        Console.WriteLine();
    }

    private static async Task Example2_QueryWithFilter(MsSqlService service)
    {
        Console.WriteLine("Example 2: Query with Filter (Premium & VIP Customers)");
        Console.WriteLine("----------------------------------------");

        var sql = @"
            SELECT * FROM [crm].[Customers]
            WHERE CustomerType IN ('Premium', 'VIP')
            AND IsActive = @isActive
            ORDER BY TotalSpent DESC";

        var parameters = new Dictionary<string, object>
        {
            ["isActive"] = true
        };

        var command = new SqlConnectionCommand(sql, parameters);
        var result = await service.Execute<IEnumerable<Customer>>(command, CancellationToken.None);

        if (result.IsSuccess)
        {
            Console.WriteLine($"Found {result.Value.Count()} premium/VIP customers:");
            foreach (var customer in result.Value)
            {
                Console.WriteLine($"  {customer.FullName} - {customer.CustomerType} - Total Spent: ${customer.TotalSpent:N2}");
            }
        }
        else
        {
            Console.WriteLine($"Query failed: {result.CurrentMessage}");
        }

        Console.WriteLine();
    }

    private static async Task Example3_ScalarQueries(MsSqlService service)
    {
        Console.WriteLine("Example 3: Scalar Queries (COUNT, SUM, AVG)");
        Console.WriteLine("----------------------------------------");

        // COUNT example
        var countSql = "SELECT COUNT(*) FROM [sales].[Orders] WHERE Status = @status";
        var countParams = new Dictionary<string, object> { ["status"] = "Delivered" };
        var countCommand = new SqlConnectionCommand(countSql, countParams);
        var countResult = await service.Execute<int>(countCommand, CancellationToken.None);

        if (countResult.IsSuccess)
        {
            Console.WriteLine($"Delivered orders count: {countResult.Value}");
        }

        // SUM example
        var sumSql = "SELECT SUM(TotalAmount) FROM [sales].[Orders] WHERE Status = 'Delivered'";
        var sumCommand = new SqlConnectionCommand(sumSql);
        var sumResult = await service.Execute<decimal>(sumCommand, CancellationToken.None);

        if (sumResult.IsSuccess)
        {
            Console.WriteLine($"Total revenue from delivered orders: ${sumResult.Value:N2}");
        }

        // AVG example
        var avgSql = "SELECT AVG(TotalAmount) FROM [sales].[Orders]";
        var avgCommand = new SqlConnectionCommand(avgSql);
        var avgResult = await service.Execute<decimal>(avgCommand, CancellationToken.None);

        if (avgResult.IsSuccess)
        {
            Console.WriteLine($"Average order value: ${avgResult.Value:N2}");
        }

        Console.WriteLine();
    }

    private static async Task Example4_JoinQueries(MsSqlService service)
    {
        Console.WriteLine("Example 4: Join Queries (Orders with Customer Info)");
        Console.WriteLine("----------------------------------------");

        var sql = @"
            SELECT
                o.OrderId,
                o.OrderDate,
                (c.FirstName + ' ' + c.LastName) AS CustomerName,
                c.Email AS CustomerEmail,
                (SELECT COUNT(*) FROM [sales].[OrderItems] WHERE OrderId = o.OrderId) AS ItemCount,
                o.TotalAmount,
                o.Status,
                o.PaymentStatus
            FROM [sales].[Orders] o
            INNER JOIN [crm].[Customers] c ON o.CustomerId = c.CustomerId
            ORDER BY o.OrderDate DESC";

        var command = new SqlConnectionCommand(sql);
        var result = await service.Execute<IEnumerable<OrderSummary>>(command, CancellationToken.None);

        if (result.IsSuccess)
        {
            Console.WriteLine($"Found {result.Value.Count()} orders:");
            foreach (var order in result.Value)
            {
                Console.WriteLine($"  Order #{order.OrderId} - {order.OrderDate:yyyy-MM-dd} - {order.CustomerName}");
                Console.WriteLine($"    Items: {order.ItemCount}, Total: ${order.TotalAmount:N2}, Status: {order.Status}/{order.PaymentStatus}");
            }
        }
        else
        {
            Console.WriteLine($"Query failed: {result.CurrentMessage}");
        }

        Console.WriteLine();
    }

    private static async Task Example5_PagingAndSorting(MsSqlService service)
    {
        Console.WriteLine("Example 5: Paging and Sorting (Products Page 2)");
        Console.WriteLine("----------------------------------------");

        var sql = @"
            SELECT * FROM [inventory].[Products]
            WHERE IsActive = @isActive
            ORDER BY Price DESC
            OFFSET @offset ROWS
            FETCH NEXT @pageSize ROWS ONLY";

        var parameters = new Dictionary<string, object>
        {
            ["isActive"] = true,
            ["offset"] = 5,      // Skip first 5
            ["pageSize"] = 5     // Take next 5
        };

        var command = new SqlConnectionCommand(sql, parameters);
        var result = await service.Execute<IEnumerable<Product>>(command, CancellationToken.None);

        if (result.IsSuccess)
        {
            Console.WriteLine($"Products (Page 2, 5 per page):");
            foreach (var product in result.Value)
            {
                Console.WriteLine($"  {product.ProductName} - ${product.Price:N2} (Stock: {product.QuantityInStock})");
            }
        }
        else
        {
            Console.WriteLine($"Query failed: {result.CurrentMessage}");
        }

        Console.WriteLine();
    }

    private static async Task Example6_InsertOperation(MsSqlService service)
    {
        Console.WriteLine("Example 6: Insert Operation (New Category)");
        Console.WriteLine("----------------------------------------");

        var sql = @"
            INSERT INTO [inventory].[Categories] (CategoryName, Description, IsActive)
            VALUES (@name, @description, @isActive);
            SELECT SCOPE_IDENTITY();";

        var parameters = new Dictionary<string, object>
        {
            ["name"] = $"Test Category {DateTime.Now.Ticks}",
            ["description"] = "Created by sample application",
            ["isActive"] = true
        };

        var command = new SqlConnectionCommand(sql, parameters);
        var result = await service.Execute<decimal>(command, CancellationToken.None);

        if (result.IsSuccess)
        {
            Console.WriteLine($"New category created with ID: {result.Value}");
        }
        else
        {
            Console.WriteLine($"Insert failed: {result.CurrentMessage}");
        }

        Console.WriteLine();
    }

    private static async Task Example7_UpdateOperation(MsSqlService service)
    {
        Console.WriteLine("Example 7: Update Operation (Update Product Price)");
        Console.WriteLine("----------------------------------------");

        // First, get a product
        var selectSql = "SELECT TOP 1 * FROM [inventory].[Products] WHERE IsActive = 1";
        var selectCommand = new SqlConnectionCommand(selectSql);
        var selectResult = await service.Execute<IEnumerable<Product>>(selectCommand, CancellationToken.None);

        if (selectResult.IsSuccess && selectResult.Value.Any())
        {
            var product = selectResult.Value.First();
            var newPrice = product.Price * 1.10m; // 10% increase

            var updateSql = @"
                UPDATE [inventory].[Products]
                SET Price = @newPrice, LastModifiedDate = GETUTCDATE()
                WHERE ProductId = @productId";

            var parameters = new Dictionary<string, object>
            {
                ["newPrice"] = newPrice,
                ["productId"] = product.ProductId
            };

            var updateCommand = new SqlConnectionCommand(updateSql, parameters);
            var updateResult = await service.Execute<int>(updateCommand, CancellationToken.None);

            if (updateResult.IsSuccess)
            {
                Console.WriteLine($"Updated product '{product.ProductName}':");
                Console.WriteLine($"  Old price: ${product.Price:N2}");
                Console.WriteLine($"  New price: ${newPrice:N2}");
                Console.WriteLine($"  Rows affected: {updateResult.Value}");
            }
            else
            {
                Console.WriteLine($"Update failed: {updateResult.CurrentMessage}");
            }
        }

        Console.WriteLine();
    }

    private static async Task Example8_DeleteOperation(MsSqlService service)
    {
        Console.WriteLine("Example 8: Delete Operation (Remove Test Category)");
        Console.WriteLine("----------------------------------------");

        var sql = @"
            DELETE FROM [inventory].[Categories]
            WHERE CategoryName LIKE 'Test Category%'";

        var command = new SqlConnectionCommand(sql);
        var result = await service.Execute<int>(command, CancellationToken.None);

        if (result.IsSuccess)
        {
            Console.WriteLine($"Deleted {result.Value} test category/categories");
        }
        else
        {
            Console.WriteLine($"Delete failed: {result.CurrentMessage}");
        }

        Console.WriteLine();
    }

    private static async Task Example9_TransactionHandling(MsSqlService service, MsSqlConfiguration config)
    {
        Console.WriteLine("Example 9: Transaction Handling");
        Console.WriteLine("----------------------------------------");

        // Enable transactions temporarily
        var originalUseTransactions = config.UseTransactions;

        Console.WriteLine("Note: Transactions are controlled via configuration.");
        Console.WriteLine($"Current setting: UseTransactions = {config.UseTransactions}");
        Console.WriteLine("In production, set UseTransactions = true for operations that need atomicity.");
        Console.WriteLine();
    }

    private static async Task Example10_ErrorHandling(MsSqlService service)
    {
        Console.WriteLine("Example 10: Error Handling and Retry Logic");
        Console.WriteLine("----------------------------------------");

        // Intentional syntax error
        var sql = "SELECT * FROM NonExistentTable";
        var command = new SqlConnectionCommand(sql);
        var result = await service.Execute<IEnumerable<Customer>>(command, CancellationToken.None);

        if (!result.IsSuccess)
        {
            Console.WriteLine($"Expected error occurred: {result.CurrentMessage}");
            Console.WriteLine("Error handling works correctly!");
        }
        else
        {
            Console.WriteLine("Unexpected: Query succeeded when it should have failed");
        }

        Console.WriteLine();
        Console.WriteLine("Note: Retry logic automatically handles transient errors:");
        Console.WriteLine("- Timeouts (error -2)");
        Console.WriteLine("- Deadlocks (error 1205)");
        Console.WriteLine("- Connection errors (errors 2, 20, 64, 233, 10053, 10054, 10060)");
        Console.WriteLine("- Azure SQL transient errors (errors 40197, 40501, 40613)");
        Console.WriteLine();
    }
}
