using ModelContextProtocol.Server;
using RoslynMcpServer.Services;
using System.ComponentModel;
using System.Text.Json;

namespace RoslynMcpServer.Tools;

[McpServerToolType]
public class ProjectDependencyTools
{
    private readonly WorkspaceSessionManager _sessionManager;
    private readonly ProjectDependencyService _dependencyService;

    public ProjectDependencyTools(WorkspaceSessionManager sessionManager, ProjectDependencyService dependencyService)
    {
        _sessionManager = sessionManager;
        _dependencyService = dependencyService;
    }

    [McpServerTool]
    [Description("Get project dependency graph and statistics. Shows which projects depend on others, enabling incremental builds and impact analysis")]
    public string GetProjectDependencies(
        [Description("Session ID from StartSession")] string sessionId)
    {
        var session = _sessionManager.GetSession(sessionId);
        if (session == null)
        {
            return JsonSerializer.Serialize(new
            {
                success = false,
                error = "Session not found"
            }, new JsonSerializerOptions { WriteIndented = true });
        }

        var dependencyGraph = _dependencyService.GetDependencyGraph(sessionId);
        if (dependencyGraph == null)
        {
            return JsonSerializer.Serialize(new
            {
                success = false,
                error = "Dependency graph not built. Try refreshing the session."
            }, new JsonSerializerOptions { WriteIndented = true });
        }

        var stats = _dependencyService.GetDependencyStats(sessionId);
        var leafProjects = dependencyGraph.GetLeafProjects();
        var rootProjects = dependencyGraph.GetRootProjects();

        return JsonSerializer.Serialize(new
        {
            success = true,
            sessionId,
            stats = new
            {
                totalProjects = stats.TotalProjects,
                leafProjects = stats.LeafProjects,
                rootProjects = stats.RootProjects,
                maxDepth = stats.MaxDepth,
                createdAt = stats.CreatedAt
            },
            leafProjectIds = leafProjects.Select(id => id.ToString()).ToArray(),
            rootProjectIds = rootProjects.Select(id => id.ToString()).ToArray(),
            projectInfo = dependencyGraph.ProjectInfo.Values.Select(p => new
            {
                id = p.Id.ToString(),
                name = p.Name,
                language = p.Language,
                documentCount = p.DocumentCount,
                referenceCount = p.MetadataReferenceCount
            }).ToArray()
        }, new JsonSerializerOptions { WriteIndented = true });
    }

    [McpServerTool]
    [Description("Get which projects will be affected by changes to a specific project. Critical for incremental builds and impact analysis")]
    public string GetImpactAnalysis(
        [Description("Session ID from StartSession")] string sessionId,
        [Description("Project ID to analyze impact for")] string projectId)
    {
        var session = _sessionManager.GetSession(sessionId);
        if (session == null)
        {
            return JsonSerializer.Serialize(new
            {
                success = false,
                error = "Session not found"
            }, new JsonSerializerOptions { WriteIndented = true });
        }

        Microsoft.CodeAnalysis.ProjectId parsedProjectId;
        try
        {
            parsedProjectId = Microsoft.CodeAnalysis.ProjectId.CreateFromSerialized(Guid.Parse(projectId));
        }
        catch (FormatException)
        {
            return JsonSerializer.Serialize(new
            {
                success = false,
                error = "Invalid project ID format"
            }, new JsonSerializerOptions { WriteIndented = true });
        }

        var affectedProjects = _dependencyService.GetAffectedProjects(sessionId, parsedProjectId);
        var dependencyGraph = _dependencyService.GetDependencyGraph(sessionId);
        
        if (dependencyGraph == null)
        {
            return JsonSerializer.Serialize(new
            {
                success = false,
                error = "Dependency graph not available"
            }, new JsonSerializerOptions { WriteIndented = true });
        }

        var projectInfo = dependencyGraph.ProjectInfo;
        projectInfo.TryGetValue(parsedProjectId, out var targetProject);

        return JsonSerializer.Serialize(new
        {
            success = true,
            sessionId,
            targetProject = targetProject != null ? new
            {
                id = targetProject.Id.ToString(),
                name = targetProject.Name,
                language = targetProject.Language
            } : null,
            affectedProjectCount = affectedProjects.Count,
            affectedProjects = affectedProjects
                .Where(id => projectInfo.ContainsKey(id))
                .Select(id => new
                {
                    id = id.ToString(),
                    name = projectInfo[id].Name,
                    language = projectInfo[id].Language,
                    documentCount = projectInfo[id].DocumentCount
                })
                .ToArray()
        }, new JsonSerializerOptions { WriteIndented = true });
    }

    [McpServerTool]
    [Description("Get optimal compilation order based on project dependencies. Use this to compile projects in the right sequence for incremental builds")]
    public string GetCompilationOrder(
        [Description("Session ID from StartSession")] string sessionId)
    {
        var session = _sessionManager.GetSession(sessionId);
        if (session == null)
        {
            return JsonSerializer.Serialize(new
            {
                success = false,
                error = "Session not found"
            }, new JsonSerializerOptions { WriteIndented = true });
        }

        var compilationOrder = _dependencyService.GetCompilationOrder(sessionId);
        var dependencyGraph = _dependencyService.GetDependencyGraph(sessionId);
        
        if (dependencyGraph == null)
        {
            return JsonSerializer.Serialize(new
            {
                success = false,
                error = "Dependency graph not available"
            }, new JsonSerializerOptions { WriteIndented = true });
        }

        var projectInfo = dependencyGraph.ProjectInfo;

        return JsonSerializer.Serialize(new
        {
            success = true,
            sessionId,
            compilationOrder = compilationOrder
                .Where(id => projectInfo.ContainsKey(id))
                .Select((id, index) => new
                {
                    order = index + 1,
                    id = id.ToString(),
                    name = projectInfo[id].Name,
                    language = projectInfo[id].Language,
                    documentCount = projectInfo[id].DocumentCount
                })
                .ToArray()
        }, new JsonSerializerOptions { WriteIndented = true });
    }

    [McpServerTool]
    [Description("Get detailed information about a specific project including its direct dependencies and dependents")]
    public string GetProjectDetails(
        [Description("Session ID from StartSession")] string sessionId,
        [Description("Project ID to get details for")] string projectId)
    {
        var session = _sessionManager.GetSession(sessionId);
        if (session == null)
        {
            return JsonSerializer.Serialize(new
            {
                success = false,
                error = "Session not found"
            }, new JsonSerializerOptions { WriteIndented = true });
        }

        Microsoft.CodeAnalysis.ProjectId parsedProjectId;
        try
        {
            parsedProjectId = Microsoft.CodeAnalysis.ProjectId.CreateFromSerialized(Guid.Parse(projectId));
        }
        catch (FormatException)
        {
            return JsonSerializer.Serialize(new
            {
                success = false,
                error = "Invalid project ID format"
            }, new JsonSerializerOptions { WriteIndented = true });
        }

        var dependencyGraph = _dependencyService.GetDependencyGraph(sessionId);
        if (dependencyGraph == null)
        {
            return JsonSerializer.Serialize(new
            {
                success = false,
                error = "Dependency graph not available"
            }, new JsonSerializerOptions { WriteIndented = true });
        }

        if (!dependencyGraph.ProjectInfo.TryGetValue(parsedProjectId, out var projectInfo))
        {
            return JsonSerializer.Serialize(new
            {
                success = false,
                error = "Project not found in dependency graph"
            }, new JsonSerializerOptions { WriteIndented = true });
        }

        var dependencies = dependencyGraph.ProjectReferences.TryGetValue(parsedProjectId, out var deps) 
            ? deps : [];
        var dependents = dependencyGraph.ReverseDependencies.TryGetValue(parsedProjectId, out var rdeps) 
            ? rdeps : [];

        return JsonSerializer.Serialize(new
        {
            success = true,
            sessionId,
            project = new
            {
                id = projectInfo.Id.ToString(),
                name = projectInfo.Name,
                filePath = projectInfo.FilePath,
                language = projectInfo.Language,
                documentCount = projectInfo.DocumentCount,
                metadataReferenceCount = projectInfo.MetadataReferenceCount
            },
            directDependencies = dependencies
                .Where(id => dependencyGraph.ProjectInfo.ContainsKey(id))
                .Select(id => new
                {
                    id = id.ToString(),
                    name = dependencyGraph.ProjectInfo[id].Name
                })
                .ToArray(),
            directDependents = dependents
                .Where(id => dependencyGraph.ProjectInfo.ContainsKey(id))
                .Select(id => new
                {
                    id = id.ToString(),
                    name = dependencyGraph.ProjectInfo[id].Name
                })
                .ToArray()
        }, new JsonSerializerOptions { WriteIndented = true });
    }
}