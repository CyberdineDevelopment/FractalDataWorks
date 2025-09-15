using ModelContextProtocol.Server;
using RoslynMcpServer.Services;
using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace RoslynMcpServer.Tools;

[McpServerToolType]
public sealed class ServerInfoTool
{
    private const string StateFile = "roslyn-mcp-state.json";
    private readonly WorkspaceSessionManager _sessionManager;
    
    public ServerInfoTool(WorkspaceSessionManager sessionManager)
    {
        _sessionManager = sessionManager;
    }
    
    [McpServerTool]
    [Description("Get information about the running MCP server")]
    public string GetServerInfo()
    {
        var processId = Environment.ProcessId;
        var uptime = DateTime.UtcNow - System.Diagnostics.Process.GetCurrentProcess().StartTime.ToUniversalTime();
        var activeSessions = _sessionManager.GetActiveSessions();
        
        return JsonSerializer.Serialize(new
        {
            success = true,
            processId = processId,
            uptimeSeconds = (int)uptime.TotalSeconds,
            uptimeFormatted = $"{(int)uptime.TotalHours:00}:{uptime.Minutes:00}:{uptime.Seconds:00}",
            dotnetVersion = Environment.Version.ToString(),
            msbuildRegistered = Microsoft.Build.Locator.MSBuildLocator.IsRegistered,
            activeSessions = activeSessions.Count,
            totalProjects = activeSessions.Sum(s => s.ProjectCount),
            workingDirectory = Environment.CurrentDirectory,
            tempDirectory = Path.GetTempPath(),
            stateFile = Path.Combine(Path.GetTempPath(), StateFile)
        });
    }
}