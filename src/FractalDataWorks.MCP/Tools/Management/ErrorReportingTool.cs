using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using RoslynMcpServer.Logging;
using RoslynMcpServer.Services;
using System.ComponentModel;
using System.Diagnostics;
using System.Text.Json;

namespace RoslynMcpServer.Tools;

[McpServerToolType]
public class ErrorReportingTool
{
    private readonly WorkspaceSessionManager _sessionManager;
    private readonly CompilationCacheService _cache;
    private readonly ILogger<ErrorReportingTool> _logger;

    public ErrorReportingTool(
        WorkspaceSessionManager sessionManager,
        CompilationCacheService cache,
        ILogger<ErrorReportingTool> logger)
    {
        _sessionManager = sessionManager;
        _cache = cache;
        _logger = logger;
    }

    [McpServerTool]
    [Description("Report MCP server errors, performance issues, or unexpected behavior. Includes diagnostic information to help with troubleshooting.")]
    public string ReportError(
        [Description("Description of the error or issue")] string errorDescription,
        [Description("Session ID if the error is related to a specific session")] string? sessionId = null,
        [Description("Steps to reproduce the error")] string? reproductionSteps = null,
        [Description("Expected vs actual behavior")] string? expectedVsActual = null)
    {
        var reportId = Guid.NewGuid().ToString("N")[..8];
        var timestamp = DateTime.UtcNow;
        
        ToolLogMessages.ErrorReportSubmitted(_logger, reportId, errorDescription, sessionId ?? "none");

        try
        {
            // Gather diagnostic information
            var diagnosticInfo = new
            {
                reportId,
                timestamp,
                serverInfo = new
                {
                    processId = Environment.ProcessId,
                    dotNetVersion = Environment.Version.ToString(),
                    machineName = Environment.MachineName,
                    workingDirectory = Environment.CurrentDirectory,
                    uptime = DateTime.UtcNow - Process.GetCurrentProcess().StartTime
                },
                sessionInfo = sessionId != null ? GetSessionDiagnostics(sessionId) : null,
                cacheStats = _cache.GetCacheStats(),
                userReport = new
                {
                    errorDescription,
                    reproductionSteps,
                    expectedVsActual,
                    sessionId
                }
            };

            // Save report to temp directory for investigation
            var reportPath = Path.Combine(Path.GetTempPath(), "RoslynMcpServer", "error-reports", $"error-{reportId}-{timestamp:yyyyMMdd-HHmmss}.json");
            Directory.CreateDirectory(Path.GetDirectoryName(reportPath)!);
            
            var reportJson = JsonSerializer.Serialize(diagnosticInfo, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(reportPath, reportJson);

            ToolLogMessages.ErrorReportSaved(_logger, reportId, reportPath);

            return JsonSerializer.Serialize(new
            {
                success = true,
                reportId,
                message = "Error report submitted successfully",
                reportPath,
                timestamp,
                diagnosticInfo = new
                {
                    processId = diagnosticInfo.serverInfo.processId,
                    sessionCount = _sessionManager.GetActiveSessions().Count,
                    cacheEntries = diagnosticInfo.cacheStats.ProjectCompilationCount,
                    uptime = diagnosticInfo.serverInfo.uptime
                },
                instructions = new[]
                {
                    $"Report ID: {reportId}",
                    $"Report saved to: {reportPath}",
                    "Include this Report ID when reporting issues",
                    "Check the report file for detailed diagnostic information"
                }
            }, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            ToolLogMessages.ErrorReportFailed(_logger, reportId, errorDescription, ex);

            return JsonSerializer.Serialize(new
            {
                success = false,
                reportId,
                error = "Failed to generate error report",
                details = ex.Message,
                fallbackInfo = new
                {
                    originalError = errorDescription,
                    processId = Environment.ProcessId,
                    timestamp
                }
            }, new JsonSerializerOptions { WriteIndented = true });
        }
    }

    [McpServerTool]
    [Description("Get detailed diagnostic information about the MCP server state for troubleshooting performance or reliability issues.")]
    public string GetDiagnosticInfo(
        [Description("Include detailed session information")] bool includeSessionDetails = false,
        [Description("Include cache statistics")] bool includeCacheStats = true)
    {
        ToolLogMessages.DiagnosticInfoRequested(_logger, includeSessionDetails, includeCacheStats);

        try
        {
            var process = Process.GetCurrentProcess();
            var activeSessions = _sessionManager.GetActiveSessions();
            
            var diagnostics = new
            {
                timestamp = DateTime.UtcNow,
                server = new
                {
                    processId = Environment.ProcessId,
                    processName = process.ProcessName,
                    dotNetVersion = Environment.Version.ToString(),
                    machineName = Environment.MachineName,
                    workingDirectory = Environment.CurrentDirectory,
                    startTime = process.StartTime,
                    uptime = DateTime.UtcNow - process.StartTime,
                    workingSet = process.WorkingSet64 / (1024 * 1024), // MB
                    privateMemory = process.PrivateMemorySize64 / (1024 * 1024), // MB
                    threadCount = process.Threads.Count
                },
                sessions = new
                {
                    activeCount = activeSessions.Count,
                    sessionIds = activeSessions.Select(s => s.Id).ToArray(),
                    details = includeSessionDetails ? activeSessions.Select(s => {
                        var session = _sessionManager.GetSession(s.Id);
                        return new
                        {
                            id = s.Id,
                            solutionPath = s.SolutionPath,
                            createdAt = s.CreatedAt,
                            lastAccessedAt = s.LastAccessedAt,
                            projectCount = session?.Solution?.ProjectIds.Count ?? 0,
                            hasPendingChanges = session?.HasPendingChanges ?? false,
                            pendingChangeCount = session?.PendingChanges?.Count ?? 0,
                            isPaused = session?.IsPaused ?? false,
                            pausedAt = session?.PausedAt,
                            analyzerCount = session?.Analyzers.Length ?? 0
                        };
                    }).ToArray() : null
                },
                cache = includeCacheStats ? _cache.GetCacheStats() : null,
                performance = new
                {
                    memoryPressure = GC.GetTotalMemory(false) / (1024 * 1024), // MB
                    gen0Collections = GC.CollectionCount(0),
                    gen1Collections = GC.CollectionCount(1),
                    gen2Collections = GC.CollectionCount(2)
                }
            };

            return JsonSerializer.Serialize(diagnostics, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            ToolLogMessages.DiagnosticInfoFailed(_logger, ex);

            return JsonSerializer.Serialize(new
            {
                success = false,
                error = "Failed to gather diagnostic information",
                details = ex.Message,
                timestamp = DateTime.UtcNow
            }, new JsonSerializerOptions { WriteIndented = true });
        }
    }

    private object? GetSessionDiagnostics(string sessionId)
    {
        var session = _sessionManager.GetSession(sessionId);
        if (session == null) return null;

        return new
        {
            id = session.Id,
            solutionPath = session.SolutionPath,
            createdAt = session.CreatedAt,
            lastAccessedAt = session.LastAccessedAt,
            projectCount = session.Solution.ProjectIds.Count,
            projectNames = session.Solution.Projects.Select(p => p.Name).ToArray(),
            hasPendingChanges = session.HasPendingChanges,
            pendingChangeCount = session.PendingChanges.Count,
            isPaused = session.IsPaused,
            pausedAt = session.PausedAt,
            analyzerCount = session.Analyzers.Length
        };
    }
}

// Add to ToolLogMessages.cs - Error Reporting Messages (10001-10010)
public static partial class ErrorReportingLogMessages
{
    [LoggerMessage(
        EventId = 10001,
        Level = LogLevel.Warning,
        Message = "Error report submitted - ID: {reportId}, Description: '{errorDescription}', Session: {sessionId}")]
    public static partial void ErrorReportSubmitted(ILogger logger, string reportId, string errorDescription, string sessionId);

    [LoggerMessage(
        EventId = 10002,
        Level = LogLevel.Information,
        Message = "Error report {reportId} saved to '{reportPath}'")]
    public static partial void ErrorReportSaved(ILogger logger, string reportId, string reportPath);

    [LoggerMessage(
        EventId = 10003,
        Level = LogLevel.Error,
        Message = "Failed to generate error report {reportId} for '{errorDescription}'")]
    public static partial void ErrorReportFailed(ILogger logger, string reportId, string errorDescription, Exception ex);

    [LoggerMessage(
        EventId = 10004,
        Level = LogLevel.Information,
        Message = "Diagnostic information requested - SessionDetails: {includeSessionDetails}, CacheStats: {includeCacheStats}")]
    public static partial void DiagnosticInfoRequested(ILogger logger, bool includeSessionDetails, bool includeCacheStats);

    [LoggerMessage(
        EventId = 10005,
        Level = LogLevel.Error,
        Message = "Failed to gather diagnostic information")]
    public static partial void DiagnosticInfoFailed(ILogger logger, Exception ex);
}