using Microsoft.Extensions.Logging;

namespace RoslynMcpServer.Logging;

public static partial class ToolLogMessages
{
    // Session Tools
    [LoggerMessage(
        EventId = 1001,
        Level = LogLevel.Information,
        Message = "Starting compilation session for solution '{solutionPath}' with useDefaults={useDefaults}")]
    public static partial void StartingSession(ILogger logger, string solutionPath, bool useDefaults);

    [LoggerMessage(
        EventId = 1002,
        Level = LogLevel.Information,
        Message = "Session started successfully: {sessionId} for '{solutionPath}' in {elapsedMs}ms")]
    public static partial void SessionStarted(ILogger logger, string sessionId, string solutionPath, long elapsedMs);

    [LoggerMessage(
        EventId = 1003,
        Level = LogLevel.Error,
        Message = "Failed to start session for '{solutionPath}' after {elapsedMs}ms")]
    public static partial void SessionStartFailed(ILogger logger, string solutionPath, long elapsedMs, Exception ex);

    [LoggerMessage(
        EventId = 1004,
        Level = LogLevel.Information,
        Message = "Ending session {sessionId}")]
    public static partial void EndingSession(ILogger logger, string sessionId);

    [LoggerMessage(
        EventId = 1005,
        Level = LogLevel.Information,
        Message = "Session {sessionId} ended successfully in {elapsedMs}ms")]
    public static partial void SessionEnded(ILogger logger, string sessionId, long elapsedMs);

    [LoggerMessage(
        EventId = 1006,
        Level = LogLevel.Information,
        Message = "Getting status for session {sessionId}")]
    public static partial void GettingSessionStatus(ILogger logger, string sessionId);

    [LoggerMessage(
        EventId = 1007,
        Level = LogLevel.Information,
        Message = "Refreshing session {sessionId}")]
    public static partial void RefreshingSession(ILogger logger, string sessionId);

    // Diagnostic Tools
    [LoggerMessage(
        EventId = 2001,
        Level = LogLevel.Information,
        Message = "Getting diagnostics for session {sessionId}, includeAnalyzers={includeAnalyzers}")]
    public static partial void GettingDiagnostics(ILogger logger, string sessionId, bool includeAnalyzers);

    [LoggerMessage(
        EventId = 2002,
        Level = LogLevel.Information,
        Message = "Diagnostics retrieved for session {sessionId}: {diagnosticCount} diagnostics in {elapsedMs}ms")]
    public static partial void DiagnosticsRetrieved(ILogger logger, string sessionId, int diagnosticCount, long elapsedMs);

    [LoggerMessage(
        EventId = 2003,
        Level = LogLevel.Information,
        Message = "Getting diagnostic summary for session {sessionId}")]
    public static partial void GettingDiagnosticSummary(ILogger logger, string sessionId);

    [LoggerMessage(
        EventId = 2004,
        Level = LogLevel.Information,
        Message = "Getting file diagnostics for '{filePath}' in session {sessionId}")]
    public static partial void GettingFileDiagnostics(ILogger logger, string filePath, string sessionId);

    [LoggerMessage(
        EventId = 2005,
        Level = LogLevel.Error,
        Message = "Failed to get diagnostics for session {sessionId}")]
    public static partial void DiagnosticsFailed(ILogger logger, string sessionId, Exception ex);

    // Virtual Edit Tools
    [LoggerMessage(
        EventId = 3001,
        Level = LogLevel.Information,
        Message = "Applying virtual edit to '{filePath}' in session {sessionId} (line {lineNumber})")]
    public static partial void ApplyingVirtualEdit(ILogger logger, string filePath, string sessionId, int lineNumber);

    [LoggerMessage(
        EventId = 3002,
        Level = LogLevel.Information,
        Message = "Virtual edit applied successfully to '{filePath}' in session {sessionId} in {elapsedMs}ms")]
    public static partial void VirtualEditApplied(ILogger logger, string filePath, string sessionId, long elapsedMs);

    [LoggerMessage(
        EventId = 3003,
        Level = LogLevel.Information,
        Message = "Committing {changeCount} pending changes in session {sessionId}")]
    public static partial void CommittingChanges(ILogger logger, int changeCount, string sessionId);

    [LoggerMessage(
        EventId = 3004,
        Level = LogLevel.Information,
        Message = "Committed {changeCount} changes in session {sessionId} in {elapsedMs}ms")]
    public static partial void ChangesCommitted(ILogger logger, int changeCount, string sessionId, long elapsedMs);

    [LoggerMessage(
        EventId = 3005,
        Level = LogLevel.Information,
        Message = "Rolling back {changeCount} pending changes in session {sessionId}")]
    public static partial void RollingBackChanges(ILogger logger, int changeCount, string sessionId);

    [LoggerMessage(
        EventId = 3006,
        Level = LogLevel.Error,
        Message = "Failed to apply virtual edit to '{filePath}' in session {sessionId}")]
    public static partial void VirtualEditFailed(ILogger logger, string filePath, string sessionId, Exception ex);

    // Type Analysis Tools
    [LoggerMessage(
        EventId = 4001,
        Level = LogLevel.Information,
        Message = "Searching for types matching '{pattern}' in session {sessionId}")]
    public static partial void SearchingTypes(ILogger logger, string pattern, string sessionId);

    [LoggerMessage(
        EventId = 4002,
        Level = LogLevel.Information,
        Message = "Found {typeCount} types matching '{pattern}' in session {sessionId} in {elapsedMs}ms")]
    public static partial void TypesFound(ILogger logger, int typeCount, string pattern, string sessionId, long elapsedMs);

    [LoggerMessage(
        EventId = 4003,
        Level = LogLevel.Information,
        Message = "Finding ambiguous types in session {sessionId}")]
    public static partial void FindingAmbiguousTypes(ILogger logger, string sessionId);

    [LoggerMessage(
        EventId = 4004,
        Level = LogLevel.Information,
        Message = "Found {ambiguousCount} ambiguous type conflicts in session {sessionId} in {elapsedMs}ms")]
    public static partial void AmbiguousTypesFound(ILogger logger, int ambiguousCount, string sessionId, long elapsedMs);

    [LoggerMessage(
        EventId = 4005,
        Level = LogLevel.Information,
        Message = "Getting type details for '{typeName}' in session {sessionId}")]
    public static partial void GettingTypeDetails(ILogger logger, string typeName, string sessionId);

    // Refactoring Tools
    [LoggerMessage(
        EventId = 5001,
        Level = LogLevel.Information,
        Message = "Renaming symbol '{oldName}' to '{newName}' in session {sessionId}")]
    public static partial void RenamingSymbol(ILogger logger, string oldName, string newName, string sessionId);

    [LoggerMessage(
        EventId = 5002,
        Level = LogLevel.Information,
        Message = "Symbol renamed from '{oldName}' to '{newName}' in session {sessionId}: {changeCount} files affected in {elapsedMs}ms")]
    public static partial void SymbolRenamed(ILogger logger, string oldName, string newName, string sessionId, int changeCount, long elapsedMs);

    [LoggerMessage(
        EventId = 5003,
        Level = LogLevel.Information,
        Message = "Moving type '{typeName}' to new file '{targetFile}' in session {sessionId}")]
    public static partial void MovingTypeToFile(ILogger logger, string typeName, string targetFile, string sessionId);

    [LoggerMessage(
        EventId = 5004,
        Level = LogLevel.Information,
        Message = "Type '{typeName}' moved to '{targetFile}' in session {sessionId} in {elapsedMs}ms")]
    public static partial void TypeMovedToFile(ILogger logger, string typeName, string targetFile, string sessionId, long elapsedMs);

    [LoggerMessage(
        EventId = 5005,
        Level = LogLevel.Error,
        Message = "Failed to rename symbol '{oldName}' to '{newName}' in session {sessionId}")]
    public static partial void SymbolRenameFailed(ILogger logger, string oldName, string newName, string sessionId, Exception ex);

    // Performance and General
    [LoggerMessage(
        EventId = 6001,
        Level = LogLevel.Warning,
        Message = "Tool operation took {elapsedMs}ms which exceeds performance threshold of {thresholdMs}ms")]
    public static partial void PerformanceThresholdExceeded(ILogger logger, long elapsedMs, int thresholdMs);

    [LoggerMessage(
        EventId = 6002,
        Level = LogLevel.Debug,
        Message = "Tool '{toolName}' method '{methodName}' started with parameters: {parameters}")]
    public static partial void ToolMethodStarted(ILogger logger, string toolName, string methodName, string parameters);

    [LoggerMessage(
        EventId = 6003,
        Level = LogLevel.Debug,
        Message = "Tool '{toolName}' method '{methodName}' completed in {elapsedMs}ms")]
    public static partial void ToolMethodCompleted(ILogger logger, string toolName, string methodName, long elapsedMs);

    [LoggerMessage(
        EventId = 6004,
        Level = LogLevel.Error,
        Message = "Tool '{toolName}' method '{methodName}' failed after {elapsedMs}ms")]
    public static partial void ToolMethodFailed(ILogger logger, string toolName, string methodName, long elapsedMs, Exception ex);

    // Project Dependency Tools
    [LoggerMessage(
        EventId = 7001,
        Level = LogLevel.Information,
        Message = "Getting project dependencies for session {sessionId}")]
    public static partial void GettingProjectDependencies(ILogger logger, string sessionId);

    [LoggerMessage(
        EventId = 7002,
        Level = LogLevel.Information,
        Message = "Retrieved {projectCount} projects with {dependencyCount} dependencies in session {sessionId} in {elapsedMs}ms")]
    public static partial void ProjectDependenciesRetrieved(ILogger logger, int projectCount, int dependencyCount, string sessionId, long elapsedMs);

    [LoggerMessage(
        EventId = 7003,
        Level = LogLevel.Information,
        Message = "Getting impact analysis for project {projectId} in session {sessionId}")]
    public static partial void GettingImpactAnalysis(ILogger logger, string projectId, string sessionId);

    // Session Lifecycle Tools (9001-9010)
    [LoggerMessage(
        EventId = 9001,
        Level = LogLevel.Information,
        Message = "Pausing session {sessionId} with file watching: {watchFiles}")]
    public static partial void PausingSession(ILogger logger, string sessionId, bool watchFiles);

    [LoggerMessage(
        EventId = 9002,
        Level = LogLevel.Information,
        Message = "Session {sessionId} paused successfully, watching files: {watchFiles} in {elapsedMs}ms")]
    public static partial void SessionPaused(ILogger logger, string sessionId, bool watchFiles, long elapsedMs);

    [LoggerMessage(
        EventId = 9003,
        Level = LogLevel.Error,
        Message = "Failed to pause session {sessionId} after {elapsedMs}ms")]
    public static partial void SessionPauseFailed(ILogger logger, string sessionId, long elapsedMs, Exception ex);

    [LoggerMessage(
        EventId = 9004,
        Level = LogLevel.Information,
        Message = "Resuming session {sessionId}, force full rebuild: {forceFullRebuild}")]
    public static partial void ResumingSession(ILogger logger, string sessionId, bool forceFullRebuild);

    [LoggerMessage(
        EventId = 9005,
        Level = LogLevel.Information,
        Message = "Session {sessionId} resumed: {changedFiles} files changed, {affectedProjects} projects affected, full rebuild: {forceFullRebuild} in {elapsedMs}ms")]
    public static partial void SessionResumed(ILogger logger, string sessionId, int changedFiles, int affectedProjects, bool forceFullRebuild, long elapsedMs);

    [LoggerMessage(
        EventId = 9006,
        Level = LogLevel.Error,
        Message = "Failed to resume session {sessionId} after {elapsedMs}ms")]
    public static partial void SessionResumeFailed(ILogger logger, string sessionId, long elapsedMs, Exception ex);

    [LoggerMessage(
        EventId = 9007,
        Level = LogLevel.Information,
        Message = "Getting pause changes for session {sessionId}")]
    public static partial void GettingPauseChanges(ILogger logger, string sessionId);

    [LoggerMessage(
        EventId = 9008,
        Level = LogLevel.Error,
        Message = "Failed to get pause changes for session {sessionId}")]
    public static partial void GetPauseChangesFailed(ILogger logger, string sessionId, Exception ex);

    [LoggerMessage(
        EventId = 9009,
        Level = LogLevel.Debug,
        Message = "External file changed in session {sessionId}: '{filePath}'")]
    public static partial void ExternalFileChanged(ILogger logger, string sessionId, string filePath);

    [LoggerMessage(
        EventId = 9010,
        Level = LogLevel.Debug,
        Message = "External batch file changes in session {sessionId}: {fileCount} files")]
    public static partial void ExternalBatchFileChanged(ILogger logger, string sessionId, int fileCount);

    // File System Watcher Messages (8001-8006)
    [LoggerMessage(
        EventId = 8001,
        Level = LogLevel.Information,
        Message = "Started file system watcher for session {sessionId} at '{rootPath}' with {patternCount} patterns")]
    public static partial void FileWatcherStarted(ILogger logger, string sessionId, string rootPath, int patternCount);

    [LoggerMessage(
        EventId = 8002,
        Level = LogLevel.Error,
        Message = "Failed to start file system watcher for session {sessionId} at '{rootPath}'")]
    public static partial void FileWatcherStartFailed(ILogger logger, string sessionId, string rootPath, Exception ex);

    [LoggerMessage(
        EventId = 8003,
        Level = LogLevel.Information,
        Message = "Stopped file system watcher for session {sessionId}")]
    public static partial void FileWatcherStopped(ILogger logger, string sessionId);

    [LoggerMessage(
        EventId = 8004,
        Level = LogLevel.Information,
        Message = "Paused file system watcher for session {sessionId}")]
    public static partial void FileWatcherPaused(ILogger logger, string sessionId);

    [LoggerMessage(
        EventId = 8005,
        Level = LogLevel.Information,
        Message = "Resumed file system watcher for session {sessionId}")]
    public static partial void FileWatcherResumed(ILogger logger, string sessionId);

    [LoggerMessage(
        EventId = 8006,
        Level = LogLevel.Debug,
        Message = "File change detected in session {sessionId}: '{filePath}' ({changeType})")]
    public static partial void FileChangeDetected(ILogger logger, string sessionId, string filePath, string changeType);

    // Error Reporting Messages (10001-10005)
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

    // Type Resolution Tools (11001-11010)
    [LoggerMessage(
        EventId = 11001,
        Level = LogLevel.Information,
        Message = "Starting missing type analysis for session {sessionId}")]
    public static partial void MissingTypeAnalysisStarted(ILogger logger, string sessionId);

    [LoggerMessage(
        EventId = 11002,
        Level = LogLevel.Information,
        Message = "Missing type analysis completed for session {sessionId}: found {missingTypeCount} unique types with {totalOccurrences} total occurrences")]
    public static partial void MissingTypeAnalysisCompleted(ILogger logger, string sessionId, int missingTypeCount, int totalOccurrences);

    [LoggerMessage(
        EventId = 11003,
        Level = LogLevel.Error,
        Message = "Missing type analysis failed for session {sessionId}")]
    public static partial void MissingTypeAnalysisFailed(ILogger logger, string sessionId, Exception error);

    [LoggerMessage(
        EventId = 11004,
        Level = LogLevel.Information,
        Message = "Starting using statement suggestions for session {sessionId}, type '{typeName}'")]
    public static partial void UsingSuggestionStarted(ILogger logger, string sessionId, string typeName);

    [LoggerMessage(
        EventId = 11005,
        Level = LogLevel.Information,
        Message = "Using statement suggestions completed for session {sessionId}, type '{typeName}': found {suggestionCount} suggestions")]
    public static partial void UsingSuggestionCompleted(ILogger logger, string sessionId, string typeName, int suggestionCount);

    [LoggerMessage(
        EventId = 11006,
        Level = LogLevel.Error,
        Message = "Using statement suggestions failed for session {sessionId}, type '{typeName}'")]
    public static partial void UsingSuggestionFailed(ILogger logger, string sessionId, string typeName, Exception error);

    [LoggerMessage(
        EventId = 11007,
        Level = LogLevel.Information,
        Message = "Starting bulk using statement addition for session {sessionId} with {fixCount} fixes")]
    public static partial void BulkUsingAdditionStarted(ILogger logger, string sessionId, int fixCount);

    [LoggerMessage(
        EventId = 11008,
        Level = LogLevel.Information,
        Message = "Bulk using statement addition completed for session {sessionId}: {successCount}/{totalCount} files processed successfully")]
    public static partial void BulkUsingAdditionCompleted(ILogger logger, string sessionId, int successCount, int totalCount);

    [LoggerMessage(
        EventId = 11009,
        Level = LogLevel.Error,
        Message = "Bulk using statement addition failed for session {sessionId}")]
    public static partial void BulkUsingAdditionFailed(ILogger logger, string sessionId, Exception error);
}