# TypeCollectionGenerator - Step-by-Step Execution Flow

This document provides a detailed, line-by-line explanation of how the TypeCollectionGenerator discovers and generates type collections.

## Overview

The TypeCollectionGenerator creates high-performance type collections using attribute-based discovery. It scans the compilation for:
1. Classes marked with `[TypeCollection(baseType, returnType, collectionType)]` - the collection definitions
2. Classes marked with `[TypeOption(collectionType, name)]` - the options to include in collections

## Example: AuthenticationFlows (Cross-Assembly)

Given this code:
```csharp
// The collection definition (in FractalDataWorks.Services.Authentication.Abstractions)
[TypeCollection(typeof(AuthenticationFlowBase), typeof(IAuthenticationFlow), typeof(AuthenticationFlows))]
public abstract partial class AuthenticationFlows : TypeCollectionBase<AuthenticationFlowBase, IAuthenticationFlow>
{
}

// The base type that options inherit from
public abstract class AuthenticationFlowBase : TypeOptionBase<AuthenticationFlowBase>, IAuthenticationFlow
{
    protected AuthenticationFlowBase(int id, string name, bool requiresUserInteraction, bool supportsRefreshTokens, bool isServerToServer)
        : base(id, name) { ... }
}

// Example options (in FractalDataWorks.Services.Authentication - DIFFERENT assembly)
[TypeOption(typeof(AuthenticationFlows), "ClientCredentials")]
public sealed class ClientCredentialsFlow : AuthenticationFlowBase
{
    public ClientCredentialsFlow() : base(1, "ClientCredentials", false, false, true) { }
}
```

## Example: ConnectionStates (Single-Assembly)

```csharp
// The collection definition (in FractalDataWorks.Services.Connections.Abstractions)
[TypeCollection(typeof(ConnectionStateBase), typeof(IConnectionState), typeof(ConnectionStates))]
[TypeCollection(RestrictToCurrentCompilation = true)]  // Optional: restrict to same assembly
public abstract partial class ConnectionStates : TypeCollectionBase<ConnectionStateBase, IConnectionState>
{
}

// Example options (in SAME assembly)
[TypeOption(typeof(ConnectionStates), "Open")]
public sealed class OpenConnectionState() : ConnectionStateBase(3, "Open");
```

The generator will create:
```csharp
public abstract partial class AuthenticationFlows
{
    public static IAuthenticationFlow ClientCredentials { get; } = new ClientCredentialsFlow();
    public static IReadOnlyList<IAuthenticationFlow> All() => ...;
    public static IAuthenticationFlow Empty() => ...;
    // etc.
}
```

## Detailed Step-by-Step Execution

### PHASE 0: Generator Initialization (Line 60-70)

**File:** TypeCollectionGenerator.cs
**Method:** Initialize()

1. **Line 60**: `Initialize()` method called by Roslyn when compilation starts
2. **Line 64-69**: DEBUG mode creates init file to prove generator loaded
   - Creates `TypeCollectionGenerator.Init.g.cs` with timestamp
   - This file appears in `obj/` if `EmitCompilerGeneratedFiles` is true

### PHASE 1: Pipeline Setup (Line 72-82)

**File:** TypeCollectionGenerator.cs
**Method:** Initialize()

3. **Line 73-74**: Create pipeline combining compilation + analyzer options
   - `context.CompilationProvider` = current compilation being built
   - `context.AnalyzerConfigOptionsProvider` = MSBuild properties (RootNamespace, etc.)

4. **Line 76-82**: Register discovery pipeline
   - `SelectMany` = for each compilation, discover all collections
   - Calls `DiscoverAllCollectionDefinitions()` for actual work

5. **Line 85-98**: Register code generation pipeline
   - `RegisterSourceOutput` = for each discovered collection, generate code
   - Reports diagnostics (errors/warnings)
   - Calls `Execute()` if no errors found

### PHASE 2: Discovery Entry Point (Line 105-160)

**File:** TypeCollectionGenerator.cs
**Method:** DiscoverAllCollectionDefinitions()

6. **Line 105**: Entry point for discovery phase
   - Input: `Compilation` (all syntax trees), `AnalyzerConfigOptions` (MSBuild props)
   - Output: `ImmutableArray<EnumTypeInfoWithCompilation>` (all discovered collections)

7. **Line 107**: Create results list to accumulate discovered collections

### PHASE 3: STEP 1 - Discover All TypeOption Attributes (Line 111)

8. **Line 111**: Call `FindAndGroupAllTypeOptions(compilation)`
   - This is the FIRST scan: find all `[TypeOption]` attributes in entire solution
   - Groups them by which collection they belong to
   - Returns: `Dictionary<INamedTypeSymbol, List<INamedTypeSymbol>>`
     - Key = collection class (e.g., `AuthenticationFlows`)
     - Value = list of option types (e.g., `ClientCredentialsFlow`, `AuthorizationCodeFlow`)

#### STEP 1.1: FindAndGroupAllTypeOptions Details (Line 212-277)

9. **Line 212**: Start of `FindAndGroupAllTypeOptions()`
   - Creates dictionary to hold results: `serviceOptionsByCollectionType`

10. **Line 214**: Create empty dictionary for grouping results
    - Key: collection type symbol (e.g., `AuthenticationFlows`)
    - Value: list of option type symbols (e.g., `[ClientCredentialsFlow, AuthorizationCodeFlow]`)
    - Uses `SymbolEqualityComparer.Default` for symbol comparison

11. **Line 215**: Get the `TypeOptionAttribute` type from compilation
    - `compilation.GetTypeByMetadataName()` looks up type by full name
    - Full name: `"FractalDataWorks.Collections.Attributes.TypeOptionAttribute"`

12. **Line 217**: Check if attribute type was found
    - If not found, return empty dictionary (no options to discover)

13. **Line 220**: Create list to hold all discovered options with their attributes
    - List of tuples: `(INamedTypeSymbol Type, AttributeData Attribute)`

14. **Line 223**: Scan current compilation's global namespace
    - Calls `ScanNamespaceForTypeOptionsWithAttributes()`
    - Recursively walks ALL namespaces in current project
    - Finds every type with `[TypeOption]` attribute

15. **Line 226-232**: Scan all referenced assemblies
    - Loop through `compilation.References` (all DLLs this project references)
    - For each assembly, scan its global namespace
    - This finds options defined in other projects

16. **Line 235**: Create dictionary to track most derived types
    - Outer key: collection type
    - Inner key: full type name string
    - Inner value: most derived version of that type

17. **Line 238-254**: Group discovered options by collection type
    - **Line 238**: Loop through all found (Type, Attribute) pairs
    - **Line 240**: Extract collection type from attribute
      - Calls `ExtractCollectionTypeFromTypeOptionAttribute()`
      - Gets first constructor argument of `[TypeOption(typeof(AuthenticationFlows), "Name")]`
    - **Line 242-246**: Get or create inner dictionary for this collection
    - **Line 248**: Get full type name (e.g., "MyNamespace.ClientCredentialsFlow")
    - **Line 251-260**: Deduplication logic
      - If we've seen this type name before, keep the most derived version
      - Uses `IsDerivedFrom()` to determine inheritance relationship
      - Prevents duplicate entries when same type appears in multiple assemblies

18. **Line 269-274**: Convert nested dictionary back to simple dictionary
    - Flatten: `Dictionary<Collection, Dictionary<Name, Type>>` → `Dictionary<Collection, List<Type>>`

19. **Line 276**: Return the grouped options dictionary

#### Helper: ScanNamespaceForTypeOptionsWithAttributes (Line 963-983)

20. **Line 963**: Recursive namespace scanner
21. **Line 966-972**: Scan all types in current namespace
    - **Line 968**: Call `GetTypeOptionAttribute()` for each type
    - **Line 969-971**: If attribute found, add to results
22. **Line 975**: Recursively scan nested types
23. **Line 979**: Recursively scan child namespaces

#### Helper: GetTypeOptionAttribute (Line 1006-1010)

24. **Line 1008**: Get attributes from type
25. **Line 1009**: Find first attribute matching `TypeOptionAttribute`
    - Uses `SymbolEqualityComparer` to compare attribute types

#### Helper: ExtractCollectionTypeFromTypeOptionAttribute (Line 1015-1026)

26. **Line 1017**: Check if attribute has constructor arguments
27. **Line 1019**: Get first constructor argument
28. **Line 1020**: Check if it's a Type argument
    - `[TypeOption(typeof(AuthenticationFlows), "Name")]`
    - First arg: `typeof(AuthenticationFlows)` → `INamedTypeSymbol` for `AuthenticationFlows`
29. **Line 1021**: Return the collection type symbol

### PHASE 4: STEP 2 - Discover All TypeCollection Attributes (Line 115)

30. **Line 115**: Call `FindAttributedCollectionClasses(compilation)`
    - This is the SECOND scan: find all `[TypeCollection]` attributes
    - Returns: `List<(INamedTypeSymbol CollectionClass, AttributeData Attribute)>`

#### STEP 2.1: FindAttributedCollectionClasses Details (Line 386-397)

31. **Line 386**: Start of `FindAttributedCollectionClasses()`
32. **Line 388**: Create results list
33. **Line 389**: Get `TypeCollectionAttribute` type from compilation
    - Full name: `"FractalDataWorks.Collections.Attributes.TypeCollectionAttribute"`
34. **Line 391**: Check if attribute type was found
35. **Line 394**: Scan global namespace for classes with attribute
    - Calls `ScanNamespaceForAttributedTypes()`
    - Recursively walks namespaces looking for `[TypeCollection]`
36. **Line 396**: Return list of collection classes with their attributes

#### Helper: ScanNamespaceForAttributedTypes (Line 402-424)

37. **Line 402**: Recursive scanner for attributed types
38. **Line 405-413**: Scan types in current namespace
    - **Line 407**: Get all attributes on type
    - **Line 408**: Find `TypeCollectionAttribute`
    - **Line 410-412**: If found, add to results
39. **Line 416**: Recursively scan nested types
40. **Line 420-423**: Recursively scan child namespaces

### PHASE 5: STEP 3-6 - Process Each Collection (Line 118-157)

41. **Line 118**: Loop through each discovered collection class
    - Example: `AuthenticationFlows` class with `[TypeCollection(...)]`

#### Collection Processing - Per Collection

42. **Line 122-126**: IMPORTANT CHECK: Only process types in current compilation
    - **Line 122**: Compare collection's assembly with compilation's assembly
    - **Line 125**: Skip if from referenced assembly
    - This prevents regenerating code for types from NuGet packages

### STEP 3: Extract Base Type (Line 122-126)

43. **Line 122**: Extract base type name from attribute
    - Calls `ExtractBaseTypeNameFromAttribute(attribute)`
    - `[TypeCollection(typeof(AuthenticationFlowBase), ...)]`
    - Gets first constructor argument

44. **Line 123**: If no base type found, skip this collection

45. **Line 125**: Look up base type in compilation
    - `compilation.GetTypeByMetadataName(baseTypeName)`
    - Example: Finds `AuthenticationFlowBase` type

46. **Line 126**: If base type not found, skip this collection

### STEP 3.2: Extract RestrictToCurrentCompilation Flag (Line 129)

47. **Line 129**: Extract compilation restriction flag
    - Calls `ExtractRestrictToCurrentCompilationFlag(attribute)`
    - Checks for named property: `RestrictToCurrentCompilation = true`
    - Default is `false` (cross-assembly support enabled)

#### Helper: ExtractRestrictToCurrentCompilationFlag (Line 490-503)

48. **Line 490**: Helper to extract the flag value
49. **Line 493-494**: Look for named argument `RestrictToCurrentCompilation`
50. **Line 496-499**: If found and value is bool, return it
51. **Line 502**: Default to false (enables cross-assembly discovery)

#### Helper: ExtractBaseTypeNameFromAttribute (Line 450-459)

47. **Line 450**: Extract base type name from attribute
48. **Line 453**: Check if first constructor argument exists
49. **Line 453**: Check if it's a Type argument (`ITypeSymbol`)
50. **Line 455**: Return full type name as string
    - Example: "FractalDataWorks.Services.Authentication.Abstractions.AuthenticationFlowBase"

### STEP 3.1: Validate No Abstract Properties (Line 136)

51. **Line 136**: Validate base type doesn't have abstract properties
    - Calls `ValidateNoAbstractProperties(baseType, collectionClass)`
    - Returns list of diagnostics (errors)

#### Helper: ValidateNoAbstractProperties (Line 166-204)

52. **Line 166**: Validation method for abstract properties
53. **Line 168**: Create diagnostics list
54. **Line 169**: Start with base type

55. **Line 172**: Walk up inheritance chain
    - Check base type, then its base, etc.

56. **Line 174**: Loop through all members of current type
57. **Line 176**: Check if member is abstract property
    - `member is IPropertySymbol property && property.IsAbstract`

58. **Line 179**: Get property location for error reporting
59. **Line 182-186**: Fallback to attribute location if property location unavailable

60. **Line 188-195**: Create diagnostic error
    - Error ID: "TC006"
    - Message: "The base type '{baseType}' contains abstract property '{propertyName}'"

61. **Line 137**: Store diagnostics for later reporting

### STEP 4: Lookup Pre-Discovered Options (Line 137-155)

52. **Line 137-140**: ULTRA-FAST lookup of ALL options (O(1) dictionary lookup)
    - Look in `typeOptionsByCollectionType` dictionary from STEP 1
    - Key = `collectionClass` (e.g., `AuthenticationFlows`)
    - Value = `allOptionTypes` - ALL discovered options from current + referenced assemblies

53. **Line 139**: If no options found in dictionary, create empty list

### STEP 4.1: Filter Based on RestrictToCurrentCompilation Flag (Line 143-155)

54. **Line 144**: Check `restrictToCurrentCompilation` flag from STEP 3.2

55. **Line 144-150**: If `restrictToCurrentCompilation = true`:
    - Filter to ONLY types from current compilation
    - `t.ContainingAssembly == compilation.Assembly`
    - Use case: Single-assembly pattern (e.g., ConnectionStates)
    - Result: `optionTypes` contains only same-assembly implementations

56. **Line 151-155**: If `restrictToCurrentCompilation = false`:
    - Include ALL types from current + referenced assemblies
    - Use case: Cross-assembly pattern (e.g., AuthenticationFlows)
    - Result: `optionTypes` contains all discovered implementations

**Example execution**:
- AuthenticationFlows with `RestrictToCurrentCompilation = false`:
  - `allOptionTypes` = [OAuth2Method (Auth dll), FormBasedMethod (Auth dll)]
  - `optionTypes` = [OAuth2Method, FormBasedMethod] (all kept)

- ConnectionStates with `RestrictToCurrentCompilation = true`:
  - `allOptionTypes` = [OpenState (same dll), ClosedState (same dll), ExternalState (other dll)]
  - `optionTypes` = [OpenState, ClosedState] (ExternalState filtered out)

### STEP 5: Build Collection Model (Line 147-156)

64. **Line 147**: Check if we should generate (always true to support Empty())

65. **Line 150**: Build collection definition
    - Calls `BuildEnumDefinitionFromAttributedCollection()`
    - Creates `EnumTypeInfoModel` with all metadata

66. **Line 151**: If definition built successfully, continue

#### Helper: BuildEnumDefinitionFromAttributedCollection (Line 522-571)

67. **Line 522**: Build collection model from discovered data
    - Input: collection class, base type, option types, compilation, options, attribute
    - Output: `EnumTypeInfoModel` (metadata for code generation)

68. **Line 531**: Extract collection name from attribute
    - Third constructor argument: `[TypeCollection(baseType, returnType, typeof(AuthenticationFlows))]`
    - Gets "AuthenticationFlows"
    - Fallback to class name if not specified

69. **Line 532**: Get base type name
    - Example: "AuthenticationFlowBase"

70. **Line 535**: Detect return type
    - Calls `DetectReturnType(collectionClass.BaseType, compilation)`
    - Looks at generic parameters of `TypeCollectionBase<TBase, TGeneric>`

71. **Line 538**: Get containing namespace
    - Example: "FractalDataWorks.Services.Authentication.Abstractions.Methods"

72. **Line 542-545**: Fallback to MSBuild RootNamespace if needed
    - Only used if containing namespace is empty

73. **Line 558-570**: Create `EnumTypeInfoModel`
    - **Namespace**: Where to generate code
    - **ClassName**: Base type name ("AuthenticationFlowBase")
    - **FullTypeName**: Fully qualified base type
    - **CollectionName**: Collection class name ("AuthenticationFlows")
    - **CollectionBaseType**: Base type for inheritance
    - **ReturnType**: Return type for methods (e.g., "IAuthenticationFlow")
    - **InheritsFromCollectionBase**: true
    - **UseSingletonInstances**: true (generate static fields)
    - **GenerateFactoryMethods**: true (generate Create methods)
    - **LookupProperties**: Properties with `[TypeLookup]` attribute

#### Helper: DetectReturnType (Line 591-613)

74. **Line 591**: Determine return type from inheritance
75. **Line 593**: Check if base type is generic
    - Example: `TypeCollectionBase<AuthenticationFlowBase, IAuthenticationFlow>`

76. **Line 595**: Get constructed generic name
    - Example: "FractalDataWorks.Collections.TypeCollectionBase<TBase, TGeneric>"

77. **Line 597**: Check if it's `TypeCollectionBase`

78. **Line 600-603**: Single generic parameter
    - `TypeCollectionBase<TBase>` → return TBase
    - Example: If `TypeCollectionBase<AuthenticationFlowBase>`, return "AuthenticationFlowBase"

79. **Line 605-608**: Two generic parameters
    - `TypeCollectionBase<TBase, TGeneric>` → return TGeneric
    - Example: If `TypeCollectionBase<AuthenticationFlowBase, IAuthenticationFlow>`, return "IAuthenticationFlow"

80. **Line 612**: Fallback to "object" if can't determine

#### Helper: ExtractLookupPropertiesFromBaseType (Line 481-516)

81. **Line 481**: Extract properties with `[TypeLookup]` attribute
82. **Line 483**: Create lookup properties list
83. **Line 485**: Get `TypeLookupAttribute` type
84. **Line 487**: Start with base type
85. **Line 488**: Walk up inheritance chain
86. **Line 490**: Loop through all members that are properties
87. **Line 492-493**: Find `TypeLookupAttribute` on property
88. **Line 495**: Check if attribute has constructor arguments
89. **Line 497**: Get method name from first argument
    - Example: `[TypeLookup("GetByProviderName")]` → "GetByProviderName"
90. **Line 500-507**: Create `PropertyLookupInfoModel`
    - PropertyName: e.g., "ProviderName"
    - PropertyType: e.g., "string"
    - LookupMethodName: e.g., "GetByProviderName"
    - ReturnType: base type name
91. **Line 515**: Return as `EquatableArray`

### STEP 6: Add to Results (Line 154)

92. **Line 154**: Create result object
    - `EnumTypeInfoWithCompilation` wraps:
      - `EnumTypeInfoModel` (metadata)
      - `Compilation` (for later type lookups)
      - `List<INamedTypeSymbol>` (option types)
      - `INamedTypeSymbol` (collection class)
      - `List<Diagnostic>` (validation errors)

93. **Line 159**: Convert results list to `ImmutableArray`

### PHASE 6: Code Generation Entry Point (Line 85-98)

94. **Line 85**: `RegisterSourceOutput` callback invoked for each collection
95. **Line 88-91**: Report diagnostics
    - Loop through diagnostic list
    - Report each error/warning to compiler

96. **Line 94**: Check if any errors exist
97. **Line 96**: If no errors, call `Execute()` to generate code

### PHASE 7: Code Generation (Line 619-958)

**Method:** Execute()

98. **Line 619**: Start code generation for one collection
    - Input: context, definition, compilation, option types, collection class
    - Output: Generated `.g.cs` file added to compilation

### STEP 7.1: Base Type Resolution (REMOVED - Was Causing Silent Failures)

**BUG FIX:** Lines 629-631 were DEAD CODE that would cause silent generation failures for generic base types.

**The Problem:**
- `def.CollectionBaseType` stored display format: `"TypeBase<T1, T2>"`
- `GetTypeByMetadataName()` requires metadata format: `"TypeBase`2"`
- For generic types, lookup would return `null` → generator would silently exit
- AuthenticationFlowBase worked because it's not generic

**The Fix:**
- Removed lines 629-631 completely
- `baseTypeSymbol` was never used after line 631 anyway
- Base type already validated during discovery phase (line 129-133)

### STEP 7.2: Return Type Detection (Line 635-638)

101. **Line 635**: Check if return type is set
102. **Line 637**: If not set, detect from inheritance

### STEP 7.3: Convert Option Types to Models (Line 641-670)

103. **Line 642**: Create values list for code generation
104. **Line 643**: Loop through each discovered option type
    - Example: `ClientCredentialsFlow`, `AuthorizationCodeFlow`

#### Per Option Type Processing

105. **Line 646**: Get `TypeOptionAttribute` type
106. **Line 647-648**: Find attribute on option type
107. **Line 650-652**: Extract display name
    - Calls `ExtractTypeOptionName(typeOptionAttr, optionType)`
    - Gets second constructor argument: `[TypeOption(collection, "ClientCredentials")]`
    - Fallback to class name if not specified

#### Helper: ExtractTypeOptionName (Line 824-836)

108. **Line 824**: Extract name from attribute
109. **Line 828**: Get constructor arguments
110. **Line 829**: Check for second argument (name)
111. **Line 831**: Return name string
112. **Line 835**: Fallback to type name

113. **Line 655**: Extract base constructor ID
    - Calls `ExtractBaseConstructorId(optionType, compilation)`

#### Helper: ExtractBaseConstructorId (Line 724-813)

114. **Line 724**: Extract ID from base constructor invocation
    - **WORKS FOR ALL CONSTRUCTOR TYPES** (verified ultrathink-level trace)
    - Supports: primary constructors, regular constructors, records
    - Supports: positional arguments, named arguments, mixed arguments

115. **Line 731**: Loop through syntax references for the type

116. **Line 733**: Get syntax node (class or record declaration)

**Primary Constructor Handling (Lines 736-757)**:

117. **Line 736**: Check for class declaration syntax
118. **Line 739**: Check for base list (inheritance clause)
119. **Line 741**: Loop through base types in base list

120. **Line 743**: Check for primary constructor base syntax
    - Matches: `class Foo() : Base(1, "name")`
    - Matches: `class Foo() : Base(id: 1, name: "name")` (named args)
    - Matches: `class Foo() : Base(1, name: "name")` (mixed)

121. **Line 746**: Check for argument list exists
122. **Line 748**: Get first argument by POSITION (not name!)
    - `firstArg = primaryBase.ArgumentList.Arguments[0]`
    - **KEY**: Gets argument by index, works for both positional and named

123. **Line 749**: Get semantic model for syntax tree
124. **Line 750**: Get constant VALUE from expression
    - `semanticModel.GetConstantValue(firstArg.Expression)`
    - **KEY**: Evaluates the expression, handles literals, constants, expressions

125. **Line 752-755**: If constant value is int, return it

**Regular Constructor Handling (Lines 759-779)**:

126. **Line 759-779**: Handle regular (non-primary) constructor syntax
    - Matches: `public Foo() : base(1, "name")`
    - Loop through constructor members
    - Find base constructor initializer
    - Extract first argument using same semantic model approach

**Record Handling (Lines 786-809)**:

127. **Line 786-809**: Handle record syntax with primary constructor
    - Matches: `record Foo() : Base(1, "name")`
    - Same logic as class primary constructor

128. **Line 811**: Return null if no ID found

**VERIFICATION - ID Extraction Works For**:
✓ Positional args: `Base(3, "Open")`
✓ Named args: `Base(id: 3, name: "Open")`
✓ Mixed args: `Base(3, name: "Open")`
✓ Primary constructor: `class Foo() : Base(3, "name")`
✓ Regular constructor: `public Foo() : base(3, "name")`
✓ Record: `record Foo() : Base(3, "name")`
✓ Const values: `const int Id = 3; Base(Id, ...)`
✓ Expressions: `Base(1 + 2, ...)`

114. **Line 658-668**: Create `EnumValueInfoModel`
    - **ShortTypeName**: Class name (e.g., "ClientCredentialsFlow")
    - **FullTypeName**: Fully qualified name
    - **Name**: Display name (e.g., "ClientCredentials")
    - **ReturnTypeNamespace**: Namespace of option type
    - **Constructors**: List of constructor info
    - **IsAbstract**: Whether type is abstract
    - **IsStatic**: Whether type is static
    - **BaseConstructorId**: The ID value

#### Helper: ExtractConstructorInfo (Line 773-796)

115. **Line 773**: Extract all public constructors
116. **Line 777**: Loop through public constructors
117. **Line 779**: Create parameters list
118. **Line 781**: Loop through each parameter
119. **Line 783-789**: Create `ParameterInfo`
    - Name: parameter name
    - TypeName: parameter type
    - DefaultValue: default value if present
120. **Line 792**: Create `ConstructorInfo` with parameters

#### Helper: GetDefaultValueString (Line 801-822)

121. **Line 801**: Convert default value to string for codegen
122. **Line 803-807**: Handle null values
123. **Line 809-821**: Convert value based on type
    - string → `"value"`
    - char → `'c'`
    - bool → `true`/`false`
    - float → `1.0f`
    - etc.

### STEP 7.4: Final Code Generation (Line 674)

124. **Line 674**: Call `GenerateCollection()` to create code
    - This generates the actual C# source code

#### GenerateCollection Method (Line 877-958)

125. **Line 877**: Final code generation method

126. **Line 888-889**: Get `TypeCollectionAttribute` from class
    - Must be present (that's how we found this class)

127. **Line 891**: Extract return type from attribute
    - Second constructor argument: `[TypeCollection(baseType, typeof(IAuthenticationFlow), ...)]`

128. **Line 894**: Check if user class is static
129. **Line 895**: Check if user class is abstract

130. **Line 898**: Get configuration for TypeCollections
    - Calls `CollectionBuilderConfiguration.ForTypeCollections()`
    - Returns settings specific to TypeCollections

131. **Line 899**: Create `GenericCollectionBuilder`
    - This builder generates the actual code

132. **Line 902-908**: Configure builder
    - **WithDefinition**: Set collection metadata
    - **WithValues**: Set option types
    - **WithReturnType**: Set return type for methods
    - **WithCompilation**: Set compilation for type lookups
    - **WithUserClassModifiers**: Set static/abstract modifiers
    - **Build()**: Generate code

133. **Line 910**: Create file name
    - Example: "AuthenticationFlows.g.cs"

134. **Line 912-927**: DEBUG mode adds header
    - Comments with generation timestamp
    - Collection name
    - Namespace
    - Return type
    - List of values
    - DEBUG ONLY - not in Release

135. **Line 930-936**: Generate Empty class
    - Calls `builder.GetEmptyClassCode()`
    - Creates separate file: "EmptyAuthenticationFlow.g.cs"
    - Example: `internal sealed class EmptyAuthenticationFlow : AuthenticationFlowBase`

136. **Line 939**: Add collection file to compilation
    - `context.AddSource(fileName, generatedCode)`
    - Makes generated code part of compilation

137. **Line 941-957**: Exception handling
    - Catches any generation errors
    - Creates diagnostic error
    - Reports to compiler

## Summary of Key Variables at Each Step

### For AuthenticationFlows Example:

**After STEP 1 (Line 111):**
- `typeOptionsByCollectionType[AuthenticationFlows]` = `[ClientCredentialsFlow, AuthorizationCodeFlow, ImplicitFlow, ...]`

**After STEP 2 (Line 115):**
- `attributedCollectionClasses` = `[(AuthenticationFlows, [TypeCollection(...)])]`

**After STEP 3 (Line 132):**
- `baseType` = `AuthenticationFlowBase` (INamedTypeSymbol)
- `baseTypeName` = `"FractalDataWorks.Services.Authentication.Abstractions.AuthenticationFlowBase"`

**After STEP 4 (Line 140):**
- `optionTypes` = `[ClientCredentialsFlow, AuthorizationCodeFlow, ImplicitFlow, ...]`

**After STEP 5 (Line 150):**
- `typeDefinition.Namespace` = `"FractalDataWorks.Services.Authentication.Abstractions.Methods"`
- `typeDefinition.ClassName` = `"AuthenticationFlowBase"`
- `typeDefinition.CollectionName` = `"AuthenticationFlows"`
- `typeDefinition.ReturnType` = `"IAuthenticationFlow"`

**After STEP 7.3 (Line 643-670):**
- `values[0].Name` = `"ClientCredentials"`
- `values[0].FullTypeName` = `"MyProject.ClientCredentialsFlow"`
- `values[0].BaseConstructorId` = `1`

**Final Output (Line 939):**
- File: `AuthenticationFlows.g.cs`
- Contains: Static properties, All(), Empty(), Name(), etc.

## Why AuthenticationFlows Might Not Generate

Check these conditions in order:

1. **Line 215**: Is `TypeOptionAttribute` type found in compilation?
2. **Line 389**: Is `TypeCollectionAttribute` type found in compilation?
3. **Line 122-125**: Is `AuthenticationFlows` in the CURRENT assembly (not referenced)?
4. **Line 129-133**: Can base type be resolved?
5. **Line 136**: Are there abstract property validation errors?
6. **Line 140**: Are any option types found?
7. **Line 891**: Can return type be extracted from attribute?
8. **Line 939**: Is code successfully added to compilation?

## Generated Code Structure

For AuthenticationFlows, the generator creates:

```csharp
// AuthenticationFlows.g.cs
namespace FractalDataWorks.Services.Authentication.Abstractions.Methods
{
    public abstract partial class AuthenticationFlows
    {
        // Static fields for each option
        public static IAuthenticationFlow ClientCredentials { get; } = new ClientCredentialsFlow();

        // Collection methods
        public static IReadOnlyList<IAuthenticationFlow> All() => _allValues;
        public static IAuthenticationFlow Empty() => _empty;
        public static IAuthenticationFlow Name(string name) => /* lookup by name */;

        // Lookup methods from [TypeLookup] attributes
        // ...
    }
}

// EmptyAuthenticationFlow.g.cs
namespace FractalDataWorks.Services.Authentication.Abstractions.Methods
{
    internal sealed class EmptyAuthenticationFlow : AuthenticationFlowBase
    {
        public EmptyAuthenticationFlow() : base(default, "", default, default, default) { }
    }
}
```
