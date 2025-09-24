using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FastEndpoints;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using FractalDataWorks.Results;
using FractalDataWorks.Services.DataGateway.Services;
using FractalDataWorks.Web.RestEndpoints.Logging;
// using FractalDataWorks.Services.Execution.Abstractions; // TODO: Add when available
// using FractalDataWorks.Services.Scheduling.Abstractions; // TODO: Add when available

namespace FractalDataWorks.Web.RestEndpoints.Base;

/// <summary>
/// Base endpoint class that provides authentication, RBAC, and FractalDataWorks service integration.
/// Inherits from FastEndpoints for natural HTTP handling and adds FractalDataWorks service injection.
/// </summary>
/// <typeparam name="TRequest">The request type.</typeparam>
/// <typeparam name="TResponse">The response type.</typeparam>
public abstract class FractalEndpoint<TRequest, TResponse> : Endpoint<TRequest, TResponse>
    where TRequest : notnull
{
    /// <summary>
    /// Gets the logger instance for this endpoint.
    /// </summary>
    protected new ILogger Logger { get; private set; } = null!;
    
    /// <summary>
    /// Gets the data provider for database operations.
    /// </summary>
    protected IDataGateway DataGateway { get; private set; } = null!;
    
    // /// <summary>
    // /// Gets the execution service for running processes.
    // /// </summary>
    // protected IExecutor Executor { get; private set; } = null!;
    
    // /// <summary>
    // /// Gets the scheduler for scheduling tasks.
    // /// </summary>
    // protected IFractalScheduler Scheduler { get; private set; } = null!;

    /// <summary>
    /// Handles the HTTP request with authentication, authorization, and FractalDataWorks result patterns.
    /// </summary>
    public override async Task HandleAsync(TRequest req, CancellationToken ct)
    {
        // Resolve services using FastEndpoints' DI integration
        Logger = Resolve<ILogger<FractalEndpoint<TRequest, TResponse>>>();
        DataGateway = Resolve<IDataGateway>();
        // Executor = Resolve<IExecutor>(); // TODO: Uncomment when available
        // Scheduler = Resolve<IFractalScheduler>(); // TODO: Uncomment when available
        
        try
        {
            // Check authorization if required
            var authResult = await CheckAuthorizationAsync(req, ct).ConfigureAwait(false);
            if (!authResult.IsSuccess)
            {
                HttpContext.Response.StatusCode = 401;
                return;
            }

            // Execute business logic
            var result = await ExecuteAsync(req, ct).ConfigureAwait(false);
            
            // Handle IFdwResult conversion to FastEndpoints responses
            if (result is IFdwResult<TResponse> recResult)
            {
                if (recResult.IsSuccess)
                    Response = recResult.Value; // FastEndpoints sends 200 OK
                else
                    HttpContext.Response.StatusCode = 400;
            }
            // else if (result is IExecutionResult executionResult) // TODO: Uncomment when available
            // {
            //     await HandleExecutionResultAsync(executionResult, ct);
            // }
            else if (result is TResponse directResponse)
            {
                Response = directResponse;
            }
        }
        catch (Exception ex)
        {
            FractalEndpointLog.EndpointError(Logger, GetType().Name, ex);
            HttpContext.Response.StatusCode = 500;
        }
    }

    /// <summary>
    /// Executes the business logic for this endpoint.
    /// Override this method to implement your endpoint's functionality.
    /// </summary>
    protected new abstract Task<object> ExecuteAsync(TRequest request, CancellationToken ct);
    
    /// <summary>
    /// Checks authorization for the current request.
    /// Override to implement custom authorization logic.
    /// </summary>
    protected virtual Task<IFdwResult> CheckAuthorizationAsync(TRequest request, CancellationToken ct)
        => Task.FromResult<IFdwResult>(FdwResult.Success());

    /// <summary>
    /// Creates error messages from an IFdwResult for FastEndpoints.
    /// </summary>
    protected virtual string[] CreateErrorMessages(IFdwResult result)
    {
        var messages = new List<string>();
        
        if (!string.IsNullOrEmpty(result.Message))
            messages.Add(result.Message);
            
        // TODO: Extract detailed messages when FdwResult is available
        // if (result is FdwResult recResult && recResult.Messages.Count > 0)
        // {
        //     messages.AddRange(recResult.Messages.Select(m => m.Message));
        // }
        
        return messages.Count > 0 ? messages.ToArray() : ["An error occurred"];
    }

    // /// <summary>
    // /// Handles execution results from the FractalDataWorks execution system.
    // /// </summary>
    // private async Task HandleExecutionResultAsync(IExecutionResult executionResult, CancellationToken ct)
    // {
    //     if (executionResult.IsSuccess)
    //     {
    //         Response = new ExecutionResponse 
    //         { 
    //             RequestId = executionResult.RequestId,
    //             Status = executionResult.Status.Name,
    //             Data = executionResult.Data,
    //             Duration = executionResult.Duration,
    //             Metrics = executionResult.Metrics
    //         };
    //     }
    //     else
    //     {
    //         await SendErrors(new[] { executionResult.ErrorMessage ?? "Execution failed" });
    //     }
    // }
}

/// <summary>
/// Base endpoint class for endpoints that don't need a request body.
/// </summary>
/// <typeparam name="TResponse">The response type.</typeparam>
public abstract class FractalEndpoint<TResponse> : FractalEndpoint<EmptyRequest, TResponse>
{
}