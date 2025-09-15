using Microsoft.CodeAnalysis;
using ModelContextProtocol.Server;
using RoslynMcpServer.Models;
using RoslynMcpServer.Services;
using System.ComponentModel;
using System.Text.Json;

namespace RoslynMcpServer.Tools;

[McpServerToolType]
public class DiagnosticTools
{
    private readonly WorkspaceSessionManager _sessionManager;
    private readonly CompilationCacheService _cache;

    public DiagnosticTools(
        WorkspaceSessionManager sessionManager,
        CompilationCacheService cache)
    {
        _sessionManager = sessionManager;
        _cache = cache;
    }

    [McpServerTool]
    [Description("Get diagnostic summary by severity and project")]
    public async Task<DiagnosticSummaryResponse> GetDiagnosticSummaryAsync(
        [Description("Session ID")] string sessionId,
        [Description("Include analyzer diagnostics")] bool includeAnalyzerDiagnostics = true)
    {
        try
        {
            var session = _sessionManager.GetSession(sessionId);
            if (session == null)
                return new DiagnosticSummaryResponse { Success = false, Error = "Session not found", SessionId = sessionId };

            var allDiagnostics = new List<DiagnosticInfo>();

            foreach (var project in session.Solution.Projects)
            {
                var diagnostics = await _cache.GetDiagnosticsAsync(
                    sessionId,
                    project,
                    session.Analyzers,
                    includeAnalyzerDiagnostics);

                allDiagnostics.AddRange(diagnostics.Select(d => CreateDiagnosticInfoWithRelativePath(d, project.Name, session.Solution.FilePath)));
            }

            var summary = CreateDiagnosticSummary(allDiagnostics);

            return new DiagnosticSummaryResponse
            {
                SessionId = sessionId,
                Summary = summary
            };
        }
        catch (Exception ex)
        {
            return new DiagnosticSummaryResponse
            {
                Success = false,
                Error = ex.Message,
                SessionId = sessionId
            };
        }
    }

    [McpServerTool]
    [Description("Get diagnostic overview with counts and top issues")]
    public async Task<DiagnosticOverviewResponse> GetDiagnosticsAsync(
        [Description("Session ID")] string sessionId,
        [Description("Filter by severity (Error, Warning, Info, Hidden)")] string? severity = null,
        [Description("Filter by project name")] string? project = null,
        [Description("Filter by diagnostic rule ID")] string? ruleId = null,
        [Description("Filter by file path")] string? filePath = null,
        [Description("Include analyzer diagnostics")] bool includeAnalyzerDiagnostics = true)
    {
        try
        {
            var session = _sessionManager.GetSession(sessionId);
            if (session == null)
                throw new ArgumentException("Session not found", nameof(sessionId));

            var allDiagnostics = new List<DiagnosticInfo>();

            var projectsToProcess = string.IsNullOrEmpty(project) 
                ? session.Solution.Projects 
                : session.Solution.Projects.Where(p => p.Name.Contains(project, StringComparison.OrdinalIgnoreCase));

            foreach (var proj in projectsToProcess)
            {
                var diagnostics = await _cache.GetDiagnosticsAsync(
                    sessionId, 
                    proj, 
                    session.Analyzers, 
                    includeAnalyzerDiagnostics);

                allDiagnostics.AddRange(diagnostics.Select(d => CreateDiagnosticInfoWithRelativePath(d, proj.Name, session.Solution.FilePath)));
            }

            var filteredDiagnostics = FilterDiagnostics(allDiagnostics, severity, ruleId, filePath);
            
            var errorCount = filteredDiagnostics.Count(d => d.Severity == DiagnosticSeverity.Error);
            var warningCount = filteredDiagnostics.Count(d => d.Severity == DiagnosticSeverity.Warning);
            
            var topErrorRules = filteredDiagnostics
                .Where(d => d.Severity == DiagnosticSeverity.Error)
                .GroupBy(d => d.Id, StringComparer.Ordinal)
                .OrderByDescending(g => g.Count())
                .Take(3)
                .Select(g => $"{g.Key} ({g.Count()})").ToList();
                
            var topWarningRules = filteredDiagnostics
                .Where(d => d.Severity == DiagnosticSeverity.Warning)
                .GroupBy(d => d.Id, StringComparer.Ordinal)
                .OrderByDescending(g => g.Count())
                .Take(3)
                .Select(g => $"{g.Key} ({g.Count()})").ToList();
                
            var affectedProjects = filteredDiagnostics
                .GroupBy(d => d.Project, StringComparer.Ordinal)
                .OrderByDescending(g => g.Count())
                .Take(5)
                .Select(g => $"{g.Key} ({g.Count()})").ToList();

            return new DiagnosticOverviewResponse
            {
                SessionId = sessionId,
                ErrorCount = errorCount,
                WarningCount = warningCount,
                TotalCount = filteredDiagnostics.Count,
                TopErrorRules = topErrorRules,
                TopWarningRules = topWarningRules,
                AffectedProjects = affectedProjects
            };
        }
        catch (Exception ex)
        {
            return new DiagnosticOverviewResponse
            {
                Success = false,
                Error = ex.Message,
                SessionId = sessionId
            };
        }
    }

    [McpServerTool]
    [Description("Get detailed diagnostic list with filtering and pagination")]
    public async Task<DiagnosticListResponse> GetDiagnosticDetailsAsync(
        [Description("Session ID")] string sessionId,
        [Description("Filter by severity (Error, Warning, Info, Hidden)")] string? severity = null,
        [Description("Filter by project name")] string? project = null,
        [Description("Filter by diagnostic rule ID")] string? ruleId = null,
        [Description("Filter by file path")] string? filePath = null,
        [Description("Maximum number of results")] int maxResults = 50,
        [Description("Include analyzer diagnostics")] bool includeAnalyzerDiagnostics = true,
        [Description("Pagination cursor for large result sets")] string? cursor = null)
    {
        try
        {
            var session = _sessionManager.GetSession(sessionId);
            if (session == null)
                return new DiagnosticListResponse { Success = false, Error = "Session not found", SessionId = sessionId };

            var allDiagnostics = new List<DiagnosticInfo>();

            var projectsToProcess = string.IsNullOrEmpty(project) 
                ? session.Solution.Projects 
                : session.Solution.Projects.Where(p => p.Name.Contains(project, StringComparison.OrdinalIgnoreCase));

            foreach (var proj in projectsToProcess)
            {
                var diagnostics = await _cache.GetDiagnosticsAsync(
                    sessionId, 
                    proj, 
                    session.Analyzers, 
                    includeAnalyzerDiagnostics);

                allDiagnostics.AddRange(diagnostics.Select(d => CreateDiagnosticInfoWithRelativePath(d, proj.Name, session.Solution.FilePath)));
            }

            var filteredDiagnostics = FilterDiagnostics(allDiagnostics, severity, ruleId, filePath);
            
            var skipCount = 0;
            if (!string.IsNullOrEmpty(cursor) && int.TryParse(cursor, out var parsedCursor))
            {
                skipCount = parsedCursor;
            }
            
            var pagedDiagnostics = filteredDiagnostics.Skip(skipCount).Take(maxResults).ToList();
            var hasMore = filteredDiagnostics.Count > skipCount + maxResults;
            var nextCursor = hasMore ? (skipCount + maxResults).ToString() : null;

            return new DiagnosticListResponse
            {
                SessionId = sessionId,
                Diagnostics = pagedDiagnostics,
                TotalCount = filteredDiagnostics.Count,
                HasMore = hasMore,
                NextCursor = nextCursor
            };
        }
        catch (Exception ex)
        {
            return new DiagnosticListResponse
            {
                Success = false,
                Error = ex.Message,
                SessionId = sessionId
            };
        }
    }

    [McpServerTool]
    [Description("Get diagnostics for specific file")]
    public async Task<string> GetFileDiagnosticsAsync(
        [Description("Session ID")] string sessionId,
        [Description("File path to analyze")] string filePath,
        [Description("Include analyzer diagnostics")] bool includeAnalyzerDiagnostics = true)
    {
        try
        {
            var session = _sessionManager.GetSession(sessionId);
            if (session == null)
                throw new ArgumentException("Session not found", nameof(sessionId));

            var document = session.Solution.Projects
                .SelectMany(p => p.Documents)
                .FirstOrDefault(d => string.Equals(d.FilePath, filePath, StringComparison.OrdinalIgnoreCase));

            if (document == null)
                throw new FileNotFoundException($"File not found in solution: {filePath}");

            var diagnostics = await _cache.GetDiagnosticsAsync(
                sessionId, 
                document.Project, 
                session.Analyzers, 
                includeAnalyzerDiagnostics);

            var fileDiagnostics = diagnostics
                .Where(d => string.Equals(d.Location.SourceTree?.FilePath, filePath, StringComparison.OrdinalIgnoreCase))
                .Select(d => CreateDiagnosticInfo(d, document.Project.Name))
                .OrderBy(d => d.Line)
                .ThenBy(d => d.Column)
                .ToList();

            return JsonSerializer.Serialize(new
            {
                success = true,
                sessionId,
                filePath,
                diagnosticCount = fileDiagnostics.Count,
                diagnostics = fileDiagnostics.Select(d => new
                {
                    id = d.Id,
                    severity = d.Severity.ToString(),
                    message = d.Message,
                    line = d.Line,
                    column = d.Column,
                    category = d.Category,
                    hasCodeFix = d.HasCodeFix
                })
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
    [Description("Get active analyzer information")]
    public string GetAnalyzerInfo(
        [Description("Session ID")] string sessionId)
    {
        try
        {
            var session = _sessionManager.GetSession(sessionId);
            if (session == null)
                throw new ArgumentException("Session not found", nameof(sessionId));

            var analyzerInfo = AnalyzerService.GetAnalyzerInfo(session.Analyzers);

            return JsonSerializer.Serialize(new
            {
                success = true,
                sessionId,
                analyzerCount = session.Analyzers.Length,
                analyzers = analyzerInfo.Select(a => new
                {
                    name = a.Name,
                    assemblyName = a.AssemblyName,
                    ruleCount = a.SupportedDiagnostics.Count,
                    rules = a.SupportedDiagnostics.Select(r => new
                    {
                        id = r.Id,
                        title = r.Title,
                        category = r.Category,
                        severity = r.Severity.ToString(),
                        description = r.Description.Length > 200 ? r.Description[..200] + "..." : r.Description,
                        helpLink = r.HelpLinkUri
                    }).Take(10) // Limit rules per analyzer for readability
                })
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
    [Description("Check if diagnostic has auto-fix available")]
    public static async Task<string> CheckCodeFixAvailabilityAsync(
        [Description("Diagnostic rule ID to check")] string diagnosticId)
    {
        try
        {
            var hasCodeFix = await AnalyzerService.HasCodeFixProviderAsync(diagnosticId);

            return JsonSerializer.Serialize(new
            {
                success = true,
                diagnosticId,
                hasCodeFix,
                message = hasCodeFix ? "Code fix may be available" : "No known code fix available"
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

    private static DiagnosticInfo CreateDiagnosticInfo(Diagnostic diagnostic, string projectName)
    {
        var location = diagnostic.Location;
        var linePosition = location.GetLineSpan().StartLinePosition;

        return new DiagnosticInfo
        {
            Id = diagnostic.Id,
            Severity = diagnostic.Severity,
            Message = diagnostic.GetMessage(),
            File = location.SourceTree?.FilePath ?? string.Empty,
            Line = linePosition.Line + 1,
            Column = linePosition.Character + 1,
            Project = projectName,
            Category = diagnostic.Descriptor.Category,
            HasCodeFix = IsKnownCodeFixRule(diagnostic.Id)
        };
    }

    private static DiagnosticInfo CreateDiagnosticInfoWithRelativePath(Diagnostic diagnostic, string projectName, string? solutionFilePath)
    {
        var location = diagnostic.Location;
        var linePosition = location.GetLineSpan().StartLinePosition;
        var filePath = location.SourceTree?.FilePath ?? string.Empty;
        
        // Make path relative to solution directory
        if (!string.IsNullOrEmpty(solutionFilePath) && !string.IsNullOrEmpty(filePath))
        {
            var solutionDir = Path.GetDirectoryName(solutionFilePath);
            if (!string.IsNullOrEmpty(solutionDir) && filePath.StartsWith(solutionDir, StringComparison.OrdinalIgnoreCase))
            {
                filePath = Path.GetRelativePath(solutionDir, filePath);
            }
        }

        return new DiagnosticInfo
        {
            Id = diagnostic.Id,
            Severity = diagnostic.Severity,
            Message = diagnostic.GetMessage(),
            File = filePath,
            Line = linePosition.Line + 1,
            Column = linePosition.Character + 1,
            Project = projectName,
            Category = diagnostic.Descriptor.Category,
            HasCodeFix = IsKnownCodeFixRule(diagnostic.Id)
        };
    }

    private static bool IsKnownCodeFixRule(string diagnosticId)
    {
        return diagnosticId.StartsWith("CS") ||    // Compiler diagnostics
               diagnosticId.StartsWith("CA") ||    // Code Analysis
               diagnosticId.StartsWith("IDE") ||   // IDE suggestions
               diagnosticId.StartsWith("SA");      // StyleCop
    }

    private static DiagnosticSummary CreateDiagnosticSummary(List<DiagnosticInfo> diagnostics)
    {
        var bySeverity = diagnostics
            .GroupBy(d => d.Severity)
            .ToDictionary(g => g.Key, g => g.Count());

        var byRule = diagnostics
            .GroupBy(d => d.Id, StringComparer.Ordinal)
            .ToDictionary(g => g.Key, g => g.Count(), StringComparer.Ordinal)
            .OrderByDescending(kvp => kvp.Value)
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value, StringComparer.Ordinal);

        var byProject = diagnostics
            .Where(d => !string.IsNullOrEmpty(d.Project))
            .GroupBy(d => d.Project, StringComparer.Ordinal)
            .ToDictionary(g => g.Key, g => g.Count(), StringComparer.Ordinal);

        var byFile = diagnostics
            .Where(d => !string.IsNullOrEmpty(d.File))
            .GroupBy(d => Path.GetFileName(d.File), StringComparer.Ordinal)
            .ToDictionary(g => g.Key, g => g.Count(), StringComparer.Ordinal)
            .OrderByDescending(kvp => kvp.Value)
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value, StringComparer.Ordinal);

        return new DiagnosticSummary
        {
            TotalCount = diagnostics.Count,
            ErrorCount = bySeverity.GetValueOrDefault(DiagnosticSeverity.Error, 0),
            WarningCount = bySeverity.GetValueOrDefault(DiagnosticSeverity.Warning, 0),
            InfoCount = bySeverity.GetValueOrDefault(DiagnosticSeverity.Info, 0),
            HiddenCount = bySeverity.GetValueOrDefault(DiagnosticSeverity.Hidden, 0),
            BySeverity = bySeverity,
            ByRule = byRule,
            ByProject = byProject,
            ByFile = byFile
        };
    }

    private static List<DiagnosticInfo> FilterDiagnostics(
        List<DiagnosticInfo> diagnostics,
        string? severity,
        string? ruleId,
        string? filePath)
    {
        var filtered = diagnostics.AsEnumerable();

        if (!string.IsNullOrEmpty(severity) && 
            Enum.TryParse<DiagnosticSeverity>(severity, true, out var severityEnum))
        {
            filtered = filtered.Where(d => d.Severity == severityEnum);
        }

        if (!string.IsNullOrEmpty(ruleId))
        {
            filtered = filtered.Where(d => d.Id.Contains(ruleId, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrEmpty(filePath))
        {
            filtered = filtered.Where(d => d.File.Contains(filePath, StringComparison.OrdinalIgnoreCase));
        }

        return filtered.ToList();
    }
}