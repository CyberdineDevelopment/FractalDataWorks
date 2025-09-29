using System;

namespace FractalDataWorks.EnhancedEnums.Attributes;

/// <summary>
/// Marks a concrete Enhanced Enum option with explicit collection targeting.
/// Follows the same pattern as TypeOptionAttribute and ServiceTypeOptionAttribute.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public sealed class EnumOptionAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EnumOptionAttribute"/> class.
    /// </summary>
    /// <param name="collectionType">The type of the collection this option belongs to.</param>
    /// <param name="name">The name for the method/property in the generated collection.</param>
    public EnumOptionAttribute(Type collectionType, string name)
    {
        CollectionType = collectionType ?? throw new ArgumentNullException(nameof(collectionType));
        Name = name ?? throw new ArgumentNullException(nameof(name));
    }

    /// <summary>
    /// Gets the type of the collection this option belongs to.
    /// </summary>
    public Type CollectionType { get; }

    /// <summary>
    /// Gets the name for the method/property in the generated collection.
    /// </summary>
    public string Name { get; }
}