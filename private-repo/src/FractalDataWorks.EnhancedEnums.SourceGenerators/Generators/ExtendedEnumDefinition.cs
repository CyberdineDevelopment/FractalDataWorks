using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using FractalDataWorks.EnhancedEnums.ExtendedEnums.Attributes;

namespace FractalDataWorks.EnhancedEnums.SourceGenerators.Generators;

internal sealed class ExtendedEnumDefinition
{
    public string BaseClassName { get; set; } = string.Empty;
    public string BaseClassFullName { get; set; } = string.Empty;
    public string BaseClassNamespace { get; set; } = string.Empty;
    public INamedTypeSymbol? EnumType { get; set; }
    public IList<EnumValueInfo> EnumValues { get; set; } = new List<EnumValueInfo>();
    public IList<INamedTypeSymbol> CustomOptions { get; set; } = new List<INamedTypeSymbol>();
    public bool IsGlobal { get; set; }
    public string CollectionName { get; set; } = string.Empty;
    public StringComparison NameComparison { get; set; }
    public bool GenerateFactoryMethods { get; set; }
    public bool GenerateStaticCollection { get; set; }
    public bool UseSingletonInstances { get; set; }
    public bool Generic { get; set; }
}