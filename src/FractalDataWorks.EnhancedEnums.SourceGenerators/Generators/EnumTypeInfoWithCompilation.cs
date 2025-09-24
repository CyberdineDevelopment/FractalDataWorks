using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Text;
using FractalDataWorks.EnhancedEnums.Models;
using FractalDataWorks.EnhancedEnums.Discovery;
using FractalDataWorks.EnhancedEnums.Services;
using FractalDataWorks.EnhancedEnums.SourceGenerators.Services.Builders;
using FractalDataWorks.EnhancedEnums.Attributes;
using FractalDataWorks.EnhancedEnums.SourceGenerators.Models;
using FractalDataWorks.SourceGenerators.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace FractalDataWorks.EnhancedEnums.SourceGenerators.Generators;

/// <summary>
/// Internal model for passing data between discovery and generation phases.
/// </summary>
internal sealed record EnumTypeInfoWithCompilation(
    EnumTypeInfoModel EnumTypeInfoModel,
    Compilation Compilation,
    ImmutableArray<INamedTypeSymbol> DiscoveredOptionTypes);