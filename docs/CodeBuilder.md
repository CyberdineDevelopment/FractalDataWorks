# CodeBuilder and Analysis

## Overview
The CodeBuilder ecosystem provides programmatic C# code generation and analysis capabilities for source generators, metaprogramming, and automated code transformations.

## FractalDataWorks.CodeBuilder

### Core Components

#### ClassBuilder
Programmatically construct C# classes:
```csharp
var classBuilder = new ClassBuilder("MyClass")
    .WithNamespace("MyNamespace")
    .WithAccessModifier(AccessModifier.Public)
    .WithBaseClass("BaseClass")
    .WithInterface("IMyInterface");
```

#### MethodBuilder
Create methods with parameters and bodies:
```csharp
var methodBuilder = new MethodBuilder("ProcessData")
    .WithReturnType("Task<bool>")
    .WithParameter("data", "string")
    .WithParameter("cancellationToken", "CancellationToken", defaultValue: "default")
    .WithBody("return Task.FromResult(true);");
```

#### PropertyBuilder
Generate properties with getters and setters:
```csharp
var propertyBuilder = new PropertyBuilder("Name")
    .WithType("string")
    .WithGetter()
    .WithSetter()
    .WithInitializer("string.Empty");
```

#### FieldBuilder
Create class fields:
```csharp
var fieldBuilder = new FieldBuilder("_data")
    .WithType("string")
    .WithAccessModifier(AccessModifier.Private)
    .WithReadonly();
```

#### ConstructorBuilder
Build constructors with dependency injection:
```csharp
var constructorBuilder = new ConstructorBuilder()
    .WithParameter("logger", "ILogger<MyClass>")
    .WithBody("_logger = logger ?? throw new ArgumentNullException(nameof(logger));");
```

#### InterfaceBuilder
Create interface definitions:
```csharp
var interfaceBuilder = new InterfaceBuilder("IMyService")
    .WithNamespace("MyNamespace")
    .WithMethod("ProcessAsync", "Task<IFdwResult>");
```

#### EnumBuilder
Generate enumerations:
```csharp
var enumBuilder = new EnumBuilder("Status")
    .WithValue("Active", 1)
    .WithValue("Inactive", 2)
    .WithValue("Pending", 3);
```

## Usage Patterns

### Source Generator Integration
```csharp
public class MySourceGenerator : ISourceGenerator
{
    public void Execute(GeneratorExecutionContext context)
    {
        var classBuilder = new ClassBuilder("GeneratedService")
            .WithNamespace("Generated")
            .WithMethod("Execute", "void", body: "// Generated implementation");
            
        var sourceCode = classBuilder.Build();
        context.AddSource("GeneratedService.cs", sourceCode);
    }
}
```

### Dynamic Service Creation
```csharp
var serviceBuilder = new ClassBuilder($"{serviceName}Service")
    .WithNamespace("Dynamic.Services")
    .WithBaseClass($"ServiceBase<{executorName}, {configurationName}>")
    .WithInterface($"I{serviceName}Service");
    
foreach (var operation in operations)
{
    serviceBuilder.WithMethod($"{operation.Name}Async", 
        "Task<IFdwResult>", 
        body: GenerateOperationBody(operation));
}

var generatedService = serviceBuilder.Build();
```

## FractalDataWorks.CodeBuilder.Analysis

### Analysis Components

#### SyntaxAnalyzer
Analyze existing code structures:
```csharp
var analyzer = new SyntaxAnalyzer(sourceCode);
var classes = analyzer.FindClasses()
    .Where(c => c.InheritsFrom("ServiceBase"))
    .ToList();
```

#### SemanticAnalyzer
Perform semantic analysis with symbol information:
```csharp
var semanticAnalyzer = new SemanticAnalyzer(compilation, syntaxTree);
var typeSymbol = semanticAnalyzer.GetTypeSymbol("MyClass");
var members = semanticAnalyzer.GetMembers(typeSymbol);
```

#### CodeTransformer
Transform existing code structures:
```csharp
var transformer = new CodeTransformer(sourceCode);
var transformedCode = transformer
    .ReplaceMethodBody("OldMethod", "// New implementation")
    .AddUsingDirective("System.Threading.Tasks")
    .Build();
```

#### PatternMatcher
Find and analyze code patterns:
```csharp
var patternMatcher = new PatternMatcher();
var matches = patternMatcher
    .FindPattern("class * : ServiceBase<*, *>")
    .In(sourceCode);
```

## Integration with Enhanced Enums

CodeBuilder is used internally by the Enhanced Enums source generator:

```csharp
// Enhanced Enum collection generation
var collectionBuilder = new ClassBuilder(collectionName)
    .WithNamespace(enumNamespace)
    .WithAccessModifier(AccessModifier.Public)
    .WithStaticModifier();

// Add All() method
collectionBuilder.WithMethod("All", $"ImmutableArray<{enumTypeName}>",
    body: "return _all;",
    isStatic: true);

// Add GetByName method
collectionBuilder.WithMethod("GetByName", $"{enumTypeName}?",
    parameters: new[] { ("name", "string") },
    body: "_nameDict.TryGetValue(name, out var result) ? result : null;",
    isStatic: true);
```

## Best Practices

### 1. Builder Pattern
Always use the fluent builder pattern for complex code generation:
```csharp
var service = new ClassBuilder("MyService")
    .WithNamespace("Services")
    .WithBaseClass("ServiceBase")
    .WithInterface("IMyService")
    .WithConstructor(ctor => ctor
        .WithParameter("logger", "ILogger")
        .WithBody("_logger = logger;"))
    .WithMethod("ProcessAsync", "Task<IFdwResult>")
    .Build();
```

### 2. Validation
Validate generated code using Roslyn compilation:
```csharp
var compilation = CSharpCompilation.Create("TempAssembly")
    .AddReferences(MetadataReference.CreateFromFile(typeof(object).Assembly.Location))
    .AddSyntaxTrees(CSharpSyntaxTree.ParseText(generatedCode));

var diagnostics = compilation.GetDiagnostics();
if (diagnostics.Any(d => d.Severity == DiagnosticSeverity.Error))
{
    throw new InvalidOperationException("Generated code has compilation errors");
}
```

### 3. Formatting
Use consistent formatting and indentation:
```csharp
var formattedCode = Formatter.Format(syntaxNode, new AdhocWorkspace());
```

### 4. Comments
Include generated code markers and documentation:
```csharp
var classBuilder = new ClassBuilder("GeneratedClass")
    .WithComment("// <auto-generated />")
    .WithComment("// Generated by FractalDataWorks CodeBuilder")
    .WithXmlDocumentation("Generated service class");
```

### 5. Performance
Cache builders and reuse them when generating similar structures:
```csharp
private static readonly ConcurrentDictionary<string, ClassBuilder> _builderCache = new();

public ClassBuilder GetCachedBuilder(string className)
{
    return _builderCache.GetOrAdd(className, name => 
        new ClassBuilder(name).WithDefaultConfiguration());
}
```

## Common Use Cases

### Source Generators
Create compile-time code generation for repetitive patterns:
```csharp
[Generator]
public class ServiceGenerator : ISourceGenerator
{
    public void Execute(GeneratorExecutionContext context)
    {
        var serviceConfigs = DiscoverServiceConfigurations(context);
        
        foreach (var config in serviceConfigs)
        {
            var service = GenerateService(config);
            context.AddSource($"{config.Name}Service.g.cs", service);
        }
    }
}
```

### Code Scaffolding
Generate boilerplate service implementations:
```csharp
public string ScaffoldService(string serviceName, List<Operation> operations)
{
    var builder = new ClassBuilder($"{serviceName}Service")
        .WithBaseClass("ServiceBase")
        .WithInterface($"I{serviceName}Service");
        
    foreach (var op in operations)
    {
        builder.WithMethod($"{op.Name}Async", 
            $"Task<IFdwResult<{op.ReturnType}>>",
            body: "throw new NotImplementedException();");
    }
    
    return builder.Build();
}
```

### Template Processing
Transform code templates into concrete implementations:
```csharp
public string ProcessTemplate(string template, Dictionary<string, string> replacements)
{
    var transformer = new CodeTransformer(template);
    
    foreach (var replacement in replacements)
    {
        transformer.Replace($"{{{{ {replacement.Key} }}}}", replacement.Value);
    }
    
    return transformer.Build();
}
```

### Refactoring Tools
Automated code transformations and migrations:
```csharp
public string RefactorServiceToUseNewPattern(string sourceCode)
{
    return new CodeTransformer(sourceCode)
        .ReplacePattern("ServiceBase<TExecutor>", "ServiceBase<TExecutor, TConfiguration>")
        .AddUsingDirective("FractalDataWorks.Services.Configuration")
        .UpdateConstructorParameters()
        .Build();
}
```

### API Generation
Create client SDKs from service definitions:
```csharp
public string GenerateClient(ApiDefinition api)
{
    var clientBuilder = new ClassBuilder($"{api.Name}Client")
        .WithInterface($"I{api.Name}Client");
        
    foreach (var endpoint in api.Endpoints)
    {
        var method = GenerateClientMethod(endpoint);
        clientBuilder.WithMethod(method);
    }
    
    return clientBuilder.Build();
}
```