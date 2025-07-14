# FractalDataWorks

Part of the FractalDataWorks toolkit.

## Build Status

[![Master Build](https://github.com/CyberdineDevelopment/FractalDataWorks/actions/workflows/ci.yml/badge.svg?branch=master)](https://github.com/CyberdineDevelopment/FractalDataWorks/actions/workflows/ci.yml)
[![Develop Build](https://github.com/CyberdineDevelopment/FractalDataWorks/actions/workflows/ci.yml/badge.svg?branch=develop)](https://github.com/CyberdineDevelopment/FractalDataWorks/actions/workflows/ci.yml)

## Release Status

![GitHub release (latest by date)](https://img.shields.io/github/v/release/CyberdineDevelopment/FractalDataWorks)
![GitHub release (latest by date including pre-releases)](https://img.shields.io/github/v/release/CyberdineDevelopment/FractalDataWorks?include_prereleases&label=pre-release)

## Package Status

![Nuget](https://img.shields.io/nuget/v/FractalDataWorks)
![GitHub Packages](https://img.shields.io/badge/github%20packages-available-blue)

## License

[![License](https://img.shields.io/badge/License-Apache%202.0-blue.svg)](https://opensource.org/licenses/Apache-2.0)

Core interface definitions for the FractalDataWorks platform, providing foundational contracts for enhanced functionality.

## Overview

FractalDataWorks is a Layer 0.5 package containing pure interface definitions that serve as the foundation for the FractalDataWorks platform. This package includes interfaces like `IEnhancedEnum` which enable advanced enumeration patterns.

## Features

- **IEnhancedEnum**: Interface for enhanced enumeration types with ID and name properties
- **IEnhancedEnum<T>**: Generic interface supporting self-referencing enum patterns
- Pure interfaces with no implementation dependencies
- netstandard2.0 compatible for broad framework support

## Getting Started

### Prerequisites

- .NET Standard 2.0 compatible runtime
- For development: .NET 9.0 SDK or later

### Installation

From the REC-DevOps Azure Artifacts feed:

```bash
dotnet add package FractalDataWorks
```

### Building from Source

```bash
# Clone the repository
git clone https://REC-DevOps@dev.azure.com/REC-DevOps/Developer-Kit/_git/FractalDataWorks

# Navigate to the project directory
cd FractalDataWorks

# Restore dependencies
dotnet restore

# Build the project
dotnet build
```

### Running Tests

```bash
# Run all tests
dotnet test

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"
```

## Usage

```csharp
using FractalDataWorks;

public class OrderStatus : IEnhancedEnum<OrderStatus>
{
    public int Id { get; }
    public string Name { get; }
    
    private OrderStatus(int id, string name)
    {
        Id = id;
        Name = name;
    }
    
    public static OrderStatus Pending = new(1, "Pending");
    public static OrderStatus Processing = new(2, "Processing");
    public static OrderStatus Complete = new(3, "Complete");
    
    public OrderStatus Empty() => new OrderStatus(0, "None");
}
```

## Project Structure

```
├── src/                    # Source code
│   └── FractalDataWorks/   # Core interfaces
├── tests/                  # Unit and integration tests
├── docs/                   # Documentation
├── samples/                # Sample projects (future)
├── azure-pipelines.yml     # CI/CD pipeline configuration
├── Directory.Build.props   # Common build properties
└── README.md              # This file
```

## Package Structure

This package contains only interface definitions:
- No implementation code
- No external dependencies
- Minimal runtime footprint

## Related Packages

- **FractalDataWorks.EnhancedEnums**: Source generator for implementing enhanced enumerations
- **FractalDataWorks.SmartSwitches**: Advanced pattern matching capabilities (coming soon)

## Versioning

This project uses [Nerdbank.GitVersioning](https://github.com/dotnet/Nerdbank.GitVersioning) for automatic versioning based on git history.

## License

This project is licensed under the Apache License 2.0 - see the [LICENSE](LICENSE) file for details.

## Acknowledgments

- Part of the FractalDataWorks platform architecture
- Layer 0.5 pure interface package
- Follows semantic versioning