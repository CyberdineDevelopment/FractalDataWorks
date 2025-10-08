using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using FractalDataWorks.Collections.Attributes;
using FractalDataWorks.Collections.Models;
using FractalDataWorks.SourceGenerators.Models;
using FractalDataWorks.CodeBuilder.CSharp.Builders;
using FractalDataWorks.CodeBuilder.Abstractions;

namespace FractalDataWorks.Collections.SourceGenerators.Services.Builders;

/// <summary>
/// Concrete implementation of IEnumCollectionBuilder that builds enhanced enum collections
/// using the Gang of Four Builder pattern with fluent API.
/// This builder generates collection classes from enum type definitions and values during source generation.
/// </summary>
#pragma warning disable MA0026 // TODO
// TODO: Break this class into smaller, focused classes (e.g., MethodBuilder, FieldBuilder, ConstructorBuilder)
#pragma warning restore MA0026
public sealed class EnumCollectionBuilder : IEnumCollectionBuilder
{
    private EnumTypeInfoModel? _definition;
    private IList<EnumValueInfoModel>? _values;
    private string? _returnType;
    private string? _fullReturnType;
    private string? _returnTypeNamespace;
    private Compilation? _compilation;
    private ClassBuilder? _classBuilder;
    private bool _isUserClassStatic;
    private bool _isUserClassAbstract;

    /// <summary>
    /// Initializes a new instance of the <see cref="EnumCollectionBuilder"/> class.
    /// </summary>
    public EnumCollectionBuilder()
    {
    }


    /// <inheritdoc/>
    public IEnumCollectionBuilder WithDefinition(EnumTypeInfoModel definition)
    {
        _definition = definition ?? throw new ArgumentNullException(nameof(definition));
        return this;
    }

    /// <inheritdoc/>
    public IEnumCollectionBuilder WithValues(IList<EnumValueInfoModel> values)
    {
        _values = values ?? throw new ArgumentNullException(nameof(values));
        return this;
    }

    /// <inheritdoc/>
    public IEnumCollectionBuilder WithReturnType(string returnType)
    {
        if (string.IsNullOrEmpty(returnType))
        {
            throw new ArgumentException("Return type cannot be null or empty.", nameof(returnType));
        }

        // Store the full type for namespace extraction
        _fullReturnType = returnType;

        // Extract the short name (everything after the last dot)
        var lastDot = returnType.LastIndexOf('.');
        _returnType = lastDot >= 0 ? returnType.Substring(lastDot + 1) : returnType;

        // Extract namespace if present
        if (lastDot >= 0)
        {
            _returnTypeNamespace = returnType.Substring(0, lastDot);
        }

        return this;
    }

    /// <inheritdoc/>
    public IEnumCollectionBuilder WithCompilation(Compilation compilation)
    {
        _compilation = compilation ?? throw new ArgumentNullException(nameof(compilation));
        return this;
    }

    /// <summary>
    /// Sets the modifiers from the user's declared partial class.
    /// </summary>
    /// <param name="isStatic">Whether the user's class is static.</param>
    /// <param name="isAbstract">Whether the user's class is abstract.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public IEnumCollectionBuilder WithUserClassModifiers(bool isStatic, bool isAbstract)
    {
        _isUserClassStatic = isStatic;
        _isUserClassAbstract = isAbstract;
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
        AddCommonElements();
        
        // Always build standard static collection with all features
        return BuildDefaultCollection();
    }

    /// <summary>
    /// Validates that all required configuration has been provided.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when required configuration is missing.</exception>
    private void ValidateConfiguration()
    {
        if (_definition == null)
        {
            throw new InvalidOperationException("Enum type definition must be provided using WithDefinition().");
        }

        if (_values == null)
        {
            throw new InvalidOperationException("Enum values must be provided using WithValues().");
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
            "System.Collections.Frozen",
            "System.Collections.Generic",
            "System.Collections.Immutable",
            "System.Collections.ObjectModel",
            "System.Linq"
        };

        // Add EnhancedEnums namespace if inheriting from base
        if (_definition!.InheritsFromCollectionBase)
        {
            usings.Add("FractalDataWorks.Collections");
        }

        // Add namespaces for all enum value types so constructors can find the classes
        foreach (var value in _values!.Where(v => v.Include))
        {
            if (!string.IsNullOrEmpty(value.ReturnTypeNamespace) && 
                !string.Equals(value.ReturnTypeNamespace, _definition!.Namespace, StringComparison.Ordinal))
            {
                usings.Add(value.ReturnTypeNamespace!);
            }
        }

        // Add the return type's namespace if it's different from the current namespace
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
        // For TypeCollectionBase, generate a class without "Base" suffix
        var generatedClassName = _definition!.InheritsFromCollectionBase && _definition.CollectionName.EndsWith("Base", StringComparison.Ordinal)
            ? _definition.CollectionName.Substring(0, _definition.CollectionName.Length - 4)
            : _definition.CollectionName;

        var classBuilder = _classBuilder!.WithName(generatedClassName)
                      .WithXmlDoc($"Provides a collection of {_definition.ClassName} enum values.")
                      .WithAccessModifier("public");

        // Apply the correct modifiers based on the user's declared partial class
        // Rule: If user's class is static, generate as static
        //       Match the modifiers from the user's partial class declaration
        //       Both partial declarations must have the same modifiers
        if (_isUserClassStatic)
        {
            classBuilder.AsStatic();
        }
        if (_isUserClassAbstract)
        {
            classBuilder.AsAbstract();
        }
        // Always add partial since we're generating a partial class
        classBuilder.AsPartial();

        // For TypeCollectionBase inheritance, copy static members from base class
        if (_definition.InheritsFromCollectionBase)
        {
            CopyStaticMembersFromBaseClass((ClassBuilder)classBuilder, generatedClassName);
        }

        // No inheritance in generated partial - user's partial already has it
        // We'll generate all the collection members
    }

    /// <summary>
    /// Adds common elements shared across all generation modes.
    /// </summary>
    private void AddCommonElements()
    {
        // Add static fields for enum values (but not for TypeCollections)
        if (!IsTypeCollection())
        {
            foreach (var value in _values!.Where(v => v.Include))
            {
                var fieldBuilder = new FieldBuilder()
                    .WithName(value.Name)
                    .WithType(_returnType!)
                    .WithAccessModifier("public")
                    .AsStatic()
                    .AsReadOnly()
                    .WithInitializer(GenerateValueInitializer(value))
                    .WithXmlDoc($"Gets the {value.Name} enum value.");

                _classBuilder!.WithField(fieldBuilder);
            }
        }
        
        // Parse and reconstruct members from base class if inheriting from EnumCollectionBase
        if (_definition!.InheritsFromCollectionBase && _compilation != null)
        {
            ReconstructMembersFromBase();
        }
    }
    
    /// <summary>
    /// Parses and reconstructs all members from EnumCollectionBase&lt;T&gt;, handling hiding and overriding.
    /// </summary>
    private void ReconstructMembersFromBase()
    {
        // Get the EnumCollectionBase<T> type
        var enumCollectionBase = _compilation!.GetTypeByMetadataName("FractalDataWorks.Collections.TypeCollectionBase`1");
        if (enumCollectionBase == null)
        {
            throw new InvalidOperationException("Cannot find TypeCollectionBase<T> in compilation. Ensure FractalDataWorks.Collections is referenced.");
        }
        
        // Get the concrete collection type (e.g., ProcessStateCollectionBase)
        var collectionType = _compilation.GetTypeByMetadataName(_definition!.FullTypeName);
        if (collectionType == null)
        {
            throw new InvalidOperationException($"Cannot find collection type {_definition.FullTypeName} in compilation.");
        }
        
        // Add interfaces from the return type if available
        AddInterfacesFromReturnType();
        
        // Construct the closed generic type (e.g., EnumCollectionBase<ProcessStateBase>)
        var baseType = collectionType.BaseType;
        while (baseType != null)
        {
            if (baseType.OriginalDefinition.Equals(enumCollectionBase, SymbolEqualityComparer.Default))
            {
                // Found the EnumCollectionBase<T> in the inheritance chain
                ReconstructMembersFromType(baseType, collectionType);
                break;
            }
            baseType = baseType.BaseType;
        }
    }
    
    /// <summary>
    /// Adds interfaces from the return type to the generated class.
    /// </summary>
    private static void AddInterfacesFromReturnType()
    {
        // Collections shouldn't implement the interfaces of their items
        // A ProcessStates collection doesn't implement IProcessState
        // That would be mixing collection behavior with item behavior
        return;
    }
    
    /// <summary>
    /// Reconstructs members from a specific type, handling hiding and overriding.
    /// </summary>
    private void ReconstructMembersFromType(INamedTypeSymbol baseType, INamedTypeSymbol derivedType)
    {
        // Get ALL members from the derived type (includes inherited members)
        var allMembers = derivedType.GetMembers();
        
        // Track which members we've already processed to avoid duplicates
        var processedMembers = new HashSet<string>(StringComparer.Ordinal);
        
        // Process members from most derived to base, ensuring we only take the most derived version
        foreach (var member in allMembers)
        {
            // Skip constructors and special members
            if (member is IMethodSymbol { MethodKind: MethodKind.Constructor or MethodKind.StaticConstructor })
                continue;
            
            // Create a unique signature for this member
            var signature = GetMemberSignature(member);
            
            // Skip if we've already processed this member (avoids duplicates from overrides/hiding)
            if (!processedMembers.Add(signature))
                continue;
            
            // Reconstruct the member based on its type
            switch (member)
            {
                case IFieldSymbol field:
                    ReconstructField(field);
                    break;
                case IPropertySymbol property:
                    ReconstructProperty(property);
                    break;
                case IMethodSymbol method:
                    ReconstructMethod(method);
                    break;
            }
        }
    }
    
    /// <summary>
    /// Gets a unique signature for a member to detect overrides/hiding.
    /// </summary>
    private static string GetMemberSignature(ISymbol member)
    {
        return member switch
        {
            IMethodSymbol method => $"{method.Name}({string.Join(",", method.Parameters.Select(p => p.Type.ToDisplayString()))})",
            IPropertySymbol property => $"{property.Name}:Property",
            IFieldSymbol field => $"{field.Name}:Field",
            _ => member.Name
        };
    }
    
    /// <summary>
    /// Reconstructs a field from the base type as static.
    /// </summary>
    private void ReconstructField(IFieldSymbol field)
    {
        // Replace generic type parameter T with concrete type
        var fieldType = ReplaceGenericType(field.Type.ToDisplayString());
        
        var fieldBuilder = new FieldBuilder()
            .WithName(field.Name)
            .WithType(fieldType)
            .WithAccessModifier(GetAccessModifier(field.DeclaredAccessibility))
            .AsStatic();
        
        if (field.IsReadOnly)
            fieldBuilder.AsReadOnly();
        
        if (field.IsConst)
        {
            fieldBuilder.AsConst();
            if (field.HasConstantValue)
                fieldBuilder.WithInitializer(field.ConstantValue?.ToString() ?? "default");
        }
        
        _classBuilder!.WithField(fieldBuilder);
    }
    
    /// <summary>
    /// Reconstructs a property from the base type as static.
    /// </summary>
    private void ReconstructProperty(IPropertySymbol property)
    {
        // Replace generic type parameter T with concrete type
        var propertyType = ReplaceGenericType(property.Type.ToDisplayString());
        
        var propertyBuilder = new PropertyBuilder()
            .WithName(property.Name)
            .WithType(propertyType)
            .WithAccessModifier(GetAccessModifier(property.DeclaredAccessibility))
            .AsStatic();
        
        // Handle getter
        if (property.GetMethod != null)
        {
            var getterBody = GetPropertyGetterBody(property);
            propertyBuilder.WithGetter(getterBody);
        }
        
        // Handle setter
        if (property.SetMethod != null)
        {
            var setterBody = GetPropertySetterBody(property);
            propertyBuilder.WithSetter(setterBody);
        }
        
        _classBuilder!.WithProperty(propertyBuilder);
    }
    
    /// <summary>
    /// Reconstructs a method from the base type as static.
    /// </summary>
    private void ReconstructMethod(IMethodSymbol method)
    {
        // Skip special methods and property accessors
        if (method.MethodKind != MethodKind.Ordinary || method.AssociatedSymbol != null)
            return;
        
        // Replace generic type parameter T with concrete type
        var returnType = ReplaceGenericType(method.ReturnType.ToDisplayString());
        
        var methodBuilder = new MethodBuilder()
            .WithName(method.Name)
            .WithReturnType(returnType)
            .WithAccessModifier(GetAccessModifier(method.DeclaredAccessibility))
            .AsStatic();
        
        // Add parameters
        foreach (var parameter in method.Parameters)
        {
            var paramType = ReplaceGenericType(parameter.Type.ToDisplayString());
            var paramName = parameter.Name;
            
            // Handle ref/out/params modifiers
            if (parameter.IsParams)
            {
                paramType = $"params {paramType}";
            }
            else if (parameter.RefKind == RefKind.Out)
            {
                paramType = $"out {paramType}";
            }
            else if (parameter.RefKind == RefKind.Ref)
            {
                paramType = $"ref {paramType}";
            }
            
            // Add parameter with or without default value
            if (parameter.HasExplicitDefaultValue)
            {
                var defaultValue = GetDefaultValueString(parameter.ExplicitDefaultValue, parameter.Type);
                methodBuilder.WithParameter(paramType, paramName, defaultValue);
            }
            else
            {
                methodBuilder.WithParameter(paramType, paramName);
            }
        }
        
        // Get the method body (this is simplified - in reality we'd need to parse the actual implementation)
        var methodBody = GetMethodBody(method);
        methodBuilder.WithBody(methodBody);
        
        _classBuilder!.WithMethod(methodBuilder);
        
        // Special case: if this is All(), also create GetAll() wrapper
        if (string.Equals(method.Name, "All", StringComparison.Ordinal) && method.IsStatic && method.Parameters.Length == 0)
        {
            var getAllBuilder = new MethodBuilder()
                .WithName("GetAll")
                .WithReturnType(returnType)
                .WithAccessModifier("public")
                .AsStatic()
                .WithXmlDoc("Gets all enum values in the collection.")
                .WithBody("return All();");
            
            _classBuilder!.WithMethod(getAllBuilder);
        }
    }
    
    /// <summary>
    /// Replaces generic type parameter T with the concrete return type.
    /// </summary>
    private string ReplaceGenericType(string type)
    {
        // Replace T with the concrete type
        // Handle various forms: T, T?, ImmutableArray<T>, IEnumerable<T>, etc.
        return type
            .Replace("<T>", $"<{_returnType}>")
            .Replace("<T?>", $"<{_returnType}?>")
            .Replace(" T ", $" {_returnType} ")
            .Replace(" T?", $" {_returnType}?")
            .Replace("(T ", $"({_returnType} ")
            .Replace("(T?", $"({_returnType}?")
            .Replace(", T ", $", {_returnType} ")
            .Replace(", T?", $", {_returnType}?")
            .Replace("<T, ", $"<{_returnType}, ")
            .Replace(", T>", $", {_returnType}>")
            .Replace("^T", $"{_returnType}"); // Start of string
    }
    
    private static string GetDefaultValueString(object? defaultValue, ITypeSymbol parameterType)
    {
        if (defaultValue == null)
        {
            // For reference types and nullable value types
            if (parameterType.IsReferenceType || parameterType.NullableAnnotation == NullableAnnotation.Annotated)
            {
                return "null";
            }
            return "default";
        }
        
        // Handle string literals
        if (defaultValue is string str)
        {
            return $"\"{str}\"";
        }
        
        // Handle char literals
        if (defaultValue is char ch)
        {
            return $"'{ch}'";
        }
        
        // Handle boolean
        if (defaultValue is bool b)
        {
            return b ? "true" : "false";
        }
        
        // Handle numeric types with proper suffixes
        if (defaultValue is float f)
        {
            return $"{f}f";
        }
        if (defaultValue is double d)
        {
            return $"{d}d";
        }
        if (defaultValue is decimal dec)
        {
            return $"{dec}m";
        }
        if (defaultValue is long l)
        {
            return $"{l}L";
        }
        if (defaultValue is uint ui)
        {
            return $"{ui}u";
        }
        if (defaultValue is ulong ul)
        {
            return $"{ul}ul";
        }
        
        // For other types, use ToString()
        return defaultValue.ToString() ?? "default";
    }
    
    /// <summary>
    /// Gets the method body for reconstruction.
    /// </summary>
    private static string GetMethodBody(IMethodSymbol method)
    {
        // This is a simplified implementation
        // In a real implementation, we'd need to parse the actual method body from source
        // or use semantic analysis to understand what the method does
        
        // For now, provide implementations for known methods
        switch (method.Name)
        {
            case "All":
                return "return _all;";
            case "Empty":
                return "return _empty;";
            case "GetByName":
                return @"if (name is null || string.IsNullOrWhiteSpace(name)) return null;
                return _all.FirstOrDefault(x => string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase));";
            case "GetById":
                return @"return _all.FirstOrDefault(x => x.Id == id);";
            case "TryGetByName":
                return @"value = GetByName(name);
                return value != null;";
            case "TryGetById":
                return @"value = GetById(id);
                return value != null;";
            case "AsEnumerable":
                return "return _all;";
            case "Any":
                return "return _all.Length > 0;";
            case "GetByIndex":
                return "return _all[index];";
            default:
                // For unknown methods, provide a placeholder
                return $"throw new NotImplementedException(\"Method {method.Name} needs implementation\");";
        }
    }
    
    /// <summary>
    /// Gets the property getter body for reconstruction.
    /// </summary>
    private static string GetPropertyGetterBody(IPropertySymbol property)
    {
        // Simplified implementation for known properties
        switch (property.Name)
        {
            case "Count":
                return "return _all.Length;";
            default:
                return $"throw new NotImplementedException(\"Property {property.Name} getter needs implementation\");";
        }
    }
    
    /// <summary>
    /// Gets the property setter body for reconstruction.
    /// </summary>
    private static string GetPropertySetterBody(IPropertySymbol property)
    {
        return $"throw new NotImplementedException(\"Property {property.Name} setter needs implementation\");";
    }
    
    /// <summary>
    /// Converts Roslyn accessibility to string modifier.
    /// </summary>
    private static string GetAccessModifier(Accessibility accessibility)
    {
        return accessibility switch
        {
            Accessibility.Public => "public",
            Accessibility.Protected => "protected",
            Accessibility.Private => "private",
            Accessibility.Internal => "internal",
            Accessibility.ProtectedOrInternal => "protected internal",
            Accessibility.ProtectedAndInternal => "private protected",
            _ => "private"
        };
    }

    /// <summary>
    /// Generates the initializer expression for an enum value field.
    /// </summary>
    /// <param name="value">The enum value to generate an initializer for.</param>
    /// <returns>The initializer expression as a string.</returns>
    private static string GenerateValueInitializer(EnumValueInfoModel value)
    {
        if (value.Constructors.Count > 0)
        {
            var constructor = value.Constructors.FirstOrDefault(c => c.Parameters.Count == 0) ?? value.Constructors.First();
            var parameters = string.Join(", ", constructor.Parameters.Select(p => p.DefaultValue ?? "default"));
            return $"new {value.ShortTypeName}({parameters})";
        }

        return $"new {value.ShortTypeName}()";
    }

    /// <summary>
    /// Builds default collection as self-contained abstract class.
    /// </summary>
    /// <returns>The generated source code.</returns>
    private string BuildDefaultCollection()
    {
        // If not a TypeCollection, use original EnumCollection generation
        if (!IsTypeCollection())
        {
            // Add all EnumCollectionBase fields (protected static)
            AddStaticFields();
            
#pragma warning disable MA0026 // TODO
            // TODO: Add compiler directives for FrozenDictionary support on .NET 8+
#pragma warning restore MA0026
            // #if NET8_0_OR_GREATER use FrozenDictionary, else use Dictionary
            // This requires adding preprocessor directive support to FieldBuilder
            
            // Add dictionary fields for all lookup properties
            AddLookupDictionaries();
            
            // Add static constructor
            AddStaticConstructor();
            
            // Add all EnumCollectionBase static methods with concrete type
            BuildAllMethod(true);
            AddGetAllMethod();  // Add GetAll() wrapper for compatibility
            AddEmptyMethod();
            AddAsEnumerableMethod();
            BuildCountProperty(true);
            AddAnyMethod();
            AddGetByIndexMethod();
            
            // Add lookup methods from attributes (including Name and Id from EnumOptionBase)
            // These will use the dictionaries for O(1) lookups
            BuildLookupMethods(true);
            
            return _classBuilder!.Build();
        }
        
        // Use new TypeCollection generation with FrozenDictionary support
        AddDefaultCollectionMembers();
        
        return _classBuilder!.Build();
    }

    /// <summary>
    /// Builds collection for instance generation mode (singleton pattern).
    /// </summary>
    /// <returns>The generated source code.</returns>
    private string BuildInstanceCollection()
    {
        // Add singleton instance field
        var instanceField = new FieldBuilder()
            .WithName("Instance")
            .WithType(_definition!.CollectionName)
            .WithAccessModifier("public")
            .AsStatic()
            .AsReadOnly()
            .WithInitializer($"new {_definition.CollectionName}()")
            .WithXmlDoc("Gets the singleton instance of the collection.");
        
        _classBuilder!.WithField(instanceField);

        // Add private constructor
        var constructor = new ConstructorBuilder()
            .WithClassName(_definition.CollectionName)
            .WithAccessModifier("private")
            .WithXmlDoc("Initializes a new instance of the collection (private to enforce singleton pattern).");
        
        _classBuilder.WithConstructor(constructor);

        BuildAllMethod(false);
        BuildCountProperty(false);
        BuildLookupMethods(false);
        
        return _classBuilder.Build();
    }

    /// <summary>
    /// Builds collection for factory generation mode.
    /// </summary>
    /// <returns>The generated source code.</returns>
    private string BuildFactoryCollection()
    {
        BuildAllMethod(true);
        BuildCountProperty(true);
        BuildLookupMethods(true);
        
        // Add factory methods for individual values with overloads for each constructor
        foreach (var value in _values!.Where(v => v.Include))
        {
            GenerateFactoryMethodOverloads(value);
        }
        
        return _classBuilder!.Build();
    }

    /// <summary>
    /// Builds collection for service generation mode (dependency injection).
    /// </summary>
    /// <returns>The generated source code.</returns>
    private string BuildServiceCollection()
    {
        // Add public constructor for DI
        var constructor = new ConstructorBuilder()
            .WithClassName(_definition!.CollectionName)
            .WithAccessModifier("public")
            .WithXmlDoc("Initializes a new instance of the service collection.");
        
        _classBuilder!.WithConstructor(constructor);

        BuildAllMethod(false);
        BuildCountProperty(false);
        BuildLookupMethods(false);
        
        return _classBuilder.Build();
    }

    /// <summary>
    /// Generates factory method overloads for all constructors of an enum value.
    /// </summary>
    /// <param name="value">The enum value to generate factory methods for.</param>
    private void GenerateFactoryMethodOverloads(EnumValueInfoModel value)
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
                .WithXmlDoc($"Creates a new instance of the {value.Name} enum value.")
                .WithReturnDoc($"A new instance of the {value.Name} enum value.")
                .WithExpressionBody($"new {value.ShortTypeName}()");
            
            _classBuilder!.WithMethod(basicMethod);
            return;
        }

        foreach (var constructor in value.Constructors)
        {
            // For ServiceMessage types, use the enum name without "Message" suffix and without "Create" prefix
            var methodName = GetFactoryMethodName(value);
            
            var methodBuilder = new MethodBuilder()
                .WithName(methodName)
                .WithReturnType(_returnType!)
                .WithAccessModifier("public")
                .AsStatic();

            // Add parameters from constructor
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

            // Build method body
            var arguments = string.Join(", ", argumentList);
            methodBuilder.WithExpressionBody($"new {value.ShortTypeName}({arguments})");

            // Build XML documentation
            var xmlDoc = $"Creates a new instance of the {value.Name} enum value";
            if (constructor.Parameters.Count > 0)
            {
                xmlDoc += $" with {constructor.Parameters.Count} parameter{(constructor.Parameters.Count > 1 ? "s" : "")}";
            }
            xmlDoc += ".";

            methodBuilder.WithXmlDoc(xmlDoc)
                         .WithReturnDoc($"A new instance of the {value.Name} enum value.");

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
        
        if (paramName.Contains("message", StringComparison.OrdinalIgnoreCase) || paramName.Contains("error", StringComparison.OrdinalIgnoreCase))
        {
            return "The message or error description.";
        }
        
        if (paramName.Contains("type", StringComparison.OrdinalIgnoreCase) || paramName.Contains("entity", StringComparison.OrdinalIgnoreCase))
        {
            return "The type or entity identifier.";
        }
        
        if (typeName.Contains("dictionary") || typeName.Contains("idictionary"))
        {
            return "The validation details dictionary.";
        }
        
        if (typeName.Contains("object", StringComparison.OrdinalIgnoreCase) && paramName.Contains("failed", StringComparison.OrdinalIgnoreCase))
        {
            return "The object that failed validation.";
        }
        
        // Fallback to generic description
        return $"The {paramName} parameter.";
    }

    /// <summary>
    /// Gets the factory method name for an enum value.
    /// For ServiceMessage types, removes "Message" suffix. For others, adds "Create" prefix.
    /// </summary>
    /// <param name="value">The enum value information.</param>
    /// <returns>The method name to use for factory methods.</returns>
    private static string GetFactoryMethodName(EnumValueInfoModel value)
    {
        var name = value.Name;
        
        // For ServiceMessage types, remove "Message" suffix if present
        if (name.EndsWith("Message", StringComparison.Ordinal))
        {
            return name.Substring(0, name.Length - "Message".Length);
        }
        
        // For other types, use Create prefix
        return $"Create{name}";
    }

    /// <summary>
    /// Builds the All() method that returns all enum values.
    /// </summary>
    /// <param name="isStatic">Whether the method should be static.</param>
    private void BuildAllMethod(bool isStatic)
    {
        var returnTypeForCollection = $"ImmutableArray<{_returnType}>";
        string methodBody;

        if (_definition!.UseSingletonInstances)
        {
            // Singleton mode: return cached instances
            methodBody = "_all";
        }
        else
        {
            // Factory mode: create new instances each time by reconstructing from types
            var factoryCalls = string.Join(", ", _values!.Where(v => v.Include).Select(v => $"new {v.ShortTypeName}()"));
            methodBody = $"ImmutableArray.Create<{_returnType}>({factoryCalls})";
        }

        var methodBuilder = new MethodBuilder()
            .WithName("All")
            .WithReturnType(returnTypeForCollection)
            .WithAccessModifier("public")
            .WithXmlDoc("Gets all enum values in the collection.")
            .WithReturnDoc("A read-only list containing all enum values.");

        methodBuilder.WithExpressionBody(methodBody);

        if (isStatic)
        {
            methodBuilder.AsStatic();
        }

        _classBuilder!.WithMethod(methodBuilder);
    }

    /// <summary>
    /// Builds the Count property that returns the total number of enum values.
    /// </summary>
    /// <param name="isStatic">Whether the property should be static.</param>
    private void BuildCountProperty(bool isStatic)
    {
        var propertyBuilder = new PropertyBuilder()
            .WithName("Count")
            .WithType("int")
            .WithAccessModifier("public")
            .AsReadOnly()
            .WithXmlDoc("Gets the total number of enum values in the collection.")
            .WithExpressionBody("_all.Length");

        if (isStatic)
        {
            propertyBuilder.AsStatic();
        }

        _classBuilder!.WithProperty(propertyBuilder);
    }

    /// <summary>
    /// Builds lookup methods for properties defined in the enum type definition.
    /// </summary>
    /// <param name="isStatic">Whether the methods should be static.</param>
    private void BuildLookupMethods(bool isStatic)
    {
        foreach (var lookup in _definition!.LookupProperties)
        {
            BuildByPropertyMethod(lookup, isStatic);
            
            if (lookup.GenerateTryGet)
            {
                BuildTryGetByPropertyMethod(lookup, isStatic);
            }
        }

        // Don't add default ByName methods - they come from EnumLookup attributes on EnumOptionBase
    }

    /// <summary>
    /// Builds a lookup method for a specific property.
    /// </summary>
    /// <param name="lookup">The property lookup information.</param>
    /// <param name="isStatic">Whether the method should be static.</param>
#pragma warning disable MA0051 // Method is too long
    private void BuildByPropertyMethod(PropertyLookupInfoModel lookup, bool isStatic)
    {
        // All lookups should return non-nullable types and use _empty instead of null
        var returnType = lookup.AllowMultiple ? $"IEnumerable<{_returnType}>" : _returnType!;
        
        var fieldName = $"_by{lookup.PropertyName}";
        string methodBody;
        
        if (_definition!.UseSingletonInstances)
        {
            // Use dictionaries for O(1) lookup
            if (lookup.AllowMultiple)
            {
                methodBody = $"return {fieldName}[value];";
            }
            else
            {
                methodBody = $"return {fieldName}.TryGetValue(value, out var result) ? result : _empty;";
            }
        }
        else
        {
            // Factory mode - linear search with new instances
            var comparisonLogic = GenerateComparisonLogic(lookup);
            if (lookup.AllowMultiple)
            {
                methodBody = $"return All().Where(x => {comparisonLogic});";
            }
            else
            {
                methodBody = $"return All().FirstOrDefault(x => {comparisonLogic}) ?? _empty;";
            }
        }

        var methodBuilder = new MethodBuilder()
            .WithName(lookup.LookupMethodName)
            .WithReturnType(returnType)
            .WithAccessModifier("public")
            .WithParameter(lookup.PropertyType, "value")
            .WithXmlDoc($"Gets enum value(s) by {lookup.PropertyName}.")
            .WithParamDoc("value", $"The {lookup.PropertyName} value to search for.")
            .WithReturnDoc($"{(lookup.AllowMultiple ? "All enum values" : "The enum value")} with the specified {lookup.PropertyName}.")
            .WithBody(methodBody);

        if (isStatic)
        {
            methodBuilder.AsStatic();
        }

        _classBuilder!.WithMethod(methodBuilder);
    }
#pragma warning restore MA0051 // Method is too long

    /// <summary>
    /// Builds a TryGet lookup method for a specific property.
    /// </summary>
    /// <param name="lookup">The property lookup information.</param>
    /// <param name="isStatic">Whether the method should be static.</param>
    private void BuildTryGetByPropertyMethod(PropertyLookupInfoModel lookup, bool isStatic)
    {
        var comparisonLogic = GenerateComparisonLogic(lookup);
        var methodName = $"TryGet{lookup.LookupMethodName.Substring(2)}";
        
        var methodBody = $"result = All().FirstOrDefault(x => {comparisonLogic});\nreturn result != null;";

        var methodBuilder = new MethodBuilder()
            .WithName(methodName)
            .WithReturnType("bool")
            .WithAccessModifier("public")
            .WithParameter(lookup.PropertyType, "value")
            .WithParameter($"out {_returnType}?", "result")
            .WithXmlDoc($"Attempts to find an enum value by {lookup.PropertyName}.")
            .WithParamDoc("value", $"The {lookup.PropertyName} value to search for.")
            .WithParamDoc("result", "When this method returns, contains the enum value if found; otherwise, null.")
            .WithReturnDoc("true if an enum value was found; otherwise, false.")
            .WithBody(methodBody);

        if (isStatic)
        {
            methodBuilder.AsStatic();
        }

        _classBuilder!.WithMethod(methodBuilder);
    }

    /// <summary>
    /// Builds the default ByName lookup method.
    /// </summary>
    /// <param name="isStatic">Whether the method should be static.</param>
    private void BuildByNameMethod(bool isStatic)
    {
        var comparison = _definition!.NameComparison == StringComparison.Ordinal ? 
            "string.Equals(x.Name, name, StringComparison.Ordinal)" :
            "string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase)";

        var methodBody = $"return All().FirstOrDefault(x => {comparison}) ?? throw new ArgumentException($\"No enum value found with name: {{name}}\");";

        var methodBuilder = new MethodBuilder()
            .WithName("ByName")
            .WithReturnType(_returnType!)
            .WithAccessModifier("public")
            .WithParameter("string", "name")
            .WithXmlDoc("Gets an enum value by its name.")
            .WithParamDoc("name", "The name of the enum value to find.")
            .WithReturnDoc("The enum value with the specified name.")
            .WithBody(methodBody);

        if (isStatic)
        {
            methodBuilder.AsStatic();
        }

        _classBuilder!.WithMethod(methodBuilder);
    }

    /// <summary>
    /// Builds the default TryGetByName lookup method.
    /// </summary>
    /// <param name="isStatic">Whether the method should be static.</param>
    private void BuildTryGetByNameMethod(bool isStatic)
    {
        var comparison = _definition!.NameComparison == StringComparison.Ordinal ? 
            "string.Equals(x.Name, name, StringComparison.Ordinal)" :
            "string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase)";

        var methodBody = $"result = All().FirstOrDefault(x => {comparison});\nreturn result != null;";

        var methodBuilder = new MethodBuilder()
            .WithName("TryGetByName")
            .WithReturnType("bool")
            .WithAccessModifier("public")
            .WithParameter("string", "name")
            .WithParameter($"out {_returnType}?", "result")
            .WithXmlDoc("Attempts to find an enum value by its name.")
            .WithParamDoc("name", "The name of the enum value to find.")
            .WithParamDoc("result", "When this method returns, contains the enum value if found; otherwise, null.")
            .WithReturnDoc("true if an enum value was found; otherwise, false.")
            .WithBody(methodBody);

        if (isStatic)
        {
            methodBuilder.AsStatic();
        }

        _classBuilder!.WithMethod(methodBuilder);
    }

    /// <summary>
    /// Generates comparison logic for property lookups.
    /// </summary>
    /// <param name="lookup">The property lookup information.</param>
    /// <returns>The comparison logic as a string.</returns>
    private static string GenerateComparisonLogic(PropertyLookupInfoModel lookup)
    {
        if (string.Equals(lookup.PropertyType, "string", StringComparison.Ordinal))
        {
            return lookup.StringComparison == StringComparison.Ordinal
                ? $"string.Equals(x.{lookup.PropertyName}, value, StringComparison.Ordinal)"
                : $"string.Equals(x.{lookup.PropertyName}, value, StringComparison.OrdinalIgnoreCase)";
        }

        if (!string.IsNullOrEmpty(lookup.Comparer))
        {
            return $"{lookup.Comparer}.Equals(x.{lookup.PropertyName}, value)";
        }

        return $"EqualityComparer<{lookup.PropertyType}>.Default.Equals(x.{lookup.PropertyName}, value)";
    }
    
    /// <summary>
    /// Adds lookup dictionary fields for all properties with EnumLookup attribute.
    /// </summary>
    private void AddLookupDictionaries()
    {
        foreach (var lookup in _definition!.LookupProperties)
        {
            string fieldName = $"_by{lookup.PropertyName}";
            string fieldType;
            
            if (lookup.AllowMultiple)
            {
                // For multiple values, use ILookup
                fieldType = $"ILookup<{lookup.PropertyType}, {_returnType}>";
            }
            else
            {
                // For single values, use IReadOnlyDictionary
#pragma warning disable MA0026 // TODO
                // TODO: When we add preprocessor support, use FrozenDictionary for .NET 8+
#pragma warning restore MA0026
                fieldType = $"IReadOnlyDictionary<{lookup.PropertyType}, {_returnType}>";
            }
            
            var dictionaryField = new FieldBuilder()
                .WithName(fieldName)
                .WithType(fieldType)
                .WithAccessModifier("private")
                .AsStatic()
                .AsReadOnly()
                .WithXmlDoc($"Lookup dictionary for {lookup.PropertyName}-based searches.");
            
            _classBuilder!.WithField(dictionaryField);
        }
    }
    
    /// <summary>
    /// Adds static fields (_all and _empty).
    /// </summary>
    private void AddStaticFields()
    {
        if (_definition!.UseSingletonInstances)
        {
            // Singleton mode: store instances
            var allField = new FieldBuilder()
                .WithName("_all")
                .WithType($"ImmutableArray<{_returnType}>")
                .WithAccessModifier("private")
                .AsStatic()
                .WithInitializer("ImmutableArray<" + _returnType + ">.Empty")
                .WithXmlDoc("Static collection of all enum options.");
            
            _classBuilder!.WithField(allField);
        }
        else
        {
            // Factory mode: store all instances like singleton mode for consistency
            var allField = new FieldBuilder()
                .WithName("_all")
                .WithType($"ImmutableArray<{_returnType}>")
                .WithAccessModifier("private")
                .AsStatic()
                .WithInitializer($"ImmutableArray<{_returnType}>.Empty")
                .WithXmlDoc("Static collection of all enum options.");
            
            _classBuilder!.WithField(allField);
        }
        
        // Add _empty field (always needed)
        var emptyField = new FieldBuilder()
            .WithName("_empty")
            .WithType(_returnType!)
            .WithAccessModifier("private")
            .AsStatic()
            .WithInitializer("default!")
            .WithXmlDoc("Static empty instance.");
        
        _classBuilder!.WithField(emptyField);
    }
    
    /// <summary>
    /// Adds default collection members with FrozenDictionary support for TypeCollections.
    /// </summary>
    private void AddDefaultCollectionMembers()
    {
        // Generate the Empty class first
        GenerateEmptyClass();
        
        // Get the empty class name
        var baseTypeName = _definition!.ClassName;
        var emptyClassName = $"Empty{baseTypeName.Replace("Base", "")}";
        
        // Add _all FrozenDictionary<int, TBase> field - primary storage with ID-based lookup and alternate key support
        var allField = new FieldBuilder()
            .WithName("_all")
            .WithType($"FrozenDictionary<int, {_returnType}>")
            .WithAccessModifier("private")
            .AsStatic()
            .AsReadOnly()
            .WithXmlDoc("Static collection of all type options with ID-based primary key and alternate key lookup support.");

        _classBuilder!.WithField(allField);

        
        // Add _empty field using the Empty class
        var emptyField = new FieldBuilder()
            .WithName("_empty")
            .WithType(_returnType!)
            .WithAccessModifier("private")
            .AsStatic()
            .AsReadOnly()
            .WithInitializer($"new {emptyClassName}()")
            .WithXmlDoc("Static empty instance with default values.");
        
        _classBuilder!.WithField(emptyField);
        
        // Add All() method - returns FrozenDictionary values as readonly collection
        var allMethod = new MethodBuilder()
            .WithName("All")
            .WithReturnType($"IReadOnlyCollection<{_returnType}>")
            .WithAccessModifier("public")
            .AsStatic()
            .WithXmlDoc("Gets all type options in the collection.")
            .WithReturnDoc("A read-only collection containing all type options.")
            .WithExpressionBody("_all.Values");

        _classBuilder!.WithMethod(allMethod);

        // Add dynamic lookup methods based on [TypeLookup] attributes
        AddDynamicLookupMethods();
        
        // Add Empty() method
        var emptyMethod = new MethodBuilder()
            .WithName("Empty")
            .WithReturnType(_returnType!)
            .WithAccessModifier("public")
            .AsStatic()
            .WithXmlDoc("Gets an empty instance with default values.")
            .WithReturnDoc("An empty instance with default values.")
            .WithExpressionBody("_empty");
        
        _classBuilder!.WithMethod(emptyMethod);
        
        // Add dynamic lookup methods based on [TypeLookup] attributes
        AddDynamicLookupMethods();
        
        // Generate static access members (properties or methods) for each discovered type
        // Rules:
        // 1. If ONLY parameterless constructor exists -> generate PROPERTY
        // 2. If ANY parameterized constructors exist -> generate METHOD(S) only
        // 3. Abstract/static types -> generate PROPERTY that returns _empty

        foreach (var value in _values!.Where(v => v.Include))
        {
            bool hasParameterlessConstructor = value.Constructors.Any(c => c.Parameters.Count == 0);
            bool hasParameterizedConstructors = value.Constructors.Any(c => c.Parameters.Count > 0);

            // Determine whether to generate property or method
            bool shouldGenerateProperty = false;
            bool shouldGenerateMethods = false;

            if (value.IsAbstract || value.IsStatic)
            {
                // Abstract/static types always get a property returning _empty
                shouldGenerateProperty = true;
            }
            else if (hasParameterizedConstructors)
            {
                // If there are ANY parameterized constructors, use methods only
                shouldGenerateMethods = true;
            }
            else if (hasParameterlessConstructor)
            {
                // Only parameterless constructor exists - use property
                shouldGenerateProperty = true;
            }
            else if (!value.Constructors.Any())
            {
                // No constructors defined (using default) - use property
                shouldGenerateProperty = true;
            }

            // Generate property if applicable
            if (shouldGenerateProperty)
            {
                GenerateTypeProperty(value);
            }

            // Generate methods if applicable
            if (shouldGenerateMethods)
            {
                GenerateTypeStaticMethods(value);
            }
        }
        
        // Add static constructor to initialize the FrozenDictionary with ID-based primary key
        var constructorBody = new StringBuilder();

        constructorBody.AppendLine("        var dictionary = new Dictionary<int, " + _returnType + ">();");
        constructorBody.AppendLine();

        // Create instances and add to dictionary using ID as key (only for concrete types)
        foreach (var value in _values.Where(v => v.Include))
        {
            if (!value.IsAbstract && !value.IsStatic)
            {
                // Concrete type - create instance and add to dictionary
                var initializer = GenerateValueInitializer(value);
                var varName = value.Name.ToLower(System.Globalization.CultureInfo.InvariantCulture);

                // Use BaseConstructorId if available, otherwise get from instance
                if (value.BaseConstructorId.HasValue)
                {
                    constructorBody.AppendLine($"        var {varName} = {initializer};");
                    constructorBody.AppendLine($"        dictionary.Add({value.BaseConstructorId.Value}, {varName});");
                }
                else
                {
                    // Fallback: get ID from instance property
                    constructorBody.AppendLine($"        var {varName} = {initializer};");
                    constructorBody.AppendLine($"        dictionary.Add({varName}.Id, {varName});");
                }
                constructorBody.AppendLine();
            }
            else
            {
                // Abstract or static type - include in collection but don't instantiate
                constructorBody.AppendLine($"        // {value.Name} is {(value.IsAbstract ? "abstract" : "static")} - included in collection but not instantiated");
            }
        }

        // Initialize primary FrozenDictionary with ID-based key and alternate key lookup support
        constructorBody.AppendLine("        _all = dictionary.ToFrozenDictionary();");

        // Use the collection name directly for the constructor
        var generatedClassName = _definition!.CollectionName;
            
        var constructor = new ConstructorBuilder()
            .WithClassName(generatedClassName)
            .AsStatic()
            .WithBody(constructorBody.ToString())
            .WithXmlDoc("Initializes the static collection with all type options.");
        
        _classBuilder!.WithConstructor(constructor);
    }
    
    /// <summary>
    /// Generates a static property for a type that has no parameterized constructors.
    /// </summary>
    private void GenerateTypeProperty(EnumValueInfoModel value)
    {
        string expressionBody;
        string xmlDoc;

        if (value.IsAbstract || value.IsStatic)
        {
            expressionBody = "_empty";
            xmlDoc = $"Gets the {value.Name} type option. Returns empty instance since type is {(value.IsAbstract ? "abstract" : "static")}.";
        }
        else if (value.BaseConstructorId.HasValue)
        {
            expressionBody = $"_all.TryGetValue({value.BaseConstructorId.Value}, out var result) ? result : _empty";
            xmlDoc = $"Gets the {value.Name} type option from the collection using ID lookup.";
        }
        else
        {
            expressionBody = "_all.TryGetValue(0, out var result) ? result : _empty";
            xmlDoc = $"Gets the {value.Name} type option from the collection using ID lookup.";
        }

        var typeProperty = new PropertyBuilder()
            .WithName(value.Name)
            .WithType(_returnType!)
            .WithAccessModifier("public")
            .AsStatic()
            .WithXmlDoc(xmlDoc)
            .WithExpressionBody(expressionBody);

        _classBuilder!.WithProperty(typeProperty);
    }

    /// <summary>
    /// Generates static methods for a discovered type, creating overloads for each constructor.
    /// </summary>
    private void GenerateTypeStaticMethods(EnumValueInfoModel value)
    {
        // Get all constructors
        var hasParameterlessConstructor = value.Constructors.Any(c => c.Parameters.Count == 0);
        var constructorsWithParams = value.Constructors.Where(c => c.Parameters.Count > 0).ToList();

        // If only parameterless constructor exists, it's handled by property generation
        if (constructorsWithParams.Count == 0 && hasParameterlessConstructor)
        {
            return;
        }

        // Generate a method for the parameterless constructor if it exists alongside parameterized ones
        if (hasParameterlessConstructor && constructorsWithParams.Count > 0)
        {
            var methodBuilder = new MethodBuilder()
                .WithName(value.Name)
                .WithReturnType(_returnType!)
                .WithAccessModifier("public")
                .AsStatic()
                .WithXmlDoc($"Gets the {value.Name} type option from the collection.");

            // Use the extracted ID for lookup if available
            if (value.BaseConstructorId.HasValue)
            {
                methodBuilder.WithExpressionBody($"_all.TryGetValue({value.BaseConstructorId.Value}, out var result) ? result : _empty");
            }
            else
            {
                methodBuilder.WithExpressionBody("_all.TryGetValue(0, out var result) ? result : _empty");
            }

            _classBuilder!.WithMethod(methodBuilder);
        }

        // Generate a method for each constructor that has parameters
        for (int constructorIndex = 0; constructorIndex < constructorsWithParams.Count; constructorIndex++)
        {
            var constructor = constructorsWithParams[constructorIndex];
            
            // Method name: use type name without "Create" prefix, add index if multiple constructors
            var methodName = constructorsWithParams.Count == 1 
                ? value.Name 
                : $"{value.Name}{constructorIndex + 1}";
            
            var methodBuilder = new MethodBuilder()
                .WithName(methodName)
                .WithReturnType(_returnType!)
                .WithAccessModifier("public")
                .AsStatic();

            // Add parameters from constructor
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

            // Build method body
            var arguments = string.Join(", ", argumentList);
            methodBuilder.WithExpressionBody($"new {value.ShortTypeName}({arguments})");

            // Build XML documentation
            var xmlDoc = $"Gets a new instance of the {value.Name} type option";
            if (constructor.Parameters.Count > 0)
            {
                xmlDoc += $" with {constructor.Parameters.Count} parameter{(constructor.Parameters.Count > 1 ? "s" : "")}";
            }
            xmlDoc += ".";

            methodBuilder.WithXmlDoc(xmlDoc)
                         .WithReturnDoc($"A new instance of the {value.Name} type option.");

            _classBuilder!.WithMethod(methodBuilder);
        }
    }
    
    /// <summary>
    /// Generates an Empty class that inherits from the base type with default constructor values.
    /// </summary>
    private void GenerateEmptyClass()
    {
        if (_definition == null || _values == null || _values.Count == 0)
            return;
            
        // Get the base type name (e.g., "DataStoreTypeBase" from full type name)
        var baseTypeName = _definition.ClassName;  // This should be like "DataStoreTypeBase"
        var emptyClassName = $"Empty{baseTypeName.Replace("Base", "")}"; // "EmptyDataStoreType"
        
        // Create the Empty class as nested class (no namespace)
        var emptyClassBuilder = new ClassBuilder()
            .WithName(emptyClassName)
            .WithAccessModifier("internal")
            .AsSealed()
            .WithBaseClass(baseTypeName)
            .WithXmlDoc($"Empty implementation of {baseTypeName} with default values for all properties.");
        
        // For TypeCollections, we need to call the base class constructor with proper parameters
        // The discovered types inherit from the base class, so we need to look at the base class constructor
        var constructorBuilder = new ConstructorBuilder()
            .WithClassName(emptyClassName)
            .WithAccessModifier("internal")
            .WithXmlDoc("Initializes a new instance of the Empty class with default values.");

        // Determine base constructor parameters by looking at the base type
        var baseTypeFullName = $"{_definition.Namespace}.{baseTypeName}";
        var baseTypeSymbol = _compilation?.GetTypeByMetadataName(baseTypeFullName);

        if (baseTypeSymbol != null)
        {
            // Find the base constructor with minimum parameters
            var baseConstructor = baseTypeSymbol.Constructors
                .Where(c => c.DeclaredAccessibility == Microsoft.CodeAnalysis.Accessibility.Protected ||
                            c.DeclaredAccessibility == Microsoft.CodeAnalysis.Accessibility.Public)
                .OrderBy(c => c.Parameters.Length)
                .FirstOrDefault();

            if (baseConstructor != null)
            {
                // Generate base call with appropriate default values
                var baseCallArgs = new List<string>();
                foreach (var param in baseConstructor.Parameters)
                {
                    var defaultValue = GetDefaultValueForType(param.Type);
                    baseCallArgs.Add(defaultValue);
                }
                constructorBuilder.WithBaseCall(baseCallArgs.ToArray());
            }
            else
            {
                // Fallback to standard defaults
                constructorBuilder.WithBaseCall("0", "string.Empty");
            }
        }
        else
        {
            // Fallback to standard defaults for EnumOptionBase
            constructorBuilder.WithBaseCall("0", "string.Empty");
        }
            
        emptyClassBuilder.WithConstructor(constructorBuilder);

        // Implement abstract methods with default/empty implementations
        AddAbstractMethodImplementations(emptyClassBuilder, baseTypeName);

        // Generate the Empty class as a separate file
        var emptyClassCode = emptyClassBuilder.Build();

        // We need to generate this as a separate source file
        // For now, let's add it to the existing class builder as a nested class
        _classBuilder!.WithNestedClass(emptyClassBuilder);
    }

    /// <summary>
    /// Adds implementations for abstract methods in the base class with appropriate default values.
    /// </summary>
    private void AddAbstractMethodImplementations(IClassBuilder classBuilder, string baseTypeName)
    {
        if (_compilation == null || _definition == null) return;

        // Get the base type symbol to discover abstract methods
        var baseTypeFullName = $"{_definition.Namespace}.{baseTypeName}";
        var baseTypeSymbol = _compilation.GetTypeByMetadataName(baseTypeFullName);
        if (baseTypeSymbol == null) return;

        // Find all abstract methods in the inheritance chain
        var abstractMethods = GetAbstractMethods(baseTypeSymbol);

        foreach (var method in abstractMethods)
        {
            var methodBuilder = new MethodBuilder()
                .WithName(method.Name)
                .WithReturnType(method.ReturnType.ToDisplayString())
                .WithAccessModifier("public")
                .AsOverride()
                .WithXmlDoc($"Empty implementation of {method.Name}.");

            // Add parameters
            foreach (var param in method.Parameters)
            {
                methodBuilder.WithParameter(param.Type.ToDisplayString(), param.Name);
            }

            // Generate appropriate return value based on return type
            if (method.ReturnsVoid)
            {
                // Void method - empty body
                methodBuilder.WithBody("");
            }
            else if (method.ReturnType.SpecialType == Microsoft.CodeAnalysis.SpecialType.System_Boolean)
            {
                methodBuilder.WithExpressionBody("false");
            }
            else if (method.ReturnType.SpecialType == Microsoft.CodeAnalysis.SpecialType.System_String)
            {
                methodBuilder.WithExpressionBody("string.Empty");
            }
            else if (method.ReturnType.IsReferenceType ||
                     method.ReturnType.NullableAnnotation == Microsoft.CodeAnalysis.NullableAnnotation.Annotated)
            {
                methodBuilder.WithExpressionBody("null!");
            }
            else if (method.ReturnType.IsValueType)
            {
                methodBuilder.WithExpressionBody("default");
            }
            else
            {
                methodBuilder.WithExpressionBody("default!");
            }

            classBuilder.WithMethod(methodBuilder);
        }
    }
    
    /// <summary>
    /// Gets the default value for a given type symbol.
    /// </summary>
    private static string GetDefaultValueForType(ITypeSymbol typeSymbol)
    {
        // Check for special known types
        if (typeSymbol.SpecialType == Microsoft.CodeAnalysis.SpecialType.System_String)
            return "string.Empty";
        if (typeSymbol.SpecialType == Microsoft.CodeAnalysis.SpecialType.System_Int32)
            return "0";
        if (typeSymbol.SpecialType == Microsoft.CodeAnalysis.SpecialType.System_Boolean)
            return "false";

        // Check if nullable
        if (typeSymbol.NullableAnnotation == Microsoft.CodeAnalysis.NullableAnnotation.Annotated ||
            typeSymbol.IsReferenceType)
            return "null";

        // Default for value types
        return "default";
    }

    /// <summary>
    /// Gets all abstract methods from a type and its base types.
    /// </summary>
    private static List<IMethodSymbol> GetAbstractMethods(ITypeSymbol typeSymbol)
    {
        var abstractMethods = new List<IMethodSymbol>();
        var current = typeSymbol;
        
        while (current != null)
        {
            foreach (var member in current.GetMembers().OfType<IMethodSymbol>())
            {
                // Only include abstract methods (not constructors, properties, etc.)
                if (member.IsAbstract && member.MethodKind == Microsoft.CodeAnalysis.MethodKind.Ordinary)
                {
                    abstractMethods.Add(member);
                }
            }
            current = current.BaseType;
        }

        return abstractMethods;
    }
    
    /// <summary>
    /// Gets an appropriate default value for a property based on its type and name.
    /// </summary>
    private static string GetDefaultValueForProperty(string typeName, string propertyName)
    {
        // Handle specific property names with known defaults
        return propertyName.ToLowerInvariant() switch
        {
            "sqloperator" => "string.Empty",
            "sqlkeyword" => "string.Empty", 
            "issinglevalue" => "true",
            "isascending" => "true",
            "precedence" => "0",
            _ => GetDefaultValueForType(typeName)
        };
    }
    
    /// <summary>
    /// Gets the default value for a given type name.
    /// </summary>
    private static string GetDefaultValueForType(string typeName)
    {
        return typeName.ToLowerInvariant() switch
        {
            "string" => "string.Empty",
            "string?" => "string.Empty",
            "int" => "0",
            "long" => "0L", 
            "float" => "0f",
            "double" => "0d",
            "decimal" => "0m",
            "bool" => "false",
            "byte" => "0",
            "short" => "0",
            "uint" => "0u",
            "ulong" => "0ul",
            "ushort" => "0",
            "char" => "'\\0'",
            "guid" => "Guid.Empty",
            "datetime" => "DateTime.MinValue",
            "datetimeoffset" => "DateTimeOffset.MinValue",
            "timespan" => "TimeSpan.Zero",
            _ when typeName.EndsWith("?", StringComparison.Ordinal) => "null",
            _ when typeName.Contains("Dictionary") || typeName.Contains("IDictionary") => "new Dictionary<string, object>()",
            _ => "default"
        };
    }
    
    /// <summary>
    /// Adds the static constructor to initialize the collection.
    /// </summary>
#pragma warning disable MA0051 // Method is too long
    private void AddStaticConstructor()
    {
        var constructorBody = new StringBuilder();
        
        if (_definition!.UseSingletonInstances)
        {
            // Singleton mode: initialize instances and dictionaries
            constructorBody.AppendLine($"var values = new {_returnType}[]");
            constructorBody.AppendLine("{");
            
            foreach (var value in _values!.Where(v => v.Include))
            {
                constructorBody.AppendLine($"    {value.Name},");
            }
            
            constructorBody.AppendLine("};");
            constructorBody.AppendLine("_all = values.ToImmutableArray();");
            
            // Initialize lookup dictionaries
            foreach (var lookup in _definition.LookupProperties)
            {
                var fieldName = $"_by{lookup.PropertyName}";
                
                if (lookup.AllowMultiple)
                {
                    // Use ToLookup for multiple values
                    constructorBody.AppendLine($"{fieldName} = values.ToLookup(x => x.{lookup.PropertyName});");
                }
                else
                {
                    // Use ToDictionary for single values
                    constructorBody.AppendLine($"{fieldName} = values.ToDictionary(x => x.{lookup.PropertyName});");
                }
            }
        }
        else
        {
            // Factory mode: initialize instances like singleton mode for consistency with _all field
            constructorBody.AppendLine($"var values = new {_returnType}[]");
            constructorBody.AppendLine("{");
            
            foreach (var value in _values!.Where(v => v.Include))
            {
                constructorBody.AppendLine($"    new {value.ShortTypeName}(),");
            }
            
            constructorBody.AppendLine("};");
            constructorBody.AppendLine("_all = values.ToImmutableArray();");
            
            // For factory mode, initialize lookup dictionaries the same way as singleton mode
            foreach (var lookup in _definition!.LookupProperties)
            {
                var fieldName = $"_by{lookup.PropertyName}";
                
                if (lookup.AllowMultiple)
                {
                    constructorBody.AppendLine($"{fieldName} = values.ToLookup(x => x.{lookup.PropertyName});");
                }
                else
                {
                    constructorBody.AppendLine($"{fieldName} = values.ToDictionary(x => x.{lookup.PropertyName});");
                }
            }
        }
        
        // Initialize empty instance
        var emptyValue = _values!.FirstOrDefault(v => v.Name.Contains("Empty", StringComparison.OrdinalIgnoreCase));
        if (emptyValue != null)
        {
            constructorBody.AppendLine($"_empty = {emptyValue.Name};");
        }
        else
        {
            constructorBody.AppendLine("_empty = default!;");
        }
        
        var constructor = new ConstructorBuilder()
            .WithClassName(_definition!.CollectionName)
            .AsStatic()
            .WithBody(constructorBody.ToString());
        
        _classBuilder!.WithConstructor(constructor);
    }
#pragma warning restore MA0051 // Method is too long
    
    /// <summary>
    /// Adds a static constructor for classes that inherit from EnumCollectionBase.
    /// </summary>
    private void AddStaticConstructorForInheritance()
    {
        var constructorBody = new StringBuilder();
        constructorBody.AppendLine($"var values = new {_returnType}[]");
        constructorBody.AppendLine("{");
        
        foreach (var value in _values!.Where(v => v.Include))
        {
            constructorBody.AppendLine($"    {value.Name},");
        }
        
        constructorBody.AppendLine("};");
        constructorBody.AppendLine("_all = values.ToImmutableArray();");
        
        // Check if there's an empty value defined
        var emptyValue = _values.FirstOrDefault(v => v.Name.Contains("Empty", StringComparison.OrdinalIgnoreCase));
        if (emptyValue != null)
        {
            constructorBody.AppendLine($"_empty = {emptyValue.Name};");
        }
        else
        {
            constructorBody.AppendLine($"_empty = default!;");
        }
        
        var constructor = new ConstructorBuilder()
            .WithClassName(_definition!.CollectionName)
            .AsStatic()
            .WithBody(constructorBody.ToString());
        
        _classBuilder!.WithConstructor(constructor);
    }
    
    /// <summary>
    /// Adds the Empty() method.
    /// </summary>
    private void AddEmptyMethod()
    {
        // Check if there's an empty value in the collection
        var emptyValue = _values?.FirstOrDefault(v => v.Name.Contains("Empty", StringComparison.OrdinalIgnoreCase));
        var emptyExpression = emptyValue != null ? emptyValue.Name : "default!";
        
        var method = new MethodBuilder()
            .WithName("Empty")
            .WithReturnType(_returnType!)
            .WithAccessModifier("public")
            .AsStatic()
            .WithXmlDoc("Gets an empty instance of the enum option type.")
            .WithExpressionBody(emptyExpression);
        
        _classBuilder!.WithMethod(method);
    }
    
    /// <summary>
    /// Adds the GetByName() method.
    /// </summary>
    private void AddGetByNameMethod()
    {
        var method = new MethodBuilder()
            .WithName("GetByName")
            .WithReturnType($"{_returnType}?")
            .WithAccessModifier("public")
            .AsStatic()
            .WithParameter("string", "name")
            .WithXmlDoc("Gets an enum option by name (case-insensitive).")
            .WithBody(@"if (string.IsNullOrWhiteSpace(name)) return null;
return All().FirstOrDefault(x => string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase));");
        
        _classBuilder!.WithMethod(method);
    }
    
    /// <summary>
    /// Adds the GetById() method.
    /// </summary>
    private void AddGetByIdMethod()
    {
        var method = new MethodBuilder()
            .WithName("GetById")
            .WithReturnType($"{_returnType}?")
            .WithAccessModifier("public")
            .AsStatic()
            .WithParameter("int", "id")
            .WithXmlDoc("Gets an enum option by ID.")
            .WithBody("return All().FirstOrDefault(x => x.Id == id);");
        
        _classBuilder!.WithMethod(method);
    }
    
    /// <summary>
    /// Adds the TryGetByName() method.
    /// </summary>
    private void AddTryGetByNameMethod()
    {
        var method = new MethodBuilder()
            .WithName("TryGetByName")
            .WithReturnType("bool")
            .WithAccessModifier("public")
            .AsStatic()
            .WithParameter("string", "name")
            .WithParameter($"out {_returnType}?", "value")
            .WithXmlDoc("Tries to get an enum option by name.")
            .WithBody(@"value = GetByName(name);
return value != null;");
        
        _classBuilder!.WithMethod(method);
    }
    
    /// <summary>
    /// Adds the TryGetById() method.
    /// </summary>
    private void AddTryGetByIdMethod()
    {
        var method = new MethodBuilder()
            .WithName("TryGetById")
            .WithReturnType("bool")
            .WithAccessModifier("public")
            .AsStatic()
            .WithParameter("int", "id")
            .WithParameter($"out {_returnType}?", "value")
            .WithXmlDoc("Tries to get an enum option by ID.")
            .WithBody(@"value = GetById(id);
return value != null;");
        
        _classBuilder!.WithMethod(method);
    }
    
    /// <summary>
    /// Adds the AsEnumerable() method.
    /// </summary>
    private void AddAsEnumerableMethod()
    {
        var method = new MethodBuilder()
            .WithName("AsEnumerable")
            .WithReturnType($"IEnumerable<{_returnType}>")
            .WithAccessModifier("public")
            .AsStatic()
            .WithXmlDoc("Gets all enum options as an enumerable.")
            .WithBody("return All();");
        
        _classBuilder!.WithMethod(method);
    }
    
    /// <summary>
    /// Adds the Any() method.
    /// </summary>
    private void AddAnyMethod()
    {
        var method = new MethodBuilder()
            .WithName("Any")
            .WithReturnType("bool")
            .WithAccessModifier("public")
            .AsStatic()
            .WithXmlDoc("Checks if the collection contains any items.")
            .WithBody("return _all.Length > 0;");
        
        _classBuilder!.WithMethod(method);
    }
    
    /// <summary>
    /// Adds the GetAll() method that wraps All() for backward compatibility.
    /// </summary>
    private void AddGetAllMethod()
    {
        var method = new MethodBuilder()
            .WithName("GetAll")
            .WithReturnType($"ImmutableArray<{_returnType}>")
            .WithAccessModifier("public")
            .AsStatic()
            .WithXmlDoc("Gets all enum values in the collection.")
            .WithBody("return All();");
        
        _classBuilder!.WithMethod(method);
    }
    
    /// <summary>
    /// Adds the GetByIndex() method.
    /// </summary>
    private void AddGetByIndexMethod()
    {
        var method = new MethodBuilder()
            .WithName("GetByIndex")
            .WithReturnType(_returnType!)
            .WithAccessModifier("public")
            .AsStatic()
            .WithParameter("int", "index")
            .WithXmlDoc("Gets an enum option by index.")
            .WithBody("return _all[index];");
        
        _classBuilder!.WithMethod(method);
    }

    /// <summary>
    /// Determines if the definition represents a TypeCollection by checking actual type inheritance.
    /// </summary>
    private bool IsTypeCollection()
    {
        if (_definition == null || _compilation == null || !_definition.InheritsFromCollectionBase)
            return false;

        // Get the TypeCollectionBase generic types using typeof
        var typeCollectionBaseSingle = _compilation.GetTypeByMetadataName(typeof(FractalDataWorks.Collections.TypeCollectionBase<>).FullName!.Replace("+", "."));
        var typeCollectionBaseDouble = _compilation.GetTypeByMetadataName(typeof(FractalDataWorks.Collections.TypeCollectionBase<,>).FullName!.Replace("+", "."));

        if (typeCollectionBaseSingle == null && typeCollectionBaseDouble == null)
            return false;

        // Try to resolve the collection class to check its base type
        var collectionType = _compilation.GetTypeByMetadataName(_definition.Namespace + "." + _definition.CollectionName);
        if (collectionType == null)
            return false;

        // Check inheritance chain
        var currentType = collectionType.BaseType;
        while (currentType != null)
        {
            if (currentType is INamedTypeSymbol { IsGenericType: true } namedBase)
            {
                var constructedFrom = namedBase.ConstructedFrom;
                
                if (typeCollectionBaseSingle != null && SymbolEqualityComparer.Default.Equals(constructedFrom, typeCollectionBaseSingle))
                    return true;
                    
                if (typeCollectionBaseDouble != null && SymbolEqualityComparer.Default.Equals(constructedFrom, typeCollectionBaseDouble))
                    return true;
            }

            currentType = currentType.BaseType;
        }

        return false;
    }
    
    /// <summary>
    /// Copies static members from the base TypeCollection class to the generated class.
    /// </summary>
    private void CopyStaticMembersFromBaseClass(ClassBuilder classBuilder, string generatedClassName)
    {
        // Get the original base class (e.g., DataStoreTypesBase)
        if (_definition?.Namespace == null || _definition?.CollectionName == null || _compilation == null) return;
        var baseClassType = _compilation.GetTypeByMetadataName(_definition.Namespace + "." + _definition.CollectionName);
        if (baseClassType == null) return;

        // Skip methods and properties that will be generated by AddDefaultCollectionMembers
        var skipMethods = new HashSet<string>(StringComparer.Ordinal) { "All", "Empty", "Name", "Id" };
        
        // Copy all static members (methods, properties, fields) from the base class
        foreach (var member in baseClassType.GetMembers().Where(m => m.IsStatic))
        {
            switch (member)
            {
                case IMethodSymbol method when method.MethodKind == MethodKind.Ordinary:
                    if (!skipMethods.Contains(method.Name))
                        CopyStaticMethod(classBuilder, method);
                    break;
                    
                case IPropertySymbol property:
                    CopyStaticProperty(classBuilder, property);
                    break;
                    
                case IFieldSymbol field:
                    CopyStaticField(classBuilder, field);
                    break;
            }
        }
    }
    
    /// <summary>
    /// Copies a static method from the base class.
    /// </summary>
    private void CopyStaticMethod(ClassBuilder classBuilder, IMethodSymbol method)
    {
        // Skip compiler-generated methods
        if (method.IsImplicitlyDeclared) return;
        
        var methodBuilder = new MethodBuilder()
            .WithName(method.Name)
            .WithReturnType(method.ReturnType.ToDisplayString())
            .WithAccessModifier(GetAccessModifierString(method.DeclaredAccessibility))
            .AsStatic();
            
        // Add parameters
        foreach (var param in method.Parameters)
        {
            methodBuilder.WithParameter(param.Type.ToDisplayString(), param.Name);
        }
        
        // Copy XML documentation if available
        var xmlDoc = method.GetDocumentationCommentXml();
        if (!string.IsNullOrEmpty(xmlDoc))
        {
            // Extract summary from XML doc using manual string parsing to avoid DoS vulnerabilities
            var summaryStart = xmlDoc!.IndexOf("<summary>", StringComparison.Ordinal);
            if (summaryStart >= 0)
            {
                summaryStart += "<summary>".Length;
                var summaryEnd = xmlDoc!.IndexOf("</summary>", summaryStart, StringComparison.Ordinal);
                if (summaryEnd > summaryStart)
                {
                    var summary = xmlDoc!.Substring(summaryStart, summaryEnd - summaryStart).Trim();
                    methodBuilder.WithXmlDoc(summary);
                }
            }
        }
        
        // For now, generate a placeholder body that delegates to the base class
        var parameters = string.Join(", ", method.Parameters.Select(p => p.Name));
        methodBuilder.WithExpressionBody($"{_definition!.CollectionName}.{method.Name}({parameters})");
        
        classBuilder.WithMethod(methodBuilder);
    }
    
    /// <summary>
    /// Copies a static property from the base class.
    /// </summary>
    private void CopyStaticProperty(ClassBuilder classBuilder, IPropertySymbol property)
    {
        // Skip compiler-generated properties
        if (property.IsImplicitlyDeclared) return;
        
        var propertyBuilder = new PropertyBuilder()
            .WithName(property.Name)
            .WithType(property.Type.ToDisplayString())
            .WithAccessModifier(GetAccessModifierString(property.DeclaredAccessibility))
            .AsStatic();
            
        // Copy XML documentation if available
        var xmlDoc = property.GetDocumentationCommentXml();
        if (!string.IsNullOrEmpty(xmlDoc))
        {
            // Extract summary from XML doc using manual string parsing to avoid DoS vulnerabilities
            var summaryStart = xmlDoc!.IndexOf("<summary>", StringComparison.Ordinal);
            if (summaryStart >= 0)
            {
                summaryStart += "<summary>".Length;
                var summaryEnd = xmlDoc!.IndexOf("</summary>", summaryStart, StringComparison.Ordinal);
                if (summaryEnd > summaryStart)
                {
                    var summary = xmlDoc!.Substring(summaryStart, summaryEnd - summaryStart).Trim();
                    propertyBuilder.WithXmlDoc(summary);
                }
            }
        }
        
        // Delegate to base class
        propertyBuilder.WithExpressionBody($"{_definition!.CollectionName}.{property.Name}");
        
        classBuilder.WithProperty(propertyBuilder);
    }
    
    /// <summary>
    /// Copies a static field from the base class.
    /// </summary>
    private static void CopyStaticField(ClassBuilder classBuilder, IFieldSymbol field)
    {
        // Skip compiler-generated fields and const fields
        if (field.IsImplicitlyDeclared || field.IsConst) return;
        
        var fieldBuilder = new FieldBuilder()
            .WithName(field.Name)
            .WithType(field.Type.ToDisplayString())
            .WithAccessModifier(GetAccessModifierString(field.DeclaredAccessibility))
            .AsStatic();
            
        if (field.IsReadOnly)
        {
            fieldBuilder.AsReadOnly();
        }
        
        // Copy XML documentation if available
        var xmlDoc = field.GetDocumentationCommentXml();
        if (!string.IsNullOrEmpty(xmlDoc))
        {
            // Extract summary from XML doc using manual string parsing to avoid DoS vulnerabilities
            var summaryStart = xmlDoc!.IndexOf("<summary>", StringComparison.Ordinal);
            if (summaryStart >= 0)
            {
                summaryStart += "<summary>".Length;
                var summaryEnd = xmlDoc!.IndexOf("</summary>", summaryStart, StringComparison.Ordinal);
                if (summaryEnd > summaryStart)
                {
                    var summary = xmlDoc!.Substring(summaryStart, summaryEnd - summaryStart).Trim();
                    fieldBuilder.WithXmlDoc(summary);
                }
            }
        }
        
        classBuilder.WithField(fieldBuilder);
    }
    
    /// <summary>
    /// Converts accessibility to string.
    /// </summary>
    private static string GetAccessModifierString(Accessibility accessibility)
    {
        return accessibility switch
        {
            Accessibility.Public => "public",
            Accessibility.Private => "private",
            Accessibility.Protected => "protected",
            Accessibility.Internal => "internal",
            Accessibility.ProtectedOrInternal => "protected internal",
            Accessibility.ProtectedAndInternal => "private protected",
            _ => "public"
        };
    }

    /// <summary>
    /// Adds dynamic lookup cache fields for all properties marked with [TypeLookup] attributes.
    /// </summary>
    private void AddDynamicLookupFields()
    {
        if (_definition?.LookupProperties == null) return;

        foreach (var lookup in _definition.LookupProperties)
        {
            var fieldName = $"_by{lookup.PropertyName}";
            var fieldType = $"FrozenDictionary<{lookup.PropertyType}, {_returnType}>";

            var field = new FieldBuilder()
                .WithName(fieldName)
                .WithType(fieldType)
                .WithAccessModifier("private")
                .AsStatic()
                .AsReadOnly()
                .WithXmlDoc($"Static lookup cache for {lookup.PropertyName}-based access.");

            _classBuilder!.WithField(field);
        }
    }

    /// <summary>
    /// Adds dynamic lookup methods for all properties marked with [TypeLookup] attributes.
    /// Uses clean method names like Id(int id) instead of GetById(int id).
    /// Uses FrozenDictionary.GetAlternateLookup for efficient alternate key lookups.
    /// </summary>
    private void AddDynamicLookupMethods()
    {
        if (_definition?.LookupProperties == null) return;

        foreach (var lookup in _definition.LookupProperties)
        {
            var methodName = lookup.PropertyName; // Clean name: Id, Name, Category
            var parameterName = lookup.PropertyName.ToLower(System.Globalization.CultureInfo.InvariantCulture); // id, name, category

            string methodBody;
            if (string.Equals(lookup.PropertyType, "int", StringComparison.Ordinal) && string.Equals(lookup.PropertyName, "Id", StringComparison.Ordinal))
            {
                // For ID lookups, use the primary key directly
                methodBody = $"_all.TryGetValue({parameterName}, out var result) ? result : _empty";
            }
            else
            {
                // For alternate key lookups, use GetAlternateLookup<T>()
                methodBody = $@"var alternateLookup = _all.GetAlternateLookup<{lookup.PropertyType}>();
        return alternateLookup.TryGetValue({parameterName}, out var result) ? result : _empty";
            }

            var method = new MethodBuilder()
                .WithName(methodName)
                .WithReturnType(_returnType!)
                .WithAccessModifier("public")
                .AsStatic()
                .WithParameter(lookup.PropertyType, parameterName)
                .WithXmlDoc($"Gets a type option by its {lookup.PropertyName} using {(string.Equals(lookup.PropertyName, "Id", StringComparison.Ordinal) ? "primary key lookup" : "alternate key lookup")}.")
                .WithParamDoc(parameterName, $"The {lookup.PropertyName} value to search for.")
                .WithReturnDoc($"The type option with the specified {lookup.PropertyName}, or empty instance if not found.");

            if (string.Equals(lookup.PropertyType, "int", StringComparison.Ordinal) && string.Equals(lookup.PropertyName, "Id", StringComparison.Ordinal))
            {
                method.WithExpressionBody(methodBody);
            }
            else
            {
                method.WithBody(methodBody);
            }

            _classBuilder!.WithMethod(method);
        }
    }
}
