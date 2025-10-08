using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis;
using FractalDataWorks.CodeBuilder.CSharp.Builders;
using FractalDataWorks.SourceGenerators.Configuration;
using FractalDataWorks.SourceGenerators.Generators;
using FractalDataWorks.SourceGenerators.Models;
using FractalDataWorks.SourceGenerators.Services;

namespace FractalDataWorks.SourceGenerators.Builders;

/// <summary>
/// Orchestrates the generation of collection classes using specialized generators.
/// This class follows the Single Responsibility Principle by delegating to focused generators.
/// </summary>
public sealed class GenericCollectionBuilder : IGenericCollectionBuilder
{
    private readonly CollectionBuilderConfiguration _config;
    private readonly FieldGenerator _fieldGenerator;
    private readonly LookupMethodGenerator _lookupMethodGenerator;
    private readonly EmptyClassGenerator _emptyClassGenerator;
    private readonly StaticConstructorGenerator _staticConstructorGenerator;
    private readonly ValuePropertyGenerator _valuePropertyGenerator;

    private CollectionGenerationMode _mode;
    private GenericTypeInfoModel? _definition;
    private IList<GenericValueInfoModel>? _values;
    private string? _returnType;
    private Compilation? _compilation;
    private bool _isUserClassStatic;
    private bool _isUserClassAbstract;

    /// <summary>
    /// Initializes a new instance of the <see cref="GenericCollectionBuilder"/> class.
    /// </summary>
    /// <param name="config">The configuration for collection generation.</param>
    public GenericCollectionBuilder(CollectionBuilderConfiguration config)
    {
        _config = config ?? throw new ArgumentNullException(nameof(config));
        _config.Validate();

        // Initialize generators
        _fieldGenerator = new FieldGenerator(config);
        _lookupMethodGenerator = new LookupMethodGenerator(config);
        _emptyClassGenerator = new EmptyClassGenerator(config);
        _staticConstructorGenerator = new StaticConstructorGenerator(config);
        _valuePropertyGenerator = new ValuePropertyGenerator(config);
    }

    /// <summary>
    /// Configures the generation mode for the collection.
    /// </summary>
    /// <param name="mode">The generation mode to use.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public IGenericCollectionBuilder Configure(CollectionGenerationMode mode)
    {
        _mode = mode;
        return this;
    }

    /// <summary>
    /// Sets the type definition for the collection.
    /// </summary>
    /// <param name="definition">The type definition containing collection metadata.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public IGenericCollectionBuilder WithDefinition(GenericTypeInfoModel definition)
    {
        _definition = definition ?? throw new ArgumentNullException(nameof(definition));
        return this;
    }

    /// <summary>
    /// Sets the collection values (options/implementations) to include.
    /// </summary>
    /// <param name="values">The list of value types to include in the collection.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public IGenericCollectionBuilder WithValues(IList<GenericValueInfoModel> values)
    {
        _values = values ?? throw new ArgumentNullException(nameof(values));
        return this;
    }

    /// <summary>
    /// Sets the return type for collection methods.
    /// </summary>
    /// <param name="returnType">The fully qualified return type name.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public IGenericCollectionBuilder WithReturnType(string returnType)
    {
        if (string.IsNullOrEmpty(returnType))
            throw new ArgumentException("Return type cannot be null or empty.", nameof(returnType));

        _returnType = returnType;
        return this;
    }

    /// <summary>
    /// Sets the compilation context for type resolution.
    /// </summary>
    /// <param name="compilation">The Roslyn compilation context.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public IGenericCollectionBuilder WithCompilation(Compilation compilation)
    {
        _compilation = compilation ?? throw new ArgumentNullException(nameof(compilation));
        return this;
    }

    /// <summary>
    /// Sets the modifiers from the user's declared partial class.
    /// The generated partial must match what the user declared.
    /// </summary>
    public IGenericCollectionBuilder WithUserClassModifiers(bool isStatic, bool isAbstract)
    {
        _isUserClassStatic = isStatic;
        _isUserClassAbstract = isAbstract;
        return this;
    }

    /// <summary>
    /// Builds the complete collection class source code.
    /// </summary>
    /// <returns>The generated C# source code for the collection class.</returns>
    public string Build()
    {
        ValidateConfiguration();

        var classBuilder = new ClassBuilder()
            .WithNamespace(_definition!.Namespace)
            .WithName(_definition.CollectionName)
            .WithAccessModifier("public")
            .AsPartial();

        // Match the user's declared class modifiers
        // The generated partial must match what the user declared
        if (_isUserClassStatic)
        {
            classBuilder.AsStatic();
        }

        // Note: We don't add abstract to the generated partial even if the user's class is abstract
        // because you can't have a partial class where one part is abstract and the other isn't

        // Add using directives
        classBuilder.WithUsings("System", "System.Collections.Generic", "System.Collections.Frozen", "System.Linq");

        // Generate fields using FieldGenerator
        var allField = _fieldGenerator.GenerateAllField(_returnType!);
        classBuilder.WithField(allField);

        // Always generate _empty field (initialized in static constructor)
        var emptyField = _fieldGenerator.GenerateEmptyField(_returnType!);
        classBuilder.WithField(emptyField);

        // Generate lookup dictionary fields (with conditional compilation)
        var lookupFields = _fieldGenerator.GenerateLookupDictionaryFields(_definition!, _returnType!);
        foreach (var field in lookupFields)
        {
            classBuilder.WithField(field);
        }

        // Generate lookup methods using LookupMethodGenerator
        var lookupMethods = _lookupMethodGenerator.GenerateDynamicLookupMethods(_definition!, _returnType!);
        foreach (var method in lookupMethods)
        {
            classBuilder.WithMethod(method);
        }

        // Generate All() method
        var allMethod = new MethodBuilder()
            .WithName("All")
            .WithReturnType($"IReadOnlyList<{_returnType}>")
            .WithAccessModifier("public")
            .AsStatic()
            .WithExpressionBody("_all.Values.ToList()")
            .WithXmlDoc("Gets all collection items.")
            .WithReturnDoc("A read-only list of all items in the collection.");

        classBuilder.WithMethod(allMethod);

        // Generate NotFound() method
        var notFoundMethod = new MethodBuilder()
            .WithName("NotFound")
            .WithReturnType(_returnType!)
            .WithAccessModifier("public")
            .AsStatic()
            .WithExpressionBody("_empty")
            .WithXmlDoc("Gets the empty/not-found instance for this collection.")
            .WithReturnDoc("The empty instance representing a not-found value.");

        classBuilder.WithMethod(notFoundMethod);

        // Generate static properties for each value
        var valueProperties = _valuePropertyGenerator.GenerateValueProperties(
            _values!,
            _returnType!,
            _definition!.UseMethods);
        foreach (var property in valueProperties)
        {
            classBuilder.WithProperty(property);
        }

        // Generate static constructor
        var staticConstructor = _staticConstructorGenerator.GenerateStaticConstructor(
            _definition!,
            _values!,
            _returnType!,
            _compilation!);
        classBuilder.WithConstructor(staticConstructor);

        // Build the class
        return classBuilder.Build();
    }

    /// <summary>
    /// Gets the generated Empty class code.
    /// </summary>
    public string GetEmptyClassCode()
    {
        if (_definition == null || _compilation == null)
            return string.Empty;

        // Use ClassName which contains the base type from TypeCollectionAttribute first parameter
        // This can be a class like "ConnectionStateBase" or an interface like "IAuthenticationCommand"
        var baseTypeName = _definition.ClassName;
        if (string.IsNullOrEmpty(baseTypeName))
            return string.Empty;

        // Check if the base type is actually a class (not an interface)
        // If it's an interface, skip Empty class generation
        var fullyQualifiedTypeName = baseTypeName.Contains(".")
            ? baseTypeName
            : $"{_definition.Namespace}.{baseTypeName}";

        var baseTypeSymbol = _compilation.GetTypeByMetadataName(fullyQualifiedTypeName);
        if (baseTypeSymbol == null || baseTypeSymbol.TypeKind != Microsoft.CodeAnalysis.TypeKind.Class)
            return string.Empty;

        return _emptyClassGenerator.GenerateEmptyClass(
            _definition,
            baseTypeName,
            _definition.Namespace,
            _compilation);
    }

    private void ValidateConfiguration()
    {
        if (_definition == null)
            throw new InvalidOperationException("Definition must be set before building.");

        if (_values == null)
            throw new InvalidOperationException("Values must be set before building.");

        if (string.IsNullOrEmpty(_returnType))
            throw new InvalidOperationException("Return type must be set before building.");

        if (_compilation == null)
            throw new InvalidOperationException("Compilation must be set before building.");
    }
}
