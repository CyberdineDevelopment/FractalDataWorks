using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using FractalDataWorks.MCP.Abstractions;
using FractalDataWorks.MCP.EnhancedEnums;
using FractalDataWorks.Results;
using ModelContextProtocol.Abstractions;

namespace FractalDataWorks.MCP.Plugins;

/// <summary>
/// Wrapper to adapt existing tool methods to the IMcpTool interface.
/// </summary>
public sealed class McpToolWrapper : IMcpTool
{
    private readonly Func<object?, CancellationToken, Task<IFdwResult<object>>> _executeFunc;
    private readonly Func<object?, CancellationToken, Task<IFdwResult>>? _validateFunc;

    public McpToolWrapper(
        string name,
        string description,
        IToolPlugin plugin,
        ToolCategoryBase category,
        Func<object?, CancellationToken, Task<IFdwResult<object>>> executeFunc,
        Func<object?, CancellationToken, Task<IFdwResult>>? validateFunc = null,
        int priority = 100,
        bool isEnabled = true)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Description = description ?? throw new ArgumentNullException(nameof(description));
        OwningPlugin = plugin ?? throw new ArgumentNullException(nameof(plugin));
        Category = category ?? throw new ArgumentNullException(nameof(category));
        _executeFunc = executeFunc ?? throw new ArgumentNullException(nameof(executeFunc));
        _validateFunc = validateFunc;
        Priority = priority;
        IsEnabled = isEnabled;

        // Build input schema dynamically
        InputSchema = new Dictionary<string, object>(StringComparer.Ordinal)
        {
            ["type"] = "object",
            ["properties"] = new Dictionary<string, object>(StringComparer.Ordinal)
        };
    }

    public string Name { get; }
    public string Description { get; }
    public IReadOnlyDictionary<string, object> InputSchema { get; }
    public IToolPlugin OwningPlugin { get; }
    public ToolCategoryBase Category { get; }
    public bool IsEnabled { get; }
    public int Priority { get; }

    public async Task<object> InvokeAsync(object? arguments, CancellationToken cancellationToken = default)
    {
        var result = await ExecuteAsync(arguments, cancellationToken);
        if (result.IsSuccess)
        {
            return result.Value ?? new { success = true };
        }

        throw new InvalidOperationException(result.Message ?? "Tool execution failed");
    }

    public async Task<IFdwResult<object>> ExecuteAsync(object? arguments, CancellationToken cancellationToken = default)
    {
        try
        {
            // Validate arguments if validator provided
            if (_validateFunc != null)
            {
                var validationResult = await _validateFunc(arguments, cancellationToken);
                if (!validationResult.IsSuccess)
                {
                    return FdwResult<object>.Failure(validationResult.Message);
                }
            }

            // Execute the tool
            return await _executeFunc(arguments, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            return FdwResult<object>.Failure("Operation was cancelled");
        }
        catch (Exception ex)
        {
            return FdwResult<object>.Failure($"Tool execution failed: {ex.Message}");
        }
    }

    public async Task<IFdwResult> ValidateArgumentsAsync(object? arguments, CancellationToken cancellationToken = default)
    {
        if (_validateFunc != null)
        {
            return await _validateFunc(arguments, cancellationToken);
        }

        // Default validation - just check for null if required
        return FdwResult.Success();
    }
}

/// <summary>
/// Static factory for creating MCP tool wrappers from delegates.
/// </summary>
public static class McpToolFactory
{
    /// <summary>
    /// Creates a tool wrapper from a synchronous function.
    /// </summary>
    public static IMcpTool CreateFromFunc<TArgs, TResult>(
        string name,
        string description,
        IToolPlugin plugin,
        ToolCategoryBase category,
        Func<TArgs, TResult> func,
        Func<TArgs, bool>? validator = null)
        where TArgs : class
    {
        return new McpToolWrapper(
            name,
            description,
            plugin,
            category,
            executeFunc: async (args, ct) =>
            {
                try
                {
                    var typedArgs = ConvertArguments<TArgs>(args);
                    var result = func(typedArgs);
                    return await Task.FromResult(FdwResult<object>.Success(result));
                }
                catch (Exception ex)
                {
                    return FdwResult<object>.Failure(ex.Message);
                }
            },
            validateFunc: validator != null
                ? async (args, ct) =>
                {
                    try
                    {
                        var typedArgs = ConvertArguments<TArgs>(args);
                        var isValid = validator(typedArgs);
                        return await Task.FromResult(isValid
                            ? FdwResult.Success()
                            : FdwResult.Failure("Validation failed"));
                    }
                    catch (Exception ex)
                    {
                        return FdwResult.Failure($"Validation error: {ex.Message}");
                    }
                }
                : null);
    }

    /// <summary>
    /// Creates a tool wrapper from an asynchronous function.
    /// </summary>
    public static IMcpTool CreateFromTask<TArgs, TResult>(
        string name,
        string description,
        IToolPlugin plugin,
        ToolCategoryBase category,
        Func<TArgs, CancellationToken, Task<TResult>> func,
        Func<TArgs, CancellationToken, Task<bool>>? validator = null)
        where TArgs : class
    {
        return new McpToolWrapper(
            name,
            description,
            plugin,
            category,
            executeFunc: async (args, ct) =>
            {
                try
                {
                    var typedArgs = ConvertArguments<TArgs>(args);
                    var result = await func(typedArgs, ct);
                    return FdwResult<object>.Success(result);
                }
                catch (Exception ex)
                {
                    return FdwResult<object>.Failure(ex.Message);
                }
            },
            validateFunc: validator != null
                ? async (args, ct) =>
                {
                    try
                    {
                        var typedArgs = ConvertArguments<TArgs>(args);
                        var isValid = await validator(typedArgs, ct);
                        return isValid
                            ? FdwResult.Success()
                            : FdwResult.Failure("Validation failed");
                    }
                    catch (Exception ex)
                    {
                        return FdwResult.Failure($"Validation error: {ex.Message}");
                    }
                }
                : null);
    }

    private static TArgs ConvertArguments<TArgs>(object? args) where TArgs : class
    {
        if (args == null)
        {
            throw new ArgumentNullException(nameof(args));
        }

        if (args is TArgs typedArgs)
        {
            return typedArgs;
        }

        // Try to deserialize from JSON
        if (args is JsonElement jsonElement)
        {
            var json = jsonElement.GetRawText();
            var result = JsonSerializer.Deserialize<TArgs>(json);
            return result ?? throw new InvalidOperationException("Failed to deserialize arguments");
        }

        // Try to convert from dictionary
        if (args is IDictionary<string, object> dict)
        {
            var json = JsonSerializer.Serialize(dict);
            var result = JsonSerializer.Deserialize<TArgs>(json);
            return result ?? throw new InvalidOperationException("Failed to convert arguments from dictionary");
        }

        throw new InvalidOperationException($"Cannot convert arguments of type {args.GetType()} to {typeof(TArgs)}");
    }
}