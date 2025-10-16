using FractalDataWorks.Collections.Models;
using Microsoft.CodeAnalysis;
using System;using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace FractalDataWorks.Collections.SourceGenerators.Models;

/// <summary>
/// Helper class to carry EnumTypeInfoModel with its compilation context and discovered option types.
/// </summary>
/// <remarks>
/// This code is excluded from code coverage because source generators run at compile-time and cannot be unit tested via runtime tests.
/// </remarks>

internal sealed class EnumTypeInfoWithCompilation
{
    public EnumTypeInfoModel EnumTypeInfoModel { get; }
    public Compilation Compilation { get; }
    public List<INamedTypeSymbol> DiscoveredOptionTypes { get; }
    public INamedTypeSymbol CollectionClass { get; }
    public List<Diagnostic> Diagnostics { get; }

    public EnumTypeInfoWithCompilation(
        EnumTypeInfoModel enumTypeInfoModel,
        Compilation compilation,
        List<INamedTypeSymbol> discoveredOptionTypes,
        INamedTypeSymbol collectionClass,
        List<Diagnostic>? diagnostics = null)
    {
        EnumTypeInfoModel = enumTypeInfoModel;
        Compilation = compilation;
        DiscoveredOptionTypes = discoveredOptionTypes;
        CollectionClass = collectionClass;
        Diagnostics = diagnostics ?? [];
    }

    public void Deconstruct(
        out EnumTypeInfoModel enumTypeInfoModel,
        out Compilation compilation,
        out List<INamedTypeSymbol> discoveredOptionTypes,
        out INamedTypeSymbol collectionClass,
        out List<Diagnostic> diagnostics)
    {
        enumTypeInfoModel = EnumTypeInfoModel;
        compilation = Compilation;
        discoveredOptionTypes = DiscoveredOptionTypes;
        collectionClass = CollectionClass;
        diagnostics = Diagnostics;
    }
}
