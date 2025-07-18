# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Common Development Commands

### Build Commands
```bash
# Standard build
dotnet build

# Build with specific configuration
dotnet build --configuration Release  # For production builds
dotnet build --configuration Beta     # Recommended for development (enables all analyzers)

# Clean build artifacts
dotnet clean
```

### Test Commands
```bash
# Run all tests
dotnet test

# Run tests with code coverage
dotnet test --collect:"XPlat Code Coverage" --settings coverlet.runsettings

# Run a single test by name
dotnet test --filter "FullyQualifiedName~TestClassName.TestMethodName"

# Run tests from specific class
dotnet test --filter "FullyQualifiedName~FractalDataWorks.Tests.FractalTests"
```

### Code Quality Commands
```bash
# Build with Beta configuration to run all analyzers and enforce code style
dotnet build --configuration Beta

# Check for vulnerable packages
dotnet list package --vulnerable --include-transitive
```

### Package Commands
```bash
# Create NuGet package
dotnet pack --configuration Release
```

## High-Level Architecture

FractalDataWorks is a **Layer 0.5 foundation library** providing pure interface definitions for the FractalDataWorks platform. Key architectural principles:

### Core Components

1. **IGenericResult<T>** - Implements Result/Either monad pattern for functional error handling
   - Located in: src/FractalDataWorks/IGenericResult.cs
   - Provides railway-oriented programming with Map() and Match() methods
   - Enforces safe access to Value/Error through documented exceptions

2. **IEnhancedEnumOption<T>** - Enhanced enumeration pattern beyond standard .NET enums
   - Located in: src/FractalDataWorks/IEnhancedEnumOption.cs
   - Uses self-referencing generic constraint: `where T : IEnhancedEnumOption<T>`
   - Combines numeric ID with string Name for richer semantics

3. **Fractal** - Unit type struct (functional programming's void-as-value)
   - Located in: src/FractalDataWorks/Fractal.cs
   - All instances are equal (singleton behavior)
   - Used for operations without meaningful return values

### Design Patterns

- **Functional Programming**: Result monads, unit types, immutable interfaces
- **Self-Referencing Generics**: Type-safe implementations with flexibility
- **Interface Segregation**: Non-generic base interfaces with generic extensions
- **Zero Dependencies**: Pure interfaces on .NET Standard 2.0

### Build Configurations

The project uses 6 progressive build configurations:
- **Debug**: Fast development, no analyzers
- **Experimental**: Minimal enforcement
- **Alpha**: Basic checks
- **Beta**: Recommended for development (warnings as errors, all analyzers)
- **Preview**: Strict checks
- **Release**: Production-ready

Use **Beta** configuration during development to catch issues early.

### Testing

- **Framework**: xUnit v3 with Shouldly assertions
- **Coverage**: Coverlet integration
- Tests run in parallel by default (xunit.runner.json)

### CI/CD

GitHub Actions workflow handles:
- Multi-branch builds (master, develop, beta/*, release/*, etc.)
- Automated testing with coverage
- Security scanning with CodeQL
- Package publishing to GitHub Packages and NuGet.org
- Automatic versioning via Nerdbank.GitVersioning