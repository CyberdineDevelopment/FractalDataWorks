using Microsoft.Extensions.Logging;
using System;

namespace RoslynMcpServer.Logging;

/// <summary>
/// Source-generated logging messages for the Roslyn MCP Server
/// </summary>
internal static partial class LoggerMessages
{
    // Server lifecycle messages
    [LoggerMessage(
        EventId = 1000,
        Level = LogLevel.Information,
        Message = "Roslyn MCP Server starting up")]
    public static partial void ServerStarting(this ILogger logger);
    
    [LoggerMessage(
        EventId = 1001,
        Level = LogLevel.Information,
        Message = "Server started successfully. Process ID: {ProcessId}, .NET Version: {DotNetVersion}")]
    public static partial void ServerStarted(this ILogger logger, int processId, string dotNetVersion);
    
    [LoggerMessage(
        EventId = 1002,
        Level = LogLevel.Information,
        Message = "Server shutdown requested")]
    public static partial void ServerShutdownRequested(this ILogger logger);
    
    [LoggerMessage(
        EventId = 1003,
        Level = LogLevel.Information,
        Message = "Server stopped gracefully")]
    public static partial void ServerStopped(this ILogger logger);
    
    // MSBuild registration messages
    [LoggerMessage(
        EventId = 2000,
        Level = LogLevel.Information,
        Message = "MSBuild registered: {InstanceName} v{Version} from {Path}")]
    public static partial void MSBuildRegistered(this ILogger logger, string instanceName, string version, string path);
    
    [LoggerMessage(
        EventId = 2001,
        Level = LogLevel.Warning,
        Message = "No MSBuild instances found, trying .NET SDK fallback")]
    public static partial void NoMSBuildInstancesFound(this ILogger logger);
    
    [LoggerMessage(
        EventId = 2002,
        Level = LogLevel.Debug,
        Message = "Found {Count} SDK versions")]
    public static partial void FoundSdkVersions(this ILogger logger, int count);
    
    [LoggerMessage(
        EventId = 2003,
        Level = LogLevel.Error,
        Message = "MSBuild registration failed")]
    public static partial void MSBuildRegistrationFailed(this ILogger logger, Exception exception);
    
    // Session management messages
    [LoggerMessage(
        EventId = 3000,
        Level = LogLevel.Information,
        Message = "Starting session {SessionId} for {SolutionPath}")]
    public static partial void StartingSession(this ILogger logger, string sessionId, string solutionPath);
    
    [LoggerMessage(
        EventId = 3001,
        Level = LogLevel.Information,
        Message = "Session {SessionId} started successfully with {ProjectCount} projects")]
    public static partial void SessionStarted(this ILogger logger, string sessionId, int projectCount);
    
    [LoggerMessage(
        EventId = 3002,
        Level = LogLevel.Information,
        Message = "Ending session {SessionId}")]
    public static partial void EndingSession(this ILogger logger, string sessionId);
    
    [LoggerMessage(
        EventId = 3003,
        Level = LogLevel.Warning,
        Message = "Session {SessionId} not found")]
    public static partial void SessionNotFound(this ILogger logger, string sessionId);
    
    [LoggerMessage(
        EventId = 3004,
        Level = LogLevel.Information,
        Message = "Saved {Count} session(s) to state file: {FilePath}")]
    public static partial void SessionStateSaved(this ILogger logger, int count, string filePath);
    
    // Workspace loading messages
    [LoggerMessage(
        EventId = 4000,
        Level = LogLevel.Information,
        Message = "Loading solution with MSBuildWorkspace: {SolutionPath}")]
    public static partial void LoadingSolution(this ILogger logger, string solutionPath);
    
    [LoggerMessage(
        EventId = 4001,
        Level = LogLevel.Information,
        Message = "Solution loaded: {ProjectCount} projects, {DocumentCount} documents")]
    public static partial void SolutionLoaded(this ILogger logger, int projectCount, int documentCount);
    
    [LoggerMessage(
        EventId = 4002,
        Level = LogLevel.Warning,
        Message = "Workspace diagnostic: [{Kind}] {Message}")]
    public static partial void WorkspaceDiagnostic(this ILogger logger, string kind, string message);
    
    [LoggerMessage(
        EventId = 4003,
        Level = LogLevel.Warning,
        Message = "Solution loaded but contains no projects")]
    public static partial void EmptySolutionLoaded(this ILogger logger);
    
    [LoggerMessage(
        EventId = 4004,
        Level = LogLevel.Error,
        Message = "Failed to load solution: {SolutionPath}")]
    public static partial void SolutionLoadFailed(this ILogger logger, string solutionPath, Exception exception);
    
    // Diagnostic analysis messages
    [LoggerMessage(
        EventId = 5000,
        Level = LogLevel.Debug,
        Message = "Running diagnostics for session {SessionId} with severity {Severity}")]
    public static partial void RunningDiagnostics(this ILogger logger, string sessionId, string? severity);
    
    [LoggerMessage(
        EventId = 5001,
        Level = LogLevel.Information,
        Message = "Found {Count} diagnostics in {ElapsedMs}ms")]
    public static partial void DiagnosticsCompleted(this ILogger logger, int count, long elapsedMs);
    
    // Virtual edit messages
    [LoggerMessage(
        EventId = 6000,
        Level = LogLevel.Information,
        Message = "Applying virtual edit to {FilePath}")]
    public static partial void ApplyingVirtualEdit(this ILogger logger, string filePath);
    
    [LoggerMessage(
        EventId = 6001,
        Level = LogLevel.Information,
        Message = "Committing {Count} pending changes")]
    public static partial void CommittingChanges(this ILogger logger, int count);
    
    [LoggerMessage(
        EventId = 6002,
        Level = LogLevel.Information,
        Message = "Rolling back {Count} pending changes")]
    public static partial void RollingBackChanges(this ILogger logger, int count);
    
    // Cache messages
    [LoggerMessage(
        EventId = 7000,
        Level = LogLevel.Debug,
        Message = "Cache hit for project {ProjectName}")]
    public static partial void CacheHit(this ILogger logger, string projectName);
    
    [LoggerMessage(
        EventId = 7001,
        Level = LogLevel.Debug,
        Message = "Cache miss for project {ProjectName}")]
    public static partial void CacheMiss(this ILogger logger, string projectName);
    
    [LoggerMessage(
        EventId = 7002,
        Level = LogLevel.Information,
        Message = "Cache cleared: {EntriesRemoved} entries removed")]
    public static partial void CacheCleared(this ILogger logger, int entriesRemoved);
    
    // Performance messages
    [LoggerMessage(
        EventId = 8000,
        Level = LogLevel.Information,
        Message = "{OperationName} completed in {ElapsedMs}ms")]
    public static partial void OperationCompleted(this ILogger logger, string operationName, long elapsedMs);
    
    [LoggerMessage(
        EventId = 8001,
        Level = LogLevel.Warning,
        Message = "{OperationName} is taking longer than expected: {ElapsedMs}ms")]
    public static partial void SlowOperation(this ILogger logger, string operationName, long elapsedMs);
    
    // Error messages
    [LoggerMessage(
        EventId = 9000,
        Level = LogLevel.Error,
        Message = "Unhandled exception in {Context}")]
    public static partial void UnhandledException(this ILogger logger, string context, Exception exception);
    
    [LoggerMessage(
        EventId = 9001,
        Level = LogLevel.Error,
        Message = "Failed to execute tool {ToolName}")]
    public static partial void ToolExecutionFailed(this ILogger logger, string toolName, Exception exception);
    
    // Bootstrap logging messages
    [LoggerMessage(
        EventId = 10000,
        Level = LogLevel.Information,
        Message = "Logging to Seq at {SeqUrl}")]
    public static partial void LoggingToSeq(this ILogger logger, string seqUrl);
    
    [LoggerMessage(
        EventId = 10001,
        Level = LogLevel.Information,
        Message = "Seq not available, logging to file: {FilePath}")]
    public static partial void LoggingToFile(this ILogger logger, string filePath);
    
    [LoggerMessage(
        EventId = 10002,
        Level = LogLevel.Information,
        Message = "Host built successfully")]
    public static partial void HostBuiltSuccessfully(this ILogger logger);
    
    [LoggerMessage(
        EventId = 10003,
        Level = LogLevel.Critical,
        Message = "Startup failed")]
    public static partial void StartupFailed(this ILogger logger, Exception exception);
    
    // Workspace loading messages
    [LoggerMessage(
        EventId = 11000,
        Level = LogLevel.Information,
        Message = "Using AdhocWorkspace for Roslyn operations")]
    public static partial void UsingAdhocWorkspace(this ILogger logger);
    
    [LoggerMessage(
        EventId = 11001,
        Level = LogLevel.Error,
        Message = "MSBuildLocator not registered, falling back to basic loading. This should not happen - check Program.cs initialization")]
    public static partial void MSBuildLocatorNotRegisteredError(this ILogger logger);
    
    [LoggerMessage(
        EventId = 11002,
        Level = LogLevel.Debug,
        Message = "MSBuildLocator.IsRegistered: {IsRegistered}")]
    public static partial void MSBuildLocatorStatus(this ILogger logger, bool isRegistered);
    
    [LoggerMessage(
        EventId = 11003,
        Level = LogLevel.Information,
        Message = "Opening solution async...")]
    public static partial void OpeningSolutionAsync(this ILogger logger);
    
    [LoggerMessage(
        EventId = 11004,
        Level = LogLevel.Information,
        Message = "  - Project: {ProjectName} ({Language})")]
    public static partial void ProjectDetails(this ILogger logger, string projectName, string language);
    
    [LoggerMessage(
        EventId = 11005,
        Level = LogLevel.Information,
        Message = "    Documents: {DocumentCount}")]
    public static partial void ProjectDocumentCount(this ILogger logger, int documentCount);
    
    [LoggerMessage(
        EventId = 11006,
        Level = LogLevel.Information,
        Message = "    References: {ReferenceCount}")]
    public static partial void ProjectReferenceCount(this ILogger logger, int referenceCount);
    
    [LoggerMessage(
        EventId = 11007,
        Level = LogLevel.Information,
        Message = "Solution successfully transferred to AdhocWorkspace")]
    public static partial void SolutionTransferredToAdhoc(this ILogger logger);
    
    [LoggerMessage(
        EventId = 11008,
        Level = LogLevel.Warning,
        Message = "Failed to transfer solution to AdhocWorkspace")]
    public static partial void SolutionTransferToAdhocFailed(this ILogger logger);
    
    [LoggerMessage(
        EventId = 11009,
        Level = LogLevel.Error,
        Message = "MSBuildWorkspace error: {ErrorMessage}")]
    public static partial void MSBuildWorkspaceError(this ILogger logger, string errorMessage);
    
    [LoggerMessage(
        EventId = 11010,
        Level = LogLevel.Information,
        Message = "Falling back to basic loading")]
    public static partial void FallingBackToBasicLoading(this ILogger logger);
    
    [LoggerMessage(
        EventId = 11011,
        Level = LogLevel.Error,
        Message = "Unexpected error loading solution")]
    public static partial void UnexpectedSolutionLoadError(this ILogger logger, Exception exception);
    
    [LoggerMessage(
        EventId = 11012,
        Level = LogLevel.Information,
        Message = "Loading project with MSBuildWorkspace: {ProjectPath}")]
    public static partial void LoadingProjectWithMSBuild(this ILogger logger, string projectPath);
    
    [LoggerMessage(
        EventId = 11013,
        Level = LogLevel.Information,
        Message = "Opening project async...")]
    public static partial void OpeningProjectAsync(this ILogger logger);
    
    [LoggerMessage(
        EventId = 11014,
        Level = LogLevel.Information,
        Message = "Project loaded: {ProjectName} ({Language})")]
    public static partial void ProjectLoaded(this ILogger logger, string projectName, string language);
    
    [LoggerMessage(
        EventId = 11015,
        Level = LogLevel.Information,
        Message = "Documents: {DocumentCount}")]
    public static partial void DocumentCount(this ILogger logger, int documentCount);
    
    [LoggerMessage(
        EventId = 11016,
        Level = LogLevel.Information,
        Message = "References: {ReferenceCount}")]
    public static partial void ReferenceCount(this ILogger logger, int referenceCount);
    
    [LoggerMessage(
        EventId = 11017,
        Level = LogLevel.Information,
        Message = "Project references: {ProjectReferenceCount}")]
    public static partial void ProjectReferenceCount2(this ILogger logger, int projectReferenceCount);
    
    [LoggerMessage(
        EventId = 11018,
        Level = LogLevel.Warning,
        Message = "Project loaded but contains no documents. Check diagnostics above.")]
    public static partial void ProjectLoadedButNoDocuments(this ILogger logger);
    
    [LoggerMessage(
        EventId = 11019,
        Level = LogLevel.Information,
        Message = "Project successfully transferred to AdhocWorkspace")]
    public static partial void ProjectTransferredToAdhoc(this ILogger logger);
    
    [LoggerMessage(
        EventId = 11020,
        Level = LogLevel.Warning,
        Message = "Failed to transfer project to AdhocWorkspace")]
    public static partial void ProjectTransferToAdhocFailed(this ILogger logger);
    
    [LoggerMessage(
        EventId = 11021,
        Level = LogLevel.Error,
        Message = "Unexpected error loading project")]
    public static partial void UnexpectedProjectLoadError(this ILogger logger, Exception exception);
    
    [LoggerMessage(
        EventId = 11022,
        Level = LogLevel.Debug,
        Message = "LoadProjectBasicAsync: Loading {ProjectPath}")]
    public static partial void LoadProjectBasicLoading(this ILogger logger, string projectPath);
    
    [LoggerMessage(
        EventId = 11023,
        Level = LogLevel.Debug,
        Message = "LoadProjectBasicAsync: Solution has {ProjectCount} projects")]
    public static partial void LoadProjectBasicSolutionProjects(this ILogger logger, int projectCount);
    
    [LoggerMessage(
        EventId = 11024,
        Level = LogLevel.Debug,
        Message = "LoadProjectBasicAsync: Changes applied successfully")]
    public static partial void LoadProjectBasicChangesApplied(this ILogger logger);
    
    [LoggerMessage(
        EventId = 11025,
        Level = LogLevel.Warning,
        Message = "LoadProjectBasicAsync: Failed to apply changes")]
    public static partial void LoadProjectBasicChangesFailedToApply(this ILogger logger);
    
    [LoggerMessage(
        EventId = 11026,
        Level = LogLevel.Debug,
        Message = "LoadProjectBasicAsync: Final solution has {ProjectCount} projects")]
    public static partial void LoadProjectBasicFinalProjects(this ILogger logger, int projectCount);
    
    // Server tool messages
    [LoggerMessage(
        EventId = 12000,
        Level = LogLevel.Information,
        Message = "Server shutdown requested via tool")]
    public static partial void ServerShutdownRequestedViaTool(this ILogger logger);
    
    [LoggerMessage(
        EventId = 12001,
        Level = LogLevel.Information,
        Message = "Server restarted - previous state file found")]
    public static partial void ServerRestartedWithState(this ILogger logger);
    
    [LoggerMessage(
        EventId = 12002,
        Level = LogLevel.Error,
        Message = "Error during shutdown")]
    public static partial void ErrorDuringShutdown(this ILogger logger, Exception exception);
    
    [LoggerMessage(
        EventId = 11027,
        Level = LogLevel.Information,
        Message = "Solution loaded directly from MSBuildWorkspace")]
    public static partial void SolutionLoadedDirectly(this ILogger logger);
    
    [LoggerMessage(
        EventId = 11028,
        Level = LogLevel.Information,
        Message = "Project loaded directly from MSBuildWorkspace")]
    public static partial void ProjectLoadedDirectly(this ILogger logger);
}