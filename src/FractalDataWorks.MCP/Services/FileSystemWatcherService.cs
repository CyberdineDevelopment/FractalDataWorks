using Microsoft.Extensions.Logging;
using RoslynMcpServer.Logging;
using System.Collections.Concurrent;

namespace RoslynMcpServer.Services;

public sealed class FileSystemWatcherService : IDisposable
{
    private readonly ILogger<FileSystemWatcherService> _logger;
    private readonly ConcurrentDictionary<string, FileSystemWatcher> _watchers = new(StringComparer.Ordinal);
    private readonly ConcurrentDictionary<string, DateTime> _lastChangeTime = new(StringComparer.Ordinal);
    private readonly Timer _debounceTimer;
    
    // Events for notifying consumers
    public event EventHandler<FileChangedEventArgs>? FileChanged;
    public event EventHandler<BatchFileChangedEventArgs>? BatchFileChanged;
    
    private const int DebounceMs = 500; // Wait 500ms for batch changes
    private readonly ConcurrentDictionary<string, HashSet<string>> _pendingChanges = new(StringComparer.Ordinal);
    private readonly Lock _pendingLock = new();

    public FileSystemWatcherService(ILogger<FileSystemWatcherService> logger)
    {
        _logger = logger;
        _debounceTimer = new Timer(ProcessPendingChanges, null, Timeout.Infinite, Timeout.Infinite);
    }

    public void StartWatching(string sessionId, string rootPath, IEnumerable<string> filePatterns)
    {
        if (_watchers.ContainsKey(sessionId))
        {
            StopWatching(sessionId);
        }

        try
        {
            var watcher = new FileSystemWatcher(rootPath)
            {
                IncludeSubdirectories = true,
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName,
                EnableRaisingEvents = false
            };

            // Watch common code file patterns
            foreach (var pattern in filePatterns.DefaultIfEmpty("*.cs"))
            {
                watcher.Filter = pattern;
                break; // FileSystemWatcher only supports one filter
            }

            watcher.Changed += (sender, e) => OnFileChanged(sessionId, e.FullPath, e.ChangeType);
            watcher.Created += (sender, e) => OnFileChanged(sessionId, e.FullPath, e.ChangeType);
            watcher.Deleted += (sender, e) => OnFileChanged(sessionId, e.FullPath, e.ChangeType);
            watcher.Renamed += (sender, e) => OnFileRenamed(sessionId, e.OldFullPath, e.FullPath);

            watcher.EnableRaisingEvents = true;
            _watchers[sessionId] = watcher;

            ToolLogMessages.FileWatcherStarted(_logger, sessionId, rootPath, filePatterns.Count());
        }
        catch (Exception ex)
        {
            ToolLogMessages.FileWatcherStartFailed(_logger, sessionId, rootPath, ex);
        }
    }

    public void StopWatching(string sessionId)
    {
        if (_watchers.TryRemove(sessionId, out var watcher))
        {
            watcher.EnableRaisingEvents = false;
            watcher.Dispose();
            
            lock (_pendingLock)
            {
                _pendingChanges.TryRemove(sessionId, out _);
            }
            
            ToolLogMessages.FileWatcherStopped(_logger, sessionId);
        }
    }

    public void PauseWatching(string sessionId)
    {
        if (_watchers.TryGetValue(sessionId, out var watcher))
        {
            watcher.EnableRaisingEvents = false;
            ToolLogMessages.FileWatcherPaused(_logger, sessionId);
        }
    }

    public void ResumeWatching(string sessionId)
    {
        if (_watchers.TryGetValue(sessionId, out var watcher))
        {
            watcher.EnableRaisingEvents = true;
            ToolLogMessages.FileWatcherResumed(_logger, sessionId);
        }
    }

    public IEnumerable<string> GetRecentChanges(string sessionId, TimeSpan since)
    {
        var cutoff = DateTime.UtcNow - since;
        return _lastChangeTime
            .Where(kvp => kvp.Key.StartsWith($"{sessionId}:") && kvp.Value > cutoff)
            .Select(kvp => kvp.Key.Substring($"{sessionId}:".Length))
            .ToList();
    }

    private void OnFileChanged(string sessionId, string filePath, WatcherChangeTypes changeType)
    {
        // Debounce rapid changes (e.g., editor auto-save)
        var key = $"{sessionId}:{filePath}";
        var now = DateTime.UtcNow;
        
        if (_lastChangeTime.TryGetValue(key, out var lastChange) && 
            now - lastChange < TimeSpan.FromMilliseconds(100))
        {
            return; // Skip rapid successive changes
        }

        _lastChangeTime[key] = now;

        lock (_pendingLock)
        {
            if (!_pendingChanges.TryGetValue(sessionId, out var changes))
            {
                changes = new HashSet<string>(StringComparer.Ordinal);
                _pendingChanges[sessionId] = changes;
            }
            
            changes.Add(filePath);
        }

        // Reset debounce timer
        _debounceTimer.Change(DebounceMs, Timeout.Infinite);
        
        ToolLogMessages.FileChangeDetected(_logger, sessionId, filePath, changeType.ToString());
    }

    private void OnFileRenamed(string sessionId, string oldPath, string newPath)
    {
        OnFileChanged(sessionId, oldPath, WatcherChangeTypes.Deleted);
        OnFileChanged(sessionId, newPath, WatcherChangeTypes.Created);
    }

    private void ProcessPendingChanges(object? state)
    {
        Dictionary<string, HashSet<string>> toProcess;
        
        lock (_pendingLock)
        {
            toProcess = new Dictionary<string, HashSet<string>>(_pendingChanges, StringComparer.Ordinal);
            _pendingChanges.Clear();
        }

        foreach (var (sessionId, changes) in toProcess)
        {
            if (changes.Count == 1)
            {
                FileChanged?.Invoke(this, new FileChangedEventArgs(sessionId, changes.First()));
            }
            else if (changes.Count > 1)
            {
                BatchFileChanged?.Invoke(this, new BatchFileChangedEventArgs(sessionId, changes));
            }
        }
    }

    public void Dispose()
    {
        _debounceTimer?.Dispose();
        
        foreach (var watcher in _watchers.Values)
        {
            watcher.EnableRaisingEvents = false;
            watcher.Dispose();
        }
        
        _watchers.Clear();
        _lastChangeTime.Clear();
        _pendingChanges.Clear();
    }
}

public sealed class FileChangedEventArgs : EventArgs
{
    public string SessionId { get; }
    public string FilePath { get; }

    public FileChangedEventArgs(string sessionId, string filePath)
    {
        SessionId = sessionId;
        FilePath = filePath;
    }
}

public sealed class BatchFileChangedEventArgs : EventArgs
{
    public string SessionId { get; }
    public IEnumerable<string> FilePaths { get; }

    public BatchFileChangedEventArgs(string sessionId, IEnumerable<string> filePaths)
    {
        SessionId = sessionId;
        FilePaths = filePaths;
    }
}

public static partial class FileSystemWatcherLogMessages
{
    [LoggerMessage(
        EventId = 8001,
        Level = LogLevel.Information,
        Message = "Started file system watcher for session {sessionId} at '{rootPath}' with {patternCount} patterns")]
    public static partial void FileWatcherStarted(this ILogger logger, string sessionId, string rootPath, int patternCount);

    [LoggerMessage(
        EventId = 8002,
        Level = LogLevel.Error,
        Message = "Failed to start file system watcher for session {sessionId} at '{rootPath}'")]
    public static partial void FileWatcherStartFailed(this ILogger logger, string sessionId, string rootPath, Exception ex);

    [LoggerMessage(
        EventId = 8003,
        Level = LogLevel.Information,
        Message = "Stopped file system watcher for session {sessionId}")]
    public static partial void FileWatcherStopped(this ILogger logger, string sessionId);

    [LoggerMessage(
        EventId = 8004,
        Level = LogLevel.Information,
        Message = "Paused file system watcher for session {sessionId}")]
    public static partial void FileWatcherPaused(this ILogger logger, string sessionId);

    [LoggerMessage(
        EventId = 8005,
        Level = LogLevel.Information,
        Message = "Resumed file system watcher for session {sessionId}")]
    public static partial void FileWatcherResumed(this ILogger logger, string sessionId);

    [LoggerMessage(
        EventId = 8006,
        Level = LogLevel.Debug,
        Message = "File change detected in session {sessionId}: '{filePath}' ({changeType})")]
    public static partial void FileChangeDetected(this ILogger logger, string sessionId, string filePath, string changeType);
}