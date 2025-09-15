using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using RoslynMcpServer.Models;
using System.Collections.Immutable;

namespace RoslynMcpServer.Services;

public sealed class VirtualEditService
{
    private readonly WorkspaceSessionManager _sessionManager;
    private readonly CompilationCacheService _cache;

    public VirtualEditService(WorkspaceSessionManager sessionManager, CompilationCacheService cache)
    {
        _sessionManager = sessionManager;
        _cache = cache;
    }

    public async Task<ChangePreview> ApplyVirtualEditAsync(string sessionId, FileEdit edit)
    {
        var session = _sessionManager.GetSession(sessionId);
        if (session == null)
            throw new ArgumentException($"Session not found: {sessionId}", nameof(sessionId));

        var document = FindDocument(session.Solution, edit.FilePath);
        if (document == null)
            throw new FileNotFoundException($"Document not found: {edit.FilePath}");

        var oldSyntaxTree = await document.GetSyntaxTreeAsync();
        if (oldSyntaxTree == null)
            throw new InvalidOperationException("Unable to get syntax tree for document");

        var oldText = await oldSyntaxTree.GetTextAsync();
        var newText = ApplyTextEdits(oldText, edit.Changes);
        var parseOptions = (CSharpParseOptions?)oldSyntaxTree.Options;
        var newSyntaxTree = CSharpSyntaxTree.ParseText(
            newText, 
            parseOptions ?? CSharpParseOptions.Default,
            path: document.FilePath ?? string.Empty);

        var diagnosticsBeforeChange = await GetAllDiagnosticsAsync(session);
        
        session.PendingChanges[document.Id] = newSyntaxTree;

        var diagnosticsAfterChange = await GetAllDiagnosticsWithChangesAsync(session);
        var diagnosticComparison = CompareDiagnostics(diagnosticsBeforeChange, diagnosticsAfterChange);

        var hasCompilationErrors = diagnosticsAfterChange
            .Any(d => d.Severity == DiagnosticSeverity.Error);

        return new ChangePreview
        {
            SessionId = sessionId,
            Changes = new List<FileChange>
            {
                new()
                {
                    FilePath = edit.FilePath,
                    Type = ChangeType.Modified,
                    OldContent = oldText.ToString(),
                    NewContent = newText.ToString(),
                    TextChanges = edit.Changes
                }
            },
            DiagnosticDiff = diagnosticComparison,
            CompilationSucceeded = !hasCompilationErrors,
            CompilationErrors = diagnosticsAfterChange
                .Where(d => d.Severity == DiagnosticSeverity.Error)
                .Select(d => d.Message)
                .ToList()
        };
    }

    public async Task<ChangePreview> ApplyMultipleEditsAsync(string sessionId, List<FileEdit> edits)
    {
        var session = _sessionManager.GetSession(sessionId);
        if (session == null)
            throw new ArgumentException($"Session not found: {sessionId}", nameof(sessionId));

        var diagnosticsBeforeChange = await GetAllDiagnosticsAsync(session);
        var changes = new List<FileChange>();

        foreach (var edit in edits)
        {
            var document = FindDocument(session.Solution, edit.FilePath);
            if (document == null)
                continue;

            var oldSyntaxTree = await document.GetSyntaxTreeAsync();
            if (oldSyntaxTree == null)
                continue;

            var oldText = await oldSyntaxTree.GetTextAsync();
            var newText = ApplyTextEdits(oldText, edit.Changes);
            var parseOptions = (CSharpParseOptions?)oldSyntaxTree.Options;
            var newSyntaxTree = CSharpSyntaxTree.ParseText(
                newText,
                parseOptions ?? CSharpParseOptions.Default,
                path: document.FilePath ?? string.Empty);

            session.PendingChanges[document.Id] = newSyntaxTree;

            changes.Add(new FileChange
            {
                FilePath = edit.FilePath,
                Type = ChangeType.Modified,
                OldContent = oldText.ToString(),
                NewContent = newText.ToString(),
                TextChanges = edit.Changes
            });
        }

        var diagnosticsAfterChange = await GetAllDiagnosticsWithChangesAsync(session);
        var diagnosticComparison = CompareDiagnostics(diagnosticsBeforeChange, diagnosticsAfterChange);

        var hasCompilationErrors = diagnosticsAfterChange
            .Any(d => d.Severity == DiagnosticSeverity.Error);

        return new ChangePreview
        {
            SessionId = sessionId,
            Changes = changes,
            DiagnosticDiff = diagnosticComparison,
            CompilationSucceeded = !hasCompilationErrors,
            CompilationErrors = diagnosticsAfterChange
                .Where(d => d.Severity == DiagnosticSeverity.Error)
                .Select(d => d.Message)
                .ToList()
        };
    }

    public async Task<CommitResult> CommitChangesAsync(string sessionId)
    {
        var session = _sessionManager.GetSession(sessionId);
        if (session == null)
            throw new ArgumentException($"Session not found: {sessionId}", nameof(sessionId));

        if (!session.HasPendingChanges)
            return new CommitResult { Success = true };

        var modifiedFiles = new List<string>();
        var errors = new List<string>();

        try
        {
            foreach (var (documentId, newSyntaxTree) in session.PendingChanges)
            {
                var document = session.Solution.GetDocument(documentId);
                if (document?.FilePath == null)
                    continue;

                try
                {
                    var newText = await newSyntaxTree.GetTextAsync();
                    await File.WriteAllTextAsync(document.FilePath, newText.ToString());
                    modifiedFiles.Add(document.FilePath);
                    
                    _cache.InvalidateProject(sessionId, document.Project.Id);
                }
                catch (Exception ex)
                {
                    errors.Add($"Failed to write {document.FilePath}: {ex.Message}");
                }
            }

            session.PendingChanges.Clear();
            
            await _sessionManager.RefreshSessionAsync(sessionId);

            return new CommitResult
            {
                Success = errors.Count == 0,
                ModifiedFiles = modifiedFiles,
                Errors = errors
            };
        }
        catch (Exception ex)
        {
            errors.Add($"Commit failed: {ex.Message}");
            return new CommitResult
            {
                Success = false,
                Errors = errors
            };
        }
    }

    public Task<bool> RollbackChangesAsync(string sessionId)
    {
        var session = _sessionManager.GetSession(sessionId);
        if (session == null)
            return Task.FromResult(false);

        session.PendingChanges.Clear();
        return Task.FromResult(true);
    }

    public List<PendingChangeInfo> GetPendingChanges(string sessionId)
    {
        var session = _sessionManager.GetSession(sessionId);
        if (session == null)
            return new List<PendingChangeInfo>();

        return session.PendingChanges.Select(kvp =>
        {
            var document = session.Solution.GetDocument(kvp.Key);
            return new PendingChangeInfo
            {
                DocumentId = kvp.Key.ToString(),
                FilePath = document?.FilePath ?? "Unknown",
                ProjectName = document?.Project.Name ?? "Unknown"
            };
        }).ToList();
    }

    private static Document? FindDocument(Solution solution, string filePath)
    {
        return solution.Projects
            .SelectMany(p => p.Documents)
            .FirstOrDefault(d => string.Equals(d.FilePath, filePath, StringComparison.OrdinalIgnoreCase));
    }

    private static SourceText ApplyTextEdits(SourceText oldText, List<Microsoft.CodeAnalysis.Text.TextChange> changes)
    {
        if (!changes.Any())
            return oldText;

        var sortedChanges = changes.OrderByDescending(c => c.Span.Start);
        var newText = oldText;
        
        foreach (var change in sortedChanges)
        {
            newText = newText.WithChanges(change);
        }

        return newText;
    }

    private async Task<List<DiagnosticInfo>> GetAllDiagnosticsAsync(CompilationSession session)
    {
        var allDiagnostics = new List<DiagnosticInfo>();

        foreach (var project in session.Solution.Projects)
        {
            var diagnostics = await _cache.GetDiagnosticsAsync(
                session.Id, 
                project, 
                session.Analyzers, 
                includeAnalyzerDiagnostics: true);

            allDiagnostics.AddRange(diagnostics.Select(d => CreateDiagnosticInfo(d, project.Name)));
        }

        return allDiagnostics;
    }

    private async Task<List<DiagnosticInfo>> GetAllDiagnosticsWithChangesAsync(CompilationSession session)
    {
        var allDiagnostics = new List<DiagnosticInfo>();

        foreach (var project in session.Solution.Projects)
        {
            var compilation = await _cache.GetCompilationWithChangesAsync(
                session.Id, 
                project, 
                session.PendingChanges);

            var diagnostics = compilation.GetDiagnostics();
            
            if (!session.Analyzers.IsEmpty)
            {
                var analyzerDiagnostics = await compilation
                    .WithAnalyzers(session.Analyzers)
                    .GetAnalyzerDiagnosticsAsync();
                
                diagnostics = diagnostics.AddRange(analyzerDiagnostics);
            }

            allDiagnostics.AddRange(diagnostics.Select(d => CreateDiagnosticInfo(d, project.Name)));
        }

        return allDiagnostics;
    }

    private static DiagnosticComparison CompareDiagnostics(
        List<DiagnosticInfo> before, 
        List<DiagnosticInfo> after)
    {
        var beforeSet = before.ToHashSet(new DiagnosticInfoComparer());
        var afterSet = after.ToHashSet(new DiagnosticInfoComparer());

        var resolved = beforeSet.Except(afterSet, new DiagnosticInfoComparer()).ToList();
        var introduced = afterSet.Except(beforeSet, new DiagnosticInfoComparer()).ToList();
        var unchanged = beforeSet.Intersect(afterSet, new DiagnosticInfoComparer()).ToList();

        return new DiagnosticComparison
        {
            Resolved = resolved,
            Introduced = introduced,
            Unchanged = unchanged
        };
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
            Category = diagnostic.Descriptor.Category
        };
    }
}

public sealed record FileEdit
{
    public string FilePath { get; init; } = string.Empty;
    public List<Microsoft.CodeAnalysis.Text.TextChange> Changes { get; init; } = new();
}

public sealed record PendingChangeInfo
{
    public string DocumentId { get; init; } = string.Empty;
    public string FilePath { get; init; } = string.Empty;
    public string ProjectName { get; init; } = string.Empty;
}

internal sealed class DiagnosticInfoComparer : IEqualityComparer<DiagnosticInfo>
{
    public bool Equals(DiagnosticInfo? x, DiagnosticInfo? y)
    {
        if (ReferenceEquals(x, y)) return true;
        if (x == null || y == null) return false;
        
        return x.Id == y.Id &&
               x.File == y.File &&
               x.Line == y.Line &&
               x.Column == y.Column &&
               x.Message == y.Message;
    }

    public int GetHashCode(DiagnosticInfo obj)
    {
        return HashCode.Combine(obj.Id, obj.File, obj.Line, obj.Column, obj.Message);
    }
}