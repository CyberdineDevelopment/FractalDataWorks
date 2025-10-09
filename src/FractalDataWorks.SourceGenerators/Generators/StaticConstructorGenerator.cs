using System;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using FractalDataWorks.CodeBuilder.Abstractions;
using FractalDataWorks.CodeBuilder.CSharp.Builders;
using FractalDataWorks.SourceGenerators.Configuration;
using FractalDataWorks.SourceGenerators.Models;
using FractalDataWorks.SourceGenerators.Services;

namespace FractalDataWorks.SourceGenerators.Generators;

/// <summary>
/// Generates static constructors for collection classes.
/// </summary>
public sealed class StaticConstructorGenerator
{
    private readonly CollectionBuilderConfiguration _config;

    /// <summary>
    /// Initializes a new instance of the <see cref="StaticConstructorGenerator"/> class.
    /// </summary>
    /// <param name="config">The configuration for collection generation.</param>
    public StaticConstructorGenerator(CollectionBuilderConfiguration config)
    {
        _config = config ?? throw new ArgumentNullException(nameof(config));
    }

    /// <summary>
    /// Generates the static constructor that initializes _all and _empty fields.
    /// </summary>
    public IConstructorBuilder GenerateStaticConstructor(
        GenericTypeInfoModel definition,
        System.Collections.Generic.IList<GenericValueInfoModel> values,
        string returnType,
        Compilation compilation)
    {
        var constructorBody = new StringBuilder();

        var includedValues = values.Where(v => v.Include && !v.IsAbstract && !v.IsStatic).ToList();

        if (includedValues.Count > 0)
        {
            // Build dictionary of instances keyed by their Id property
            constructorBody.AppendLine("var dictionary = new System.Collections.Generic.Dictionary<int, " + returnType + ">();");
            constructorBody.AppendLine();

            // Create instances and add to dictionary
            foreach (var value in includedValues)
            {
                if (value.BaseConstructorId.HasValue)
                {
                    // Use literal ID from base constructor argument
                    constructorBody.AppendLine($"        dictionary.Add({value.BaseConstructorId.Value}, new {value.ShortTypeName}());");
                }
                else
                {
                    // Fallback: instantiate and read Id property
                    var varName = value.Name.ToLower(System.Globalization.CultureInfo.InvariantCulture);
                    constructorBody.AppendLine($"        var {varName} = new {value.ShortTypeName}();");
                    constructorBody.AppendLine($"        dictionary.Add({varName}.Id, {varName});");
                }
                constructorBody.AppendLine();
            }

            constructorBody.AppendLine("_all = dictionary.ToFrozenDictionary();");
        }
        else
        {
            // No values - create empty FrozenDictionary
            constructorBody.AppendLine("_all = System.Collections.Frozen.FrozenDictionary<int, " + returnType + ">.Empty;");
        }

        constructorBody.AppendLine();

        // Initialize _empty field
        var baseTypeName = definition.ClassName;

        // Try to resolve the base type symbol using metadata format for generic types
        INamedTypeSymbol? baseTypeSymbol = null;
        if (!string.IsNullOrEmpty(definition.FullTypeName))
        {
            var fullTypeName = definition.FullTypeName;
            var genericIndex = fullTypeName.IndexOf('<');

            if (genericIndex > 0)
            {
                // Generic type - extract metadata name
                var baseName = fullTypeName.Substring(0, genericIndex);
                var typeParamSection = fullTypeName.Substring(genericIndex);
                var arity = typeParamSection.Count(c => c == ',') + 1;
                var metadataName = $"{baseName}`{arity}";
                baseTypeSymbol = compilation.GetTypeByMetadataName(metadataName);
            }
            else
            {
                // Non-generic type
                baseTypeSymbol = compilation.GetTypeByMetadataName(fullTypeName);
            }
        }

        // Fallback to simple name resolution
        if (baseTypeSymbol == null)
        {
            var fullyQualifiedTypeName = baseTypeName.Contains(".")
                ? baseTypeName
                : $"{definition.Namespace}.{baseTypeName}";
            baseTypeSymbol = compilation.GetTypeByMetadataName(fullyQualifiedTypeName);
        }

        if (baseTypeSymbol != null && baseTypeSymbol.TypeKind == TypeKind.Class && !GenericTypeHelper.IsGenericType(baseTypeSymbol))
        {
            // Non-generic base class exists - use EmptyClassName instance
            var emptyClassName = $"Empty{baseTypeName}";
            constructorBody.AppendLine($"_empty = new {emptyClassName}();");
        }
        else
        {
            // Interface only OR generic base type - use null!
            // For generic types, we cannot instantiate an open generic, so _empty is null
            constructorBody.AppendLine("_empty = null!;");
        }

        // Initialize lookup dictionaries for netstandard2.0 (NET8+ uses GetAlternateLookup instead)
        if (definition.LookupProperties != null && definition.LookupProperties.Count() > 0)
        {
            var nonIdLookups = definition.LookupProperties
                .Where(l => !string.Equals(l.PropertyName, "Id", StringComparison.Ordinal))
                .ToList();

            if (nonIdLookups.Count > 0)
            {
                constructorBody.AppendLine();
                constructorBody.AppendLine("#if !NET8_0_OR_GREATER");

                foreach (var lookup in nonIdLookups)
                {
                    var dictionaryName = $"_by{lookup.PropertyName}";
                    constructorBody.AppendLine($"{dictionaryName} = _all.Values.ToFrozenDictionary(x => x.{lookup.PropertyName});");
                }

                constructorBody.AppendLine("#endif");
            }
        }

        var constructor = new ConstructorBuilder()
            .WithClassName(definition.CollectionName)
            .AsStatic()
            .WithBody(constructorBody.ToString());

        return constructor;
    }
}
