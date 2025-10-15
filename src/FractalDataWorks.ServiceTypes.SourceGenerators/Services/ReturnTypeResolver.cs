using System;
using System.Linq;
using FractalDataWorks.ServiceTypes.SourceGenerators.Models;
using Microsoft.CodeAnalysis;

namespace FractalDataWorks.ServiceTypes.SourceGenerators.Services;

/// <summary>
/// Resolves the effective return type for enum collections.
/// </summary>
public static class ReturnTypeResolver
{
    /// <summary>
    /// Resolves the effective return type for a collection based on various settings and constraints.
    /// </summary>
    public static string ResolveEffectiveReturnType(EnumTypeInfoModel definition, INamedTypeSymbol? baseTypeSymbol)
    {
        // Priority 1: Explicit ReturnType from attribute
        if (!string.IsNullOrEmpty(definition.ReturnType))
        {
            return definition.ReturnType!;
        }

        // Priority 2: For generic types, use DefaultGenericReturnType
        if (definition.IsGenericType && !string.IsNullOrEmpty(definition.DefaultGenericReturnType))
        {
            return definition.DefaultGenericReturnType!;
        }

        // Priority 3: For generic types, try to infer from constraints
        if (definition.IsGenericType && baseTypeSymbol != null)
        {
            var constraintType = GetReturnTypeFromConstraints(baseTypeSymbol, definition);
            if (constraintType != null)
            {
                return constraintType;
            }
        }

        // Priority 4: For non-generic types, use the full type name
        if (!definition.IsGenericType)
        {
            return definition.FullTypeName;
        }

#pragma warning disable CS0618 // Type or member is obsolete
        // FUTURE: Complex solution - resolve generic type arguments from constraints
        // When the base type is generic (e.g., ConnectionTypeBase<TService, TConfiguration, TFactory>)
        // and we need the concrete type (not just the interface), we should:
        // 1. Examine the type parameter constraints on the generic base type
        // 2. Search the derived class to find types that satisfy each constraint
        // 3. Substitute actual types for the type parameters to construct the fully closed generic type
        // Current approach: Use interface type (from returnType parameter) to avoid this complexity
        // See: GenericTypeHelper.GetTypeConstraints() for constraint inspection utilities
#pragma warning restore CS0618 // Type or member is obsolete

        // Fallback: Use object for generic types without better options
        return "object";
    }

    /// <summary>
    /// Attempts to get a suitable return type from generic type constraints.
    /// </summary>
    public static string? GetReturnTypeFromConstraints(INamedTypeSymbol baseTypeSymbol, EnumTypeInfoModel definition)
    {
        if (!baseTypeSymbol.IsGenericType)
        {
            return null;
        }

        foreach (var typeParam in baseTypeSymbol.TypeParameters)
        {
            // Look for interface constraints first
            var interfaceConstraint = typeParam.ConstraintTypes
                .OfType<INamedTypeSymbol>()
                .FirstOrDefault(t => t.TypeKind == TypeKind.Interface);

            if (interfaceConstraint != null)
            {
                return interfaceConstraint.ToDisplayString();
            }

            // Then look for class constraints
            var classConstraint = typeParam.ConstraintTypes
                .OfType<INamedTypeSymbol>()
                .FirstOrDefault(t => t.TypeKind == TypeKind.Class);

            if (classConstraint != null)
            {
                return classConstraint.ToDisplayString();
            }
        }

        return null;
    }

    /// <summary>
    /// Detects if there's a common interface implemented by enum values and uses it as return type.
    /// </summary>
    public static string? DetectCommonInterface(INamedTypeSymbol valueType)
    {
        // Get all interfaces implemented by the type
        var interfaces = valueType.AllInterfaces
            .Where(i => !IsSystemInterface(i))
            .ToList();

        // Return the first non-system interface if any
        return interfaces.FirstOrDefault()?.ToDisplayString();
    }

    private static bool IsSystemInterface(INamedTypeSymbol interfaceSymbol)
    {
        var ns = interfaceSymbol.ContainingNamespace?.ToDisplayString() ?? "";
        return ns.StartsWith(nameof(System), StringComparison.Ordinal) || 
               ns.StartsWith(nameof(Microsoft), StringComparison.Ordinal);
    }
}
