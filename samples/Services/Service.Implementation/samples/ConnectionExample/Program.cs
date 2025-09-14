using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FractalDataWorks.Services.Connections;
using FractalDataWorks.Services.Connections.Abstractions;
using FractalDataWorks.Services.Connections.MsSql;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

Console.WriteLine("FractalDataWorks Connection Service Demo");
Console.WriteLine("=========================================");

// Setup dependency injection
var services = new ServiceCollection();
services.AddLogging(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Information));

var serviceProvider = services.BuildServiceProvider();
var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();

// Create connection provider and register the MsSql factory
var provider = new ConnectionProvider(loggerFactory.CreateLogger<ConnectionProvider>());
var msSqlFactory = new MsSqlConnectionFactory(loggerFactory);
provider.RegisterFactory(msSqlFactory);

Console.WriteLine($"Registered connection types: {string.Join(", ", provider.GetSupportedConnectionTypes())}");

// Test connection configuration
var config = new MsSqlConfiguration
{
    ConnectionString = "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=master;Integrated Security=true;",
    CommandTimeout = 30,
    MaxRetryCount = 3
};

Console.WriteLine($"Testing connection support for '{config.ConnectionTypeName}': {provider.IsConnectionTypeSupported(config.ConnectionTypeName)}");

try
{
    // Create and test a connection
    Console.WriteLine("Creating connection...");
    using var connection = await provider.Create(config);

    Console.WriteLine($"Connection created successfully!");
    Console.WriteLine($"  Connection ID: {connection.ConnectionId}");
    Console.WriteLine($"  Provider Name: {connection.ProviderName}");

    // Create a test command
    var testCommand = new TestDataCommand("Query", "Users", new Dictionary<string, object>());

    Console.WriteLine("Executing test command...");
    var result = await connection.Execute<object>(testCommand);

    if (result.IsSuccess)
    {
        Console.WriteLine("✅ Command executed successfully!");
    }
    else
    {
        Console.WriteLine($"❌ Command failed: {result.Error}");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"❌ Error: {ex.Message}");
}

Console.WriteLine("Demo completed!");

// Simple test implementation of IDataCommand
public class TestDataCommand : IDataCommand
{
    public TestDataCommand(string commandType, string entityName, IReadOnlyDictionary<string, object> parameters)
    {
        CommandType = commandType;
        EntityName = entityName;
        Parameters = parameters;
        Filters = new Dictionary<string, object>();
        Values = new Dictionary<string, object>();
    }

    public string CommandType { get; }
    public string EntityName { get; }
    public IReadOnlyDictionary<string, object> Parameters { get; }
    public IReadOnlyDictionary<string, object> Filters { get; }
    public IReadOnlyDictionary<string, object> Values { get; }
}
