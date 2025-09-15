using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using FractalDataWorks.ServiceTypes.SourceGenerators.Models;
using FractalDataWorks.SourceGenerators.Models;
using Microsoft.CodeAnalysis;

namespace FractalDataWorks.ServiceTypes.SourceGenerators.Services;

/// <summary>
/// Parses attributes related to Enhanced Enums and extracts configuration.
/// </summary>
/// <remarks>
/// This code is excluded from code coverage because source generators run at compile-time and cannot be unit tested via runtime tests.
/// </remarks>

public static class EnumAttributeParser
{
    /// <summary>
    /// Parses EnumCollection attributes from a type symbol and returns EnumTypeInfoModel for each collection.
    /// </summary>
    public static IList<EnumTypeInfoModel> ParseEnhancedEnumBase(INamedTypeSymbol symbol)
    {
        var attrs = symbol.GetAttributes()
            .Where(ad => string.Equals(ad.AttributeClass?.Name, "EnumCollectionAttribute", StringComparison.Ordinal) ||
                        string.Equals(ad.AttributeClass?.Name, "EnumCollection", StringComparison.Ordinal))
            .ToList();

        if (attrs.Count == 0)
        {
            return new List<EnumTypeInfoModel>();
        }
        
        // Verify the type inherits from EnumOptionBase<T>
        if (!InheritsFromEnhancedEnumBase(symbol))
        {
            // Type must inherit from EnumOptionBase<T> to use EnumCollection attribute
            return new List<EnumTypeInfoModel>();
        }

        // First, collect lookup properties (same for all collections)
        var lookupProperties = ExtractLookupProperties(symbol);

        // Process each EnumCollection attribute to create separate collections
        List<EnumTypeInfoModel> results = [];

        foreach (var attr in attrs)
        {
            var collectionInfo = CreateEnumTypeInfo(symbol, attr, lookupProperties);
            results.Add(collectionInfo);
        }

        return results;
    }

    /// <summary>
    /// Extracts the collection name from an attribute.
    /// </summary>
    public static string ExtractCollectionName(AttributeData attr, INamedTypeSymbol symbol)
    {
        // Check named arguments first for CollectionName property
        var named = attr.NamedArguments.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        if (named.TryGetValue("CollectionName", out var cn) && cn.Value is string cns && !string.IsNullOrEmpty(cns))
        {
            return cns;
        }

        // Check constructor argument
        if (attr.ConstructorArguments.Length > 0 && attr.ConstructorArguments[0].Value is string collName && !string.IsNullOrEmpty(collName))
        {
            return collName;
        }

        // Return empty string to indicate missing collection name - analyzer should warn about this
        return string.Empty;
    }

    /// <summary>
    /// Parses an EnumOption attribute and returns the configuration.
    /// </summary>
    public static string ParseEnumOption(AttributeData attr, INamedTypeSymbol typeSymbol)
    {
        // New simplified format: [EnumOption("Name")] or [EnumOption()]
        // Check constructor arguments first
        if (attr.ConstructorArguments.Length > 0 && attr.ConstructorArguments[0].Value is string constructorName && !string.IsNullOrEmpty(constructorName))
        {
            return constructorName;
        }
        
        // Fallback: use class name if no Name provided (parameterless constructor)
        return typeSymbol.Name;
    }

    private static List<PropertyLookupInfoModel> ExtractLookupProperties(INamedTypeSymbol symbol)
    {
        List<PropertyLookupInfoModel> lookupProperties = [];
        
        foreach (var prop in symbol.GetMembers().OfType<IPropertySymbol>())
        {
            var lookupAttr = prop.GetAttributes()
                .FirstOrDefault(ad => string.Equals(ad.AttributeClass?.Name, "EnumLookupAttribute", StringComparison.Ordinal) ||
                                    string.Equals(ad.AttributeClass?.Name, "EnumLookup", StringComparison.Ordinal));
            
            if (lookupAttr == null)
            {
                continue;
            }

            var lnamed = lookupAttr.NamedArguments.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            var methodName = lnamed.TryGetValue("MethodName", out var mn) && mn.Value is string ms
                ? ms : $"GetBy{prop.Name}";
            var allowMultiple = lnamed.TryGetValue("AllowMultiple", out var am) && am.Value is bool mu && mu;
            var returnType = lnamed.TryGetValue("ReturnType", out var rt) && rt.Value is string rs ? rs : null;

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

        return lookupProperties;
    }

    private static EnumTypeInfoModel CreateEnumTypeInfo(INamedTypeSymbol symbol, AttributeData attr, IList<PropertyLookupInfoModel> lookupProperties)
    {
        var collectionName = ExtractCollectionName(attr, symbol);
        var named = attr.NamedArguments.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

        // Extract GenerationMode and StorageMode from constructor arguments
        var generationMode = 0; // Methods by default
        var storageMode = 0;    // Dictionary by default
        
        if (attr.ConstructorArguments.Length > 1 && attr.ConstructorArguments[1].Value is int genMode)
        {
            generationMode = genMode;
        }
        
        if (attr.ConstructorArguments.Length > 2 && attr.ConstructorArguments[2].Value is int storeMode)
        {
            storageMode = storeMode;
        }

        var collectionInfo = new EnumTypeInfoModel
        {
            Namespace = named.TryGetValue("Namespace", out var ns) && ns.Value is string nsStr && !string.IsNullOrEmpty(nsStr)
                ? nsStr : symbol.ContainingNamespace.ToDisplayString(),
            ClassName = symbol.Name,
            FullTypeName = symbol.ToDisplayString(),
            IsGenericType = symbol.IsGenericType,
            CollectionName = collectionName,
            GenerateFactoryMethods = generationMode == 0, // Methods = true, Singletons = false
            NameComparison = StringComparison.OrdinalIgnoreCase, // Fixed for consistency
            UseSingletonInstances = generationMode == 1, // Singletons = true, Methods = false
            ReturnType = named.TryGetValue("ReturnType", out var rt) && rt.Value is string rs ? rs : null,
            ReturnTypeNamespace = named.TryGetValue("ReturnTypeNamespace", out var rtn) && rtn.Value is string rtns ? rtns : null,
            LookupProperties = new EquatableArray<PropertyLookupInfoModel>(lookupProperties),
        };

        // Extract generic type information
        if (symbol.IsGenericType)
        {
            ExtractGenericTypeInfo(symbol, collectionInfo);
        }

        // Get default generic return type from attribute
        collectionInfo.DefaultGenericReturnType = named.TryGetValue("DefaultGenericReturnType", out var dgrt) && dgrt.Value is string dgrts ? dgrts : null;
        collectionInfo.DefaultGenericReturnTypeNamespace = named.TryGetValue("DefaultGenericReturnTypeNamespace", out var dgrtn) && dgrtn.Value is string dgrtns ? dgrtns : null;

        return collectionInfo;
    }

    private static void ExtractGenericTypeInfo(INamedTypeSymbol symbol, EnumTypeInfoModel infoModel)
    {
        if (!symbol.IsGenericType)
            return;

        var unboundType = symbol.ConstructUnboundGenericType();
        infoModel.UnboundTypeName = unboundType.ToDisplayString();

        infoModel.TypeParameters = symbol.TypeParameters.Select(tp => tp.Name).ToList();
        
        infoModel.TypeConstraints = symbol.TypeParameters
            .Where(tp => tp.ConstraintTypes.Length > 0)
            .Select(tp => $"where {tp.Name} : {string.Join(", ", tp.ConstraintTypes.Select(ct => ct.ToDisplayString()))}")
            .ToList();
    }
    
    /// <summary>
    /// Checks if a type inherits from EnumOptionBase&lt;T&gt;.
    /// </summary>
    private static bool InheritsFromEnhancedEnumBase(INamedTypeSymbol symbol)
    {
        var currentType = symbol.BaseType;
        while (currentType != null)
        {
            var typeName = currentType.Name;
            var fullName = currentType.ToDisplayString();
            
            // Check if it's EnumOptionBase or EnumOptionBase<T>
            if (string.Equals(typeName, "EnumOptionBase", StringComparison.Ordinal) ||
                fullName.Contains("EnumOptionBase<"))
            {
                return true;
            }
            
            currentType = currentType.BaseType;
        }
        
        return false;
    }
}
