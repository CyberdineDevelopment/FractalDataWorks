### New Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|-------
ENH004 | Usage | Error | DuplicateEnumOptionAnalyzer - Duplicate enhanced enum option ID detected
ENH005 | Usage | Error | EnumOptionConstructorAnalyzer - Enhanced enum option constructor issues
ENH006 | Design | Warning | AbstractMemberAnalyzer - Abstract property in enhanced enum
ENH007 | Design | Error | AbstractMemberAnalyzer - Abstract field in enhanced enum  
ENH008 | Usage | Error | EnumCollectionAttributeAnalyzer - EnumCollection attribute must specify CollectionName
ENH009 | Usage | Error | EnumCollectionAttributeAnalyzer - EnumCollection classes must inherit from EnumOptionBase<T>
ENH010 | Usage | Error | EnumCollectionAttributeAnalyzer - Generic EnumCollection must specify a non-generic interface constraint for T
ENHENUM001 | Collections | Warning | DuplicateLookupValueAnalyzer - Duplicate lookup values detected without AllowMultiple
TC001 | Usage | Warning | MissingTypeOptionAnalyzer - Type option missing required [TypeOption] attribute
TC002 | Usage | Error | MissingTypeOptionAnalyzer - TGeneric in base class doesn't match defaultReturnType in TypeCollection attribute
TC003 | Usage | Error | MissingTypeOptionAnalyzer - TBase in base class doesn't match baseType in TypeCollection attribute
