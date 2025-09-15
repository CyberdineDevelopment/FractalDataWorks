# Roslyn MCP Server

A Model Context Protocol (MCP) server that provides compilation-aware tools for .NET solutions using Microsoft.CodeAnalysis (Roslyn). This server enables AI agents and development tools to perform intelligent analysis and refactoring of .NET codebases with full compilation context.

## Features

### Core Capabilities
- **Long-lived Compilation Sessions**: Maintain workspace and compilation state in memory across multiple operations
- **Roslyn Analyzer Integration**: Support for StyleCop, .NET analyzers, and custom analyzers
- **Virtual Edit System**: "Try before commit" - preview changes without writing to disk
- **Intelligent Type Analysis**: Find ambiguous types, duplicates, and perform solution-wide type searches
- **Smart Refactoring**: Rename symbols, separate types to files, and apply code fixes
- **Comprehensive Diagnostics**: Get errors/warnings with filtering, summarization, and code fix availability
- **Context Optimization**: Reduce token usage by 70-85% through intelligent summarization
- **Incremental Compilation**: Fast updates with pause/resume capability for external changes

## Installation and Setup

### Prerequisites
- .NET 8.0 SDK or later
- Visual Studio 2022 or Visual Studio Build Tools (for MSBuild support)
- Windows (recommended) or Linux/macOS with .NET SDK

### Building from Source

```bash
# Clone the repository
git clone <repository-url>
cd dotnet-reference-resolution

# Build the solution
dotnet build RoslynMcpServer.sln

# Run the server
cd RoslynMcpServer
dotnet run
```

### Configuration

The server automatically registers MSBuild using multiple strategies:
1. Latest Visual Studio instance
2. Default MSBuild installation
3. Manual .NET SDK detection

No additional configuration is required for basic operation.

## MCP Tools Reference

### Session Management
- `mcp__roslyn-analyzer__start_session` - Initialize compilation for a solution/project
- `mcp__roslyn-analyzer__get_session_status` - Check compilation health and statistics
- `mcp__roslyn-analyzer__refresh_session` - Reload solution from disk
- `mcp__roslyn-analyzer__end_session` - Clean up resources
- `mcp__roslyn-analyzer__get_active_sessions` - List all active sessions
- `mcp__roslyn-analyzer__get_cache_stats` - View compilation cache performance

### Session Lifecycle (Advanced)
- `mcp__roslyn-analyzer__pause_session` - Pause session for external changes
- `mcp__roslyn-analyzer__resume_session` - Resume with incremental rebuild
- `mcp__roslyn-analyzer__get_pause_changes` - View changes detected while paused

### Diagnostic Analysis
- `mcp__roslyn-analyzer__get_diagnostic_summary` - Aggregate counts by severity, project, rule
- `mcp__roslyn-analyzer__get_diagnostics` - Overview with filtering options (optimized for low context)
- `mcp__roslyn-analyzer__get_diagnostic_details` - Detailed diagnostics with pagination
- `mcp__roslyn-analyzer__get_file_diagnostics` - Diagnostics for specific files
- `mcp__roslyn-analyzer__get_analyzer_info` - Information about active analyzers and rules
- `mcp__roslyn-analyzer__check_code_fix_availability` - Check if diagnostic has available fixes

### Virtual Editing (Try-Before-Commit)
- `mcp__roslyn-analyzer__apply_virtual_edit` - Apply text changes without saving to disk
- `mcp__roslyn-analyzer__apply_multiple_virtual_edits` - Batch multiple file changes atomically
- `mcp__roslyn-analyzer__commit_changes` - Write all pending changes to disk
- `mcp__roslyn-analyzer__rollback_changes` - Discard all pending changes
- `mcp__roslyn-analyzer__get_pending_changes` - List uncommitted modifications
- `mcp__roslyn-analyzer__replace_text` - Precise line/column-based edits

### Type Analysis
- `mcp__roslyn-analyzer__find_ambiguous_types` - Identify types with same name in different namespaces
- `mcp__roslyn-analyzer__find_duplicate_types` - Find non-partial types with multiple declarations
- `mcp__roslyn-analyzer__get_type_details` - Detailed information about specific type and members
- `mcp__roslyn-analyzer__search_types` - Search types by name pattern across solution
- `mcp__roslyn-analyzer__get_type_statistics` - Statistics about types in the solution

### Type Resolution
- `mcp__roslyn-analyzer__find_missing_types` - Find CS0246 errors grouped by type
- `mcp__roslyn-analyzer__suggest_using_statements` - Suggest using statements for missing types
- `mcp__roslyn-analyzer__bulk_add_using_statements` - Add using statements to multiple files

### Refactoring
- `mcp__roslyn-analyzer__rename_symbol` - Rename symbols solution-wide with preview
- `mcp__roslyn-analyzer__separate_types_to_files` - One type per file organization
- `mcp__roslyn-analyzer__move_type_to_new_file` - Move specific type to correctly named file
- `mcp__roslyn-analyzer__apply_code_fix` - Apply available code fixes for diagnostics
- `mcp__roslyn-analyzer__analyze_redundant_null_defense` - Find unnecessary null checks

### Project Dependencies
- `mcp__roslyn-analyzer__get_project_dependencies` - Get dependency graph and statistics
- `mcp__roslyn-analyzer__get_project_details` - Detailed project information
- `mcp__roslyn-analyzer__get_impact_analysis` - Which projects are affected by changes
- `mcp__roslyn-analyzer__get_compilation_order` - Optimal build order

### Server Management
- `mcp__roslyn-analyzer__get_server_info` - Server version and capabilities
- `mcp__roslyn-analyzer__get_diagnostic_info` - Server diagnostics for troubleshooting
- `mcp__roslyn-analyzer__report_error` - Report issues with diagnostic information
- `mcp__roslyn-analyzer__shutdown_server` - Graceful server shutdown
- `mcp__roslyn-analyzer__clear_cache` - Clear all compilation caches

## Architecture

### Long-Lived Sessions
- Uses `MSBuildWorkspace` to maintain solution state in memory
- Incremental compilation for fast updates
- Session-based resource management with automatic cleanup
- Compilation caching for performance

### Virtual Edit System  
- Apply changes to in-memory `SyntaxTree` objects
- Preview diagnostic impact before committing to disk
- Atomic commit/rollback operations
- Change tracking and comparison

### Analyzer Integration
- Default analyzers: StyleCop, .NET Code Analysis
- Support for custom analyzer loading from packages/DLLs
- Configurable rule severities and disabled rules
- Real-time analyzer diagnostic evaluation

## Usage Examples

### Basic Session Management

```bash
# Start a session
mcp__roslyn-analyzer__start_session:
  solutionPath: "C:/MyProject/MySolution.sln"
  useDefaults: true
  additionalAnalyzers: ["Roslynator.Analyzers"]
  disabledRules: ["CA1062"]
# Returns: sessionId

# Get session status
mcp__roslyn-analyzer__get_session_status:
  sessionId: "abc123"
# Returns: compilation statistics, project count, analyzer info
```

### Diagnostic Analysis Workflow

```bash
# 1. Get overview (low context - ~100 tokens)
mcp__roslyn-analyzer__get_diagnostics:
  sessionId: "abc123"
# Returns: summary with counts by severity

# 2. Drill down to specific issues (high context)
mcp__roslyn-analyzer__get_diagnostic_details:
  sessionId: "abc123"
  severity: "Error"
  maxResults: 10
# Returns: detailed diagnostic list with locations

# 3. Check if fixes are available
mcp__roslyn-analyzer__check_code_fix_availability:
  diagnosticId: "CS0103"
# Returns: available code fixes
```

### Virtual Editing (Try-Before-Commit)

```bash
# Apply changes without saving to disk
mcp__roslyn-analyzer__apply_virtual_edit:
  sessionId: "abc123"
  filePath: "Services/UserService.cs"
  textChangesJson: '[{"start": 100, "length": 7, "newText": "GetUserById"}]'

# Check the impact
mcp__roslyn-analyzer__get_diagnostic_summary:
  sessionId: "abc123"
# Shows new diagnostic counts

# Commit if satisfied
mcp__roslyn-analyzer__commit_changes:
  sessionId: "abc123"
# Or rollback if not
mcp__roslyn-analyzer__rollback_changes:
  sessionId: "abc123"
```

### Type Analysis and Resolution

```bash
# Find missing types
mcp__roslyn-analyzer__find_missing_types:
  sessionId: "abc123"
# Returns: CS0246 errors grouped by missing type

# Get suggestions for missing types
mcp__roslyn-analyzer__suggest_using_statements:
  sessionId: "abc123"
  missingTypeName: "List"
# Returns: possible namespaces (System.Collections.Generic)

# Add using statements in bulk
mcp__roslyn-analyzer__bulk_add_using_statements:
  sessionId: "abc123"
  usingFixes: [
    {
      "typeName": "List",
      "namespaceName": "System.Collections.Generic",
      "filePaths": ["Services/UserService.cs"]
    }
  ]
```

## Troubleshooting

### Common Issues

#### Project Loading Problems
**Symptom**: Session starts but `projectCount` is 0

**Cause**: MSBuild registration failed or project file issues

**Solutions**:
1. Ensure Visual Studio 2022 or Build Tools are installed
2. Check that .NET SDK is available
3. Verify project/solution file is valid
4. Check server logs for MSBuild registration messages

```bash
# Check session status
mcp__roslyn-analyzer__get_session_status:
  sessionId: "your-session-id"
# Look for projectCount > 0
```

#### Analyzer Issues
**Symptom**: No analyzer diagnostics returned

**Solutions**:
1. Ensure `includeAnalyzerDiagnostics: true` in diagnostic calls
2. Check analyzer loading with `get_analyzer_info`
3. Verify StyleCop.Analyzers and other packages are installed in target project

#### Performance Issues
**Symptom**: Slow response times or high memory usage

**Solutions**:
1. Use `get_diagnostics` (summary) before `get_diagnostic_details` (full list)
2. Enable pagination with `maxResults` parameter
3. Check cache statistics with `get_cache_stats`
4. Consider pausing/resuming sessions for large operations

### Error Reporting

For bugs or unexpected behavior, use the built-in error reporting:

```bash
mcp__roslyn-analyzer__report_error:
  errorDescription: "Describe what went wrong"
  expectedVsActual: "What you expected vs what happened"
  reproductionSteps: "Steps to reproduce"
  sessionId: "session-id-if-applicable"
```

## Performance Optimization

### Context Reduction Features

- **Diagnostic Summary**: Use `get_diagnostics` (≈100 tokens) before drilling down with `get_diagnostic_details` (≈2000+ tokens)
- **Pagination**: All detail commands support `maxResults` and `cursor` parameters
- **Relative Paths**: File paths are shortened relative to solution root
- **Structured Responses**: Typed objects instead of verbose JSON
- **Comment Stripping**: Code responses exclude comments by default

### Best Practices

1. **Reuse Sessions**: Sessions are designed to be long-lived - don't create new sessions for each operation
2. **Virtual Edits**: Always preview changes before committing
3. **Incremental Updates**: Use pause/resume for external file changes
4. **Targeted Analysis**: Use file-specific tools when possible
5. **Cache Awareness**: Monitor cache stats for memory usage

## Current Implementation Status

### ✅ Fully Implemented
- Session management with long-lived compilations
- Comprehensive diagnostic analysis with analyzer support
- Virtual editing with preview capabilities
- Type analysis and search functionality
- Symbol renaming and refactoring
- Project dependency analysis
- Type resolution and using statement management
- Server diagnostics and error reporting

### 🚧 Partially Implemented
- Code fix application (basic fixes working, complex fixes in progress)
- Advanced type separation (works for simple cases)

### 📋 Future Enhancements
- Real-time file watching during active sessions
- Advanced refactoring patterns (extract method, etc.)
- Custom analyzer loading from NuGet packages
- Integration with external build systems

This creates a powerful foundation for AI agents to perform intelligent, compilation-aware analysis and refactoring of .NET codebases with optimal context usage.