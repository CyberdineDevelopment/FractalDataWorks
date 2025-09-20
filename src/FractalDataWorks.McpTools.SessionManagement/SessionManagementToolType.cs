using FractalDataWorks.MCP.Abstractions;

namespace FractalDataWorks.McpTools.SessionManagement;

/// <summary>
/// Represents the Session Management tool type in the MCP tools collection.
/// </summary>
public sealed class SessionManagementToolType : McpToolType
{
    public static SessionManagementToolType Instance { get; } = new();

    private SessionManagementToolType()
        : base(4,
               "SessionManagement",
               "Session Management Tool",
               "Manages Roslyn analysis sessions with pause, resume, and state tracking")
    {
    }
}