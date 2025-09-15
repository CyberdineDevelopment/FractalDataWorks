using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Host;
using Microsoft.CodeAnalysis.MSBuild;
using Microsoft.CodeAnalysis.Text;
using Microsoft.Extensions.Logging;
using RoslynMcpServer.Logging;
using RoslynMcpServer.Models;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reflection;

namespace RoslynMcpServer.Services;

public sealed class WorkspaceSessionManager : IDisposable
{
    private readonly ConcurrentDictionary<string, CompilationSession> _sessions = new(StringComparer.Ordinal);
    private readonly CompilationCacheService _cache;
    private readonly AnalyzerService _analyzerService;
    private readonly ILogger<WorkspaceSessionManager> _logger;
    private readonly Timer _cleanupTimer;

    public WorkspaceSessionManager(CompilationCacheService cache, AnalyzerService analyzerService, ILogger<WorkspaceSessionManager> logger)
    {
        _cache = cache;
        _analyzerService = analyzerService;
        _logger = logger;
        
        _cleanupTimer = new Timer(CleanupStaleSessionsCallback, null, TimeSpan.FromMinutes(30), TimeSpan.FromMinutes(30));
    }

    public async Task<string> StartSessionAsync(string solutionPath, AnalyzerConfiguration? config = null)
    {
        Console.WriteLine($"[WSM DEBUG] StartSessionAsync called with: {solutionPath}");
        
        if (!File.Exists(solutionPath))
        {
            Console.WriteLine($"[WSM DEBUG] File not found: {solutionPath}");
            throw new FileNotFoundException($"Solution file not found: {solutionPath}");
        }

        var sessionId = Guid.NewGuid().ToString("N");
        Console.WriteLine($"[WSM DEBUG] Generated session ID: {sessionId}");
        
        try
        {
            Solution solution;
            
            // Load solution or project using AdhocWorkspace
            if (solutionPath.EndsWith(".sln", StringComparison.OrdinalIgnoreCase))
            {
                // For .sln files, create a basic solution structure
                solution = await LoadSolutionAsync(solutionPath);
            }
            else if (solutionPath.EndsWith(".csproj", StringComparison.OrdinalIgnoreCase))
            {
                // For .csproj files, load as single project
                solution = await LoadProjectAsync(solutionPath);
            }
            else
            {
                throw new ArgumentException($"Unsupported file type: {Path.GetExtension(solutionPath)}", nameof(solutionPath));
            }
            
            var analyzers = await _analyzerService.LoadAnalyzersAsync(config ?? new AnalyzerConfiguration());

            var session = new CompilationSession
            {
                Id = sessionId,
                SolutionPath = solutionPath,
                Workspace = solution.Workspace,
                Solution = solution,
                Analyzers = analyzers
            };

            _sessions[sessionId] = session;
            
            await _cache.PrewarmAsync(session);
            
            return sessionId;
        }
        catch (Exception)
        {
            // Workspace is shared, don't dispose
            throw;
        }
    }

    public CompilationSession? GetSession(string sessionId)
    {
        if (_sessions.TryGetValue(sessionId, out var session))
        {
            session.UpdateAccess();
            return session;
        }
        return null;
    }

    public async Task<bool> RefreshSessionAsync(string sessionId)
    {
        var session = GetSession(sessionId);
        if (session == null)
            return false;

        try
        {
            // Reload the solution/project
            Solution newSolution;
            if (session.SolutionPath.EndsWith(".sln", StringComparison.OrdinalIgnoreCase))
            {
                newSolution = await LoadSolutionAsync(session.SolutionPath);
            }
            else
            {
                newSolution = await LoadProjectAsync(session.SolutionPath);
            }
            
            session.Solution = newSolution;
            session.PendingChanges.Clear();
            
            await _cache.InvalidateAsync(sessionId);
            await _cache.PrewarmAsync(session);
            
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> EndSessionAsync(string sessionId)
    {
        if (_sessions.TryRemove(sessionId, out var session))
        {
            await _cache.InvalidateAsync(sessionId);
            // Dispose the workspace if it's an MSBuildWorkspace (not AdhocWorkspace from basic loading)
            if (session.Workspace is MSBuildWorkspace msbuildWorkspace)
            {
                msbuildWorkspace.Dispose();
            }
            else if (session.Workspace is AdhocWorkspace adhocWorkspace)
            {
                adhocWorkspace.Dispose();
            }
            return true;
        }
        return false;
    }

    public List<SessionInfo> GetActiveSessions()
    {
        return _sessions.Values.Select(s => new SessionInfo
        {
            Id = s.Id,
            SolutionPath = s.SolutionPath,
            CreatedAt = s.CreatedAt,
            LastAccessedAt = s.LastAccessedAt,
            ProjectCount = s.Solution.ProjectIds.Count,
            HasPendingChanges = s.HasPendingChanges,
            AnalyzerCount = s.Analyzers.Length,
            IsPaused = s.IsPaused,
            PausedAt = s.PausedAt
        }).ToList();
    }

    public async Task<Solution> GetSolutionWithChangesAsync(string sessionId, bool includePendingChanges = true)
    {
        var session = GetSession(sessionId);
        if (session == null)
            throw new ArgumentException($"Session not found: {sessionId}", nameof(sessionId));

        var solution = session.Solution;
        
        if (includePendingChanges && session.HasPendingChanges)
        {
            foreach (var (documentId, syntaxTree) in session.PendingChanges)
            {
                var document = solution.GetDocument(documentId);
                if (document != null)
                {
                    var root = await syntaxTree.GetRootAsync();
                    solution = solution.WithDocumentSyntaxRoot(documentId, root);
                }
            }
        }

        return solution;
    }

    private async Task<Solution> LoadSolutionAsync(string solutionPath)
    {
        try
        {
            // Check if MSBuildLocator has been registered
            if (!Microsoft.Build.Locator.MSBuildLocator.IsRegistered)
            {
                Console.WriteLine("[WSM DEBUG] MSBuildLocator not registered, falling back to basic loading");
                _logger.MSBuildLocatorNotRegisteredError();
                return await LoadProjectBasicAsync(solutionPath);
            }
            else
            {
                Console.WriteLine("[WSM DEBUG] MSBuildLocator is registered, proceeding with MSBuildWorkspace");
            }
            
            _logger.MSBuildLocatorStatus(Microsoft.Build.Locator.MSBuildLocator.IsRegistered);
            
            _logger.LoadingSolution(solutionPath);
            
            // Use MSBuildWorkspace with simplified properties
            var properties = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["Configuration"] = "Debug",
                ["Platform"] = "AnyCPU"
            };
            
            // Create MSBuildWorkspace with proper settings
            var workspace = MSBuildWorkspace.Create(properties);
            workspace.LoadMetadataForReferencedProjects = true;
            workspace.SkipUnrecognizedProjects = true;
            
            var diagnostics = new List<string>();
            
            // Register workspace diagnostics handler
            workspace.WorkspaceFailed += (sender, args) =>
            {
                var message = $"[ROSLYN-MCP] Workspace diagnostic [{args.Diagnostic.Kind}]: {args.Diagnostic.Message}";
                _logger.WorkspaceDiagnostic(args.Diagnostic.Kind.ToString(), args.Diagnostic.Message);
                diagnostics.Add(message);
            };
            
            // Check supported languages before loading
            var supportedLanguages = workspace.Services.SupportedLanguages;
            _logger.LogInformation("Supported languages: {Languages}", string.Join(", ", supportedLanguages));
            if (!supportedLanguages.Contains("C#", StringComparer.Ordinal))
            {
                _logger.LogWarning("C# language support not available in workspace");
                throw new InvalidOperationException("C# language support not available - check package references");
            }
            
            // Load the solution - this will resolve all project references
            _logger.OpeningSolutionAsync();
            Console.WriteLine($"[WSM DEBUG] About to call OpenSolutionAsync for: {solutionPath}");
            var solution = await workspace.OpenSolutionAsync(solutionPath);
            Console.WriteLine($"[WSM DEBUG] OpenSolutionAsync completed. Projects: {solution.ProjectIds.Count}");
            
            // Check workspace diagnostics collection as well
            foreach (var diagnostic in workspace.Diagnostics)
            {
                Console.WriteLine($"[WSM DEBUG] Workspace diagnostic: {diagnostic.Kind} - {diagnostic.Message}");
                _logger.WorkspaceDiagnostic(diagnostic.Kind.ToString(), diagnostic.Message);
            }
            
            if (diagnostics.Count > 0)
            {
                Console.WriteLine($"[WSM DEBUG] WorkspaceFailed diagnostics: {diagnostics.Count}");
                foreach (var diag in diagnostics)
                {
                    Console.WriteLine($"[WSM DEBUG] - {diag}");
                }
            }
            
            _logger.SolutionLoaded(solution.ProjectIds.Count, 0);
            
            foreach (var project in solution.Projects)
            {
                _logger.ProjectDetails(project.Name, project.Language);
                _logger.ProjectDocumentCount(project.DocumentIds.Count);
                _logger.ProjectReferenceCount(project.MetadataReferences.Count);
            }
            
            if (solution.ProjectIds.Count == 0 && diagnostics.Count > 0)
            {
                _logger.EmptySolutionLoaded();
            }
            
            // FIXED: Return the solution directly from MSBuildWorkspace - don't transfer!
            _logger.SolutionLoadedDirectly();
            return solution;
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("MSBuild"))
        {
            Console.WriteLine($"[WSM DEBUG] MSBuild error: {ex.Message}");
            _logger.MSBuildWorkspaceError(ex.Message);
            _logger.FallingBackToBasicLoading();
            return await LoadProjectBasicAsync(solutionPath);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[WSM DEBUG] Unexpected error: {ex.Message}");
            Console.WriteLine($"[WSM DEBUG] Stack trace: {ex.StackTrace}");
            _logger.UnexpectedSolutionLoadError(ex);
            _logger.FallingBackToBasicLoading();
            return await LoadProjectBasicAsync(solutionPath);
        }
    }
    
    private async Task<Solution> LoadProjectAsync(string projectPath)
    {
        try
        {
            // Check if MSBuildLocator has been registered
            if (!Microsoft.Build.Locator.MSBuildLocator.IsRegistered)
            {
                _logger.LogWarning("MSBuildLocator not registered, falling back to basic loading");
                return await LoadProjectBasicAsync(projectPath);
            }
            
            _logger.LogInformation("MSBuildLocator is registered, using MSBuildWorkspace for project loading");
            
            _logger.LoadingProjectWithMSBuild(projectPath);
            
            // Use MSBuildWorkspace with simplified properties
            var properties = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["Configuration"] = "Debug",
                ["Platform"] = "AnyCPU"
            };
            
            // Create MSBuildWorkspace with proper settings
            var workspace = MSBuildWorkspace.Create(properties);
            workspace.LoadMetadataForReferencedProjects = true;
            workspace.SkipUnrecognizedProjects = true;
            
            var diagnostics = new List<string>();
            
            // Register workspace diagnostics handler
            workspace.WorkspaceFailed += (sender, args) =>
            {
                var message = $"[ROSLYN-MCP] Workspace diagnostic [{args.Diagnostic.Kind}]: {args.Diagnostic.Message}";
                _logger.WorkspaceDiagnostic(args.Diagnostic.Kind.ToString(), args.Diagnostic.Message);
                diagnostics.Add(message);
            };
            
            // Check supported languages before loading
            var supportedLanguages = workspace.Services.SupportedLanguages;
            _logger.LogInformation("Supported languages: {Languages}", string.Join(", ", supportedLanguages));
            if (!supportedLanguages.Contains("C#", StringComparer.Ordinal))
            {
                _logger.LogWarning("C# language support not available in workspace");
                throw new InvalidOperationException("C# language support not available - check package references");
            }
            
            // Load the project - this will resolve all references from the .csproj
            _logger.OpeningProjectAsync();
            var project = await workspace.OpenProjectAsync(projectPath);
            
            // Check workspace diagnostics collection as well
            foreach (var diagnostic in workspace.Diagnostics)
            {
                _logger.WorkspaceDiagnostic(diagnostic.Kind.ToString(), diagnostic.Message);
            }
            
            _logger.ProjectLoaded(project.Name, project.Language);
            _logger.DocumentCount(project.DocumentIds.Count);
            _logger.ReferenceCount(project.MetadataReferences.Count);
            _logger.ProjectReferenceCount2(project.ProjectReferences.Count());
            
            if (project.DocumentIds.Count == 0 && diagnostics.Count > 0)
            {
                _logger.ProjectLoadedButNoDocuments();
            }
            
            // FIXED: Return the project's solution directly from MSBuildWorkspace - don't transfer!
            _logger.ProjectLoadedDirectly();
            return project.Solution;
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("MSBuild"))
        {
            _logger.MSBuildWorkspaceError(ex.Message);
            _logger.FallingBackToBasicLoading();
            return await LoadProjectBasicAsync(projectPath);
        }
        catch (Exception ex)
        {
            _logger.UnexpectedProjectLoadError(ex);
            _logger.FallingBackToBasicLoading();
            return await LoadProjectBasicAsync(projectPath);
        }
    }
    
    private async Task<Solution> LoadProjectBasicAsync(string projectPath)
    {
        _logger.LoadProjectBasicLoading(projectPath);
        
        // Basic loading without MSBuildWorkspace - limited reference resolution
        // Create a new AdhocWorkspace for basic loading
        var adhocWorkspace = new AdhocWorkspace();
        
        var solutionInfo = SolutionInfo.Create(
            SolutionId.CreateNewId(),
            VersionStamp.Create(),
            Path.GetFileNameWithoutExtension(projectPath));
        
        var solution = adhocWorkspace.AddSolution(solutionInfo);
        solution = await AddProjectToSolutionAsync(solution, projectPath);
        
        _logger.LoadProjectBasicSolutionProjects(solution.ProjectIds.Count);
        
        // Apply the solution back to the workspace
        if (adhocWorkspace.TryApplyChanges(solution))
        {
            _logger.LoadProjectBasicChangesApplied();
            var result = adhocWorkspace.CurrentSolution;
            _logger.LoadProjectBasicFinalProjects(result.ProjectIds.Count);
            return result;
        }
        else
        {
            _logger.LoadProjectBasicChangesFailedToApply();
            // Return the solution we built even if TryApplyChanges failed
            _logger.LoadProjectBasicFinalProjects(solution.ProjectIds.Count);
            return solution;
        }
    }
    
    private static async Task<Solution> AddProjectToSolutionAsync(Solution solution, string projectPath)
    {
        Console.WriteLine($"[AddProject DEBUG] Processing project: {projectPath}");
        var projectName = Path.GetFileNameWithoutExtension(projectPath);
        var projectId = ProjectId.CreateNewId();
        var projectDir = Path.GetDirectoryName(projectPath) ?? "";
        Console.WriteLine($"[AddProject DEBUG] Project dir: {projectDir}");
        
        // Create project with C# compilation options  
        var projectInfo = Microsoft.CodeAnalysis.ProjectInfo.Create(
            projectId,
            VersionStamp.Create(),
            projectName,
            projectName,
            LanguageNames.CSharp)
            .WithFilePath(projectPath)
            .WithCompilationOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
            .WithParseOptions(new CSharpParseOptions(LanguageVersion.Latest));
        
        solution = solution.AddProject(projectInfo);
        Console.WriteLine($"[AddProject DEBUG] Added project, solution now has {solution.ProjectIds.Count} projects");
        
        // Add all .cs files from the project directory
        var csFiles = Directory.GetFiles(projectDir, "*.cs", SearchOption.AllDirectories)
            .Where(f => !f.Contains("\\obj\\") && !f.Contains("\\bin\\"));
        
        Console.WriteLine($"[AddProject DEBUG] Found {csFiles.Count()} C# files");
        
        foreach (var csFile in csFiles)
        {
            var documentName = Path.GetFileName(csFile);
            var documentText = await File.ReadAllTextAsync(csFile);
            var sourceText = SourceText.From(documentText);
            
            solution = solution.AddDocument(
                DocumentId.CreateNewId(projectId),
                documentName,
                sourceText,
                filePath: csFile);
        }
        
        // Add comprehensive references
        var references = new List<MetadataReference>();
        
        // Add basic runtime references
        references.Add(MetadataReference.CreateFromFile(typeof(object).Assembly.Location));
        references.Add(MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location));
        references.Add(MetadataReference.CreateFromFile(Assembly.Load("System.Runtime").Location));
        
        // Add common System assemblies
        var systemAssemblies = new[]
        {
            "System.Collections",
            "System.Collections.Concurrent", 
            "System.Collections.Immutable",
            "System.ComponentModel",
            "System.ComponentModel.Primitives",
            "System.Console",
            "System.Diagnostics.Process",
            "System.IO.FileSystem",
            "System.Linq",
            "System.Linq.Expressions",
            "System.Memory",
            "System.Text.Json",
            "System.Text.RegularExpressions",
            "System.Threading",
            "System.Threading.Tasks",
            "System.Threading.Tasks.Extensions",
            "netstandard"
        };
        
        foreach (var assemblyName in systemAssemblies)
        {
            try
            {
                var assembly = Assembly.Load(assemblyName);
                references.Add(MetadataReference.CreateFromFile(assembly.Location));
            }
            catch
            {
                // Some assemblies might not be available
            }
        }
        
        // Add references from the current app domain (includes NuGet packages)
        var currentAssemblies = AppDomain.CurrentDomain.GetAssemblies()
            .Where(a => !a.IsDynamic && !string.IsNullOrEmpty(a.Location))
            .Select(a => MetadataReference.CreateFromFile(a.Location));
        references.AddRange(currentAssemblies);
        
        solution = solution.AddMetadataReferences(projectId, references.Distinct());
        
        return solution;
    }
    
    private void CleanupStaleSessionsCallback(object? state)
    {
        var cutoff = DateTime.UtcNow.AddHours(-6); // 6 hours of inactivity
        var staleSessions = _sessions
            .Where(kvp => kvp.Value.LastAccessedAt < cutoff)
            .ToList();

        foreach (var (sessionId, _) in staleSessions)
        {
            _ = Task.Run(() => EndSessionAsync(sessionId));
        }
    }

    public void Dispose()
    {
        _cleanupTimer?.Dispose();
        
        // Dispose all session workspaces
        foreach (var session in _sessions.Values)
        {
            session.Workspace?.Dispose();
        }
        
        _sessions.Clear();
    }
}

public sealed record SessionInfo
{
    public string Id { get; init; } = string.Empty;
    public string SolutionPath { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
    public DateTime LastAccessedAt { get; init; }
    public int ProjectCount { get; init; }
    public bool HasPendingChanges { get; init; }
    public int AnalyzerCount { get; init; }
    public bool IsPaused { get; init; }
    public DateTime? PausedAt { get; init; }
}