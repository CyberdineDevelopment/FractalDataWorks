using System;
using System.Collections.Generic;
using System.Linq;
using FractalDataWorks.EnhancedEnums.Models;
using FractalDataWorks.EnhancedEnums.Attributes;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace FractalDataWorks.EnhancedEnums.Services;

/// <summary>
/// Service responsible for discovering enum values using attribute-based discovery.
/// </summary>
public static class EnumValueDiscoveryService
{
    /// <summary>
    /// Discovers all enum values for a given enum definition.
    /// </summary>
    public static IList<EnumValueInfoModel> DiscoverEnumValues(EnumTypeInfoModel def, INamedTypeSymbol baseTypeSymbol, Compilation compilation, SourceProductionContext context)
    {
        List<EnumValueInfoModel> values = [];

        // Scan current compilation only (like TypeCollectionGenerator)
        var allTypes = ScanCurrentCompilation(compilation);

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
                if (!HasEnumOptionAttribute(typeDecl))
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


    private static void ProcessTypeForEnumValues(INamedTypeSymbol typeSymbol, EnumTypeInfoModel def, INamedTypeSymbol baseTypeSymbol, List<EnumValueInfoModel> values)
    {
        var attrs = typeSymbol.GetAttributes()
            .Where(ad => string.Equals(ad.AttributeClass?.Name, nameof(EnumOptionAttribute), StringComparison.Ordinal) ||
                        string.Equals(ad.AttributeClass?.Name, "EnumOption", StringComparison.Ordinal))
            .ToList();

        if (attrs.Count == 0)
            return;

        // Process each EnumOption attribute separately (for multiple collections support)
        foreach (var attr in attrs)
        {
            var named = attr.NamedArguments.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            var name = named.TryGetValue(nameof(EnumOptionAttribute.Name), out var n) && n.Value is string ns
                ? ns : typeSymbol.Name;

            // Check if this enum option specifies a collection name (may be a future feature)
            var collectionName = named.TryGetValue("CollectionName", out var cn) && cn.Value is string cns
                ? cns : null;

            // If a collection name is specified, only include this option in matching collections
            if (!string.IsNullOrEmpty(collectionName) && !string.Equals(collectionName, def.CollectionName, StringComparison.OrdinalIgnoreCase))
                continue;

            if (MatchesDefinition(typeSymbol, baseTypeSymbol))
            {
                var info = new EnumValueInfoModel
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
    /// Checks if a type declaration has the EnumOption attribute.
    /// </summary>
    private static bool HasEnumOptionAttribute(TypeDeclarationSyntax syntax)
    {
        return syntax.AttributeLists
            .SelectMany(al => al.Attributes)
            .Any(attr => attr.Name.ToString().Contains("EnumOption"));
    }

    /// <summary>
    /// Extracts namespaces from an option type's inheritance hierarchy.
    /// </summary>
    private static void ExtractNamespacesFromOptionType(INamedTypeSymbol optionType, INamedTypeSymbol baseTypeSymbol, EnumTypeInfoModel def)
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
