using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using FractalDataWorks.Collections.SourceGenerators.Diagnostics;

namespace FractalDataWorks.Collections.SourceGenerators.Analyzers;

/// <summary>
/// Stub analyzer that registers diagnostic rules used by TypeCollectionGenerator.
/// This analyzer doesn't perform any actual analysis - it exists solely to register
/// the diagnostic rules so they're recognized by the analyzer release tracking system.
/// The actual diagnostics are reported by the TypeCollectionGenerator source generator.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class TypeCollectionDiagnosticsAnalyzer : DiagnosticAnalyzer
{
    /// <summary>
    /// Gets the supported diagnostic descriptors for this analyzer.
    /// </summary>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(
            TypeCollectionDiagnostics.AbstractPropertyInBaseTypeRule,
            TypeCollectionDiagnostics.GenerationFailureRule);

    /// <summary>
    /// Initializes the analyzer.
    /// This analyzer doesn't register any analysis actions because the diagnostics
    /// are reported by TypeCollectionGenerator, not by this analyzer.
    /// </summary>
    /// <param name="context">The analysis context.</param>
    public override void Initialize(AnalysisContext context)
    {
        // No analysis actions needed - this is just for diagnostic registration
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
    }
}
