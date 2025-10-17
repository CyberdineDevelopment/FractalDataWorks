using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using FractalDataWorks.Services.Connections.GraphQL;
using FractalDataWorks.Services.Connections.GraphQL.Commands;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FractalDataWorks.GraphQL.Sample;

/// <summary>
/// Sample application demonstrating FractalDataWorks GraphQL connection capabilities.
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

        // Add HTTP client
        services.AddHttpClient<GraphQLService>();

        // Add GraphQL configuration and service
        var graphQLConfig = new GraphQLConnectionConfiguration
        {
            EndpointUrl = configuration["GraphQLConnection:EndpointUrl"] ?? "https://countries.trevorblades.com/graphql",
            TimeoutSeconds = 30,
            RetryCount = 3,
            MaxQueryDepth = 10
        };

        services.AddSingleton(graphQLConfig);
        services.AddScoped<GraphQLService>();

        var serviceProvider = services.BuildServiceProvider();

        using var scope = serviceProvider.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<GraphQLService>();

        Console.WriteLine("===============================================================================");
        Console.WriteLine("FractalDataWorks GraphQL Connection Sample");
        Console.WriteLine("===============================================================================");
        Console.WriteLine();

        try
        {
            // Example 1: Simple GraphQL query
            await Example1_SimpleQuery(service);

            // Example 2: Query with variables
            await Example2_QueryWithVariables(service);

            // Example 3: Query with nested fields
            await Example3_NestedQuery(service);

            // Example 4: Mutation example
            await Example4_Mutation(service);

            Console.WriteLine();
            Console.WriteLine("===============================================================================");
            Console.WriteLine("All GraphQL examples completed successfully!");
            Console.WriteLine("===============================================================================");
        }
        catch (Exception ex)
        {
            Console.WriteLine();
            Console.WriteLine($"ERROR: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
        }
    }

    private static async Task Example1_SimpleQuery(GraphQLService service)
    {
        Console.WriteLine("Example 1: Simple GraphQL Query (All Countries)");
        Console.WriteLine("----------------------------------------");

        var query = @"
            query {
                countries {
                    code
                    name
                    capital
                }
            }";

        var command = new GraphQLConnectionCommand(query);

        var result = await service.Execute<CountriesResponse>(command, CancellationToken.None);

        if (result.IsSuccess)
        {
            Console.WriteLine($"Retrieved {result.Value.Countries.Count} countries");
            foreach (var country in result.Value.Countries.Take(5))
            {
                Console.WriteLine($"  {country.Code}: {country.Name} (Capital: {country.Capital ?? "N/A"})");
            }
            if (result.Value.Countries.Count > 5)
            {
                Console.WriteLine($"  ... and {result.Value.Countries.Count - 5} more");
            }
        }
        else
        {
            Console.WriteLine($"Query failed: {result.CurrentMessage}");
        }

        Console.WriteLine();
    }

    private static async Task Example2_QueryWithVariables(GraphQLService service)
    {
        Console.WriteLine("Example 2: GraphQL Query with Variables");
        Console.WriteLine("----------------------------------------");

        var query = @"
            query GetCountry($code: ID!) {
                country(code: $code) {
                    code
                    name
                    capital
                    currency
                    languages {
                        code
                        name
                    }
                }
            }";

        var variables = new Dictionary<string, object>
        {
            ["code"] = "US"
        };

        var command = new GraphQLConnectionCommand(query, variables, "GetCountry");

        var result = await service.Execute<CountryResponse>(command, CancellationToken.None);

        if (result.IsSuccess && result.Value.Country != null)
        {
            var country = result.Value.Country;
            Console.WriteLine($"Country: {country.Name} ({country.Code})");
            Console.WriteLine($"Capital: {country.Capital ?? "N/A"}");
            Console.WriteLine($"Currency: {country.Currency ?? "N/A"}");
            if (country.Languages != null && country.Languages.Any())
            {
                Console.WriteLine($"Languages: {string.Join(", ", country.Languages.Select(l => l.Name))}");
            }
        }
        else
        {
            Console.WriteLine($"Query failed: {result.CurrentMessage}");
        }

        Console.WriteLine();
    }

    private static async Task Example3_NestedQuery(GraphQLService service)
    {
        Console.WriteLine("Example 3: GraphQL Query with Nested Fields");
        Console.WriteLine("----------------------------------------");

        var query = @"
            query {
                continents {
                    code
                    name
                    countries {
                        code
                        name
                    }
                }
            }";

        var command = new GraphQLConnectionCommand(query);

        var result = await service.Execute<ContinentsResponse>(command, CancellationToken.None);

        if (result.IsSuccess)
        {
            Console.WriteLine($"Retrieved {result.Value.Continents.Count} continents");
            foreach (var continent in result.Value.Continents.Take(3))
            {
                Console.WriteLine($"  {continent.Name}: {continent.Countries.Count} countries");
            }
        }
        else
        {
            Console.WriteLine($"Query failed: {result.CurrentMessage}");
        }

        Console.WriteLine();
    }

    private static async Task Example4_Mutation(GraphQLService service)
    {
        Console.WriteLine("Example 4: GraphQL Mutation (Conceptual)");
        Console.WriteLine("----------------------------------------");

        // Note: This is a conceptual example. The countries API is read-only.
        // In a real application with a mutable GraphQL API, you would use:

        var mutation = @"
            mutation CreateUser($name: String!, $email: String!) {
                createUser(name: $name, email: $email) {
                    id
                    name
                    email
                }
            }";

        var variables = new Dictionary<string, object>
        {
            ["name"] = "John Doe",
            ["email"] = "john.doe@example.com"
        };

        Console.WriteLine("Mutation structure:");
        Console.WriteLine(mutation);
        Console.WriteLine();
        Console.WriteLine("Variables:");
        foreach (var kvp in variables)
        {
            Console.WriteLine($"  {kvp.Key}: {kvp.Value}");
        }

        Console.WriteLine();
        Console.WriteLine("Note: This is a conceptual example. The countries API is read-only.");

        Console.WriteLine();
    }
}

// Response models
public sealed class CountriesResponse
{
    public List<Country> Countries { get; set; } = new();
}

public sealed class CountryResponse
{
    public Country? Country { get; set; }
}

public sealed class ContinentsResponse
{
    public List<Continent> Continents { get; set; } = new();
}

public sealed class Country
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Capital { get; set; }
    public string? Currency { get; set; }
    public List<Language>? Languages { get; set; }
}

public sealed class Continent
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public List<Country> Countries { get; set; } = new();
}

public sealed class Language
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}
