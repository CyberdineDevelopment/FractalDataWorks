using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using FractalDataWorks.McpTools.Abstractions;

namespace FractalDataWorks.Services.MCP;

/// <summary>
/// Orchestrates all MCP tool services and provides unified access to all tools.
/// </summary>
public class McpOrchestrationService
{
    private readonly ILogger<McpOrchestrationService> _logger;
    // private readonly IEnumerable<IMcpToolService> _toolServices;
    private readonly Dictionary<string, IMcpTool> _toolRegistry;

    public McpOrchestrationService(
        // IEnumerable<IMcpToolService> toolServices,
        ILogger<McpOrchestrationService> logger)
    {
        // _toolServices = toolServices ?? throw new ArgumentNullException(nameof(toolServices));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _toolRegistry = new Dictionary<string, IMcpTool>(StringComparer.OrdinalIgnoreCase);
        // RegisterAllTools();
    }

    /// <summary>
    /// Gets all registered tools.
    /// </summary>
    public IEnumerable<IMcpTool> AllTools => _toolRegistry.Values;

    /// <summary>
    /// Gets tools by category.
    /// </summary>
    public IEnumerable<IMcpTool> GetToolsByCategory(string category)
    {
        return _toolRegistry.Values.Where(t =>
            string.Equals(t.Category, category, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Gets a specific tool by name.
    /// </summary>
    public IMcpTool? GetTool(string toolName)
    {
        return _toolRegistry.TryGetValue(toolName, out var tool) ? tool : null;
    }

    /// <summary>
    /// Gets all available categories.
    /// </summary>
    public IEnumerable<string> GetCategories()
    {
        return _toolRegistry.Values
            .Select(t => t.Category)
            .Distinct(StringComparer.OrdinalIgnoreCase);
    }

    // private void RegisterAllTools()
    // {
    //     foreach (var service in _toolServices)
    //     {
    //         _logger.LogInformation("Registering tools from service: {ServiceName}", service.ServiceName);

    //         foreach (var tool in service.GetTools())
    //         {
    //             if (_toolRegistry.ContainsKey(tool.Name))
    //             {
    //                 _logger.LogWarning("Tool {ToolName} is already registered, skipping duplicate", tool.Name);
    //                 continue;
    //             }

    //             _toolRegistry[tool.Name] = tool;
    //             _logger.LogDebug("Registered tool: {ToolName} in category {Category}",
    //                 tool.Name, tool.Category);
    //         }
    //     }

    //     _logger.LogInformation("Total tools registered: {Count}", _toolRegistry.Count);
    // }
}