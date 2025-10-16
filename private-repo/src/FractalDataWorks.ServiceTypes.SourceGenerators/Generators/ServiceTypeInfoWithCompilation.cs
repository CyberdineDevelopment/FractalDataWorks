using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using FractalDataWorks.ServiceTypes.SourceGenerators.Models;

namespace FractalDataWorks.ServiceTypes.SourceGenerators.Generators;

/// <summary>
/// Helper class to carry ServiceType info with compilation context.
/// </summary>
internal sealed class ServiceTypeInfoWithCompilation
{
    public EnumTypeInfoModel ServiceTypeInfoModel { get; }
    public Compilation Compilation { get; }
    public ImmutableArray<INamedTypeSymbol> DiscoveredServiceTypes { get; }
    public INamedTypeSymbol CollectionClass { get; }
    public List<Diagnostic> Diagnostics { get; }

    public ServiceTypeInfoWithCompilation(
        EnumTypeInfoModel serviceTypeInfoModel,
        Compilation compilation,
        ImmutableArray<INamedTypeSymbol> discoveredServiceTypes,
        INamedTypeSymbol collectionClass,
        List<Diagnostic>? diagnostics = null)
    {
        ServiceTypeInfoModel = serviceTypeInfoModel;
        Compilation = compilation;
        DiscoveredServiceTypes = discoveredServiceTypes;
        CollectionClass = collectionClass;
        Diagnostics = diagnostics ?? [];
    }

    public void Deconstruct(out EnumTypeInfoModel serviceTypeInfoModel, out Compilation compilation, out ImmutableArray<INamedTypeSymbol> discoveredServiceTypes, out INamedTypeSymbol collectionClass)
    {
        serviceTypeInfoModel = ServiceTypeInfoModel;
        compilation = Compilation;
        discoveredServiceTypes = DiscoveredServiceTypes;
        collectionClass = CollectionClass;
    }
}