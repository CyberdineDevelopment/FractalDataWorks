# Roslyn MCP Server - Known Issues

## False Positive Warnings in Self-Analysis

When the Roslyn MCP server analyzes itself (or other projects), it may report warnings that don't appear in the actual build. This is because the Roslyn compilation context created by the MCP server may not perfectly match the MSBuild compilation context.

### CS8632 - Nullable Reference Type Annotations
- **Symptom**: "The annotation for nullable reference types should only be used in code within a '#nullable' annotations context"
- **Reality**: The project has `<Nullable>enable</Nullable>` and builds with 0 warnings
- **Cause**: The MCP server's Roslyn workspace doesn't properly recognize the nullable context from Directory.Build.props or csproj settings
- **Impact**: Cosmetic only - the actual build is clean

### CS0246 & CS0103 - Type/Name Not Found  
- **Count**: 628 CS0246 errors, 226 CS0103 errors
- **Cause**: The MCP server's compilation doesn't have all the implicit references that MSBuild includes
- **Examples**: System types, LINQ extensions, common namespaces
- **Impact**: Analysis features still work, but error count is inflated

## Unimplemented Features

### 1. ApplyCodeFix
- **Location**: RefactoringTools.cs:247
- **Status**: Returns "not yet implemented" error
- **Required**: Integration with Roslyn CodeFixProviders

### 2. SeparateTypesToFiles / MoveTypeToNewFile
- **Location**: RefactoringTools.cs:473
- **Status**: Returns "not yet implemented" error  
- **Required**: Complex file manipulation and syntax tree rewriting

## Workarounds

1. **For nullable warnings**: Ignore CS8632 warnings from the MCP analyzer - trust the actual build output
2. **For type resolution**: The core analysis features (rename, find references, etc.) still work despite the errors
3. **For code fixes**: Manual fixes are required until CodeFixProvider integration is implemented