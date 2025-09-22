using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using FractalDataWorks.ServiceTypes.SourceGenerators.Models;

namespace FractalDataWorks.ServiceTypes.SourceGenerators.Generators;

/// <summary>
/// Result structure for batched service type discovery.
/// </summary>
internal readonly record struct ServiceTypeCollectionResult(
    EnumTypeInfoModel Definition,
    Compilation Compilation,
    IList<INamedTypeSymbol> ServiceTypes,
    INamedTypeSymbol CollectionClass);