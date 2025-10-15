namespace FractalDataWorks.Collections;

/// <summary>
/// Base class for type collections that provides inheritance structure for the TypeCollectionGenerator.
/// The TypeCollectionGenerator will populate all methods and properties in the generated partial class.
/// </summary>
/// <typeparam name="TBase">The type option that must derive from the base type</typeparam>
public abstract class TypeCollectionBase<TBase> where TBase : class
{
    // TypeCollectionGenerator will generate all functionality in the partial class
}

/// <summary>
/// Base class for type collections with different return type.
/// Use this when you want the collection to work with TBase types but return TGeneric instances.
/// The TypeCollectionGenerator will populate all methods and properties in the generated partial class.
/// </summary>
/// <typeparam name="TBase">The concrete base type that collection items derive from</typeparam>
/// <typeparam name="TGeneric">The return type for all collection methods (must be base of TBase)</typeparam>
public abstract class TypeCollectionBase<TBase, TGeneric> where TBase : class, TGeneric where TGeneric : class
{
    // TypeCollectionGenerator will generate all functionality in the partial class
}