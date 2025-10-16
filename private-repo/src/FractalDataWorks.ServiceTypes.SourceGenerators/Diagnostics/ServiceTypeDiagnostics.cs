using Microsoft.CodeAnalysis;

namespace FractalDataWorks.ServiceTypes.SourceGenerators.Diagnostics;

/// <summary>
/// Defines diagnostic descriptors for ServiceType source generation and analysis.
/// These descriptors are shared between the source generator and analyzer for consistent reporting.
/// </summary>
internal static class ServiceTypeDiagnostics
{
    /// <summary>
    /// Diagnostic descriptor for abstract properties in base types.
    /// </summary>
    public static readonly DiagnosticDescriptor AbstractPropertyInBaseTypeRule = new(
        id: "ST006",
        title: "Abstract properties not allowed in ServiceType base types",
        messageFormat: "The base type '{0}' contains abstract property '{1}'. ServiceType base types must not have abstract properties - use constructor parameters to pass property values instead.",
        category: "ServiceTypes",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "ServiceType base types should only have abstract methods, not abstract properties. All properties should be set via constructor parameters.");

    /// <summary>
    /// Diagnostic descriptor for ServiceType collection generation failures.
    /// </summary>
    public static readonly DiagnosticDescriptor GenerationFailureRule = new(
        id: "STCG001",
        title: "ServiceType Collection Generation Failed",
        messageFormat: "Failed to generate ServiceType collection {0}: {1}",
        category: "ServiceTypeCollectionGenerator",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "An exception occurred during ServiceType collection generation. Check the error message for details.");
}
