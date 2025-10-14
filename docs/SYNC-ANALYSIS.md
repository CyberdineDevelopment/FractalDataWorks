# Sync Workflow Analysis

This document provides a comprehensive analysis of the sync workflow (`.github/workflows/sync-public-mirror.yml`) and identifies which packages will and will not be synced to the public repository.

## Sync Workflow Overview

The sync workflow copies projects from this private repository to the public mirror repository. It uses two criteria to determine what gets synced:

1. **IsPublicProject Flag**: Projects must have `<IsPublicProject>true</IsPublicProject>` in their `.csproj` file
2. **Exclusion Patterns**: Even if marked public, certain patterns are excluded

## Exclusion Patterns

The workflow defines the following exclusion patterns (line 36-43 in `sync-public-mirror.yml`):

| Pattern | Description | Reason |
|---------|-------------|--------|
| `*McpTools*` | MCP tools | Private only |
| `*\.Mcp\.*` | MCP projects | Private only |
| `*\.MCP\.*` | MCP projects (uppercase) | Private only |
| `*SqlServer*` | SQL Server implementations | Private only |
| `*\.Rest\.*` | REST implementations | May have proprietary integrations |

## Projects That WILL Be Synced (11)

These projects are marked as `<IsPublicProject>true</IsPublicProject>` and do not match any exclusion pattern:

1. ✓ **FractalDataWorks.Abstractions** - Core abstractions
2. ✓ **FractalDataWorks.Collections** - Collection utilities
3. ✓ **FractalDataWorks.Collections.Analyzers** - Collection analyzers
4. ✓ **FractalDataWorks.EnhancedEnums** - Enhanced enum support
5. ✓ **FractalDataWorks.EnhancedEnums.Analyzers** - Enhanced enum analyzers
6. ✓ **FractalDataWorks.Messages** - Message abstractions
7. ✓ **FractalDataWorks.Results** - Result pattern implementations
8. ✓ **FractalDataWorks.Services** - Service abstractions
9. ✓ **FractalDataWorks.Services.Execution** - Execution services
10. ✓ **FractalDataWorks.ServiceTypes** - Service type abstractions
11. ✓ **FractalDataWorks.ServiceTypes.Analyzers** - Service type analyzers

## Projects That WILL NOT Be Synced (55)

### Not Marked as Public (55 projects)

These projects are not marked with `<IsPublicProject>true</IsPublicProject>`:

#### Code Builder & Analysis (4)
- ✗ FractalDataWorks.CodeBuilder.Abstractions
- ✗ FractalDataWorks.CodeBuilder.Analysis
- ✗ FractalDataWorks.CodeBuilder.Analysis.CSharp
- ✗ FractalDataWorks.CodeBuilder.CSharp

#### Source Generators & Code Fixes (6)
- ✗ FractalDataWorks.Collections.CodeFixes
- ✗ FractalDataWorks.Collections.SourceGenerators
- ✗ FractalDataWorks.EnhancedEnums.CodeFixes
- ✗ FractalDataWorks.EnhancedEnums.SourceGenerators
- ✗ FractalDataWorks.ServiceTypes.CodeFixes
- ✗ FractalDataWorks.ServiceTypes.SourceGenerators

#### Message & Command Infrastructure (2)
- ✗ FractalDataWorks.Commands.Abstractions
- ✗ FractalDataWorks.Messages.SourceGenerators

#### Configuration (2)
- ✗ FractalDataWorks.Configuration
- ✗ FractalDataWorks.Configuration.Abstractions

#### Data Layer (7)
- ✗ FractalDataWorks.Data.Abstractions
- ✗ FractalDataWorks.Data.DataContainers.Abstractions
- ✗ FractalDataWorks.Data.DataSets.Abstractions
- ✗ FractalDataWorks.Data.DataStores
- ✗ FractalDataWorks.Data.DataStores.Abstractions
- ✗ FractalDataWorks.Data.DataStores.FileSystem
- ✗ FractalDataWorks.Data.DataStores.Rest *(would also match exclusion pattern `*\.Rest\.*`)*
- ✗ FractalDataWorks.Data.DataStores.SqlServer *(would also match exclusion pattern `*SqlServer*`)*

#### Dependency Injection (1)
- ✗ FractalDataWorks.DependencyInjection

#### MCP Projects (10)
These would be excluded by pattern even if marked public:
- ✗ FractalDataWorks.MCP.Abstractions *(matches `*\.MCP\.*`)*
- ✗ FractalDataWorks.McpTools.Abstractions *(matches `*McpTools*`)*
- ✗ FractalDataWorks.McpTools.CodeAnalysis *(matches `*McpTools*`)*
- ✗ FractalDataWorks.McpTools.ProjectDependencies *(matches `*McpTools*`)*
- ✗ FractalDataWorks.McpTools.Refactoring *(matches `*McpTools*`)*
- ✗ FractalDataWorks.McpTools.ServerManagement *(matches `*McpTools*`)*
- ✗ FractalDataWorks.McpTools.SessionManagement *(matches `*McpTools*`)*
- ✗ FractalDataWorks.McpTools.TypeAnalysis *(matches `*McpTools*`)*
- ✗ FractalDataWorks.McpTools.VirtualEditing *(matches `*McpTools*`)*

#### Results (1)
- ✗ FractalDataWorks.Results.Abstractions

#### Services Layer (21)
- ✗ FractalDataWorks.Services.Abstractions
- ✗ FractalDataWorks.Services.Authentication
- ✗ FractalDataWorks.Services.Authentication.Abstractions
- ✗ FractalDataWorks.Services.Authentication.Entra
- ✗ FractalDataWorks.Services.Connections
- ✗ FractalDataWorks.Services.Connections.Abstractions
- ✗ FractalDataWorks.Services.Connections.Http.Abstractions
- ✗ FractalDataWorks.Services.Connections.MsSql *(SQL Server connection)*
- ✗ FractalDataWorks.Services.Connections.Rest *(matches `*\.Rest\.*`)*
- ✗ FractalDataWorks.Services.Data
- ✗ FractalDataWorks.Services.Data.Abstractions
- ✗ FractalDataWorks.Services.Execution.Abstractions
- ✗ FractalDataWorks.Services.Scheduling
- ✗ FractalDataWorks.Services.Scheduling.Abstractions
- ✗ FractalDataWorks.Services.SecretManagers
- ✗ FractalDataWorks.Services.SecretManagers.Abstractions
- ✗ FractalDataWorks.Services.SecretManagers.AzureKeyVault
- ✗ FractalDataWorks.Services.Transformations
- ✗ FractalDataWorks.Services.Transformations.Abstractions

#### Source Generators (1)
- ✗ FractalDataWorks.SourceGenerators

#### Web (2)
- ✗ FractalDataWorks.Web.Http.Abstractions
- ✗ FractalDataWorks.Web.RestEndpoints

## Summary Statistics

| Category | Count |
|----------|-------|
| **Total Projects** | **66** |
| **Will be synced** | **11** (16.7%) |
| **Will NOT be synced** | **55** (83.3%) |
| - Not marked public | 55 |
| - Public but excluded | 0 |

## Key Findings

1. **Only 11 out of 66 projects (16.7%) will be synced** to the public repository
2. **No projects are currently marked public but excluded by pattern** - all exclusion patterns are redundant since those projects are not marked as public
3. **Major areas not synced include**:
   - All MCP/McpTools projects (10 projects)
   - Services layer implementations (21 projects)
   - Data layer implementations (7 projects)
   - Source generators and code fixes (6 projects)
   - Configuration, DI, and web infrastructure

## Packages Excluded by Specific Patterns

If any of these were marked public, they would still be excluded:

### SQL Server Pattern (`*SqlServer*`)
- FractalDataWorks.Data.DataStores.SqlServer (not public)
- FractalDataWorks.Services.Connections.MsSql (not public, but name doesn't match pattern)

### REST Pattern (`*\.Rest\.*`)
- FractalDataWorks.Data.DataStores.Rest (not public)
- FractalDataWorks.Services.Connections.Rest (not public)

### MCP Patterns (`*McpTools*`, `*\.Mcp\.*`, `*\.MCP\.*`)
- All 10 MCP-related projects (none are public)

## Recommendations

If you want to sync additional packages:
1. Add `<IsPublicProject>true</IsPublicProject>` to their `.csproj` files
2. Ensure they don't contain proprietary code or match exclusion patterns
3. Verify they don't have dependencies on non-public packages

If you want to prevent certain packages from syncing even if marked public:
- The current exclusion patterns will catch: MCP tools, SQL Server implementations, and REST implementations
- Consider whether other patterns are needed
