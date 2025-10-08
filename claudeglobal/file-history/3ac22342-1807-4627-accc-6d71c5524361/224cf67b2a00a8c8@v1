using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using FractalDataWorks.Messages.SourceGenerators.Services.Builders;
using FractalDataWorks.Messages.Attributes;
using FractalDataWorks.Messages.SourceGenerators.Models;
using FractalDataWorks.SourceGenerators.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace FractalDataWorks.Messages.SourceGenerators.Generators;

/// <summary>
/// Source generator for Enhanced Enums that supports both local and cross-assembly discovery.
/// 
/// LOCAL DISCOVERY (default):
/// - Uses [MessageCollection] attribute
/// - Scans the current compilation for enum options
/// 
/// GLOBAL DISCOVERY (opt-in):
/// - Uses [GlobalMessageCollection] attribute  
/// - Scans ALL referenced assemblies for enum options
/// - Enables cross-assembly enum composition patterns
/// </summary>
/// <remarks>
/// This code is excluded from code coverage because source generators run at compile-time and cannot be unit tested via runtime tests.
/// </remarks>
[Generator]

public class MessageCollectionGenerator : IIncrementalGenerator
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
            ctx.AddSource("MessageCollectionGenerator_Debug.cs", 
                "// MessageCollectionGenerator Initialize() was called - generator is loaded!");
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
                Execute(context, enumDefinition.CollectionTypeInfoModel, enumDefinition.Compilation, enumDefinition.DiscoveredOptionTypes);
            }
        });
    }

    /// <summary>
    /// Discovers all collection definitions by scanning for classes with [MessageCollection] or [GlobalMessageCollection] attributes.
    /// </summary>
    private static ImmutableArray<MessageTypeInfoWithCompilation> DiscoverAllCollectionDefinitions(Compilation compilation)
    {
        var results = new List<MessageTypeInfoWithCompilation>();

        // Step 1: Scan for collection classes with [MessageCollection] or [GlobalMessageCollection]
        var collectionClasses = new HashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default);
        
        // Always scan current compilation
        ScanForCollectionClasses(compilation.GlobalNamespace, collectionClasses);
        
        // For each collection class found, determine if we should scan globally
        foreach (var collectionClass in collectionClasses)
        {
            var isGlobal = HasGlobalMessageCollectionAttribute(collectionClass);
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
                    results.Add(new MessageTypeInfoWithCompilation(enumDefinition, compilation, optionTypes.ToList()));
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
            string.Equals(attr.AttributeClass?.Name, nameof(MessageCollectionAttribute), StringComparison.Ordinal) ||
            string.Equals(attr.AttributeClass?.Name, "MessageCollection", StringComparison.Ordinal) ||
            string.Equals(attr.AttributeClass?.Name, nameof(GlobalMessageCollectionAttribute), StringComparison.Ordinal) ||
            string.Equals(attr.AttributeClass?.Name, "GlobalMessageCollection", StringComparison.Ordinal));
    }

    private static bool HasGlobalMessageCollectionAttribute(INamedTypeSymbol type)
    {
        return type.GetAttributes().Any(attr =>
            string.Equals(attr.AttributeClass?.Name, nameof(GlobalMessageCollectionAttribute), StringComparison.Ordinal) ||
            string.Equals(attr.AttributeClass?.Name, "GlobalMessageCollection", StringComparison.Ordinal));
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
        
        // For classes that inherit from MessageCollectionBase<T>, extract T
        // e.g., DocumentTypeCollection : MessageCollectionBase<DocumentTypeBase>
        var currentBase = collectionClass.BaseType;
        while (currentBase != null)
        {
            if (currentBase.IsGenericType && currentBase.TypeArguments.Length > 0)
            {
                // Check if this is MessageCollectionBase<T>
                var constructedFrom = currentBase.ConstructedFrom;
                if (constructedFrom != null && 
                    (string.Equals(constructedFrom.Name, "MessageCollectionBase", StringComparison.Ordinal) || 
                     constructedFrom.ToDisplayString().Contains("MessageCollectionBase")))
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
                // Check for [MessageOption] attribute is optional - any concrete type deriving from base is included
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
        // First check direct inheritance
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
        
        // Also check if baseType is an interface and derivedType implements it
        if (baseType.TypeKind == TypeKind.Interface)
        {
            foreach (var implementedInterface in derivedType.AllInterfaces)
            {
                if (SymbolEqualityComparer.Default.Equals(implementedInterface, baseType))
                {
                    return true;
                }
                
                // For generic interfaces, compare the unbound generic type
                if (baseType.IsGenericType && implementedInterface.IsGenericType)
                {
                    var interfaceUnbound = implementedInterface.ConstructedFrom ?? implementedInterface;
                    var baseUnbound = baseType.ConstructedFrom ?? baseType;
                    
                    if (SymbolEqualityComparer.Default.Equals(interfaceUnbound, baseUnbound))
                    {
                        return true;
                    }
                }
            }
        }
        
        return false;
    }

    private static bool CheckIfInheritsFromMessageCollectionBase(INamedTypeSymbol collectionClass, Compilation compilation)
    {
        // Get the MessageCollectionBase<T> type from the compilation
        var MessageCollectionBase = compilation.GetTypeByMetadataName("FractalDataWorks.Messages.MessageCollectionBase`1");
        if (MessageCollectionBase == null)
            return false;
        
        // Check if the collection class inherits from MessageCollectionBase<T>
        var currentBase = collectionClass.BaseType;
        while (currentBase != null)
        {
            // Check for generic match
            if (currentBase.IsGenericType && currentBase.ConstructedFrom != null)
            {
                if (SymbolEqualityComparer.Default.Equals(currentBase.ConstructedFrom, MessageCollectionBase))
                {
                    return true;
                }
            }
            
            currentBase = currentBase.BaseType;
        }
        
        return false;
    }

    private static CollectionTypeInfoModel? BuildEnumDefinitionFromCollection(
        INamedTypeSymbol collectionClass, 
        INamedTypeSymbol baseType,
        List<INamedTypeSymbol> optionTypes, 
        Compilation compilation)
    {
        // Extract attribute data
        var attr = collectionClass.GetAttributes().FirstOrDefault(a => 
            string.Equals(a.AttributeClass?.Name, nameof(MessageCollectionAttribute), StringComparison.Ordinal) ||
            string.Equals(a.AttributeClass?.Name, "MessageCollection", StringComparison.Ordinal) ||
            string.Equals(a.AttributeClass?.Name, nameof(GlobalMessageCollectionAttribute), StringComparison.Ordinal) ||
            string.Equals(a.AttributeClass?.Name, "GlobalMessageCollection", StringComparison.Ordinal));
        if (attr == null)
            return null;
            
        var named = attr.NamedArguments.ToDictionary(kvp => kvp.Key, kvp => kvp.Value, StringComparer.Ordinal);
        
        // Extract properties from the attribute - NO OPTIONS, always factory methods
        var returnType = named.TryGetValue("ReturnType", out var rtVal) && rtVal.Value is INamedTypeSymbol rt ? rt.ToDisplayString() : null;
        
        // Check if the collection class inherits from MessageCollectionBase<T>
        var inheritsFromBase = CheckIfInheritsFromMessageCollectionBase(collectionClass, compilation);
        
        // Build CollectionTypeInfoModel from the collection class
        var defaultNamespace = collectionClass.ContainingNamespace?.ToDisplayString() ?? string.Empty;
        var MessageTypeInfo = new CollectionTypeInfoModel
        {
            Namespace = defaultNamespace,
            ClassName = baseType.Name,
            FullTypeName = baseType.ToDisplayString(),
            CollectionName = ExtractCollectionName(attr, collectionClass),
            CollectionBaseType = baseType.ToDisplayString(),
            IsGenericType = baseType.IsGenericType,
            GenerateFactoryMethods = true, // ALWAYS generate factory methods - no options
            GenerateStaticCollection = false, // Don't generate static collection
            Generic = false,
            NameComparison = StringComparison.OrdinalIgnoreCase,
            UseSingletonInstances = false, // Never use singletons - always factory methods
            UseDictionaryStorage = false, // No dictionary storage needed
            ReturnType = returnType,
            ReturnTypeNamespace = returnType != null ? ExtractNamespaceFromType(returnType) : null,
            DefaultGenericReturnType = returnType,
            DefaultGenericReturnTypeNamespace = returnType != null ? ExtractNamespaceFromType(returnType) : null,
            InheritsFromCollectionBase = inheritsFromBase,
            CollectionClassName = collectionClass.ToDisplayString()
        };
        
        return MessageTypeInfo;
    }

    private static string? ExtractNamespaceFromType(string fullTypeName)
    {
        var lastDotIndex = fullTypeName.LastIndexOf('.');
        return lastDotIndex > 0 ? fullTypeName.Substring(0, lastDotIndex) : null;
    }

    private static string ExtractCollectionName(AttributeData attr, INamedTypeSymbol collectionClass)
    {
        // First, check the first constructor argument (the 'name' parameter)
        if (attr.ConstructorArguments.Length > 0 && attr.ConstructorArguments[0].Value is string constructorName && !string.IsNullOrEmpty(constructorName))
        {
            return constructorName;
        }
        
        // Then check the Name property from named arguments
        var named = attr.NamedArguments.ToDictionary(kvp => kvp.Key, kvp => kvp.Value, StringComparer.Ordinal);
        if (named.TryGetValue("Name", out var nameVal) && nameVal.Value is string namedName && !string.IsNullOrEmpty(namedName))
        {
            return namedName;
        }
        
        // No fallback - collection name is required
        throw new InvalidOperationException($"MessageCollection attribute on {collectionClass.Name} must specify a collection name as the first constructor parameter.");
    }

    private static void Execute(
        SourceProductionContext context, 
        CollectionTypeInfoModel def, 
        Compilation compilation,
        List<INamedTypeSymbol> discoveredOptionTypes)
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

        // Convert discovered option types to CollectionValueInfoModel objects
        var values = new List<CollectionValueInfoModel>();
        foreach (var optionType in discoveredOptionTypes)
        {
            var enumValueInfo = new CollectionValueInfoModel
            {
                ShortTypeName = optionType.Name,
                FullTypeName = optionType.ToDisplayString(),
                Name = optionType.Name,
                ReturnTypeNamespace = optionType.ContainingNamespace?.ToDisplayString() ?? string.Empty,
                Constructors = ExtractConstructors(optionType)
            };
            values.Add(enumValueInfo);
        }

        // Generate the collection class
        GenerateCollection(context, def, new EquatableArray<CollectionValueInfoModel>(values), compilation);
    }

    private static string DetectReturnType(INamedTypeSymbol baseTypeSymbol, Compilation compilation)
    {
        // Get IMessageOption interface from FractalDataWorks core
        var enhancedEnumInterface = compilation.GetTypeByMetadataName("FractalDataWorks.IMessageOption") 
            ?? compilation.GetTypeByMetadataName("FractalDataWorks.Messages.IMessageOption");
        if (enhancedEnumInterface == null)
        {
            return baseTypeSymbol.ToDisplayString();
        }

        // Check all interfaces implemented by the base type
        foreach (var iface in baseTypeSymbol.AllInterfaces)
        {
            if (iface.AllInterfaces.Contains(enhancedEnumInterface, SymbolEqualityComparer.Default))
            {
                return iface.ToDisplayString();
            }
            
            if (SymbolEqualityComparer.Default.Equals(iface, enhancedEnumInterface))
            {
                continue;
            }
        }

        return baseTypeSymbol.ToDisplayString();
    }
    
    private static void GenerateCollection(SourceProductionContext context, CollectionTypeInfoModel def, EquatableArray<CollectionValueInfoModel> values, Compilation compilation)
    {
        if (def == null)
            throw new ArgumentNullException(nameof(def));

        var baseTypeSymbol = GetBaseTypeSymbol(def, compilation);
        var effectiveReturnType = DetermineEffectiveReturnType(def, baseTypeSymbol, compilation);

        // Use builder directly - no director needed
        var builder = new MessageCollectionBuilder();

#if Debug
        // Write debug info to see what's being passed to builder
        var debugInfo = $@"/*
Debug Info for {def.CollectionName}
=====================================
Definition:
{def.ToString()}

Values Count: {values.Count()}
Values:
{string.Join("\n", values.Select(v => $"  {v.ToString()}"))}

Effective Return Type: {effectiveReturnType ?? "None"}
*/";
        
        context.AddSource($"{def.CollectionName}Info.g.cs", debugInfo);
#endif
        
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

    private static INamedTypeSymbol? GetBaseTypeSymbol(CollectionTypeInfoModel def, Compilation compilation)
    {
        return def.IsGenericType && !string.IsNullOrEmpty(def.UnboundTypeName)
            ? compilation.GetTypeByMetadataName(def.UnboundTypeName)
            : compilation.GetTypeByMetadataName(def.FullTypeName);
    }

    private static string DetermineEffectiveReturnType(CollectionTypeInfoModel def, INamedTypeSymbol? baseTypeSymbol, Compilation compilation)
    {
        if (!string.IsNullOrEmpty(def.ReturnType))
            return def.ReturnType!;
        
        if (def.IsGenericType && !string.IsNullOrEmpty(def.DefaultGenericReturnType))
            return def.DefaultGenericReturnType!;
        
        // Default to IFractalMessage if no return type specified
        return "FractalDataWorks.Messages.IFractalMessage";
    }

    /// <summary>
    /// Extracts constructor information from a type symbol.
    /// </summary>
    private static List<ConstructorInfo> ExtractConstructors(INamedTypeSymbol typeSymbol)
    {
        var constructors = new List<ConstructorInfo>();
        
        foreach (var constructor in typeSymbol.Constructors)
        {
            // Only include public constructors
            if (constructor.DeclaredAccessibility != Accessibility.Public)
                continue;
                
            var constructorInfo = new ConstructorInfo
            {
                Accessibility = constructor.DeclaredAccessibility,
                IsPrimary = constructor.IsImplicitlyDeclared == false,
                Parameters = new List<ParameterInfo>()
            };
            
            foreach (var parameter in constructor.Parameters)
            {
                var parameterInfo = new ParameterInfo
                {
                    Name = parameter.Name,
                    TypeName = parameter.Type.ToDisplayString(),
                    HasDefaultValue = parameter.HasExplicitDefaultValue,
                    DefaultValue = parameter.HasExplicitDefaultValue ? parameter.ExplicitDefaultValue?.ToString() : null,
                    Namespace = parameter.Type.ContainingNamespace?.ToDisplayString()
                };
                constructorInfo.Parameters.Add(parameterInfo);
            }
            
            constructors.Add(constructorInfo);
        }
        
        return constructors;
    }

    /// <summary>
    /// Conditionally emits the generated file to disk if GeneratorOutPutTo MSBuild property is set.
    /// </summary>
    private static void EmitFileToDiskIfRequested(SourceProductionContext context, string fileName, string content)
    {
        // File I/O removed - not allowed in source generators
    }
}
