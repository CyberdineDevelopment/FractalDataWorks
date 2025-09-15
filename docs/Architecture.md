# FractalDataWorks Framework Architecture

## Overview
The FractalDataWorks Developer Kit follows a strict layered architecture where each layer builds upon the previous layer's contracts and implementations.

## Architecture Layers

### 1. Core Layer (`FractalDataWorks.Services.Abstractions` + `FractalDataWorks.Services`)
- **Abstractions**: Pure interfaces and contracts (`IFractalService`, `IServiceFactory`, etc.)
- **Services**: Base implementations (`ServiceBase`, `ServiceFactoryBase`, `ServiceTypeBase`) and framework infrastructure

### 2. Domain Abstractions Layer (`FractalDataWorks.Services.{Domain}.Abstractions`)
- Domain-specific interfaces that extend core interfaces with domain constraints and methods
- Domain-specific base classes that inherit from core base classes
- Domain-specific Enhanced Enums (TypeBase + CollectionBase)

### 3. Implementation Layer (`FractalDataWorks.Services.{Domain}.{Provider}`)
- Concrete implementations that inherit from domain abstractions
- Specific Enhanced Enum entries
- Provider-specific logic

## Inheritance Rules

- **Interfaces**: Each layer extends the previous layer's interfaces with additional domain-specific methods and constraints
- **Implementations**: Concrete services inherit from `ServiceBase` and implement domain interfaces
- **Enhanced Enums**: Each layer has its own `TypeBase` and `CollectionBase` classes
- **Base Classes**: Domain layers create base classes that add domain constraints to core base classes
- **No Override Rule**: Implementations should NOT override `ServiceBase` methods unless there's a specific reason to change the implementation

## Service Type Hierarchy

All services follow a three-parameter generic pattern:

```csharp
public abstract class ServiceTypeBase<TService, TConfiguration, TFactory> : EnumOptionBase<ServiceTypeBase<TService, TConfiguration, TFactory>>
    where TService : class, IFractalService<TExecutor, TConfiguration>
    where TConfiguration : class
    where TFactory : class, IServiceFactory<TService, TConfiguration>
```

### Type Parameter Constraints
- `TService`: The service interface implementing `IFractalService<TExecutor, TConfiguration>`
- `TConfiguration`: Configuration class (typically inherits from `ConfigurationBase<T>`)
- `TFactory`: Factory class implementing `IServiceFactory<TService, TConfiguration>`

## Dependency Flow

```
Application Layer
    ↓
Implementation Layer (FractalDataWorks.Services.{Domain}.{Provider})
    ↓  
Domain Abstractions Layer (FractalDataWorks.Services.{Domain}.Abstractions)
    ↓
Core Layer (FractalDataWorks.Services.Abstractions + FractalDataWorks.Services)
    ↓
Enhanced Enums (FractalDataWorks.EnhancedEnums)
    ↓
Results & Messages (FractalDataWorks.Results + FractalDataWorks.Messages)
```

## Cross-Cutting Concerns

### Configuration Management
- All configurations inherit from `ConfigurationBase<T>`
- FluentValidation integration for type-safe validation
- Environment-specific configuration loading

### Message Handling  
- Enhanced Enum-based message system
- Structured error reporting with severity levels
- Localization support through message templates

### Logging and Observability
- Built-in activity tracking and distributed tracing
- Performance metrics collection
- Structured logging with correlation IDs

### Dependency Injection
- Service registration through extension methods
- Factory pattern for service creation
- Singleton and scoped lifetime management