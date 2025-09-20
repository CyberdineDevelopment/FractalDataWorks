# Documentation vs Code Discrepancies

## FractalDataWorks.Services.Abstractions

### Interface Name Wrong
**Location**: src/FractalDataWorks.Services.Abstractions/README.md lines 16-31
**Documentation says**: `IFractalService` and `IFractalService<TCommand>`
**Actual code** (IRecService.cs): Interface is actually named `IFdwService` and `IFdwService<TCommand>`

### ICommand Interface
**Location**: src/FractalDataWorks.Services.Abstractions/README.md line 49
**Documentation says**: `IEnumerable<string> Validate();`
**Actual code** (ICommand.cs line 37): `IFdwResult<ValidationResult> Validate();`

### ICommand Configuration Property
**Location**: src/FractalDataWorks.Services.Abstractions/README.md
**Documentation**: Does not mention Configuration property
**Actual code** (ICommand.cs line 31): `IFdwConfiguration? Configuration { get; }`

### IFractalResult vs IFdwResult
**Location**: src/FractalDataWorks.Services.Abstractions/README.md line 30
**Documentation says**: `Task<IFractalResult<TResult>>`
**Actual code** (IRecService.cs line 100): `Task<IFdwResult>` and line 144: `Task<IFdwResult<T>>`

## FractalDataWorks.Services

### ServiceBase Validation Pipeline
**Location**: src/FractalDataWorks.Services/README.md lines 734-781
**Documentation says**: ServiceBase has ValidateCommand method, command validation pipeline, configuration validation
**Actual code** (ServiceBase.cs): NO ValidateCommand method, NO validation pipeline, just abstract Execute methods

### ServiceBase Logger Property
**Location**: src/FractalDataWorks.Services/README.md
**Documentation says**: Protected Logger property available
**Actual code** (ServiceBase.cs line 106): CORRECT - has `protected ILogger<TService> Logger`

### ServiceMessages Usage in ServiceBase
**Location**: src/FractalDataWorks.Services/README.md lines 269-288
**Documentation says**: ServiceBase uses ServiceMessages for validation errors
**Actual code** (ServiceBase.cs): NO ServiceMessages usage at all, no validation

### ServiceFactoryBase
**Location**: src/FractalDataWorks.Services/README.md
**Documentation**: Not clearly described
**Actual code** (ServiceFactoryBase.cs): DOES use ServiceMessages.ConfigurationCannotBeNull() on lines 62 and 114

### IFractalService vs IFdwService
**Location**: src/FractalDataWorks.Services/README.md line 241
**Documentation says**: ServiceBase implements `IFractalService<TCommand>`
**Actual code** (ServiceBase.cs line 20): Actually implements `IFdwService<TCommand, TConfiguration, TService>`

## CRITICAL ISSUE: Source Generators Not Working

### Source Generator DLL Locking
**Location**: Build output
**Problem**: RoslynMcpServer (PID 15872) is locking the source generator DLLs
**Files Locked**:
- FractalDataWorks.Messages.SourceGenerators.dll
- FractalDataWorks.EnhancedEnums.SourceGenerators.dll
- FractalDataWorks.Collections.SourceGenerators.dll
- FractalDataWorks.ServiceTypes.SourceGenerators.dll

**Impact**: Source generators CANNOT run, which means:
- ServiceMessages collection factory methods NOT generated
- ConnectionTypes.All property NOT generated
- ServiceTypes discovery methods NOT generated
- The entire plugin architecture described in READMEs DOES NOT WORK

## FractalDataWorks.Services BUILD ERRORS

### Wrong Namespace Import
**Location**: src/FractalDataWorks.Services/Extensions/ServiceFactoryRegistrationExtensions.cs line 4
**Code has**: `using FractalDataWorks.Services.Connections.Abstractions;`
**Should be**: Project doesn't exist under Services namespace - Connections.Abstractions is a separate project

### Generic Constraint Errors
**Location**: src/FractalDataWorks.Services/IServiceFactoryProvider.cs line 48
**Error**: TConfiguration cannot be used as type parameter - missing IFdwConfiguration constraint

### ServiceLifetime.Name() Method Doesn't Exist
**Location**: src/FractalDataWorks.Services/Extensions/ServiceFactoryRegistrationExtensions.cs line 131
**Code has**: `ServiceLifetime.Name(lifetimeName)`
**Problem**: ServiceLifetime enum doesn't have a Name() method

### THE SERVICES PROJECT DOESN'T EVEN COMPILE