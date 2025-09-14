using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using FractalDataWorks.Services;
using FractalDataWorks.Services.Extensions;
using FractalDataWorks.Services.DataGateway.Abstractions;
using FractalDataWorks.DataSets.Abstractions;
using FractalDataWorks.Services.Connections.Abstractions;

namespace ConnectionProviderDemo;

/// <summary>
/// Demonstrates the Universal Data Access Layer - showing how the same LINQ query
/// can be executed against different data sources (SQL, REST API, File) and return
/// the same results.
/// </summary>
/// <remarks>
/// This demo showcases the complete implementation of Steps 13-23:
/// - ServiceFactoryProvider for managing connection factories
/// - DataGateway for orchestrating query execution
/// - ConnectionTypes for connection discovery and selection
/// - Same LINQ query working across multiple backend types
/// </remarks>
public class Program
{
    public static async Task Main(string[] args)
    {
        Console.WriteLine("=== Universal Data Access Layer Demo ===");
        Console.WriteLine("Demonstrating the same query executed across different data sources");
        Console.WriteLine();

        // Step 1: Setup Dependency Injection Container
        var services = new ServiceCollection();

        // Step 2: Configure logging
        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Information);
        });

        // Step 3: Add configuration
        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: true)
            .AddInMemoryCollection(GetSampleConfiguration())
            .Build();

        services.AddSingleton<IConfiguration>(configuration);

        // Step 4: Register the ServiceFactoryProvider
        services.AddServiceFactoryProvider();

        // Step 5: Register connection factories (this would normally use the generated ConnectionTypes)
        services.RegisterConnectionFactories(builder =>
        {
            // Note: In a real implementation, these factories would be auto-discovered
            // from the generated ConnectionTypes class. For the demo, we're showing
            // the manual registration pattern that the generator would produce.
            
            Console.WriteLine("Registering connection factories...");
            
            // MsSql factory registration
            // builder.RegisterFactory<MsSqlConnectionFactory>("MsSql", ServiceLifetime.Scoped);
            
            // Rest API factory registration  
            // builder.RegisterFactory<RestConnectionFactory>("Rest", ServiceLifetime.Transient);
            
            // File system factory registration
            // builder.RegisterFactory<FileConnectionFactory>("File", ServiceLifetime.Transient);
            
            Console.WriteLine("Connection factories registered (Note: Actual factories not implemented in this demo)");
        });

        // Step 6: Register DataGateway
        services.AddTransient<IDataGateway, DataGateway.DataGateway>();

        // Step 7: Build the service provider
        var serviceProvider = services.BuildServiceProvider();

        try
        {
            // Step 8: Get the DataGateway
            var dataProvider = serviceProvider.GetRequiredService<IDataGateway>();
            var logger = serviceProvider.GetRequiredService<ILogger<Program>>();

            logger.LogInformation("DataGateway initialized successfully");

            // Step 9: Demo the universal query interface
            await DemonstrateUniversalDataAccess(dataProvider, logger);

        }
        catch (Exception ex)
        {
            Console.WriteLine($"Demo failed: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
        }
        finally
        {
            await serviceProvider.DisposeAsync();
        }

        Console.WriteLine();
        Console.WriteLine("Demo completed. Press any key to exit...");
        Console.ReadKey();
    }

    /// <summary>
    /// Demonstrates the universal data access pattern - same query, different backends.
    /// </summary>
    /// <param name="dataProvider">The data provider for executing queries.</param>
    /// <param name="logger">Logger for demonstration output.</param>
    private static async Task DemonstrateUniversalDataAccess(IDataGateway dataProvider, ILogger logger)
    {
        Console.WriteLine();
        Console.WriteLine("=== Universal Query Demonstration ===");
        Console.WriteLine();

        // Note: In the actual implementation, this would look like:
        // var query = DataSets.Weather
        //     .Where(w => w.Location == "Seattle" && w.Date > DateTime.Now.AddDays(-7))
        //     .Select(w => new { w.Temperature, w.Date, w.Location });

        Console.WriteLine("Sample Query (conceptual):");
        Console.WriteLine("var query = DataSets.Weather");
        Console.WriteLine("    .Where(w => w.Location == \"Seattle\" && w.Date > DateTime.Now.AddDays(-7))");
        Console.WriteLine("    .Select(w => new { w.Temperature, w.Date, w.Location });");
        Console.WriteLine();

        // Demonstrate the pattern with each connection type
        await DemonstrateConnectionType("MsSql", 
            "SELECT Temperature, Date, Location FROM Weather WHERE Location = 'Seattle' AND Date > @date",
            dataProvider, logger);

        await DemonstrateConnectionType("Rest", 
            "GET /api/weather?location=Seattle&from=2024-09-02",
            dataProvider, logger);

        await DemonstrateConnectionType("File", 
            "Read weather_seattle_*.csv, filter by date range",
            dataProvider, logger);

        Console.WriteLine("=== Factory Provider Demonstration ===");
        Console.WriteLine();

        // Demonstrate factory provider capabilities
        var factoryProvider = dataProvider.GetType()
            .GetField("_factoryProvider", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.GetValue(dataProvider) as IServiceFactoryProvider;

        if (factoryProvider != null)
        {
            Console.WriteLine("Available connection types:");
            foreach (var typeName in factoryProvider.GetRegisteredTypeNames())
            {
                Console.WriteLine($"  - {typeName}: {(factoryProvider.IsRegistered(typeName) ? "✓ Registered" : "✗ Not registered")}");
            }
        }
        else
        {
            Console.WriteLine("Factory provider not accessible via reflection (expected in production)");
        }

        Console.WriteLine();
        Console.WriteLine("=== Architecture Benefits ===");
        Console.WriteLine("✓ Single query interface across all data sources");
        Console.WriteLine("✓ Connection-specific translators handle query conversion");
        Console.WriteLine("✓ Connection-specific mappers handle result conversion");
        Console.WriteLine("✓ ServiceFactoryProvider enables dynamic connection creation");
        Console.WriteLine("✓ Configuration-driven connection selection");
        Console.WriteLine("✓ Type-safe query building with IntelliSense support");
    }

    /// <summary>
    /// Demonstrates how each connection type would handle the same logical query.
    /// </summary>
    /// <param name="connectionType">The connection type name.</param>
    /// <param name="translatedQuery">How the query would be translated for this connection.</param>
    /// <param name="dataProvider">The data provider instance.</param>
    /// <param name="logger">Logger for output.</param>
    private static async Task DemonstrateConnectionType(string connectionType, string translatedQuery, IDataGateway dataProvider, ILogger logger)
    {
        Console.WriteLine($"Connection Type: {connectionType}");
        Console.WriteLine($"Translated Query: {translatedQuery}");

        try
        {
            // In a real implementation, this would execute the actual query
            // var result = await dataProvider.Execute<WeatherData>(query, connectionType);

            // For the demo, we simulate the execution
            logger.LogInformation("Executing query against {ConnectionType}", connectionType);
            
            // Simulate processing time
            await Task.Delay(100);

            Console.WriteLine($"Result: Simulated execution successful");
            Console.WriteLine($"Note: Would return WeatherData[] with Seattle weather from last 7 days");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            Console.WriteLine("This is expected since actual connection implementations are not included in this demo");
        }

        Console.WriteLine();
    }

    /// <summary>
    /// Provides sample configuration for the demo.
    /// </summary>
    /// <returns>Key-value pairs for configuration.</returns>
    private static System.Collections.Generic.Dictionary<string, string?> GetSampleConfiguration()
    {
        return new System.Collections.Generic.Dictionary<string, string?>
        {
            // Connection configurations
            ["Connections:SqlServer:ConnectionType"] = "MsSql",
            ["Connections:SqlServer:ConnectionString"] = "Server=localhost;Database=WeatherDB;Integrated Security=true",
            ["Connections:SqlServer:Lifetime"] = "Scoped",

            ["Connections:WeatherAPI:ConnectionType"] = "Rest", 
            ["Connections:WeatherAPI:BaseUrl"] = "https://api.weather.com",
            ["Connections:WeatherAPI:ApiKey"] = "demo-api-key",
            ["Connections:WeatherAPI:Lifetime"] = "Transient",

            ["Connections:LocalFiles:ConnectionType"] = "File",
            ["Connections:LocalFiles:BasePath"] = "./data",
            ["Connections:LocalFiles:Lifetime"] = "Transient",

            // DataSet configurations
            ["DataSets:Weather:Sources:0:ConnectionType"] = "MsSql",
            ["DataSets:Weather:Sources:0:Table"] = "Weather", 
            ["DataSets:Weather:Sources:0:Schema"] = "dbo",
            ["DataSets:Weather:Sources:0:Priority"] = "1",

            ["DataSets:Weather:Sources:1:ConnectionType"] = "Rest",
            ["DataSets:Weather:Sources:1:Endpoint"] = "/api/v1/weather",
            ["DataSets:Weather:Sources:1:Priority"] = "2",

            ["DataSets:Weather:Sources:2:ConnectionType"] = "File", 
            ["DataSets:Weather:Sources:2:PathPattern"] = "weather_*.csv",
            ["DataSets:Weather:Sources:2:Priority"] = "3"
        };
    }
}

/// <summary>
/// Sample data model that would be generated from DataSet configuration.
/// </summary>
/// <remarks>
/// In the actual implementation, this would be generated by the DataSetCollectionGenerator
/// based on DataSetBase implementations or configuration files.
/// </remarks>
public class WeatherData
{
    public string Location { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public double Temperature { get; set; }
    public double Humidity { get; set; }
    public string Forecast { get; set; } = string.Empty;
}