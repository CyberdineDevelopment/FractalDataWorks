using System;
using System.Globalization;
using System.Linq;
using FractalDataWorks.CodeBuilder.Abstractions;
using FractalDataWorks.CodeBuilder.CSharp.Builders;
using FractalDataWorks.SourceGenerators.Configuration;
using FractalDataWorks.SourceGenerators.Models;

namespace FractalDataWorks.SourceGenerators.Generators;

/// <summary>
/// Generates lookup methods for collection classes (Name, Id, etc.).
/// Uses conditional compilation for NET8+ vs netstandard2.0.
/// </summary>
public sealed class LookupMethodGenerator
{
    private readonly CollectionBuilderConfiguration _config;

    /// <summary>
    /// Initializes a new instance of the <see cref="LookupMethodGenerator"/> class.
    /// </summary>
    /// <param name="config">The configuration for collection generation.</param>
    public LookupMethodGenerator(CollectionBuilderConfiguration config)
    {
        _config = config ?? throw new ArgumentNullException(nameof(config));
    }

    /// <summary>
    /// Generates dynamic lookup methods based on [TypeLookup] attributes.
    /// Creates methods like Name(string name) and Id(int id).
    /// Uses GetAlternateLookup on NET8+ and separate dictionaries on older targets.
    /// </summary>
    public static IMethodBuilder[] GenerateDynamicLookupMethods(
        GenericTypeInfoModel definition,
        string returnType)
    {
        if (definition?.LookupProperties == null)
            return Array.Empty<IMethodBuilder>();

        var methods = definition.LookupProperties
            .Select(lookup => GenerateLookupMethod(lookup, returnType, definition.TargetFramework))
            .ToArray();

        return methods;
    }

    private static IMethodBuilder GenerateLookupMethod(
        PropertyLookupInfoModel lookup,
        string returnType,
        string? targetFramework)
    {
        var methodName = lookup.PropertyName; // Clean name: Id, Name, Category
        var parameterName = lookup.PropertyName.ToLower(CultureInfo.InvariantCulture); // id, name, category

        string methodBody;
        bool useExpressionBody;

        if (string.Equals(lookup.PropertyType, "int", StringComparison.Ordinal) &&
            string.Equals(lookup.PropertyName, "Id", StringComparison.Ordinal))
        {
            // For ID lookups, use the primary key directly (same on all platforms)
            methodBody = $"_all.TryGetValue({parameterName}, out var result) ? result : _empty";
            useExpressionBody = true;
        }
        else
        {
            // For alternate key lookups, check target framework
            bool supportsAlternateLookup = IsNet8OrGreater(targetFramework);
            var dictionaryName = $"_by{lookup.PropertyName}";

            if (supportsAlternateLookup)
            {
                // NET8.0+ approach using AlternateLookup
                methodBody = $@"var alternateLookup = _all.GetAlternateLookup<{lookup.PropertyType}>();
        return alternateLookup.TryGetValue({parameterName}, out var result) ? result : _empty;";
            }
            else
            {
                // Pre-NET8.0 approach using separate dictionary
                methodBody = $"return {dictionaryName}.TryGetValue({parameterName}, out var result) ? result : _empty;";
            }

            useExpressionBody = false;
        }

        var method = new MethodBuilder()
            .WithName(methodName)
            .WithReturnType(returnType)
            .WithAccessModifier("public")
            .AsStatic()
            .WithParameter(lookup.PropertyType, parameterName)
            .WithXmlDoc($"Gets a type option by its {lookup.PropertyName} using {(string.Equals(lookup.PropertyName, "Id", StringComparison.Ordinal) ? "primary key lookup" : "alternate key lookup")}.")
            .WithParamDoc(parameterName, $"The {lookup.PropertyName} value to search for.")
            .WithReturnDoc($"The type option with the specified {lookup.PropertyName}, or empty instance if not found.");

        if (useExpressionBody)
        {
            method.WithExpressionBody(methodBody);
        }
        else
        {
            method.WithBody(methodBody);
        }

        return method;
    }

    /// <summary>
    /// Determines if the target framework is NET8.0 or greater.
    /// </summary>
    private static bool IsNet8OrGreater(string? targetFramework)
    {
        if (string.IsNullOrEmpty(targetFramework))
            return false;

        // Check for net8.0, net9.0, net10.0, etc.
        if (targetFramework!.StartsWith("net", StringComparison.OrdinalIgnoreCase))
        {
            // Extract version number (e.g., "net8.0" -> "8", "net10.0" -> "10")
            var versionPart = targetFramework.Substring(3);
            var dotIndex = versionPart.IndexOf('.');
            if (dotIndex > 0)
            {
                versionPart = versionPart.Substring(0, dotIndex);
            }

            if (int.TryParse(versionPart, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out var version))
            {
                return version >= 8;
            }
        }

        return false;
    }
}
