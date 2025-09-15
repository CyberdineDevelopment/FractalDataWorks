using Microsoft.CodeAnalysis.Text;
using ModelContextProtocol.Server;
using RoslynMcpServer.Services;
using System.ComponentModel;
using System.Text.Json;

namespace RoslynMcpServer.Tools;

[McpServerToolType]
public class VirtualEditTools
{
    private readonly VirtualEditService _editService;
    private readonly WorkspaceSessionManager _sessionManager;

    public VirtualEditTools(VirtualEditService editService, WorkspaceSessionManager sessionManager)
    {
        _editService = editService;
        _sessionManager = sessionManager;
    }

    [McpServerTool]
    [Description("Preview changes before committing")]
    public async Task<string> ApplyVirtualEditAsync(
        [Description("Session ID")] string sessionId,
        [Description("File path to edit")] string filePath,
        [Description("JSON array of text changes: [{\"start\": 0, \"length\": 5, \"newText\": \"replacement\"}]")] string textChangesJson)
    {
        try
        {
            var textChanges = JsonSerializer.Deserialize<TextChangeJson[]>(textChangesJson);
            if (textChanges == null)
                throw new ArgumentException("Invalid text changes JSON", nameof(textChangesJson));

            var roslynTextChanges = textChanges.Select(tc => new TextChange(
                new TextSpan(tc.Start, tc.Length),
                tc.NewText ?? string.Empty
            )).ToList();

            var edit = new Services.FileEdit
            {
                FilePath = filePath,
                Changes = roslynTextChanges
            };

            var preview = await _editService.ApplyVirtualEditAsync(sessionId, edit);

            return JsonSerializer.Serialize(new
            {
                success = true,
                sessionId,
                preview = new
                {
                    compilationSucceeded = preview.CompilationSucceeded,
                    compilationErrors = preview.CompilationErrors,
                    changes = preview.Changes.Select(c => new
                    {
                        filePath = c.FilePath,
                        type = c.Type.ToString(),
                        textChangeCount = c.TextChanges.Count
                    }),
                    diagnosticDiff = new
                    {
                        resolvedCount = preview.DiagnosticDiff.ResolvedCount,
                        introducedCount = preview.DiagnosticDiff.IntroducedCount,
                        unchangedCount = preview.DiagnosticDiff.UnchangedCount,
                        resolved = preview.DiagnosticDiff.Resolved.Take(10).Select(d => new
                        {
                            id = d.Id,
                            severity = d.Severity.ToString(),
                            message = d.Message,
                            file = Path.GetFileName(d.File),
                            line = d.Line
                        }),
                        introduced = preview.DiagnosticDiff.Introduced.Take(10).Select(d => new
                        {
                            id = d.Id,
                            severity = d.Severity.ToString(),
                            message = d.Message,
                            file = Path.GetFileName(d.File),
                            line = d.Line
                        })
                    }
                }
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
    [Description("CRITICAL for coordinated multi-file changes (renaming, refactoring). Ensures atomic changes that maintain compilation. Use for any change affecting >1 file. Shows net diagnostic improvement")]
    public async Task<string> ApplyMultipleVirtualEditsAsync(
        [Description("Session ID")] string sessionId,
        [Description("JSON array of file edits: [{\"filePath\": \"path\", \"changes\": [{\"start\": 0, \"length\": 5, \"newText\": \"text\"}]}]")] string fileEditsJson)
    {
        try
        {
            var fileEditRequests = JsonSerializer.Deserialize<FileEditRequest[]>(fileEditsJson);
            if (fileEditRequests == null)
                throw new ArgumentException("Invalid file edits JSON", nameof(fileEditsJson));

            var fileEdits = fileEditRequests.Select(fer => new Services.FileEdit
            {
                FilePath = fer.FilePath,
                Changes = fer.Changes.Select(tc => new TextChange(
                    new TextSpan(tc.Start, tc.Length),
                    tc.NewText ?? string.Empty
                )).ToList()
            }).ToList();

            var preview = await _editService.ApplyMultipleEditsAsync(sessionId, fileEdits);

            return JsonSerializer.Serialize(new
            {
                success = true,
                sessionId,
                preview = new
                {
                    compilationSucceeded = preview.CompilationSucceeded,
                    compilationErrors = preview.CompilationErrors,
                    fileCount = preview.Changes.Count,
                    changes = preview.Changes.Select(c => new
                    {
                        filePath = c.FilePath,
                        type = c.Type.ToString(),
                        textChangeCount = c.TextChanges.Count
                    }),
                    diagnosticDiff = new
                    {
                        resolvedCount = preview.DiagnosticDiff.ResolvedCount,
                        introducedCount = preview.DiagnosticDiff.IntroducedCount,
                        unchangedCount = preview.DiagnosticDiff.UnchangedCount,
                        netImprovement = preview.DiagnosticDiff.ResolvedCount - preview.DiagnosticDiff.IntroducedCount,
                        resolved = preview.DiagnosticDiff.Resolved.Take(5).Select(d => new
                        {
                            id = d.Id,
                            severity = d.Severity.ToString(),
                            message = d.Message.Length > 100 ? d.Message[..100] + "..." : d.Message,
                            file = Path.GetFileName(d.File),
                            line = d.Line
                        }),
                        introduced = preview.DiagnosticDiff.Introduced.Take(5).Select(d => new
                        {
                            id = d.Id,
                            severity = d.Severity.ToString(),
                            message = d.Message.Length > 100 ? d.Message[..100] + "..." : d.Message,
                            file = Path.GetFileName(d.File),
                            line = d.Line
                        })
                    }
                }
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
    [Description("Apply all pending virtual changes permanently to disk. ALWAYS verify build success before commit. Final step after successful preview. Cannot be undone after commit")]
    public async Task<string> CommitChangesAsync(
        [Description("Session ID")] string sessionId)
    {
        try
        {
            var result = await _editService.CommitChangesAsync(sessionId);

            return JsonSerializer.Serialize(new
            {
                success = result.Success,
                sessionId,
                modifiedFileCount = result.ModifiedFiles.Count,
                modifiedFiles = result.ModifiedFiles,
                errors = result.Errors,
                commitTime = result.CommitTime
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
    [Description("SAFETY NET: Discard all pending changes when virtual edits break compilation or produce unexpected results. Essential for experimental changes. Use when preview shows negative results")]
    public async Task<string> RollbackChangesAsync(
        [Description("Session ID")] string sessionId)
    {
        try
        {
            var success = await _editService.RollbackChangesAsync(sessionId);

            return JsonSerializer.Serialize(new
            {
                success,
                sessionId,
                message = success ? "All pending changes rolled back" : "Session not found or no changes to rollback"
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
    [Description("Review what changes are pending before commit/rollback. Use to understand scope of pending modifications or debug unexpected session state")]
    public string GetPendingChanges(
        [Description("Session ID")] string sessionId)
    {
        try
        {
            var pendingChanges = _editService.GetPendingChanges(sessionId);

            return JsonSerializer.Serialize(new
            {
                success = true,
                sessionId,
                pendingChangeCount = pendingChanges.Count,
                pendingChanges = pendingChanges.Select(pc => new
                {
                    documentId = pc.DocumentId,
                    filePath = pc.FilePath,
                    projectName = pc.ProjectName,
                    fileName = Path.GetFileName(pc.FilePath)
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
    [Description("Convenience tool for precise line/column-based edits. Automatically converts line positions to character offsets. Use for targeted small changes when you know exact location")]
    public async Task<string> ReplaceTextAsync(
        [Description("Session ID")] string sessionId,
        [Description("File path")] string filePath,
        [Description("Start line (1-based)")] int startLine,
        [Description("Start column (1-based)")] int startColumn,
        [Description("End line (1-based)")] int endLine,
        [Description("End column (1-based)")] int endColumn,
        [Description("Replacement text")] string newText)
    {
        try
        {
            // This is a helper tool that converts line/column positions to character positions
            // We need to get the document first to calculate positions
            var session = _sessionManager.GetSession(sessionId);
            if (session == null)
                throw new ArgumentException("Session not found", nameof(sessionId));

            var document = session.Solution.Projects
                .SelectMany(p => p.Documents)
                .FirstOrDefault(d => string.Equals(d.FilePath, filePath, StringComparison.OrdinalIgnoreCase));

            if (document == null)
                throw new FileNotFoundException($"Document not found: {filePath}");

            var text = await document.GetTextAsync();
            
            var startPosition = text.Lines[startLine - 1].Start + startColumn - 1;
            var endPosition = text.Lines[endLine - 1].Start + endColumn - 1;
            var length = endPosition - startPosition;

            var textChange = new TextChange(new TextSpan(startPosition, length), newText);

            var edit = new Services.FileEdit
            {
                FilePath = filePath,
                Changes = new List<TextChange> { textChange }
            };

            var preview = await _editService.ApplyVirtualEditAsync(sessionId, edit);

            return JsonSerializer.Serialize(new
            {
                success = true,
                sessionId,
                filePath,
                textReplaced = new
                {
                    startLine,
                    startColumn,
                    endLine,
                    endColumn,
                    oldText = text.GetSubText(new TextSpan(startPosition, length)).ToString(),
                    newText
                },
                preview = new
                {
                    compilationSucceeded = preview.CompilationSucceeded,
                    diagnosticDiff = new
                    {
                        resolvedCount = preview.DiagnosticDiff.ResolvedCount,
                        introducedCount = preview.DiagnosticDiff.IntroducedCount,
                        netImprovement = preview.DiagnosticDiff.ResolvedCount - preview.DiagnosticDiff.IntroducedCount
                    }
                }
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
}

internal sealed record FileEditRequest
{
    public string FilePath { get; init; } = string.Empty;
    public List<TextChangeJson> Changes { get; init; } = new();
}

internal sealed record TextChangeJson
{
    public int Start { get; init; }
    public int Length { get; init; }
    public string? NewText { get; init; }
}