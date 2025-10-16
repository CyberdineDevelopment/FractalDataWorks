using FractalDataWorks.Mcp.Abstractions;

namespace FractalDataWorks.McpTools.ServerManagement;

/// <summary>
/// Represents the Server Management tool type in the MCP tools collection.
/// </summary>
public sealed class ServerManagementToolType : McpToolType
{
    /// <summary>
    /// Gets the singleton instance of the <see cref="ServerManagementToolType"/>.
    /// </summary>
    public static ServerManagementToolType Instance { get; } = new();

    private ServerManagementToolType()
        : base(7,
               "ServerManagement",
               "Server Management Tool",
               "Manages MCP server lifecycle including start, stop, and restart operations")
    {
    }
}