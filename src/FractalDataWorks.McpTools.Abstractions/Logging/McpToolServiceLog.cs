using Microsoft.Extensions.Logging;

namespace FractalDataWorks.McpTools.Abstractions.Logging;

/// <summary>
/// High-performance source-generated logging for MCP tool services.
/// </summary>
public static partial class McpToolServiceLog
{
    /// <summary>
    /// Logs when tools are registered in an MCP tool service.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="count">The number of tools registered.</param>
    /// <param name="category">The tool category name.</param>
    [LoggerMessage(
        EventId = 1,
        Level = LogLevel.Information,
        Message = "Registered {Count} {Category} tools")]
    public static partial void ToolsRegistered(ILogger logger, int count, string category);
}
