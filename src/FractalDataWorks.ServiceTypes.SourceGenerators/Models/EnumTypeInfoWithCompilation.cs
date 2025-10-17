using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using FractalDataWorks.ServiceTypes.SourceGenerators.Models;
using Microsoft.CodeAnalysis;

namespace FractalDataWorks.ServiceTypes.SourceGenerators.Models;

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

    public EnumTypeInfoWithCompilation(EnumTypeInfoModel enumTypeInfoModel, Compilation compilation, List<INamedTypeSymbol> discoveredOptionTypes, INamedTypeSymbol collectionClass)
    {
        EnumTypeInfoModel = enumTypeInfoModel;
        Compilation = compilation;
        DiscoveredOptionTypes = discoveredOptionTypes;
        CollectionClass = collectionClass;
    }

    public void Deconstruct(out EnumTypeInfoModel enumTypeInfoModel, out Compilation compilation, out List<INamedTypeSymbol> discoveredOptionTypes, out INamedTypeSymbol collectionClass)
    {
        enumTypeInfoModel = EnumTypeInfoModel;
        compilation = Compilation;
        discoveredOptionTypes = DiscoveredOptionTypes;
        collectionClass = CollectionClass;
    }
}
