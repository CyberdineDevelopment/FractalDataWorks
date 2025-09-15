using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Microsoft.Extensions.Configuration;
using ModelContextProtocol.Server;
using RoslynMcpServer.Models;
using RoslynMcpServer.Services;
using System.ComponentModel;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace RoslynMcpServer.Tools;

[McpServerToolType]
public class RefactoringTools
{
    private readonly WorkspaceSessionManager _sessionManager;
    private readonly VirtualEditService _editService;
    private readonly CodeFixConfiguration _codeFixConfig;

    public RefactoringTools(WorkspaceSessionManager sessionManager, VirtualEditService editService, IConfiguration configuration)
    {
        _sessionManager = sessionManager;
        _editService = editService;
        _codeFixConfig = configuration.GetSection("CodeFix").Get<CodeFixConfiguration>() ?? new CodeFixConfiguration();
    }

    [McpServerTool]
    [Description("Rename symbol throughout solution")]
    public async Task<string> RenameSymbolAsync(
        [Description("Session ID")] string sessionId,
        [Description("Current symbol name")] string oldName,
        [Description("New symbol name")] string newName,
        [Description("File path containing the symbol (optional - will search all files if not provided)")] string? filePath = null,
        [Description("Apply changes immediately or preview first")] bool previewOnly = true)
    {
        try
        {
            var session = _sessionManager.GetSession(sessionId);
            if (session == null)
                throw new ArgumentException("Session not found", nameof(sessionId));

            var renameCandidates = await FindRenameSymbolCandidatesAsync(session, oldName, filePath);
            if (!renameCandidates.Any())
            {
                return JsonSerializer.Serialize(new
                {
                    success = false,
                    error = $"Symbol '{oldName}' not found in the solution"
                }, new JsonSerializerOptions { WriteIndented = true });
            }

            var fileEdits = new List<Services.FileEdit>();

            foreach (var candidate in renameCandidates)
            {
                var document = session.Solution.GetDocument(candidate.DocumentId);
                if (document?.FilePath == null) continue;

                var sourceText = await document.GetTextAsync();
                var textChanges = new List<TextChange>();

                foreach (var location in candidate.Locations)
                {
                    var span = location.SourceSpan;
                    var originalText = sourceText.GetSubText(span).ToString();
                    
                    if (string.Equals(originalText, oldName, StringComparison.Ordinal))
                    {
                        textChanges.Add(new TextChange(span, newName));
                    }
                }

                if (textChanges.Any())
                {
                    fileEdits.Add(new Services.FileEdit
                    {
                        FilePath = document.FilePath,
                        Changes = textChanges
                    });
                }
            }

            if (!fileEdits.Any())
            {
                return JsonSerializer.Serialize(new
                {
                    success = false,
                    error = "No renameable occurrences found"
                }, new JsonSerializerOptions { WriteIndented = true });
            }

            if (previewOnly)
            {
                var preview = await _editService.ApplyMultipleEditsAsync(sessionId, fileEdits);
                
                return JsonSerializer.Serialize(new
                {
                    success = true,
                    sessionId,
                    previewOnly = true,
                    oldName,
                    newName,
                    filesAffected = fileEdits.Count,
                    occurrencesRenamed = fileEdits.Sum(fe => fe.Changes.Count),
                    preview = new
                    {
                        compilationSucceeded = preview.CompilationSucceeded,
                        diagnosticDiff = new
                        {
                            resolvedCount = preview.DiagnosticDiff.ResolvedCount,
                            introducedCount = preview.DiagnosticDiff.IntroducedCount,
                            netImprovement = preview.DiagnosticDiff.ResolvedCount - preview.DiagnosticDiff.IntroducedCount
                        },
                        affectedFiles = preview.Changes.Select(c => Path.GetFileName(c.FilePath))
                    }
                }, new JsonSerializerOptions { WriteIndented = true });
            }
            else
            {
                _ = await _editService.ApplyMultipleEditsAsync(sessionId, fileEdits);
                var commitResult = await _editService.CommitChangesAsync(sessionId);

                return JsonSerializer.Serialize(new
                {
                    success = commitResult.Success,
                    sessionId,
                    oldName,
                    newName,
                    filesAffected = commitResult.ModifiedFiles.Count,
                    modifiedFiles = commitResult.ModifiedFiles.Select(Path.GetFileName),
                    errors = commitResult.Errors
                }, new JsonSerializerOptions { WriteIndented = true });
            }
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
    [Description("Enforce 'one class per file' rule in bulk. Ideal for legacy codebase cleanup and organizing large files with multiple types. Groups generic variations together. PREVIEW FIRST - creates many files")]
    public async Task<string> SeparateTypesToFilesAsync(
        [Description("Session ID")] string sessionId,
        [Description("Project name to process (optional - will process all projects if not provided)")] string? projectName = null,
        [Description("Apply changes immediately or preview first")] bool previewOnly = true)
    {
        try
        {
            var session = _sessionManager.GetSession(sessionId);
            if (session == null)
                throw new ArgumentException("Session not found", nameof(sessionId));

            var projectsToProcess = string.IsNullOrEmpty(projectName) 
                ? session.Solution.Projects 
                : session.Solution.Projects.Where(p => p.Name.Contains(projectName, StringComparison.OrdinalIgnoreCase));

            var separationPlan = new List<TypeSeparationPlan>();

            foreach (var project in projectsToProcess)
            {
                var filesToProcess = await AnalyzeFilesForTypeSeparationAsync(project);
                separationPlan.AddRange(filesToProcess);
            }

            if (!separationPlan.Any())
            {
                return JsonSerializer.Serialize(new
                {
                    success = true,
                    message = "No files need type separation - all files already have one type per file",
                    sessionId
                }, new JsonSerializerOptions { WriteIndented = true });
            }

            if (previewOnly)
            {
                return JsonSerializer.Serialize(new
                {
                    success = true,
                    sessionId,
                    previewOnly = true,
                    separationPlan = new
                    {
                        filesToModify = separationPlan.Count,
                        newFilesToCreate = separationPlan.Sum(sp => sp.NewFiles.Count),
                        files = separationPlan.Select(sp => new
                        {
                            originalFile = Path.GetFileName(sp.OriginalFilePath),
                            typeCount = sp.Types.Count,
                            newFiles = sp.NewFiles.Select(nf => new
                            {
                                fileName = Path.GetFileName(nf.FilePath),
                                typeName = nf.TypeName,
                                hasGenericVariations = nf.GenericVariations.Any()
                            })
                        })
                    }
                }, new JsonSerializerOptions { WriteIndented = true });
            }
            else
            {
                var result = await ExecuteTypeSeparation(separationPlan, session);

                return JsonSerializer.Serialize(new
                {
                    success = result.Success,
                    sessionId,
                    filesModified = result.ModifiedFiles.Count,
                    filesCreated = result.CreatedFiles.Count,
                    modifiedFiles = result.ModifiedFiles.Select(Path.GetFileName),
                    createdFiles = result.CreatedFiles.Select(Path.GetFileName),
                    errors = result.Errors
                }, new JsonSerializerOptions { WriteIndented = true });
            }
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
    [Description("Auto-fix compiler and analyzer diagnostics")]
    public async Task<string> ApplyCodeFix(
        [Description("Session ID")] string sessionId,
        [Description("Diagnostic ID to fix (e.g., 'CS0103', 'CA1822')")] string diagnosticId,
        [Description("File path containing the diagnostic (optional)")] string? filePath = null,
        [Description("Maximum fixes to apply")] int maxFixes = 10,
        [Description("Apply changes immediately or preview first")] bool previewOnly = true)
    {
        try
        {
            var session = _sessionManager.GetSession(sessionId);
            if (session == null)
                throw new ArgumentException("Session not found", nameof(sessionId));

            var fixesApplied = await ApplyCodeFixesAsync(session, diagnosticId, filePath, maxFixes, previewOnly);
            
            return JsonSerializer.Serialize(new
            {
                success = true,
                fixesApplied = fixesApplied.Count,
                fixes = fixesApplied,
                previewOnly
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
    [Description("Extract single type to new file with proper naming. Perfect for splitting God classes into focused files. Handles namespaces, using statements automatically. Use for targeted file organization")]
    public async Task<string> MoveTypeToNewFileAsync(
        [Description("Session ID")] string sessionId,
        [Description("Type name to move")] string typeName,
        [Description("Source file path")] string sourceFilePath,
        [Description("Target file path (optional - will use type name if not provided)")] string? targetFilePath = null,
        [Description("Apply changes immediately or preview first")] bool previewOnly = true)
    {
        try
        {
            var session = _sessionManager.GetSession(sessionId);
            if (session == null)
                throw new ArgumentException("Session not found", nameof(sessionId));

            var sourceDocument = session.Solution.Projects
                .SelectMany(p => p.Documents)
                .FirstOrDefault(d => string.Equals(d.FilePath, sourceFilePath, StringComparison.OrdinalIgnoreCase));

            if (sourceDocument == null)
                throw new FileNotFoundException($"Source file not found: {sourceFilePath}");

            var syntaxTree = await sourceDocument.GetSyntaxTreeAsync();
            if (syntaxTree == null)
                throw new InvalidOperationException("Unable to get syntax tree");

            var root = await syntaxTree.GetRootAsync();
            var typeDeclaration = root.DescendantNodes()
                .OfType<BaseTypeDeclarationSyntax>()
                .FirstOrDefault(t => t.Identifier.Text == typeName);

            if (typeDeclaration == null)
                throw new ArgumentException($"Type '{typeName}' not found in file '{sourceFilePath}'", nameof(typeName));

            if (targetFilePath == null)
            {
                var sourceDirectory = Path.GetDirectoryName(sourceFilePath) ?? string.Empty;
                targetFilePath = Path.Combine(sourceDirectory, $"{typeName}.cs");
            }

            if (previewOnly)
            {
                return JsonSerializer.Serialize(new
                {
                    success = true,
                    sessionId,
                    previewOnly = true,
                    moveOperation = new
                    {
                        typeName,
                        sourceFile = Path.GetFileName(sourceFilePath),
                        targetFile = Path.GetFileName(targetFilePath),
                        typeKind = typeDeclaration.GetType().Name.Replace("DeclarationSyntax", ""),
                        hasMembers = typeDeclaration.ChildNodes().Any(n => n is MemberDeclarationSyntax),
                        willCreateNewFile = !File.Exists(targetFilePath)
                    }
                }, new JsonSerializerOptions { WriteIndented = true });
            }
            else
            {
                return JsonSerializer.Serialize(new
                {
                    success = false,
                    error = "Type moving implementation is not complete - this requires complex syntax tree manipulation"
                }, new JsonSerializerOptions { WriteIndented = true });
            }
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

    private static async Task<List<RenameCandidate>> FindRenameSymbolCandidatesAsync(
        CompilationSession session, 
        string symbolName, 
        string? filePath)
    {
        var candidates = new List<RenameCandidate>();

        var documentsToSearch = string.IsNullOrEmpty(filePath) 
            ? session.Solution.Projects.SelectMany(p => p.Documents)
            : session.Solution.Projects.SelectMany(p => p.Documents)
                .Where(d => string.Equals(d.FilePath, filePath, StringComparison.OrdinalIgnoreCase));

        foreach (var document in documentsToSearch)
        {
            var syntaxTree = await document.GetSyntaxTreeAsync();
            if (syntaxTree == null) continue;

            var root = await syntaxTree.GetRootAsync();
            _ = await syntaxTree.GetTextAsync();

            var locations = new List<Location>();

            // Find identifier tokens that match the symbol name
            var tokens = root.DescendantTokens()
                .Where(t => t.IsKind(SyntaxKind.IdentifierToken) && t.ValueText == symbolName);

            foreach (var token in tokens)
            {
                locations.Add(Location.Create(syntaxTree, token.Span));
            }

            if (locations.Any())
            {
                candidates.Add(new RenameCandidate
                {
                    DocumentId = document.Id,
                    Locations = locations
                });
            }
        }

        return candidates;
    }

    private static async Task<List<TypeSeparationPlan>> AnalyzeFilesForTypeSeparationAsync(Project project)
    {
        var separationPlans = new List<TypeSeparationPlan>();

        foreach (var document in project.Documents)
        {
            if (!document.FilePath?.EndsWith(".cs", StringComparison.OrdinalIgnoreCase) == true)
                continue;

            var syntaxTree = await document.GetSyntaxTreeAsync();
            if (syntaxTree == null) continue;

            var root = await syntaxTree.GetRootAsync();
            var typeDeclarations = root.DescendantNodes()
                .Where(node => node is ClassDeclarationSyntax or 
                              StructDeclarationSyntax or 
                              InterfaceDeclarationSyntax or 
                              EnumDeclarationSyntax or
                              RecordDeclarationSyntax)
                .Cast<BaseTypeDeclarationSyntax>()
                .ToList();

            if (typeDeclarations.Count <= 1)
                continue; // File already has one or no types

            var types = typeDeclarations.Select(td => new TypeInFile
            {
                Name = td.Identifier.Text,
                Declaration = td,
                IsGeneric = td is ClassDeclarationSyntax classDecl && classDecl.TypeParameterList != null ||
                           td is StructDeclarationSyntax structDecl && structDecl.TypeParameterList != null ||
                           td is InterfaceDeclarationSyntax interfaceDecl && interfaceDecl.TypeParameterList != null
            }).ToList();

            var newFiles = CreateNewFileNames(types, Path.GetDirectoryName(document.FilePath) ?? string.Empty);

            separationPlans.Add(new TypeSeparationPlan
            {
                OriginalFilePath = document.FilePath ?? string.Empty,
                Types = types,
                NewFiles = newFiles
            });
        }

        return separationPlans;
    }

    private static List<NewFileInfo> CreateNewFileNames(List<TypeInFile> types, string directory)
    {
        var newFiles = new List<NewFileInfo>();
        var typeGroups = types.GroupBy(t => GetBaseTypeName(t.Name), StringComparer.Ordinal);

        foreach (var group in typeGroups)
        {
            var baseTypeName = group.Key;
            var typesInGroup = group.ToList();
            
            newFiles.Add(new NewFileInfo
            {
                FilePath = Path.Combine(directory, $"{baseTypeName}.cs"),
                TypeName = baseTypeName,
                Types = typesInGroup,
                GenericVariations = typesInGroup.Where(t => t.IsGeneric).Select(t => t.Name).ToList()
            });
        }

        return newFiles;
    }

    private static string GetBaseTypeName(string typeName)
    {
        // Remove generic type parameter indicators like `1, `2, etc.
        var backtickIndex = typeName.IndexOf('`');
        return backtickIndex > 0 ? typeName[..backtickIndex] : typeName;
    }

    private async Task<SeparationResult> ExecuteTypeSeparation(List<TypeSeparationPlan> plans, CompilationSession session)
    {
        var modifiedFiles = new List<string>();
        var createdFiles = new List<string>();
        var errors = new List<string>();

        foreach (var plan in plans)
        {
            try
            {
                var separationResult = await ExecuteTypeSeparationAsync(plan, session);
                modifiedFiles.AddRange(separationResult.ModifiedFiles);
                createdFiles.AddRange(separationResult.CreatedFiles);
                errors.AddRange(separationResult.Errors);
            }
            catch (Exception ex)
            {
                errors.Add($"Failed to separate types in {Path.GetFileName(plan.OriginalFilePath)}: {ex.Message}");
            }
        }

        return new SeparationResult
        {
            Success = errors.Count == 0,
            ModifiedFiles = modifiedFiles,
            CreatedFiles = createdFiles,
            Errors = errors
        };
    }

    [McpServerTool]
    [Description("Analyze code for redundant null defense (ArgumentNullException throws, null-forgiving operators on non-nullable types). Helps clean up unnecessary null checks and operators when nullable reference types are properly configured")]
    public async Task<string> AnalyzeRedundantNullDefenseAsync(
        [Description("Session ID")] string sessionId,
        [Description("File path to analyze (optional - analyzes all if not provided)")] string? filePath = null,
        [Description("Include suggestions for fixing redundant null defenses")] bool includeSuggestions = true)
    {
        try
        {
            var session = _sessionManager.GetSession(sessionId);
            if (session == null)
                throw new ArgumentException("Session not found", nameof(sessionId));

            var redundantDefenses = await FindRedundantNullDefensesAsync(session, filePath);

            return JsonSerializer.Serialize(new
            {
                success = true,
                redundantDefenses = redundantDefenses.Count,
                findings = redundantDefenses.Select(r => new
                {
                    file = r.FilePath,
                    line = r.Line,
                    column = r.Column,
                    type = r.DefenseType,
                    description = r.Description,
                    parameterType = r.ParameterType,
                    suggestion = includeSuggestions ? r.Suggestion : null,
                    confidence = r.Confidence
                }),
                summary = new
                {
                    redundantArgumentNullChecks = redundantDefenses.Count(r => r.DefenseType == "ArgumentNullException"),
                    redundantNullForgivingOperators = redundantDefenses.Count(r => r.DefenseType == "NullForgivingOperator"),
                    redundantNullChecks = redundantDefenses.Count(r => r.DefenseType == "NullCheck"),
                    totalIssues = redundantDefenses.Count
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

    // Code fix implementation methods
    private async Task<List<CodeFixResult>> ApplyCodeFixesAsync(
        CompilationSession session, 
        string diagnosticId, 
        string? filePath, 
        int maxFixes, 
        bool previewOnly)
    {
        var fixes = new List<CodeFixResult>();
        
        // Get diagnostics matching the specified ID
        var allDiagnostics = await GetDiagnosticsForFixingAsync(session, diagnosticId, filePath);
        var diagnosticsToFix = allDiagnostics.Take(maxFixes).ToList();

        foreach (var diagnostic in diagnosticsToFix)
        {
            try
            {
                var fix = await ApplySpecificCodeFixAsync(session, diagnostic, previewOnly);
                if (fix != null)
                    fixes.Add(fix);
            }
            catch (Exception ex)
            {
                fixes.Add(new CodeFixResult
                {
                    Success = false,
                    DiagnosticId = diagnostic.Id,
                    FilePath = diagnostic.Location.SourceTree?.FilePath ?? "Unknown",
                    Error = ex.Message
                });
            }
        }

        return fixes;
    }

    private static async Task<List<Diagnostic>> GetDiagnosticsForFixingAsync(
        CompilationSession session, 
        string diagnosticId, 
        string? filePath)
    {
        var allDiagnostics = new List<Diagnostic>();

        foreach (var project in session.Solution.Projects)
        {
            var compilation = await project.GetCompilationAsync();
            if (compilation == null) continue;

            var projectDiagnostics = compilation.GetDiagnostics()
                .Where(d => d.Id.Equals(diagnosticId, StringComparison.OrdinalIgnoreCase))
                .Where(d => d.Severity >= DiagnosticSeverity.Warning);

            if (!string.IsNullOrEmpty(filePath))
            {
                projectDiagnostics = projectDiagnostics
                    .Where(d => string.Equals(d.Location.SourceTree?.FilePath, filePath, StringComparison.OrdinalIgnoreCase));
            }

            allDiagnostics.AddRange(projectDiagnostics);
        }

        return allDiagnostics;
    }

    private async Task<CodeFixResult?> ApplySpecificCodeFixAsync(
        CompilationSession session, 
        Diagnostic diagnostic, 
        bool previewOnly)
    {
        var document = session.Solution.GetDocument(diagnostic.Location.SourceTree);
        if (document == null) return null;

        // Handle common diagnostic types with manual implementations
        return diagnostic.Id.ToUpperInvariant() switch
        {
            "CA1822" => await FixCA1822MakeStaticAsync(session, document, diagnostic, previewOnly),
            "CS0103" => await FixCS0103UndefinedNameAsync(session, document, diagnostic, previewOnly),
            "CA1307" => await FixCA1307AddStringComparisonAsync(session, document, diagnostic, previewOnly),
            "CA1309" => await FixCA1309UseOrdinalComparisonAsync(session, document, diagnostic, previewOnly),
            "CA1310" => await FixCA1310SpecifyStringComparisonAsync(session, document, diagnostic, previewOnly),
            "MA0002" => await FixMA0002AddStringComparerAsync(session, document, diagnostic, previewOnly),
            "MA0006" => await FixMA0006UseStringEqualsAsync(session, document, diagnostic, previewOnly),
            "MA0009" => await FixMA0009RegexTimeoutAsync(session, document, diagnostic, previewOnly),
            _ => new CodeFixResult
            {
                Success = false,
                DiagnosticId = diagnostic.Id,
                FilePath = document.FilePath ?? "Unknown",
                Error = $"No code fix available for diagnostic {diagnostic.Id}"
            }
        };
    }

    private async Task<CodeFixResult> FixCA1822MakeStaticAsync(CompilationSession session, Document document, Diagnostic diagnostic, bool previewOnly)
    {
        var root = await document.GetSyntaxRootAsync();
        var node = root?.FindNode(diagnostic.Location.SourceSpan);
        
        if (node is MethodDeclarationSyntax method)
        {
            var newMethod = method.AddModifiers(SyntaxFactory.Token(SyntaxKind.StaticKeyword));
            
            if (!previewOnly)
            {
                var edit = new Services.FileEdit
                {
                    FilePath = document.FilePath!,
                    Changes = new List<Microsoft.CodeAnalysis.Text.TextChange>
                    {
                        new(diagnostic.Location.SourceSpan, newMethod.ToFullString())
                    }
                };
                
                await _editService.ApplyVirtualEditAsync(session.Id, edit);
            }

            return new CodeFixResult
            {
                Success = true,
                DiagnosticId = diagnostic.Id,
                FilePath = document.FilePath ?? "Unknown",
                FixDescription = $"Made method '{method.Identifier}' static",
                ChangesPreview = previewOnly ? $"+ static {method.Identifier}" : null
            };
        }

        return new CodeFixResult
        {
            Success = false,
            DiagnosticId = diagnostic.Id,
            FilePath = document.FilePath ?? "Unknown",
            Error = "Could not find method to make static"
        };
    }

    private async Task<CodeFixResult> FixCS0103UndefinedNameAsync(CompilationSession session, Document document, Diagnostic diagnostic, bool previewOnly)
    {
        var root = await document.GetSyntaxRootAsync();
        if (root is not CompilationUnitSyntax compilationUnit) 
        {
            return new CodeFixResult
            {
                Success = false,
                DiagnosticId = diagnostic.Id,
                FilePath = document.FilePath ?? "Unknown",
                Error = "Could not access compilation unit"
            };
        }

        // Common missing using statements based on diagnostic message
        var missingUsings = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
        {
            ["console"] = ["System"],
            ["string"] = ["System"],
            ["datetime"] = ["System"],
            ["list"] = ["System.Collections.Generic"],
            ["dictionary"] = ["System.Collections.Generic"],
            ["task"] = ["System.Threading.Tasks"],
            ["cancellationtoken"] = ["System.Threading"],
            ["httpcontext"] = ["Microsoft.AspNetCore.Http"],
            ["ilogger"] = ["Microsoft.Extensions.Logging"]
        };

        var diagnosticText = diagnostic.GetMessage().ToLowerInvariant();
        foreach (var kvp in missingUsings)
        {
            if (diagnosticText.Contains(kvp.Key))
            {
                foreach (var namespaceName in kvp.Value)
                {
                    if (!HasUsingDirective(compilationUnit, namespaceName))
                    {
                        if (!previewOnly)
                        {
                            var edit = new Services.FileEdit
                            {
                                FilePath = document.FilePath!,
                                Changes = new List<Microsoft.CodeAnalysis.Text.TextChange>
                                {
                                    new(Microsoft.CodeAnalysis.Text.TextSpan.FromBounds(0, 0), $"using {namespaceName};\n")
                                }
                            };
                            
                            await _editService.ApplyVirtualEditAsync(session.Id, edit);
                        }

                        return new CodeFixResult
                        {
                            Success = true,
                            DiagnosticId = diagnostic.Id,
                            FilePath = document.FilePath ?? "Unknown",
                            FixDescription = $"Added using {namespaceName};",
                            ChangesPreview = previewOnly ? $"+ using {namespaceName};" : null
                        };
                    }
                }
                break;
            }
        }

        return new CodeFixResult
        {
            Success = false,
            DiagnosticId = diagnostic.Id,
            FilePath = document.FilePath ?? "Unknown",
            Error = "Could not determine missing using statement"
        };
    }

    private static bool HasUsingDirective(CompilationUnitSyntax compilationUnit, string namespaceName)
    {
        return compilationUnit.Usings.Any(u => u.Name?.ToString() == namespaceName);
    }

    private async Task<SeparationResult> ExecuteTypeSeparationAsync(TypeSeparationPlan plan, CompilationSession session)
    {
        var modifiedFiles = new List<string>();
        var createdFiles = new List<string>();
        var errors = new List<string>();

        try
        {
            // Get the original document and its content
            var originalDocument = FindDocument(session.Solution, plan.OriginalFilePath);
            if (originalDocument == null)
            {
                errors.Add($"Original file not found: {plan.OriginalFilePath}");
                return new SeparationResult { Success = false, Errors = errors };
            }

            var originalRoot = await originalDocument.GetSyntaxRootAsync() as CompilationUnitSyntax;
            if (originalRoot == null)
            {
                errors.Add($"Could not parse original file: {plan.OriginalFilePath}");
                return new SeparationResult { Success = false, Errors = errors };
            }

            // Create new files for each type
            foreach (var newFile in plan.NewFiles)
            {
                try
                {
                    var newFileContent = CreateNewFileContent(originalRoot, newFile);
                    
                    // Create the new file on disk
                    var directory = Path.GetDirectoryName(newFile.FilePath);
                    if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                    {
                        Directory.CreateDirectory(directory);
                    }

                    await File.WriteAllTextAsync(newFile.FilePath, newFileContent);
                    createdFiles.Add(newFile.FilePath);
                }
                catch (Exception ex)
                {
                    errors.Add($"Failed to create file {newFile.FilePath}: {ex.Message}");
                }
            }

            // Update the original file by removing moved types
            if (createdFiles.Count > 0)
            {
                var updatedOriginal = UpdateOriginalFile(originalRoot, plan.Types, plan.NewFiles);
                
                if (updatedOriginal.HasTypes)
                {
                    // Original file still has types, update it
                    await File.WriteAllTextAsync(plan.OriginalFilePath, updatedOriginal.Content);
                    modifiedFiles.Add(plan.OriginalFilePath);
                }
                else
                {
                    // Original file is now empty, delete it
                    File.Delete(plan.OriginalFilePath);
                    modifiedFiles.Add($"DELETED: {plan.OriginalFilePath}");
                }
            }

            // Refresh the session to reflect file changes
            await _sessionManager.RefreshSessionAsync(session.Id);
        }
        catch (Exception ex)
        {
            errors.Add($"Type separation failed: {ex.Message}");
        }

        return new SeparationResult
        {
            Success = errors.Count == 0,
            ModifiedFiles = modifiedFiles,
            CreatedFiles = createdFiles,
            Errors = errors
        };
    }

    private static string CreateNewFileContent(CompilationUnitSyntax originalRoot, NewFileInfo newFile)
    {
        // Start with original using directives
        var newRoot = SyntaxFactory.CompilationUnit()
            .WithUsings(originalRoot.Usings);

        // Add namespace if present in original
        if (originalRoot.Members.OfType<BaseNamespaceDeclarationSyntax>().FirstOrDefault() is BaseNamespaceDeclarationSyntax originalNamespace)
        {
            // Create new namespace with only the types for this file
            var newNamespace = SyntaxFactory.NamespaceDeclaration(originalNamespace.Name)
                .WithNamespaceKeyword(originalNamespace.NamespaceKeyword)
                .WithOpenBraceToken(SyntaxFactory.Token(SyntaxKind.OpenBraceToken))
                .WithCloseBraceToken(SyntaxFactory.Token(SyntaxKind.CloseBraceToken))
                .WithMembers(SyntaxFactory.List(newFile.Types.Select(t => t.Declaration).Cast<MemberDeclarationSyntax>()));

            newRoot = newRoot.WithMembers(SyntaxFactory.List<MemberDeclarationSyntax>(new[] { newNamespace }));
        }
        else
        {
            // No namespace, add types directly to compilation unit
            newRoot = newRoot.WithMembers(SyntaxFactory.List(newFile.Types.Select(t => t.Declaration).Cast<MemberDeclarationSyntax>()));
        }

        return newRoot.ToFullString();
    }

    private static (bool HasTypes, string Content) UpdateOriginalFile(
        CompilationUnitSyntax originalRoot, 
        List<TypeInFile> allTypes,
        List<NewFileInfo> newFiles)
    {
        // Get all types that were moved to new files
        var movedTypes = newFiles.SelectMany(f => f.Types).ToHashSet();
        
        // Find types that should remain in original file
        var remainingTypes = allTypes.Where(t => !movedTypes.Contains(t)).ToList();

        if (remainingTypes.Count == 0)
        {
            return (false, string.Empty);
        }

        // Create updated compilation unit
        CompilationUnitSyntax newRoot;
        
        if (originalRoot.Members.OfType<BaseNamespaceDeclarationSyntax>().FirstOrDefault() is BaseNamespaceDeclarationSyntax originalNamespace)
        {
            // Update namespace with only remaining types
            var newNamespace = originalNamespace
                .WithMembers(SyntaxFactory.List(remainingTypes.Select(t => t.Declaration).Cast<MemberDeclarationSyntax>()));

            newRoot = originalRoot
                .WithMembers(SyntaxFactory.List<MemberDeclarationSyntax>(new[] { newNamespace }));
        }
        else
        {
            // No namespace, update compilation unit directly
            newRoot = originalRoot
                .WithMembers(SyntaxFactory.List(remainingTypes.Select(t => t.Declaration).Cast<MemberDeclarationSyntax>()));
        }

        return (true, newRoot.ToFullString());
    }

    private static Document? FindDocument(Solution solution, string filePath)
    {
        return solution.Projects
            .SelectMany(p => p.Documents)
            .FirstOrDefault(d => string.Equals(d.FilePath, filePath, StringComparison.OrdinalIgnoreCase));
    }

    // String comparison and collection code fixes
    private async Task<CodeFixResult> FixCA1307AddStringComparisonAsync(CompilationSession session, Document document, Diagnostic diagnostic, bool previewOnly)
    {
        var root = await document.GetSyntaxRootAsync();
        var node = root?.FindNode(diagnostic.Location.SourceSpan);
        
        if (node is InvocationExpressionSyntax invocation)
        {
            var newInvocation = AddStringComparisonToInvocation(invocation, _codeFixConfig.DefaultStringComparison.CaseSensitive);
            
            if (!previewOnly)
            {
                var edit = new Services.FileEdit
                {
                    FilePath = document.FilePath!,
                    Changes = new List<Microsoft.CodeAnalysis.Text.TextChange>
                    {
                        new(diagnostic.Location.SourceSpan, newInvocation.ToFullString())
                    }
                };
                
                await _editService.ApplyVirtualEditAsync(session.Id, edit);
            }

            return new CodeFixResult
            {
                Success = true,
                DiagnosticId = diagnostic.Id,
                FilePath = document.FilePath ?? "Unknown",
                FixDescription = $"Added {_codeFixConfig.DefaultStringComparison.CaseSensitive} parameter",
                ChangesPreview = previewOnly ? newInvocation.ToString() : null  // ToString() excludes comments by default
            };
        }

        return new CodeFixResult
        {
            Success = false,
            DiagnosticId = diagnostic.Id,
            FilePath = document.FilePath ?? "Unknown",
            Error = "Could not find string method invocation"
        };
    }

    private async Task<CodeFixResult> FixCA1309UseOrdinalComparisonAsync(CompilationSession session, Document document, Diagnostic diagnostic, bool previewOnly)
    {
        var root = await document.GetSyntaxRootAsync();
        var node = root?.FindNode(diagnostic.Location.SourceSpan);
        
        if (node is InvocationExpressionSyntax invocation)
        {
            var newInvocation = ReplaceStringComparisonArgument(invocation, _codeFixConfig.DefaultStringComparison.CaseSensitive);
            
            if (!previewOnly)
            {
                var edit = new Services.FileEdit
                {
                    FilePath = document.FilePath!,
                    Changes = new List<Microsoft.CodeAnalysis.Text.TextChange>
                    {
                        new(diagnostic.Location.SourceSpan, newInvocation.ToFullString())
                    }
                };
                
                await _editService.ApplyVirtualEditAsync(session.Id, edit);
            }

            return new CodeFixResult
            {
                Success = true,
                DiagnosticId = diagnostic.Id,
                FilePath = document.FilePath ?? "Unknown",
                FixDescription = $"Changed to {_codeFixConfig.DefaultStringComparison.CaseSensitive}",
                ChangesPreview = previewOnly ? newInvocation.ToFullString() : null
            };
        }

        return new CodeFixResult
        {
            Success = false,
            DiagnosticId = diagnostic.Id,
            FilePath = document.FilePath ?? "Unknown",
            Error = "Could not find string method invocation"
        };
    }

    private Task<CodeFixResult> FixCA1310SpecifyStringComparisonAsync(CompilationSession session, Document document, Diagnostic diagnostic, bool previewOnly)
    {
        // CA1310 is similar to CA1307 - specify StringComparison for correctness
        return FixCA1307AddStringComparisonAsync(session, document, diagnostic, previewOnly);
    }

    private async Task<CodeFixResult> FixMA0002AddStringComparerAsync(CompilationSession session, Document document, Diagnostic diagnostic, bool previewOnly)
    {
        var root = await document.GetSyntaxRootAsync();
        var node = root?.FindNode(diagnostic.Location.SourceSpan);
        
        if (node is ObjectCreationExpressionSyntax objectCreation)
        {
            var newObjectCreation = AddStringComparerToConstructor(objectCreation);
            
            if (!previewOnly)
            {
                var edit = new Services.FileEdit
                {
                    FilePath = document.FilePath!,
                    Changes = new List<Microsoft.CodeAnalysis.Text.TextChange>
                    {
                        new(diagnostic.Location.SourceSpan, newObjectCreation.ToFullString())
                    }
                };
                
                await _editService.ApplyVirtualEditAsync(session.Id, edit);
            }

            return new CodeFixResult
            {
                Success = true,
                DiagnosticId = diagnostic.Id,
                FilePath = document.FilePath ?? "Unknown",
                FixDescription = "Added StringComparer.Ordinal parameter",
                ChangesPreview = previewOnly ? newObjectCreation.ToFullString() : null
            };
        }

        return new CodeFixResult
        {
            Success = false,
            DiagnosticId = diagnostic.Id,
            FilePath = document.FilePath ?? "Unknown",
            Error = "Could not find collection constructor"
        };
    }

    private async Task<CodeFixResult> FixMA0006UseStringEqualsAsync(CompilationSession session, Document document, Diagnostic diagnostic, bool previewOnly)
    {
        var root = await document.GetSyntaxRootAsync();
        var node = root?.FindNode(diagnostic.Location.SourceSpan);
        
        if (node is BinaryExpressionSyntax binaryExpression && 
            (binaryExpression.OperatorToken.IsKind(SyntaxKind.EqualsEqualsToken) ||
             binaryExpression.OperatorToken.IsKind(SyntaxKind.ExclamationEqualsToken)))
        {
            var isNotEquals = binaryExpression.OperatorToken.IsKind(SyntaxKind.ExclamationEqualsToken);
            var stringEqualsCall = CreateStringEqualsCall(binaryExpression.Left, binaryExpression.Right, isNotEquals);
            
            if (!previewOnly)
            {
                var edit = new Services.FileEdit
                {
                    FilePath = document.FilePath!,
                    Changes = new List<Microsoft.CodeAnalysis.Text.TextChange>
                    {
                        new(diagnostic.Location.SourceSpan, stringEqualsCall.ToFullString())
                    }
                };
                
                await _editService.ApplyVirtualEditAsync(session.Id, edit);
            }

            return new CodeFixResult
            {
                Success = true,
                DiagnosticId = diagnostic.Id,
                FilePath = document.FilePath ?? "Unknown",
                FixDescription = $"Replaced {(isNotEquals ? "!=" : "==")} with String.Equals",
                ChangesPreview = previewOnly ? stringEqualsCall.ToFullString() : null
            };
        }

        return new CodeFixResult
        {
            Success = false,
            DiagnosticId = diagnostic.Id,
            FilePath = document.FilePath ?? "Unknown",
            Error = "Could not find string equality comparison"
        };
    }

    private async Task<CodeFixResult> FixMA0009RegexTimeoutAsync(CompilationSession session, Document document, Diagnostic diagnostic, bool previewOnly)
    {
        var root = await document.GetSyntaxRootAsync();
        var node = root?.FindNode(diagnostic.Location.SourceSpan);
        
        if (node is ObjectCreationExpressionSyntax regexCreation)
        {
            var newRegexCreation = AddTimeoutToRegexConstructor(regexCreation);
            
            if (!previewOnly)
            {
                var edit = new Services.FileEdit
                {
                    FilePath = document.FilePath!,
                    Changes = new List<Microsoft.CodeAnalysis.Text.TextChange>
                    {
                        new(diagnostic.Location.SourceSpan, newRegexCreation.ToFullString())
                    }
                };
                
                await _editService.ApplyVirtualEditAsync(session.Id, edit);
            }

            return new CodeFixResult
            {
                Success = true,
                DiagnosticId = diagnostic.Id,
                FilePath = document.FilePath ?? "Unknown",
                FixDescription = $"Added {_codeFixConfig.RegexTimeouts.DefaultTimeoutMs}ms timeout",
                ChangesPreview = previewOnly ? newRegexCreation.ToFullString() : null
            };
        }

        return new CodeFixResult
        {
            Success = false,
            DiagnosticId = diagnostic.Id,
            FilePath = document.FilePath ?? "Unknown",
            Error = "Could not find Regex constructor"
        };
    }

    // Helper methods for syntax transformations
    private static InvocationExpressionSyntax AddStringComparisonToInvocation(InvocationExpressionSyntax invocation, string stringComparison)
    {
        var stringComparisonArg = SyntaxFactory.Argument(
            SyntaxFactory.MemberAccessExpression(
                SyntaxKind.SimpleMemberAccessExpression,
                SyntaxFactory.IdentifierName("StringComparison"),
                SyntaxFactory.IdentifierName(stringComparison.Split('.').Last())));

        var newArgumentList = invocation.ArgumentList.AddArguments(stringComparisonArg);
        return invocation.WithArgumentList(newArgumentList);
    }

    private static InvocationExpressionSyntax ReplaceStringComparisonArgument(InvocationExpressionSyntax invocation, string stringComparison)
    {
        var args = invocation.ArgumentList.Arguments;
        if (args.Count == 0) return invocation;

        // Find the StringComparison argument (usually last)
        var lastArg = args[args.Count - 1];
        var newArg = SyntaxFactory.Argument(
            SyntaxFactory.MemberAccessExpression(
                SyntaxKind.SimpleMemberAccessExpression,
                SyntaxFactory.IdentifierName("StringComparison"),
                SyntaxFactory.IdentifierName(stringComparison.Split('.').Last())));

        var newArgs = args.Replace(lastArg, newArg);
        return invocation.WithArgumentList(SyntaxFactory.ArgumentList(newArgs));
    }

    private static ObjectCreationExpressionSyntax AddStringComparerToConstructor(ObjectCreationExpressionSyntax objectCreation)
    {
        var stringComparerArg = SyntaxFactory.Argument(
            SyntaxFactory.MemberAccessExpression(
                SyntaxKind.SimpleMemberAccessExpression,
                SyntaxFactory.IdentifierName("StringComparer"),
                SyntaxFactory.IdentifierName("Ordinal")));

        var argumentList = objectCreation.ArgumentList ?? SyntaxFactory.ArgumentList();
        var newArgumentList = argumentList.AddArguments(stringComparerArg);
        return objectCreation.WithArgumentList(newArgumentList);
    }

    private ExpressionSyntax CreateStringEqualsCall(ExpressionSyntax left, ExpressionSyntax right, bool isNotEquals)
    {
        var stringEqualsCall = SyntaxFactory.InvocationExpression(
            SyntaxFactory.MemberAccessExpression(
                SyntaxKind.SimpleMemberAccessExpression,
                SyntaxFactory.IdentifierName("String"),
                SyntaxFactory.IdentifierName("Equals")))
            .WithArgumentList(SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList(new[]
            {
                SyntaxFactory.Argument(left),
                SyntaxFactory.Argument(right),
                SyntaxFactory.Argument(SyntaxFactory.MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    SyntaxFactory.IdentifierName("StringComparison"),
                    SyntaxFactory.IdentifierName(_codeFixConfig.DefaultStringComparison.CaseSensitive.Split('.').Last())))
            })));

        return isNotEquals 
            ? SyntaxFactory.PrefixUnaryExpression(SyntaxKind.LogicalNotExpression, stringEqualsCall)
            : stringEqualsCall;
    }

    private ObjectCreationExpressionSyntax AddTimeoutToRegexConstructor(ObjectCreationExpressionSyntax regexCreation)
    {
        var timeoutArg = SyntaxFactory.Argument(
            SyntaxFactory.InvocationExpression(
                SyntaxFactory.MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    SyntaxFactory.IdentifierName("TimeSpan"),
                    SyntaxFactory.IdentifierName("FromMilliseconds")))
                .WithArgumentList(SyntaxFactory.ArgumentList(SyntaxFactory.SingletonSeparatedList(
                    SyntaxFactory.Argument(SyntaxFactory.LiteralExpression(
                        SyntaxKind.NumericLiteralExpression,
                        SyntaxFactory.Literal(_codeFixConfig.RegexTimeouts.DefaultTimeoutMs)))))));

        var argumentList = regexCreation.ArgumentList ?? SyntaxFactory.ArgumentList();
        var newArgumentList = argumentList.AddArguments(timeoutArg);
        return regexCreation.WithArgumentList(newArgumentList);
    }

    // Redundant null defense analysis
    private async Task<List<RedundantNullDefense>> FindRedundantNullDefensesAsync(CompilationSession session, string? filePath)
    {
        var redundantDefenses = new List<RedundantNullDefense>();

        if (!_codeFixConfig.EnableRedundantNullDefenseDetection)
            return redundantDefenses;

        var documentsToAnalyze = string.IsNullOrEmpty(filePath) 
            ? session.Solution.Projects.SelectMany(p => p.Documents)
            : session.Solution.Projects.SelectMany(p => p.Documents)
                .Where(d => string.Equals(d.FilePath, filePath, StringComparison.OrdinalIgnoreCase));

        foreach (var document in documentsToAnalyze)
        {
            if (document.FilePath?.EndsWith(".cs", StringComparison.OrdinalIgnoreCase) != true)
                continue;

            var semanticModel = await document.GetSemanticModelAsync();
            var root = await document.GetSyntaxRootAsync();
            
            if (semanticModel == null || root == null) continue;

            var findings = AnalyzeDocumentForRedundantNullDefense(document, semanticModel, root);
            redundantDefenses.AddRange(findings);
        }

        return redundantDefenses;
    }

    private List<RedundantNullDefense> AnalyzeDocumentForRedundantNullDefense(Document document, SemanticModel semanticModel, SyntaxNode root)
    {
        var findings = new List<RedundantNullDefense>();

        if (_codeFixConfig.NullDefense.DetectRedundantArgumentNullChecks)
        {
            findings.AddRange(FindRedundantArgumentNullChecks(document, semanticModel, root));
        }

        if (_codeFixConfig.NullDefense.DetectRedundantNullForgivingOperators)
        {
            findings.AddRange(FindRedundantNullForgivingOperators(document, semanticModel, root));
        }

        return findings;
    }

    private static List<RedundantNullDefense> FindRedundantArgumentNullChecks(Document document, SemanticModel semanticModel, SyntaxNode root)
    {
        var findings = new List<RedundantNullDefense>();

        var argumentNullChecks = root.DescendantNodes()
            .OfType<InvocationExpressionSyntax>()
            .Where(inv => IsArgumentNullExceptionThrow(inv, semanticModel));

        foreach (var check in argumentNullChecks)
        {
            var linePosition = check.GetLocation().GetLineSpan().StartLinePosition;
            var parameterName = ExtractParameterNameFromArgumentNullCheck(check);
            
            if (!string.IsNullOrEmpty(parameterName))
            {
                var parameterType = GetParameterTypeInMethod(check, parameterName, semanticModel);
                
                if (parameterType != null && !IsNullableType(parameterType))
                {
                    findings.Add(new RedundantNullDefense
                    {
                        FilePath = document.FilePath ?? "Unknown",
                        Line = linePosition.Line + 1,
                        Column = linePosition.Character + 1,
                        DefenseType = "ArgumentNullException",
                        Description = $"Redundant ArgumentNullException check for non-nullable parameter '{parameterName}'",
                        ParameterType = parameterType.ToDisplayString(),
                        Suggestion = $"Remove ArgumentNullException check for '{parameterName}' - parameter is non-nullable",
                        Confidence = "High"
                    });
                }
            }
        }

        return findings;
    }

    private static List<RedundantNullDefense> FindRedundantNullForgivingOperators(Document document, SemanticModel semanticModel, SyntaxNode root)
    {
        var findings = new List<RedundantNullDefense>();

        var nullForgivingOperators = root.DescendantNodes()
            .OfType<PostfixUnaryExpressionSyntax>()
            .Where(expr => expr.OperatorToken.IsKind(SyntaxKind.ExclamationToken));

        foreach (var op in nullForgivingOperators)
        {
            var linePosition = op.GetLocation().GetLineSpan().StartLinePosition;
            var operandType = semanticModel.GetTypeInfo(op.Operand).Type;
            
            if (operandType != null && !IsNullableType(operandType))
            {
                findings.Add(new RedundantNullDefense
                {
                    FilePath = document.FilePath ?? "Unknown",
                    Line = linePosition.Line + 1,
                    Column = linePosition.Character + 1,
                    DefenseType = "NullForgivingOperator",
                    Description = $"Redundant null-forgiving operator (!) on non-nullable type '{operandType.Name}'",
                    ParameterType = operandType.ToDisplayString(),
                    Suggestion = $"Remove null-forgiving operator - type '{operandType.Name}' is already non-nullable",
                    Confidence = "High"
                });
            }
        }

        return findings;
    }

    private static bool IsArgumentNullExceptionThrow(InvocationExpressionSyntax invocation, SemanticModel semanticModel)
    {
        if (invocation.Expression is MemberAccessExpressionSyntax memberAccess)
        {
            if (memberAccess.Name.Identifier.ValueText == "ThrowIfNull" ||
                memberAccess.Name.Identifier.ValueText == "ArgumentNullException")
            {
                return true;
            }
        }

        // Check for throw new ArgumentNullException patterns
        if (invocation.Parent is ThrowStatementSyntax)
        {
            var symbolInfo = semanticModel.GetSymbolInfo(invocation);
            return symbolInfo.Symbol?.ContainingType?.Name == "ArgumentNullException";
        }

        return false;
    }

    private static string? ExtractParameterNameFromArgumentNullCheck(InvocationExpressionSyntax invocation)
    {
        if (invocation.ArgumentList.Arguments.Count > 0)
        {
            var firstArg = invocation.ArgumentList.Arguments[0];
            if (firstArg.Expression is LiteralExpressionSyntax literal &&
                literal.Token.IsKind(SyntaxKind.StringLiteralToken))
            {
                return literal.Token.ValueText;
            }
            
            if (firstArg.Expression is InvocationExpressionSyntax nameofCall &&
                nameofCall.Expression is IdentifierNameSyntax nameofId &&
                nameofId.Identifier.ValueText == "nameof" &&
                nameofCall.ArgumentList.Arguments.Count > 0)
            {
                if (nameofCall.ArgumentList.Arguments[0].Expression is IdentifierNameSyntax paramName)
                {
                    return paramName.Identifier.ValueText;
                }
            }
        }

        return null;
    }

    private static ITypeSymbol? GetParameterTypeInMethod(SyntaxNode node, string parameterName, SemanticModel semanticModel)
    {
        var method = node.Ancestors().OfType<MethodDeclarationSyntax>().FirstOrDefault();
        if (method != null)
        {
            var parameter = method.ParameterList.Parameters
                .FirstOrDefault(p => p.Identifier.ValueText == parameterName);
            
            if (parameter != null)
            {
                return semanticModel.GetTypeInfo(parameter.Type!).Type;
            }
        }

        return null;
    }

    private static bool IsNullableType(ITypeSymbol type)
    {
        return type.CanBeReferencedByName && 
               (type.IsReferenceType || 
                (type.IsValueType && type.Name == "Nullable"));
    }

    private sealed record RedundantNullDefense
    {
        public string FilePath { get; init; } = string.Empty;
        public int Line { get; init; }
        public int Column { get; init; }
        public string DefenseType { get; init; } = string.Empty; // "ArgumentNullException", "NullForgivingOperator", "NullCheck"
        public string Description { get; init; } = string.Empty;
        public string ParameterType { get; init; } = string.Empty;
        public string Suggestion { get; init; } = string.Empty;
        public string Confidence { get; init; } = string.Empty; // "High", "Medium", "Low"
    }

    private sealed record CodeFixResult
    {
        public bool Success { get; init; }
        public string DiagnosticId { get; init; } = string.Empty;
        public string FilePath { get; init; } = string.Empty;
        public string? FixDescription { get; init; }
        public string? ChangesPreview { get; init; }
        public string? Error { get; init; }
    }
}

internal sealed record RenameCandidate
{
    public required DocumentId DocumentId { get; init; }
    public List<Location> Locations { get; init; } = new();
}

internal sealed record TypeSeparationPlan
{
    public string OriginalFilePath { get; init; } = string.Empty;
    public List<TypeInFile> Types { get; init; } = new();
    public List<NewFileInfo> NewFiles { get; init; } = new();
}

internal sealed record TypeInFile
{
    public string Name { get; init; } = string.Empty;
    public BaseTypeDeclarationSyntax Declaration { get; init; } = null!;
    public bool IsGeneric { get; init; }
}

internal sealed record NewFileInfo
{
    public string FilePath { get; init; } = string.Empty;
    public string TypeName { get; init; } = string.Empty;
    public List<TypeInFile> Types { get; init; } = new();
    public List<string> GenericVariations { get; init; } = new();
}

internal sealed record SeparationResult
{
    public bool Success { get; init; }
    public List<string> ModifiedFiles { get; init; } = new();
    public List<string> CreatedFiles { get; init; } = new();
    public List<string> Errors { get; init; } = new();
}