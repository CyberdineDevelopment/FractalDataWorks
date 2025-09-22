using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using FractalDataWorks.ServiceTypes.SourceGenerators.Models;
using FractalDataWorks.ServiceTypes.SourceGenerators.Services.Builders;
using FractalDataWorks.SourceGenerators.Models;
using FractalDataWorks.SourceGenerators.Services;

namespace FractalDataWorks.ServiceTypes.SourceGenerators.Generators;

/// <summary>
/// ServiceTypeCollectionGenerator - Specialized generator for ServiceType collections.
/// 
/// DISCOVERY STRATEGY:
/// - Looks for classes that inherit from ServiceTypeCollectionBase&lt;TBase,TGeneric,TService,TConfiguration,TFactory&gt;
/// - Uses inheritance-based detection (no attributes required)
/// - Global assembly scanning for comprehensive service discovery
/// - Generates high-performance collections with FrozenDictionary support
/// 
/// NAMING CONVENTION:
/// - ConnectionTypeCollectionBase -&gt; generates ConnectionTypes (removes "CollectionBase" suffix)
/// - Uses the collection class's namespace
/// - Creates partial class for the generated name
/// 
/// GENERATED API:
/// - ConnectionTypes.All() -&gt; IReadOnlyList&lt;ConnectionTypeBase&gt;
/// - ConnectionTypes.Empty() -> EmptyConnectionType instance
/// - ConnectionTypes.Name(string) / Id(int) -> lookup with _empty fallback
/// - ConnectionTypes.GetByServiceType/ConfigurationType/SectionName -> attribute-based lookups
/// - ConnectionTypes.Create{TypeName}() -&gt; factory methods for each constructor
/// </summary>
[Generator]
public sealed class ServiceTypeCollectionGenerator : IIncrementalGenerator
{
    /// <summary>
    /// Initializes the incremental generator for ServiceType collections with optimized attribute-based discovery.
    /// </summary>
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Use optimized attribute-based discovery instead of global scanning
        var serviceTypeCollectionsProvider = context.CompilationProvider
            .Select(static (compilation, token) => 
            {
                token.ThrowIfCancellationRequested();
                return DiscoverServiceTypeCollectionsOptimized(compilation);
            });

        context.RegisterSourceOutput(serviceTypeCollectionsProvider, static (context, results) =>
        {
            foreach (var result in results)
            {
                Execute(context, result.ServiceTypeInfoModel, result.Compilation, result.DiscoveredServiceTypes, result.CollectionClass);
            }
        });
    }

    /// <summary>
    /// Optimized discovery that searches for ServiceTypeCollectionAttribute-annotated classes first,
    /// then performs targeted service type discovery instead of expensive global scanning.
    /// </summary>
    private static ImmutableArray<ServiceTypeInfoWithCompilation> DiscoverServiceTypeCollectionsOptimized(Compilation compilation)
    {
        var results = new List<ServiceTypeInfoWithCompilation>();

        // STEP 1: Find classes with ServiceTypeCollectionAttribute - O(n) where n = classes in current compilation only
        var attributedCollections = FindAttributedCollectionClasses(compilation);

        // STEP 2: For each attributed collection, perform targeted service type discovery
        foreach (var (collectionClass, baseTypeName, collectionName) in attributedCollections)
        {
            var baseType = GetBaseTypeFromName(baseTypeName, compilation);
            if (baseType == null) continue;

            // STEP 3: Targeted service type discovery with limited scope
            var serviceTypes = new HashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default);
            DiscoverServiceTypesTargeted(baseType, compilation, serviceTypes);

            // STEP 4: Build definition and add to results
            var definition = BuildServiceTypeCollectionDefinition(
                collectionClass, baseType, serviceTypes.ToList(), compilation);
            
            if (definition != null)
            {
                results.Add(new ServiceTypeInfoWithCompilation(
                    definition, compilation, serviceTypes.ToList(), collectionClass));
            }
        }

        return [..results];
    }

    /// <summary>
    /// Finds classes decorated with ServiceTypeCollectionAttribute - much faster than inheritance scanning.
    /// </summary>
    private static List<(INamedTypeSymbol CollectionClass, string BaseTypeName, string CollectionName)> FindAttributedCollectionClasses(Compilation compilation)
    {
        var results = new List<(INamedTypeSymbol, string, string)>();
        var attributeType = compilation.GetTypeByMetadataName("FractalDataWorks.ServiceTypes.Attributes.ServiceTypeCollectionAttribute");
        
        if (attributeType == null) return results;

        // Only scan current compilation namespace - not all references
        ScanNamespaceForAttributedClasses(compilation.GlobalNamespace, attributeType, results);
        
        return results;
    }

    /// <summary>
    /// Recursively scans namespace hierarchy for classes with ServiceTypeCollectionAttribute.
    /// </summary>
    private static void ScanNamespaceForAttributedClasses(
        INamespaceSymbol namespaceSymbol, 
        INamedTypeSymbol attributeType,
        List<(INamedTypeSymbol CollectionClass, string BaseTypeName, string CollectionName)> results)
    {
        // Check types in current namespace
        foreach (var type in namespaceSymbol.GetTypeMembers())
        {
            var attributeData = type.GetAttributes()
                .FirstOrDefault(a => SymbolEqualityComparer.Default.Equals(a.AttributeClass, attributeType));
            
            if (attributeData?.ConstructorArguments.Length > 0)
            {
                var baseTypeName = attributeData.ConstructorArguments[0].Value?.ToString();
                if (!string.IsNullOrEmpty(baseTypeName))
                {
                    var collectionName = attributeData.ConstructorArguments.Length > 1
                        ? attributeData.ConstructorArguments[1].Value?.ToString()
                        : DeriveName(type.Name);
                    
                    results.Add((type, baseTypeName!, collectionName ?? DeriveName(type.Name)));
                }
            }
        }

        // Recursively scan child namespaces
        foreach (var childNamespace in namespaceSymbol.GetNamespaceMembers())
        {
            ScanNamespaceForAttributedClasses(childNamespace, attributeType, results);
        }
    }

    /// <summary>
    /// Derives collection name by removing "Base" suffix.
    /// </summary>
    private static string DeriveName(string className)
    {
        return className.EndsWith("Base", StringComparison.Ordinal)
            ? className.Substring(0, className.Length - 4)
            : className;
    }

    /// <summary>
    /// Gets a base type by its simple name, searching efficiently through known namespaces.
    /// </summary>
    private static INamedTypeSymbol? GetBaseTypeFromName(string baseTypeName, Compilation compilation)
    {
        // Try common ServiceType namespaces first (most likely locations)
        var commonNamespaces = new[]
        {
            "FractalDataWorks.Services.Connections.Abstractions",
            "FractalDataWorks.Services.Authentication.Abstractions", 
            "FractalDataWorks.Services.DataGateway.Abstractions",
            "FractalDataWorks.Services.Scheduling.Abstractions",
            "FractalDataWorks.Services.SecretManagement.Abstractions",
            "FractalDataWorks.Services.Transformations.Abstractions",
            "FractalDataWorks.DataContainers.Abstractions"
        };

        foreach (var ns in commonNamespaces)
        {
            var fullName = $"{ns}.{baseTypeName}";
            var type = compilation.GetTypeByMetadataName(fullName);
            if (type != null) return type;
        }

        // Fallback: search more broadly but still avoid global scanning
        return SearchTypeInAssemblies(baseTypeName, compilation);
    }

    /// <summary>
    /// Searches for a type in referenced assemblies without doing expensive global traversal.
    /// </summary>
    private static INamedTypeSymbol? SearchTypeInAssemblies(string typeName, Compilation compilation)
    {
        // Check current compilation first
        var types = GetTypesFromNamespace(compilation.GlobalNamespace, typeName);
        if (types.Any()) return types.First();

        // Check only essential referenced assemblies (avoid scanning everything)
        foreach (var reference in compilation.References.Take(10)) // Limit to avoid performance issues
        {
            if (compilation.GetAssemblyOrModuleSymbol(reference) is IAssemblySymbol assemblySymbol)
            {
                types = GetTypesFromNamespace(assemblySymbol.GlobalNamespace, typeName);
                if (types.Any()) return types.First();
            }
        }

        return null;
    }

    /// <summary>
    /// Gets types with a specific name from a namespace hierarchy efficiently.
    /// </summary>
    private static IEnumerable<INamedTypeSymbol> GetTypesFromNamespace(INamespaceSymbol namespaceSymbol, string typeName)
    {
        // Check types in current namespace
        foreach (var type in namespaceSymbol.GetTypeMembers(typeName))
        {
            yield return type;
        }

        // Recursively check child namespaces (but limit depth to avoid performance issues)
        foreach (var childNamespace in namespaceSymbol.GetNamespaceMembers())
        {
            foreach (var type in GetTypesFromNamespace(childNamespace, typeName))
            {
                yield return type;
            }
        }
    }

    /// <summary>
    /// Discovers service types that inherit from a base type using targeted searches instead of global scanning.
    /// </summary>
    private static void DiscoverServiceTypesTargeted(INamedTypeSymbol baseType, Compilation compilation, HashSet<INamedTypeSymbol> results)
    {
        // Search in current compilation
        SearchInheritorsInNamespace(compilation.GlobalNamespace, baseType, results);

        // Search in referenced assemblies but limit scope to avoid performance issues
        foreach (var reference in compilation.References.Take(5)) // Limit assembly scanning
        {
            if (compilation.GetAssemblyOrModuleSymbol(reference) is IAssemblySymbol assemblySymbol)
            {
                SearchInheritorsInNamespace(assemblySymbol.GlobalNamespace, baseType, results);
            }
        }
    }

    /// <summary>
    /// Searches for types inheriting from a base type within a namespace hierarchy.
    /// </summary>
    private static void SearchInheritorsInNamespace(INamespaceSymbol namespaceSymbol, INamedTypeSymbol baseType, HashSet<INamedTypeSymbol> results)
    {
        // Check types in current namespace
        foreach (var type in namespaceSymbol.GetTypeMembers())
        {
            if (InheritsFromType(type, baseType))
            {
                results.Add(type);
            }
        }

        // Recursively check child namespaces
        foreach (var childNamespace in namespaceSymbol.GetNamespaceMembers())
        {
            SearchInheritorsInNamespace(childNamespace, baseType, results);
        }
    }

    /// <summary>
    /// Checks if a type inherits from a specified base type.
    /// </summary>
    private static bool InheritsFromType(INamedTypeSymbol type, INamedTypeSymbol baseType)
    {
        var current = type.BaseType;
        while (current != null)
        {
            if (SymbolEqualityComparer.Default.Equals(current.OriginalDefinition, baseType.OriginalDefinition))
                return true;
            current = current.BaseType;
        }
        return false;
    }


    /// <summary>
    /// STEP 5: Definition Construction
    /// Builds collection definition for ServiceType collections.
    /// </summary>
    private static EnumTypeInfoModel? BuildServiceTypeCollectionDefinition(
        INamedTypeSymbol collectionClass, 
        INamedTypeSymbol baseType, 
        List<INamedTypeSymbol> serviceTypes, 
        Compilation compilation)
    {
        var collectionName = collectionClass.Name;
        var baseTypeName = baseType.Name;
        
        // Detect return type based on inheritance (like TypeCollectionGenerator)
        var returnType = DetectReturnType(collectionClass.BaseType, compilation);

        // Use collection class namespace
        var containingNamespace = collectionClass.ContainingNamespace?.ToDisplayString() ?? string.Empty;
            
        return new EnumTypeInfoModel
        {
            Namespace = containingNamespace,
            ClassName = baseTypeName,
            FullTypeName = baseType.ToDisplayString(),
            CollectionName = collectionName,
            CollectionBaseType = baseType.ToDisplayString(),
            ReturnType = returnType,
            InheritsFromCollectionBase = true,
            UseSingletonInstances = true,
            GenerateFactoryMethods = true,
            LookupProperties = ExtractServiceTypeLookupProperties(baseType, compilation)
        };
    }

    /// <summary>
    /// STEP 5.2: Return type detection
    /// Determines return type based on generic arity (TBase vs TGeneric).
    /// </summary>
    private static string DetectReturnType(ITypeSymbol? baseType, Compilation compilation)
    {
        if (baseType is INamedTypeSymbol { IsGenericType: true } namedBase)
        {
            var constructedFrom = namedBase.ConstructedFrom.ToDisplayString();
            
            if (constructedFrom.Contains("ServiceTypeCollectionBase"))
            {
                // ServiceTypeCollectionBase<TBase,TGeneric,TService,TConfiguration,TFactory>
                if (namedBase.TypeArguments.Length >= 2)
                {
                    // Return TGeneric (second type parameter)
                    return namedBase.TypeArguments[1].ToDisplayString();
                }
                // Single generic: ServiceTypeCollectionBase<TBase> -> return TBase
                else if (namedBase.TypeArguments.Length == 1)
                {
                    return namedBase.TypeArguments[0].ToDisplayString();
                }
            }
        }

        return "object"; // Fallback
    }

    /// <summary>
    /// Extract lookup properties from ServiceTypeBase for automatic method generation.
    /// </summary>
    private static EquatableArray<PropertyLookupInfoModel> ExtractServiceTypeLookupProperties(INamedTypeSymbol baseType, Compilation compilation)
    {
        var lookupProperties = new List<PropertyLookupInfoModel>();
        
        var typeLookupAttributeType = compilation.GetTypeByMetadataName("FractalDataWorks.ServiceTypes.Attributes.TypeLookupAttribute");
        
        var currentType = baseType;
        while (currentType != null)
        {
            foreach (var property in currentType.GetMembers().OfType<IPropertySymbol>())
            {
                var typeLookupAttr = property.GetAttributes()
                    .FirstOrDefault(ad => typeLookupAttributeType != null && SymbolEqualityComparer.Default.Equals(ad.AttributeClass, typeLookupAttributeType));

                if (typeLookupAttr != null && typeLookupAttr.ConstructorArguments.Length > 0)
                {
                    var methodName = typeLookupAttr.ConstructorArguments[0].Value?.ToString();
                    if (!string.IsNullOrEmpty(methodName))
                    {
                        lookupProperties.Add(new PropertyLookupInfoModel
                        {
                            PropertyName = property.Name,
                            PropertyType = property.Type.ToDisplayString(),
                            LookupMethodName = methodName!,
                            AllowMultiple = false,
                            ReturnType = baseType.ToDisplayString()
                        });
                    }
                }
            }
            
            currentType = currentType.BaseType;
        }
        
        return new EquatableArray<PropertyLookupInfoModel>(lookupProperties);
    }


    /// <summary>
    /// PHASE 7: Code Generation and Output
    /// Executes the final code generation for discovered ServiceType collections.
    /// </summary>
    private static void Execute(
        SourceProductionContext context,
        EnumTypeInfoModel def,
        Compilation compilation,
        List<INamedTypeSymbol> discoveredServiceTypes,
        INamedTypeSymbol collectionClass)
    {
        if (def == null) throw new ArgumentNullException(nameof(def));
        if (compilation == null) throw new ArgumentNullException(nameof(compilation));

        // STEP 7.1: Base Type Resolution
        var baseTypeSymbol = compilation.GetTypeByMetadataName(def.CollectionBaseType ?? def.FullTypeName);
        if (baseTypeSymbol == null) return;

        // STEP 7.2: Return Type Detection
        if (string.IsNullOrEmpty(def.ReturnType))
        {
            def.ReturnType = "FractalDataWorks.ServiceTypes.ServiceTypeBase";
        }

        // STEP 7.3: Service Type Model Conversion
        var values = new List<EnumValueInfoModel>();
        foreach (var serviceType in discoveredServiceTypes)
        {
            var name = serviceType.Name;

            var serviceTypeValueInfo = new EnumValueInfoModel
            {
                ShortTypeName = serviceType.Name,
                FullTypeName = serviceType.ToDisplayString(),
                Name = name,
                ReturnTypeNamespace = serviceType.ContainingNamespace?.ToDisplayString() ?? string.Empty,
                Constructors = ExtractConstructorInfo(serviceType)
            };
            values.Add(serviceTypeValueInfo);
        }

        // STEP 7.4: Final Code Generation
        GenerateServiceTypeCollection(context, def, new EquatableArray<EnumValueInfoModel>(values), compilation, collectionClass);
    }

    /// <summary>
    /// Constructor information extraction for Create method overloads.
    /// </summary>
    private static List<ConstructorInfo> ExtractConstructorInfo(INamedTypeSymbol serviceType)
    {
        var constructors = new List<ConstructorInfo>();

        foreach (var constructor in serviceType.Constructors.Where(c => c.DeclaredAccessibility == Accessibility.Public))
        {
            var parameters = new List<ParameterInfo>();

            foreach (var param in constructor.Parameters)
            {
                parameters.Add(new ParameterInfo
                {
                    Name = param.Name,
                    TypeName = param.Type.ToDisplayString(),
                    DefaultValue = param.HasExplicitDefaultValue ?
                        GetDefaultValueString(param.ExplicitDefaultValue, param.Type) : null
                });
            }

            constructors.Add(new ConstructorInfo { Parameters = parameters });
        }

        return constructors;
    }

    /// <summary>
    /// Helper method to convert default values to string representation.
    /// </summary>
    private static string GetDefaultValueString(object? defaultValue, ITypeSymbol parameterType)
    {
        if (defaultValue == null)
        {
            return parameterType.IsReferenceType || parameterType.NullableAnnotation == NullableAnnotation.Annotated
                ? "null" : "default";
        }

        return defaultValue switch
        {
            string str => $"\"{str}\"",
            char ch => $"'{ch}'",
            bool b => b ? "true" : "false",
            float f => $"{f}f",
            double d => $"{d}d",
            decimal dec => $"{dec}m",
            long l => $"{l}L",
            uint ui => $"{ui}u",
            ulong ul => $"{ul}ul",
            _ => defaultValue.ToString() ?? "default"
        };
    }

    /// <summary>
    /// Determines the effective return type for the collection based on inheritance.
    /// For ServiceTypeCollectionBase&lt;TBase,TGeneric,...&gt; returns TGeneric, for ServiceTypeCollectionBase&lt;TBase&gt; returns TBase.
    /// </summary>
    private static string? DetermineReturnType(INamedTypeSymbol collectionClass)
    {
        var currentType = collectionClass.BaseType;
        
        while (currentType != null)
        {
            if (currentType.IsGenericType)
            {
                var genericDefinition = currentType.ConstructedFrom;
                
                // Check for ServiceTypeCollectionBase<TBase,TGeneric,...> (multiple generics)
                if (string.Equals(genericDefinition.Name, "ServiceTypeCollectionBase", StringComparison.Ordinal) && currentType.TypeArguments.Length >= 2)
                {
                    // Return TGeneric (second type parameter)
                    return currentType.TypeArguments[1].Name;
                }
                
                // Check for ServiceTypeCollectionBase<TBase> (single generic)
                if (string.Equals(genericDefinition.Name, "ServiceTypeCollectionBase", StringComparison.Ordinal) && currentType.TypeArguments.Length == 1)
                {
                    // Return TBase (first type parameter)
                    return currentType.TypeArguments[0].Name;
                }
            }
            
            currentType = currentType.BaseType;
        }
        
        return null;
    }

    /// <summary>
    /// Generates the ServiceType collection class using the existing builder infrastructure.
    /// </summary>
    private static void GenerateServiceTypeCollection(
        SourceProductionContext context,
        EnumTypeInfoModel def,
        EquatableArray<EnumValueInfoModel> values,
        Compilation compilation,
        INamedTypeSymbol collectionClass)
    {
        try
        {
            // Determine the effective return type based on collection inheritance (like TypeCollectionGenerator)
            var effectiveReturnType = DetermineReturnType(collectionClass);
            if (effectiveReturnType == null)
            {
                // Fallback to base type name
                effectiveReturnType = def.ClassName;
            }

            // Use the enhanced EnumCollectionBuilder with FrozenDictionary support
            var builder = new EnumCollectionBuilder();

            // Generate the collection with all enhanced features
            var generatedCode = builder
                .WithDefinition(def)
                .WithValues(values.ToList())
                .WithReturnType(effectiveReturnType)
                .WithCompilation(compilation)
                .Build();

            var fileName = $"{def.CollectionName}.g.cs";
            context.AddSource(fileName, generatedCode);
        }
        catch (Exception ex)
        {
            // Generate diagnostic for any errors during generation
            var diagnostic = Diagnostic.Create(
                new DiagnosticDescriptor(
                    "STCG001",
                    "ServiceType Collection Generation Failed",
                    "Failed to generate ServiceType collection {0}: {1}",
                    "ServiceTypeCollectionGenerator",
                    DiagnosticSeverity.Error,
                    isEnabledByDefault: true),
                Location.None,
                def.CollectionName,
                ex.Message);

            context.ReportDiagnostic(diagnostic);
        }
    }
}