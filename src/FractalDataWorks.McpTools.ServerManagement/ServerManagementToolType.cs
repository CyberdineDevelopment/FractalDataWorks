using FractalDataWorks.MCP.Abstractions;

namespace FractalDataWorks.McpTools.ServerManagement;

/// <summary>
/// Represents the Server Management tool type in the MCP tools collection.
/// </summary>
public sealed class ServerManagementToolType : McpToolType
{
    public static ServerManagementToolType Instance { get; } = new();

    private ServerManagementToolType()
        : base(7,
               "ServerManagement",
               "Server Management Tool",
               "Manages MCP server lifecycle including start, stop, and restart operations")
    {
    }
}