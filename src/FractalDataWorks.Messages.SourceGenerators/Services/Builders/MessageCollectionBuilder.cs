using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using FractalDataWorks.Messages.Attributes;
using FractalDataWorks.CodeBuilder.CSharp.Builders;
using FractalDataWorks.SourceGenerators.Models;

namespace FractalDataWorks.Messages.SourceGenerators.Services.Builders;

/// <summary>
/// Concrete implementation of IMessageCollectionBuilder that builds message collections
/// using the Gang of Four Builder pattern with fluent API.
/// This builder generates collection classes from message type definitions and values during source generation.
/// </summary>
public sealed class MessageCollectionBuilder : IMessageCollectionBuilder
{
    private CollectionTypeInfoModel? _definition;
    private IList<CollectionValueInfoModel>? _values;
    private string? _returnType;
    private string? _returnTypeNamespace;
    private Compilation? _compilation;
    private ClassBuilder? _classBuilder;

    /// <summary>
    /// Initializes a new instance of the <see cref="MessageCollectionBuilder"/> class.
    /// </summary>
    public MessageCollectionBuilder()
    {
    }

    /// <inheritdoc/>
    public IMessageCollectionBuilder WithDefinition(CollectionTypeInfoModel definition)
    {
        _definition = definition ?? throw new ArgumentNullException(nameof(definition));
        return this;
    }

    /// <inheritdoc/>
    public IMessageCollectionBuilder WithValues(IList<CollectionValueInfoModel> values)
    {
        _values = values ?? throw new ArgumentNullException(nameof(values));
        return this;
    }

    /// <inheritdoc/>
    public IMessageCollectionBuilder WithReturnType(string returnType)
    {
        if (string.IsNullOrEmpty(returnType))
        {
            throw new ArgumentException("Return type cannot be null or empty.", nameof(returnType));
        }

        // Extract namespace and short name from full type name
        var lastDotIndex = returnType.LastIndexOf('.');
        if (lastDotIndex > 0)
        {
            _returnTypeNamespace = returnType.Substring(0, lastDotIndex);
            _returnType = returnType.Substring(lastDotIndex + 1);
        }
        else
        {
            _returnTypeNamespace = null;
            _returnType = returnType;
        }
        
        return this;
    }

    /// <inheritdoc/>
    public IMessageCollectionBuilder WithCompilation(Compilation compilation)
    {
        _compilation = compilation ?? throw new ArgumentNullException(nameof(compilation));
        return this;
    }

    /// <inheritdoc/>
    public string Build()
    {
        ValidateConfiguration();

        _classBuilder = new ClassBuilder();
        
        BuildUsings();
        
        // Add #nullable enable directive after usings
        var result = BuildCore();
        
        // Insert #nullable enable after the using statements
        var lines = result.Split('\n').ToList();
        var lastUsingIndex = lines.FindLastIndex(l => l.TrimStart().StartsWith("using ", StringComparison.Ordinal));
        if (lastUsingIndex >= 0)
        {
            lines.Insert(lastUsingIndex + 1, "");
            lines.Insert(lastUsingIndex + 2, "#nullable enable");
        }
        
        return string.Join("\n", lines);
    }
    
    private string BuildCore()
    {
        BuildNamespace();
        BuildClass();
        
        // Always generate factory methods for each constructor
        foreach (var value in _values!.Where(v => v.Include))
        {
            GenerateFactoryMethodOverloads(value);
        }
        
        return _classBuilder!.Build();
    }

    /// <summary>
    /// Validates that all required configuration has been provided.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when required configuration is missing.</exception>
    private void ValidateConfiguration()
    {
        if (_definition == null)
        {
            throw new InvalidOperationException("CurrentMessage type definition must be provided using WithDefinition().");
        }

        if (_values == null)
        {
            throw new InvalidOperationException("CurrentMessage values must be provided using WithValues().");
        }

        if (string.IsNullOrEmpty(_returnType))
        {
            throw new InvalidOperationException("Return type must be provided using WithReturnType().");
        }

        if (_compilation == null)
        {
            throw new InvalidOperationException("Compilation context must be provided using WithCompilation().");
        }
    }

    /// <summary>
    /// Builds the using statements for the generated class.
    /// </summary>
    private void BuildUsings()
    {
        var usings = new List<string>
        {
            nameof(System),
            "System.Collections.Generic",
            "System.Collections.Immutable",
            "System.Collections.ObjectModel",
            "System.Linq"
        };

        // Add Messages namespace if inheriting from base
        if (_definition!.InheritsFromCollectionBase)
        {
            usings.Add("FractalDataWorks.Messages");
        }

        // Add namespaces for all message value types so constructors can find the classes
        foreach (var value in _values!.Where(v => v.Include))
        {
            if (!string.IsNullOrEmpty(value.ReturnTypeNamespace) && 
                !string.Equals(value.ReturnTypeNamespace, _definition!.Namespace, StringComparison.Ordinal))
            {
                usings.Add(value.ReturnTypeNamespace!);
            }
        }

        // Add return type namespace from builder (extracted from full return type name)
        if (!string.IsNullOrEmpty(_returnTypeNamespace) && 
            !string.Equals(_returnTypeNamespace, _definition!.Namespace, StringComparison.Ordinal))
        {
            usings.Add(_returnTypeNamespace!);
        }

        // Add additional namespaces based on return type and requirements
        if (!string.IsNullOrEmpty(_definition!.ReturnTypeNamespace) && 
            !string.Equals(_definition.ReturnTypeNamespace, _definition.Namespace, StringComparison.Ordinal))
        {
            usings.Add(_definition.ReturnTypeNamespace!);
        }

        foreach (var ns in _definition.RequiredNamespaces)
        {
            if (!string.Equals(ns, _definition.Namespace, StringComparison.Ordinal))
            {
                usings.Add(ns);
            }
        }

        _classBuilder!.WithUsings(usings.Distinct(StringComparer.Ordinal).ToArray());
    }

    /// <summary>
    /// Builds the namespace declaration.
    /// </summary>
    private void BuildNamespace()
    {
        _classBuilder!.WithNamespace(_definition!.Namespace);
    }

    /// <summary>
    /// Builds the class structure and contents.
    /// </summary>
    private void BuildClass()
    {
        _classBuilder!.WithName(_definition!.CollectionName)
                      .WithXmlDoc($"Provides a collection of {_definition.ClassName} message values.")
                      .AsAbstract();
    }

    /// <summary>
    /// Generates factory method overloads for all constructors of a message value.
    /// </summary>
    /// <param name="value">The message value to generate factory methods for.</param>
    private void GenerateFactoryMethodOverloads(CollectionValueInfoModel value)
    {
        if (value.Constructors.Count == 0)
        {
            // No constructors found, generate basic parameterless method
            var methodName = GetFactoryMethodName(value);
            var basicMethod = new MethodBuilder()
                .WithName(methodName)
                .WithReturnType(_returnType!)
                .WithAccessModifier("public")
                .AsStatic()
                .WithXmlDoc($"Creates a new instance of the {value.Name} message value (no constructors found).")
                .WithReturnDoc($"A new instance of the {value.Name} message value.")
                .WithExpressionBody($"new {value.ShortTypeName}()");
            
            _classBuilder!.WithMethod(basicMethod);
            return;
        }

        // Generate one method for EACH constructor
        foreach (var constructor in value.Constructors)
        {
            var methodName = GetFactoryMethodName(value);
            
            var methodBuilder = new MethodBuilder()
                .WithName(methodName)
                .WithReturnType(_returnType!)
                .WithAccessModifier("public")
                .AsStatic();

            // Add parameters from constructor - each constructor gets its own method
            var argumentList = new List<string>();

            for (int i = 0; i < constructor.Parameters.Count; i++)
            {
                var param = constructor.Parameters[i];
                var paramName = string.IsNullOrEmpty(param.Name) ? $"param{i}" : param.Name;
                
                methodBuilder.WithParameter(param.TypeName, paramName);
                argumentList.Add(paramName);
                
                // Add parameter documentation
                var paramDescription = GetParameterDescription(param, paramName);
                methodBuilder.WithParamDoc(paramName, paramDescription);
            }

            // Build method body with the exact constructor parameters
            var arguments = string.Join(", ", argumentList);
            methodBuilder.WithExpressionBody($"new {value.ShortTypeName}({arguments})");

            // Build XML documentation  
            var xmlDoc = $"Creates a new instance of the {value.Name} message value";
            if (constructor.Parameters.Count > 0)
            {
                xmlDoc += $" with {constructor.Parameters.Count} parameter{(constructor.Parameters.Count > 1 ? "s" : "")}";
                // Debug: add parameter info to XML doc
                xmlDoc += $" (params: {string.Join(", ", constructor.Parameters.Select(p => p.TypeName))})";
            }
            xmlDoc += ".";

            methodBuilder.WithXmlDoc(xmlDoc)
                         .WithReturnDoc($"A new instance of the {value.Name} message value.");

            _classBuilder!.WithMethod(methodBuilder);
        }
    }

    /// <summary>
    /// Gets a description for a constructor parameter for XML documentation.
    /// </summary>
    /// <param name="param">The parameter information.</param>
    /// <param name="paramName">The parameter name.</param>
    /// <returns>A description for the parameter.</returns>
    private static string GetParameterDescription(ParameterInfo param, string paramName)
    {
        // Try to generate meaningful descriptions based on parameter type and name
        var typeName = param.TypeName.ToLowerInvariant();
        
        if (paramName.ToLowerInvariant().Contains("message", StringComparison.OrdinalIgnoreCase) || paramName.ToLowerInvariant().Contains("error", StringComparison.OrdinalIgnoreCase))
        {
            return "The message or error description.";
        }
        
        if (paramName.ToLowerInvariant().Contains("type", StringComparison.OrdinalIgnoreCase) || paramName.ToLowerInvariant().Contains("entity", StringComparison.OrdinalIgnoreCase))
        {
            return "The type or entity identifier.";
        }
        
        if (typeName.Contains("dictionary") || typeName.Contains("idictionary"))
        {
            return "The validation details dictionary.";
        }
        
        if (typeName.Contains("object", StringComparison.OrdinalIgnoreCase) && paramName.ToLowerInvariant().Contains("failed", StringComparison.OrdinalIgnoreCase))
        {
            return "The object that failed validation.";
        }
        
        // Fallback to generic description
        return $"The {paramName} parameter.";
    }

    /// <summary>
    /// Gets the factory method name for a message value.
    /// For ServiceMessage types, removes "CurrentMessage" suffix. For others, adds "Create" prefix.
    /// </summary>
    /// <param name="value">The message value information.</param>
    /// <returns>The method name to use for factory methods.</returns>
    private static string GetFactoryMethodName(CollectionValueInfoModel value)
    {
        var name = value.Name;
        
        // For ServiceMessage types, remove "CurrentMessage" suffix if present
        if (name.EndsWith("CurrentMessage", StringComparison.Ordinal))
        {
            return name.Substring(0, name.Length - "CurrentMessage".Length);
        }
        
        // For other types, use Create prefix
        return $"Create{name}";
    }
}
