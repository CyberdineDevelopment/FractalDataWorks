using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using FractalDataWorks.Messages.SourceGenerators.Discovery;
using FractalDataWorks.Messages.SourceGenerators.Models;

namespace FractalDataWorks.Messages.SourceGenerators.Services;

/// <summary>
/// Service responsible for discovering enum values from current compilation and referenced assemblies.
/// </summary>
public static class MessageValueDiscoveryService
{
    private static readonly CrossAssemblyTypeDiscoveryService _discoveryService = new();

    /// <summary>
    /// Discovers all enum values for a given enum definition.
    /// </summary>
    public static IList<MessageValueInfoModel> DiscoverEnumValues(MessageTypeInfoModel def, INamedTypeSymbol baseTypeSymbol, Compilation compilation, SourceProductionContext context)
    {
        List<MessageValueInfoModel> values = [];
        List<INamedTypeSymbol> allTypes = [];

        // Step 1: Scan current compilation
        allTypes.AddRange(ScanCurrentCompilation(compilation));

        // Step 2: Scan referenced assemblies if enabled
        if (def.IncludeReferencedAssemblies)
        {
            var crossAssemblyTypes = ScanReferencedAssemblies(compilation, context);
            allTypes.AddRange(crossAssemblyTypes);
        }

        // Process all found types
        foreach (var typeSymbol in allTypes)
        {
            ProcessTypeForEnumValues(typeSymbol, def, baseTypeSymbol, values);
        }

        return values;
    }

    private static List<INamedTypeSymbol> ScanCurrentCompilation(Compilation compilation)
    {
        List<INamedTypeSymbol> allTypes = [];

        foreach (var tree in compilation.SyntaxTrees)
        {
            var root = tree.GetRoot();
            var model = compilation.GetSemanticModel(tree);
            
            // Find all type declarations (classes, structs, records)
            foreach (var typeDecl in root.DescendantNodes().OfType<TypeDeclarationSyntax>())
            {
                if (!HasMessageAttribute(typeDecl))
                    continue;

                var symbol = model.GetDeclaredSymbol(typeDecl) as INamedTypeSymbol;
                if (symbol != null)
                {
                    allTypes.Add(symbol);
                }
            }
        }

        return allTypes;
    }

    private static IEnumerable<INamedTypeSymbol> ScanReferencedAssemblies(Compilation compilation, SourceProductionContext context)
    {
        // Use the discovery service to find types with MessageOption attribute by name
        var typesWithAttribute = _discoveryService.FindTypesWithAttributeName(nameof(MessageAttribute), compilation);
        
        // Report diagnostic about cross-assembly scanning
        var typeCount = typesWithAttribute.Count();
        context.ReportDiagnostic(Diagnostic.Create(
            new DiagnosticDescriptor(
                "ENH_INFO_001",
                "Cross-assembly scan complete",
                $"Found {typeCount} types with MessageOption attribute from assemblies that opted in",
                "EnhancedMessageOptions",
                DiagnosticSeverity.Info,
                isEnabledByDefault: true),
            null));

        return typesWithAttribute;
    }

    private static void ProcessTypeForEnumValues(INamedTypeSymbol typeSymbol, MessageTypeInfoModel def, INamedTypeSymbol baseTypeSymbol, List<MessageValueInfoModel> values)
    {
        var attrs = typeSymbol.GetAttributes()
            .Where(ad => string.Equals(ad.AttributeClass?.Name, nameof(MessageAttribute), StringComparison.Ordinal) ||
                        string.Equals(ad.AttributeClass?.Name, "MessageOption", StringComparison.Ordinal))
            .ToList();

        if (attrs.Count == 0)
            return;

        // Process each MessageOption attribute separately (for multiple collections support)
        foreach (var attr in attrs)
        {
            var named = attr.NamedArguments.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            var name = named.TryGetValue(nameof(MessageAttribute.Name), out var n) && n.Value is string ns
                ? ns : typeSymbol.Name;

            // Check if this enum option specifies a collection name (may be a future feature)
            var collectionName = named.TryGetValue("CollectionName", out var cn) && cn.Value is string cns
                ? cns : null;

            // If a collection name is specified, only include this option in matching collections
            if (!string.IsNullOrEmpty(collectionName) && !string.Equals(collectionName, def.CollectionName, StringComparison.OrdinalIgnoreCase))
                continue;

            if (MatchesDefinition(typeSymbol, baseTypeSymbol))
            {
                var info = new MessageValueInfoModel
                {
                    ShortTypeName = typeSymbol.Name,
                    FullTypeName = typeSymbol.ToDisplayString(),
                    Name = name,
                };
                values.Add(info);
                
                // Extract namespaces from the option type's base type arguments
                ExtractNamespacesFromOptionType(typeSymbol, baseTypeSymbol, def);
                
                break; // Only add once per type per collection, even if multiple matching attributes
            }
        }
    }

    /// <summary>
    /// Determines whether an enum value belongs to a given enum definition.
    /// </summary>
    private static bool MatchesDefinition(INamedTypeSymbol valueSymbol, INamedTypeSymbol baseTypeSymbol)
    {
        // interface implementation
        if (valueSymbol.AllInterfaces.Any(i => SymbolEqualityComparer.Default.Equals(i, baseTypeSymbol)))
        {
            return true;
        }

        // base type inheritance
        for (var baseType = valueSymbol.BaseType; baseType != null; baseType = baseType.BaseType)
        {
            if (SymbolEqualityComparer.Default.Equals(baseType, baseTypeSymbol))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Checks if a type declaration has the MessageOption attribute.
    /// </summary>
    private static bool HasMessageAttribute(TypeDeclarationSyntax syntax)
    {
        return syntax.AttributeLists
            .SelectMany(al => al.Attributes)
            .Any(attr => attr.Name.ToString().Contains("MessageOption"));
    }

    /// <summary>
    /// Extracts namespaces from an option type's inheritance hierarchy.
    /// </summary>
    private static void ExtractNamespacesFromOptionType(INamedTypeSymbol optionType, INamedTypeSymbol baseTypeSymbol, MessageTypeInfoModel def)
    {
        // Extract namespaces from the option type itself
        ExtractNamespacesFromType(optionType, def.RequiredNamespaces);
        
        // Walk up the inheritance chain to find the base type
        for (var current = optionType.BaseType; current != null; current = current.BaseType)
        {
            // If this is a generic type, extract namespaces from type arguments
            if (current.IsGenericType)
            {
                foreach (var typeArg in current.TypeArguments)
                {
                    ExtractNamespacesFromType(typeArg, def.RequiredNamespaces);
                }
            }
            
            // If we've reached the base type, stop
            // For generic types, we need to compare the unbound/original definition
            var currentToCompare = current.IsGenericType ? current.OriginalDefinition : current;
            var baseToCompare = baseTypeSymbol.IsGenericType ? baseTypeSymbol.OriginalDefinition : baseTypeSymbol;
            
            if (SymbolEqualityComparer.Default.Equals(currentToCompare, baseToCompare))
            {
                break;
            }
        }
        
        // Also extract from interfaces in case the base type is an interface
        foreach (var iface in optionType.AllInterfaces)
        {
            if (iface.IsGenericType)
            {
                foreach (var typeArg in iface.TypeArguments)
                {
                    ExtractNamespacesFromType(typeArg, def.RequiredNamespaces);
                }
            }
        }
    }

    /// <summary>
    /// Extracts namespace information from a type symbol.
    /// </summary>
    private static void ExtractNamespacesFromType(ITypeSymbol type, ISet<string> namespaces)
    {
        if (type.ContainingNamespace != null && !type.ContainingNamespace.IsGlobalNamespace)
        {
            namespaces.Add(type.ContainingNamespace.ToDisplayString());
        }
        
        // Handle generic type arguments
        if (type is INamedTypeSymbol namedType && namedType.IsGenericType)
        {
            foreach (var arg in namedType.TypeArguments)
            {
                ExtractNamespacesFromType(arg, namespaces);
            }
        }
    }
}
