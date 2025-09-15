using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using RoslynMcpServer.Logging;
using RoslynMcpServer.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace RoslynMcpServer.Tools;

[McpServerToolType]
public sealed class ServerShutdownTool
{
    private const string StateFile = "roslyn-mcp-state.json";
    private readonly WorkspaceSessionManager _sessionManager;
    private readonly IHostApplicationLifetime _applicationLifetime;
    private readonly ILogger<ServerShutdownTool> _logger;
    
    public ServerShutdownTool(
        WorkspaceSessionManager sessionManager,
        IHostApplicationLifetime applicationLifetime,
        ILogger<ServerShutdownTool> logger)
    {
        _sessionManager = sessionManager;
        _applicationLifetime = applicationLifetime;
        _logger = logger;
    }
    
    [McpServerTool]
    [Description("Gracefully shutdown the MCP server after saving session state")]
    public async Task<string> ShutdownServerAsync()
    {
        try
        {
            _logger.ServerShutdownRequestedViaTool();
            
            // Save active sessions state
            var activeSessions = _sessionManager.GetActiveSessions();
            if (activeSessions.Count > 0)
            {
                var state = new ServerState
                {
                    ShutdownTime = DateTime.UtcNow,
                    ActiveSessions = activeSessions.Select(s => new SessionState
                    {
                        Id = s.Id,
                        SolutionPath = s.SolutionPath,
                        HasPendingChanges = s.HasPendingChanges
                    }).ToList()
                };
                
                var stateJson = JsonSerializer.Serialize(state, new JsonSerializerOptions 
                { 
                    WriteIndented = true 
                });
                
                var statePath = Path.Combine(Path.GetTempPath(), StateFile);
                await File.WriteAllTextAsync(statePath, stateJson);
                
                _logger.SessionStateSaved(activeSessions.Count, statePath);
            }
            
            // Schedule graceful shutdown after a short delay to allow response to be sent
            _ = Task.Run(async () =>
            {
                await Task.Delay(1000);
                _applicationLifetime.StopApplication();
            });
            
            return JsonSerializer.Serialize(new
            {
                success = true,
                message = "Server shutdown initiated",
                savedSessions = activeSessions.Count,
                stateFile = Path.Combine(Path.GetTempPath(), StateFile),
                processId = Environment.ProcessId
            });
        }
        catch (Exception ex)
        {
            _logger.ErrorDuringShutdown(ex);
            return JsonSerializer.Serialize(new
            {
                success = false,
                error = ex.Message
            });
        }
    }
    
    private sealed class ServerState
    {
        public DateTime ShutdownTime { get; set; }
        public List<SessionState> ActiveSessions { get; set; } = new();
    }
    
    private sealed class SessionState
    {
        public string Id { get; set; } = string.Empty;
        public string SolutionPath { get; set; } = string.Empty;
        public bool HasPendingChanges { get; set; }
    }
}