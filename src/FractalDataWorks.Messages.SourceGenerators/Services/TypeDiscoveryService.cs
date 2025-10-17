using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using FractalDataWorks.Messages.SourceGenerators.Models;

namespace FractalDataWorks.Messages.SourceGenerators.Services;

/// <summary>
/// Discovers enum option types by finding classes that inherit from enhanced enum base classes.
/// MessageOption attributes are used for configuration but inheritance determines inclusion.
/// </summary>
/// <remarks>
/// This code is excluded from code coverage because source generators run at compile-time and cannot be unit tested via runtime tests.
/// </remarks>

public static class TypeDiscoveryService
{
    /// <summary>
    /// Discovers all enum option types for the given enum definition by finding classes that inherit from the base class.
    /// </summary>
    public static IList<INamedTypeSymbol> DiscoverMessageOptions(Compilation compilation, MessageTypeInfoModel definition)
    {
        // Find the base type symbol in the current compilation first
        var baseTypeSymbol = FindTypeInCurrentCompilation(compilation, definition);
        if (baseTypeSymbol == null)
            return new List<INamedTypeSymbol>();
            
        return ScanForInheritingTypes(compilation, baseTypeSymbol);
    }

    /// <summary>
    /// Scans the current compilation for types that inherit from the specified base type.
    /// </summary>
    public static IList<INamedTypeSymbol> ScanForInheritingTypes(Compilation compilation, INamedTypeSymbol baseType)
    {
        var types = new List<INamedTypeSymbol>();

        foreach (var tree in compilation.SyntaxTrees)
        {
            var root = tree.GetRoot();
            var model = compilation.GetSemanticModel(tree);
            
            // Find all type declarations (classes, structs, records)
            foreach (var typeDecl in root.DescendantNodes().OfType<TypeDeclarationSyntax>())
            {
                var symbol = model.GetDeclaredSymbol(typeDecl) as INamedTypeSymbol;
                if (symbol == null || symbol.IsAbstract)
                    continue;

                // Check if this type inherits from the base type
                if (InheritsFrom(symbol, baseType))
                {
                    types.Add(symbol);
                }
            }
        }

        return types;
    }

    /// <summary>
    /// Checks if a type inherits from the specified base type.
    /// Handles both generic and non-generic inheritance patterns.
    /// </summary>
    private static bool InheritsFrom(INamedTypeSymbol type, INamedTypeSymbol baseType)
    {
        var current = type.BaseType;
        while (current != null)
        {
            // Direct match (handles generic types with same type arguments)
            if (SymbolEqualityComparer.Default.Equals(current, baseType))
                return true;
                
            // For generic base types, also check if the generic definition matches
            if (baseType.IsGenericType && current.IsGenericType)
            {
                var currentGenericDef = current.ConstructedFrom ?? current;
                var baseGenericDef = baseType.ConstructedFrom ?? baseType;
                
                if (SymbolEqualityComparer.Default.Equals(currentGenericDef, baseGenericDef))
                    return true;
            }
            
            current = current.BaseType;
        }
        return false;
    }

    /// <summary>
    /// Finds a type in the current compilation by its full type name.
    /// </summary>
    private static INamedTypeSymbol? FindTypeInCurrentCompilation(Compilation compilation, MessageTypeInfoModel definition)
    {
        foreach (var tree in compilation.SyntaxTrees)
        {
            var root = tree.GetRoot();
            var model = compilation.GetSemanticModel(tree);
            
            foreach (var typeDecl in root.DescendantNodes().OfType<TypeDeclarationSyntax>())
            {
                var symbol = model.GetDeclaredSymbol(typeDecl) as INamedTypeSymbol;
                if (symbol == null)
                    continue;

                // Check if this matches our definition
                if (string.Equals(symbol.Name, definition.ClassName, StringComparison.Ordinal) && 
                    string.Equals(GetFullNamespace(symbol), definition.Namespace, StringComparison.Ordinal))
                {
                    return symbol;
                }
            }
        }
        
        return null;
    }

    /// <summary>
    /// Gets the full namespace of a symbol.
    /// </summary>
    private static string GetFullNamespace(ISymbol symbol)
    {
        List<string> parts = [];
        var current = symbol.ContainingNamespace;
        
        while (current != null && !current.IsGlobalNamespace)
        {
            parts.Insert(0, current.Name);
            current = current.ContainingNamespace;
        }
        
        return string.Join(".", parts);
    }

    /// <summary>
    /// Checks if a type declaration has the MessageOption attribute (for configuration).
    /// </summary>
    public static bool HasMessageAttribute(TypeDeclarationSyntax typeDecl)
    {
        return typeDecl.AttributeLists
            .SelectMany(al => al.Attributes)
            .Any(a => 
            {
                var name = a.Name.ToString();
                return name.Contains("MessageOption", StringComparison.Ordinal) && !name.Contains("EnhancedMessageOption", StringComparison.Ordinal);
            });
    }
}
