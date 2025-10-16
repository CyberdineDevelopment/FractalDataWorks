using Microsoft.CodeAnalysis;

namespace FractalDataWorks.Messages.SourceGenerators.Discovery;

/// <summary>
/// Symbol extension methods for metadata and helper operations.
/// </summary>
/// <remarks>
/// This code is excluded from code coverage because source generators run at compile-time and cannot be unit tested via runtime tests.
/// </remarks>

internal static class SymbolExtensions
{
    /// <summary>
    /// Gets the full metadata name of the symbol.
    /// </summary>
    /// <param name="symbol">Symbol.</param>
    /// <returns>Full metadata name.</returns>
    public static string GetFullMetadataName(this ISymbol symbol)
    {
        if (symbol == null)
            return string.Empty;

        var containingNamespace = symbol.ContainingNamespace;
        var namespaceName = containingNamespace == null || containingNamespace.IsGlobalNamespace
            ? string.Empty
            : containingNamespace.GetFullMetadataName();

        return string.IsNullOrEmpty(namespaceName)
            ? symbol.MetadataName
            : $"{namespaceName}.{symbol.MetadataName}";
    }
}
