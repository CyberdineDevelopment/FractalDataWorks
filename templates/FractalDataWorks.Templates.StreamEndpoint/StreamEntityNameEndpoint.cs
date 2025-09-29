using System.Runtime.CompilerServices;
using FractalDataWorks.Web.RestEndpoints.Base;
using FractalDataWorks.Results;

namespace TemplateNamespace.EntityName;

/// <summary>
/// Stream endpoint for real-time EntityName data using FractalDataWorks FractalEndpoint base class.
/// Provides server-sent events for continuous data streaming.
/// </summary>
public class StreamEntityNameEndpoint : FractalEndpoint<StreamEntityNameRequest, IAsyncEnumerable<StreamEntityNameResponse>>
{
    public override void Configure()
    {
        Get("/api/EntityName/stream");
        
#if (RequireAuthentication)
        // Configure authentication using FractalDataWorks patterns
        ConfigureEndpointSecurity();
#endif
        
        // Configure rate limiting using FractalDataWorks patterns
        ConfigureRateLimiting();
        
        Summary(s =>
        {
            s.Summary = "Stream EntityName data in real-time";
            s.Description = "Provides real-time streaming of EntityName records using Server-Sent Events";
            s.Responses[200] = "EntityName stream started successfully";
            s.Responses[400] = "Invalid stream parameters";
#if (RequireAuthentication)
            s.Responses[401] = "Unauthorized access";
#endif
        });
    }
    
#if (RequireAuthentication)
    /// <summary>
    /// Configure authentication for this endpoint using FractalDataWorks security patterns.
    /// </summary>
    protected override void ConfigureEndpointSecurity()
    {
        // Example: Require authentication but allow any authenticated user
        Policies("RequireUser");
        
        // Or require specific roles:
#if (RequireAdminRole)
        Roles("Admin");
#else
        // Roles("User", "Admin");
#endif
        
        // Or use specific authentication schemes:
        // AuthSchemes("Bearer");
    }
#endif

    protected override async Task<IGenericResult<IAsyncEnumerable<StreamEntityNameResponse>>> ExecuteQueryAsync(StreamEntityNameRequest query, CancellationToken ct)
    {
        try
        {
            // Create the async enumerable for streaming
            var stream = StreamEntityNameDataAsync(query, ct);
            return GenericResult<IAsyncEnumerable<StreamEntityNameResponse>>.Success(stream);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error starting EntityName stream");
            return GenericResult<IAsyncEnumerable<StreamEntityNameResponse>>.Failure("Failed to start EntityName stream");
        }
    }

    /// <summary>
    /// Streams EntityName data asynchronously.
    /// </summary>
    private async IAsyncEnumerable<StreamEntityNameResponse> StreamEntityNameDataAsync(
        StreamEntityNameRequest request, 
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        Logger.LogInformation("Starting EntityName stream with interval: {Interval}ms", request.IntervalMs);

        var counter = 0;
        while (!cancellationToken.IsCancellationRequested && counter < request.MaxItems)
        {
            try
            {
                // TODO: Replace with actual data streaming logic using DataGateway
                // Example:
                // var streamCommand = new GetStreamEntityNameCommand(request.Filter, counter);
                // var result = await DataGateway.Execute<List<EntityName>>(streamCommand, cancellationToken);
                // if (result.IsSuccess)
                // {
                //     foreach (var item in result.Value)
                //     {
                //         yield return new StreamEntityNameResponse
                //         {
                //             Id = item.Id,
                //             Name = item.Name,
                //             Timestamp = DateTime.UtcNow,
                //             SequenceNumber = counter++
                //         };
                //     }
                // }

                // For now, generate sample streaming data
                yield return new StreamEntityNameResponse
                {
                    Id = counter + 1,
                    Name = $"Stream EntityName {counter + 1}",
                    Description = $"Streamed at {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss.fff}",
                    Timestamp = DateTime.UtcNow,
                    SequenceNumber = counter
#if (EnableFilters)
                    , Category = request.Category ?? "Default"
#endif
                };

                counter++;

                // Wait for the specified interval before next item
                await Task.Delay(request.IntervalMs, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                Logger.LogInformation("EntityName stream cancelled by client");
                yield break;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error in EntityName stream at item {Counter}", counter);
                // Continue streaming despite individual item errors
            }
        }

        Logger.LogInformation("EntityName stream completed. Total items streamed: {Counter}", counter);
    }
}