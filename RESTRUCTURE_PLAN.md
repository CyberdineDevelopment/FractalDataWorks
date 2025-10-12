# Architecture Restructure Plan - Detailed Execution Checklist

**Created:** 2025-10-10
**Status:** NOT STARTED
**Estimated Tasks:** 100+

---

## 🎯 Goals

1. Eliminate circular dependencies
2. Align project structure with architecture
3. Support source generators properly
4. Clean separation of concerns
5. Proper namespace hierarchy

---

## Core Architectural Principle

**Foundation types (used by source generators) MUST live in `FractalDataWorks.Abstractions` to avoid circular dependencies.**

This is the same pattern as `IGenericService` - it's in `FractalDataWorks.Abstractions` for source generator access.

---

# PHASE 1: Foundation Layer - Update FractalDataWorks.Abstractions

## Step 1.1: Add IGenericCommand to FractalDataWorks.Abstractions

- [x] Create file `src\FractalDataWorks.Abstractions\IGenericCommand.cs`
- [x] Add interface definition with CommandId, CreatedAt, CommandType properties
- [x] Use namespace `FractalDataWorks.Abstractions`
- [x] Add XML documentation comments
- [x] Verify file compiles

**File Content:**
```csharp
using System;

namespace FractalDataWorks.Abstractions;

/// <summary>
/// Base interface for all commands in the FractalDataWorks framework.
/// </summary>
/// <remarks>
/// This interface is in FractalDataWorks.Abstractions to avoid circular dependencies
/// with source generators, following the same pattern as IGenericService.
/// </remarks>
public interface IGenericCommand
{
    /// <summary>
    /// Gets the unique identifier for this command instance.
    /// </summary>
    Guid CommandId { get; }

    /// <summary>
    /// Gets when this command was created.
    /// </summary>
    DateTime CreatedAt { get; }

    /// <summary>
    /// Gets the command type identifier.
    /// </summary>
    string CommandType { get; }
}
```

## Step 1.2: Verify Existing Foundation Types

- [x] Verify `IGenericConfiguration.cs` exists and is correct
- [x] Verify `IGenericResult.cs` exists and is correct
- [x] Verify `IGenericMessage.cs` exists and is correct
- [x] Verify `IEnumOption.cs` exists (if present)
- [x] Document current state of FractalDataWorks.Abstractions

**DO NOT DELETE** any existing files in FractalDataWorks.Abstractions

**Found:** IGenericConfiguration.cs, IGenericResult.cs, IGenericMessage.cs, IEnumOption.cs, IGenericCommand.cs (new)

## Step 1.3: Move IServiceFactory to Root

- [x] Locate `src\FractalDataWorks.Abstractions\Services\Abstractions\IServiceFactory.cs`
- [x] Copy to `src\FractalDataWorks.Abstractions\IServiceFactory.cs`
- [x] Update namespace from `FractalDataWorks.Services.Abstractions` to `FractalDataWorks.Abstractions`
- [x] Update using statements if needed
- [x] Find all references to old namespace
- [x] Update all references to new namespace (none found)
- [x] Verify builds
- [ ] Delete old file location (will be done in Phase 2.4)

---

# PHASE 2: Delete Broken/Duplicate Files and Projects

## Step 2.1: Remove FractalDataWorks.Commands.Abstractions Project

- [x] Backup project directory to `../BACKUP/FractalDataWorks.Commands.Abstractions`
- [x] Remove project from solution file `FractalDataWorks.DeveloperKit.sln`
- [x] Find all projects referencing `FractalDataWorks.Commands.Abstractions`
- [x] Document dependent projects (before deletion)
  - Found: FractalDataWorks.Services.Abstractions
  - Found: FractalDataWorks.Data.Sql
  - Found: samples\FractalDataWorks.Data.Sql.Sample
- [x] Delete directory `src\FractalDataWorks.Commands.Abstractions\`
- [x] Verify solution loads without errors (will have missing references)

## Step 2.2: Remove Duplicate IGenericService

- [x] Verify `src\FractalDataWorks.Services.Abstractions\IGenericService.cs` exists (KEEP this one)
- [x] Backup `src\FractalDataWorks.Abstractions\Services\Abstractions\IGenericService.cs` (in ../BACKUP)
- [x] Delete `src\FractalDataWorks.Abstractions\Services\Abstractions\IGenericService.cs`
- [x] Verify no compilation errors

## Step 2.3: Remove Commands Folder from FractalDataWorks.Abstractions

- [x] Backup entire `src\FractalDataWorks.Abstractions\Services\Abstractions\Commands\` folder (in ../BACKUP)
- [x] Delete `src\FractalDataWorks.Abstractions\Services\Abstractions\Commands\` folder
- [x] Verify no other code depends on this location

## Step 2.4: Remove Services Subdirectory

- [x] After IServiceFactory is moved, verify `src\FractalDataWorks.Abstractions\Services\` is empty
- [x] Delete `src\FractalDataWorks.Abstractions\Services\` directory
- [x] Verify FractalDataWorks.Abstractions has clean structure

**Final FractalDataWorks.Abstractions structure:** IEnumOption.cs, IGenericCommand.cs, IGenericConfiguration.cs, IGenericMessage.cs, IGenericResult.cs, IServiceFactory.cs

---

# PHASE 3: Rename Projects to Proper Namespace

## Step 3.1: Rename DataStores.Abstractions

### 3.1.1: FractalDataWorks.DataStores.Abstractions → FractalDataWorks.Data.DataStores.Abstractions

- [ ] Create new directory `src\FractalDataWorks.Data.DataStores.Abstractions\`
- [ ] Copy all files from `src\FractalDataWorks.DataStores.Abstractions\` to new location
- [ ] Rename csproj: `FractalDataWorks.DataStores.Abstractions.csproj` → `FractalDataWorks.Data.DataStores.Abstractions.csproj`
- [ ] Update `<RootNamespace>` in csproj to `FractalDataWorks.Data.DataStores.Abstractions`
- [ ] Update `<AssemblyName>` in csproj to `FractalDataWorks.Data.DataStores.Abstractions`
- [ ] Find all `.cs` files in project
- [ ] Update namespace in each `.cs` file: `namespace FractalDataWorks.DataStores.Abstractions` → `namespace FractalDataWorks.Data.DataStores.Abstractions`
- [ ] Update all using statements in `.cs` files
- [ ] Add renamed project to solution file
- [ ] Remove old project from solution file
- [ ] Build new project to verify
- [ ] Delete old directory `src\FractalDataWorks.DataStores.Abstractions\`

### 3.1.2: FractalDataWorks.DataStores.SqlServer → FractalDataWorks.Data.DataStores.SqlServer

- [ ] Create new directory `src\FractalDataWorks.Data.DataStores.SqlServer\`
- [ ] Copy all files from `src\FractalDataWorks.DataStores.SqlServer\` to new location
- [ ] Rename csproj to `FractalDataWorks.Data.DataStores.SqlServer.csproj`
- [ ] Update `<RootNamespace>` to `FractalDataWorks.Data.DataStores.SqlServer`
- [ ] Update `<AssemblyName>` to `FractalDataWorks.Data.DataStores.SqlServer`
- [ ] Update all namespace declarations in `.cs` files
- [ ] Update project reference to `FractalDataWorks.Data.DataStores.Abstractions`
- [ ] Update using statements
- [ ] Add to solution file
- [ ] Remove old project from solution
- [ ] Build to verify
- [ ] Delete old directory

### 3.1.3: FractalDataWorks.DataStores.FileSystem → FractalDataWorks.Data.DataStores.FileSystem

- [ ] Create new directory `src\FractalDataWorks.Data.DataStores.FileSystem\`
- [ ] Copy all files from `src\FractalDataWorks.DataStores.FileSystem\`
- [ ] Rename csproj to `FractalDataWorks.Data.DataStores.FileSystem.csproj`
- [ ] Update `<RootNamespace>` to `FractalDataWorks.Data.DataStores.FileSystem`
- [ ] Update `<AssemblyName>` to `FractalDataWorks.Data.DataStores.FileSystem`
- [ ] Update all namespaces in `.cs` files
- [ ] Update project references
- [ ] Update using statements
- [ ] Add to solution
- [ ] Remove old project
- [ ] Build to verify
- [ ] Delete old directory

### 3.1.4: FractalDataWorks.DataStores.Rest → FractalDataWorks.Data.DataStores.Rest

- [ ] Create new directory `src\FractalDataWorks.Data.DataStores.Rest\`
- [ ] Copy all files from `src\FractalDataWorks.DataStores.Rest\`
- [ ] Rename csproj to `FractalDataWorks.Data.DataStores.Rest.csproj`
- [ ] Update `<RootNamespace>` to `FractalDataWorks.Data.DataStores.Rest`
- [ ] Update `<AssemblyName>` to `FractalDataWorks.Data.DataStores.Rest`
- [ ] Update all namespaces in `.cs` files
- [ ] Update project references
- [ ] Update using statements
- [ ] Add to solution
- [ ] Remove old project
- [ ] Build to verify
- [ ] Delete old directory

### 3.1.5: FractalDataWorks.DataStores → FractalDataWorks.Data.DataStores (if exists)

- [ ] Check if `src\FractalDataWorks.DataStores\` directory exists
- [ ] If exists: Create new directory `src\FractalDataWorks.Data.DataStores\`
- [ ] If exists: Copy all files
- [ ] If exists: Rename csproj
- [ ] If exists: Update RootNamespace and AssemblyName
- [ ] If exists: Update namespaces in all `.cs` files
- [ ] If exists: Update references
- [ ] If exists: Add to solution, remove old, build, delete old
- [ ] If not exists: Mark as N/A

## Step 3.2: Rename DataContainers Projects

### 3.2.1: FractalDataWorks.DataContainers.Abstractions → FractalDataWorks.Data.DataContainers.Abstractions

- [ ] Create new directory `src\FractalDataWorks.Data.DataContainers.Abstractions\`
- [ ] Copy all files from `src\FractalDataWorks.DataContainers.Abstractions\`
- [ ] Rename csproj to `FractalDataWorks.Data.DataContainers.Abstractions.csproj`
- [ ] Update `<RootNamespace>` to `FractalDataWorks.Data.DataContainers.Abstractions`
- [ ] Update `<AssemblyName>` to `FractalDataWorks.Data.DataContainers.Abstractions`
- [ ] Update namespaces in all `.cs` files
- [ ] Update project references to Data.DataStores.Abstractions
- [ ] Update using statements
- [ ] Add to solution
- [ ] Remove old project
- [ ] Build to verify
- [ ] Delete old directory

## Step 3.3: Rename DataSets Projects

### 3.3.1: FractalDataWorks.DataSets.Abstractions → FractalDataWorks.Data.DataSets.Abstractions

- [ ] Create new directory `src\FractalDataWorks.Data.DataSets.Abstractions\`
- [ ] Copy all files from `src\FractalDataWorks.DataSets.Abstractions\`
- [ ] Rename csproj to `FractalDataWorks.Data.DataSets.Abstractions.csproj`
- [ ] Update `<RootNamespace>` to `FractalDataWorks.Data.DataSets.Abstractions`
- [ ] Update `<AssemblyName>` to `FractalDataWorks.Data.DataSets.Abstractions`
- [ ] Update namespaces in all `.cs` files
- [ ] Update project references
- [ ] Update using statements
- [ ] Add to solution
- [ ] Remove old project
- [ ] Build to verify
- [ ] Delete old directory

---

# PHASE 4: Create New Projects

## Step 4.1: Create FractalDataWorks.Commands.Abstractions (Generic Pattern)

### 4.1.1: Create Project Structure

- [ ] Create directory `src\FractalDataWorks.Commands.Abstractions\`
- [ ] Create csproj file `FractalDataWorks.Commands.Abstractions.csproj`
- [ ] Set TargetFramework to netstandard2.0
- [ ] Set RootNamespace to `FractalDataWorks.Commands.Abstractions`
- [ ] Set AssemblyName to `FractalDataWorks.Commands.Abstractions`
- [ ] Enable nullable reference types
- [ ] Add ProjectReference to `FractalDataWorks.Abstractions`
- [ ] Add to solution file

**Project File:**
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>netstandard2.0</TargetFramework>
    <RootNamespace>FractalDataWorks.Commands.Abstractions</RootNamespace>
    <AssemblyName>FractalDataWorks.Commands.Abstractions</AssemblyName>
    <Nullable>enable</Nullable>
  </PropertyGroup>

  <ItemGroup>
    <ProjectReference Include="..\FractalDataWorks.Abstractions\FractalDataWorks.Abstractions.csproj" />
  </ItemGroup>
</Project>
```

### 4.1.2: Create ICommandType.cs

- [ ] Create file `src\FractalDataWorks.Commands.Abstractions\ICommandType.cs`
- [ ] Add using statements for `FractalDataWorks.Abstractions`
- [ ] Define interface extending `IEnumOption`
- [ ] Add properties: SupportsTransactions, IsIdempotent
- [ ] Add XML documentation
- [ ] Verify builds

### 4.1.3: Create CommandTypeBase.cs

- [ ] Create file `src\FractalDataWorks.Commands.Abstractions\CommandTypeBase.cs`
- [ ] Implement ICommandType
- [ ] Add constructor with id, name, description
- [ ] Add abstract properties for SupportsTransactions, IsIdempotent
- [ ] Add XML documentation
- [ ] Verify builds

### 4.1.4: Create ICommandTranslator.cs

- [ ] Create file `src\FractalDataWorks.Commands.Abstractions\ICommandTranslator.cs`
- [ ] Add using for System.Linq.Expressions
- [ ] Define interface with Translate method
- [ ] Add SourceFormat and TargetFormat properties
- [ ] Add XML documentation
- [ ] Verify builds

### 4.1.5: Create ITranslatorType.cs

- [ ] Create file `src\FractalDataWorks.Commands.Abstractions\ITranslatorType.cs`
- [ ] Define interface extending IEnumOption
- [ ] Add SourceFormat and TargetFormat properties
- [ ] Add XML documentation
- [ ] Verify builds

### 4.1.6: Create TranslatorTypeBase.cs

- [ ] Create file `src\FractalDataWorks.Commands.Abstractions\TranslatorTypeBase.cs`
- [ ] Implement ITranslatorType
- [ ] Add constructor with all parameters
- [ ] Add XML documentation
- [ ] Verify builds

### 4.1.7: Create ITranslationContext.cs

- [ ] Create file `src\FractalDataWorks.Commands.Abstractions\ITranslationContext.cs`
- [ ] Define interface with DataSource and Metadata properties
- [ ] Add XML documentation
- [ ] Verify builds

### 4.1.8: Create CommandTypes.cs

- [ ] Create file `src\FractalDataWorks.Commands.Abstractions\CommandTypes.cs`
- [ ] Add [TypeCollection] attribute
- [ ] Define as static partial class
- [ ] Add XML documentation noting it's source-generated
- [ ] Verify builds

### 4.1.9: Create TranslatorTypes.cs

- [ ] Create file `src\FractalDataWorks.Commands.Abstractions\TranslatorTypes.cs`
- [ ] Add [TypeCollection] attribute
- [ ] Define as static partial class
- [ ] Add XML documentation
- [ ] Verify builds

### 4.1.10: Build and Verify

- [ ] Build FractalDataWorks.Commands.Abstractions project
- [ ] Verify no errors
- [ ] Verify no warnings
- [ ] Check source generator runs (if applicable)

## Step 4.2: Create FractalDataWorks.Data.Commands.Abstractions

### 4.2.1: Create Project Structure

- [ ] Create directory `src\FractalDataWorks.Data.Commands.Abstractions\`
- [ ] Create csproj file `FractalDataWorks.Data.Commands.Abstractions.csproj`
- [ ] Set TargetFramework to netstandard2.0
- [ ] Set RootNamespace to `FractalDataWorks.Data.Commands.Abstractions`
- [ ] Set AssemblyName to `FractalDataWorks.Data.Commands.Abstractions`
- [ ] Enable nullable
- [ ] Add ProjectReference to `FractalDataWorks.Abstractions`
- [ ] Add ProjectReference to `FractalDataWorks.Commands.Abstractions`
- [ ] Add ProjectReference to `FractalDataWorks.Data.Abstractions`
- [ ] Add ProjectReference to `FractalDataWorks.Data.DataSets.Abstractions`
- [ ] Add to solution file
- [ ] Verify project references resolve

**Project File:**
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>netstandard2.0</TargetFramework>
    <RootNamespace>FractalDataWorks.Data.Commands.Abstractions</RootNamespace>
    <AssemblyName>FractalDataWorks.Data.Commands.Abstractions</AssemblyName>
    <Nullable>enable</Nullable>
  </PropertyGroup>

  <ItemGroup>
    <ProjectReference Include="..\FractalDataWorks.Abstractions\FractalDataWorks.Abstractions.csproj" />
    <ProjectReference Include="..\FractalDataWorks.Commands.Abstractions\FractalDataWorks.Commands.Abstractions.csproj" />
    <ProjectReference Include="..\FractalDataWorks.Data.Abstractions\FractalDataWorks.Data.Abstractions.csproj" />
    <ProjectReference Include="..\FractalDataWorks.Data.DataSets.Abstractions\FractalDataWorks.Data.DataSets.Abstractions.csproj" />
  </ItemGroup>
</Project>
```

### 4.2.2: Create IDataCommand.cs

- [ ] Create file `src\FractalDataWorks.Data.Commands.Abstractions\IDataCommand.cs`
- [ ] Add using statements (System, System.Linq.Expressions, FractalDataWorks.Abstractions, FluentValidation)
- [ ] Define interface extending IGenericCommand
- [ ] Add QueryExpression property (Expression type)
- [ ] Add DataSource, TargetSchema, RequiresTransaction, TimeoutMs properties
- [ ] Add Validate() method
- [ ] Add XML documentation emphasizing Expression not SQL text
- [ ] Verify builds

### 4.2.3: Create IQueryCommand.cs

- [ ] Create file `src\FractalDataWorks.Data.Commands.Abstractions\IQueryCommand.cs`
- [ ] Define interface extending IDataCommand
- [ ] Add SelectFields, FilterCriteria, OrderBy, Skip, Take properties
- [ ] Add IncludeRelations, IsCacheable, CacheDurationSeconds properties
- [ ] Add XML documentation
- [ ] Verify builds

### 4.2.4: Create IMutationCommand.cs

- [ ] Create file `src\FractalDataWorks.Data.Commands.Abstractions\IMutationCommand.cs`
- [ ] Define interface extending IDataCommand
- [ ] Add Operation, Data, FilterCriteria, UpdateFields properties
- [ ] Add ConflictStrategy, ReturnAffectedRecords, ValidateConstraints properties
- [ ] Add XML documentation
- [ ] Verify builds

### 4.2.5: Create IBulkCommand.cs

- [ ] Create file `src\FractalDataWorks.Data.Commands.Abstractions\IBulkCommand.cs`
- [ ] Define interface extending IDataCommand
- [ ] Add Operation, Items, BatchSize properties
- [ ] Add ContinueOnError, PreValidate, ParallelOptions, ProgressCallback properties
- [ ] Add XML documentation
- [ ] Verify builds

### 4.2.6: Create Supporting Interface Files

- [ ] Locate old Commands.Abstractions backup for reference
- [ ] Create `IOrderSpecification.cs`
- [ ] Create `IMutationOperation.cs`
- [ ] Create `IConflictStrategy.cs`
- [ ] Create `ConflictStrategyBase.cs`
- [ ] Create `ConflictStrategies.cs`
- [ ] Create `ConflictResolution.cs`
- [ ] Create `IBulkOperation.cs`
- [ ] Create `IParallelOptions.cs`
- [ ] Create `IProgressCallback.cs`
- [ ] Update all namespaces to `FractalDataWorks.Data.Commands.Abstractions`
- [ ] Verify all files build

### 4.2.7: Build and Verify

- [ ] Build FractalDataWorks.Data.Commands.Abstractions project
- [ ] Verify no errors
- [ ] Verify no warnings
- [ ] Check all dependencies resolve

---

# PHASE 5: Move Schema Types from Old Commands.Abstractions

## Step 5.1: Move IDataSchema to FractalDataWorks.Data.Abstractions

- [ ] Locate `IDataSchema.cs` in Commands.Abstractions backup
- [ ] Copy to `src\FractalDataWorks.Data.Abstractions\IDataSchema.cs`
- [ ] Update namespace to `FractalDataWorks.Data.Abstractions`
- [ ] Update using statements
- [ ] Verify builds

## Step 5.2: Move ISchemaField to FractalDataWorks.Data.Abstractions

- [ ] Locate `ISchemaField.cs` in backup
- [ ] Copy to `src\FractalDataWorks.Data.Abstractions\ISchemaField.cs`
- [ ] Update namespace to `FractalDataWorks.Data.Abstractions`
- [ ] Update using statements
- [ ] Verify builds

## Step 5.3: Move ISchemaConstraint to FractalDataWorks.Data.Abstractions

- [ ] Locate `ISchemaConstraint.cs` in backup
- [ ] Copy to `src\FractalDataWorks.Data.Abstractions\ISchemaConstraint.cs`
- [ ] Update namespace to `FractalDataWorks.Data.Abstractions`
- [ ] Update using statements
- [ ] Verify builds

## Step 5.4: Update FractalDataWorks.Data.Abstractions Project

- [ ] Build FractalDataWorks.Data.Abstractions
- [ ] Verify schema types compile
- [ ] Verify no errors

---

# PHASE 6: Update All Project References

## Step 6.1: Find All Old FractalDataWorks.Commands.Abstractions References

- [ ] Run grep: `grep -r "FractalDataWorks\.Commands\.Abstractions" **/*.csproj`
- [ ] Document all files with references
- [ ] Create list of projects to update

### Expected Projects:
1. FractalDataWorks.Services.Abstractions
2. FractalDataWorks.Data.Sql
3. samples\FractalDataWorks.Data.Sql.Sample

## Step 6.2: Update FractalDataWorks.Services.Abstractions

- [ ] Open `src\FractalDataWorks.Services.Abstractions\FractalDataWorks.Services.Abstractions.csproj`
- [ ] Remove old Commands.Abstractions reference (if exists)
- [ ] Add reference to `FractalDataWorks.Data.Commands.Abstractions` (if needed)
- [ ] Save file
- [ ] Build to verify

## Step 6.3: Update FractalDataWorks.Data.Sql

- [ ] Open `src\FractalDataWorks.Data.Sql\FractalDataWorks.Data.Sql.csproj`
- [ ] Remove old `FractalDataWorks.Commands.Abstractions` reference
- [ ] Add reference to `FractalDataWorks.Commands.Abstractions` (new)
- [ ] Add reference to `FractalDataWorks.Data.Commands.Abstractions`
- [ ] Save file
- [ ] Update all `.cs` files with new using statements
- [ ] Build to verify

## Step 6.4: Update Sample Projects

- [ ] Open `samples\FractalDataWorks.Data.Sql.Sample\FractalDataWorks.Data.Sql.Sample.csproj`
- [ ] Remove old Commands.Abstractions reference
- [ ] Add new references as needed
- [ ] Update using statements in sample code
- [ ] Build to verify

## Step 6.5: Find All Renamed Project References

- [ ] Run: `grep -r "FractalDataWorks\.DataStores\." **/*.csproj`
- [ ] Run: `grep -r "FractalDataWorks\.DataContainers\." **/*.csproj`
- [ ] Run: `grep -r "FractalDataWorks\.DataSets\." **/*.csproj`
- [ ] Document all files to update
- [ ] Create update list

## Step 6.6: Update DataStores References

For each project with old DataStores references:
- [ ] Replace `FractalDataWorks.DataStores.Abstractions` → `FractalDataWorks.Data.DataStores.Abstractions`
- [ ] Replace `FractalDataWorks.DataStores.SqlServer` → `FractalDataWorks.Data.DataStores.SqlServer`
- [ ] Replace `FractalDataWorks.DataStores.FileSystem` → `FractalDataWorks.Data.DataStores.FileSystem`
- [ ] Replace `FractalDataWorks.DataStores.Rest` → `FractalDataWorks.Data.DataStores.Rest`
- [ ] Save and build each project

## Step 6.7: Update DataContainers References

- [ ] Find all references to `FractalDataWorks.DataContainers.Abstractions`
- [ ] Replace with `FractalDataWorks.Data.DataContainers.Abstractions`
- [ ] Save and build

## Step 6.8: Update DataSets References

- [ ] Find all references to `FractalDataWorks.DataSets.Abstractions`
- [ ] Replace with `FractalDataWorks.Data.DataSets.Abstractions`
- [ ] Save and build

## Step 6.9: Update Using Statements in C# Files

### Find and Replace Old Commands Namespace

- [ ] Run: `grep -r "using FractalDataWorks\.Commands\.Abstractions;" **/*.cs`
- [ ] For data command files: Replace with `using FractalDataWorks.Data.Commands.Abstractions;`
- [ ] For generic command files: Replace with `using FractalDataWorks.Commands.Abstractions;`
- [ ] Verify each file context before replacing

### Update DataStores Using Statements

- [ ] Run: `grep -r "using FractalDataWorks\.DataStores" **/*.cs`
- [ ] Replace `using FractalDataWorks.DataStores.Abstractions;` → `using FractalDataWorks.Data.DataStores.Abstractions;`
- [ ] Replace `using FractalDataWorks.DataStores.SqlServer;` → `using FractalDataWorks.Data.DataStores.SqlServer;`
- [ ] Replace `using FractalDataWorks.DataStores.FileSystem;` → `using FractalDataWorks.Data.DataStores.FileSystem;`
- [ ] Replace `using FractalDataWorks.DataStores.Rest;` → `using FractalDataWorks.Data.DataStores.Rest;`

### Update DataContainers Using Statements

- [ ] Run: `grep -r "using FractalDataWorks\.DataContainers" **/*.cs`
- [ ] Replace `using FractalDataWorks.DataContainers.Abstractions;` → `using FractalDataWorks.Data.DataContainers.Abstractions;`

### Update DataSets Using Statements

- [ ] Run: `grep -r "using FractalDataWorks\.DataSets" **/*.cs`
- [ ] Replace `using FractalDataWorks.DataSets.Abstractions;` → `using FractalDataWorks.Data.DataSets.Abstractions;`

## Step 6.10: Special Case - FractalDataWorks.Data.Sql

- [ ] Open `src\FractalDataWorks.Data.Sql\Commands\SqlQueryCommand.cs`
- [ ] Verify namespace is `FractalDataWorks.Data.Sql.Commands`
- [ ] Update using statements:
  - `using FractalDataWorks.Abstractions;`
  - `using FractalDataWorks.Data.Commands.Abstractions;`
  - `using FractalDataWorks.Data.Abstractions;`
- [ ] Verify SqlQueryCommand implements IQueryCommand
- [ ] Check all other files in FractalDataWorks.Data.Sql
- [ ] Update translators to reference new namespaces
- [ ] Build to verify

---

# PHASE 7: Update Solution File

## Step 7.1: Remove Deleted Projects from Solution

- [ ] Open `FractalDataWorks.DeveloperKit.sln` in text editor
- [ ] Find and remove `FractalDataWorks.Commands.Abstractions` (old) entry
- [ ] Find and remove `FractalDataWorks.DataStores.Abstractions` entry
- [ ] Find and remove `FractalDataWorks.DataStores.SqlServer` entry
- [ ] Find and remove `FractalDataWorks.DataStores.FileSystem` entry
- [ ] Find and remove `FractalDataWorks.DataStores.Rest` entry
- [ ] Find and remove `FractalDataWorks.DataStores` entry (if exists)
- [ ] Find and remove `FractalDataWorks.DataContainers.Abstractions` entry
- [ ] Find and remove `FractalDataWorks.DataSets.Abstractions` entry
- [ ] Save solution file

## Step 7.2: Add New/Renamed Projects to Solution

- [ ] Run: `dotnet sln FractalDataWorks.DeveloperKit.sln add src\FractalDataWorks.Commands.Abstractions\FractalDataWorks.Commands.Abstractions.csproj`
- [ ] Run: `dotnet sln FractalDataWorks.DeveloperKit.sln add src\FractalDataWorks.Data.Commands.Abstractions\FractalDataWorks.Data.Commands.Abstractions.csproj`
- [ ] Run: `dotnet sln add src\FractalDataWorks.Data.DataStores.Abstractions\FractalDataWorks.Data.DataStores.Abstractions.csproj`
- [ ] Run: `dotnet sln add src\FractalDataWorks.Data.DataStores.SqlServer\FractalDataWorks.Data.DataStores.SqlServer.csproj`
- [ ] Run: `dotnet sln add src\FractalDataWorks.Data.DataStores.FileSystem\FractalDataWorks.Data.DataStores.FileSystem.csproj`
- [ ] Run: `dotnet sln add src\FractalDataWorks.Data.DataStores.Rest\FractalDataWorks.Data.DataStores.Rest.csproj`
- [ ] Run: `dotnet sln add src\FractalDataWorks.Data.DataContainers.Abstractions\FractalDataWorks.Data.DataContainers.Abstractions.csproj`
- [ ] Run: `dotnet sln add src\FractalDataWorks.Data.DataSets.Abstractions\FractalDataWorks.Data.DataSets.Abstractions.csproj`
- [ ] Verify solution loads in Visual Studio/Rider

## Step 7.3: Organize Solution Folders

- [ ] Open solution in IDE
- [ ] Create/verify solution folder structure:
  - Data/
  - Data/Abstractions/
  - Data/Commands/
  - Data/DataStores/
  - Commands/
- [ ] Move projects to appropriate folders
- [ ] Save solution

## Step 7.4: Verify Solution Structure

- [ ] Open solution in IDE
- [ ] Verify all projects load
- [ ] Verify folder structure is clean
- [ ] Close and reopen solution to verify

---

# PHASE 8: Roslyn Analyzer Verification

## Step 8.1: Build Solution

- [ ] Run: `dotnet clean FractalDataWorks.DeveloperKit.sln`
- [ ] Run: `dotnet restore FractalDataWorks.DeveloperKit.sln`
- [ ] Run: `dotnet build FractalDataWorks.DeveloperKit.sln --no-incremental`
- [ ] Document any errors
- [ ] Document any warnings
- [ ] If errors exist, fix before proceeding

## Step 8.2: Start Roslyn Analyzer Session

- [ ] Run mcp__roslyn-analyzer__start_session
- [ ] Note session ID: `__________________`
- [ ] Verify session starts successfully
- [ ] Document any startup errors

## Step 8.3: Check for Duplicate Types

- [ ] Run mcp__roslyn-analyzer__find_duplicate_types
- [ ] Review results for:
  - IGenericCommand duplicates
  - IDataCommand duplicates
  - IDataSchema duplicates
  - Any other duplicates
- [ ] Document duplicate types found
- [ ] Create plan to fix duplicates (if any)
- [ ] Fix any duplicates found
- [ ] Re-run check to verify fixes

## Step 8.4: Check for Missing Types

- [ ] Run mcp__roslyn-analyzer__find_missing_types
- [ ] Review all CS0246 errors
- [ ] Document missing types by file
- [ ] Group by type of fix needed
- [ ] Prioritize fixes

## Step 8.5: Suggest and Apply Using Statement Fixes

For each missing type:
- [ ] Run mcp__roslyn-analyzer__suggest_using_statements
- [ ] Review suggested namespaces
- [ ] Verify correct namespace
- [ ] Apply using statement or fix reference
- [ ] Build to verify fix

Bulk fix if many files:
- [ ] Prepare bulk_add_using_statements payload
- [ ] Run mcp__roslyn-analyzer__bulk_add_using_statements
- [ ] Verify changes
- [ ] Build to confirm

## Step 8.6: Get Full Diagnostic Summary

- [ ] Run mcp__roslyn-analyzer__get_diagnostics with includeAnalyzerDiagnostics: true
- [ ] Review error count (target: 0)
- [ ] Review warning count
- [ ] Review info messages
- [ ] Document any remaining issues
- [ ] Create fix plan for remaining issues

## Step 8.7: Get Detailed Diagnostics by Severity

- [ ] Run get_diagnostic_details for Errors
- [ ] Run get_diagnostic_details for Warnings
- [ ] Document top 10 issues
- [ ] Fix critical errors
- [ ] Re-run diagnostics
- [ ] Verify error count decreased

## Step 8.8: Verify Project Dependencies

- [ ] Run mcp__roslyn-analyzer__get_project_dependencies
- [ ] Review dependency graph
- [ ] Check for circular dependencies
- [ ] Verify proper layering:
  - FractalDataWorks.Abstractions has no dependencies
  - Commands.Abstractions depends only on FractalDataWorks.Abstractions
  - Data.Commands.Abstractions depends on Commands.Abstractions + Data abstractions
  - No circular references
- [ ] Document dependency structure

## Step 8.9: Check Impact Analysis

For each major project:
- [ ] Run mcp__roslyn-analyzer__get_impact_analysis
- [ ] Review which projects depend on it
- [ ] Verify expected dependency chain
- [ ] Document unexpected dependencies

## Step 8.10: End Roslyn Session

- [ ] Review all findings
- [ ] Ensure all critical issues fixed
- [ ] Run mcp__roslyn-analyzer__end_session
- [ ] Document session results

---

# PHASE 9: Final Verification Build

## Step 9.1: Clean Build

- [ ] Run: `dotnet clean FractalDataWorks.DeveloperKit.sln`
- [ ] Delete all bin/ directories
- [ ] Delete all obj/ directories
- [ ] Verify clean state

## Step 9.2: Restore and Build

- [ ] Run: `dotnet restore FractalDataWorks.DeveloperKit.sln`
- [ ] Verify all packages restore
- [ ] Run: `dotnet build FractalDataWorks.DeveloperKit.sln -c Debug`
- [ ] Document Debug build result
- [ ] Run: `dotnet build FractalDataWorks.DeveloperKit.sln -c Release`
- [ ] Document Release build result
- [ ] Verify 0 errors

## Step 9.3: Run Tests

- [ ] Run: `dotnet test FractalDataWorks.DeveloperKit.sln --no-build -c Debug`
- [ ] Document test results
- [ ] Verify all tests pass
- [ ] Run: `dotnet test FractalDataWorks.DeveloperKit.sln --no-build -c Release`
- [ ] Document test results
- [ ] Investigate any test failures

## Step 9.4: Verify Source Generators

- [ ] Build with diagnostic output: `dotnet build -v detailed`
- [ ] Check for source generator execution
- [ ] Verify CommandTypes.cs is generated
- [ ] Verify TranslatorTypes.cs is generated
- [ ] Verify ServiceTypes.cs still generates
- [ ] Check generated file contents

## Step 9.5: IDE Verification

- [ ] Open solution in Visual Studio (if available)
- [ ] Verify IntelliSense works
- [ ] Verify no red squiggles
- [ ] Build from IDE
- [ ] Run tests from IDE

Or:
- [ ] Open solution in Rider (if available)
- [ ] Verify IntelliSense works
- [ ] Build from Rider
- [ ] Run tests from Rider

## Step 9.6: Final Checklist

- [ ] All projects build successfully
- [ ] All tests pass
- [ ] No compilation errors
- [ ] No circular dependencies
- [ ] Source generators work
- [ ] Solution loads properly
- [ ] No duplicate types
- [ ] All using statements correct
- [ ] Namespaces aligned with architecture

---

# PHASE 10: Documentation and Cleanup

## Step 10.1: Update Documentation

- [ ] Update Data-Architecture-Plan.md with new structure
- [ ] Update any README files
- [ ] Document migration guide (if needed)
- [ ] Update architecture diagrams

## Step 10.2: Git Status

- [ ] Run: `git status`
- [ ] Review all changed files
- [ ] Verify expected changes
- [ ] Check for unexpected modifications

## Step 10.3: Create Git Commit

- [ ] Stage changes: `git add .`
- [ ] Review staged changes
- [ ] Create commit with detailed message
- [ ] Push to feature branch (NOT main)

**Suggested commit message:**
```
refactor: Restructure command and data domain architecture

- Move IGenericCommand to FractalDataWorks.Abstractions for generator access
- Delete polluted FractalDataWorks.Commands.Abstractions project
- Create clean FractalDataWorks.Commands.Abstractions (generic pattern only)
- Create FractalDataWorks.Data.Commands.Abstractions (data-specific)
- Rename DataStores.* → Data.DataStores.*
- Rename DataContainers.* → Data.DataContainers.*
- Rename DataSets.* → Data.DataSets.*
- Move schema types to FractalDataWorks.Data.Abstractions
- Update all project references and using statements
- Eliminate circular dependencies
- Align with architecture specification

BREAKING CHANGE: Namespace changes require using statement updates
```

## Step 10.4: Backup Creation

- [ ] Create backup of final state
- [ ] Document backup location
- [ ] Verify backup integrity

## Step 10.5: Mark Completion

- [ ] Update this document status to COMPLETED
- [ ] Document completion date/time
- [ ] Archive this plan
- [ ] Create post-restructure verification document

---

# Summary Statistics

## Projects Deleted
- [ ] FractalDataWorks.Commands.Abstractions (old)

## Projects Created
- [ ] FractalDataWorks.Commands.Abstractions (new, clean)
- [ ] FractalDataWorks.Data.Commands.Abstractions

## Projects Renamed
- [ ] FractalDataWorks.DataStores.Abstractions → FractalDataWorks.Data.DataStores.Abstractions
- [ ] FractalDataWorks.DataStores.SqlServer → FractalDataWorks.Data.DataStores.SqlServer
- [ ] FractalDataWorks.DataStores.FileSystem → FractalDataWorks.Data.DataStores.FileSystem
- [ ] FractalDataWorks.DataStores.Rest → FractalDataWorks.Data.DataStores.Rest
- [ ] FractalDataWorks.DataContainers.Abstractions → FractalDataWorks.Data.DataContainers.Abstractions
- [ ] FractalDataWorks.DataSets.Abstractions → FractalDataWorks.Data.DataSets.Abstractions

## Files Created
- [ ] IGenericCommand.cs in FractalDataWorks.Abstractions
- [ ] ~15 files in FractalDataWorks.Commands.Abstractions
- [ ] ~12 files in FractalDataWorks.Data.Commands.Abstractions

## Files Moved
- [ ] IDataSchema.cs → FractalDataWorks.Data.Abstractions
- [ ] ISchemaField.cs → FractalDataWorks.Data.Abstractions
- [ ] ISchemaConstraint.cs → FractalDataWorks.Data.Abstractions
- [ ] IServiceFactory.cs → FractalDataWorks.Abstractions root

## Files Deleted
- [ ] src\FractalDataWorks.Abstractions\Services\Abstractions\IGenericService.cs
- [ ] src\FractalDataWorks.Abstractions\Services\Abstractions\Commands\ICommand.cs
- [ ] All files in old FractalDataWorks.Commands.Abstractions

---

# Error Log

*Document any errors encountered during execution here*

**Date/Time** | **Phase** | **Error** | **Resolution**
--- | --- | --- | ---
| | | |

---

# Notes and Observations

*Add notes during execution*

---

**Status Updated:** IN PROGRESS - Phase 3 Complete
**Last Updated:** 2025-10-10 20:15 UTC
**Completed:** Phases 1-3 (Foundation, deletions, project renames)
**Next:** Phase 4 (Create new clean projects)

### Phase 3 Summary:
✅ FractalDataWorks.DataStores.Abstractions → FractalDataWorks.Data.DataStores.Abstractions
✅ FractalDataWorks.DataStores.SqlServer → FractalDataWorks.Data.DataStores.SqlServer
✅ FractalDataWorks.DataStores.FileSystem → FractalDataWorks.Data.DataStores.FileSystem
✅ FractalDataWorks.DataStores.Rest → FractalDataWorks.Data.DataStores.Rest
✅ FractalDataWorks.DataContainers.Abstractions → FractalDataWorks.Data.DataContainers.Abstractions
✅ FractalDataWorks.DataSets.Abstractions → FractalDataWorks.Data.DataSets.Abstractions
