# FractalDataWorks.CodeBuilder.Analysis

This package provides code analysis capabilities for the FractalDataWorks CodeBuilder system, enabling introspection and analysis of C# code structures for code generation and transformation purposes.

## Features

- **Syntax Tree Analysis**: Deep analysis of C# syntax trees and semantic models
- **Symbol Resolution**: Advanced symbol resolution and type analysis
- **Dependency Mapping**: Analysis of dependencies between types and members
- **Metadata Extraction**: Extraction of metadata for code generation purposes

## Core Components

### Analysis Services
- **SyntaxAnalyzer**: Analyzes C# syntax trees for structural patterns
- **SemanticAnalyzer**: Provides semantic analysis using Roslyn semantic models
- **SymbolCollector**: Collects and categorizes symbols from compiled assemblies
- **DependencyMapper**: Maps dependencies between types and namespaces

### Analysis Models
- **AnalysisResult**: Represents the results of code analysis operations
- **SymbolInfo**: Contains detailed information about analyzed symbols
- **DependencyGraph**: Models relationships between code elements

## Installation

```xml
<PackageReference Include="FractalDataWorks.CodeBuilder.Analysis" Version="1.0.0" />
```

## Usage

```csharp
using FractalDataWorks.CodeBuilder.Analysis;

// Analyze a syntax tree
var analyzer = new SyntaxAnalyzer();
var result = await analyzer.AnalyzeAsync(syntaxTree);

// Perform semantic analysis
var semanticAnalyzer = new SemanticAnalyzer();
var semanticInfo = await semanticAnalyzer.AnalyzeAsync(compilation, syntaxTree);
```

## Integration

- Works with `FractalDataWorks.CodeBuilder.CSharp` for C#-specific analysis
- Integrates with `FractalDataWorks.CodeBuilder.Analysis.CSharp` for enhanced C# support
- Provides foundation for code generation tools

## Build Status

This project builds successfully with 0 warnings and 0 errors.

## Test Coverage

Currently under development. Test coverage analysis in progress.