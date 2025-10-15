using System;
using System.Linq;
using System.Threading.Tasks;
using DataConceptDemo.Models;
using DataConceptDemo.Transformers;
using FractalDataWorks.Data.DataSets;
using FractalDataWorks.Data.Execution;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace DataConceptDemo;

/// <summary>
/// Milestone 1 Demo: Multi-source data concepts.
/// Demonstrates the foundation for querying data from multiple sources with unified schema.
/// </summary>
internal sealed class Program
{
    private static async Task<int> Main(string[] args)
    {
        Console.WriteLine("=== DataPath Forward - Milestone 1 Demo ===");
        Console.WriteLine("Multi-source Data Concepts\n");

        try
        {
            // Setup configuration
            var configuration = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            // Setup logging
            using var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder
                    .AddConfiguration(configuration.GetSection("Logging"))
                    .AddConsole();
            });

            var conceptLogger = loggerFactory.CreateLogger<DataConceptRegistry>();
            var executorLogger = loggerFactory.CreateLogger<DataConceptQueryExecutor>();

            // Create registry and executor
            var conceptRegistry = new DataConceptRegistry(configuration, conceptLogger);
            var queryExecutor = new DataConceptQueryExecutor(conceptRegistry, executorLogger);

            // Register mock transformer
            queryExecutor.RegisterTransformer(new MockTransformer());

            Console.WriteLine("✓ Configuration loaded");
            Console.WriteLine("✓ DataConceptRegistry initialized");
            Console.WriteLine("✓ DataConceptQueryExecutor created");
            Console.WriteLine("✓ Mock transformer registered\n");

            // Demonstrate concept retrieval
            Console.WriteLine("--- Testing Data Concept Registry ---");
            var concept = conceptRegistry.GetDataConcept("TransactionData");
            Console.WriteLine($"✓ Retrieved concept: {concept.DataSetName}");
            Console.WriteLine($"  Description: {concept.Description}");
            Console.WriteLine($"  Sources: {concept.Sources.Count}");

            foreach (var source in concept.Sources)
            {
                Console.WriteLine($"    - {source.Key}: ConnectionType={source.Value.ConnectionType}, Priority={source.Value.Priority}");
            }

            Console.WriteLine();

            // Demonstrate query execution
            Console.WriteLine("--- Testing Query Execution ---");
            var result = await queryExecutor.Execute<Transaction>("TransactionData");

            if (result.IsSuccess)
            {
                Console.WriteLine($"✓ Query executed successfully");
                Console.WriteLine($"  Records returned: {result.Value.Count()}");
                Console.WriteLine($"  (Milestone 1: Infrastructure in place, actual extraction coming in later milestones)");
            }
            else
            {
                Console.WriteLine($"✗ Query failed: {result.IsFailure}");
            }

            Console.WriteLine();
            Console.WriteLine("=== Milestone 1 Complete ===");
            Console.WriteLine("✓ Project structure created");
            Console.WriteLine("✓ DataConceptRegistry working");
            Console.WriteLine("✓ DataConceptQueryExecutor structure in place");
            Console.WriteLine("✓ Transformer infrastructure ready");
            Console.WriteLine();
            Console.WriteLine("Next: Milestone 2 will add static transformers with TypeCollection pattern");

            return 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ Error: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
            return 1;
        }
    }
}
