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
/// Source generator for Static Enhanced Enum collections that always inherits base methods.
/// 
/// LOCAL DISCOVERY:
/// - Uses [StaticEnumCollection] attribute
/// - Scans the current compilation for enum options
/// - Always inherits base class methods (no optionality)
/// 
/// GLOBAL DISCOVERY:
/// - Uses [GlobalStaticEnumCollection] attribute  
/// - Scans ALL referenced assemblies for enum options
/// - Always inherits base class methods (no optionality)
/// - Enables cross-assembly enum composition patterns
/// </summary>
/// <remarks>
/// This generator automatically reconstructs base class methods without requiring InheritsBaseMethods attribute.
/// It's designed for scenarios where method inheritance is always desired.
/// </remarks>
[Generator]
public class StaticEnumCollectionGenerator : IIncrementalGenerator
{
    // Cache for assembly types to avoid re-scanning
    private static readonly ConcurrentDictionary<string, List<INamedTypeSymbol>> _assemblyTypeCache = new(StringComparer.Ordinal);

    /// <summary>
    /// Initializes the incremental source generator.
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
            ctx.AddSource("StaticEnumCollectionGenerator_Debug.cs", 
                "// StaticEnumCollectionGenerator Initialize() was called - generator is loaded!");
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
            var isGlobal = HasGlobalStaticEnumCollectionAttribute(collectionClass);
            var baseType = ExtractBaseTypeFromCollection(collectionClass);
            
            if (baseType == null)
                continue;
                
            var optionTypes = new HashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default);
            
            if (isGlobal)
            {
                // Global discovery - scan all referenced assemblies
                var allAssemblies = new HashSet<IAssemblySymbol>(SymbolEqualityComparer.Default)
                {
                    compilation.Assembly
                };
                
                foreach (var reference in compilation.References)
                {
                    if (compilation.GetAssemblyOrModuleSymbol(reference) is IAssemblySymbol assemblySymbol)
                    {
                        allAssemblies.Add(assemblySymbol);
                    }
                }
                
                // Scan all assemblies for option types
                foreach (var assembly in allAssemblies)
                {
                    ScanForOptionTypes(assembly.GlobalNamespace, baseType, optionTypes);
                }
            }
            else
            {
                // Local discovery - only scan current compilation
                ScanForOptionTypes(compilation.GlobalNamespace, baseType, optionTypes);
            }
            
            // Create the enum type info model with forced method inheritance
            var enumTypeInfo = CreateEnumTypeInfo(collectionClass, baseType, optionTypes);
            
            if (enumTypeInfo != null)
            {
                results.Add(new EnumTypeInfoWithCompilation(enumTypeInfo, compilation, optionTypes.ToImmutableArray()));
            }
        }
        
        return results.ToImmutableArray();
    }

    /// <summary>
    /// Scans for collection classes with StaticEnumCollection or GlobalStaticEnumCollection attributes.
    /// </summary>
    private static void ScanForCollectionClasses(INamespaceSymbol namespaceSymbol, HashSet<INamedTypeSymbol> collectionClasses)
    {
        foreach (var member in namespaceSymbol.GetMembers())
        {
            if (member is INamedTypeSymbol typeSymbol)
            {
                if (HasStaticEnumCollectionAttribute(typeSymbol) || HasGlobalStaticEnumCollectionAttribute(typeSymbol))
                {
                    collectionClasses.Add(typeSymbol);
                }
            }
            else if (member is INamespaceSymbol childNamespace)
            {
                ScanForCollectionClasses(childNamespace, collectionClasses);
            }
        }
    }

    /// <summary>
    /// Checks if a type has the StaticEnumCollection attribute.
    /// </summary>
    private static bool HasStaticEnumCollectionAttribute(INamedTypeSymbol typeSymbol)
    {
        return typeSymbol.GetAttributes().Any(attr =>
            attr.AttributeClass != null &&
            string.Equals(attr.AttributeClass.ToDisplayString(), typeof(StaticEnumCollectionAttribute).FullName, StringComparison.Ordinal));
    }

    /// <summary>
    /// Checks if a type has the GlobalStaticEnumCollection attribute.
    /// </summary>
    private static bool HasGlobalStaticEnumCollectionAttribute(INamedTypeSymbol typeSymbol)
    {
        return typeSymbol.GetAttributes().Any(attr =>
            attr.AttributeClass != null &&
            string.Equals(attr.AttributeClass.ToDisplayString(), typeof(GlobalStaticEnumCollectionAttribute).FullName, StringComparison.Ordinal));
    }

    /// <summary>
    /// Extracts the base type from a collection class (e.g., EnumCollectionBase&lt;T&gt; -> T).
    /// </summary>
    private static INamedTypeSymbol? ExtractBaseTypeFromCollection(INamedTypeSymbol collectionClass)
    {
        var baseType = collectionClass.BaseType;
        while (baseType != null)
        {
            if (string.Equals(baseType.Name, "EnumCollectionBase", StringComparison.Ordinal) && baseType.TypeArguments.Length == 1)
            {
                return baseType.TypeArguments[0] as INamedTypeSymbol;
            }
            baseType = baseType.BaseType;
        }
        return null;
    }

    /// <summary>
    /// Recursively scans for option types that derive from the specified base type.
    /// </summary>
    private static void ScanForOptionTypes(INamespaceSymbol namespaceSymbol, INamedTypeSymbol baseType, HashSet<INamedTypeSymbol> optionTypes)
    {
        foreach (var member in namespaceSymbol.GetMembers())
        {
            if (member is INamedTypeSymbol typeSymbol && !typeSymbol.IsAbstract)
            {
                if (IsDerivedFrom(typeSymbol, baseType))
                {
                    optionTypes.Add(typeSymbol);
                }
            }
            else if (member is INamespaceSymbol childNamespace)
            {
                ScanForOptionTypes(childNamespace, baseType, optionTypes);
            }
        }
    }

    /// <summary>
    /// Checks if a type derives from the specified base type.
    /// </summary>
    private static bool IsDerivedFrom(INamedTypeSymbol typeSymbol, INamedTypeSymbol baseType)
    {
        var current = typeSymbol.BaseType;
        while (current != null)
        {
            if (SymbolEqualityComparer.Default.Equals(current, baseType))
            {
                return true;
            }
            current = current.BaseType;
        }
        return false;
    }

    /// <summary>
    /// Creates an EnumTypeInfoModel with forced method inheritance.
    /// </summary>
    private static EnumTypeInfoModel? CreateEnumTypeInfo(INamedTypeSymbol collectionClass, INamedTypeSymbol baseType, HashSet<INamedTypeSymbol> optionTypes)
    {
        // Get the collection name from attribute
        var collectionName = GetCollectionName(collectionClass);
        if (string.IsNullOrEmpty(collectionName))
            return null;

        // Get the default generic return type from attribute (if any)
        var defaultGenericReturnType = GetDefaultGenericReturnType(collectionClass);

        // Create enum options using EnumValueInfoModel
        var enumOptions = new List<EnumValueInfoModel>();
        foreach (var optionType in optionTypes.OrderBy(t => t.Name, StringComparer.Ordinal))
        {
            var constructor = optionType.Constructors.FirstOrDefault(c => c.Parameters.Length == 0 && !c.IsStatic);
            if (constructor != null)
            {
                // Extract name from EnumOption attribute, fallback to class name
                var enumOptionAttr = optionType.GetAttributes()
                    .FirstOrDefault(ad => string.Equals(ad.AttributeClass?.Name, "EnumOptionAttribute", StringComparison.Ordinal) ||
                                          string.Equals(ad.AttributeClass?.Name, "EnumOption", StringComparison.Ordinal));
                
                var name = enumOptionAttr != null 
                    ? EnumAttributeParser.ParseEnumOption(enumOptionAttr, optionType)
                    : optionType.Name;

                enumOptions.Add(new EnumValueInfoModel
                {
                    Name = name,
                    FullTypeName = optionType.ToDisplayString(),
                    ShortTypeName = optionType.Name,
                    ReturnTypeNamespace = optionType.ContainingNamespace?.ToDisplayString() ?? string.Empty,
                    Include = true
                });
            }
        }

        return new EnumTypeInfoModel
        {
            FullTypeName = baseType?.ToDisplayString() ?? string.Empty,
            CollectionName = collectionName!,
            Namespace = collectionClass.ContainingNamespace?.ToDisplayString() ?? "Global",
            ConcreteTypes = new EquatableArray<CollectionValueInfoModel>(enumOptions.Cast<CollectionValueInfoModel>().ToArray()),
            DefaultGenericReturnType = defaultGenericReturnType,
            InheritsFromCollectionBase = true, // ALWAYS TRUE for StaticEnumCollection
            UseSingletonInstances = GetUseSingletonInstances(collectionClass)
        };
    }

    /// <summary>
    /// Gets the collection name from the StaticEnumCollection or GlobalStaticEnumCollection attribute.
    /// </summary>
    private static string? GetCollectionName(INamedTypeSymbol collectionClass)
    {
        var attribute = collectionClass.GetAttributes().FirstOrDefault(attr =>
            attr.AttributeClass != null &&
            (string.Equals(attr.AttributeClass.ToDisplayString(), typeof(StaticEnumCollectionAttribute).FullName, StringComparison.Ordinal) ||
             string.Equals(attr.AttributeClass.ToDisplayString(), typeof(GlobalStaticEnumCollectionAttribute).FullName, StringComparison.Ordinal)));

        if (attribute != null)
        {
            // Try named argument first
            var namedArg = attribute.NamedArguments.FirstOrDefault(arg => string.Equals(arg.Key, "CollectionName", StringComparison.Ordinal));
            if (namedArg.Value.Value is string namedValue)
                return namedValue;

            // Fall back to constructor argument
            if (attribute.ConstructorArguments.Length > 0 && attribute.ConstructorArguments[0].Value is string ctorValue)
                return ctorValue;
        }

        return null;
    }

    /// <summary>
    /// Gets the DefaultGenericReturnType from the attribute.
    /// </summary>
    private static string? GetDefaultGenericReturnType(INamedTypeSymbol collectionClass)
    {
        var attribute = collectionClass.GetAttributes().FirstOrDefault(attr =>
            attr.AttributeClass != null &&
            (string.Equals(attr.AttributeClass.ToDisplayString(), typeof(StaticEnumCollectionAttribute).FullName, StringComparison.Ordinal) ||
             string.Equals(attr.AttributeClass.ToDisplayString(), typeof(GlobalStaticEnumCollectionAttribute).FullName, StringComparison.Ordinal)));

        if (attribute != null)
        {
            var namedArg = attribute.NamedArguments.FirstOrDefault(arg => string.Equals(arg.Key, "DefaultGenericReturnType", StringComparison.Ordinal));
            if (namedArg.Value.Value is INamedTypeSymbol typeSymbol)
                return typeSymbol.ToDisplayString();
        }

        return null;
    }

    /// <summary>
    /// Gets the UseSingletonInstances flag from the attribute.
    /// </summary>
    private static bool GetUseSingletonInstances(INamedTypeSymbol collectionClass)
    {
        var attribute = collectionClass.GetAttributes().FirstOrDefault(attr =>
            attr.AttributeClass != null &&
            (string.Equals(attr.AttributeClass.ToDisplayString(), typeof(StaticEnumCollectionAttribute).FullName, StringComparison.Ordinal) ||
             string.Equals(attr.AttributeClass.ToDisplayString(), typeof(GlobalStaticEnumCollectionAttribute).FullName, StringComparison.Ordinal)));

        if (attribute != null)
        {
            var namedArg = attribute.NamedArguments.FirstOrDefault(arg => string.Equals(arg.Key, "UseSingletonInstances", StringComparison.Ordinal));
            if (namedArg.Value.Value is bool value)
                return value;
        }

        return false;
    }

    /// <summary>
    /// Executes the code generation for a single enum collection.
    /// </summary>
    private static void Execute(SourceProductionContext context, EnumTypeInfoModel enumTypeInfo, Compilation compilation, ImmutableArray<INamedTypeSymbol> discoveredOptionTypes)
    {
        try
        {
            // Always use method reconstruction since InheritsFromCollectionBase is always true
            var builder = new EnumCollectionBuilder();
            var director = new EnumCollectionDirector(builder);
            
            // Use ConstructSimplifiedCollection since InheritsFromCollectionBase is true
            var generatedCode = director.ConstructSimplifiedCollection(
                enumTypeInfo, 
                enumTypeInfo.ConcreteTypes.Cast<EnumValueInfoModel>().ToList(), 
                enumTypeInfo.DefaultGenericReturnType ?? enumTypeInfo.FullTypeName, 
                compilation);

            if (!string.IsNullOrWhiteSpace(generatedCode))
            {
                var fileName = $"{enumTypeInfo.CollectionName}_StaticGenerated.g.cs";
                context.AddSource(fileName, generatedCode);
            }
        }
        catch (Exception ex)
        {
            var diagnostic = Diagnostic.Create(
                new DiagnosticDescriptor(
                    "SESG001",
                    "Static Enum Collection Generation Error",
                    $"Error generating static enum collection: {ex.Message}",
                    "StaticEnumCollectionGenerator",
                    DiagnosticSeverity.Error,
                    isEnabledByDefault: true),
                Location.None);
            
            context.ReportDiagnostic(diagnostic);
        }
    }

    /// <summary>
    /// Internal model for passing data between discovery and generation phases.
    /// </summary>
    private sealed record EnumTypeInfoWithCompilation(
        EnumTypeInfoModel EnumTypeInfoModel,
        Compilation Compilation,
        ImmutableArray<INamedTypeSymbol> DiscoveredOptionTypes);
}