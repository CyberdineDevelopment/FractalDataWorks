# FractalDataWorks.EnhancedEnums.CodeFixes

This package provides code fix providers for the FractalDataWorks Enhanced Enums system, working in conjunction with the FractalDataWorks.EnhancedEnums.Analyzers package to provide automated fixes for common Enhanced Enum issues.

## Features

- **Automated Code Fixes**: Provides quick fixes for analyzer warnings and errors
- **IDE Integration**: Seamlessly integrates with Visual Studio and other IDEs supporting Roslyn analyzers
- **Enhanced Enum Compliance**: Helps maintain proper Enhanced Enum patterns and conventions

## Code Fix Providers

This package includes code fix providers for:

- **Enhanced Enum Declaration Issues**: Fixes for improper Enhanced Enum declarations
- **Collection Registration Problems**: Automated fixes for Enhanced Enum collection registration issues
- **Naming Convention Violations**: Quick fixes for Enhanced Enum naming convention issues
- **Attribute Usage Problems**: Fixes for incorrect Enhanced Enum attribute usage

## Installation

This package is typically installed automatically as a dependency of the Enhanced Enums system. For manual installation:

```xml
<PackageReference Include="FractalDataWorks.EnhancedEnums.CodeFixes" Version="1.0.0" PrivateAssets="all" />
```

## Usage

Code fixes are applied automatically through your IDE's quick fix functionality (e.g., Ctrl+. in Visual Studio). The fixes work in conjunction with the analyzer warnings to provide contextual, automated solutions.

## Integration

- Works with `FractalDataWorks.EnhancedEnums.Analyzers`
- Supports Visual Studio, Visual Studio Code, and other Roslyn-compatible editors
- Provides automated refactoring suggestions for Enhanced Enum patterns

## Build Status

This project builds successfully with 0 warnings and 0 errors.