using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using RoslynMcpServer.Logging;
using RoslynMcpServer.Models;
using RoslynMcpServer.Services;
using System.ComponentModel;
using System.Diagnostics;
using System.Text.Json;

namespace RoslynMcpServer.Tools;

[McpServerToolType]
public class SessionTools
{
    private readonly WorkspaceSessionManager _sessionManager;
    private readonly CompilationCacheService _cache;
    private readonly ILogger<SessionTools> _logger;

    public SessionTools(WorkspaceSessionManager sessionManager, CompilationCacheService cache, ILogger<SessionTools> logger)
    {
        _sessionManager = sessionManager;
        _cache = cache;
        _logger = logger;
    }

    [McpServerTool]
    [Description("Start compilation session for solution/project")]
    public async Task<string> StartSessionAsync(
        [Description("Path to the .sln or .csproj file")] string solutionPath,
        [Description("Use default analyzers (StyleCop, .NET analyzers)")] bool useDefaults = true,
        [Description("Additional analyzer packages or paths")] string[]? additionalAnalyzers = null,
        [Description("Disabled analyzer rule IDs")] string[]? disabledRules = null)
    {
        var config = new AnalyzerConfiguration
        {
            UseDefaults = useDefaults,
            AdditionalAnalyzers = additionalAnalyzers?.ToList() ?? new List<string>(),
            DisabledRules = disabledRules?.ToList() ?? new List<string>()
        };

        var stopwatch = Stopwatch.StartNew();
        ToolLogMessages.StartingSession(_logger, solutionPath, useDefaults);
        
        try
        {
            var sessionId = await _sessionManager.StartSessionAsync(solutionPath, config);
            stopwatch.Stop();
            
            ToolLogMessages.SessionStarted(_logger, sessionId, solutionPath, stopwatch.ElapsedMilliseconds);
            
            return JsonSerializer.Serialize(new
            {
                success = true,
                sessionId,
                message = "Session started successfully",
                solutionPath,
                instruction = $"SessionId: {sessionId}\n\nIMPORTANT: This session will persist until explicitly ended with EndSession.\nDO NOT end session after each operation - reuse for multiple tasks on this solution.\nAdd this SessionId to your CLAUDE.md for persistence across conversations."
            }, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            ToolLogMessages.SessionStartFailed(_logger, solutionPath, stopwatch.ElapsedMilliseconds, ex);
            
            return JsonSerializer.Serialize(new
            {
                success = false,
                error = ex.Message,
                stackTrace = ex.StackTrace
            }, new JsonSerializerOptions { WriteIndented = true });
        }
    }

    [McpServerTool]
    [Description("Get session status and health")]
    public string GetSessionStatus(
        [Description("Session ID returned from StartSession")] string sessionId)
    {
        ToolLogMessages.GettingSessionStatus(_logger, sessionId);
        
        var session = _sessionManager.GetSession(sessionId);
        if (session == null)
        {
            return JsonSerializer.Serialize(new
            {
                success = false,
                error = "Session not found"
            }, new JsonSerializerOptions { WriteIndented = true });
        }

        var cacheStats = _cache.GetCacheStats();
        
        return JsonSerializer.Serialize(new
        {
            success = true,
            sessionId,
            solutionPath = session.SolutionPath,
            createdAt = session.CreatedAt,
            lastAccessedAt = session.LastAccessedAt,
            projectCount = session.Solution.ProjectIds.Count,
            hasPendingChanges = session.HasPendingChanges,
            pendingChangeCount = session.PendingChanges.Count,
            analyzerCount = session.Analyzers.Length,
            cacheStats = new
            {
                projectCompilations = cacheStats.ProjectCompilationCount,
                diagnosticsCache = cacheStats.DiagnosticsCacheCount
            }
        }, new JsonSerializerOptions { WriteIndented = true });
    }

    [McpServerTool]
    [Description("List all active sessions")]
    public string GetActiveSessions()
    {
        var sessions = _sessionManager.GetActiveSessions();
        
        return JsonSerializer.Serialize(new
        {
            success = true,
            sessionCount = sessions.Count,
            sessions = sessions.Select(s => new
            {
                id = s.Id,
                solutionPath = s.SolutionPath,
                createdAt = s.CreatedAt,
                lastAccessedAt = s.LastAccessedAt,
                projectCount = s.ProjectCount,
                hasPendingChanges = s.HasPendingChanges,
                analyzerCount = s.AnalyzerCount
            })
        }, new JsonSerializerOptions { WriteIndented = true });
    }

    [McpServerTool]
    [Description("Reload solution from disk")]
    public async Task<string> RefreshSessionAsync(
        [Description("Session ID to refresh")] string sessionId)
    {
        try
        {
            var success = await _sessionManager.RefreshSessionAsync(sessionId);
            
            return JsonSerializer.Serialize(new
            {
                success,
                message = success ? "Session refreshed successfully" : "Session not found or refresh failed",
                sessionId
            }, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new
            {
                success = false,
                error = ex.Message
            }, new JsonSerializerOptions { WriteIndented = true });
        }
    }

    [McpServerTool]
    [Description("End session and free memory")]
    public async Task<string> EndSessionAsync(
        [Description("Session ID to end")] string sessionId)
    {
        try
        {
            var success = await _sessionManager.EndSessionAsync(sessionId);
            
            return JsonSerializer.Serialize(new
            {
                success,
                message = success ? "Session ended successfully" : "Session not found",
                sessionId
            }, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new
            {
                success = false,
                error = ex.Message
            }, new JsonSerializerOptions { WriteIndented = true });
        }
    }

    [McpServerTool]
    [Description("Get cache statistics and memory usage")]
    public string GetCacheStats()
    {
        var stats = _cache.GetCacheStats();
        
        return JsonSerializer.Serialize(new
        {
            success = true,
            projectCompilationCount = stats.ProjectCompilationCount,
            diagnosticsCacheCount = stats.DiagnosticsCacheCount,
            sessionCount = stats.SessionCount,
            sessionBreakdown = stats.SessionBreakdown
        }, new JsonSerializerOptions { WriteIndented = true });
    }

    [McpServerTool]
    [Description("Clear all caches (forces recompilation)")]
    public string ClearCache()
    {
        try
        {
            _cache.ClearCache();
            
            return JsonSerializer.Serialize(new
            {
                success = true,
                message = "Cache cleared successfully"
            }, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new
            {
                success = false,
                error = ex.Message
            }, new JsonSerializerOptions { WriteIndented = true });
        }
    }
}