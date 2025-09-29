# FractalDataWorks Collection Systems Comprehensive Analysis

## Table of Contents

1. [Overview of Collection Systems](#overview-of-collection-systems)
2. [Enhanced Enums System](#enhanced-enums-system)
   - [ExecutionStatus Collection](#executionstatus-collection)
   - [CacheStrategy Collection](#cachestrategy-collection)
   - [ConnectionState Collection](#connectionstate-collection)
   - [TriggerType Collection](#triggertype-collection)
   - [HttpMethod Collection](#httpmethod-collection)
   - [HttpProtocol Collection](#httpprotocol-collection)
   - [SecurityMethod Collection](#securitymethod-collection)
   - [RateLimitPolicy Collection](#ratelimitpolicy-collection)
   - [EndpointType Collection](#endpointtype-collection)
   - [ProcessState Collection](#processstate-collection)
   - [ProcessType Collection](#processtype-collection)
   - [ConfigurationSourceType Collection](#configurationsourcetype-collection)
   - [ConfigurationChangeType Collection](#configurationchangetype-collection)
   - [SecretCommandType Collection](#secretcommandtype-collection)
   - [AuthenticationMethod Collection](#authenticationmethod-collection)
3. [Extended Enums System](#extended-enums-system)
4. [Type Options System](#type-options-system)
   - [DataSetType Collection](#datasettype-collection)
   - [DataContainerType Collection](#datacontainertype-collection)
   - [ComparisonOperator Collection](#comparisonoperator-collection)
   - [LogicalOperator Collection](#logicaloperator-collection)
   - [SortDirection Collection](#sortdirection-collection)
5. [Service Types System](#service-types-system)
   - [MsSqlConnectionType](#mssqlconnectiontype)
   - [RestConnectionType](#restconnectiontype)
   - [McpServerServiceType](#mcpserverservicetype)
   - [StandardTransformationServiceType](#standardtransformationservicetype)
   - [SqlDataGatewayServiceType](#sqldatagatewayservicetype)
   - [AzureEntraAuthenticationServiceType](#azureentraauthenticationservicetype)
6. [Critical Issues Summary](#critical-issues-summary)

---

## Overview of Collection Systems

FractalDataWorks uses four distinct collection systems, each serving different architectural purposes:

1. **Enhanced Enums** - Dynamic enumerations with behavior, using `EnumOptionBase<T>`
2. **Extended Enums** - Wraps existing C# enums with additional behavior, using `ExtendedEnumOptionBase<T, TEnum>`
3. **Type Options** - Type-safe collections for framework types, using `TypeOptionBase<T>`
4. **Service Types** - Service discovery and registration metadata, using `ServiceTypeBase<TService, TConfiguration, TFactory>`

Each system has its own:
- Base classes
- Attributes for source generation
- Collection base classes
- Source generators
- Discovery patterns

---

## Enhanced Enums System

Enhanced Enums use `EnumOptionBase<T>` as their base class and are discovered through `[EnumOption]` or `[TypeOption]` attributes for collection membership.

### ExecutionStatus Collection

**Purpose**: Represents the execution states of scheduled jobs in the scheduling service.

**Location**: `src/FractalDataWorks.Services.Scheduling.Abstractions/Interfaces/`

**Components**:
- **Base Class**: `ExecutionStatus.cs`
  - Extends: `EnumOptionBase<ExecutionStatus>`
  - Implements: `IExecutionStatus`
  - Constructor: Protected, takes `(int id, string name)`

- **Collection**: `ExecutionStatuses.cs`
  - Extends: `TypeCollectionBase<ExecutionStatus, IExecutionStatus>`
  - Attribute: `[TypeCollection(typeof(ExecutionStatus), typeof(IExecutionStatus), typeof(ExecutionStatuses))]`
  - Location: ✅ **CORRECT** - In abstractions project

- **Options** (All with `[TypeOption(typeof(ExecutionStatuses), "Name")]`):
  - `PendingExecutionStatus` - ID: 0, "Pending"
  - `RunningExecutionStatus` - ID: 1, "Running"
  - `CompletedExecutionStatus` - ID: 2, "Completed"
  - `FailedExecutionStatus` - ID: 3, "Failed"
  - `CancelledExecutionStatus` - ID: 4, "Cancelled"
  - `TimedOutExecutionStatus` - ID: 5, "TimedOut"

**Status**: ✅ **PERFECT IMPLEMENTATION**

### CacheStrategy Collection

**Purpose**: Defines caching strategies for the Data Gateway service schema discovery.

**Location Split**:
- Base and Options: `src/FractalDataWorks.Services.DataGateway.Abstractions/Configuration/`
- Collection: `src/FractalDataWorks.Services.DataGateway/`

**Components**:
- **Base Class**: `CacheStrategy.cs`
  - Extends: `EnumOptionBase<CacheStrategy>`
  - Implements: `ICacheStrategy`
  - Constructor: Protected, takes `(int id, string name)`

- **Collection**: `CacheStrategies.cs`
  - Extends: `TypeCollectionBase<CacheStrategy, ICacheStrategy>`
  - Attribute: `[TypeCollection(typeof(CacheStrategy), typeof(ICacheStrategy), typeof(CacheStrategies))]`
  - Location: ❌ **INCORRECT** - Should be in abstractions project

- **Options** (All with `[TypeOption(typeof(CacheStrategies), "Name")]`):
  - `NoneCacheStrategy` - ID: 1, "None" - No caching, discover on every request
  - `MemoryCacheStrategy` - ID: 2, "Memory" - In-memory cache for application lifetime
  - `PersistentCacheStrategy` - ID: 3, "Persistent" - Disk-based persistent cache
  - `HybridCacheStrategy` - ID: 4, "Hybrid" - Memory with persistent backup

**Status**: ❌ **INCORRECT PROJECT PLACEMENT** - Collection should move to abstractions

### ConnectionState Collection

**Purpose**: Tracks the lifecycle states of external connections for connection management and pooling.

**Location**: `src/FractalDataWorks.Services.Connections.Abstractions/States/`

**Components**:
- **Base Class**: `ConnectionStateBase.cs`
  - Extends: `EnumOptionBase<ConnectionStateBase>`
  - Implements: `IConnectionState`
  - Constructor: Protected, takes `(int id, string name)`

- **Collection**: `ConnectionStates.cs`
  - Extends: `TypeCollectionBase<ConnectionStateBase, IConnectionState>`
  - Attribute: `[TypeCollection(typeof(ConnectionStateBase), typeof(IConnectionState), typeof(ConnectionStates))]`
  - Location: ✅ **CORRECT** - In abstractions project

- **Options** (All with `[TypeOption(typeof(ConnectionStates), "Name")]`, using primary constructors):
  - `CreatedConnectionState()` - ID: 0, "Created"
  - `OpeningConnectionState()` - ID: 1, "Opening"
  - `OpenConnectionState()` - ID: 3, "Open"
  - `ExecutingConnectionState()` - ID: 4, "Executing"
  - `ClosingConnectionState()` - ID: 5, "Closing"
  - `ClosedConnectionState()` - ID: 6, "Closed"
  - `BrokenConnectionState()` - ID: 7, "Broken"
  - `DisposedConnectionState()` - ID: 8, "Disposed"
  - `UnknownConnectionState()` - ID: 9, "Unknown"

**Status**: ✅ **PERFECT IMPLEMENTATION** with modern C# 12 primary constructors

### TriggerType Collection

**Purpose**: Defines trigger types for the scheduling service.

**Location**: `src/FractalDataWorks.Services.Scheduling.Abstractions/EnhancedEnums/`

**Components**:
- **Base Class**: `TriggerTypeBase.cs`
  - Extends: `EnumOptionBase<TriggerTypeBase>`
  - Implements: `ITriggerType`

- **Collection**: `TriggerTypes.cs` (location unknown, likely in same folder)

- **Options** in `TriggerTypeImplementations/`:
  - `Once.cs` - One-time execution trigger
  - `Cron.cs` - Cron expression-based trigger
  - `Interval.cs` - Fixed interval trigger
  - `Manual.cs` - Manual trigger only

**Status**: ✅ Appears correct but collection location not verified

### HttpMethod Collection

**Purpose**: Represents HTTP methods for HTTP-based connections.

**Location**: `src/FractalDataWorks.Services.Connections.Http.Abstractions/EnhancedEnums/HttpMethods/`

**Components**:
- **Base Class**: `HttpMethodBase.cs`
  - Extends: `EnumOptionBase<HttpMethodBase>`
  - Implements: `IHttpMethod`

- **Collection**: `HttpMethods.cs` (assumed)

- **Options**:
  - `GetMethod.cs` - HTTP GET
  - `PostMethod.cs` - HTTP POST
  - `PutMethod.cs` - HTTP PUT
  - `PatchMethod.cs` - HTTP PATCH
  - `DeleteMethod.cs` - HTTP DELETE

**Status**: ✅ Appears correct structure

### HttpProtocol Collection

**Purpose**: Represents HTTP protocol versions.

**Location**: `src/FractalDataWorks.Services.Connections.Http.Abstractions/EnhancedEnums/HttpProtocols/`

**Components**:
- **Base Class**: `HttpProtocolBase.cs`
  - Extends: `EnumOptionBase<HttpProtocolBase>`
  - Implements: `IHttpProtocol`

**Status**: Base class exists, options not examined

### SecurityMethod Collection

**Purpose**: Security methods for web HTTP endpoints.

**Location**: `src/FractalDataWorks.Web.Http.Abstractions/Security/`

**Options Found**:
- `None.cs` - No security
- `ApiKey.cs` - API key authentication
- `JWT.cs` - JSON Web Token authentication
- `OAuth2.cs` - OAuth2 authentication
- `Certificate.cs` - Certificate-based authentication

**Status**: Options exist, base class and collection not examined

### RateLimitPolicy Collection

**Purpose**: Rate limiting policies for web HTTP endpoints.

**Location**: `src/FractalDataWorks.Web.Http.Abstractions/EnhancedEnums/Policies/`

**Options Found**:
- `None.cs` - No rate limiting
- `TokenBucket.cs` - Token bucket algorithm
- `FixedWindow.cs` - Fixed window counter
- `SlidingWindow.cs` - Sliding window counter
- `Concurrency.cs` - Concurrency limiting

**Status**: Options exist, base class and collection not examined

### EndpointType Collection

**Purpose**: Types of web HTTP endpoints.

**Location**: `src/FractalDataWorks.Web.Http.Abstractions/EndPoints/`

**Options Found**:
- `CRUD.cs` - CRUD operations endpoint
- `Command.cs` - Command pattern endpoint
- `Query.cs` - Query endpoint
- `EventEndpoint.cs` - Event streaming endpoint
- `File.cs` - File upload/download endpoint
- `Health.cs` - Health check endpoint

**Status**: Options exist, base class and collection not examined

### ProcessState Collection

**Purpose**: Execution process states.

**Location**: `src/FractalDataWorks.Services.Execution.Abstractions/EnhancedEnums/`

**Components**:
- **Base Class**: `ProcessStateBase.cs`
  - Extends: `EnumOptionBase<ProcessStateBase>`

- **Options** in `States/`:
  - `Created.cs` - Process created
  - `Running.cs` - Process running
  - `Completed.cs` - Process completed
  - `Failed.cs` - Process failed
  - `Cancelled.cs` - Process cancelled

**Status**: ✅ Structure appears correct

### ProcessType Collection

**Purpose**: Types of execution processes.

**Location**: `src/FractalDataWorks.Services.Execution.Abstractions/EnhancedEnums/`

**Components**:
- **Base Class**: `ProcessTypeBase.cs`
  - Extends: `EnumOptionBase<ProcessTypeBase>`

**Status**: Base class exists, options not found

### ConfigurationSourceType Collection

**Purpose**: Sources of configuration data.

**Location**: `src/FractalDataWorks.Configuration.Abstractions/EnhancedEnums/ConfigurationSourceTypes/`

**Components**:
- **Base Class**: `ConfigurationSourceTypeBase.cs`
  - Extends: `EnumOptionBase<ConfigurationSourceTypeBase>`

- **Options** (All with `[EnumOption]`):
  - `CommandLine.cs` - Command line arguments
  - `Custom.cs` - Custom configuration source
  - `Database.cs` - Database configuration
  - `Environment.cs` - Environment variables
  - `FileConfigurationSource.cs` - File-based configuration
  - `Memory.cs` - In-memory configuration
  - `Remote.cs` - Remote configuration service

**Status**: ✅ Complete implementation

### ConfigurationChangeType Collection

**Purpose**: Types of configuration changes.

**Location**: `src/FractalDataWorks.Configuration.Abstractions/EnhancedEnums/ConfigurationChangeTypes/`

**Components**:
- **Base Class**: `ConfigurationChangeTypeBase.cs`
  - Extends: `EnumOptionBase<ConfigurationChangeTypeBase>`

- **Options** (All with `[EnumOption]`):
  - `Added.cs` - Configuration added
  - `Updated.cs` - Configuration updated
  - `Deleted.cs` - Configuration deleted
  - `Reloaded.cs` - Configuration reloaded

**Status**: ✅ Complete implementation

### SecretCommandType Collection

**Purpose**: Azure Key Vault secret management commands.

**Location**: `src/FractalDataWorks.Services.SecretManagers.AzureKeyVault/EnhancedEnums/`

**Components**:
- **Base Class**: `SecretCommandTypeBase.cs`
  - Extends: `EnumOptionBase<SecretCommandTypeBase>`

- **Options**:
  - `GetSecret.cs` - Retrieve secret
  - `SetSecret.cs` - Store secret
  - `DeleteSecret.cs` - Delete secret
  - `ListSecrets.cs` - List all secrets
  - `GetSecretVersions.cs` - Get secret version history

**Status**: ✅ Complete implementation

### AuthenticationMethod Collection

**Purpose**: Authentication methods for authentication services.

**Location**: `src/FractalDataWorks.Services.Authentication/`

**Components**:
- **Base Class**: `AuthenticationMethodBase.cs` (in Abstractions)
  - Implements: `IAuthenticationMethod`

- **Collection**: `AuthenticationMethods.cs`

- **Options**:
  - `FormBasedAuthenticationMethod.cs` - Form-based authentication
  - `OAuth2AuthenticationMethod.cs` - OAuth2 authentication

**Status**: Implementation exists

---

## Extended Enums System

**Purpose**: Wraps existing C# enums with additional behavior and Enhanced Enum features.

**Infrastructure**:
- Base class: `ExtendedEnumOptionBase<T, TEnum>` in `src/FractalDataWorks.EnhancedEnums/ExtendedEnums/`
- Attribute: `ExtendEnumAttribute` in `src/FractalDataWorks.EnhancedEnums/ExtendedEnums/Attributes/`

**Implementations Found**: **NONE**

**Status**: ❌ **UNUSED SYSTEM** - Infrastructure exists but no actual implementations in codebase

---

## Type Options System

Type Options use `TypeOptionBase<T>` or `TypeOptionBase` as their base class and are discovered through `[TypeOption]` attributes.

### DataSetType Collection

**Purpose**: Defines dataset types for data operations.

**Location Split**:
- Base: `src/FractalDataWorks.DataSets.Abstractions/`
- Collection: `src/FractalDataWorks.DataSets/`

**Components**:
- **Base Class**: `DataSetTypeBase.cs`
  - Extends: `TypeOptionBase<DataSetTypeBase>`
  - Implements: `IDataSetType`
  - Constructor: Complex with fields, record type, category
  - Features: Field validation, key field requirements, LINQ query builder

- **Collection**: `DataSetTypes.cs`
  - Extends: `TypeCollectionBase<DataSetTypeBase, IDataSetType>`
  - Attribute: `[TypeCollection(typeof(DataSetTypeBase), typeof(IDataSetType), typeof(DataSetTypes))]`
  - Location: ❌ **INCORRECT** - Should be in abstractions project

- **Options**: **NONE FOUND**

**Status**: ❌ **INCOMPLETE** - No actual dataset type implementations

### DataContainerType Collection

**Purpose**: Defines container types for data storage (CSV, JSON, SQL table, etc.).

**Location**: `src/FractalDataWorks.DataContainers/`

**Components**:
- **Base Classes** (TWO COMPETING BASES):
  1. `DataContainerTypeBase.cs`
     - Extends: `TypeOptionBase` ✅ CORRECT
     - Implements: `IDataContainerType`
     - Features: File extension, MIME type, schema inference support

  2. `DataContainerType.cs`
     - Does NOT extend TypeOptionBase ❌ INCORRECT
     - Implements: `IDataContainerType`
     - Simple class with Id, Name, Description

- **Collection**: `DataContainerTypes.cs`
  - Extends: `TypeCollectionBase<DataContainerType, IDataContainerType>`
  - Attribute: `[TypeCollection(typeof(DataContainerType), typeof(DataContainerType), typeof(DataContainerTypes))]`
  - Problem: References wrong base class (DataContainerType instead of DataContainerTypeBase)

- **Options**: **NONE FOUND**

**Status**: ❌ **BROKEN IMPLEMENTATION** - Conflicting base classes, no implementations

### ComparisonOperator Collection

**Purpose**: Comparison operators for dataset queries (=, !=, <, >, etc.).

**Location Split**:
- Base: `src/FractalDataWorks.DataSets.Abstractions/Operators/`
- Collection: `src/FractalDataWorks.DataSets/`
- Options: `src/FractalDataWorks.DataSets.Abstractions/Operators/Comparison/`

**Components**:
- **Base Class**: `ComparisonOperatorBase.cs`
  - Extends: `TypeOptionBase<ComparisonOperatorBase>`
  - Implements: `IComparisonOperator`
  - Constructor: Takes id, name, description, sqlOperator, isSingleValue, category
  - Features: SQL operator translation support

- **Collection**: `ComparisonOperators.cs`
  - Extends: `TypeCollectionBase<ComparisonOperatorBase, IComparisonOperator>`
  - Attribute: `[TypeCollection(typeof(ComparisonOperatorBase), typeof(IComparisonOperator), typeof(ComparisonOperators))]`
  - Location: ❌ **INCORRECT** - Should be in abstractions project

- **Options** (NO TypeOption attributes - relies on inheritance discovery):
  - `EqualOperator` - ID: 1, "Equal", SQL: "="
  - `NotEqualOperator` - ID: 2, "NotEqual", SQL: "!="
  - `LessThanOperator` - ID: 3, "LessThan", SQL: "<"
  - `LessThanOrEqualOperator` - ID: 4, "LessThanOrEqual", SQL: "<="
  - `GreaterThanOperator` - ID: 5, "GreaterThan", SQL: ">"
  - `GreaterThanOrEqualOperator` - ID: 6, "GreaterThanOrEqual", SQL: ">="
  - `InOperator` - ID: 7, "In", SQL: "IN", isSingleValue: false
  - `NotInOperator` - ID: 8, "NotIn", SQL: "NOT IN", isSingleValue: false
  - `ContainsOperator` - ID: 9, "Contains", SQL: "LIKE", with %value%
  - `StartsWithOperator` - ID: 10, "StartsWith", SQL: "LIKE", with value%
  - `EndsWithOperator` - ID: 11, "EndsWith", SQL: "LIKE", with %value

**Status**: ✅ **COMPLETE** but collection in wrong project

### LogicalOperator Collection

**Purpose**: Logical operators for combining conditions (AND, OR).

**Location Split**:
- Base: `src/FractalDataWorks.DataSets.Abstractions/Operators/`
- Collection: `src/FractalDataWorks.DataSets/`
- Options: `src/FractalDataWorks.DataSets.Abstractions/Operators/Logical/`

**Components**:
- **Base Class**: `LogicalOperatorBase.cs`
  - Extends: `TypeOptionBase<LogicalOperatorBase>`
  - Implements: `ILogicalOperator`
  - Features: Precedence support for operator evaluation order

- **Collection**: `LogicalOperators.cs`
  - Extends: `TypeCollectionBase<LogicalOperatorBase, ILogicalOperator>`
  - Location: ❌ **INCORRECT** - Should be in abstractions project

- **Options**:
  - `AndOperator` - ID: 1, "And", precedence: 2, SQL: "AND"
  - `OrOperator` - ID: 2, "Or", precedence: 1, SQL: "OR"

**Status**: ✅ **COMPLETE** but collection in wrong project

### SortDirection Collection

**Purpose**: Sort directions for query ordering.

**Location Split**:
- Base: `src/FractalDataWorks.DataSets.Abstractions/Operators/`
- Collection: `src/FractalDataWorks.DataSets/`
- Options: `src/FractalDataWorks.DataSets.Abstractions/Operators/Sort/`

**Components**:
- **Base Class**: `SortDirectionBase.cs`
  - Extends: `TypeOptionBase<SortDirectionBase>`
  - Implements: `ISortDirection`
  - Features: isAscending flag, SQL keyword

- **Collection**: `SortDirections.cs`
  - Extends: `TypeCollectionBase<SortDirectionBase, ISortDirection>`
  - Location: ❌ **INCORRECT** - Should be in abstractions project

- **Options**:
  - `AscendingDirection` - ID: 1, "Ascending", isAscending: true, SQL: "ASC"
  - `DescendingDirection` - ID: 2, "Descending", isAscending: false, SQL: "DESC"

**Status**: ✅ **COMPLETE** but collection in wrong project

---

## Service Types System

Service Types use `ServiceTypeBase<TService, TConfiguration, TFactory>` and represent service registration metadata. They are NOT Enhanced Enums.

### MsSqlConnectionType

**Purpose**: Service type for Microsoft SQL Server connections.

**Location**: `src/FractalDataWorks.Services.Connections.MsSql/MsSqlConnectionType.cs`

**Implementation**:
```csharp
public sealed class MsSqlConnectionType :
    ConnectionTypeBase<IGenericConnection, MsSqlConfiguration, IMsSqlConnectionFactory>
{
    public static MsSqlConnectionType Instance { get; } = new(); // ✅ SINGLETON
    private MsSqlConnectionType() : base(2, "MsSql", "Database Connections") { }

    public override Type FactoryType => typeof(IMsSqlConnectionFactory);

    public override void Register(IServiceCollection services)
    {
        services.AddScoped<IMsSqlConnectionFactory, MsSqlConnectionFactory>();
        services.AddScoped<MsSqlService>();
        services.AddScoped<MsSqlCommandTranslator>();
        services.AddScoped<ExpressionTranslator>();
    }
}
```

**Status**: ✅ **PERFECT IMPLEMENTATION** - Singleton pattern, proper inheritance

### RestConnectionType

**Purpose**: Service type for REST API connections.

**Location**: `src/FractalDataWorks.Services.Connections.Rest/RestConnectionType.cs`

**Implementation**:
```csharp
[ServiceTypeOption(typeof(ConnectionTypes), "Rest")]
public sealed class RestConnectionType :
    ConnectionTypeBase<RestService, RestConnectionConfiguration, RestConnectionFactory>
{
    public RestConnectionType() : base(1, "REST", "Http") { } // ❌ NO SINGLETON

    public override void Register(IServiceCollection services)
    {
        services.AddHttpClient<RestService>();
        services.AddScoped<RestService>();
        services.AddSingleton<RestConnectionFactory>();
    }
}
```

**Issues**:
- ❌ Missing singleton pattern (should have `public static RestConnectionType Instance`)
- ✅ Has ServiceTypeOption attribute for discovery
- ✅ Proper service registration

**Status**: ❌ **MISSING SINGLETON PATTERN**

### McpServerServiceType

**Purpose**: Service type for MCP Server (code analysis and refactoring).

**Location**: `src/FractalDataWorks.Services.MCP/McpServerServiceType.cs`

**Implementation**:
```csharp
public class McpServerServiceType :
    ServiceTypeBase<McpOrchestrationService, object, object> // ❌ USES OBJECT
{
    public McpServerServiceType() : base(...) { } // ❌ NO SINGLETON

    public override void Register(IServiceCollection services)
    {
        services.AddSingleton<McpOrchestrationService>();
    }
}
```

**Issues**:
- ❌ Uses `object` for configuration and factory types (should have proper types)
- ❌ Missing singleton pattern
- ❌ Missing ServiceTypeOption attribute (not discoverable)

**Status**: ❌ **BROKEN IMPLEMENTATION**

### StandardTransformationServiceType

**Purpose**: Service type for data transformation services.

**Location**: `src/FractalDataWorks.Services.Transformations/StandardTransformationServiceType.cs`

**Implementation**:
```csharp
[ServiceTypeOption(typeof(TransformationTypes), "StandardTransformation")]
public sealed class StandardTransformationServiceType :
    TransformationTypeBase<ITransformationProvider, ITransformationsConfiguration,
                          IServiceFactory<ITransformationProvider, ITransformationsConfiguration>>,
    IEnumOption<StandardTransformationServiceType> // ❌ WRONG INTERFACE
{
    public StandardTransformationServiceType() : base(...) { } // ❌ NO SINGLETON

    public override Type GetFactoryImplementationType()
    {
        return typeof(GenericServiceFactory<ITransformationProvider, ITransformationsConfiguration>);
    }
}
```

**Issues**:
- ❌ Implements `IEnumOption<>` (ServiceTypes should NOT implement Enhanced Enum interfaces)
- ❌ Missing singleton pattern
- ✅ Has ServiceTypeOption attribute

**Status**: ❌ **INCORRECT INTERFACE IMPLEMENTATION**

### SqlDataGatewayServiceType

**Purpose**: Service type for SQL Server data gateway.

**Location**: `src/FractalDataWorks.Services.DataGateway/SqlDataGatewayServiceType.cs`

**Implementation**:
```csharp
[ServiceTypeOption(typeof(DataGatewayTypes), "SqlDataGateway")]
public sealed class SqlDataGatewayServiceType :
    DataGatewayTypeBase<IDataService, IDataGatewaysConfiguration,
                        IServiceFactory<IDataService, IDataGatewaysConfiguration>>
{
    public SqlDataGatewayServiceType() : base(...) { } // ❌ NO SINGLETON

    public override Type GetFactoryImplementationType()
    {
        return typeof(GenericServiceFactory<IDataGateway, IDataGatewayConfiguration>);
    }
}
```

**Issues**:
- ❌ Missing singleton pattern
- ✅ Has ServiceTypeOption attribute
- ⚠️ Factory return type doesn't match generic parameters

**Status**: ❌ **MISSING SINGLETON, TYPE MISMATCH**

### AzureEntraAuthenticationServiceType

**Purpose**: Service type for Azure Entra (Azure AD) authentication.

**Location**: `src/FractalDataWorks.Services.Authentication.AzureEntra/AzureEntraAuthenticationServiceType.cs`

**Implementation**:
```csharp
[ServiceTypeOption(typeof(AuthenticationTypes), "AzureEntraService")]
public sealed class AzureEntraAuthenticationServiceType :
    AuthenticationTypeBase<IAuthenticationService, IAuthenticationConfiguration,
                          IAzureEntraAuthenticationServiceFactory>,
    IEnumOption<AzureEntraAuthenticationServiceType> // ❌ WRONG INTERFACE
{
    public AzureEntraAuthenticationServiceType() : base(id: 1, name: "AzureEntra") { } // ❌ NO SINGLETON

    public override Type GetFactoryImplementationType()
    {
        return typeof(GenericServiceFactory<IAuthenticationService, IAuthenticationConfiguration>);
    }
}
```

**Issues**:
- ❌ Implements `IEnumOption<>` (ServiceTypes should NOT implement Enhanced Enum interfaces)
- ❌ Missing singleton pattern
- ✅ Has ServiceTypeOption attribute

**Status**: ❌ **INCORRECT INTERFACE IMPLEMENTATION**

---

## Critical Issues Summary

### High Priority Issues

1. **Collections in Wrong Projects** (Affects discoverability):
   - `CacheStrategies` - Move from DataGateway to DataGateway.Abstractions
   - `DataSetTypes` - Move from DataSets to DataSets.Abstractions
   - `ComparisonOperators` - Move from DataSets to DataSets.Abstractions
   - `LogicalOperators` - Move from DataSets to DataSets.Abstractions
   - `SortDirections` - Move from DataSets to DataSets.Abstractions

2. **ServiceType Implementation Errors**:
   - `McpServerServiceType` - Uses `object` types, no singleton, no attribute
   - `RestConnectionType` - Missing singleton pattern
   - `StandardTransformationServiceType` - Implements IEnumOption (wrong), no singleton
   - `SqlDataGatewayServiceType` - Missing singleton, type mismatch in factory
   - `AzureEntraAuthenticationServiceType` - Implements IEnumOption (wrong), no singleton

3. **Broken/Incomplete Type Options**:
   - `DataContainerType` - Two competing base classes, collection references wrong one
   - `DataSetType` - No actual implementations found
   - `DataContainerType` - No actual implementations found

### Medium Priority Issues

1. **Extended Enums** - Infrastructure exists but completely unused
2. **Missing TypeOption Attributes** - Some operator implementations don't have TypeOption attributes

### Correct Implementations (Reference Examples)

1. **Enhanced Enums**:
   - `ExecutionStatus` collection - Perfect implementation
   - `ConnectionState` collection - Perfect with modern C# 12 syntax

2. **ServiceTypes**:
   - `MsSqlConnectionType` - Perfect singleton pattern and implementation

3. **Type Options**:
   - `ComparisonOperator` implementations - Complete and functional
   - `LogicalOperator` implementations - Complete and functional
   - `SortDirection` implementations - Complete and functional

### Recommendations

1. **Immediate Actions**:
   - Move all collections to their abstractions projects
   - Fix ServiceType singleton patterns
   - Remove IEnumOption from ServiceType implementations
   - Fix McpServerServiceType to use proper types

2. **Architecture Cleanup**:
   - Resolve DataContainerType dual base class issue
   - Either implement Extended Enums or remove the infrastructure
   - Add missing TypeOption attributes to operator implementations

3. **Documentation**:
   - Document the distinction between the four systems
   - Create implementation guides for each system
   - Add analyzer rules to enforce patterns