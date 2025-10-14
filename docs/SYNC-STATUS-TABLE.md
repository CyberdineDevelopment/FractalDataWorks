# Sync Status - Quick Reference

## Projects That Will Be Synced ✓

| Project | Category |
|---------|----------|
| FractalDataWorks.Abstractions | Core |
| FractalDataWorks.Collections | Core |
| FractalDataWorks.Collections.Analyzers | Analyzers |
| FractalDataWorks.EnhancedEnums | Core |
| FractalDataWorks.EnhancedEnums.Analyzers | Analyzers |
| FractalDataWorks.Messages | Core |
| FractalDataWorks.Results | Core |
| FractalDataWorks.Services | Core |
| FractalDataWorks.Services.Execution | Services |
| FractalDataWorks.ServiceTypes | Core |
| FractalDataWorks.ServiceTypes.Analyzers | Analyzers |

**Total: 11 projects**

## Projects That Will NOT Be Synced ✗

### Code Builder & Analysis (4)
| Project | Reason |
|---------|--------|
| FractalDataWorks.CodeBuilder.Abstractions | Not public |
| FractalDataWorks.CodeBuilder.Analysis | Not public |
| FractalDataWorks.CodeBuilder.Analysis.CSharp | Not public |
| FractalDataWorks.CodeBuilder.CSharp | Not public |

### Source Generators & Code Fixes (6)
| Project | Reason |
|---------|--------|
| FractalDataWorks.Collections.CodeFixes | Not public |
| FractalDataWorks.Collections.SourceGenerators | Not public |
| FractalDataWorks.EnhancedEnums.CodeFixes | Not public |
| FractalDataWorks.EnhancedEnums.SourceGenerators | Not public |
| FractalDataWorks.ServiceTypes.CodeFixes | Not public |
| FractalDataWorks.ServiceTypes.SourceGenerators | Not public |
| FractalDataWorks.SourceGenerators | Not public |
| FractalDataWorks.Messages.SourceGenerators | Not public |

### Configuration (2)
| Project | Reason |
|---------|--------|
| FractalDataWorks.Configuration | Not public |
| FractalDataWorks.Configuration.Abstractions | Not public |

### Data Layer (7)
| Project | Reason |
|---------|--------|
| FractalDataWorks.Data.Abstractions | Not public |
| FractalDataWorks.Data.DataContainers.Abstractions | Not public |
| FractalDataWorks.Data.DataSets.Abstractions | Not public |
| FractalDataWorks.Data.DataStores | Not public |
| FractalDataWorks.Data.DataStores.Abstractions | Not public |
| FractalDataWorks.Data.DataStores.FileSystem | Not public |
| FractalDataWorks.Data.DataStores.Rest | Not public (+ matches `*\.Rest\.*` pattern) |
| FractalDataWorks.Data.DataStores.SqlServer | Not public (+ matches `*SqlServer*` pattern) |

### Dependency Injection (1)
| Project | Reason |
|---------|--------|
| FractalDataWorks.DependencyInjection | Not public |

### MCP Projects (10)
| Project | Reason |
|---------|--------|
| FractalDataWorks.MCP.Abstractions | Not public (+ matches `*\.MCP\.*` pattern) |
| FractalDataWorks.McpTools.Abstractions | Not public (+ matches `*McpTools*` pattern) |
| FractalDataWorks.McpTools.CodeAnalysis | Not public (+ matches `*McpTools*` pattern) |
| FractalDataWorks.McpTools.ProjectDependencies | Not public (+ matches `*McpTools*` pattern) |
| FractalDataWorks.McpTools.Refactoring | Not public (+ matches `*McpTools*` pattern) |
| FractalDataWorks.McpTools.ServerManagement | Not public (+ matches `*McpTools*` pattern) |
| FractalDataWorks.McpTools.SessionManagement | Not public (+ matches `*McpTools*` pattern) |
| FractalDataWorks.McpTools.TypeAnalysis | Not public (+ matches `*McpTools*` pattern) |
| FractalDataWorks.McpTools.VirtualEditing | Not public (+ matches `*McpTools*` pattern) |

### Results (1)
| Project | Reason |
|---------|--------|
| FractalDataWorks.Results.Abstractions | Not public |

### Command Infrastructure (1)
| Project | Reason |
|---------|--------|
| FractalDataWorks.Commands.Abstractions | Not public |

### Services Layer (21)
| Project | Reason |
|---------|--------|
| FractalDataWorks.Services.Abstractions | Not public |
| FractalDataWorks.Services.Authentication | Not public |
| FractalDataWorks.Services.Authentication.Abstractions | Not public |
| FractalDataWorks.Services.Authentication.Entra | Not public |
| FractalDataWorks.Services.Connections | Not public |
| FractalDataWorks.Services.Connections.Abstractions | Not public |
| FractalDataWorks.Services.Connections.Http.Abstractions | Not public |
| FractalDataWorks.Services.Connections.MsSql | Not public |
| FractalDataWorks.Services.Connections.Rest | Not public (+ matches `*\.Rest\.*` pattern) |
| FractalDataWorks.Services.Data | Not public |
| FractalDataWorks.Services.Data.Abstractions | Not public |
| FractalDataWorks.Services.Execution.Abstractions | Not public |
| FractalDataWorks.Services.Scheduling | Not public |
| FractalDataWorks.Services.Scheduling.Abstractions | Not public |
| FractalDataWorks.Services.SecretManagers | Not public |
| FractalDataWorks.Services.SecretManagers.Abstractions | Not public |
| FractalDataWorks.Services.SecretManagers.AzureKeyVault | Not public |
| FractalDataWorks.Services.Transformations | Not public |
| FractalDataWorks.Services.Transformations.Abstractions | Not public |

### Web (2)
| Project | Reason |
|---------|--------|
| FractalDataWorks.Web.Http.Abstractions | Not public |
| FractalDataWorks.Web.RestEndpoints | Not public |

**Total: 55 projects**

---

## Summary by Category

| Category | Synced | Not Synced | Total |
|----------|--------|------------|-------|
| Core Abstractions | 6 | 1 | 7 |
| Analyzers | 3 | 0 | 3 |
| Source Generators | 0 | 8 | 8 |
| Services | 2 | 21 | 23 |
| Data Layer | 0 | 7 | 7 |
| MCP/Tools | 0 | 10 | 10 |
| Configuration | 0 | 2 | 2 |
| Web | 0 | 2 | 2 |
| Other | 0 | 4 | 4 |
| **TOTAL** | **11** | **55** | **66** |

---

## Exclusion Patterns Reference

These patterns in the sync workflow will exclude projects even if marked public:

| Pattern | Description | Current Matches |
|---------|-------------|-----------------|
| `*McpTools*` | MCP tools | 9 projects (all not public) |
| `*\.Mcp\.*` | MCP projects | 1 project (not public) |
| `*\.MCP\.*` | MCP projects (uppercase) | 0 projects |
| `*SqlServer*` | SQL Server | 1 project (not public) |
| `*\.Rest\.*` | REST implementations | 2 projects (both not public) |

**Note:** Currently all exclusion patterns are redundant since no matching projects are marked as public.
