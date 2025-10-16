using FractalDataWorks.ServiceTypes.SourceGenerators.Models;
using Microsoft.CodeAnalysis;
using System;using System.Collections.Generic;

namespace FractalDataWorks.ServiceTypes.SourceGenerators.Generators;

/// <summary>
/// Result structure for batched service type discovery.
/// </summary>
internal readonly record struct ServiceTypeCollectionResult(
    EnumTypeInfoModel Definition,
    Compilation Compilation,
    IList<INamedTypeSymbol> ServiceTypes,
    INamedTypeSymbol CollectionClass);