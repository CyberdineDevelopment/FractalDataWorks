using Microsoft.CodeAnalysis;
using System;using System.Collections.Generic;

namespace FractalDataWorks.Messages.SourceGenerators.Discovery;

/// <summary>
/// Equality comparer for INamedTypeSymbol to be used in dictionary keys.
/// </summary>
/// <remarks>
/// This code is excluded from code coverage because source generators run at compile-time and cannot be unit tested via runtime tests.
/// </remarks>

internal sealed class TypeSymbolEqualityComparer : IEqualityComparer<(INamedTypeSymbol, Compilation)>
{
    /// <summary>
    /// Determines if two values are equal.
    /// </summary>
    /// <param name="x">First value.</param>
    /// <param name="y">Second value.</param>
    /// <returns>True if equal.</returns>
    public bool Equals((INamedTypeSymbol, Compilation) x, (INamedTypeSymbol, Compilation) y)
    {
        return SymbolEqualityComparer.Default.Equals(x.Item1, y.Item1) &&
               x.Item2.Equals(y.Item2);
    }

    /// <summary>
    /// Gets the hash code of the value.
    /// </summary>
    /// <param name="obj">Value.</param>
    /// <returns>Hash code.</returns>
    public int GetHashCode((INamedTypeSymbol, Compilation) obj)
    {
        return (SymbolEqualityComparer.Default.GetHashCode(obj.Item1) * 397) ^
               obj.Item2.GetHashCode();
    }
}
