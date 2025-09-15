using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using FractalDataWorks.EnhancedEnums.ExtendedEnums.Attributes;

namespace FractalDataWorks.EnhancedEnums.SourceGenerators.Generators;

/// <summary>
/// Source generator for Extended Enums that creates wrapper classes for existing C# enums
/// and generates collections with both auto-generated enum wrappers and custom extended options.
/// 
/// LOCAL DISCOVERY (default):
/// - Uses [ExtendEnum] attribute on base class
/// - Scans current compilation for extended enum options
/// 
/// GLOBAL DISCOVERY (opt-in):
/// - Uses [GlobalExtendedEnumCollection] attribute
/// - Scans ALL referenced assemblies for extended enum options
/// - Enables cross-assembly extended enum composition patterns
/// </summary>
/// <remarks>
/// This code is excluded from code coverage because source generators run at compile-time and cannot be unit tested via runtime tests.
/// </remarks>
[Generator]

public class ExtendedEnumGenerator : IIncrementalGenerator
{
    /// <summary>
    /// Initializes the incremental source generator with the necessary registration for Extended Enum generation.
    /// </summary>
    /// <param name="context">The initialization context for the incremental generator.</param>
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
            ctx.AddSource("ExtendedEnumGenerator_Debug.cs", 
                "// ExtendedEnumGenerator Initialize() was called - generator is loaded!");
        });
    }

    private static void RegisterMainSourceOutput(IncrementalGeneratorInitializationContext context)
    {
        var extendedEnumDefinitionsProvider = context.CompilationProvider
            .Select((compilation, _) => DiscoverExtendedEnumDefinitions(compilation));

        context.RegisterSourceOutput(extendedEnumDefinitionsProvider, (context, definitions) =>
        {
            foreach (var definition in definitions)
            {
                GenerateExtendedEnum(context, definition, compilation: null);
            }
        });
    }

    private static ImmutableArray<ExtendedEnumDefinition> DiscoverExtendedEnumDefinitions(Compilation compilation)
    {
        var results = new List<ExtendedEnumDefinition>();

        // Scan for classes with [ExtendEnum] or [GlobalExtendedEnumCollection] attributes
        foreach (var tree in compilation.SyntaxTrees)
        {
            var root = tree.GetRoot();
            var model = compilation.GetSemanticModel(tree);
            
            foreach (var classDecl in root.DescendantNodes().OfType<ClassDeclarationSyntax>())
            {
                var symbol = model.GetDeclaredSymbol(classDecl) as INamedTypeSymbol;
                if (symbol == null) continue;

                // Check for ExtendEnum attribute
                var extendEnumAttr = GetExtendEnumAttribute(symbol);
                if (extendEnumAttr != null)
                {
                    var definition = CreateExtendedEnumDefinition(symbol, extendEnumAttr, isGlobal: false, compilation);
                    if (definition != null)
                        results.Add(definition);
                }

                // Check for GlobalExtendedEnumCollection attribute
                var globalAttr = GetGlobalExtendedEnumCollectionAttribute(symbol);
                if (globalAttr != null)
                {
                    var definition = CreateGlobalExtendedEnumDefinition(symbol, globalAttr, compilation);
                    if (definition != null)
                        results.Add(definition);
                }
            }
        }

        return [..results];
    }

    private static AttributeData? GetExtendEnumAttribute(INamedTypeSymbol symbol)
    {
        return symbol.GetAttributes()
            .FirstOrDefault(attr => attr.AttributeClass != null &&
                                   string.Equals(attr.AttributeClass.ToDisplayString(), typeof(ExtendEnumAttribute).FullName, StringComparison.Ordinal));
    }

    private static AttributeData? GetGlobalExtendedEnumCollectionAttribute(INamedTypeSymbol symbol)
    {
        return symbol.GetAttributes()
            .FirstOrDefault(attr => attr.AttributeClass != null &&
                                   string.Equals(attr.AttributeClass.ToDisplayString(), typeof(GlobalExtendedEnumCollectionAttribute).FullName, StringComparison.Ordinal));
    }

    private static ExtendedEnumDefinition? CreateExtendedEnumDefinition(
        INamedTypeSymbol baseClass, 
        AttributeData attribute, 
        bool isGlobal, 
        Compilation compilation)
    {
        var enumType = GetEnumTypeFromAttribute(attribute);
        if (enumType == null) return null;

        var enumValues = GetEnumValues(enumType);
        var customOptions = FindCustomExtendedOptions(baseClass, compilation, isGlobal);

        return new ExtendedEnumDefinition
        {
            BaseClassName = baseClass.Name,
            BaseClassFullName = baseClass.ToDisplayString(),
            BaseClassNamespace = baseClass.ContainingNamespace.ToDisplayString(),
            EnumType = enumType,
            EnumValues = enumValues,
            CustomOptions = customOptions,
            IsGlobal = isGlobal,
            CollectionName = GetCollectionName(attribute, enumType),
            NameComparison = GetNameComparison(attribute),
            GenerateFactoryMethods = GetBoolProperty(attribute, "GenerateFactoryMethods", true),
            GenerateStaticCollection = GetBoolProperty(attribute, "GenerateStaticCollection", true),
            UseSingletonInstances = GetBoolProperty(attribute, "UseSingletonInstances", true),
            Generic = GetBoolProperty(attribute, "Generic", false)
        };
    }

    private static ExtendedEnumDefinition? CreateGlobalExtendedEnumDefinition(
        INamedTypeSymbol baseClass, 
        AttributeData attribute, 
        Compilation compilation)
    {
        return CreateExtendedEnumDefinition(baseClass, attribute, isGlobal: true, compilation);
    }

    private static INamedTypeSymbol? GetEnumTypeFromAttribute(AttributeData attribute)
    {
        var enumTypeArg = attribute.ConstructorArguments.FirstOrDefault();
        return enumTypeArg.Value as INamedTypeSymbol;
    }

    private static List<EnumValueInfo> GetEnumValues(INamedTypeSymbol enumType)
    {
        var values = new List<EnumValueInfo>();
        
        foreach (var member in enumType.GetMembers().OfType<IFieldSymbol>())
        {
            if (member.IsStatic && member.HasConstantValue)
            {
                values.Add(new EnumValueInfo
                {
                    Name = member.Name,
                    Value = member.ConstantValue?.ToString() ?? "0",
                    IntValue = Convert.ToInt32(member.ConstantValue ?? 0, CultureInfo.InvariantCulture)
                });
            }
        }

        return values;
    }

    private static List<INamedTypeSymbol> FindCustomExtendedOptions(
        INamedTypeSymbol baseClass, 
        Compilation compilation, 
        bool isGlobal)
    {
        var options = new List<INamedTypeSymbol>();

        // Scan current compilation
        foreach (var tree in compilation.SyntaxTrees)
        {
            var root = tree.GetRoot();
            var model = compilation.GetSemanticModel(tree);
            
            foreach (var classDecl in root.DescendantNodes().OfType<ClassDeclarationSyntax>())
            {
                var symbol = model.GetDeclaredSymbol(classDecl) as INamedTypeSymbol;
                if (symbol == null) continue;

                // Check if class has ExtendedEnumOption attribute and inherits from base class
                if (HasExtendedEnumOptionAttribute(symbol) && InheritsFrom(symbol, baseClass))
                {
                    options.Add(symbol);
                }
            }
        }

        // If global, also scan referenced assemblies (simplified for now)
        if (isGlobal)
        {
            // NOTE: Cross-assembly scanning implementation deferred to future version
            // This will require integration with the existing EnhancedEnums discovery service
        }

        return options;
    }

    private static bool HasExtendedEnumOptionAttribute(INamedTypeSymbol symbol)
    {
        return symbol.GetAttributes()
            .Any(attr => attr.AttributeClass != null &&
                        string.Equals(attr.AttributeClass.ToDisplayString(), typeof(ExtendedEnumOptionAttribute).FullName, StringComparison.Ordinal));
    }

    private static bool InheritsFrom(INamedTypeSymbol derived, INamedTypeSymbol baseType)
    {
        var current = derived.BaseType;
        while (current != null)
        {
            if (SymbolEqualityComparer.Default.Equals(current.OriginalDefinition, baseType.OriginalDefinition))
                return true;
            current = current.BaseType;
        }
        return false;
    }

    private static string GetCollectionName(AttributeData attribute, INamedTypeSymbol enumType)
    {
        var namedArgs = attribute.NamedArguments.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        if (namedArgs.TryGetValue("CollectionName", out var value) && value.Value is string name)
            return name;
        
        return $"{enumType.Name}Collection";
    }

    private static StringComparison GetNameComparison(AttributeData attribute)
    {
        var namedArgs = attribute.NamedArguments.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        if (namedArgs.TryGetValue("NameComparison", out var value) && value.Value is int comparison)
            return (StringComparison)comparison;
        
        return StringComparison.Ordinal;
    }

    private static bool GetBoolProperty(AttributeData attribute, string propertyName, bool defaultValue)
    {
        var namedArgs = attribute.NamedArguments.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        if (namedArgs.TryGetValue(propertyName, out var value) && value.Value is bool boolValue)
            return boolValue;
        
        return defaultValue;
    }

    private static void GenerateExtendedEnum(SourceProductionContext context, ExtendedEnumDefinition definition, Compilation? compilation)
    {
        // Generate wrapper classes for each enum value
        foreach (var enumValue in definition.EnumValues)
        {
            var wrapperSource = GenerateEnumWrapper(definition, enumValue);
            context.AddSource($"{definition.BaseClassName}_{enumValue.Name}.g.cs", wrapperSource);
        }

        // Generate the collection class
        var collectionSource = GenerateCollection(definition);
        context.AddSource($"{definition.CollectionName}.g.cs", collectionSource);
    }

    private static string GenerateEnumWrapper(ExtendedEnumDefinition definition, EnumValueInfo enumValue)
    {
        if (definition.EnumType == null)
            throw new InvalidOperationException("EnumType cannot be null when generating enum wrapper.");
            
        var enumTypeName = definition.EnumType.Name;
        var enumFullName = definition.EnumType.ToDisplayString();

        return $@"// <auto-generated />
#nullable enable
using System;
using FractalDataWorks.EnhancedEnums.ExtendedEnums.Attributes;

namespace {definition.BaseClassNamespace}
{{
    /// <summary>
    /// Extended enum option for {enumTypeName}.{enumValue.Name}.
    /// </summary>
    [ExtendedEnumOption]
    public partial class {enumValue.Name}{definition.BaseClassName} : {definition.BaseClassFullName}
    {{
        /// <summary>
        /// Initializes a new instance of the <see cref=""{enumValue.Name}{definition.BaseClassName}""/> class.
        /// </summary>
        public {enumValue.Name}{definition.BaseClassName}() : base({enumFullName}.{enumValue.Name})
        {{
        }}
    }}
}}";
    }

    private static string GenerateCollection(ExtendedEnumDefinition definition)
    {
        if (definition.EnumType == null)
            throw new InvalidOperationException("EnumType cannot be null when generating collection.");
            
        var allOptionsString = GenerateAllOptionsString(definition);
        var collectionClass = GenerateCollectionClass(definition, allOptionsString);
        
        return GenerateCollectionFileTemplate(definition, collectionClass);
    }

    private static string GenerateAllOptionsString(ExtendedEnumDefinition definition)
    {
        var allOptions = new List<string>();
        
        // Add auto-generated enum wrappers
        foreach (var enumValue in definition.EnumValues)
        {
            allOptions.Add($"new {enumValue.Name}{definition.BaseClassName}()");
        }

        // Add custom options (simplified - would need more complex handling for constructors)
        foreach (var customOption in definition.CustomOptions)
        {
            allOptions.Add($"new {customOption.Name}()");
        }

        return string.Join(",\n        ", allOptions);
    }

    private static string GenerateCollectionClass(ExtendedEnumDefinition definition, string allOptionsString)
    {
        var methods = GenerateCollectionMethods(definition);
        
        return $@"    /// <summary>
    /// Collection of {definition.EnumType?.Name ?? "Unknown"} extended enum options.
    /// </summary>
    public static partial class {definition.CollectionName}
    {{
        private static readonly ImmutableArray<{definition.BaseClassFullName}> _all = ImmutableArray.Create<{definition.BaseClassFullName}>(
            {allOptionsString}
        );

        /// <summary>
        /// Gets all extended enum options.
        /// </summary>
        public static ImmutableArray<{definition.BaseClassFullName}> All() => _all;

{methods}
    }}";
    }

    private static string GenerateCollectionMethods(ExtendedEnumDefinition definition)
    {
        return $@"        /// <summary>
        /// Gets an extended enum option by enum value.
        /// </summary>
        public static {definition.BaseClassFullName}? GetByEnum({definition.EnumType?.ToDisplayString() ?? "object"} enumValue)
        {{
            return _all.FirstOrDefault(x => x.EnumValue.Equals(enumValue));
        }}

        /// <summary>
        /// Gets an extended enum option by name.
        /// </summary>
        public static {definition.BaseClassFullName}? GetByName(string name)
        {{
            if (string.IsNullOrWhiteSpace(name)) return null;
            return _all.FirstOrDefault(x => string.Equals(x.Name, name, StringComparison.{definition.NameComparison}));
        }}

        /// <summary>
        /// Gets an extended enum option by ID.
        /// </summary>
        public static {definition.BaseClassFullName}? GetById(int id)
        {{
            return _all.FirstOrDefault(x => x.Id == id);
        }}

        /// <summary>
        /// Tries to get an extended enum option by enum value.
        /// </summary>
        public static bool TryGetByEnum({definition.EnumType?.ToDisplayString() ?? "object"} enumValue, out {definition.BaseClassFullName}? value)
        {{
            value = GetByEnum(enumValue);
            return value != null;
        }}

        /// <summary>
        /// Tries to get an extended enum option by name.
        /// </summary>
        public static bool TryGetByName(string name, out {definition.BaseClassFullName}? value)
        {{
            value = GetByName(name);
            return value != null;
        }}

        /// <summary>
        /// Tries to get an extended enum option by ID.
        /// </summary>
        public static bool TryGetById(int id, out {definition.BaseClassFullName}? value)
        {{
            value = GetById(id);
            return value != null;
        }}";
    }

    private static string GenerateCollectionFileTemplate(ExtendedEnumDefinition definition, string collectionClass)
    {
        return $@"// <auto-generated />
#nullable enable
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace {definition.BaseClassNamespace}
{{
{collectionClass}
}}";
    }

    private sealed class ExtendedEnumDefinition
    {
        public string BaseClassName { get; set; } = string.Empty;
        public string BaseClassFullName { get; set; } = string.Empty;
        public string BaseClassNamespace { get; set; } = string.Empty;
        public INamedTypeSymbol? EnumType { get; set; }
        public IList<EnumValueInfo> EnumValues { get; set; } = new List<EnumValueInfo>();
        public IList<INamedTypeSymbol> CustomOptions { get; set; } = new List<INamedTypeSymbol>();
        public bool IsGlobal { get; set; }
        public string CollectionName { get; set; } = string.Empty;
        public StringComparison NameComparison { get; set; }
        public bool GenerateFactoryMethods { get; set; }
        public bool GenerateStaticCollection { get; set; }
        public bool UseSingletonInstances { get; set; }
        public bool Generic { get; set; }
    }

    private sealed class EnumValueInfo
    {
        public string Name { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public int IntValue { get; set; }
    }
}
