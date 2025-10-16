using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using FractalDataWorks.EnhancedEnums.Models;
using FractalDataWorks.EnhancedEnums.SourceGenerators.Models;
using FractalDataWorks.SourceGenerators.Models;
using FractalDataWorks.EnhancedEnums.SourceGenerators.Services.Builders;
using FractalDataWorks.EnhancedEnums.ExtendedEnums.Attributes;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace FractalDataWorks.EnhancedEnums.SourceGenerators.Generators;

/// <summary>
/// ExtendedEnumCollectionGenerator - Creates collections for C# enums with enhanced behavior.
/// Auto-generates wrapper classes for all enum values, with selective override via ExtendedEnumOptionAttribute.
///
/// DISCOVERY STRATEGY:
/// - Find classes with [ExtendedEnumCollection(enumType, returnType, collectionType)]
/// - Auto-generate wrappers for ALL values of the specified enum
/// - Find custom implementations with [ExtendedEnumOption(collectionType, enumValue)]
/// - Replace auto-generated wrappers with custom implementations where specified
///
/// GENERATED FEATURES:
/// - Wrapper class for each enum value (auto-generated or custom)
/// - Static property for each enum value
/// - FrozenDictionary for O(1) lookups
/// - Implicit conversion operators
/// </summary>
[Generator]
public sealed class ExtendedEnumCollectionGenerator : IIncrementalGenerator
{
    /// <summary>
    /// Initializes the incremental generator for Extended Enum collection discovery and generation.
    /// </summary>
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
#if DEBUG
        context.RegisterPostInitializationOutput(static context =>
        {
            context.AddSource("ExtendedEnumCollectionGenerator.Init.g.cs",
                $"// DEBUG: ExtendedEnumCollectionGenerator.Initialize() called at {System.DateTime.Now}");
        });
#endif

        var compilationAndOptions = context.CompilationProvider
            .Combine(context.AnalyzerConfigOptionsProvider);

        var collectionDefinitions = compilationAndOptions
            .SelectMany(static (compilationAndOptions, token) =>
            {
                token.ThrowIfCancellationRequested();
                var (compilation, options) = compilationAndOptions;
                return DiscoverAllCollectionDefinitions(compilation, options.GlobalOptions);
            });

        context.RegisterSourceOutput(collectionDefinitions, static (context, info) =>
        {
            // Report any diagnostics
            foreach (var diagnostic in info.Diagnostics)
            {
                context.ReportDiagnostic(diagnostic);
            }

            // Only generate code if there are no error diagnostics
            if (!info.Diagnostics.Any(d => d.Severity == DiagnosticSeverity.Error))
            {
                Execute(context, info);
            }
        });
    }

    /// <summary>
    /// Discovers all Extended Enum collection definitions.
    /// </summary>
    private static ImmutableArray<ExtendedEnumTypeInfoWithCompilation> DiscoverAllCollectionDefinitions(Compilation compilation, AnalyzerConfigOptions globalOptions)
    {
        var results = new List<ExtendedEnumTypeInfoWithCompilation>();

        // STEP 1: Find all ExtendedEnumOption custom implementations and group by collection
        var customOptionsByCollectionType = FindAndGroupAllExtendedEnumOptions(compilation);

        // STEP 2: Find all ExtendedEnumCollection attributes
        var attributedCollectionClasses = FindAttributedCollectionClasses(compilation);

        // STEP 3: For each collection, generate wrappers for all enum values
        foreach (var (collectionClass, attribute) in attributedCollectionClasses)
        {
            // Extract enum type from attribute (first parameter)
            var enumType = ExtractEnumTypeFromAttribute(attribute);
            if (enumType == null || !enumType.TypeKind.Equals(TypeKind.Enum)) continue;

            // Extract return type from attribute (second parameter)
            var returnType = ExtractReturnTypeFromAttribute(attribute);
            if (returnType == null) continue;

            // Lookup custom implementations for this collection
            if (!customOptionsByCollectionType.TryGetValue(collectionClass, out var customOptions))
            {
                customOptions = new Dictionary<object, INamedTypeSymbol>();
            }

            // Build the extended enum definition model
            var extendedEnumDefinition = BuildExtendedEnumDefinition(
                collectionClass, enumType, returnType, customOptions, compilation, globalOptions, attribute);

            if (extendedEnumDefinition != null)
            {
                var diagnostics = new List<Diagnostic>();
                results.Add(new ExtendedEnumTypeInfoWithCompilation(
                    extendedEnumDefinition,
                    compilation,
                    enumType,
                    customOptions,
                    collectionClass,
                    diagnostics));
            }
        }

        return [..results];
    }

    /// <summary>
    /// Find all ExtendedEnumOption attributes and group by collection type.
    /// Returns a dictionary of collection type -> enum value -> custom implementation type.
    /// </summary>
    private static Dictionary<INamedTypeSymbol, Dictionary<object, INamedTypeSymbol>> FindAndGroupAllExtendedEnumOptions(Compilation compilation)
    {
        var optionsByCollectionType = new Dictionary<INamedTypeSymbol, Dictionary<object, INamedTypeSymbol>>(SymbolEqualityComparer.Default);
        var extendedEnumOptionAttributeType = compilation.GetTypeByMetadataName(typeof(ExtendedEnumOptionAttribute).FullName!);

        if (extendedEnumOptionAttributeType == null) return optionsByCollectionType;

        var allOptionsWithAttributes = new List<(INamedTypeSymbol Type, AttributeData Attribute)>();

        // Scan current compilation
        ScanNamespaceForExtendedEnumOptionsWithAttributes(compilation.GlobalNamespace, extendedEnumOptionAttributeType, allOptionsWithAttributes);

        // Scan referenced assemblies
        foreach (var reference in compilation.References)
        {
            if (compilation.GetAssemblyOrModuleSymbol(reference) is IAssemblySymbol assemblySymbol)
            {
                ScanNamespaceForExtendedEnumOptionsWithAttributes(assemblySymbol.GlobalNamespace, extendedEnumOptionAttributeType, allOptionsWithAttributes);
            }
        }

        // Group by collection type and enum value
        foreach (var (optionType, attribute) in allOptionsWithAttributes)
        {
            var collectionType = ExtractCollectionTypeFromExtendedEnumOptionAttribute(attribute);
            var enumValue = ExtractEnumValueFromExtendedEnumOptionAttribute(attribute);

            if (collectionType != null && enumValue != null)
            {
                if (!optionsByCollectionType.TryGetValue(collectionType, out var enumValueMap))
                {
                    enumValueMap = new Dictionary<object, INamedTypeSymbol>();
                    optionsByCollectionType[collectionType] = enumValueMap;
                }
                enumValueMap[enumValue] = optionType;
            }
        }

        return optionsByCollectionType;
    }

    /// <summary>
    /// Find all classes with ExtendedEnumCollectionAttribute.
    /// </summary>
    private static List<(INamedTypeSymbol CollectionClass, AttributeData Attribute)> FindAttributedCollectionClasses(Compilation compilation)
    {
        var results = new List<(INamedTypeSymbol, AttributeData)>();
        var extendedEnumCollectionAttributeType = compilation.GetTypeByMetadataName(typeof(ExtendedEnumCollectionAttribute).FullName!);

        if (extendedEnumCollectionAttributeType == null) return results;

        ScanNamespaceForAttributedTypes(compilation.GlobalNamespace, extendedEnumCollectionAttributeType, results);

        return results;
    }

    /// <summary>
    /// Recursively scan namespaces for types with the specified attribute.
    /// </summary>
    private static void ScanNamespaceForAttributedTypes(INamespaceSymbol namespaceSymbol, INamedTypeSymbol attributeType, List<(INamedTypeSymbol, AttributeData)> results)
    {
        foreach (var type in namespaceSymbol.GetTypeMembers())
        {
            var attribute = type.GetAttributes()
                .FirstOrDefault(attr => SymbolEqualityComparer.Default.Equals(attr.AttributeClass, attributeType));

            if (attribute != null)
            {
                results.Add((type, attribute));
            }

            ScanNestedTypesForAttribute(type, attributeType, results);
        }

        foreach (var childNamespace in namespaceSymbol.GetNamespaceMembers())
        {
            ScanNamespaceForAttributedTypes(childNamespace, attributeType, results);
        }
    }

    /// <summary>
    /// Scan nested types for the specified attribute.
    /// </summary>
    private static void ScanNestedTypesForAttribute(INamedTypeSymbol parentType, INamedTypeSymbol attributeType, List<(INamedTypeSymbol, AttributeData)> results)
    {
        foreach (var nestedType in parentType.GetTypeMembers())
        {
            var attribute = nestedType.GetAttributes()
                .FirstOrDefault(attr => SymbolEqualityComparer.Default.Equals(attr.AttributeClass, attributeType));

            if (attribute != null)
            {
                results.Add((nestedType, attribute));
            }

            ScanNestedTypesForAttribute(nestedType, attributeType, results);
        }
    }

    /// <summary>
    /// Scan namespace for ExtendedEnumOption attributes.
    /// </summary>
    private static void ScanNamespaceForExtendedEnumOptionsWithAttributes(
        INamespaceSymbol namespaceSymbol,
        INamedTypeSymbol attributeType,
        List<(INamedTypeSymbol Type, AttributeData Attribute)> results)
    {
        foreach (var type in namespaceSymbol.GetTypeMembers())
        {
            var attribute = type.GetAttributes()
                .FirstOrDefault(attr => SymbolEqualityComparer.Default.Equals(attr.AttributeClass, attributeType));

            if (attribute != null)
            {
                results.Add((type, attribute));
            }

            ScanNestedTypesForExtendedEnumOptionWithAttributes(type, attributeType, results);
        }

        foreach (var childNamespace in namespaceSymbol.GetNamespaceMembers())
        {
            ScanNamespaceForExtendedEnumOptionsWithAttributes(childNamespace, attributeType, results);
        }
    }

    /// <summary>
    /// Scan nested types for ExtendedEnumOption attributes.
    /// </summary>
    private static void ScanNestedTypesForExtendedEnumOptionWithAttributes(
        INamedTypeSymbol parentType,
        INamedTypeSymbol attributeType,
        List<(INamedTypeSymbol Type, AttributeData Attribute)> results)
    {
        foreach (var nestedType in parentType.GetTypeMembers())
        {
            var attribute = nestedType.GetAttributes()
                .FirstOrDefault(attr => SymbolEqualityComparer.Default.Equals(attr.AttributeClass, attributeType));

            if (attribute != null)
            {
                results.Add((nestedType, attribute));
            }

            ScanNestedTypesForExtendedEnumOptionWithAttributes(nestedType, attributeType, results);
        }
    }

    /// <summary>
    /// Extract enum type from ExtendedEnumCollectionAttribute (first parameter).
    /// </summary>
    private static INamedTypeSymbol? ExtractEnumTypeFromAttribute(AttributeData attribute)
    {
        if (attribute.ConstructorArguments.Length > 0 &&
            attribute.ConstructorArguments[0].Value is ITypeSymbol enumTypeSymbol)
        {
            return enumTypeSymbol as INamedTypeSymbol;
        }
        return null;
    }

    /// <summary>
    /// Extract return type from ExtendedEnumCollectionAttribute (second parameter).
    /// </summary>
    private static INamedTypeSymbol? ExtractReturnTypeFromAttribute(AttributeData attribute)
    {
        if (attribute.ConstructorArguments.Length > 1 &&
            attribute.ConstructorArguments[1].Value is ITypeSymbol returnTypeSymbol)
        {
            return returnTypeSymbol as INamedTypeSymbol;
        }
        return null;
    }

    /// <summary>
    /// Extract collection type from ExtendedEnumOptionAttribute (first parameter).
    /// </summary>
    private static INamedTypeSymbol? ExtractCollectionTypeFromExtendedEnumOptionAttribute(AttributeData attribute)
    {
        if (attribute.ConstructorArguments.Length > 0 &&
            attribute.ConstructorArguments[0].Value is ITypeSymbol collectionTypeSymbol)
        {
            return collectionTypeSymbol as INamedTypeSymbol;
        }
        return null;
    }

    /// <summary>
    /// Extract enum value from ExtendedEnumOptionAttribute (second parameter).
    /// </summary>
    private static object? ExtractEnumValueFromExtendedEnumOptionAttribute(AttributeData attribute)
    {
        if (attribute.ConstructorArguments.Length > 1)
        {
            return attribute.ConstructorArguments[1].Value;
        }
        return null;
    }

    /// <summary>
    /// Build the extended enum definition model.
    /// </summary>
    private static ExtendedEnumDefinition? BuildExtendedEnumDefinition(
        INamedTypeSymbol collectionClass,
        INamedTypeSymbol enumType,
        INamedTypeSymbol returnType,
        Dictionary<object, INamedTypeSymbol> customOptions,
        Compilation compilation,
        AnalyzerConfigOptions globalOptions,
        AttributeData attribute)
    {
        var collectionName = collectionClass.Name;
        var containingNamespace = collectionClass.ContainingNamespace?.ToDisplayString() ?? string.Empty;

        // Check for UseSingletonInstances property (defaults to true for Extended Enums)
        var useSingletonInstances = true;
        var namedArgs = attribute.NamedArguments;
        foreach (var namedArg in namedArgs)
        {
            if (string.Equals(namedArg.Key, "UseSingletonInstances", StringComparison.Ordinal) && namedArg.Value.Value is bool value)
            {
                useSingletonInstances = value;
                break;
            }
        }

        // Get all enum values
        var enumValues = enumType.GetMembers().OfType<IFieldSymbol>()
            .Where(f => f.IsConst && f.Type.Equals(enumType, SymbolEqualityComparer.Default))
            .ToList();

        return new ExtendedEnumDefinition
        {
            Namespace = containingNamespace,
            CollectionName = collectionName,
            EnumType = enumType,
            ReturnType = returnType,
            EnumValues = enumValues,
            CustomImplementations = customOptions,
            UseSingletonInstances = useSingletonInstances
        };
    }

    /// <summary>
    /// Execute the code generation for an Extended Enum collection.
    /// </summary>
    private static void Execute(SourceProductionContext context, ExtendedEnumTypeInfoWithCompilation info)
    {
        var def = info.ExtendedEnumDefinition;
        if (def == null) return;

        var generatedCode = GenerateExtendedEnumCollection(def, info.Compilation);
        var fileName = $"{def.CollectionName}.g.cs";

#if DEBUG
        var debugHeader = $@"// DEBUG: ExtendedEnumCollectionGenerator
// Generated at: {System.DateTime.Now}
// Collection: {def.CollectionName}
// Enum Type: {def.EnumType.ToDisplayString()}
// Return Type: {def.ReturnType.ToDisplayString()}
// Enum Values: {string.Join(", ", def.EnumValues.Select(v => v.Name))}
// Custom Implementations: {string.Join(", ", def.CustomImplementations.Keys)}

";
        generatedCode = debugHeader + generatedCode;
#endif
        context.AddSource(fileName, generatedCode);
    }

    /// <summary>
    /// Generate the Extended Enum collection code.
    /// </summary>
    private static string GenerateExtendedEnumCollection(ExtendedEnumDefinition def, Compilation compilation)
    {
        var sb = new StringBuilder();

        // Usings
        sb.AppendLine("using System;");
        sb.AppendLine("using System.Collections.Frozen;");
        sb.AppendLine("using System.Collections.Generic;");
        sb.AppendLine("using System.Linq;");
        sb.AppendLine("using FractalDataWorks.EnhancedEnums;");
        sb.AppendLine("using FractalDataWorks.EnhancedEnums.ExtendedEnums;");
        sb.AppendLine();

        // Namespace
        sb.AppendLine($"namespace {def.Namespace};");
        sb.AppendLine();

        // Partial class declaration
        sb.AppendLine($"public abstract partial class {def.CollectionName} : EnumCollectionBase<{def.ReturnType.ToDisplayString()}>");
        sb.AppendLine("{");

        // Generate wrapper classes for enum values without custom implementations
        foreach (var enumValue in def.EnumValues)
        {
            if (!def.CustomImplementations.ContainsKey(enumValue.ConstantValue!))
            {
                GenerateWrapperClass(sb, enumValue, def);
            }
        }

        // Static fields for each enum value
        foreach (var enumValue in def.EnumValues)
        {
            var propertyName = enumValue.Name;
            string typeName;

            if (def.CustomImplementations.TryGetValue(enumValue.ConstantValue!, out var customType))
            {
                typeName = customType.ToDisplayString();
            }
            else
            {
                typeName = $"_{enumValue.Name}_Wrapper";
            }

            if (def.UseSingletonInstances)
            {
                sb.AppendLine($"    public static {def.ReturnType.ToDisplayString()} {propertyName} {{ get; }} = new {typeName}();");
            }
            else
            {
                sb.AppendLine($"    public static {def.ReturnType.ToDisplayString()} {propertyName} => new {typeName}();");
            }
        }
        sb.AppendLine();

        // All() method
        GenerateAllMethod(sb, def);

        // GetById() method
        GenerateGetByIdMethod(sb, def);

        // GetByName() method
        GenerateGetByNameMethod(sb, def);

        // Empty() method
        GenerateEmptyMethod(sb, def);

        sb.AppendLine("}");

        return sb.ToString();
    }

    /// <summary>
    /// Generate a wrapper class for an enum value.
    /// </summary>
    private static void GenerateWrapperClass(StringBuilder sb, IFieldSymbol enumValue, ExtendedEnumDefinition def)
    {
        var wrapperName = $"_{enumValue.Name}_Wrapper";
        var enumTypeName = def.EnumType.ToDisplayString();
        var returnTypeName = def.ReturnType.ToDisplayString();

        sb.AppendLine($"    private sealed class {wrapperName} : ExtendedEnumOptionBase<{returnTypeName}, {enumTypeName}>");
        sb.AppendLine("    {");
        sb.AppendLine($"        public {wrapperName}() : base({enumTypeName}.{enumValue.Name}) {{ }}");
        sb.AppendLine("    }");
        sb.AppendLine();
    }

    /// <summary>
    /// Generate the All() method.
    /// </summary>
    private static void GenerateAllMethod(StringBuilder sb, ExtendedEnumDefinition def)
    {
        sb.AppendLine("    private static readonly FrozenSet<" + def.ReturnType.ToDisplayString() + "> _all = FrozenSet.ToFrozenSet(new[] {");
        foreach (var enumValue in def.EnumValues)
        {
            sb.AppendLine($"        {enumValue.Name},");
        }
        sb.AppendLine("    });");
        sb.AppendLine();
        sb.AppendLine($"    public static FrozenSet<{def.ReturnType.ToDisplayString()}> All() => _all;");
        sb.AppendLine();
    }

    /// <summary>
    /// Generate the GetById() method.
    /// </summary>
    private static void GenerateGetByIdMethod(StringBuilder sb, ExtendedEnumDefinition def)
    {
        var returnType = def.ReturnType.ToDisplayString();

        sb.AppendLine($"    private static readonly FrozenDictionary<int, {returnType}> _byId = FrozenDictionary.ToFrozenDictionary(");
        sb.AppendLine("        All().ToDictionary(x => x.Id));");
        sb.AppendLine();
        sb.AppendLine($"    public static {returnType}? GetById(int id)");
        sb.AppendLine("    {");
        sb.AppendLine("        return _byId.TryGetValue(id, out var value) ? value : null;");
        sb.AppendLine("    }");
        sb.AppendLine();
    }

    /// <summary>
    /// Generate the GetByName() method.
    /// </summary>
    private static void GenerateGetByNameMethod(StringBuilder sb, ExtendedEnumDefinition def)
    {
        var returnType = def.ReturnType.ToDisplayString();

        sb.AppendLine($"    private static readonly FrozenDictionary<string, {returnType}> _byName = FrozenDictionary.ToFrozenDictionary(");
        sb.AppendLine("        All().ToDictionary(x => x.Name, StringComparer.OrdinalIgnoreCase));");
        sb.AppendLine();
        sb.AppendLine($"    public static {returnType}? GetByName(string name)");
        sb.AppendLine("    {");
        sb.AppendLine("        return string.IsNullOrEmpty(name) ? null : _byName.TryGetValue(name, out var value) ? value : null;");
        sb.AppendLine("    }");
        sb.AppendLine();
    }

    /// <summary>
    /// Generate the Empty() method.
    /// </summary>
    private static void GenerateEmptyMethod(StringBuilder sb, ExtendedEnumDefinition def)
    {
        // Use the first enum value as the empty/default
        if (def.EnumValues.Count > 0)
        {
            var firstValue = def.EnumValues[0];
            sb.AppendLine($"    public static {def.ReturnType.ToDisplayString()} Empty() => {firstValue.Name};");
        }
        else
        {
            sb.AppendLine($"    public static {def.ReturnType.ToDisplayString()}? Empty() => null;");
        }
        sb.AppendLine();
    }

    /// <summary>
    /// Container for Extended Enum definition data.
    /// </summary>
    private sealed class ExtendedEnumDefinition
    {
        public string Namespace { get; set; } = string.Empty;
        public string CollectionName { get; set; } = string.Empty;
        public INamedTypeSymbol EnumType { get; set; } = null!;
        public INamedTypeSymbol ReturnType { get; set; } = null!;
        public List<IFieldSymbol> EnumValues { get; set; } = new();
        public Dictionary<object, INamedTypeSymbol> CustomImplementations { get; set; } = new();
        public bool UseSingletonInstances { get; set; } = true;
    }

    /// <summary>
    /// Container for passing Extended Enum info through the pipeline.
    /// </summary>
    private sealed class ExtendedEnumTypeInfoWithCompilation
    {
        public ExtendedEnumDefinition ExtendedEnumDefinition { get; }
        public Compilation Compilation { get; }
        public INamedTypeSymbol EnumType { get; }
        public Dictionary<object, INamedTypeSymbol> CustomImplementations { get; }
        public INamedTypeSymbol CollectionClass { get; }
        public List<Diagnostic> Diagnostics { get; }

        public ExtendedEnumTypeInfoWithCompilation(
            ExtendedEnumDefinition extendedEnumDefinition,
            Compilation compilation,
            INamedTypeSymbol enumType,
            Dictionary<object, INamedTypeSymbol> customImplementations,
            INamedTypeSymbol collectionClass,
            List<Diagnostic> diagnostics)
        {
            ExtendedEnumDefinition = extendedEnumDefinition;
            Compilation = compilation;
            EnumType = enumType;
            CustomImplementations = customImplementations;
            CollectionClass = collectionClass;
            Diagnostics = diagnostics;
        }
    }
}