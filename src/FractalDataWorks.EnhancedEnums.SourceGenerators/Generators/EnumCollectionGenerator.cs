using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
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
/// Source generator for Enhanced Enums that supports both local and cross-assembly discovery.
/// 
/// LOCAL DISCOVERY (default):
/// - Uses [StaticEnumCollection] attribute
/// - Scans the current compilation for enum options
/// 
/// GLOBAL DISCOVERY (opt-in):
/// - Uses [GlobalStaticEnumCollection] attribute  
/// - Scans ALL referenced assemblies for enum options
/// - Enables cross-assembly enum composition patterns
/// </summary>
/// <remarks>
/// This code is excluded from code coverage because source generators run at compile-time and cannot be unit tested via runtime tests.
/// </remarks>
[Generator]

public class EnumCollectionGenerator : IIncrementalGenerator
{
    // Cache for assembly types to avoid re-scanning
    private static readonly ConcurrentDictionary<string, List<INamedTypeSymbol>> _assemblyTypeCache = new(StringComparer.Ordinal);

    /// <summary>
    /// Initializes the incremental source generator using the same pattern as GlobalEnhancedEnumGenerator.
    /// </summary>
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        RegisterMainSourceOutput(context);
#if DEBUG
        RegisterPostInitializationOutput(context);
#endif
    }

    private static void RegisterPostInitializationOutput(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(ctx =>
        {
            ctx.AddSource("EnumCollectionGenerator_Debug.cs", 
                "// EnumCollectionGenerator Initialize() was called - generator is loaded!");
        });
    }

    private static void RegisterMainSourceOutput(IncrementalGeneratorInitializationContext context)
    {
        var enumDefinitionsProvider = context.CompilationProvider
            .Select((compilation, _) => DiscoverAllCollectionDefinitions(compilation));

        context.RegisterSourceOutput(enumDefinitionsProvider, (context, enumDefinitions) =>
        {
            foreach (var enumDefinition in enumDefinitions)
            {
                Execute(context, enumDefinition.EnumTypeInfoModel, enumDefinition.Compilation, enumDefinition.DiscoveredOptionTypes);
            }
        });
    }

    /// <summary>
    /// Discovers all collection definitions by scanning for classes with [StaticEnumCollection] or [GlobalStaticEnumCollection] attributes.
    /// </summary>
    private static ImmutableArray<EnumTypeInfoWithCompilation> DiscoverAllCollectionDefinitions(Compilation compilation)
    {
        var results = new List<EnumTypeInfoWithCompilation>();

        // Step 1: Scan for collection classes with [StaticEnumCollection] or [GlobalStaticEnumCollection]
        var collectionClasses = new HashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default);
        
        // Always scan current compilation
        ScanForCollectionClasses(compilation.GlobalNamespace, collectionClasses);
        
        // For each collection class found, determine if we should scan globally
        foreach (var collectionClass in collectionClasses)
        {
            var isGlobal = HasGlobalEnumCollectionAttribute(collectionClass);
            var baseType = ExtractBaseTypeFromCollection(collectionClass);
            
            if (baseType == null)
                continue;
                
            var optionTypes = new HashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default);
            
            if (isGlobal)
            {
                // Scan all referenced assemblies
                foreach (var reference in compilation.References)
                {
                    if (compilation.GetAssemblyOrModuleSymbol(reference) is IAssemblySymbol assemblySymbol)
                    {
                        ScanForOptionTypesOfBase(assemblySymbol.GlobalNamespace, baseType, optionTypes);
                    }
                }
            }
            else
            {
                // Scan only current compilation
                ScanForOptionTypesOfBase(compilation.GlobalNamespace, baseType, optionTypes);
            }
            
            // Create enum definition for this collection
            if (optionTypes.Count > 0 || true) // Always generate even if empty for Empty() support
            {
                var enumDefinition = BuildEnumDefinitionFromCollection(collectionClass, baseType, optionTypes.ToList(), compilation);
                if (enumDefinition != null)
                {
                    results.Add(new EnumTypeInfoWithCompilation(enumDefinition, compilation, optionTypes.ToImmutableArray()));
                }
            }
        }

        return [..results];
    }

    private static void ScanForCollectionClasses(INamespaceSymbol namespaceSymbol, HashSet<INamedTypeSymbol> collectionClasses)
    {
        foreach (var type in namespaceSymbol.GetTypeMembers())
        {
            if (HasCollectionAttribute(type))
            {
                collectionClasses.Add(type);
            }
            
            // Recursively scan nested types
            ScanNestedTypesForCollections(type, collectionClasses);
        }

        // Recursively scan nested namespaces
        foreach (var nestedNamespace in namespaceSymbol.GetNamespaceMembers())
        {
            ScanForCollectionClasses(nestedNamespace, collectionClasses);
        }
    }

    private static void ScanNestedTypesForCollections(INamedTypeSymbol typeSymbol, HashSet<INamedTypeSymbol> collectionClasses)
    {
        foreach (var nestedType in typeSymbol.GetTypeMembers())
        {
            if (HasCollectionAttribute(nestedType))
            {
                collectionClasses.Add(nestedType);
            }
            
            // Recursively scan further nested types
            ScanNestedTypesForCollections(nestedType, collectionClasses);
        }
    }

    private static bool HasCollectionAttribute(INamedTypeSymbol type)
    {
        return type.GetAttributes().Any(attr =>
            attr.AttributeClass != null &&
            (string.Equals(attr.AttributeClass.ToDisplayString(), typeof(EnumCollectionAttribute).FullName, StringComparison.Ordinal) ||
             string.Equals(attr.AttributeClass.ToDisplayString(), typeof(GlobalEnumCollectionAttribute).FullName, StringComparison.Ordinal)));
    }

    private static bool HasGlobalEnumCollectionAttribute(INamedTypeSymbol type)
    {
        return type.GetAttributes().Any(attr =>
            attr.AttributeClass != null &&
            string.Equals(attr.AttributeClass.ToDisplayString(), typeof(GlobalEnumCollectionAttribute).FullName, StringComparison.Ordinal));
    }

    private static INamedTypeSymbol? ExtractBaseTypeFromCollection(INamedTypeSymbol collectionClass)
    {
        // For collection-first pattern: extract from generic constraint
        // e.g., ColorsCollectionBase<T> where T : ColorOptionBase
        if (collectionClass.IsGenericType && collectionClass.TypeParameters.Length > 0)
        {
            var firstTypeParam = collectionClass.TypeParameters[0];
            if (firstTypeParam.ConstraintTypes.Length > 0)
            {
                return firstTypeParam.ConstraintTypes[0] as INamedTypeSymbol;
            }
        }
        
        // For classes that inherit from EnumCollectionBase<T>, extract T
        // e.g., DocumentTypeCollection : EnumCollectionBase<DocumentTypeBase>
        var currentBase = collectionClass.BaseType;
        while (currentBase != null)
        {
            if (currentBase.IsGenericType && currentBase.TypeArguments.Length > 0)
            {
                // Check if this is EnumCollectionBase<T>
                var constructedFrom = currentBase.ConstructedFrom;
                if (constructedFrom != null && 
                    (string.Equals(constructedFrom.Name, "EnumCollectionBase", StringComparison.Ordinal) || 
                     constructedFrom.ToDisplayString().Contains("EnumCollectionBase")))
                {
                    return currentBase.TypeArguments[0] as INamedTypeSymbol;
                }
            }
            currentBase = currentBase.BaseType;
        }
        
        // Return null if it's not a proper collection-first pattern
        return null;
    }

    private static void ScanForOptionTypesOfBase(INamespaceSymbol namespaceSymbol, INamedTypeSymbol baseType, HashSet<INamedTypeSymbol> optionTypes)
    {
        foreach (var type in namespaceSymbol.GetTypeMembers())
        {
            if (!type.IsAbstract && DerivesFromBaseType(type, baseType))
            {
                // Check for [EnumOption] attribute is optional - any concrete type deriving from base is included
                optionTypes.Add(type);
            }
            
            // Recursively scan nested types
            ScanNestedTypesForOptionTypesOfBase(type, baseType, optionTypes);
        }

        // Recursively scan nested namespaces
        foreach (var nestedNamespace in namespaceSymbol.GetNamespaceMembers())
        {
            ScanForOptionTypesOfBase(nestedNamespace, baseType, optionTypes);
        }
    }

    private static void ScanNestedTypesForOptionTypesOfBase(INamedTypeSymbol typeSymbol, INamedTypeSymbol baseType, HashSet<INamedTypeSymbol> optionTypes)
    {
        foreach (var nestedType in typeSymbol.GetTypeMembers())
        {
            if (!nestedType.IsAbstract && DerivesFromBaseType(nestedType, baseType))
            {
                optionTypes.Add(nestedType);
            }
            
            // Recursively scan further nested types
            ScanNestedTypesForOptionTypesOfBase(nestedType, baseType, optionTypes);
        }
    }

    private static bool DerivesFromBaseType(INamedTypeSymbol derivedType, INamedTypeSymbol baseType)
    {
        var currentBase = derivedType.BaseType;
        while (currentBase != null)
        {
            if (SymbolEqualityComparer.Default.Equals(currentBase, baseType))
            {
                return true;
            }
            
            // For generic types, compare the unbound generic type
            if (baseType.IsGenericType && currentBase.IsGenericType)
            {
                var currentUnbound = currentBase.ConstructedFrom ?? currentBase;
                var baseUnbound = baseType.ConstructedFrom ?? baseType;
                
                if (SymbolEqualityComparer.Default.Equals(currentUnbound, baseUnbound))
                {
                    return true;
                }
            }
            
            currentBase = currentBase.BaseType;
        }
        return false;
    }

    private static bool CheckIfInheritsFromEnumCollectionBase(INamedTypeSymbol collectionClass, Compilation compilation)
    {
        // Get the EnumCollectionBase<T> type from the compilation
        var enumCollectionBase = compilation.GetTypeByMetadataName("FractalDataWorks.Collections.TypeCollectionBase`1");
        if (enumCollectionBase == null)
            return false;
        
        // Check if the collection class inherits from EnumCollectionBase<T>
        var currentBase = collectionClass.BaseType;
        while (currentBase != null)
        {
            // Check for generic match
            if (currentBase.IsGenericType && currentBase.ConstructedFrom != null)
            {
                if (SymbolEqualityComparer.Default.Equals(currentBase.ConstructedFrom, enumCollectionBase))
                {
                    return true;
                }
            }
            
            currentBase = currentBase.BaseType;
        }
        
        return false;
    }

    private static EnumTypeInfoModel? BuildEnumDefinitionFromCollection(
        INamedTypeSymbol collectionClass, 
        INamedTypeSymbol baseType,
        List<INamedTypeSymbol> optionTypes, 
        Compilation compilation)
    {
        // Extract attribute data
        var attr = collectionClass.GetAttributes().FirstOrDefault(a => 
            a.AttributeClass != null &&
            (string.Equals(a.AttributeClass.ToDisplayString(), typeof(EnumCollectionAttribute).FullName, StringComparison.Ordinal) ||
             string.Equals(a.AttributeClass.ToDisplayString(), typeof(GlobalEnumCollectionAttribute).FullName, StringComparison.Ordinal)));
        if (attr == null)
            return null;
            
        var named = attr.NamedArguments.ToDictionary(kvp => kvp.Key, kvp => kvp.Value, StringComparer.Ordinal);
        
        // Extract properties from the attribute
        var useSingletonInstances = named.TryGetValue("UseSingletonInstances", out var usiVal) && usiVal.Value is bool usi && usi;
        var defaultGenericReturnType = named.TryGetValue("DefaultGenericReturnType", out var dgrtVal) && dgrtVal.Value is INamedTypeSymbol dgrt ? dgrt.ToDisplayString() : null;
        
        // Check if the collection class inherits from EnumCollectionBase<T>
        var inheritsFromBase = CheckIfInheritsFromEnumCollectionBase(collectionClass, compilation);
        
        // Build EnumTypeInfoModel from the collection class
        var defaultNamespace = collectionClass.ContainingNamespace?.ToDisplayString() ?? string.Empty;
        var enumTypeInfo = new EnumTypeInfoModel
        {
            Namespace = defaultNamespace,
            ClassName = baseType.Name,
            FullTypeName = baseType.ToDisplayString(),
            CollectionName = ExtractCollectionName(attr, collectionClass),
            CollectionBaseType = baseType.ToDisplayString(),
            IsGenericType = baseType.IsGenericType,
            GenerateFactoryMethods = !useSingletonInstances, // Factory methods when not using singletons
            GenerateStaticCollection = true,
            Generic = false,
            NameComparison = StringComparison.OrdinalIgnoreCase,
            // Respect the explicit attribute setting over inheritance requirements
            UseSingletonInstances = useSingletonInstances, // Use the explicit attribute setting
            UseDictionaryStorage = true, // Always use dictionary for O(1) lookups
            ReturnType = defaultGenericReturnType,
            ReturnTypeNamespace = defaultGenericReturnType != null ? ExtractNamespaceFromType(defaultGenericReturnType) : null,
            DefaultGenericReturnType = defaultGenericReturnType,
            DefaultGenericReturnTypeNamespace = defaultGenericReturnType != null ? ExtractNamespaceFromType(defaultGenericReturnType) : null,
            LookupProperties = ExtractLookupPropertiesFromBaseType(baseType),
            InheritsFromCollectionBase = inheritsFromBase,
            CollectionClassName = collectionClass.ToDisplayString()
        };
        
        return enumTypeInfo;
    }

    private static string? ExtractNamespaceFromType(string fullTypeName)
    {
        var lastDotIndex = fullTypeName.LastIndexOf('.');
        return lastDotIndex > 0 ? fullTypeName.Substring(0, lastDotIndex) : null;
    }

    private static string ExtractCollectionName(AttributeData attr, INamedTypeSymbol collectionClass)
    {
        // Check CollectionName property from named arguments
        var named = attr.NamedArguments.ToDictionary(kvp => kvp.Key, kvp => kvp.Value, StringComparer.Ordinal);
        if (named.TryGetValue("CollectionName", out var cnVal) && cnVal.Value is string collectionName && !string.IsNullOrEmpty(collectionName))
        {
            return collectionName;
        }
        
        // Derive from class name
        var className = collectionClass.Name;
        if (className.EndsWith("CollectionBase", StringComparison.Ordinal))
        {
            return className.Substring(0, className.Length - "CollectionBase".Length);
        }
        
        return className + "s"; // Default: add 's'
    }

    private static EquatableArray<PropertyLookupInfoModel> ExtractLookupPropertiesFromBaseType(INamedTypeSymbol baseType)
    {
        var lookupProperties = new List<PropertyLookupInfoModel>();
        
        // Traverse the inheritance chain to find all properties with [EnumLookup] attributes
        var currentType = baseType;
        while (currentType != null)
        {
            foreach (var prop in currentType.GetMembers().OfType<IPropertySymbol>())
            {
                var lookupAttr = prop.GetAttributes()
                    .FirstOrDefault(ad => string.Equals(ad.AttributeClass?.Name, nameof(EnumLookupAttribute), StringComparison.Ordinal) ||
                                         string.Equals(ad.AttributeClass?.Name, "EnumLookup", StringComparison.Ordinal));
                if (lookupAttr == null)
                    continue;
                
                // Get constructor arguments - MethodName is the first parameter
                var constructorArgs = lookupAttr.ConstructorArguments;
                var methodName = constructorArgs.Length > 0 && constructorArgs[0].Value is string mn 
                    ? mn : $"GetBy{prop.Name}";
                
                var allowMultiple = constructorArgs.Length > 1 && constructorArgs[1].Value is bool mu && mu;
                var returnType = constructorArgs.Length > 2 && constructorArgs[2].Value is INamedTypeSymbol rts 
                    ? rts.ToDisplayString() : null;

                lookupProperties.Add(new PropertyLookupInfoModel
                {
                    PropertyName = prop.Name,
                    PropertyType = prop.Type.ToDisplayString(),
                    LookupMethodName = methodName,
                    AllowMultiple = allowMultiple,
                    IsNullable = prop.Type.NullableAnnotation == NullableAnnotation.Annotated,
                    ReturnType = returnType,
                    RequiresOverride = prop.IsAbstract,
                });
            }
            
            currentType = currentType.BaseType;
        }
        
        return new EquatableArray<PropertyLookupInfoModel>(lookupProperties);
    }

    private static void Execute(
        SourceProductionContext context,
        EnumTypeInfoModel def,
        Compilation compilation,
        ImmutableArray<INamedTypeSymbol> discoveredOptionTypes)
    {
        if (def == null)
            throw new ArgumentNullException(nameof(def));
        if (compilation == null)
            throw new ArgumentNullException(nameof(compilation));

        var baseTypeSymbol = compilation.GetTypeByMetadataName(def.CollectionBaseType ?? def.FullTypeName);
        if (baseTypeSymbol == null)
            return;

        // Auto-detect return type if not specified
        if (string.IsNullOrEmpty(def.ReturnType))
        {
            def.ReturnType = DetectReturnType(baseTypeSymbol, compilation);
        }

        // Convert discovered option types to EnumValueInfoModel objects
        var values = new List<EnumValueInfoModel>();
        foreach (var optionType in discoveredOptionTypes)
        {
            // Extract name from EnumOption attribute, fallback to class name
            var enumOptionAttr = optionType.GetAttributes()
                .FirstOrDefault(ad => string.Equals(ad.AttributeClass?.Name, "EnumOptionAttribute", StringComparison.Ordinal) ||
                                      string.Equals(ad.AttributeClass?.Name, "EnumOption", StringComparison.Ordinal));
            
            var name = enumOptionAttr != null 
                ? EnumAttributeParser.ParseEnumOption(enumOptionAttr, optionType)
                : optionType.Name;

            var enumValueInfo = new EnumValueInfoModel
            {
                ShortTypeName = optionType.Name,
                FullTypeName = optionType.ToDisplayString(),
                Name = name,
                ReturnTypeNamespace = optionType.ContainingNamespace?.ToDisplayString() ?? string.Empty
            };
            values.Add(enumValueInfo);
        }

        // Generate the collection class
        GenerateCollection(context, def, new EquatableArray<EnumValueInfoModel>(values), compilation);
    }

    private static string DetectReturnType(INamedTypeSymbol baseTypeSymbol, Compilation compilation)
    {
        // For collections inheriting from EnumCollectionBase<T>, just use T directly
        // The base type symbol passed in is already the correct return type
        // (e.g., ProcessStateBase from EnumCollectionBase<ProcessStateBase>)
        return baseTypeSymbol.ToDisplayString();
    }
    
    private static void GenerateCollection(SourceProductionContext context, EnumTypeInfoModel def, EquatableArray<EnumValueInfoModel> values, Compilation compilation)
    {
        if (def == null)
            throw new ArgumentNullException(nameof(def));

        var baseTypeSymbol = GetBaseTypeSymbol(def, compilation);
        var effectiveReturnType = DetermineEffectiveReturnType(def, baseTypeSymbol, compilation);

        // Use the enhanced EnumCollectionBuilder directly
        var builder = new EnumCollectionBuilder();

        var generatedCode = builder
            .WithDefinition(def)
            .WithValues(values.ToList())
            .WithReturnType(effectiveReturnType!)
            .WithCompilation(compilation)
            .Build();
        
        var fileName = $"{def.CollectionName}.g.cs";
        context.AddSource(fileName, generatedCode);
        
        // Conditionally emit to disk if GeneratorOutPutTo is specified
        EmitFileToDiskIfRequested(context, fileName, generatedCode);
    }

    private static INamedTypeSymbol? GetBaseTypeSymbol(EnumTypeInfoModel def, Compilation compilation)
    {
        return def.IsGenericType && !string.IsNullOrEmpty(def.UnboundTypeName)
            ? compilation.GetTypeByMetadataName(def.UnboundTypeName)
            : compilation.GetTypeByMetadataName(def.FullTypeName);
    }

    private static string? DetermineEffectiveReturnType(EnumTypeInfoModel def, INamedTypeSymbol? baseTypeSymbol, Compilation compilation)
    {
        if (!string.IsNullOrEmpty(def.ReturnType))
            return def.ReturnType;
        
        if (def.IsGenericType && !string.IsNullOrEmpty(def.DefaultGenericReturnType))
            return def.DefaultGenericReturnType;
        
        return baseTypeSymbol != null ? DetectReturnType(baseTypeSymbol, compilation) : def.FullTypeName;
    }

    /// <summary>
    /// Conditionally emits the generated file to disk if GeneratorOutPutTo MSBuild property is set.
    /// </summary>
    private static void EmitFileToDiskIfRequested(SourceProductionContext context, string fileName, string content)
    {
        // File I/O removed - not allowed in source generators
    }
}
