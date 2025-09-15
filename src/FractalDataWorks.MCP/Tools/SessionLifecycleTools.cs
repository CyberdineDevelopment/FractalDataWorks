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
public class SessionLifecycleTools
{
    private readonly WorkspaceSessionManager _sessionManager;
    private readonly CompilationCacheService _cache;
    private readonly FileSystemWatcherService _fileWatcher;
    private readonly ProjectDependencyService _dependencyService;
    private readonly ILogger<SessionLifecycleTools> _logger;

    public SessionLifecycleTools(
        WorkspaceSessionManager sessionManager,
        CompilationCacheService cache,
        FileSystemWatcherService fileWatcher,
        ProjectDependencyService dependencyService,
        ILogger<SessionLifecycleTools> logger)
    {
        _sessionManager = sessionManager;
        _cache = cache;
        _fileWatcher = fileWatcher;
        _dependencyService = dependencyService;
        _logger = logger;

        // Subscribe to file system changes
        _fileWatcher.FileChanged += OnFileChanged;
        _fileWatcher.BatchFileChanged += OnBatchFileChanged;
    }

    [McpServerTool]
    [Description("Pause session to allow external file changes. File system watcher tracks what changes while paused.")]
    public string PauseSession(
        [Description("Session ID to pause")] string sessionId,
        [Description("Watch for file changes while paused")] bool watchFiles = true)
    {
        var stopwatch = Stopwatch.StartNew();
        ToolLogMessages.PausingSession(_logger, sessionId, watchFiles);

        try
        {
            var session = _sessionManager.GetSession(sessionId);
            if (session == null)
            {
                return JsonSerializer.Serialize(new { success = false, error = "Session not found" });
            }

            // Mark session as paused
            session.IsPaused = true;
            session.PausedAt = DateTime.UtcNow;

            if (watchFiles)
            {
                // Start watching for file changes
                var solutionDir = Path.GetDirectoryName(session.SolutionPath) ?? session.SolutionPath;
                var filePatterns = new[] { "*.cs", "*.csproj", "*.sln", "*.props", "*.targets" };
                _fileWatcher.StartWatching(sessionId, solutionDir, filePatterns);
            }

            stopwatch.Stop();
            ToolLogMessages.SessionPaused(_logger, sessionId, watchFiles, stopwatch.ElapsedMilliseconds);

            return JsonSerializer.Serialize(new
            {
                success = true,
                sessionId,
                message = "Session paused successfully",
                watchingFiles = watchFiles,
                pausedAt = session.PausedAt,
                instruction = "External changes will be tracked. Use ResumeSession to rebuild only affected components."
            }, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            ToolLogMessages.SessionPauseFailed(_logger, sessionId, stopwatch.ElapsedMilliseconds, ex);

            return JsonSerializer.Serialize(new
            {
                success = false,
                error = ex.Message
            });
        }
    }

    [McpServerTool]
    [Description("Resume paused session and incrementally rebuild only changed/affected components.")]
    public async Task<string> ResumeSessionAsync(
        [Description("Session ID to resume")] string sessionId,
        [Description("Force full rebuild instead of incremental")] bool forceFullRebuild = false)
    {
        var stopwatch = Stopwatch.StartNew();
        ToolLogMessages.ResumingSession(_logger, sessionId, forceFullRebuild);

        try
        {
            var session = _sessionManager.GetSession(sessionId);
            if (session == null)
            {
                return JsonSerializer.Serialize(new { success = false, error = "Session not found" });
            }

            if (!session.IsPaused)
            {
                return JsonSerializer.Serialize(new { success = false, error = "Session is not paused" });
            }

            var changedFiles = new List<string>();
            var affectedProjects = new HashSet<string>(StringComparer.Ordinal);

            if (!forceFullRebuild)
            {
                // Get files that changed while paused
                var pauseDuration = DateTime.UtcNow - (session.PausedAt ?? DateTime.UtcNow);
                changedFiles = _fileWatcher.GetRecentChanges(sessionId, pauseDuration).ToList();

                // Determine affected projects using dependency analysis
                foreach (var filePath in changedFiles)
                {
                    var projectIds = GetProjectsContainingFile(session, filePath);
                    foreach (var projectId in projectIds)
                    {
                        affectedProjects.Add(projectId.ToString());
                        
                        // Get projects that depend on this changed project
                        var dependentProjects = _dependencyService.GetAffectedProjects(sessionId, projectId);
                        foreach (var depProject in dependentProjects)
                        {
                            affectedProjects.Add(depProject.ToString());
                        }
                    }
                }

                // Invalidate cache for affected projects only
                foreach (var projectIdStr in affectedProjects)
                {
                    if (Guid.TryParse(projectIdStr, out var guid))
                    {
                        var projectId = Microsoft.CodeAnalysis.ProjectId.CreateFromSerialized(guid);
                        _cache.InvalidateProject(sessionId, projectId);
                    }
                }
            }
            else
            {
                // Full rebuild - invalidate entire cache
                await _cache.InvalidateAsync(sessionId);
            }

            // Resume session
            session.IsPaused = false;
            session.LastAccessedAt = DateTime.UtcNow;
            session.PausedAt = null;

            // Stop file watching
            _fileWatcher.StopWatching(sessionId);

            stopwatch.Stop();
            ToolLogMessages.SessionResumed(_logger, sessionId, changedFiles.Count, affectedProjects.Count, forceFullRebuild, stopwatch.ElapsedMilliseconds);

            return JsonSerializer.Serialize(new
            {
                success = true,
                sessionId,
                message = "Session resumed successfully",
                resumeType = forceFullRebuild ? "full_rebuild" : "incremental",
                changedFiles = changedFiles.Count,
                affectedProjects = affectedProjects.Count,
                fileList = changedFiles.Take(10).ToArray(), // Show first 10 for brevity
                projectList = affectedProjects.Take(10).ToArray(),
                rebuildStats = new
                {
                    fullRebuild = forceFullRebuild,
                    incrementalChanges = !forceFullRebuild && changedFiles.Count > 0,
                    noChangesDetected = !forceFullRebuild && changedFiles.Count == 0
                }
            }, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            ToolLogMessages.SessionResumeFailed(_logger, sessionId, stopwatch.ElapsedMilliseconds, ex);

            return JsonSerializer.Serialize(new
            {
                success = false,
                error = ex.Message
            });
        }
    }

    [McpServerTool]
    [Description("Get files that changed while session was paused and what projects will be affected on resume.")]
    public string GetPauseChanges(
        [Description("Session ID to check")] string sessionId)
    {
        ToolLogMessages.GettingPauseChanges(_logger, sessionId);

        try
        {
            var session = _sessionManager.GetSession(sessionId);
            if (session == null)
            {
                return JsonSerializer.Serialize(new { success = false, error = "Session not found" });
            }

            if (!session.IsPaused)
            {
                return JsonSerializer.Serialize(new { success = false, error = "Session is not paused" });
            }

            var pauseDuration = DateTime.UtcNow - (session.PausedAt ?? DateTime.UtcNow);
            var changedFiles = _fileWatcher.GetRecentChanges(sessionId, pauseDuration).ToList();
            var affectedProjects = new HashSet<string>(StringComparer.Ordinal);

            // Determine what projects would be affected
            foreach (var filePath in changedFiles)
            {
                var projectIds = GetProjectsContainingFile(session, filePath);
                foreach (var projectId in projectIds)
                {
                    affectedProjects.Add(session.Solution.GetProject(projectId)?.Name ?? projectId.ToString());
                }
            }

            return JsonSerializer.Serialize(new
            {
                success = true,
                sessionId,
                isPaused = session.IsPaused,
                pausedAt = session.PausedAt,
                pausedDuration = pauseDuration.TotalMinutes,
                changedFiles = new
                {
                    count = changedFiles.Count,
                    files = changedFiles.Take(20).ToArray() // Limit output
                },
                affectedProjects = new
                {
                    count = affectedProjects.Count,
                    projects = affectedProjects.Take(10).ToArray()
                },
                impact = new
                {
                    low = changedFiles.Count <= 5,
                    medium = changedFiles.Count > 5 && changedFiles.Count <= 20,
                    high = changedFiles.Count > 20
                }
            }, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            ToolLogMessages.GetPauseChangesFailed(_logger, sessionId, ex);
            return JsonSerializer.Serialize(new { success = false, error = ex.Message });
        }
    }

    private static IEnumerable<Microsoft.CodeAnalysis.ProjectId> GetProjectsContainingFile(CompilationSession session, string filePath)
    {
        var normalizedPath = Path.GetFullPath(filePath);
        
        return session.Solution.Projects
            .Where(project => project.Documents.Any(doc => 
                string.Equals(Path.GetFullPath(doc.FilePath ?? ""), normalizedPath, StringComparison.OrdinalIgnoreCase)))
            .Select(project => project.Id);
    }

    private void OnFileChanged(object? sender, FileChangedEventArgs e)
    {
        ToolLogMessages.ExternalFileChanged(_logger, e.SessionId, e.FilePath);
    }

    private void OnBatchFileChanged(object? sender, BatchFileChangedEventArgs e)
    {
        ToolLogMessages.ExternalBatchFileChanged(_logger, e.SessionId, e.FilePaths.Count());
    }
}

// Add to ToolLogMessages.cs
public static partial class SessionLifecycleLogMessages
{
    [LoggerMessage(
        EventId = 9001,
        Level = LogLevel.Information,
        Message = "Pausing session {sessionId} with file watching: {watchFiles}")]
    public static partial void PausingSession(this ILogger logger, string sessionId, bool watchFiles);

    [LoggerMessage(
        EventId = 9002,
        Level = LogLevel.Information,
        Message = "Session {sessionId} paused successfully, watching files: {watchFiles} in {elapsedMs}ms")]
    public static partial void SessionPaused(this ILogger logger, string sessionId, bool watchFiles, long elapsedMs);

    [LoggerMessage(
        EventId = 9003,
        Level = LogLevel.Error,
        Message = "Failed to pause session {sessionId} after {elapsedMs}ms")]
    public static partial void SessionPauseFailed(this ILogger logger, string sessionId, long elapsedMs, Exception ex);

    [LoggerMessage(
        EventId = 9004,
        Level = LogLevel.Information,
        Message = "Resuming session {sessionId}, force full rebuild: {forceFullRebuild}")]
    public static partial void ResumingSession(this ILogger logger, string sessionId, bool forceFullRebuild);

    [LoggerMessage(
        EventId = 9005,
        Level = LogLevel.Information,
        Message = "Session {sessionId} resumed: {changedFiles} files changed, {affectedProjects} projects affected, full rebuild: {forceFullRebuild} in {elapsedMs}ms")]
    public static partial void SessionResumed(this ILogger logger, string sessionId, int changedFiles, int affectedProjects, bool forceFullRebuild, long elapsedMs);

    [LoggerMessage(
        EventId = 9006,
        Level = LogLevel.Error,
        Message = "Failed to resume session {sessionId} after {elapsedMs}ms")]
    public static partial void SessionResumeFailed(this ILogger logger, string sessionId, long elapsedMs, Exception ex);

    [LoggerMessage(
        EventId = 9007,
        Level = LogLevel.Information,
        Message = "Getting pause changes for session {sessionId}")]
    public static partial void GettingPauseChanges(this ILogger logger, string sessionId);

    [LoggerMessage(
        EventId = 9008,
        Level = LogLevel.Error,
        Message = "Failed to get pause changes for session {sessionId}")]
    public static partial void GetPauseChangesFailed(this ILogger logger, string sessionId, Exception ex);

    [LoggerMessage(
        EventId = 9009,
        Level = LogLevel.Debug,
        Message = "External file changed in session {sessionId}: '{filePath}'")]
    public static partial void ExternalFileChanged(this ILogger logger, string sessionId, string filePath);

    [LoggerMessage(
        EventId = 9010,
        Level = LogLevel.Debug,
        Message = "External batch file changes in session {sessionId}: {fileCount} files")]
    public static partial void ExternalBatchFileChanged(this ILogger logger, string sessionId, int fileCount);
}