using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;
using FractalDataWorks.SourceGenerators.Models;

namespace FractalDataWorks.Messages.SourceGenerators.Models;

/// <summary>
/// Helper class to carry MessageTypeInfoModel with its compilation context and discovered option types.
/// </summary>
/// <remarks>
/// This code is excluded from code coverage because source generators run at compile-time and cannot be unit tested via runtime tests.
/// </remarks>

internal sealed class MessageTypeInfoWithCompilation
{
    public CollectionTypeInfoModel CollectionTypeInfoModel { get; }
    public Compilation Compilation { get; }
    public List<INamedTypeSymbol> DiscoveredOptionTypes { get; }

    public MessageTypeInfoWithCompilation(CollectionTypeInfoModel collectionTypeInfoModel, Compilation compilation, List<INamedTypeSymbol> discoveredOptionTypes)
    {
        CollectionTypeInfoModel = collectionTypeInfoModel;
        Compilation = compilation;
        DiscoveredOptionTypes = discoveredOptionTypes;
    }
}
