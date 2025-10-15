# Sync Status - Quick Reference

**Sync Strategy**: BLACKLIST - All projects sync unless excluded by pattern

## Projects That Will Be Synced ✓

**Total: 57 projects** (86.4% of all projects)

All projects EXCEPT those matching exclusion patterns will be synced:

| Project | Category |
|---------|----------|
| FractalDataWorks.Abstractions | Core |
| FractalDataWorks.CodeBuilder.Abstractions | Code Builder |
| FractalDataWorks.CodeBuilder.Analysis | Code Builder |
| FractalDataWorks.CodeBuilder.Analysis.CSharp | Code Builder |
| FractalDataWorks.CodeBuilder.CSharp | Code Builder |
| FractalDataWorks.Collections | Core |
| FractalDataWorks.Collections.Analyzers | Analyzers |
| FractalDataWorks.Collections.CodeFixes | Code Fixes |
| FractalDataWorks.Collections.SourceGenerators | Source Generators |
| FractalDataWorks.Commands.Abstractions | Commands |
| FractalDataWorks.Configuration | Configuration |
| FractalDataWorks.Configuration.Abstractions | Configuration |
| FractalDataWorks.Data.Abstractions | Data |
| FractalDataWorks.Data.DataContainers.Abstractions | Data |
| FractalDataWorks.Data.DataSets.Abstractions | Data |
| FractalDataWorks.Data.DataStores | Data |
| FractalDataWorks.Data.DataStores.Abstractions | Data |
| FractalDataWorks.Data.DataStores.FileSystem | Data |
| FractalDataWorks.Data.DataStores.Rest | Data |
| FractalDataWorks.DependencyInjection | DI |
| FractalDataWorks.EnhancedEnums | Core |
| FractalDataWorks.EnhancedEnums.Analyzers | Analyzers |
| FractalDataWorks.EnhancedEnums.CodeFixes | Code Fixes |
| FractalDataWorks.EnhancedEnums.SourceGenerators | Source Generators |
| FractalDataWorks.MCP.Abstractions | MCP |
| FractalDataWorks.Messages | Core |
| FractalDataWorks.Messages.SourceGenerators | Source Generators |
| FractalDataWorks.Results | Core |
| FractalDataWorks.Results.Abstractions | Core |
| FractalDataWorks.Services | Core |
| FractalDataWorks.Services.Abstractions | Services |
| FractalDataWorks.Services.Authentication | Services |
| FractalDataWorks.Services.Authentication.Abstractions | Services |
| FractalDataWorks.Services.Authentication.Entra | Services |
| FractalDataWorks.Services.Connections | Services |
| FractalDataWorks.Services.Connections.Abstractions | Services |
| FractalDataWorks.Services.Connections.Http.Abstractions | Services |
| FractalDataWorks.Services.Connections.MsSql | Services |
| FractalDataWorks.Services.Connections.Rest | Services |
| FractalDataWorks.Services.Data | Services |
| FractalDataWorks.Services.Data.Abstractions | Services |
| FractalDataWorks.Services.Execution | Services |
| FractalDataWorks.Services.Execution.Abstractions | Services |
| FractalDataWorks.Services.Scheduling | Services |
| FractalDataWorks.Services.Scheduling.Abstractions | Services |
| FractalDataWorks.Services.SecretManagers | Services |
| FractalDataWorks.Services.SecretManagers.Abstractions | Services |
| FractalDataWorks.Services.SecretManagers.AzureKeyVault | Services |
| FractalDataWorks.Services.Transformations | Services |
| FractalDataWorks.Services.Transformations.Abstractions | Services |
| FractalDataWorks.ServiceTypes | Core |
| FractalDataWorks.ServiceTypes.Analyzers | Analyzers |
| FractalDataWorks.ServiceTypes.CodeFixes | Code Fixes |
| FractalDataWorks.ServiceTypes.SourceGenerators | Source Generators |
| FractalDataWorks.SourceGenerators | Source Generators |
| FractalDataWorks.Web.Http.Abstractions | Web |
| FractalDataWorks.Web.RestEndpoints | Web |

## Projects That Will NOT Be Synced ✗

**Total: 9 projects** (13.6% of all projects)

Only projects matching blacklist patterns are excluded:

### SQL Server (1 project)
| Project | Exclusion Pattern |
|---------|-------------------|
| FractalDataWorks.Data.DataStores.SqlServer | `*SqlServer*` |

### MCP Tools (8 projects)
| Project | Exclusion Pattern |
|---------|-------------------|
| FractalDataWorks.McpTools.Abstractions | `*McpTools*` |
| FractalDataWorks.McpTools.CodeAnalysis | `*McpTools*` |
| FractalDataWorks.McpTools.ProjectDependencies | `*McpTools*` |
| FractalDataWorks.McpTools.Refactoring | `*McpTools*` |
| FractalDataWorks.McpTools.ServerManagement | `*McpTools*` |
| FractalDataWorks.McpTools.SessionManagement | `*McpTools*` |
| FractalDataWorks.McpTools.TypeAnalysis | `*McpTools*` |
| FractalDataWorks.McpTools.VirtualEditing | `*McpTools*` |

---

## Summary by Category

| Category | Synced | Not Synced | Total |
|----------|--------|------------|-------|
| Core Abstractions | 7 | 0 | 7 |
| Analyzers | 4 | 0 | 4 |
| Source Generators | 5 | 0 | 5 |
| Code Fixes | 3 | 0 | 3 |
| Code Builder | 4 | 0 | 4 |
| Services | 21 | 0 | 21 |
| Data Layer | 7 | 1 | 8 |
| MCP (non-Tools) | 1 | 0 | 1 |
| MCP Tools | 0 | 8 | 8 |
| Configuration | 2 | 0 | 2 |
| Web | 2 | 0 | 2 |
| Other | 1 | 0 | 1 |
| **TOTAL** | **57** | **9** | **66** |

---

## Exclusion Patterns Reference

These patterns in the sync workflow exclude projects (blacklist):

| Pattern | Description | Current Matches |
|---------|-------------|-----------------|
| `*McpTools*` | MCP tools - private only | 8 projects ✗ |
| `*\.Mcp\.*` | MCP projects (with dots) | 0 projects |
| `*\.MCP\.*` | MCP projects uppercase (with dots) | 0 projects |
| `*SqlServer*` | SQL Server implementations | 1 project ✗ |
| `*\.Rest\.*` | REST implementations (with dots) | 0 projects |

**Notes:**
- Patterns with dots (like `*\.Rest\.*`) require the word to be between dots in the project name
- `FractalDataWorks.MCP.Abstractions` does NOT match `*\.Mcp\.*` (case sensitive, requires lowercase "Mcp")
- `FractalDataWorks.Data.DataStores.Rest` does NOT match `*\.Rest\.*` (no dots around "Rest")
