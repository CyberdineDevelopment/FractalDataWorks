# ServiceTypeCollectionGenerator - Step-by-Step Execution Flow

This document provides a detailed, line-by-line explanation of how the ServiceTypeCollectionGenerator discovers and generates ServiceType collections.

## Overview

The ServiceTypeCollectionGenerator creates high-performance ServiceType collections using attribute-based discovery. It scans the compilation for:
1. Classes marked with `[ServiceTypeCollection(baseType, returnType, collectionType)]` - the collection definitions
2. Classes marked with `[ServiceTypeOption(collectionType, name)]` - the ServiceTypes to include in collections

## Example: AuthenticationTypes

Given this code:
```csharp
// The collection definition
[ServiceTypeCollection(typeof(AuthenticationTypeBase<,,>), typeof(IAuthenticationType), typeof(AuthenticationTypes))]
public partial class AuthenticationTypes : ServiceTypeCollectionBase<...>
{
}

// The base type that ServiceTypes inherit from
public abstract class AuthenticationTypeBase<TService, TFactory, TConfiguration> : ServiceTypeBase<TService, TFactory, TConfiguration>
{
    protected AuthenticationTypeBase(int id, string name, string providerName, ...)
        : base(id, name, ...) { ... }
}

// Example ServiceTypes (would be in other projects)
[ServiceTypeOption(typeof(AuthenticationTypes), "MicrosoftEntra")]
public sealed class MicrosoftEntraAuthenticationType : AuthenticationTypeBase<...>
{
    public static MicrosoftEntraAuthenticationType Instance { get; } = new();
    private MicrosoftEntraAuthenticationType() : base(1, "MicrosoftEntra", ...) { }
}
```

The generator will create:
```csharp
public partial class AuthenticationTypes
{
    public static IAuthenticationType MicrosoftEntra => MicrosoftEntraAuthenticationType.Instance;
    public static IReadOnlyList<IAuthenticationType> All() => ...;
    public static IAuthenticationType Empty() => ...;
    // etc.
}
```

## Detailed Step-by-Step Execution

### PHASE 0: Generator Initialization (Line 53-69)

**File:** ServiceTypeCollectionGenerator.cs
**Method:** Initialize()

1. **Line 53**: `Initialize()` method called by Roslyn when compilation starts
2. **Line 64-69**: ALWAYS create init file to prove generator loaded
   - Creates `ServiceTypeCollectionGenerator.Init.g.cs` with timestamp
   - This file appears in `obj/` if `EmitCompilerGeneratedFiles` is true
   - NOT debug-only, always runs

### PHASE 1: Pipeline Setup (Line 72-95)

**File:** ServiceTypeCollectionGenerator.cs
**Method:** Initialize()

3. **Line 72**: Create discovery pipeline
   - `context.CompilationProvider` = current compilation being built
   - Uses `Select` to transform compilation into discovery results

4. **Line 73-77**: Register discovery transformation
   - `Select` = for each compilation, discover all collections
   - Calls `DiscoverServiceTypeCollectionsOptimized()` for actual work
   - Returns `ImmutableArray<ServiceTypeInfoWithCompilation>`

5. **Line 79-95**: Register code generation pipeline
   - `RegisterSourceOutput` = for each discovered collection, generate code
   - **Line 84-87**: Report diagnostics (errors/warnings)
   - **Line 90-93**: Generate code only if no errors
   - Calls `Execute()` for code generation

### PHASE 2: Discovery Entry Point (Line 147-192)

**File:** ServiceTypeCollectionGenerator.cs
**Method:** DiscoverServiceTypeCollectionsOptimized()

6. **Line 147**: Entry point for discovery phase
   - Input: `Compilation` (all syntax trees)
   - Output: `ImmutableArray<ServiceTypeInfoWithCompilation>` (all discovered collections)

7. **Line 149**: Create results list to accumulate discovered collections

### PHASE 3: STEP 1 - Discover All ServiceTypeOption Attributes (Line 153)

8. **Line 153**: Call `FindAndGroupAllServiceTypeOptions(compilation)`
   - This is the FIRST scan: find all `[ServiceTypeOption]` attributes in entire solution
   - Groups them by which collection they belong to
   - Returns: `Dictionary<INamedTypeSymbol, List<INamedTypeSymbol>>`
     - Key = collection class (e.g., `AuthenticationTypes`)
     - Value = list of ServiceType option types (e.g., `MicrosoftEntraAuthenticationType`, `Auth0AuthenticationType`)

#### STEP 1.1: FindAndGroupAllServiceTypeOptions Details (Line 199-264)

9. **Line 199**: Start of `FindAndGroupAllServiceTypeOptions()`
   - Creates dictionary to hold results: `serviceOptionsByCollectionType`

10. **Line 201**: Create empty dictionary for grouping results
    - Key: collection type symbol (e.g., `AuthenticationTypes`)
    - Value: list of ServiceType option symbols (e.g., `[MicrosoftEntraAuthenticationType, Auth0AuthenticationType]`)
    - Uses `SymbolEqualityComparer.Default` for symbol comparison

11. **Line 202**: Get the `ServiceTypeOptionAttribute` type from compilation
    - `compilation.GetTypeByMetadataName()` looks up type by full name
    - Full name: `"FractalDataWorks.ServiceTypes.Attributes.ServiceTypeOptionAttribute"`

12. **Line 204**: Check if attribute type was found
    - If not found, return empty dictionary (no options to discover)

13. **Line 207**: Create list to hold all discovered options with their attributes
    - List of tuples: `(INamedTypeSymbol Type, AttributeData Attribute)`

14. **Line 210**: Scan current compilation's global namespace
    - Calls `ScanNamespaceForServiceTypeOptionsWithAttributes()`
    - Recursively walks ALL namespaces in current project
    - Finds every type with `[ServiceTypeOption]` attribute

15. **Line 213-219**: Scan all referenced assemblies
    - Loop through `compilation.References` (all DLLs this project references)
    - For each assembly, scan its global namespace
    - This finds ServiceTypes defined in other projects

16. **Line 223**: Create dictionary to track most derived types
    - Outer key: collection type
    - Inner key: full type name string
    - Inner value: most derived version of that type
    - Purpose: Deduplication when same type appears in multiple assemblies

17. **Line 225-254**: Group discovered options by collection type
    - **Line 225**: Loop through all found (Type, Attribute) pairs
    - **Line 227**: Extract collection type from attribute
      - Calls `ExtractCollectionTypeFromServiceTypeOptionAttribute()`
      - Gets first constructor argument of `[ServiceTypeOption(typeof(AuthenticationTypes), "Name")]`
    - **Line 228**: If collection type found, continue
    - **Line 230-234**: Get or create inner dictionary for this collection
    - **Line 236**: Get full type name (e.g., "MyNamespace.MicrosoftEntraAuthenticationType")
    - **Line 239-251**: Deduplication logic
      - If we've seen this type name before, check which is more derived
      - Uses `IsDerivedFrom()` to determine inheritance relationship
      - Keeps the most derived version
      - Prevents duplicate entries when same type appears in multiple assemblies

18. **Line 257-261**: Convert nested dictionary back to simple dictionary
    - Flatten: `Dictionary<Collection, Dictionary<Name, Type>>` → `Dictionary<Collection, List<Type>>`

19. **Line 263**: Return the grouped options dictionary

#### Helper: IsDerivedFrom (Line 269-281)

20. **Line 269**: Check if candidateType inherits from baseType
21. **Line 271**: Get base type of candidate
22. **Line 272-280**: Walk up inheritance chain
    - **Line 274-277**: Compare with base type using SymbolEqualityComparer
    - **Line 278**: Move to next base type
23. **Line 280**: Return false if not found

#### Helper: ScanNamespaceForServiceTypeOptionsWithAttributes (Line 286-306)

24. **Line 286**: Recursive namespace scanner
25. **Line 289-298**: Scan all types in current namespace
    - **Line 291**: Call `GetServiceTypeOptionAttribute()` for each type
    - **Line 292-295**: If attribute found, add to results
    - **Line 298**: Recursively scan nested types
26. **Line 301-305**: Recursively scan child namespaces

#### Helper: ScanNestedTypesForServiceTypeOptionWithAttributes (Line 311-324)

27. **Line 311**: Recursive nested type scanner
28. **Line 313-322**: Loop through nested types
    - **Line 315**: Get ServiceTypeOption attribute
    - **Line 316-319**: Add to results if found
    - **Line 322**: Recursively scan deeper nested types

#### Helper: GetServiceTypeOptionAttribute (Line 329-333)

29. **Line 329**: Get ServiceTypeOption attribute from a type
30. **Line 331**: Get all attributes on type
31. **Line 332**: Find first attribute matching `ServiceTypeOptionAttribute`
    - Uses `SymbolEqualityComparer` to compare attribute types

#### Helper: ExtractCollectionTypeFromServiceTypeOptionAttribute (Line 338-350)

32. **Line 338**: Extract collection type from attribute
33. **Line 341**: Check if attribute has constructor arguments
34. **Line 343**: Get first constructor argument
35. **Line 344**: Check if it's a Type argument (`INamedTypeSymbol`)
    - `[ServiceTypeOption(typeof(AuthenticationTypes), "Name")]`
    - First arg: `typeof(AuthenticationTypes)` → `INamedTypeSymbol` for `AuthenticationTypes`
36. **Line 345**: Return the collection type symbol
37. **Line 349**: Return null if not found

### PHASE 4: STEP 2 - Discover All ServiceTypeCollection Attributes (Line 156)

38. **Line 156**: Call `FindAttributedCollectionClasses(compilation)`
    - This is the SECOND scan: find all `[ServiceTypeCollection]` attributes
    - Returns: `List<(INamedTypeSymbol CollectionClass, AttributeData Attribute)>`

#### STEP 2.1: FindAttributedCollectionClasses Details (Line 391-402)

39. **Line 391**: Start of `FindAttributedCollectionClasses()`
40. **Line 393**: Create results list
41. **Line 394**: Get `ServiceTypeCollectionAttribute` type from compilation
    - Full name: `"FractalDataWorks.ServiceTypes.Attributes.ServiceTypeCollectionAttribute"`
42. **Line 396**: Check if attribute type was found
43. **Line 399**: Scan ONLY current compilation's global namespace
    - Calls `ScanNamespaceForAttributedClasses()`
    - Does NOT scan referenced assemblies (line 398 comment)
44. **Line 401**: Return list of collection classes with their attributes

#### Helper: ScanNamespaceForAttributedClasses (Line 407-429)

45. **Line 407**: Recursive scanner for attributed classes
46. **Line 413-421**: Scan types in current namespace
    - **Line 415**: Get all attributes on type
    - **Line 416**: Find `ServiceTypeCollectionAttribute`
    - **Line 418-421**: If found, add to results
47. **Line 424-428**: Recursively scan child namespaces

### PHASE 5: STEP 3-6 - Process Each Collection (Line 160-189)

48. **Line 160**: Loop through each discovered collection class
    - Example: `AuthenticationTypes` class with `[ServiceTypeCollection(...)]`

#### Collection Processing - Per Collection

### STEP 3: Extract Base Type (Line 163-164)

49. **Line 163**: Extract base type from attribute
    - Calls `ExtractBaseTypeFromAttribute(attribute)`
    - `[ServiceTypeCollection(typeof(AuthenticationTypeBase<,,>), ...)]`
    - Gets first constructor argument

50. **Line 164**: If no base type found, skip this collection

#### Helper: ExtractBaseTypeFromAttribute (Line 570-578)

51. **Line 570**: Extract base type symbol from attribute
52. **Line 573**: Check if first constructor argument exists and is Type symbol
    - `[ServiceTypeCollection(typeof(BaseType), ...)]`
    - First arg: `typeof(BaseType)` → `INamedTypeSymbol`
53. **Line 575**: Return the base type symbol
54. **Line 577**: Return null if not found

### STEP 3.1: Validate No Abstract Properties (Line 167)

55. **Line 167**: Validate base type doesn't have abstract properties
    - Calls `ValidateNoAbstractProperties(baseType, collectionClass)`
    - Returns list of diagnostics (errors)

#### Helper: ValidateNoAbstractProperties (Line 102-141)

56. **Line 102**: Validation method for abstract properties
57. **Line 104**: Create diagnostics list
58. **Line 105**: Start with base type

59. **Line 108**: Walk up inheritance chain
    - Check base type, then its base, etc.

60. **Line 110**: Loop through all members of current type
61. **Line 112**: Check if member is abstract property
    - `member is IPropertySymbol property && property.IsAbstract`

62. **Line 115**: Get property location for error reporting
63. **Line 118-123**: Fallback to attribute location if property location unavailable

64. **Line 125-133**: Create diagnostic error
    - Error ID: "ST006"
    - Message: "The base type '{baseType}' contains abstract property '{propertyName}'"
    - Description: ServiceType base types should only have abstract methods, not properties

65. **Line 133**: Add diagnostic to list
66. **Line 140**: Return diagnostics list

### STEP 4: Lookup Pre-Discovered ServiceTypes (Line 171-175)

67. **Line 171**: ULTRA-FAST lookup of ServiceTypes (O(1) dictionary lookup)
    - Look in `serviceOptionsByCollectionType` dictionary from STEP 1
    - Key = `collectionClass` (e.g., `AuthenticationTypes`)
    - Value = list of ServiceType option types we discovered earlier
    - **IMPORTANT FIX (Line 170 comment)**: Lookup by collectionClass, NOT baseType

68. **Line 171-174**: If no ServiceTypes found, create empty list
    - Collections can have zero ServiceTypes (will still generate Empty() method)

69. **Line 175**: Convert to immutable array

### STEP 5: Build Collection Model (Line 178-188)

70. **Line 179**: Check if we should generate (always true to support Empty())

71. **Line 182**: Build collection definition
    - Calls `BuildServiceTypeCollectionDefinitionFromAttributedCollection()`
    - Creates `EnumTypeInfoModel` with all metadata

72. **Line 183**: If definition built successfully, continue

#### Helper: BuildServiceTypeCollectionDefinitionFromAttributedCollection (Line 583-615)

73. **Line 583**: Build collection model from discovered data
    - Input: collection class, base type, ServiceType list, compilation, attribute
    - Output: `EnumTypeInfoModel` (metadata for code generation)

74. **Line 591-593**: Extract collection name from attribute
    - Third constructor argument: `[ServiceTypeCollection(baseType, returnType, typeof(AuthenticationTypes))]`
    - Gets "AuthenticationTypes"
    - Line 593 fallback: Call `DeriveName(collectionClass.Name)` if not in attribute

#### Helper: DeriveName (Line 434-439)

75. **Line 434**: Derive collection name by removing "Base" suffix
76. **Line 436-438**: If class name ends with "Base", remove it
    - Example: "ConnectionTypeCollectionBase" → "ConnectionTypeCollection"
77. **Line 437**: Return modified name or original

78. **Line 596-598**: Extract default return type from attribute
    - Second constructor argument: `[ServiceTypeCollection(baseType, typeof(IAuthenticationType), ...)]`
    - Gets "IAuthenticationType"
    - Fallback to base type name if not specified

79. **Line 600**: Get containing namespace
    - Example: "FractalDataWorks.Services.Authentication.Abstractions"

80. **Line 602-614**: Create `EnumTypeInfoModel`
    - **Namespace**: Where to generate code
    - **ClassName**: Base type name (e.g., "AuthenticationTypeBase")
    - **FullTypeName**: Fully qualified base type
    - **CollectionName**: Collection class name (e.g., "AuthenticationTypes")
    - **CollectionBaseType**: Base type for inheritance
    - **ReturnType**: Return type for methods (e.g., "IAuthenticationType")
    - **InheritsFromCollectionBase**: true
    - **UseSingletonInstances**: true (ServiceTypes are singletons)
    - **GenerateFactoryMethods**: true
    - **LookupProperties**: Properties with `[ServiceTypeLookup]` attribute (Line 613)

#### Helper: ExtractServiceTypeLookupProperties (Line 683-718)

81. **Line 683**: Extract properties with `[ServiceTypeLookup]` attribute
82. **Line 685**: Create lookup properties list
83. **Line 687**: Get `ServiceTypeLookupAttribute` type
84. **Line 689**: Start with base type
85. **Line 690**: Walk up inheritance chain
86. **Line 692**: Loop through all members that are properties
87. **Line 694-695**: Find `ServiceTypeLookupAttribute` on property
88. **Line 697**: Check if attribute has constructor arguments
89. **Line 699**: Get method name from first argument
    - Example: `[ServiceTypeLookup("GetByProviderName")]` → "GetByProviderName"
90. **Line 700-710**: Create `PropertyLookupInfoModel`
    - PropertyName: e.g., "ProviderName"
    - PropertyType: e.g., "string"
    - LookupMethodName: e.g., "GetByProviderName"
    - AllowMultiple: false
    - ReturnType: base type name
91. **Line 717**: Return as `EquatableArray`

### STEP 6: Add to Results (Line 186)

92. **Line 186**: Create result object
    - `ServiceTypeInfoWithCompilation` wraps:
      - `EnumTypeInfoModel` (metadata)
      - `Compilation` (for later type lookups)
      - `ImmutableArray<INamedTypeSymbol>` (ServiceType option types)
      - `INamedTypeSymbol` (collection class)
      - `List<Diagnostic>` (validation errors)

93. **Line 191**: Convert results list to `ImmutableArray`

### PHASE 6: Code Generation Entry Point (Line 79-95)

94. **Line 79**: `RegisterSourceOutput` callback invoked for each collection
95. **Line 84-87**: Report diagnostics
    - Loop through diagnostic list
    - Report each error/warning to compiler

96. **Line 90**: Check if any errors exist
97. **Line 92**: If no errors, call `Execute()` to generate code

### PHASE 7: Code Generation (Line 725-1009)

**Method:** Execute()

98. **Line 725**: Start code generation for one collection
    - Input: context, definition, compilation, ServiceType list, collection class
    - Output: Generated `.g.cs` file added to compilation

### STEP 7.1: Base Type Resolution (REMOVED - Was Causing Silent Failures)

**BUG FIX:** Lines 736-737 were DEAD CODE that caused silent generation failures for generic base types.

**The Problem:**
- `def.CollectionBaseType` stored display format: `"AuthenticationTypeBase<TService, TFactory, TConfiguration>"`
- `GetTypeByMetadataName()` requires metadata format: `"AuthenticationTypeBase`3"`
- For generic types, lookup returned `null` → generator silently exited
- Non-generic types (like `AuthenticationFlowBase`) worked fine

**Why It Failed:**
```csharp
// What we were trying:
var baseTypeSymbol = compilation.GetTypeByMetadataName("Namespace.Type<T1, T2, T3>"); // Returns NULL!

// What would work:
var baseTypeSymbol = compilation.GetTypeByMetadataName("Namespace.Type`3"); // Returns symbol
```

**The Fix:**
- Removed lines 736-737 completely
- `baseTypeSymbol` was never used after line 737 anyway
- Base type already validated during discovery phase (line 163-164)

### STEP 7.2: Return Type Detection (Line 740-743)

101. **Line 740**: Check if return type is set
102. **Line 742**: If not set, default to "FractalDataWorks.ServiceTypes.ServiceTypeBase"

### STEP 7.3: Convert ServiceTypes to Models (Line 746-764)

103. **Line 746**: Create values list for code generation
104. **Line 747**: Loop through each discovered ServiceType
    - Example: `MicrosoftEntraAuthenticationType`, `Auth0AuthenticationType`

#### Per ServiceType Processing

105. **Line 749**: Get ServiceType name from symbol
    - Example: "MicrosoftEntraAuthenticationType"

106. **Line 752**: Extract base constructor ID
    - Calls `ExtractBaseConstructorId(serviceType, compilation)`

#### Helper: ExtractBaseConstructorId (Line 865-931)

107. **Line 865**: Extract ID from base constructor
    - Example: `public sealed class OpenState() : ConnectionStateBase(3, "Open")`
    - Extracts the "3"

108. **Line 868**: Loop through syntax references for this type

109. **Line 873**: Get syntax node

110. **Line 876**: Check for class declaration
111. **Line 879**: Check for primary constructor AND base list
    - Primary constructor: `class Foo() : Base(args)`

112. **Line 882**: Loop through base types in base list
113. **Line 884**: Check for simple or identifier name syntax

114. **Line 888**: Check for primary constructor base type syntax
    - `class Foo() : Base(3, "name")`

115. **Line 889-901**: Extract first argument
    - **Line 889**: Check for argument list
    - **Line 893**: Get first argument
    - **Line 896-900**: Check if it's literal integer, return it

116. **Line 906-926**: Check for traditional constructor with base call
    - Look for constructor with base initializer
    - Extract first argument value from base call

117. **Line 930**: Return null if no ID found

108. **Line 754-763**: Create `EnumValueInfoModel`
    - **ShortTypeName**: Class name (e.g., "MicrosoftEntraAuthenticationType")
    - **FullTypeName**: Fully qualified name
    - **Name**: Display name (from attribute or class name)
    - **ReturnTypeNamespace**: Namespace of ServiceType
    - **Constructors**: List of constructor info (Line 760)
    - **BaseConstructorId**: The ID value (Line 761)

#### Helper: ExtractConstructorInfo (Line 773-796)

109. **Line 773**: Extract all public constructors
110. **Line 777**: Loop through public constructors
111. **Line 779**: Create parameters list
112. **Line 781**: Loop through each parameter
113. **Line 783-789**: Create `ParameterInfo`
    - Name: parameter name
    - TypeName: parameter type
    - DefaultValue: default value if present (Line 787-788)

#### Helper: GetDefaultValueString (Line 801-822)

114. **Line 801**: Convert default value to string for codegen
115. **Line 803-807**: Handle null values
    - Check if reference type or nullable
    - Return "null" or "default"
116. **Line 809-821**: Convert value based on type
    - string → `"value"`
    - char → `'c'`
    - bool → `true`/`false`
    - float → `1.0f`
    - decimal → `1.0m`
    - long → `1L`
    - uint → `1u`
    - ulong → `1ul`
    - default → ToString()

117. **Line 792**: Create `ConstructorInfo` with parameters list

### STEP 7.4: Final Code Generation (Line 767)

118. **Line 767**: Call `GenerateServiceTypeCollection()` to create code
    - This generates the actual C# source code

#### GenerateServiceTypeCollection Method (Line 936-1009)

119. **Line 936**: Final code generation method

120. **Line 945**: Determine effective return type
    - Calls `DetermineReturnType(collectionClass)`

#### Helper: DetermineReturnType (Line 828-857)

121. **Line 828**: Determine return type from inheritance
122. **Line 830**: Get base type of collection class

123. **Line 832**: Walk up inheritance chain
124. **Line 834**: Check if current type is generic

125. **Line 836**: Get generic definition
    - Example: `ServiceTypeCollectionBase<TBase, TGeneric, ...>`

126. **Line 839-843**: Check for ServiceTypeCollectionBase with 2+ generics
    - Return TGeneric (second type parameter)
    - Example: `ServiceTypeCollectionBase<AuthenticationTypeBase, IAuthenticationType, ...>`
    - Returns "IAuthenticationType"

127. **Line 846-850**: Check for ServiceTypeCollectionBase with 1 generic
    - Return TBase (first type parameter)

128. **Line 853**: Move to next base type
129. **Line 856**: Return null if can't determine

130. **Line 946-951**: Fallback if return type not determined
    - Use base class name (ClassName from definition)

131. **Line 954**: Check if user class is static
132. **Line 955**: Check if user class is abstract

133. **Line 958**: Get configuration for ServiceTypeCollections
    - Calls `CollectionBuilderConfiguration.ForServiceTypeCollections()`
    - Returns settings specific to ServiceTypeCollections

134. **Line 959**: Create `GenericCollectionBuilder`
    - This builder generates the actual code

135. **Line 962-968**: Configure builder
    - **WithDefinition**: Set collection metadata
    - **WithValues**: Set ServiceType option types (cast to GenericValueInfoModel)
    - **WithReturnType**: Set return type for methods
    - **WithCompilation**: Set compilation for type lookups
    - **WithUserClassModifiers**: Set static/abstract modifiers
    - **Build()**: Generate code

136. **Line 970**: Create file name
    - Example: "AuthenticationTypes.g.cs"

137. **Line 972-989**: DEBUG mode adds header
    - Comments with generation timestamp
    - Collection name
    - Namespace
    - Class name
    - Return type
    - Discovered value count
    - List of values
    - Lookup properties
    - DEBUG ONLY - not in Release

138. **Line 990**: Add generated code to context
    - `context.AddSource(fileName, generatedCode)`
    - Makes generated code part of compilation

139. **Line 992-1007**: Exception handling
    - Catches any generation errors
    - Creates diagnostic error
    - Error ID: "STCG001"
    - Reports to compiler

## Summary of Key Variables at Each Step

### For AuthenticationTypes Example:

**After STEP 1 (Line 153):**
- `serviceOptionsByCollectionType[AuthenticationTypes]` = `[MicrosoftEntraAuthenticationType, Auth0AuthenticationType, ...]`

**After STEP 2 (Line 156):**
- `attributedCollectionClasses` = `[(AuthenticationTypes, [ServiceTypeCollection(...)])]`

**After STEP 3 (Line 163):**
- `baseType` = `AuthenticationTypeBase<,,>` (INamedTypeSymbol)

**After STEP 4 (Line 171):**
- `serviceTypes` = `[MicrosoftEntraAuthenticationType, Auth0AuthenticationType, ...]`

**After STEP 5 (Line 182):**
- `typeDefinition.Namespace` = `"FractalDataWorks.Services.Authentication.Abstractions"`
- `typeDefinition.ClassName` = `"AuthenticationTypeBase"`
- `typeDefinition.CollectionName` = `"AuthenticationTypes"`
- `typeDefinition.ReturnType` = `"IAuthenticationType"`
- `typeDefinition.LookupProperties` = `[{PropertyName="ProviderName", LookupMethodName="GetByProviderName"}]`

**After STEP 7.3 (Line 747-764):**
- `values[0].Name` = `"MicrosoftEntraAuthenticationType"`
- `values[0].FullTypeName` = `"MyProject.MicrosoftEntraAuthenticationType"`
- `values[0].BaseConstructorId` = `1`

**Final Output (Line 990):**
- File: `AuthenticationTypes.g.cs`
- Contains: Static properties (referencing Instance), All(), Empty(), Name(), GetByProviderName(), etc.

## Why AuthenticationTypes Might Not Generate

Check these conditions in order:

1. **Line 202**: Is `ServiceTypeOptionAttribute` type found in compilation?
2. **Line 394**: Is `ServiceTypeCollectionAttribute` type found in compilation?
3. **Line 163**: Can base type be extracted from attribute?
4. **Line 167**: Are there abstract property validation errors?
5. **Line 171**: Are any ServiceType options found in dictionary?
6. **Line 182**: Is type definition built successfully?
7. **Line 945**: Can return type be determined?
8. **Line 990**: Is code successfully added to compilation?

## Differences from TypeCollectionGenerator

1. **Singleton Pattern**: ServiceTypes use `.Instance` property instead of `new()`
2. **No Empty Class Generation**: ServiceTypes don't generate separate Empty class files
3. **Attribute Names**: Uses `ServiceTypeOption` and `ServiceTypeCollection` instead of `TypeOption` and `TypeCollection`
4. **Lookup Attributes**: Uses `[ServiceTypeLookup]` instead of `[TypeLookup]`
5. **No Current Assembly Check**: Line 122-126 in TypeCollectionGenerator doesn't exist here
6. **Different Base Classes**: `ServiceTypeCollectionBase` vs `TypeCollectionBase`

## Generated Code Structure

For AuthenticationTypes, the generator creates:

```csharp
// AuthenticationTypes.g.cs
namespace FractalDataWorks.Services.Authentication.Abstractions
{
    public partial class AuthenticationTypes
    {
        // Static properties referencing singletons
        public static IAuthenticationType MicrosoftEntra => MicrosoftEntraAuthenticationType.Instance;

        // Collection methods
        public static IReadOnlyList<IAuthenticationType> All() => _allValues;
        public static IAuthenticationType Empty() => _empty;
        public static IAuthenticationType Name(string name) => /* lookup by name */;

        // Lookup methods from [ServiceTypeLookup] attributes
        public static IAuthenticationType GetByProviderName(string providerName) => /* lookup by ProviderName property */;
        // ...
    }
}

// NO EmptyAuthenticationType.g.cs file - ServiceTypes handle Empty differently
```
