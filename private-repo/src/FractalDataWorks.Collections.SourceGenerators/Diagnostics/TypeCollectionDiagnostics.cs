using Microsoft.CodeAnalysis;

namespace FractalDataWorks.Collections.SourceGenerators.Diagnostics;

/// <summary>
/// Defines diagnostic descriptors for TypeCollection source generation and analysis.
/// These descriptors are shared between the source generator and analyzer for consistent reporting.
/// </summary>
internal static class TypeCollectionDiagnostics
{
    /// <summary>
    /// Diagnostic descriptor for abstract properties in base types.
    /// </summary>
    public static readonly DiagnosticDescriptor AbstractPropertyInBaseTypeRule = new(
        id: "TC006",
        title: "Abstract properties not allowed in TypeCollection base types",
        messageFormat: "The base type '{0}' contains abstract property '{1}'. TypeCollection base types must not have abstract properties - use constructor parameters to pass property values instead.",
        category: "TypeCollections",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "TypeCollection base types should only have abstract methods, not abstract properties. All properties should be set via constructor parameters.");

    /// <summary>
    /// Diagnostic descriptor for type collection generation failures.
    /// </summary>
    public static readonly DiagnosticDescriptor GenerationFailureRule = new(
        id: "TCG001",
        title: "Type Collection Generation Failed",
        messageFormat: "Failed to generate type collection {0}: {1}",
        category: "TypeCollectionGenerator",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "An exception occurred during type collection generation. Check the error message for details.");
}
