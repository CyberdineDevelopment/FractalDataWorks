using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using FractalDataWorks.Services.Connections.Rest;
using FractalDataWorks.Services.Connections.Rest.Commands;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FractalDataWorks.Rest.Sample;

/// <summary>
/// Sample application demonstrating FractalDataWorks REST API connection capabilities.
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
        services.AddHttpClient<RestService>();

        // Add REST configuration and service
        var restConfig = new RestConnectionConfiguration
        {
            BaseUrl = configuration["RestConnection:BaseUrl"] ?? "https://jsonplaceholder.typicode.com",
            TimeoutSeconds = 30,
            RetryCount = 3,
            RetryDelayMilliseconds = 1000,
            ContentType = "application/json",
            AcceptHeader = "application/json",
            UseCompression = true
        };

        services.AddSingleton(restConfig);
        services.AddScoped<RestService>();

        var serviceProvider = services.BuildServiceProvider();

        using var scope = serviceProvider.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<RestService>();

        Console.WriteLine("===============================================================================");
        Console.WriteLine("FractalDataWorks REST Connection Sample");
        Console.WriteLine("===============================================================================");
        Console.WriteLine();

        try
        {
            // Example 1: GET request
            await Example1_GetRequest(service);

            // Example 2: GET request with query parameters
            await Example2_GetWithParameters(service);

            // Example 3: POST request
            await Example3_PostRequest(service);

            // Example 4: PUT request
            await Example4_PutRequest(service);

            // Example 5: PATCH request
            await Example5_PatchRequest(service);

            // Example 6: DELETE request
            await Example6_DeleteRequest(service);

            // Example 7: Custom headers
            await Example7_CustomHeaders(service);

            Console.WriteLine();
            Console.WriteLine("===============================================================================");
            Console.WriteLine("All REST examples completed successfully!");
            Console.WriteLine("===============================================================================");
        }
        catch (Exception ex)
        {
            Console.WriteLine();
            Console.WriteLine($"ERROR: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
        }
    }

    private static async Task Example1_GetRequest(RestService service)
    {
        Console.WriteLine("Example 1: GET Request (Fetch All Posts)");
        Console.WriteLine("----------------------------------------");

        var command = new RestConnectionCommand(
            HttpMethod.Get,
            "/posts"
        );

        var result = await service.Execute<List<Post>>(command, CancellationToken.None);

        if (result.IsSuccess)
        {
            Console.WriteLine($"Retrieved {result.Value.Count} posts");
            foreach (var post in result.Value.Take(3))
            {
                Console.WriteLine($"  Post {post.Id}: {post.Title}");
            }
            if (result.Value.Count > 3)
            {
                Console.WriteLine($"  ... and {result.Value.Count - 3} more");
            }
        }
        else
        {
            Console.WriteLine($"Request failed: {result.CurrentMessage}");
        }

        Console.WriteLine();
    }

    private static async Task Example2_GetWithParameters(RestService service)
    {
        Console.WriteLine("Example 2: GET Request with Query Parameters");
        Console.WriteLine("----------------------------------------");

        var queryParams = new Dictionary<string, string>
        {
            ["userId"] = "1"
        };

        var command = new RestConnectionCommand(
            HttpMethod.Get,
            "/posts",
            queryParameters: queryParams
        );

        var result = await service.Execute<List<Post>>(command, CancellationToken.None);

        if (result.IsSuccess)
        {
            Console.WriteLine($"Retrieved {result.Value.Count} posts for userId=1:");
            foreach (var post in result.Value)
            {
                Console.WriteLine($"  Post {post.Id}: {post.Title}");
            }
        }
        else
        {
            Console.WriteLine($"Request failed: {result.CurrentMessage}");
        }

        Console.WriteLine();
    }

    private static async Task Example3_PostRequest(RestService service)
    {
        Console.WriteLine("Example 3: POST Request (Create New Post)");
        Console.WriteLine("----------------------------------------");

        var newPost = new Post
        {
            UserId = 1,
            Title = "Sample Post from FractalDataWorks",
            Body = "This is a test post created by the REST sample application."
        };

        var command = new RestConnectionCommand(
            HttpMethod.Post,
            "/posts",
            body: newPost
        );

        var result = await service.Execute<Post>(command, CancellationToken.None);

        if (result.IsSuccess)
        {
            Console.WriteLine($"Created post with ID: {result.Value.Id}");
            Console.WriteLine($"Title: {result.Value.Title}");
            Console.WriteLine($"Body: {result.Value.Body}");
        }
        else
        {
            Console.WriteLine($"Request failed: {result.CurrentMessage}");
        }

        Console.WriteLine();
    }

    private static async Task Example4_PutRequest(RestService service)
    {
        Console.WriteLine("Example 4: PUT Request (Update Post)");
        Console.WriteLine("----------------------------------------");

        var updatedPost = new Post
        {
            Id = 1,
            UserId = 1,
            Title = "Updated Post Title",
            Body = "This post has been updated via PUT request."
        };

        var command = new RestConnectionCommand(
            HttpMethod.Put,
            "/posts/1",
            body: updatedPost
        );

        var result = await service.Execute<Post>(command, CancellationToken.None);

        if (result.IsSuccess)
        {
            Console.WriteLine($"Updated post ID: {result.Value.Id}");
            Console.WriteLine($"New title: {result.Value.Title}");
        }
        else
        {
            Console.WriteLine($"Request failed: {result.CurrentMessage}");
        }

        Console.WriteLine();
    }

    private static async Task Example5_PatchRequest(RestService service)
    {
        Console.WriteLine("Example 5: PATCH Request (Partial Update)");
        Console.WriteLine("----------------------------------------");

        var partialUpdate = new
        {
            Title = "Partially Updated Title"
        };

        var command = new RestConnectionCommand(
            HttpMethod.Patch,
            "/posts/1",
            body: partialUpdate
        );

        var result = await service.Execute<Post>(command, CancellationToken.None);

        if (result.IsSuccess)
        {
            Console.WriteLine($"Patched post ID: {result.Value.Id}");
            Console.WriteLine($"New title: {result.Value.Title}");
        }
        else
        {
            Console.WriteLine($"Request failed: {result.CurrentMessage}");
        }

        Console.WriteLine();
    }

    private static async Task Example6_DeleteRequest(RestService service)
    {
        Console.WriteLine("Example 6: DELETE Request");
        Console.WriteLine("----------------------------------------");

        var command = new RestConnectionCommand(
            HttpMethod.Delete,
            "/posts/1"
        );

        var result = await service.Execute<object>(command, CancellationToken.None);

        if (result.IsSuccess)
        {
            Console.WriteLine("Post deleted successfully");
        }
        else
        {
            Console.WriteLine($"Request failed: {result.CurrentMessage}");
        }

        Console.WriteLine();
    }

    private static async Task Example7_CustomHeaders(RestService service)
    {
        Console.WriteLine("Example 7: Request with Custom Headers");
        Console.WriteLine("----------------------------------------");

        var customHeaders = new Dictionary<string, string>
        {
            ["X-Custom-Header"] = "CustomValue",
            ["X-Request-ID"] = Guid.NewGuid().ToString()
        };

        var command = new RestConnectionCommand(
            HttpMethod.Get,
            "/posts/1",
            headers: customHeaders
        );

        var result = await service.Execute<Post>(command, CancellationToken.None);

        if (result.IsSuccess)
        {
            Console.WriteLine($"Retrieved post: {result.Value.Title}");
            Console.WriteLine("Custom headers were included in the request");
        }
        else
        {
            Console.WriteLine($"Request failed: {result.CurrentMessage}");
        }

        Console.WriteLine();
    }
}

/// <summary>
/// Represents a blog post from JSONPlaceholder API.
/// </summary>
public sealed class Post
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
}
