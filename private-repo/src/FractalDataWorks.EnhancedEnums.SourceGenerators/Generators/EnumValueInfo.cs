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

internal sealed class EnumValueInfo
{
    public string Name { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public int IntValue { get; set; }
}