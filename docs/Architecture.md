# FractalDataWorks Developer Kit Architecture

## Overview
The FractalDataWorks Developer Kit follows a ServiceType-based plugin architecture where services are discovered and registered automatically through a type-safe system with high-performance lookups.

## Architecture Layers

### 1. Core Layer (`FractalDataWorks.Services.Abstractions` + `FractalDataWorks.Services`)
- **Abstractions**: Pure interfaces and contracts (`IFdwService`, `IServiceFactory`, etc.)
- **Services**: Base implementations (`ServiceBase`, `ServiceFactoryBase`) and framework infrastructure
- **ServiceTypes**: Plugin discovery framework (`ServiceTypeBase`, `ServiceTypeCollectionBase`)

### 2. Domain Abstractions Layer (`FractalDataWorks.Services.{Domain}.Abstractions`)
- Domain-specific interfaces that extend core interfaces with domain constraints and methods
- Domain-specific base classes that inherit from core base classes
- Domain-specific ServiceType bases with collection attributes for auto-discovery

### 3. Implementation Layer (`FractalDataWorks.Services.{Domain}.{Provider}`)
- Concrete ServiceType implementations with singleton pattern
- Provider-specific service implementations
- Configuration and factory implementations

## ServiceType Architecture

The framework uses a dual-purpose interface system that serves both generic constraint refinement and type discovery:

### ServiceType Interface Hierarchy

```csharp
IServiceType                                    // Base storage contract (Id, Name)
├── IServiceType<TService>                     // + Service type constraint
├── IServiceType<TService, TConfiguration>     // + Configuration constraint
└── IServiceType<TService, TConfiguration, TFactory> // + Factory constraint

// Domain-specific interfaces add specific constraints
IConnectionType : IServiceType                 // Connection base
└── IConnectionType<TService, TConfiguration, TFactory> : IServiceType<TService, TConfiguration, TFactory>
```

### ServiceType Implementation Rules

1. **Dual Inheritance Pattern**: Every service type MUST inherit from base class AND implement interface
2. **Singleton Pattern**: Service types MUST use singleton pattern with static Instance property
3. **Unique Sequential IDs**: Each service type MUST have unique integer ID
4. **Generic Type Constraints**: Generic parameters MUST follow framework constraints

```csharp
public sealed class MsSqlConnectionType :
    ConnectionTypeBase<IFdwConnection, MsSqlConfiguration, IMsSqlConnectionFactory>,
    IConnectionType<IFdwConnection, MsSqlConfiguration, IMsSqlConnectionFactory>,
    IConnectionType
{
    public static MsSqlConnectionType Instance { get; } = new();
    private MsSqlConnectionType() : base(id: 1, name: "MsSql", category: "Database") { }
}
```

### Type Parameter Constraints
- `TService`: Service interface implementing `IFdwService` or its derivatives
- `TConfiguration`: Configuration class implementing appropriate configuration interface
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
ServiceTypes Framework (FractalDataWorks.ServiceTypes)
    ↓
Enhanced Enums (FractalDataWorks.EnhancedEnums)
    ↓
Results & Messages (FractalDataWorks.Results + FractalDataWorks.Messages)
```

## ServiceType Auto-Discovery

The framework uses source generators to automatically discover and register service types:

1. **Service Type Discovery**: Source generators scan for classes inheriting from ServiceTypeBase
2. **Collection Generation**: Generates high-performance FrozenDictionary lookups in collections marked with `[ServiceTypeCollection]`
3. **Registration**: Service types are automatically available through their collections
4. **Configuration**: Service types validate and configure themselves during startup

## Cross-Cutting Concerns

### Configuration Management
- ServiceType-based configuration with section name discovery
- Dynamic configuration loading using interface metadata
- Validation through ServiceType.Configure() methods

### Service Discovery and Registration
- ServiceType collections provide O(1) lookups by ID, name, and type
- Interface-based polymorphic service usage
- Automatic service registration through ServiceType.Register() methods

### Message Handling
- Enhanced Enum-based message system
- Structured error reporting with severity levels
- Result pattern with FdwResult<T> for consistent error handling

### Factory Pattern
- ServiceType-aware factory creation
- Configuration-driven service instantiation
- Type-safe factory interfaces with generic constraints