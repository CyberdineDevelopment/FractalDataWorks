using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.Extensions.Logging;
using RoslynMcpServer.Logging;
using RoslynMcpServer.Models;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

namespace RoslynMcpServer.Services;

public sealed class CompilationCacheService : IDisposable
{
    private readonly ConcurrentDictionary<string, ProjectCompilationCache> _projectCache = new(StringComparer.Ordinal);
    private readonly ConcurrentDictionary<string, ImmutableArray<Diagnostic>> _diagnosticsCache = new(StringComparer.Ordinal);
    private readonly ConcurrentDictionary<string, string> _contentHashes = new(StringComparer.Ordinal);
    private readonly Lock _cacheLock = new();
    private readonly ILogger<CompilationCacheService> _logger;
    private readonly ProjectDependencyService _dependencyService;
    
    // Cache eviction settings
    private const int MaxCacheEntries = 1000;
    private const double EvictionThreshold = 0.8; // Evict when 80% full
    private readonly Timer _evictionTimer;

    public CompilationCacheService(ILogger<CompilationCacheService> logger, ProjectDependencyService dependencyService)
    {
        _logger = logger;
        _dependencyService = dependencyService;
        _evictionTimer = new Timer(EvictStaleEntries, null, TimeSpan.FromMinutes(10), TimeSpan.FromMinutes(10));
    }

    public async Task PrewarmAsync(CompilationSession session)
    {
        // Build dependency graph first
        var dependencyGraph = await _dependencyService.BuildDependencyGraphAsync(session.Id, session.Solution);
        
        // Get leaf projects (no dependencies) for initial compilation
        var leafProjects = dependencyGraph.GetLeafProjects();
        _logger.LogInformation("Prewarming {LeafCount} leaf projects out of {TotalCount} total projects", 
            leafProjects.Count, session.Solution.ProjectIds.Count);
        
        // Compile leaf projects first
        var leafTasks = leafProjects.Select(projectId =>
        {
            var project = session.Solution.GetProject(projectId);
            return project != null ? Task.Run(() => GetOrCreateCompilationAsync(session.Id, project)) : Task.CompletedTask;
        }).Where(task => task != Task.CompletedTask);
        
        await Task.WhenAll(leafTasks);
        
        _logger.LogInformation("Completed prewarming of leaf projects for session {SessionId}", session.Id);
    }

    public async Task<Compilation> GetOrCreateCompilationAsync(string sessionId, Project project)
    {
        var cacheKey = GetProjectCacheKey(sessionId, project.Id);
        var contentHash = await GetProjectContentHashAsync(project);
        
        if (_projectCache.TryGetValue(cacheKey, out var cached) && 
            _contentHashes.TryGetValue(cacheKey, out var cachedHash) &&
            cachedHash == contentHash &&
            cached.Compilation != null)
        {
            _logger.LogDebug("Cache hit for project {ProjectName} in session {SessionId}", project.Name, sessionId);
            // Update access time for LRU
            cached.LastAccessed = DateTime.UtcNow;
            return cached.Compilation;
        }

        _logger.LogDebug("Cache miss for project {ProjectName} in session {SessionId}. Compiling...", project.Name, sessionId);
        var compilation = await project.GetCompilationAsync();
        if (compilation == null)
            throw new InvalidOperationException($"Failed to create compilation for project {project.Name}");

        lock (_cacheLock)
        {
            _projectCache[cacheKey] = new ProjectCompilationCache
            {
                Compilation = compilation,
                Version = project.Version,
                LastUpdated = DateTime.UtcNow,
                LastAccessed = DateTime.UtcNow
            };
            _contentHashes[cacheKey] = contentHash;
            
            // Check if we need to evict entries
            if (_projectCache.Count > MaxCacheEntries * EvictionThreshold)
            {
                _ = Task.Run(EvictLeastRecentlyUsed);
            }
        }

        return compilation;
    }

    public async Task<ImmutableArray<Diagnostic>> GetDiagnosticsAsync(
        string sessionId, 
        Project project, 
        ImmutableArray<DiagnosticAnalyzer> analyzers, 
        bool includeAnalyzerDiagnostics = true)
    {
        var compilation = await GetOrCreateCompilationAsync(sessionId, project);
        var diagnosticsCacheKey = GetDiagnosticsCacheKey(sessionId, project.Id, includeAnalyzerDiagnostics);
        
        if (_diagnosticsCache.TryGetValue(diagnosticsCacheKey, out var cachedDiagnostics))
        {
            return cachedDiagnostics;
        }

        var diagnostics = compilation.GetDiagnostics();
        
        if (includeAnalyzerDiagnostics && !analyzers.IsEmpty)
        {
            var analyzerDiagnostics = await compilation
                .WithAnalyzers(analyzers)
                .GetAnalyzerDiagnosticsAsync();
            
            diagnostics = diagnostics.AddRange(analyzerDiagnostics);
        }

        lock (_cacheLock)
        {
            _diagnosticsCache[diagnosticsCacheKey] = diagnostics;
        }

        return diagnostics;
    }

    public async Task<Compilation> GetCompilationWithChangesAsync(
        string sessionId, 
        Project project, 
        Dictionary<DocumentId, SyntaxTree> pendingChanges)
    {
        var baseCompilation = await GetOrCreateCompilationAsync(sessionId, project);
        
        if (!pendingChanges.Any(kvp => project.DocumentIds.Contains(kvp.Key)))
            return baseCompilation;

        var compilation = baseCompilation;
        
        foreach (var (documentId, newTree) in pendingChanges)
        {
            if (!project.DocumentIds.Contains(documentId))
                continue;
                
            var document = project.GetDocument(documentId);
            if (document == null)
                continue;

            var oldTree = await document.GetSyntaxTreeAsync();
            if (oldTree != null)
            {
                compilation = compilation.ReplaceSyntaxTree(oldTree, newTree);
            }
        }

        return compilation;
    }

    public void InvalidateProject(string sessionId, ProjectId projectId)
    {
        var cacheKey = GetProjectCacheKey(sessionId, projectId);
        
        lock (_cacheLock)
        {
            _projectCache.TryRemove(cacheKey, out _);
            _contentHashes.TryRemove(cacheKey, out _);
            
            var diagnosticKeys = _diagnosticsCache.Keys
                .Where(k => k.StartsWith($"{sessionId}:{projectId}:"))
                .ToList();
            
            foreach (var key in diagnosticKeys)
            {
                _diagnosticsCache.TryRemove(key, out _);
            }
        }
        
        _logger.LogDebug("Invalidated cache for project {ProjectId} in session {SessionId}", projectId, sessionId);
    }
    
    public void InvalidateAffectedProjects(string sessionId, ProjectId changedProjectId)
    {
        var affectedProjects = _dependencyService.GetAffectedProjects(sessionId, changedProjectId);
        
        _logger.LogInformation("Invalidating {AffectedCount} projects affected by changes to {ProjectId} in session {SessionId}", 
            affectedProjects.Count, changedProjectId, sessionId);
            
        foreach (var projectId in affectedProjects)
        {
            InvalidateProject(sessionId, projectId);
        }
    }

    public Task InvalidateAsync(string sessionId)
    {
        lock (_cacheLock)
        {
            var projectKeys = _projectCache.Keys
                .Where(k => k.StartsWith($"{sessionId}:"))
                .ToList();
            
            foreach (var key in projectKeys)
            {
                _projectCache.TryRemove(key, out _);
                _contentHashes.TryRemove(key, out _);
            }

            var diagnosticKeys = _diagnosticsCache.Keys
                .Where(k => k.StartsWith($"{sessionId}:"))
                .ToList();
            
            foreach (var key in diagnosticKeys)
            {
                _diagnosticsCache.TryRemove(key, out _);
            }
        }
        
        _dependencyService.InvalidateGraph(sessionId);
        _logger.LogInformation("Invalidated all cache entries for session {SessionId}", sessionId);
        
        return Task.CompletedTask;
    }

    public void ClearCache()
    {
        lock (_cacheLock)
        {
            _projectCache.Clear();
            _diagnosticsCache.Clear();
            _contentHashes.Clear();
        }
        
        _logger.LogWarning("Cleared entire compilation cache");
    }

    public CacheStats GetCacheStats()
    {
        lock (_cacheLock)
        {
            var sessionGroups = _projectCache.Keys
                .Select(k => k.Split(':')[0])
                .GroupBy(s => s, StringComparer.Ordinal)
                .ToDictionary(g => g.Key, g => g.Count(), StringComparer.Ordinal);

            return new CacheStats
            {
                ProjectCompilationCount = _projectCache.Count,
                DiagnosticsCacheCount = _diagnosticsCache.Count,
                SessionCount = sessionGroups.Count,
                SessionBreakdown = sessionGroups
            };
        }
    }

    private static string GetProjectCacheKey(string sessionId, ProjectId projectId)
        => $"{sessionId}:{projectId}";

    private static string GetDiagnosticsCacheKey(string sessionId, ProjectId projectId, bool includeAnalyzers)
        => $"{sessionId}:{projectId}:diag:{includeAnalyzers}";

    private static async Task<string> GetProjectContentHashAsync(Project project)
    {
        var hashInput = new StringBuilder();
        hashInput.Append(project.Name);
        hashInput.Append(project.AssemblyName);
        hashInput.Append(project.CompilationOptions?.ToString() ?? "");
        
        // Hash all document contents
        foreach (var document in project.Documents.OrderBy(d => d.FilePath, StringComparer.Ordinal))
        {
            var text = await document.GetTextAsync();
            hashInput.Append(document.FilePath ?? document.Name);
            hashInput.Append(text.ToString());
        }
        
        // Hash project references
        foreach (var projectRef in project.ProjectReferences.OrderBy(p => p.ProjectId.ToString(), StringComparer.Ordinal))
        {
            hashInput.Append(projectRef.ProjectId.ToString());
        }
        
        // Hash metadata references
        foreach (var metaRef in project.MetadataReferences.OrderBy(m => m.Display, StringComparer.Ordinal))
        {
            hashInput.Append(metaRef.Display ?? "");
        }
        
        var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(hashInput.ToString()));
        return Convert.ToHexString(hashBytes);
    }
    
    private void EvictStaleEntries(object? state)
    {
        lock (_cacheLock)
        {
            var cutoff = DateTime.UtcNow.AddHours(-2); // Evict entries older than 2 hours
            var staleKeys = _projectCache
                .Where(kvp => kvp.Value.LastAccessed < cutoff)
                .Select(kvp => kvp.Key)
                .ToList();
                
            foreach (var key in staleKeys)
            {
                _projectCache.TryRemove(key, out _);
                _contentHashes.TryRemove(key, out _);
            }
            
            if (staleKeys.Count > 0)
            {
                _logger.LogDebug("Evicted {Count} stale cache entries", staleKeys.Count);
            }
        }
    }
    
    private void EvictLeastRecentlyUsed()
    {
        lock (_cacheLock)
        {
            var entriesToRemove = Math.Max(1, (int)(_projectCache.Count * 0.2)); // Remove 20%
            var lruEntries = _projectCache
                .OrderBy(kvp => kvp.Value.LastAccessed)
                .Take(entriesToRemove)
                .Select(kvp => kvp.Key)
                .ToList();
                
            foreach (var key in lruEntries)
            {
                _projectCache.TryRemove(key, out _);
                _contentHashes.TryRemove(key, out _);
            }
            
            _logger.LogInformation("Evicted {Count} LRU cache entries to manage memory", lruEntries.Count);
        }
    }
    
    public void Dispose()
    {
        _evictionTimer?.Dispose();
    }
    
    private sealed class ProjectCompilationCache
    {
        public Compilation? Compilation { get; init; }
        public VersionStamp Version { get; init; }
        public DateTime LastUpdated { get; init; }
        public DateTime LastAccessed { get; set; }
    }
}

public sealed record CacheStats
{
    public int ProjectCompilationCount { get; init; }
    public int DiagnosticsCacheCount { get; init; }
    public int SessionCount { get; init; }
    public Dictionary<string, int> SessionBreakdown { get; init; } = new(StringComparer.Ordinal);
}