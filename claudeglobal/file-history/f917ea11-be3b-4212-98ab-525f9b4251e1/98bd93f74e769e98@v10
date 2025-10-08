using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using FractalDataWorks.SourceGenerators.Configuration;
using FractalDataWorks.SourceGenerators.Models;

namespace FractalDataWorks.SourceGenerators.Generators;

/// <summary>
/// Generates Empty classes for collection value types.
/// </summary>
public sealed class EmptyClassGenerator
{
    private readonly CollectionBuilderConfiguration _config;

    /// <summary>
    /// Initializes a new instance of the <see cref="EmptyClassGenerator"/> class.
    /// </summary>
    /// <param name="config">The configuration for collection generation.</param>
    public EmptyClassGenerator(CollectionBuilderConfiguration config)
    {
        _config = config ?? throw new ArgumentNullException(nameof(config));
    }

    /// <summary>
    /// Generates an Empty class for a collection value type.
    /// </summary>
    /// <param name="definition">The type definition</param>
    /// <param name="returnType">The return type (e.g., "AuthenticationTypeBase" or fully qualified)</param>
    /// <param name="namespace">The namespace for the Empty class</param>
    /// <param name="compilation">The compilation context for type resolution</param>
    /// <returns>The generated Empty class code</returns>
    public string GenerateEmptyClass(
        GenericTypeInfoModel definition,
        string returnType,
        string @namespace,
        Compilation compilation)
    {
        // Extract simple type name from potentially fully qualified type
        // E.g., "FractalDataWorks.Services.Connections.Abstractions.IConnectionState" => "IConnectionState"
        var simpleTypeName = returnType.Contains(".")
            ? returnType.Substring(returnType.LastIndexOf('.') + 1)
            : returnType;

        var emptyClassName = $"Empty{simpleTypeName}";
        var sb = new StringBuilder();

        // Add usings
        sb.AppendLine("using System;");
        sb.AppendLine();

        // Namespace
        sb.AppendLine($"namespace {@namespace};");
        sb.AppendLine();

        // XML documentation
        sb.AppendLine("/// <summary>");
        sb.AppendLine($"/// Empty null-object implementation of {simpleTypeName} with default values.");
        sb.AppendLine("/// </summary>");

        // Class declaration
        sb.AppendLine($"public sealed class {emptyClassName} : {returnType}");
        sb.AppendLine("{");

        // Find the base type and its constructor
        // returnType might be simple name like "ConnectionStateBase" or fully qualified
        // Try to resolve it by combining with namespace if it's a simple name
        var fullyQualifiedTypeName = returnType.Contains(".")
            ? returnType
            : $"{@namespace}.{returnType}";

        var baseTypeSymbol = compilation.GetTypeByMetadataName(fullyQualifiedTypeName);
        if (baseTypeSymbol != null)
        {
            // Find the protected or public constructor with minimum parameters
            var baseConstructor = baseTypeSymbol.Constructors
                .Where(c => !c.IsStatic &&
                           (c.DeclaredAccessibility == Accessibility.Protected ||
                            c.DeclaredAccessibility == Accessibility.Public))
                .OrderBy(c => c.Parameters.Length)
                .FirstOrDefault();

            if (baseConstructor != null)
            {
                var baseCallArgs = new List<string>();
                foreach (var param in baseConstructor.Parameters)
                {
                    var defaultValue = GetDefaultValueForTypeSymbol(param.Type);
                    baseCallArgs.Add(defaultValue);
                }

                // Constructor
                sb.AppendLine("    /// <summary>");
                sb.AppendLine($"    /// Initializes a new instance of the <see cref=\"{emptyClassName}\"/> class with default values.");
                sb.AppendLine("    /// </summary>");
                sb.AppendLine($"    public {emptyClassName}()");
                sb.AppendLine($"        : base({string.Join(", ", baseCallArgs)})");
                sb.AppendLine("    {");
                sb.AppendLine("    }");
                sb.AppendLine();
            }

            // Implement all abstract methods
            GenerateAbstractMethodImplementations(sb, baseTypeSymbol, compilation);
        }

        sb.AppendLine("}");

        return sb.ToString();
    }

    /// <summary>
    /// Generates implementations for all abstract methods in the inheritance chain.
    /// </summary>
    private void GenerateAbstractMethodImplementations(StringBuilder sb, INamedTypeSymbol baseType, Compilation compilation)
    {
        var implementedMethods = new HashSet<string>();

        // Walk up the inheritance chain
        var currentType = baseType;
        while (currentType != null && currentType.SpecialType != SpecialType.System_Object)
        {
            foreach (var member in currentType.GetMembers())
            {
                if (member is IMethodSymbol method && method.IsAbstract && !method.IsStatic && method.MethodKind == MethodKind.Ordinary)
                {
                    // Create a unique signature for deduplication
                    var signature = GetMethodSignature(method);
                    if (implementedMethods.Contains(signature))
                        continue;

                    implementedMethods.Add(signature);
                    GenerateAbstractMethodImplementation(sb, method, compilation);
                }
            }

            currentType = currentType.BaseType;
        }
    }

    /// <summary>
    /// Generates implementation for a single abstract method.
    /// </summary>
    private void GenerateAbstractMethodImplementation(StringBuilder sb, IMethodSymbol method, Compilation compilation)
    {
        // XML documentation
        sb.AppendLine("    /// <summary>");
        sb.AppendLine($"    /// Empty implementation of {method.Name}.");
        sb.AppendLine("    /// </summary>");

        // Method signature
        var returnTypeString = method.ReturnType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        var methodName = method.Name;
        var parameters = string.Join(", ", method.Parameters.Select(p =>
            $"{p.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)} {p.Name}"));

        sb.AppendLine($"    public override {returnTypeString} {methodName}({parameters})");
        sb.AppendLine("    {");

        // Generate return statement
        var returnStatement = GenerateReturnStatement(method, compilation);
        if (!string.IsNullOrEmpty(returnStatement))
        {
            sb.AppendLine($"        {returnStatement}");
        }

        sb.AppendLine("    }");
        sb.AppendLine();
    }

    /// <summary>
    /// Generates appropriate return statement for a method.
    /// Logic:
    /// 1. If return type matches an input parameter type -> return that parameter
    /// 2. If IGenericResult&lt;T&gt; where T matches input parameter -> return GenericResult&lt;T&gt;.Success(parameter)
    /// 3. If string -> return string.Empty
    /// 4. Otherwise -> return default
    /// </summary>
    private string GenerateReturnStatement(IMethodSymbol method, Compilation compilation)
    {
        var returnType = method.ReturnType;

        // Void methods
        if (returnType.SpecialType == SpecialType.System_Void)
            return string.Empty;

        // Check if return type matches any parameter type
        foreach (var param in method.Parameters)
        {
            if (SymbolEqualityComparer.Default.Equals(returnType, param.Type))
            {
                return $"return {param.Name};";
            }
        }

        // Check for IGenericResult<T> where T matches a parameter
        if (returnType is INamedTypeSymbol namedReturnType && namedReturnType.IsGenericType)
        {
            var genericDef = namedReturnType.OriginalDefinition.ToDisplayString();

            // Check if it's IGenericResult<T>
            if (genericDef.Contains("IGenericResult") && namedReturnType.TypeArguments.Length == 1)
            {
                var resultGenericType = namedReturnType.TypeArguments[0];

                // Find parameter matching the generic type
                foreach (var param in method.Parameters)
                {
                    if (SymbolEqualityComparer.Default.Equals(resultGenericType, param.Type))
                    {
                        var genericTypeDisplay = resultGenericType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                        return $"return global::FractalDataWorks.Results.GenericResult<{genericTypeDisplay}>.Success({param.Name});";
                    }
                }
            }
        }

        // String -> return string.Empty
        if (returnType.SpecialType == SpecialType.System_String)
            return "return string.Empty;";

        // Default for everything else
        return "return default;";
    }

    /// <summary>
    /// Creates a unique signature for a method (for deduplication).
    /// </summary>
    private string GetMethodSignature(IMethodSymbol method)
    {
        var paramTypes = string.Join(",", method.Parameters.Select(p => p.Type.ToDisplayString()));
        return $"{method.Name}({paramTypes})";
    }

    /// <summary>
    /// Gets the default value for a type symbol.
    /// </summary>
    private static string GetDefaultValueForTypeSymbol(ITypeSymbol typeSymbol)
    {
        if (typeSymbol.SpecialType == SpecialType.System_String)
            return "string.Empty";
        if (typeSymbol.SpecialType == SpecialType.System_Int32)
            return "0";
        if (typeSymbol.SpecialType == SpecialType.System_Boolean)
            return "false";
        if (typeSymbol.IsValueType)
            return "default";
        if (typeSymbol.NullableAnnotation == NullableAnnotation.Annotated)
            return "null!";
        if (typeSymbol.IsReferenceType)
            return "null!";
        return "default";
    }
}
