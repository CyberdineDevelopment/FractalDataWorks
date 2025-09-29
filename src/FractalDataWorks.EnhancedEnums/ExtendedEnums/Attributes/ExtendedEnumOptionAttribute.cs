using System;

namespace FractalDataWorks.EnhancedEnums.ExtendedEnums.Attributes;

/// <summary>
/// Marks a custom Extended Enum implementation that overrides the auto-generated wrapper for a specific enum value.
/// When this attribute is present, the source generator will use this custom implementation
/// instead of generating a default wrapper for the specified enum value.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public sealed class ExtendedEnumOptionAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ExtendedEnumOptionAttribute"/> class.
    /// </summary>
    /// <param name="collectionType">The type of the collection this option belongs to.</param>
    /// <param name="enumValue">The enum value this custom implementation represents.</param>
    public ExtendedEnumOptionAttribute(Type collectionType, object enumValue)
    {
        CollectionType = collectionType ?? throw new ArgumentNullException(nameof(collectionType));
        EnumValue = enumValue ?? throw new ArgumentNullException(nameof(enumValue));

        // The enum value will be validated by the source generator to ensure it matches the collection's enum type
    }

    /// <summary>
    /// Gets the type of the collection this option belongs to.
    /// </summary>
    public Type CollectionType { get; }

    /// <summary>
    /// Gets the enum value this custom implementation represents.
    /// </summary>
    public object EnumValue { get; }

    /// <summary>
    /// Gets the name derived from the enum value.
    /// This is used as the property/method name in the generated collection.
    /// </summary>
    public string Name => EnumValue.ToString() ?? throw new InvalidOperationException("EnumValue.ToString() returned null");
}
