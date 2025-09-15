using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Logging;
using RoslynMcpServer.Logging;
using System.Collections.Concurrent;
using System.Collections.Immutable;

namespace RoslynMcpServer.Services;

public sealed class ProjectDependencyService
{
    private readonly ILogger<ProjectDependencyService> _logger;
    private readonly ConcurrentDictionary<string, ProjectDependencyGraph> _dependencyGraphs = new(StringComparer.Ordinal);

    public ProjectDependencyService(ILogger<ProjectDependencyService> logger)
    {
        _logger = logger;
    }

    public Task<ProjectDependencyGraph> BuildDependencyGraphAsync(string sessionId, Solution solution)
    {
        if (_dependencyGraphs.TryGetValue(sessionId, out var existingGraph))
            return Task.FromResult(existingGraph);

        _logger.LogInformation("Building dependency graph for session {SessionId} with {ProjectCount} projects", 
            sessionId, solution.ProjectIds.Count);

        var projectReferences = new Dictionary<ProjectId, ISet<ProjectId>>(solution.ProjectIds.Count);
        var reverseDependencies = new Dictionary<ProjectId, ISet<ProjectId>>(solution.ProjectIds.Count);
        var projectInfo = new Dictionary<ProjectId, ProjectInfo>();

        // Initialize collections
        foreach (var projectId in solution.ProjectIds)
        {
            projectReferences[projectId] = new HashSet<ProjectId>();
            reverseDependencies[projectId] = new HashSet<ProjectId>();
        }

        // Build dependency maps
        foreach (var project in solution.Projects)
        {
            projectInfo[project.Id] = new ProjectInfo
            {
                Id = project.Id,
                Name = project.Name,
                FilePath = project.FilePath ?? "",
                Language = project.Language,
                DocumentCount = project.DocumentIds.Count,
                MetadataReferenceCount = project.MetadataReferences.Count
            };

            foreach (var reference in project.ProjectReferences)
            {
                var referencedProjectId = reference.ProjectId;
                if (solution.ProjectIds.Contains(referencedProjectId))
                {
                    projectReferences[project.Id].Add(referencedProjectId);
                    reverseDependencies[referencedProjectId].Add(project.Id);
                }
            }
        }

        var graph = new ProjectDependencyGraph
        {
            SessionId = sessionId,
            ProjectReferences = projectReferences.ToImmutableDictionary(
                kvp => kvp.Key, 
                kvp => kvp.Value.ToImmutableHashSet()),
            ReverseDependencies = reverseDependencies.ToImmutableDictionary(
                kvp => kvp.Key, 
                kvp => kvp.Value.ToImmutableHashSet()),
            ProjectInfo = projectInfo.ToImmutableDictionary(),
            CreatedAt = DateTime.UtcNow
        };

        _dependencyGraphs[sessionId] = graph;
        
        _logger.LogInformation("Dependency graph built for session {SessionId}. " +
            "Leaf projects: {LeafCount}, Root projects: {RootCount}",
            sessionId, graph.GetLeafProjects().Count, graph.GetRootProjects().Count);

        return Task.FromResult(graph);
    }

    public ProjectDependencyGraph? GetDependencyGraph(string sessionId)
    {
        _dependencyGraphs.TryGetValue(sessionId, out var graph);
        return graph;
    }

    public ImmutableHashSet<ProjectId> GetAffectedProjects(string sessionId, ProjectId changedProjectId)
    {
        if (!_dependencyGraphs.TryGetValue(sessionId, out var graph))
            return ImmutableHashSet<ProjectId>.Empty;

        return graph.GetDownstreamDependencies(changedProjectId);
    }

    public ImmutableHashSet<ProjectId> GetCompilationOrder(string sessionId)
    {
        if (!_dependencyGraphs.TryGetValue(sessionId, out var graph))
            return ImmutableHashSet<ProjectId>.Empty;

        return graph.GetTopologicalOrder();
    }

    public void InvalidateGraph(string sessionId)
    {
        _dependencyGraphs.TryRemove(sessionId, out _);
        _logger.LogInformation("Invalidated dependency graph for session {SessionId}", sessionId);
    }

    public DependencyStats GetDependencyStats(string sessionId)
    {
        if (!_dependencyGraphs.TryGetValue(sessionId, out var graph))
        {
            return new DependencyStats
            {
                SessionId = sessionId,
                HasGraph = false
            };
        }

        var leafProjects = graph.GetLeafProjects();
        var rootProjects = graph.GetRootProjects();
        
        return new DependencyStats
        {
            SessionId = sessionId,
            HasGraph = true,
            TotalProjects = graph.ProjectInfo.Count,
            LeafProjects = leafProjects.Count,
            RootProjects = rootProjects.Count,
            MaxDepth = CalculateMaxDepth(graph),
            CreatedAt = graph.CreatedAt
        };
    }

    private static int CalculateMaxDepth(ProjectDependencyGraph graph)
    {
        var visited = new HashSet<ProjectId>();
        var maxDepth = 0;

        foreach (var leafProject in graph.GetLeafProjects())
        {
            var depth = CalculateDepthFromLeaf(graph, leafProject, visited);
            maxDepth = Math.Max(maxDepth, depth);
        }

        return maxDepth;
    }

    private static int CalculateDepthFromLeaf(ProjectDependencyGraph graph, ProjectId projectId, HashSet<ProjectId> visited)
    {
        if (visited.Contains(projectId))
            return 0; // Circular reference protection

        visited.Add(projectId);

        if (!graph.ReverseDependencies.TryGetValue(projectId, out var dependents) || dependents.Count == 0)
            return 1; // Leaf node

        var maxDependent = 0;
        foreach (var dependent in dependents)
        {
            var depth = CalculateDepthFromLeaf(graph, dependent, visited);
            maxDependent = Math.Max(maxDependent, depth);
        }

        visited.Remove(projectId);
        return maxDependent + 1;
    }
}

public sealed record ProjectDependencyGraph
{
    public string SessionId { get; init; } = string.Empty;
    public ImmutableDictionary<ProjectId, ImmutableHashSet<ProjectId>> ProjectReferences { get; init; } = 
        ImmutableDictionary<ProjectId, ImmutableHashSet<ProjectId>>.Empty;
    public ImmutableDictionary<ProjectId, ImmutableHashSet<ProjectId>> ReverseDependencies { get; init; } = 
        ImmutableDictionary<ProjectId, ImmutableHashSet<ProjectId>>.Empty;
    public ImmutableDictionary<ProjectId, ProjectInfo> ProjectInfo { get; init; } = 
        ImmutableDictionary<ProjectId, ProjectInfo>.Empty;
    public DateTime CreatedAt { get; init; }

    public ImmutableHashSet<ProjectId> GetLeafProjects()
    {
        return ProjectReferences
            .Where(kvp => kvp.Value.Count == 0)
            .Select(kvp => kvp.Key)
            .ToImmutableHashSet();
    }

    public ImmutableHashSet<ProjectId> GetRootProjects()
    {
        return ReverseDependencies
            .Where(kvp => kvp.Value.Count == 0)
            .Select(kvp => kvp.Key)
            .ToImmutableHashSet();
    }

    public ImmutableHashSet<ProjectId> GetDownstreamDependencies(ProjectId projectId)
    {
        var affected = new HashSet<ProjectId> { projectId };
        var queue = new Queue<ProjectId>();
        queue.Enqueue(projectId);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            
            if (ReverseDependencies.TryGetValue(current, out var dependents))
            {
                foreach (var dependent in dependents)
                {
                    if (affected.Add(dependent))
                    {
                        queue.Enqueue(dependent);
                    }
                }
            }
        }

        return affected.ToImmutableHashSet();
    }

    public ImmutableHashSet<ProjectId> GetTopologicalOrder()
    {
        var result = new List<ProjectId>();
        var visited = new HashSet<ProjectId>();
        var visiting = new HashSet<ProjectId>();

        foreach (var projectId in ProjectReferences.Keys)
        {
            if (!visited.Contains(projectId))
            {
                TopologicalSortUtil(projectId, visited, visiting, result);
            }
        }

        return result.ToImmutableHashSet();
    }

    private void TopologicalSortUtil(ProjectId projectId, HashSet<ProjectId> visited, 
        HashSet<ProjectId> visiting, List<ProjectId> result)
    {
        if (visiting.Contains(projectId))
            return; // Circular dependency - skip

        if (visited.Contains(projectId))
            return;

        visiting.Add(projectId);

        if (ProjectReferences.TryGetValue(projectId, out var references))
        {
            foreach (var reference in references)
            {
                TopologicalSortUtil(reference, visited, visiting, result);
            }
        }

        visiting.Remove(projectId);
        visited.Add(projectId);
        result.Add(projectId);
    }
}

public sealed record ProjectInfo
{
    public required ProjectId Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string FilePath { get; init; } = string.Empty;
    public string Language { get; init; } = string.Empty;
    public int DocumentCount { get; init; }
    public int MetadataReferenceCount { get; init; }
}

public sealed record DependencyStats
{
    public string SessionId { get; init; } = string.Empty;
    public bool HasGraph { get; init; }
    public int TotalProjects { get; init; }
    public int LeafProjects { get; init; }
    public int RootProjects { get; init; }
    public int MaxDepth { get; init; }
    public DateTime CreatedAt { get; init; }
}