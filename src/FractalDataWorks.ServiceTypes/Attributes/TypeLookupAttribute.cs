using System;

namespace FractalDataWorks.ServiceTypes.Attributes;

/// <summary>
/// Marks a property for which to generate lookup methods in ServiceTypes collections.
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public sealed class TypeLookupAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TypeLookupAttribute"/> class.
    /// </summary>
    /// <param name="methodName">Custom method name for the lookup (e.g. GetByServiceType).</param>
    /// <param name="allowMultiple">Allow multiple results per lookup key.</param>
    /// <param name="returnType">The return type for this specific lookup method.</param>
    public TypeLookupAttribute(
        string methodName = "",
        bool allowMultiple = false,
        Type? returnType = null)
    {
        MethodName = methodName;
        AllowMultiple = allowMultiple;
        ReturnType = returnType;
    }

    /// <summary>
    /// Gets the custom method name for the lookup (e.g. GetByServiceType).
    /// </summary>
    public string MethodName { get; }

    /// <summary>
    /// Gets whether to allow multiple results per lookup key.
    /// </summary>
    public bool AllowMultiple { get; }

    /// <summary>
    /// Gets the return type for this specific lookup method.
    /// If not specified, defaults to ServiceTypeBase.
    /// </summary>
    public Type? ReturnType { get; }
}