# TypeCollectionGenerator Complete Flow Analysis

## Phase 1: Discovery - What is Looked For
**Step 1.1 - Find Collection Classes:**
- 🔍 **Looks for:** Classes with `[TypeCollection]` or `[GlobalTypeCollection]` attributes
- 🏃 **Process:** Scans current compilation's global namespace recursively (including nested types and namespaces)
- 🗂️ **Stored in:** `HashSet<INamedTypeSymbol> collectionClasses`

## Phase 2: Per Collection Processing  
**Step 2.1 - Determine Scope:**
- 🔍 **Looks for:** `[GlobalTypeCollection]` attribute on collection class
- 🏃 **Process:** Calls `HasGlobalTypeCollectionAttribute()` 
- 🗂️ **Stored in:** `bool isGlobal`

**Step 2.2 - Extract Base Type:**
- 🔍 **Looks for:** Base type from collection class via `ExtractBaseTypeFromCollection()`
  - From generic constraint: `CollectionBase<T> where T : BaseType`
  - From inheritance: `MyCollection : EnumCollectionBase<BaseType>`
- 🏃 **Process:** Scans generic parameters and base class chain
- 🗂️ **Stored in:** `INamedTypeSymbol baseType`

## Phase 3: Option Type Discovery
**Step 3.1 - Scan for Implementation Types:**
- 🔍 **Looks for:** Non-abstract classes that derive from `baseType`
- 🏃 **Process:** 
  - If `isGlobal = true`: Scans ALL referenced assemblies
  - If `isGlobal = false`: Scans only current compilation
- 🗂️ **Stored in:** `HashSet<INamedTypeSymbol> optionTypes`

## Phase 4: Property Analysis (THIS IS WHAT YOU WANT TO CHANGE)
**Step 4.1 - Extract Lookup Properties:**
- 🔍 **Looks for:** Properties with `[TypeLookup]` attributes **ON THE BASE TYPE INHERITANCE CHAIN**
- 🏃 **Process:** 
  - Traverses inheritance: `baseType → baseType.BaseType → baseType.BaseType.BaseType...`
  - For each property: Extracts method name, return type, allowMultiple flag
- 🗂️ **Stored in:** `EquatableArray<PropertyLookupInfoModel> lookupProperties`

## Phase 5: Model Building
**Step 5.1 - Build EnumTypeInfoModel:**
- 🔍 **Uses:** Collection class attributes, base type info, lookup properties
- 🏃 **Process:** `BuildEnumDefinitionFromCollection()` creates model
- 🗂️ **Stored in:** `EnumTypeInfoModel typeInfo`

## Phase 6: Code Generation
**Step 6.1 - Convert Types to Values:**
- 🔍 **Looks for:** `[TypeOption]` attribute name on each option type
- 🏃 **Process:** Extracts name from attribute or falls back to class name
- 🗂️ **Stored in:** `List<EnumValueInfoModel> values`

**Step 6.2 - Generate Collection:**
- 🔍 **Uses:** All collected data
- 🏃 **Process:** `EnumCollectionBuilder` creates final source code
- 📄 **Creates:** `{CollectionName}.g.cs` file

---

## Summary of Key Points:

1. **Collection Classes Found:** Classes with `[TypeCollection]`/`[GlobalTypeCollection]` attributes
2. **Base Type Extracted:** From generic constraints or inheritance
3. **Option Types Found:** Non-abstract classes deriving from base type  
4. **🎯 PROPERTY ANALYSIS:** Currently traverses **base type inheritance chain** looking for `[TypeLookup]` attributes
5. **Generated Code:** Static collection with lookup methods based on discovered properties

**The issue you want to fix:** Step 4.1 currently reads properties from the inheritance chain, but you want to change this behavior while keeping the inheritance checking for determining which types qualify as options.

## Methods Involved:

### Phase 1 Methods:
- `DiscoverAllCollectionDefinitions(Compilation compilation)`
- `ScanForCollectionClasses(INamespaceSymbol namespaceSymbol, HashSet<INamedTypeSymbol> collectionClasses)`
- `HasCollectionAttribute(INamedTypeSymbol type)`

### Phase 2 Methods:
- `HasGlobalTypeCollectionAttribute(INamedTypeSymbol type)`
- `ExtractBaseTypeFromCollection(INamedTypeSymbol collectionClass)`

### Phase 3 Methods:
- `ScanForOptionTypesOfBase(INamespaceSymbol namespaceSymbol, INamedTypeSymbol baseType, HashSet<INamedTypeSymbol> optionTypes)`
- `DerivesFromBaseType(INamedTypeSymbol derivedType, INamedTypeSymbol baseType)`

### Phase 4 Methods (TARGET FOR MODIFICATION):
- `ExtractLookupPropertiesFromBaseType(INamedTypeSymbol baseType)` 
  - **Current behavior:** Traverses `baseType` inheritance chain looking for `[TypeLookup]` attributes
  - **Needed change:** Stop reading from inheritance chain for properties

### Phase 5 Methods:
- `BuildEnumDefinitionFromCollection(INamedTypeSymbol collectionClass, INamedTypeSymbol baseType, List<INamedTypeSymbol> optionTypes, Compilation compilation)`

### Phase 6 Methods:
- `Execute(SourceProductionContext context, EnumTypeInfoModel def, Compilation compilation, List<INamedTypeSymbol> discoveredOptionTypes)`
- `ExtractTypeOptionName(AttributeData typeOptionAttr, INamedTypeSymbol optionType)`
- `GenerateCollection(SourceProductionContext context, EnumTypeInfoModel def, EquatableArray<EnumValueInfoModel> values, Compilation compilation)`