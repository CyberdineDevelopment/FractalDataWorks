using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ModelContextProtocol.Server;
using RoslynMcpServer.Models;
using RoslynMcpServer.Services;
using System.ComponentModel;
using System.Text.Json;

namespace RoslynMcpServer.Tools;

[McpServerToolType]
public class TypeAnalysisTools
{
    private readonly WorkspaceSessionManager _sessionManager;

    public TypeAnalysisTools(WorkspaceSessionManager sessionManager)
    {
        _sessionManager = sessionManager;
    }

    [McpServerTool]
    [Description("Find ambiguous type names across namespaces")]
    public async Task<string> FindAmbiguousTypesAsync(
        [Description("Session ID")] string sessionId,
        [Description("Filter by type name (optional)")] string? typeName = null,
        [Description("Maximum results to return")] int maxResults = 50)
    {
        try
        {
            var session = _sessionManager.GetSession(sessionId);
            if (session == null)
                throw new ArgumentException("Session not found", nameof(sessionId));

            var allTypes = new List<TypeInfoModel>();

            foreach (var project in session.Solution.Projects)
            {
                var compilation = await project.GetCompilationAsync();
                if (compilation == null) continue;

                foreach (var document in project.Documents)
                {
                    var syntaxTree = await document.GetSyntaxTreeAsync();
                    if (syntaxTree == null) continue;

                    var root = await syntaxTree.GetRootAsync();
                    var semanticModel = compilation.GetSemanticModel(syntaxTree);

                    var typeDeclarations = root.DescendantNodes()
                        .Where(node => node is ClassDeclarationSyntax or 
                                      StructDeclarationSyntax or 
                                      InterfaceDeclarationSyntax or 
                                      EnumDeclarationSyntax or
                                      RecordDeclarationSyntax)
                        .Cast<BaseTypeDeclarationSyntax>();

                    foreach (var typeDecl in typeDeclarations)
                    {
                        var symbol = semanticModel.GetDeclaredSymbol(typeDecl);
                        if (symbol == null) continue;

                        var typeInfo = CreateTypeInfo(symbol, document.FilePath ?? string.Empty);
                        if (typeInfo != null && 
                            (string.IsNullOrEmpty(typeName) || typeInfo.Name.Contains(typeName, StringComparison.OrdinalIgnoreCase)))
                        {
                            allTypes.Add(typeInfo);
                        }
                    }
                }
            }

            var ambiguousTypes = allTypes
                .GroupBy(t => t.Name, StringComparer.Ordinal)
                .Where(g => g.Count() > 1)
                .Select(g => new AmbiguousTypeInfo
                {
                    Name = g.Key,
                    Conflicts = g.ToList()
                })
                .Take(maxResults)
                .ToList();

            return JsonSerializer.Serialize(new
            {
                success = true,
                sessionId,
                ambiguousTypeCount = ambiguousTypes.Count,
                ambiguousTypes = ambiguousTypes.Select(at => new
                {
                    name = at.Name,
                    conflictCount = at.ConflictCount,
                    conflicts = at.Conflicts.Select(c => new
                    {
                        fullName = c.FullName,
                        @namespace = c.Namespace,
                        containingFile = Path.GetFileName(c.ContainingFile),
                        kind = c.Kind.ToString(),
                        isGeneric = c.IsGeneric,
                        genericParameters = c.GenericParameters
                    })
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
    [Description("Identify duplicate type definitions causing compilation errors. Finds types declared multiple times without partial keyword. Critical for cleaning up large codebases with naming conflicts")]
    public async Task<string> FindDuplicateTypesAsync(
        [Description("Session ID")] string sessionId,
        [Description("Maximum results to return")] int maxResults = 50)
    {
        try
        {
            var session = _sessionManager.GetSession(sessionId);
            if (session == null)
                throw new ArgumentException("Session not found", nameof(sessionId));

            var typeDeclarations = new Dictionary<string, List<(string file, bool isPartial, TypeKind kind)>>(StringComparer.Ordinal);

            foreach (var project in session.Solution.Projects)
            {
                var compilation = await project.GetCompilationAsync();
                if (compilation == null) continue;

                foreach (var document in project.Documents)
                {
                    var syntaxTree = await document.GetSyntaxTreeAsync();
                    if (syntaxTree == null) continue;

                    var root = await syntaxTree.GetRootAsync();
                    var semanticModel = compilation.GetSemanticModel(syntaxTree);

                    var declarations = root.DescendantNodes()
                        .Where(node => node is ClassDeclarationSyntax or 
                                      StructDeclarationSyntax or 
                                      InterfaceDeclarationSyntax or 
                                      EnumDeclarationSyntax or
                                      RecordDeclarationSyntax)
                        .Cast<BaseTypeDeclarationSyntax>();

                    foreach (var decl in declarations)
                    {
                        var symbol = semanticModel.GetDeclaredSymbol(decl);
                        if (symbol == null) continue;

                        var fullName = symbol.ToDisplayString();
                        var isPartial = decl.Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword));

                        if (!typeDeclarations.ContainsKey(fullName))
                            typeDeclarations[fullName] = new List<(string, bool, TypeKind)>();

                        typeDeclarations[fullName].Add((document.FilePath ?? "Unknown", isPartial, symbol.TypeKind));
                    }
                }
            }

            var duplicateTypes = typeDeclarations
                .Where(kvp => kvp.Value.Count > 1 && kvp.Value.Any(v => !v.isPartial))
                .Take(maxResults)
                .Select(kvp => new
                {
                    typeName = kvp.Key,
                    declarationCount = kvp.Value.Count,
                    nonPartialCount = kvp.Value.Count(v => !v.isPartial),
                    declarations = kvp.Value.Select(v => new
                    {
                        file = Path.GetFileName(v.file),
                        fullPath = v.file,
                        isPartial = v.isPartial,
                        kind = v.kind.ToString()
                    })
                })
                .ToList();

            return JsonSerializer.Serialize(new
            {
                success = true,
                sessionId,
                duplicateTypeCount = duplicateTypes.Count,
                duplicateTypes
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
    [Description("Deep dive into specific type before refactoring. Shows all members, files, inheritance. Essential before making changes to understand full impact and dependencies")]
    public async Task<string> GetTypeDetailsAsync(
        [Description("Session ID")] string sessionId,
        [Description("Full type name (e.g., 'MyNamespace.MyClass')")] string fullTypeName)
    {
        try
        {
            var session = _sessionManager.GetSession(sessionId);
            if (session == null)
                throw new ArgumentException("Session not found", nameof(sessionId));

            foreach (var project in session.Solution.Projects)
            {
                var compilation = await project.GetCompilationAsync();
                if (compilation == null) continue;

                var symbol = compilation.GetTypeByMetadataName(fullTypeName);
                if (symbol != null)
                {
                    var typeInfo = CreateDetailedTypeInfo(symbol);
                    if (typeInfo != null)
                    {
                        return JsonSerializer.Serialize(new
                        {
                            success = true,
                            sessionId,
                            typeInfo = new
                            {
                                name = typeInfo.Name,
                                fullName = typeInfo.FullName,
                                @namespace = typeInfo.Namespace,
                                kind = typeInfo.Kind.ToString(),
                                isGeneric = typeInfo.IsGeneric,
                                genericParameters = typeInfo.GenericParameters,
                                isPartial = typeInfo.IsPartial,
                                declarationCount = typeInfo.DeclarationCount,
                                declarationFiles = typeInfo.DeclarationFiles.Select(Path.GetFileName),
                                memberCount = typeInfo.Members.Count,
                                members = typeInfo.Members.Select(m => new
                                {
                                    name = m.Name,
                                    type = m.Type,
                                    kind = m.Kind.ToString(),
                                    signature = m.Signature,
                                    isStatic = m.IsStatic,
                                    isPublic = m.IsPublic,
                                    containingFile = Path.GetFileName(m.ContainingFile),
                                    line = m.Line
                                })
                            }
                        }, new JsonSerializerOptions { WriteIndented = true });
                    }
                }
            }

            return JsonSerializer.Serialize(new
            {
                success = false,
                error = $"Type '{fullTypeName}' not found in solution"
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
    [Description("Find types for refactoring or understanding codebase structure. Use patterns like '*Service', '*Controller'. Perfect for 'Where is class X?' or architectural analysis")]
    public async Task<string> SearchTypesAsync(
        [Description("Session ID")] string sessionId,
        [Description("Type name pattern to search for")] string namePattern,
        [Description("Include namespaces in search")] bool includeNamespace = false,
        [Description("Maximum results to return")] int maxResults = 100)
    {
        try
        {
            var session = _sessionManager.GetSession(sessionId);
            if (session == null)
                throw new ArgumentException("Session not found", nameof(sessionId));

            var matchingTypes = new List<TypeInfoModel>();

            foreach (var project in session.Solution.Projects)
            {
                var compilation = await project.GetCompilationAsync();
                if (compilation == null) continue;

                foreach (var document in project.Documents)
                {
                    var syntaxTree = await document.GetSyntaxTreeAsync();
                    if (syntaxTree == null) continue;

                    var root = await syntaxTree.GetRootAsync();
                    var semanticModel = compilation.GetSemanticModel(syntaxTree);

                    var typeDeclarations = root.DescendantNodes()
                        .Where(node => node is ClassDeclarationSyntax or 
                                      StructDeclarationSyntax or 
                                      InterfaceDeclarationSyntax or 
                                      EnumDeclarationSyntax or
                                      RecordDeclarationSyntax)
                        .Cast<BaseTypeDeclarationSyntax>();

                    foreach (var typeDecl in typeDeclarations)
                    {
                        var symbol = semanticModel.GetDeclaredSymbol(typeDecl);
                        if (symbol == null) continue;

                        var searchText = includeNamespace ? symbol.ToDisplayString() : symbol.Name;
                        if (searchText.Contains(namePattern, StringComparison.OrdinalIgnoreCase))
                        {
                            var typeInfo = CreateTypeInfo(symbol, document.FilePath ?? string.Empty);
                            if (typeInfo != null)
                            {
                                matchingTypes.Add(typeInfo);
                            }
                        }
                    }
                }
            }

            var results = matchingTypes
                .Take(maxResults)
                .OrderBy(t => t.Name, StringComparer.Ordinal)
                .ToList();

            return JsonSerializer.Serialize(new
            {
                success = true,
                sessionId,
                searchPattern = namePattern,
                matchCount = results.Count,
                types = results.Select(t => new
                {
                    name = t.Name,
                    fullName = t.FullName,
                    @namespace = t.Namespace,
                    kind = t.Kind.ToString(),
                    containingFile = Path.GetFileName(t.ContainingFile),
                    isGeneric = t.IsGeneric,
                    memberCount = t.Members.Count
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
    [Description("Codebase overview and architecture analysis. Shows type distribution by kind, namespace, generics. Useful for understanding solution structure and planning refactoring")]
    public async Task<string> GetTypeStatisticsAsync(
        [Description("Session ID")] string sessionId)
    {
        try
        {
            var session = _sessionManager.GetSession(sessionId);
            if (session == null)
                throw new ArgumentException("Session not found", nameof(sessionId));

            var typeStats = new Dictionary<TypeKind, int>();
            var namespaceStats = new Dictionary<string, int>(StringComparer.Ordinal);
            var genericTypeCount = 0;
            var partialTypeCount = 0;

            foreach (var project in session.Solution.Projects)
            {
                var compilation = await project.GetCompilationAsync();
                if (compilation == null) continue;

                foreach (var document in project.Documents)
                {
                    var syntaxTree = await document.GetSyntaxTreeAsync();
                    if (syntaxTree == null) continue;

                    var root = await syntaxTree.GetRootAsync();
                    var semanticModel = compilation.GetSemanticModel(syntaxTree);

                    var typeDeclarations = root.DescendantNodes()
                        .Where(node => node is ClassDeclarationSyntax or 
                                      StructDeclarationSyntax or 
                                      InterfaceDeclarationSyntax or 
                                      EnumDeclarationSyntax or
                                      RecordDeclarationSyntax)
                        .Cast<BaseTypeDeclarationSyntax>();

                    foreach (var typeDecl in typeDeclarations)
                    {
                        var symbol = semanticModel.GetDeclaredSymbol(typeDecl);
                        if (symbol == null) continue;

                        typeStats[symbol.TypeKind] = typeStats.GetValueOrDefault(symbol.TypeKind, 0) + 1;
                        
                        var namespaceName = symbol.ContainingNamespace?.ToDisplayString() ?? "<global>";
                        namespaceStats[namespaceName] = namespaceStats.GetValueOrDefault(namespaceName, 0) + 1;

                        if (symbol.IsGenericType) genericTypeCount++;
                        
                        if (typeDecl.Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword)))
                            partialTypeCount++;
                    }
                }
            }

            return JsonSerializer.Serialize(new
            {
                success = true,
                sessionId,
                statistics = new
                {
                    totalTypes = typeStats.Values.Sum(),
                    genericTypes = genericTypeCount,
                    partialTypes = partialTypeCount,
                    byKind = typeStats.ToDictionary(kvp => kvp.Key.ToString(), kvp => kvp.Value, StringComparer.Ordinal),
                    byNamespace = namespaceStats
                        .OrderByDescending(kvp => kvp.Value)
                        .Take(20)
                        .ToDictionary(kvp => kvp.Key, kvp => kvp.Value, StringComparer.Ordinal),
                    namespaceCount = namespaceStats.Count
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

    private static Models.TypeInfoModel? CreateTypeInfo(INamedTypeSymbol symbol, string filePath)
    {
        return new Models.TypeInfoModel
        {
            Name = symbol.Name,
            FullName = symbol.ToDisplayString(),
            Namespace = symbol.ContainingNamespace?.ToDisplayString() ?? string.Empty,
            AssemblyName = symbol.ContainingAssembly?.Name ?? string.Empty,
            ContainingFile = filePath,
            Kind = symbol.TypeKind,
            IsGeneric = symbol.IsGenericType,
            GenericParameters = symbol.TypeParameters.Select(tp => tp.Name).ToList(),
            Members = GetTypeMembers(symbol, filePath),
            DeclarationCount = symbol.DeclaringSyntaxReferences.Length,
            IsPartial = symbol.DeclaringSyntaxReferences.Any(sr => 
                sr.GetSyntax() is BaseTypeDeclarationSyntax decl && 
                decl.Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword))),
            DeclarationFiles = symbol.DeclaringSyntaxReferences
                .Select(sr => sr.SyntaxTree.FilePath ?? "Unknown")
                .Distinct(StringComparer.Ordinal)
                .ToList()
        };
    }

    private static Models.TypeInfoModel? CreateDetailedTypeInfo(INamedTypeSymbol symbol)
    {
        var filePath = symbol.DeclaringSyntaxReferences.FirstOrDefault()?.SyntaxTree.FilePath ?? string.Empty;
        return CreateTypeInfo(symbol, filePath);
    }

    private static List<MemberInfo> GetTypeMembers(INamedTypeSymbol symbol, string filePath)
    {
        var members = new List<MemberInfo>();

        foreach (var member in symbol.GetMembers())
        {
            var memberInfo = CreateMemberInfo(member, filePath);
            if (memberInfo != null)
                members.Add(memberInfo);
        }

        return members;
    }

    private static MemberInfo? CreateMemberInfo(ISymbol symbol, string defaultFilePath)
    {
        var location = symbol.Locations.FirstOrDefault();
        var filePath = location?.SourceTree?.FilePath ?? defaultFilePath;
        var line = location?.GetLineSpan().StartLinePosition.Line + 1 ?? 0;

        var kind = symbol.Kind switch
        {
            SymbolKind.Field => MemberKind.Field,
            SymbolKind.Property => MemberKind.Property,
            SymbolKind.Method => symbol.Name == ".ctor" ? MemberKind.Constructor : MemberKind.Method,
            SymbolKind.Event => MemberKind.Event,
            _ => MemberKind.Method
        };

        return new MemberInfo
        {
            Name = symbol.Name,
            Type = symbol.Kind == SymbolKind.Method ? ((IMethodSymbol)symbol).ReturnType.ToDisplayString() :
                   symbol.Kind == SymbolKind.Property ? ((IPropertySymbol)symbol).Type.ToDisplayString() :
                   symbol.Kind == SymbolKind.Field ? ((IFieldSymbol)symbol).Type.ToDisplayString() :
                   symbol.Kind == SymbolKind.Event ? ((IEventSymbol)symbol).Type.ToDisplayString() :
                   "Unknown",
            Kind = kind,
            Signature = symbol.ToDisplayString(SymbolDisplayFormat.CSharpShortErrorMessageFormat),
            IsStatic = symbol.IsStatic,
            IsPublic = symbol.DeclaredAccessibility == Accessibility.Public,
            ContainingFile = filePath,
            Line = line
        };
    }
}