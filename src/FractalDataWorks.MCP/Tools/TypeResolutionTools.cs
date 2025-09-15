using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using RoslynMcpServer.Logging;
using RoslynMcpServer.Models;
using RoslynMcpServer.Services;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Text;

namespace RoslynMcpServer.Tools;

[McpServerToolType]
public sealed class TypeResolutionTools
{
    private readonly WorkspaceSessionManager _sessionManager;
    private readonly VirtualEditService _editService;
    private readonly ILogger<TypeResolutionTools> _logger;

    public TypeResolutionTools(WorkspaceSessionManager sessionManager, VirtualEditService editService, ILogger<TypeResolutionTools> logger)
    {
        _sessionManager = sessionManager;
        _editService = editService;
        _logger = logger;
    }

    [McpServerTool]
    [Description("Find missing type errors (CS0246) and group by type name with file locations")]
    public async Task<object> FindMissingTypesAsync(
        [Description("The session ID")] string sessionId,
        [Description("Optional project name filter")] string? projectFilter = null)
    {
        try
        {
            ToolLogMessages.MissingTypeAnalysisStarted(_logger, sessionId);

            var session = _sessionManager.GetSession(sessionId);
            if (session?.Solution == null)
            {
                return new { success = false, error = "Session not found or no solution loaded" };
            }

            var missingTypesByName = new ConcurrentDictionary<string, List<MissingTypeLocation>>(StringComparer.Ordinal);
            var solution = session.Solution;

            foreach (var project in solution.Projects)
            {
                if (!string.IsNullOrEmpty(projectFilter) && 
                    !project.Name.Contains(projectFilter, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var compilation = await project.GetCompilationAsync();
                if (compilation == null) continue;

                var diagnostics = compilation.GetDiagnostics()
                    .Where(d => d.Id == "CS0246") // Type or namespace name could not be found
                    .ToList();

                foreach (var diagnostic in diagnostics)
                {
                    var location = diagnostic.Location;
                    if (location?.SourceTree?.FilePath == null) continue;

                    var sourceText = await location.SourceTree.GetTextAsync();
                    var line = sourceText.Lines[location.GetLineSpan().StartLinePosition.Line];
                    var lineText = line.ToString();

                    // Extract the missing type name from the diagnostic message
                    var typeName = ExtractTypeNameFromDiagnostic(diagnostic.GetMessage());
                    if (string.IsNullOrEmpty(typeName)) continue;

                    var missingLocation = new MissingTypeLocation
                    {
                        ProjectName = project.Name,
                        FilePath = location.SourceTree.FilePath,
                        LineNumber = location.GetLineSpan().StartLinePosition.Line + 1,
                        ColumnNumber = location.GetLineSpan().StartLinePosition.Character + 1,
                        LineText = lineText.Trim(),
                        DiagnosticMessage = diagnostic.GetMessage()
                    };

                    missingTypesByName.AddOrUpdate(
                        typeName,
                        new List<MissingTypeLocation> { missingLocation },
                        (key, existing) =>
                        {
                            existing.Add(missingLocation);
                            return existing;
                        });
                }
            }

            var result = new MissingTypesAnalysis
            {
                SessionId = sessionId,
                TotalMissingTypes = missingTypesByName.Count,
                TotalOccurrences = missingTypesByName.Values.Sum(list => list.Count),
                MissingTypesByName = missingTypesByName.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value.OrderBy(l => l.ProjectName, StringComparer.Ordinal)
                                   .ThenBy(l => l.FilePath, StringComparer.Ordinal)
                                   .ThenBy(l => l.LineNumber)
                                   .ToList(),
                    StringComparer.Ordinal),
                AnalyzedAt = DateTime.UtcNow
            };

            ToolLogMessages.MissingTypeAnalysisCompleted(_logger, sessionId, result.TotalMissingTypes, result.TotalOccurrences);

            return new { success = true, data = result };
        }
        catch (Exception ex)
        {
            ToolLogMessages.MissingTypeAnalysisFailed(_logger, sessionId, ex);
            return new { success = false, error = ex.Message };
        }
    }

    [McpServerTool]
    [Description("Suggest using statements for missing types based on available namespaces")]
    public async Task<object> SuggestUsingStatementsAsync(
        [Description("The session ID")] string sessionId,
        [Description("The name of the missing type")] string missingTypeName,
        [Description("Path to a file where the type is missing for context")] string? contextFilePath = null)
    {
        try
        {
            ToolLogMessages.UsingSuggestionStarted(_logger, sessionId, missingTypeName);

            var session = _sessionManager.GetSession(sessionId);
            if (session?.Solution == null)
            {
                return new { success = false, error = "Session not found or no solution loaded" };
            }

            var suggestions = new List<UsingStatementSuggestion>();
            var solution = session.Solution;

            // Search for the type in all projects and their references
            foreach (var project in solution.Projects)
            {
                var compilation = await project.GetCompilationAsync();
                if (compilation == null) continue;

                // Search in project assemblies and referenced assemblies
                var allTypes = GetAllAvailableTypes(compilation);
                
                var matchingTypes = allTypes
                    .Where(t => t.Name == missingTypeName || t.Name.EndsWith($".{missingTypeName}"))
                    .GroupBy(t => t.ContainingNamespace?.ToDisplayString() ?? "", StringComparer.Ordinal)
                    .ToList();

                foreach (var group in matchingTypes)
                {
                    var namespaceName = group.Key;
                    if (string.IsNullOrEmpty(namespaceName)) continue;

                    var types = group.ToList();
                    var suggestion = new UsingStatementSuggestion
                    {
                        NamespaceName = namespaceName,
                        TypeName = missingTypeName,
                        FullTypeName = types[0].ToDisplayString(),
                        AssemblyName = types[0].ContainingAssembly?.Name ?? "Unknown",
                        IsSystemType = namespaceName.StartsWith("System"),
                        Confidence = CalculateConfidence(missingTypeName, namespaceName, types.Count)
                    };

                    if (!suggestions.Any(s => s.NamespaceName == namespaceName))
                    {
                        suggestions.Add(suggestion);
                    }
                }
            }

            // Sort by confidence and common patterns
            suggestions = suggestions
                .OrderByDescending(s => s.Confidence)
                .ThenBy(s => s.IsSystemType ? 0 : 1) // Prefer System types
                .ThenBy(s => s.NamespaceName, StringComparer.Ordinal)
                .ToList();

            ToolLogMessages.UsingSuggestionCompleted(_logger, sessionId, missingTypeName, suggestions.Count);

            return new { success = true, data = new { missingTypeName, suggestions } };
        }
        catch (Exception ex)
        {
            ToolLogMessages.UsingSuggestionFailed(_logger, sessionId, missingTypeName, ex);
            return new { success = false, error = ex.Message };
        }
    }

    [McpServerTool]
    [Description("Add using statements to multiple files for missing types")]
    public async Task<object> BulkAddUsingStatementsAsync(
        [Description("The session ID")] string sessionId,
        [Description("Array of using statement fixes with typeName, namespaceName, and filePaths")] UsingStatementFix[] usingFixes)
    {
        try
        {
            ToolLogMessages.BulkUsingAdditionStarted(_logger, sessionId, usingFixes.Length);

            var session = _sessionManager.GetSession(sessionId);
            if (session?.Solution == null)
            {
                return new { success = false, error = "Session not found or no solution loaded" };
            }

            var results = new List<UsingAdditionResult>();
            var processedFiles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var fix in usingFixes)
            {
                foreach (var filePath in fix.FilePaths)
                {
                    try
                    {
                        // Skip if we've already processed this file in this batch
                        if (processedFiles.Contains(filePath))
                        {
                            results.Add(new UsingAdditionResult
                            {
                                FilePath = filePath,
                                TypeName = fix.TypeName,
                                NamespaceName = fix.NamespaceName,
                                Success = true,
                                Message = "Already processed in this batch"
                            });
                            continue;
                        }

                        var document = session.Solution.Projects
                            .SelectMany(p => p.Documents)
                            .FirstOrDefault(d => d.FilePath != null && 
                                d.FilePath.Equals(filePath, StringComparison.OrdinalIgnoreCase));

                        if (document == null)
                        {
                            results.Add(new UsingAdditionResult
                            {
                                FilePath = filePath,
                                TypeName = fix.TypeName,
                                NamespaceName = fix.NamespaceName,
                                Success = false,
                                Message = "Document not found in solution"
                            });
                            continue;
                        }

                        var sourceText = await document.GetTextAsync();
                        var syntaxTree = await document.GetSyntaxTreeAsync();
                        if (syntaxTree == null)
                        {
                            results.Add(new UsingAdditionResult
                            {
                                FilePath = filePath,
                                TypeName = fix.TypeName,
                                NamespaceName = fix.NamespaceName,
                                Success = false,
                                Message = "Unable to get syntax tree"
                            });
                            continue;
                        }

                        var root = await syntaxTree.GetRootAsync() as CompilationUnitSyntax;

                        if (root == null)
                        {
                            results.Add(new UsingAdditionResult
                            {
                                FilePath = filePath,
                                TypeName = fix.TypeName,
                                NamespaceName = fix.NamespaceName,
                                Success = false,
                                Message = "Unable to parse syntax tree"
                            });
                            continue;
                        }

                        // Check if using statement already exists
                        var existingUsings = root.Usings
                            .Select(u => u.Name?.ToString())
                            .Where(n => !string.IsNullOrEmpty(n))
                            .ToHashSet(StringComparer.Ordinal);

                        if (existingUsings.Contains(fix.NamespaceName))
                        {
                            results.Add(new UsingAdditionResult
                            {
                                FilePath = filePath,
                                TypeName = fix.TypeName,
                                NamespaceName = fix.NamespaceName,
                                Success = true,
                                Message = "Using statement already exists"
                            });
                            continue;
                        }

                        // Add the using statement
                        var newUsing = SyntaxFactory.UsingDirective(
                            SyntaxFactory.ParseName(fix.NamespaceName)
                        ).WithTrailingTrivia(SyntaxFactory.EndOfLine("\n"));

                        var newRoot = root.AddUsings(newUsing);
                        
                        // Sort using statements
                        newRoot = newRoot.WithUsings(
                            SyntaxFactory.List(newRoot.Usings.OrderBy(u => u.Name?.ToString(), StringComparer.Ordinal))
                        );

                        var newSourceText = newRoot.GetText();

                        // Apply as virtual edit
                        var edit = new Services.FileEdit
                        {
                            FilePath = filePath,
                            Changes = new List<Microsoft.CodeAnalysis.Text.TextChange>
                            {
                                new Microsoft.CodeAnalysis.Text.TextChange(
                                    new Microsoft.CodeAnalysis.Text.TextSpan(0, sourceText.Length),
                                    newSourceText.ToString()
                                )
                            }
                        };

                        var changePreview = await _editService.ApplyVirtualEditAsync(sessionId, edit);

                        if (changePreview.CompilationSucceeded)
                        {
                            results.Add(new UsingAdditionResult
                            {
                                FilePath = filePath,
                                TypeName = fix.TypeName,
                                NamespaceName = fix.NamespaceName,
                                Success = true,
                                Message = "Using statement added successfully"
                            });
                            processedFiles.Add(filePath);
                        }
                        else
                        {
                            results.Add(new UsingAdditionResult
                            {
                                FilePath = filePath,
                                TypeName = fix.TypeName,
                                NamespaceName = fix.NamespaceName,
                                Success = false,
                                Message = "Compilation failed after adding using statement"
                            });
                        }
                    }
                    catch (Exception fileEx)
                    {
                        results.Add(new UsingAdditionResult
                        {
                            FilePath = filePath,
                            TypeName = fix.TypeName,
                            NamespaceName = fix.NamespaceName,
                            Success = false,
                            Message = $"Error processing file: {fileEx.Message}"
                        });
                    }
                }
            }

            var successCount = results.Count(r => r.Success);
            var totalCount = results.Count;

            ToolLogMessages.BulkUsingAdditionCompleted(_logger, sessionId, successCount, totalCount);

            return new 
            { 
                success = true, 
                data = new 
                { 
                    totalFiles = totalCount,
                    successfulFiles = successCount,
                    failedFiles = totalCount - successCount,
                    results = results.OrderBy(r => r.FilePath, StringComparer.Ordinal).ToList()
                }
            };
        }
        catch (Exception ex)
        {
            ToolLogMessages.BulkUsingAdditionFailed(_logger, sessionId, ex);
            return new { success = false, error = ex.Message };
        }
    }

    private static string ExtractTypeNameFromDiagnostic(string message)
    {
        // Message format: "The type or namespace name 'TypeName' could not be found (are you missing a using directive or an assembly reference?)"
        var start = message.IndexOf('\'');
        if (start == -1) return string.Empty;

        var end = message.IndexOf('\'', start + 1);
        if (end == -1) return string.Empty;

        return message.Substring(start + 1, end - start - 1);
    }

    private static List<INamedTypeSymbol> GetAllAvailableTypes(Compilation compilation)
    {
        var types = new List<INamedTypeSymbol>();

        // Get types from the compilation itself
        void AddTypesFromNamespace(INamespaceSymbol namespaceSymbol)
        {
            types.AddRange(namespaceSymbol.GetTypeMembers());
            foreach (var childNamespace in namespaceSymbol.GetNamespaceMembers())
            {
                AddTypesFromNamespace(childNamespace);
            }
        }

        AddTypesFromNamespace(compilation.GlobalNamespace);

        // Get types from referenced assemblies
        foreach (var reference in compilation.References)
        {
            if (compilation.GetAssemblyOrModuleSymbol(reference) is IAssemblySymbol assembly)
            {
                AddTypesFromNamespace(assembly.GlobalNamespace);
            }
        }

        return types;
    }

    private static double CalculateConfidence(string typeName, string namespaceName, int typeCount)
    {
        double confidence = 0.5; // Base confidence

        // Higher confidence for System namespaces
        if (namespaceName.StartsWith("System"))
            confidence += 0.3;

        // Higher confidence for Microsoft namespaces
        if (namespaceName.StartsWith("Microsoft"))
            confidence += 0.2;

        // Lower confidence if there are many types with the same name
        if (typeCount > 1)
            confidence -= 0.1;

        // Higher confidence for common type patterns
        if (typeName.EndsWith("Exception") && namespaceName == "System")
            confidence += 0.2;
        
        if (typeName.EndsWith("Attribute") && namespaceName == "System")
            confidence += 0.2;

        return Math.Max(0.0, Math.Min(1.0, confidence));
    }
}