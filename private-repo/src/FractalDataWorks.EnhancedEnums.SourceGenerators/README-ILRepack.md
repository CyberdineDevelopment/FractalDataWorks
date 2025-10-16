# ILRepack Configuration for Source Generators

This document describes the ILRepack process used to merge FractalDataWorks dependencies into the Enhanced Enums source generator assembly, resolving runtime dependency loading issues.

## Problem Statement

Source generators run in a restricted context where assembly loading can fail when dependencies are not available in the same folder as the source generator DLL. The Enhanced Enums source generator depends on several FractalDataWorks assemblies:

- `FractalDataWorks.CodeBuilder.Abstractions.dll`
- `FractalDataWorks.CodeBuilder.CSharp.dll`
- `FractalDataWorks.EnhancedEnums.dll`
- `FractalDataWorks.EnhancedEnums.Analyzers.dll`

Without ILRepack, these dependencies would need to be packaged separately in the NuGet package's `analyzers/dotnet/cs` folder, but the source generator still fails to load them at runtime with errors like:

```
Could not load file or assembly 'FractalDataWorks.CodeBuilder.Abstractions, Version=0.3.0.0'
```

## Solution: ILRepack Integration

ILRepack merges multiple assemblies into a single DLL, internalizing all dependencies so the source generator is self-contained.

### ILRepack Package

The project uses `ILRepack.Lib.MSBuild.Task` version 2.0.34.2:

```xml
<PackageReference Include="ILRepack.Lib.MSBuild.Task">
  <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
  <PrivateAssets>all</PrivateAssets>
</PackageReference>
```

### MSBuild Target Configuration

The ILRepack process is configured as an MSBuild target that runs after the build:

```xml
<!-- ILRepack configuration for source generator dependency merging -->
<Target Name="MergeAnalyzerDependencies" AfterTargets="Build" Condition="'$(TargetFramework)' == 'netstandard2.0'">
  <PropertyGroup>
    <MergedAssemblyPath>$(OutputPath)$(AssemblyName).Merged.dll</MergedAssemblyPath>
  </PropertyGroup>
  
  <ItemGroup>
    <!-- Main assembly first -->
    <InputAssemblies Include="$(OutputPath)$(AssemblyName).dll" />
    
    <!-- Custom dependencies to internalize -->
    <InputAssemblies Include="$(OutputPath)FractalDataWorks.CodeBuilder.Abstractions.dll" />
    <InputAssemblies Include="$(OutputPath)FractalDataWorks.CodeBuilder.CSharp.dll" />
    <InputAssemblies Include="$(OutputPath)FractalDataWorks.EnhancedEnums.dll" />
    <InputAssemblies Include="$(OutputPath)FractalDataWorks.EnhancedEnums.Analyzers.dll" />
    
    <!-- Comprehensive library search paths -->
    <LibraryPath Include="$(OutputPath)" />
    <LibraryPath Include="$(MSBuildProjectDirectory)\bin\$(Configuration)\$(TargetFramework)" />
    <!-- Use MSBuild properties for version resolution -->
    <LibraryPath Include="$(NuGetPackageRoot)microsoft.codeanalysis.common\$(MicrosoftCodeAnalysisVersion)\lib\netstandard2.0" Condition="'$(MicrosoftCodeAnalysisVersion)' != ''" />
    <LibraryPath Include="$(NuGetPackageRoot)microsoft.codeanalysis.csharp\$(MicrosoftCodeAnalysisCSharpVersion)\lib\netstandard2.0" Condition="'$(MicrosoftCodeAnalysisCSharpVersion)' != ''" />
    <LibraryPath Include="$(NuGetPackageRoot)system.collections.immutable\$(SystemCollectionsImmutableVersion)\lib\netstandard2.0" Condition="'$(SystemCollectionsImmutableVersion)' != ''" />
    <LibraryPath Include="$(NuGetPackageRoot)system.memory\$(SystemMemoryVersion)\lib\netstandard2.0" Condition="'$(SystemMemoryVersion)' != ''" />
    <!-- Fallback paths with pattern matching -->
    <LibraryPath Include="$(NuGetPackageRoot)microsoft.codeanalysis.common\**\lib\netstandard2.0" />
    <LibraryPath Include="$(NuGetPackageRoot)microsoft.codeanalysis.csharp\**\lib\netstandard2.0" />
  </ItemGroup>
  
  <Message Text="ILRepack: Merging assemblies into $(OutputPath)$(AssemblyName).dll" Importance="high" />
  <Message Text="ILRepack: Input assemblies: @(InputAssemblies)" Importance="normal" />
  
  <ILRepack 
    OutputFile="$(OutputPath)$(AssemblyName).dll"
    InputAssemblies="@(InputAssemblies)"
    LibraryPath="@(LibraryPath)"
    Internalize="true" />
  
  <Message Text="ILRepack: Successfully merged source generator dependencies" Importance="high" />
</Target>
```

### Key Configuration Aspects

#### 1. Target Timing and Conditions
- `AfterTargets="Build"` - Runs after the normal build process
- `Condition="'$(TargetFramework)' == 'netstandard2.0'"` - Only runs for the source generator target framework

#### 2. Input Assemblies
The main assembly is listed first, followed by all dependencies:
```xml
<InputAssemblies Include="$(OutputPath)$(AssemblyName).dll" />
<InputAssemblies Include="$(OutputPath)FractalDataWorks.CodeBuilder.Abstractions.dll" />
<InputAssemblies Include="$(OutputPath)FractalDataWorks.CodeBuilder.CSharp.dll" />
<InputAssemblies Include="$(OutputPath)FractalDataWorks.EnhancedEnums.dll" />
<InputAssemblies Include="$(OutputPath)FractalDataWorks.EnhancedEnums.Analyzers.dll" />
```

#### 3. Library Path Resolution
Multiple search paths are provided for Roslyn dependencies:
- Output directory: `$(OutputPath)`
- Project-specific bin directory
- NuGet package directories with version-specific and fallback patterns
- Both specific version paths (using MSBuild properties) and wildcard patterns

#### 4. ILRepack Parameters
Only essential parameters that are supported by ILRepack.Lib.MSBuild.Task v2.0.34.2:

- `OutputFile="$(OutputPath)$(AssemblyName).dll"` - Directly overwrites the original assembly
- `InputAssemblies="@(InputAssemblies)"` - Specifies which assemblies to merge
- `LibraryPath="@(LibraryPath)"` - Assembly resolution paths
- `Internalize="true"` - Merges dependencies into the main assembly

#### 5. Unsupported Parameters
The following parameters were tested but are **not supported** by ILRepack.Lib.MSBuild.Task v2.0.34.2:
- `InternalizeExcludeAssemblies` - Would exclude specific assemblies from internalization
- `AllowDuplicateResources` - Would handle resource conflicts
- `AllowMultipleAssemblyLevelAttributes` - Would handle attribute conflicts
- `DebugInfo` - Would preserve debug information
- `CopyAttributes` - Would copy assembly attributes

### Package Structure

The merged assembly is packaged in the correct location for source generators:

```xml
<ItemGroup>
  <!-- Only include the merged analyzer DLL in the package -->
  <None Include="$(OutputPath)\$(AssemblyName).dll" Pack="true" PackagePath="analyzers/dotnet/cs" Visible="false" />
  <!-- Include build props in the package -->
  <None Include="build\FractalDataWorks.EnhancedEnums.SourceGenerators.props" Pack="true" PackagePath="build\" />
</ItemGroup>
```

## Build Process Flow

1. **Normal Build**: The project builds normally, outputting the main assembly and copying all dependencies to the output directory
2. **ILRepack Execution**: The `MergeAnalyzerDependencies` target runs, merging all FractalDataWorks dependencies into the main assembly
3. **Direct Overwrite**: ILRepack outputs directly to the original assembly file path, replacing the original with the merged version
4. **Packaging**: The merged assembly (now self-contained) is packaged into the NuGet package

## Results

After ILRepack processing:
- **Original assembly size**: ~107KB
- **Merged assembly size**: ~178KB (includes all dependencies)
- **Dependencies included**: All FractalDataWorks dependencies are internalized
- **External dependencies**: Microsoft.CodeAnalysis assemblies remain external (loaded by the host)

## Verification

The source generator can be verified by:

1. **Build Output**: Check for ILRepack messages in build output:
   ```
   ILRepack: Merging assemblies into bin\Release\netstandard2.0\FractalDataWorks.EnhancedEnums.SourceGenerators.dll
   Added assembly 'bin\Release\netstandard2.0\FractalDataWorks.EnhancedEnums.SourceGenerators.dll'
   Added assembly 'bin\Release\netstandard2.0\FractalDataWorks.CodeBuilder.Abstractions.dll'
   Added assembly 'bin\Release\netstandard2.0\FractalDataWorks.CodeBuilder.CSharp.dll'
   Added assembly 'bin\Release\netstandard2.0\FractalDataWorks.EnhancedEnums.dll'
   Added assembly 'bin\Release\netstandard2.0\FractalDataWorks.EnhancedEnums.Analyzers.dll'
   Merging 5 assemblies to 'bin\Release\netstandard2.0\FractalDataWorks.EnhancedEnums.SourceGenerators.dll'
   Merge succeeded in 0.6 s
   ```

2. **Assembly Size**: The merged assembly should be significantly larger than the original

3. **Runtime Testing**: Consumer projects should no longer show assembly loading errors and the source generator should populate collections correctly

## Troubleshooting

### Common Issues

1. **"Unable to resolve assembly" errors**: 
   - Ensure all dependencies are built before the ILRepack target runs
   - Verify LibraryPath entries point to correct NuGet package locations

2. **Unsupported parameter errors**:
   - Remove unsupported parameters from the ILRepack task
   - Use only the basic parameters that work with ILRepack.Lib.MSBuild.Task v2.0.34.2

3. **Assembly name conflicts**:
   - Output directly to the final assembly name instead of using temporary files
   - Avoid using Move operations that can create naming confusion

### Diagnostic Commands

```bash
# Check merged assembly dependencies (should show minimal external references)
ildasm FractalDataWorks.EnhancedEnums.SourceGenerators.dll /output:disassembly.il
grep -c "assembly extern" disassembly.il

# Verify assembly loads correctly
powershell -Command "
[Reflection.Assembly]::LoadFrom('path\to\FractalDataWorks.EnhancedEnums.SourceGenerators.dll')
Write-Host 'Assembly loaded successfully'
"
```

## Future Considerations

- **Version Updates**: When updating ILRepack.Lib.MSBuild.Task, verify parameter compatibility
- **New Dependencies**: Add new FractalDataWorks dependencies to the InputAssemblies list
- **Performance**: Monitor merged assembly size to ensure it remains reasonable
- **Alternative Tools**: Consider other merging tools if ILRepack limitations become problematic

This configuration provides a reliable, automated solution for creating self-contained source generator assemblies that work correctly in the restricted source generator execution context.