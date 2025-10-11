using System;
using System.Collections.Generic;
using System.Linq;
using FractalDataWorks.CodeBuilder.Abstractions;
using FractalDataWorks.CodeBuilder.CSharp.Builders;
using FractalDataWorks.SourceGenerators.Configuration;
using FractalDataWorks.SourceGenerators.Models;

namespace FractalDataWorks.SourceGenerators.Generators;

/// <summary>
/// Generates field declarations for collection classes.
/// Responsible for creating _all, _empty, and lookup dictionary fields.
/// </summary>
public sealed class FieldGenerator
{
    private readonly CollectionBuilderConfiguration _config;

    /// <summary>
    /// Initializes a new instance of the <see cref="FieldGenerator"/> class.
    /// </summary>
    /// <param name="config">The configuration for collection generation.</param>
    public FieldGenerator(CollectionBuilderConfiguration config)
    {
        _config = config ?? throw new ArgumentNullException(nameof(config));
    }

    /// <summary>
    /// Generates the _all static field (FrozenDictionary or Dictionary based on target).
    /// </summary>
    public IFieldBuilder GenerateAllField(string returnType)
    {
        return new FieldBuilder()
            .WithName("_all")
            .WithType($"FrozenDictionary<int, {returnType}>")
            .WithAccessModifier("private")
            .AsStatic()
            .AsReadOnly()
            .WithXmlDoc("Primary lookup dictionary for all collection items, keyed by ID.");
    }

    /// <summary>
    /// Generates the _empty static field (initialized in static constructor).
    /// </summary>
    public IFieldBuilder GenerateEmptyField(string returnType)
    {
        return new FieldBuilder()
            .WithName("_empty")
            .WithType(returnType)
            .WithAccessModifier("private")
            .AsStatic()
            .AsReadOnly()
            .WithXmlDoc("Static empty instance with default values.");
    }

    /// <summary>
    /// Generates lookup dictionary fields for non-ID properties (e.g., _byName).
    /// Only generates fields for pre-NET8.0 targets that don't support AlternateLookup.
    /// </summary>
    public List<IFieldBuilder> GenerateLookupDictionaryFields(
        GenericTypeInfoModel definition,
        string returnType)
    {
        var fields = new List<IFieldBuilder>();

        if (definition?.LookupProperties == null)
            return fields;

        var nonIdLookups = definition.LookupProperties
            .Where(l => !string.Equals(l.PropertyName, "Id", StringComparison.Ordinal) ||
                       !string.Equals(l.PropertyType, "int", StringComparison.Ordinal))
            .ToList();

        if (nonIdLookups.Count == 0)
            return fields;

        // Check if target framework supports AlternateLookup (NET8.0+)
        bool supportsAlternateLookup = IsNet8OrGreater(definition.TargetFramework);

        // Don't generate lookup dictionaries if the target supports AlternateLookup
        if (supportsAlternateLookup)
            return fields;

        // Generate lookup dictionary fields for pre-NET8.0 targets
        foreach (var lookup in nonIdLookups)
        {
            var field = new FieldBuilder()
                .WithName($"_by{lookup.PropertyName}")
                .WithType($"FrozenDictionary<{lookup.PropertyType}, {returnType}>")
                .WithAccessModifier("private")
                .AsStatic()
                .AsReadOnly()
                .WithXmlDoc($"Lookup dictionary for {lookup.PropertyName}-based searches.");

            fields.Add(field);
        }

        return fields;
    }

    /// <summary>
    /// Determines if the target framework is NET8.0 or greater.
    /// </summary>
    private static bool IsNet8OrGreater(string? targetFramework)
    {
        if (string.IsNullOrEmpty(targetFramework))
            return false;

        // Check for net8.0, net9.0, net10.0, etc.
        if (targetFramework.StartsWith("net", StringComparison.OrdinalIgnoreCase))
        {
            // Extract version number (e.g., "net8.0" -> "8", "net10.0" -> "10")
            var versionPart = targetFramework.Substring(3);
            var dotIndex = versionPart.IndexOf('.');
            if (dotIndex > 0)
            {
                versionPart = versionPart.Substring(0, dotIndex);
            }

            if (int.TryParse(versionPart, out var version))
            {
                return version >= 8;
            }
        }

        return false;
    }

    /// <summary>
    /// Generates static fields for enum values (field-per-value pattern).
    /// </summary>
    public List<IFieldBuilder> GenerateValueFields(
        IList<GenericValueInfoModel> values,
        string returnType)
    {
        var fields = new List<IFieldBuilder>();

        foreach (var value in values)
        {
            // Skip abstract types - they can't be instantiated
            if (value.IsAbstract)
                continue;

            var field = new FieldBuilder()
                .WithName(value.Name)
                .WithType(returnType)
                .WithAccessModifier("public")
                .AsStatic()
                .AsReadOnly()
                .WithXmlDoc($"Gets the {value.Name} instance.");

            fields.Add(field);
        }

        return fields;
    }
}
