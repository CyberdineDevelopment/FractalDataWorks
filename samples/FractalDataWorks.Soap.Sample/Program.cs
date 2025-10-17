using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using FractalDataWorks.Services.Connections.Soap;
using FractalDataWorks.Services.Connections.Soap.Commands;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FractalDataWorks.Soap.Sample;

/// <summary>
/// Sample application demonstrating FractalDataWorks SOAP web service connection capabilities.
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
        services.AddHttpClient<SoapService>();

        // Add SOAP configuration and service
        var soapConfig = new SoapConnectionConfiguration
        {
            EndpointUrl = configuration["SoapConnection:EndpointUrl"] ?? "http://www.dneonline.com/calculator.asmx",
            SoapVersion = "1.1",
            XmlNamespace = "http://tempuri.org/",
            SoapAction = "http://tempuri.org/Add",
            TimeoutSeconds = 30,
            RetryCount = 3
        };

        services.AddSingleton(soapConfig);
        services.AddScoped<SoapService>();

        var serviceProvider = services.BuildServiceProvider();

        using var scope = serviceProvider.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<SoapService>();

        Console.WriteLine("===============================================================================");
        Console.WriteLine("FractalDataWorks SOAP Connection Sample");
        Console.WriteLine("===============================================================================");
        Console.WriteLine();

        try
        {
            // Example 1: Simple SOAP method invocation
            await Example1_SimpleSoapCall(service);

            // Example 2: SOAP call with multiple parameters
            await Example2_MultipleParameters(service);

            // Example 3: SOAP call with custom headers
            await Example3_CustomHeaders(service);

            Console.WriteLine();
            Console.WriteLine("===============================================================================");
            Console.WriteLine("All SOAP examples completed successfully!");
            Console.WriteLine("===============================================================================");
        }
        catch (Exception ex)
        {
            Console.WriteLine();
            Console.WriteLine($"ERROR: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
        }
    }

    private static async Task Example1_SimpleSoapCall(SoapService service)
    {
        Console.WriteLine("Example 1: Simple SOAP Method Call (Add)");
        Console.WriteLine("----------------------------------------");

        var parameters = new Dictionary<string, object>
        {
            ["intA"] = 10,
            ["intB"] = 20
        };

        var command = new SoapConnectionCommand(
            "Add",
            parameters,
            "http://tempuri.org/Add"
        );

        var result = await service.Execute<string>(command, CancellationToken.None);

        if (result.IsSuccess)
        {
            Console.WriteLine($"SOAP method invoked successfully");
            Console.WriteLine($"Response: {result.Value}");
        }
        else
        {
            Console.WriteLine($"Request failed: {result.CurrentMessage}");
        }

        Console.WriteLine();
    }

    private static async Task Example2_MultipleParameters(SoapService service)
    {
        Console.WriteLine("Example 2: SOAP Call with Multiple Parameters");
        Console.WriteLine("----------------------------------------");

        var parameters = new Dictionary<string, object>
        {
            ["intA"] = 100,
            ["intB"] = 50
        };

        var command = new SoapConnectionCommand(
            "Subtract",
            parameters,
            "http://tempuri.org/Subtract"
        );

        var result = await service.Execute<string>(command, CancellationToken.None);

        if (result.IsSuccess)
        {
            Console.WriteLine($"Subtraction result received");
            Console.WriteLine($"Response: {result.Value}");
        }
        else
        {
            Console.WriteLine($"Request failed: {result.CurrentMessage}");
        }

        Console.WriteLine();
    }

    private static async Task Example3_CustomHeaders(SoapService service)
    {
        Console.WriteLine("Example 3: SOAP Call with Custom Headers");
        Console.WriteLine("----------------------------------------");

        var parameters = new Dictionary<string, object>
        {
            ["intA"] = 5,
            ["intB"] = 3
        };

        var customHeaders = new Dictionary<string, string>
        {
            ["X-Request-ID"] = Guid.NewGuid().ToString()
        };

        var command = new SoapConnectionCommand(
            "Multiply",
            parameters,
            "http://tempuri.org/Multiply",
            customHeaders
        );

        var result = await service.Execute<string>(command, CancellationToken.None);

        if (result.IsSuccess)
        {
            Console.WriteLine($"Multiplication result with custom headers");
            Console.WriteLine($"Response: {result.Value}");
        }
        else
        {
            Console.WriteLine($"Request failed: {result.CurrentMessage}");
        }

        Console.WriteLine();
    }
}
