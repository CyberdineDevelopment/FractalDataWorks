using System.Collections.Generic;
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
    public List<INamedTypeSymbol> DiscoveredServiceTypes { get; }
    public INamedTypeSymbol CollectionClass { get; }

    public ServiceTypeInfoWithCompilation(EnumTypeInfoModel serviceTypeInfoModel, Compilation compilation, List<INamedTypeSymbol> discoveredServiceTypes, INamedTypeSymbol collectionClass)
    {
        ServiceTypeInfoModel = serviceTypeInfoModel;
        Compilation = compilation;
        DiscoveredServiceTypes = discoveredServiceTypes;
        CollectionClass = collectionClass;
    }

    public void Deconstruct(out EnumTypeInfoModel serviceTypeInfoModel, out Compilation compilation, out List<INamedTypeSymbol> discoveredServiceTypes, out INamedTypeSymbol collectionClass)
    {
        serviceTypeInfoModel = ServiceTypeInfoModel;
        compilation = Compilation;
        discoveredServiceTypes = DiscoveredServiceTypes;
        collectionClass = CollectionClass;
    }
}