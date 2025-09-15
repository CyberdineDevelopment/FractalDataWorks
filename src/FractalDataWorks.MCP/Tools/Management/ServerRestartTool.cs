using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using RoslynMcpServer.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text.Json;

namespace RoslynMcpServer.Tools;

[McpServerToolType]
public sealed class ServerRestartTool
{
    private const string StateFile = "roslyn-mcp-state.json";
    private readonly ILogger<ServerRestartTool> _logger;
    
    public ServerRestartTool(ILogger<ServerRestartTool> logger)
    {
        _logger = logger;
    }
    
    [McpServerTool]
    [Description("Check if the server has restarted and if there's saved state available")]
    public string CheckServerRestartReady()
    {
        // This tool just confirms the server has restarted successfully
        // and can optionally restore state from the state file
        
        var statePath = Path.Combine(Path.GetTempPath(), StateFile);
        var stateExists = File.Exists(statePath);
        
        if (stateExists)
        {
            try
            {
                var stateJson = File.ReadAllText(statePath);
                _ = JsonSerializer.Deserialize<Dictionary<string, object>>(stateJson);
                
                _logger.ServerRestartedWithState();
                
                return JsonSerializer.Serialize(new
                {
                    success = true,
                    message = "Server restarted successfully",
                    previousStateFound = true,
                    stateFile = statePath,
                    hint = "Use the state file to restore previous sessions if needed"
                });
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not read state file");
            }
        }
        
        return JsonSerializer.Serialize(new
        {
            success = true,
            message = "Server started successfully",
            previousStateFound = false,
            processId = Environment.ProcessId
        });
    }
}