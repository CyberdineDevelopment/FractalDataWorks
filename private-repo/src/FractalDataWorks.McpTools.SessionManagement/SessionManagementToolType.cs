using FractalDataWorks.Mcp.Abstractions;

namespace FractalDataWorks.McpTools.SessionManagement;

/// <summary>
/// Represents the Session Management tool type in the MCP tools collection.
/// </summary>
public sealed class SessionManagementToolType : McpToolType
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SessionManagementToolType"/> class.
    /// </summary>
    public SessionManagementToolType()
        : base(4,
               "SessionManagement",
               "Session Management Tool",
               "Manages Roslyn analysis sessions with pause, resume, and state tracking")
    {
    }
}