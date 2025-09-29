# FractalDataWorks.MCP Comprehensive Test Suite

This test project provides comprehensive test coverage for the MCP (Model Context Protocol) plugin architecture using xUnit.v3 and Shouldly assertion library.

## Test Structure

### 1. Plugin Interface Tests (`Abstractions/`)

#### IToolPluginTests.cs
- **Interface Contract Tests**: Validates IToolPlugin inherits from IGenericService
- **Property Tests**: Id, Name, Description, Category, Priority, IsEnabled
- **Method Tests**: GetTools(), InitializeAsync(), ValidateConfigurationAsync(), GetHealthAsync(), ShutdownAsync()
- **Generic Plugin Tests**: IToolPlugin<TConfiguration> with typed configuration
- **Configuration Tests**: IToolPluginConfiguration properties and inheritance
- **Plugin Health Tests**: PluginHealth model with all health statuses
- **Edge Cases**: Null handling, cancellation token support, timeout scenarios

#### IMcpToolTests.cs
- **Interface Contract Tests**: Validates IMcpTool inherits from ITool
- **Property Tests**: OwningPlugin, Category, IsEnabled, Priority
- **Execution Tests**: ExecuteAsync() with Railway-oriented results
- **Validation Tests**: ValidateArgumentsAsync() with various argument types
- **Error Handling**: Exception propagation, cancellation scenarios
- **Concurrency Tests**: Sequential and concurrent execution patterns
- **Complex Arguments**: Nested objects, large data, Unicode, special characters
- **Performance Tests**: Timeout handling, memory pressure scenarios

### 2. Enhanced Enum Tests (`EnhancedEnums/`)

#### ToolCategoryTests.cs
- **Base Class Tests**: ToolCategoryBase inheritance and properties
- **Category Implementations**: All 7 tool categories (SessionManagement, CodeAnalysis, VirtualEditing, Refactoring, TypeAnalysis, ProjectDependencies, ServerManagement)
- **Singleton Pattern**: Validates Instance property for all categories
- **Display Priority**: Tests default and overridden DisplayPriority values
- **Source Generation**: ToolCategories collection methods (All, ById, ByName, TryGet methods)
- **Validation**: Unique IDs, names, non-empty descriptions
- **Ordering**: DisplayPriority-based sorting

### 3. Service Type Tests (`ServiceTypes/`)

#### McpServerServiceTypeTests.cs
- **Singleton Pattern**: McpServerServiceType.Instance validation
- **Service Registration**: DI container service registration verification
- **Configuration Tests**: Valid/invalid configuration handling
- **Property Validation**: Id, Name, Category, SectionName, DisplayName, Description, FactoryType
- **Service Lifetimes**: Singleton lifetime verification for all services
- **Error Handling**: Missing configuration sections, validation failures
- **Thread Safety**: Concurrent access to singleton instance
- **Edge Cases**: Null configurations, malformed sections

### 4. Connection Type Tests (`ConnectionTypes/`)

#### RoslynWorkspaceConnectionTypeTests.cs
- **Connection Type Properties**: Id, Name, Category, DisplayName, Description
- **Service Registration**: Roslyn-specific service registrations
- **Configuration Validation**: MaxConcurrentSessions, SessionTimeoutMinutes boundaries
- **Service Lifetimes**: Mixed singleton/scoped service verification
- **Error Scenarios**: Invalid configuration values, null handling
- **Boundary Testing**: Extreme values for configuration parameters
- **Thread Safety**: Concurrent singleton access

### 5. Plugin System Tests (`PluginSystem/`)

#### PluginRegistryTests.cs
- **Plugin Registration**: RegisterPluginAsync/UnregisterPluginAsync operations
- **Plugin Discovery**: GetAllPlugins, GetEnabledPlugins, GetPluginsByCategory
- **Plugin Lookup**: GetPluginById, IsPluginRegistered, GetPluginCount
- **Priority Handling**: GetPluginsByPriority with sorting
- **Error Conditions**: Null plugins, duplicate registrations, invalid IDs
- **Bulk Operations**: ClearPluginsAsync for cleanup
- **Edge Cases**: Large plugin collections, special characters in IDs, Unicode support
- **Concurrency**: Thread-safe operations, concurrent registration

#### PluginLoaderTests.cs
- **Directory Loading**: LoadPluginsFromDirectoryAsync with various directory scenarios
- **Assembly Loading**: LoadPluginFromAssemblyAsync with valid/invalid assemblies
- **Type Loading**: LoadPluginFromTypeAsync with plugin type validation
- **Plugin Discovery**: DiscoverPluginTypesInAssemblyAsync type scanning
- **Validation**: ValidatePluginAsync with comprehensive plugin validation
- **File System**: GetSupportedPluginExtensions, IsPluginAssembly
- **Error Handling**: Corrupted assemblies, non-plugin types, missing files
- **Edge Cases**: Deep directory structures, large assemblies, special characters
- **Type Safety**: Abstract types, interfaces, generic types rejection

#### PluginHealthMonitorTests.cs
- **Monitoring Lifecycle**: StartMonitoringAsync/StopMonitoringAsync operations
- **Health Checking**: CheckPluginHealthAsync for individual plugins
- **Bulk Health Checks**: CheckAllPluginsHealthAsync for system-wide status
- **Health Summary**: GetHealthSummaryAsync with aggregated statistics
- **Plugin Management**: AddPluginToMonitoringAsync/RemovePluginFromMonitoringAsync
- **Status Tracking**: IsMonitoring, MonitoringInterval properties
- **Health Statuses**: Healthy, Degraded, Unhealthy, Unknown status handling
- **Performance**: Large-scale plugin monitoring efficiency
- **Error Recovery**: Partial failures, plugin exceptions during health checks

## Test Quality Standards

### xUnit.v3 Compliance
- **No Underscores**: All test method names use PascalCase
- **One Thing Per Test**: Each test method validates a single behavior
- **Cancellation Tokens**: Always use `TestContext.Current.CancellationToken`
- **Test Output**: Comprehensive WriteLine statements for debugging failures

### Shouldly Assertions
- **Readable Failures**: All assertions use Shouldly for clear error messages
- **Type Safety**: Proper type checking with ShouldBeOfType, ShouldBeAssignableTo
- **Collection Assertions**: ShouldContain, ShouldAllBe, ShouldNotBeEmpty
- **Exception Testing**: Should.ThrowAsync with proper exception validation

### Mocking Patterns
- **Moq Usage**: Comprehensive mocking of all dependencies
- **Setup Verification**: Mock.Verify() calls to ensure proper interaction
- **Return Value Testing**: Proper success/failure result testing
- **Async Patterns**: Correct async/await usage in all async tests

### Coverage Requirements
- **Branch Coverage**: 100% of all code paths tested
- **Exception Paths**: All error conditions covered
- **Edge Cases**: Boundary values, null inputs, extreme scenarios
- **Integration Points**: Cross-component interaction testing

## Railway-Oriented Programming Tests

All tests validate the Railway-oriented programming patterns used throughout the codebase:

### IGenericResult Pattern Testing
- **Success Results**: GenericResult.Success() and GenericResult.Success<T>(value)
- **Failure Results**: GenericResult.Failure() and GenericResult.Failure<T>(errorMessage)
- **Result Chaining**: Proper result composition and error propagation
- **Error Messages**: Meaningful error descriptions for debugging

### Result Validation Patterns
```csharp
// Success path testing
result.ShouldNotBeNull();
result.IsSuccess.ShouldBeTrue();
result.Value.ShouldBe(expectedValue);

// Failure path testing
result.ShouldNotBeNull();
result.IsFailure.ShouldBeTrue();
result.ErrorMessage.ShouldContain("expected error text");
```

## Performance Testing

### Timing Constraints
- **Fast Operations**: < 50ms for simple operations
- **Medium Operations**: < 1000ms for complex operations
- **Bulk Operations**: < 5000ms for large-scale operations
- **Timeout Testing**: Proper cancellation token handling

### Memory Testing
- **Large Collections**: 10,000+ item handling
- **Memory Pressure**: Large object scenarios
- **Resource Cleanup**: Proper disposal patterns

## Configuration Testing

### Validation Scenarios
- **Valid Configurations**: All required properties set correctly
- **Invalid Configurations**: Missing/incorrect values handled gracefully
- **Boundary Values**: Min/max values for numeric properties
- **Null Handling**: Graceful handling of missing configuration sections

### Configuration Sources
- **IConfiguration**: Microsoft configuration abstractions
- **Section Binding**: Proper section name mapping
- **Validation Rules**: Custom validation logic testing

## Integration Testing Patterns

### Service Registration Verification
- **DI Container**: Proper service lifetime management
- **Factory Patterns**: Service factory implementations
- **Plugin Discovery**: Automatic plugin registration
- **Health Monitoring**: Background service registration

### Cross-Component Testing
- **Plugin → Tool**: Plugin provides tools correctly
- **Registry → Loader**: Registry uses loader for plugin discovery
- **Monitor → Plugin**: Health monitoring accesses plugin health
- **Service → Connection**: Service types register connection types

## Error Handling Validation

### Exception Types
- **ArgumentException**: Invalid parameters
- **InvalidOperationException**: Invalid state transitions
- **OperationCancelledException**: Cancellation token support
- **Custom Exceptions**: Domain-specific error types

### Error Recovery
- **Graceful Degradation**: Partial failure handling
- **Retry Patterns**: Transient failure recovery
- **Circuit Breaker**: Health monitoring integration
- **Logging Integration**: Proper error logging patterns

## Thread Safety Testing

### Concurrent Access
- **Singleton Patterns**: Thread-safe singleton implementations
- **Collection Access**: Thread-safe collection operations
- **State Management**: Concurrent state modification
- **Resource Sharing**: Shared resource access patterns

### Performance Under Load
- **Concurrent Registration**: Multiple plugins registering simultaneously
- **Parallel Execution**: Concurrent tool execution
- **Health Monitoring**: Concurrent health check operations
- **Plugin Loading**: Parallel plugin discovery and loading

## Test Execution Guidelines

### Running Tests
```bash
# Run all tests
dotnet test

# Run specific test class
dotnet test --filter "ClassName=IToolPluginTests"

# Run with verbose output
dotnet test --logger "console;verbosity=detailed"

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"
```

### Test Categories
Tests can be filtered by category attributes:
- `[Trait("Category", "Unit")]` - Unit tests
- `[Trait("Category", "Integration")]` - Integration tests
- `[Trait("Category", "EdgeCase")]` - Edge case tests
- `[Trait("Category", "Performance")]` - Performance tests

## Continuous Integration

### Build Requirements
- All tests must pass
- No compiler warnings
- Code coverage > 95%
- Performance tests within thresholds

### Quality Gates
- Static analysis clean
- No security vulnerabilities
- Documentation coverage complete
- API breaking change validation

This comprehensive test suite ensures the MCP plugin architecture is robust, reliable, and maintainable across all scenarios and use cases.