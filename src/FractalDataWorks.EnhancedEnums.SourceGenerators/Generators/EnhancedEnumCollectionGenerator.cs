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
using FractalDataWorks.EnhancedEnums.Attributes;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace FractalDataWorks.EnhancedEnums.SourceGenerators.Generators;

/// <summary>
/// EnhancedEnumCollectionGenerator - Creates high-performance Enhanced Enum collections using attribute-based discovery.
/// Follows the same pattern as TypeCollectionGenerator for consistency.
///
/// DISCOVERY STRATEGY:
/// - Looks for classes with [EnumOption(collectionType, name)] attributes
/// - Groups options by their target collection type
/// - No inheritance scanning - purely attribute-based
/// - O(types_with_attribute) complexity instead of O(all_types × inheritance_depth)
///
/// GENERATED FEATURES:
/// - FrozenDictionary for O(1) lookups
/// - Static properties for each discovered option
/// - Empty() method returning default instance
/// - Factory methods for constructor overloads
/// </summary>
[Generator]
public sealed class EnhancedEnumCollectionGenerator : IIncrementalGenerator
{
    /// <summary>
    /// Initializes the incremental generator for Enhanced Enum collection discovery and generation.
    /// </summary>
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
#if DEBUG
        context.RegisterPostInitializationOutput(static context =>
        {
            context.AddSource("EnhancedEnumCollectionGenerator.Init.g.cs",
                $"// DEBUG: EnhancedEnumCollectionGenerator.Initialize() called at {System.DateTime.Now}");
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

        context.RegisterSourceOutput(collectionDefinitions, (context, info) =>
        {
            // Report any diagnostics first
            foreach (var diagnostic in info.Diagnostics)
            {
                context.ReportDiagnostic(diagnostic);
            }

            // Only generate code if there are no error diagnostics
            if (!info.Diagnostics.Any(d => d.Severity == DiagnosticSeverity.Error))
            {
                Execute(context, info.EnumTypeInfoModel, info.Compilation, info.DiscoveredOptionTypes, info.CollectionClass);
            }
        });
    }

    /// <summary>
    /// Discovers all collection definitions using EnumOption-first discovery.
    /// Same pattern as TypeCollectionGenerator - find options first, then match to collections.
    /// </summary>
    private static ImmutableArray<EnumTypeInfoWithCompilation> DiscoverAllCollectionDefinitions(Compilation compilation, AnalyzerConfigOptions globalOptions)
    {
        var results = new List<EnumTypeInfoWithCompilation>();

        // STEP 1: Find all EnumOption attributes and group by collection type
        var optionsByCollectionType = FindAndGroupAllEnumOptions(compilation);

        // STEP 2: Find all EnumCollection attributes
        var attributedCollectionClasses = FindAttributedCollectionClasses(compilation);

        // STEP 3: For each collection, lookup its options and build the model
        foreach (var (collectionClass, attribute) in attributedCollectionClasses)
        {
            // Extract base type from attribute (first parameter)
            var baseType = ExtractBaseTypeFromAttribute(attribute, compilation);
            if (baseType == null) continue;

            // Extract return type from attribute (second parameter)
            var returnType = ExtractReturnTypeFromAttribute(attribute, compilation);
            if (returnType == null) continue;

            // Lookup pre-discovered options for this collection
            if (!optionsByCollectionType.TryGetValue(collectionClass, out var optionTypes))
            {
                optionTypes = new List<INamedTypeSymbol>();
            }

            // Build the enum definition model
            var enumDefinition = BuildEnumDefinitionFromAttributedCollection(
                collectionClass, baseType, returnType, optionTypes, compilation, globalOptions, attribute);

            if (enumDefinition != null)
            {
                var diagnostics = new List<Diagnostic>(); // Could add validation here
                results.Add(new EnumTypeInfoWithCompilation(enumDefinition, compilation, optionTypes, collectionClass, diagnostics));
            }
        }

        return [..results];
    }

    /// <summary>
    /// STEP 1: Find all EnumOption attributes and group by collection type.
    /// This is the same pattern as TypeCollectionGenerator's FindAndGroupAllTypeOptions.
    /// </summary>
    private static Dictionary<INamedTypeSymbol, List<INamedTypeSymbol>> FindAndGroupAllEnumOptions(Compilation compilation)
    {
        var optionsByCollectionType = new Dictionary<INamedTypeSymbol, List<INamedTypeSymbol>>(SymbolEqualityComparer.Default);
        var enumOptionAttributeType = compilation.GetTypeByMetadataName(typeof(EnumOptionAttribute).FullName!);

        if (enumOptionAttributeType == null) return optionsByCollectionType;

        var allOptionsWithAttributes = new List<(INamedTypeSymbol Type, AttributeData Attribute)>();

        // Scan current compilation
        ScanNamespaceForEnumOptionsWithAttributes(compilation.GlobalNamespace, enumOptionAttributeType, allOptionsWithAttributes);

        // Scan all referenced assemblies
        foreach (var reference in compilation.References)
        {
            if (compilation.GetAssemblyOrModuleSymbol(reference) is IAssemblySymbol assemblySymbol)
            {
                ScanNamespaceForEnumOptionsWithAttributes(assemblySymbol.GlobalNamespace, enumOptionAttributeType, allOptionsWithAttributes);
            }
        }

        // Group by collection type (first parameter of EnumOptionAttribute)
        foreach (var (optionType, attribute) in allOptionsWithAttributes)
        {
            var collectionType = ExtractCollectionTypeFromEnumOptionAttribute(attribute);
            if (collectionType != null)
            {
                if (!optionsByCollectionType.TryGetValue(collectionType, out var list))
                {
                    list = new List<INamedTypeSymbol>();
                    optionsByCollectionType[collectionType] = list;
                }
                list.Add(optionType);
            }
        }

        return optionsByCollectionType;
    }

    /// <summary>
    /// STEP 2: Find all classes with EnumCollectionAttribute.
    /// </summary>
    private static List<(INamedTypeSymbol CollectionClass, AttributeData Attribute)> FindAttributedCollectionClasses(Compilation compilation)
    {
        var results = new List<(INamedTypeSymbol, AttributeData)>();
        var enhancedEnumCollectionAttributeType = compilation.GetTypeByMetadataName(typeof(EnumCollectionAttribute).FullName!);

        if (enhancedEnumCollectionAttributeType == null) return results;

        ScanNamespaceForAttributedTypes(compilation.GlobalNamespace, enhancedEnumCollectionAttributeType, results);

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
    /// Scan namespace for EnumOption attributes with their data.
    /// </summary>
    private static void ScanNamespaceForEnumOptionsWithAttributes(
        INamespaceSymbol namespaceSymbol,
        INamedTypeSymbol attributeType,
        List<(INamedTypeSymbol Type, AttributeData Attribute)> results)
    {
        foreach (var type in namespaceSymbol.GetTypeMembers())
        {
            var attribute = GetEnumOptionAttribute(type, attributeType);
            if (attribute != null)
            {
                results.Add((type, attribute));
            }

            ScanNestedTypesForEnumOptionWithAttributes(type, attributeType, results);
        }

        foreach (var childNamespace in namespaceSymbol.GetNamespaceMembers())
        {
            ScanNamespaceForEnumOptionsWithAttributes(childNamespace, attributeType, results);
        }
    }

    /// <summary>
    /// Scan nested types for EnumOption attributes.
    /// </summary>
    private static void ScanNestedTypesForEnumOptionWithAttributes(
        INamedTypeSymbol parentType,
        INamedTypeSymbol attributeType,
        List<(INamedTypeSymbol Type, AttributeData Attribute)> results)
    {
        foreach (var nestedType in parentType.GetTypeMembers())
        {
            var attribute = GetEnumOptionAttribute(nestedType, attributeType);
            if (attribute != null)
            {
                results.Add((nestedType, attribute));
            }

            ScanNestedTypesForEnumOptionWithAttributes(nestedType, attributeType, results);
        }
    }

    /// <summary>
    /// Gets the EnumOption attribute from a type.
    /// </summary>
    private static AttributeData? GetEnumOptionAttribute(INamedTypeSymbol type, INamedTypeSymbol attributeType)
    {
        return type.GetAttributes()
            .FirstOrDefault(attr => SymbolEqualityComparer.Default.Equals(attr.AttributeClass, attributeType));
    }

    /// <summary>
    /// Extract collection type from EnumOptionAttribute (first parameter).
    /// </summary>
    private static INamedTypeSymbol? ExtractCollectionTypeFromEnumOptionAttribute(AttributeData attribute)
    {
        if (attribute.ConstructorArguments.Length > 0 &&
            attribute.ConstructorArguments[0].Value is INamedTypeSymbol collectionType)
        {
            return collectionType;
        }
        return null;
    }

    /// <summary>
    /// Extract base type from EnumCollectionAttribute (first parameter).
    /// </summary>
    private static INamedTypeSymbol? ExtractBaseTypeFromAttribute(AttributeData attribute, Compilation compilation)
    {
        if (attribute.ConstructorArguments.Length > 0 &&
            attribute.ConstructorArguments[0].Value is ITypeSymbol baseTypeSymbol)
        {
            return baseTypeSymbol as INamedTypeSymbol;
        }
        return null;
    }

    /// <summary>
    /// Extract return type from EnumCollectionAttribute (second parameter).
    /// </summary>
    private static INamedTypeSymbol? ExtractReturnTypeFromAttribute(AttributeData attribute, Compilation compilation)
    {
        if (attribute.ConstructorArguments.Length > 1 &&
            attribute.ConstructorArguments[1].Value is ITypeSymbol returnTypeSymbol)
        {
            return returnTypeSymbol as INamedTypeSymbol;
        }
        return null;
    }

    /// <summary>
    /// Extract name from EnumOptionAttribute (second parameter).
    /// </summary>
    private static string ExtractNameFromEnumOptionAttribute(AttributeData attribute, INamedTypeSymbol optionType)
    {
        if (attribute.ConstructorArguments.Length > 1 &&
            attribute.ConstructorArguments[1].Value is string name)
        {
            return name;
        }
        return optionType.Name; // Fallback to class name
    }

    /// <summary>
    /// Build the enum definition model from the discovered collection and options.
    /// </summary>
    private static EnumTypeInfoModel? BuildEnumDefinitionFromAttributedCollection(
        INamedTypeSymbol collectionClass,
        INamedTypeSymbol baseType,
        INamedTypeSymbol returnType,
        List<INamedTypeSymbol> optionTypes,
        Compilation compilation,
        AnalyzerConfigOptions globalOptions,
        AttributeData attribute)
    {
        // Extract collection name from third parameter or use class name
        var collectionName = collectionClass.Name;
        if (attribute.ConstructorArguments.Length > 2 &&
            attribute.ConstructorArguments[2].Value is ITypeSymbol collectionTypeSymbol)
        {
            collectionName = collectionTypeSymbol.Name;
        }

        // Get namespace from collection class
        var containingNamespace = collectionClass.ContainingNamespace?.ToDisplayString() ?? string.Empty;

        // Check for UseSingletonInstances property
        var useSingletonInstances = false;
        var namedArgs = attribute.NamedArguments;
        foreach (var namedArg in namedArgs)
        {
            if (namedArg.Key == "UseSingletonInstances" && namedArg.Value.Value is bool value)
            {
                useSingletonInstances = value;
                break;
            }
        }

        return new EnumTypeInfoModel
        {
            Namespace = containingNamespace,
            ClassName = baseType.Name,
            FullTypeName = baseType.ToDisplayString(),
            CollectionName = collectionName,
            CollectionBaseType = baseType.ToDisplayString(),
            ReturnType = returnType.ToDisplayString(),
            DefaultGenericReturnType = returnType.ToDisplayString(),
            InheritsFromCollectionBase = true,
            UseSingletonInstances = useSingletonInstances,
            GenerateFactoryMethods = !useSingletonInstances,
            GenerateStaticCollection = true,
            UseDictionaryStorage = true,
            LookupProperties = ExtractLookupPropertiesFromBaseType(baseType, compilation)
        };
    }

    /// <summary>
    /// Extract lookup properties from the base type (for EnumLookup attribute support).
    /// </summary>
    private static EquatableArray<PropertyLookupInfoModel> ExtractLookupPropertiesFromBaseType(INamedTypeSymbol baseType, Compilation compilation)
    {
        var lookupProperties = new List<PropertyLookupInfoModel>();

        // We could look for EnumLookupAttribute here if needed
        // For now, return empty since Enhanced Enums might not use this feature

        return new EquatableArray<PropertyLookupInfoModel>(lookupProperties);
    }

    /// <summary>
    /// Execute the code generation for a discovered collection.
    /// </summary>
    private static void Execute(
        SourceProductionContext context,
        EnumTypeInfoModel def,
        Compilation compilation,
        List<INamedTypeSymbol> discoveredOptionTypes,
        INamedTypeSymbol collectionClass)
    {
        if (def == null) throw new ArgumentNullException(nameof(def));
        if (compilation == null) throw new ArgumentNullException(nameof(compilation));

        // Convert discovered option types to EnumValueInfoModel
        var values = new List<EnumValueInfoModel>();
        var enhancedEnumOptionAttributeType = compilation.GetTypeByMetadataName(typeof(EnumOptionAttribute).FullName!);

        foreach (var optionType in discoveredOptionTypes)
        {
            // Get the EnumOption attribute to extract the name
            var attribute = optionType.GetAttributes()
                .FirstOrDefault(attr => enhancedEnumOptionAttributeType != null &&
                    SymbolEqualityComparer.Default.Equals(attr.AttributeClass, enhancedEnumOptionAttributeType));

            var name = attribute != null
                ? ExtractNameFromEnumOptionAttribute(attribute, optionType)
                : optionType.Name;

            var enumValueInfo = new EnumValueInfoModel
            {
                ShortTypeName = optionType.Name,
                FullTypeName = optionType.ToDisplayString(),
                Name = name,
                ReturnTypeNamespace = optionType.ContainingNamespace?.ToDisplayString() ?? string.Empty,
                IsAbstract = optionType.IsAbstract,
                IsStatic = optionType.IsStatic
            };
            values.Add(enumValueInfo);
        }

        GenerateCollection(context, def, new EquatableArray<EnumValueInfoModel>(values), compilation, collectionClass);
    }

    /// <summary>
    /// Generate the final collection code.
    /// </summary>
    private static void GenerateCollection(
        SourceProductionContext context,
        EnumTypeInfoModel def,
        EquatableArray<EnumValueInfoModel> values,
        Compilation compilation,
        INamedTypeSymbol collectionClass)
    {
        try
        {
            var isUserClassStatic = collectionClass.IsStatic;
            var isUserClassAbstract = collectionClass.IsAbstract;

            // Use the EnumCollectionBuilder to generate the code
            var builder = new EnumCollectionBuilder();

            var generatedCode = builder
                .WithDefinition(def)
                .WithValues(values.ToList())
                .WithReturnType(def.ReturnType!)
                .WithCompilation(compilation)
                .Build();

            var fileName = $"{def.CollectionName}.g.cs";

#if DEBUG
            var debugHeader = $@"// DEBUG: EnhancedEnumCollectionGenerator
// Generated at: {System.DateTime.Now}
// Collection: {def.CollectionName}
// Base Type: {def.FullTypeName}
// Return Type: {def.ReturnType}
// Options: {string.Join(", ", values.Select(v => v.Name))}

";
            generatedCode = debugHeader + generatedCode;
#endif
            context.AddSource(fileName, generatedCode);
        }
        catch (Exception ex)
        {
            var diagnostic = Diagnostic.Create(
                new DiagnosticDescriptor(
                    "EEC001",
                    "Enhanced Enum Collection Generation Failed",
                    "Failed to generate enhanced enum collection {0}: {1}",
                    "EnhancedEnumCollectionGenerator",
                    DiagnosticSeverity.Error,
                    isEnabledByDefault: true),
                Location.None,
                def.CollectionName,
                ex.Message);

            context.ReportDiagnostic(diagnostic);
        }
    }
}