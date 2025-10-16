using System;
using FractalDataWorks.SourceGenerators.Models;

namespace FractalDataWorks.Messages.SourceGenerators.Models;

/// <summary>
/// Type alias for PropertyLookupInfoModel used in Messages context.
/// Since PropertyLookupInfoModel is sealed, we use composition instead of inheritance.
/// </summary>
public class MessagePropertyLookupInfoModel : IEquatable<MessagePropertyLookupInfoModel>
{
    private readonly PropertyLookupInfoModel _inner;

    /// <summary>
    /// Initializes a new instance of the <see cref="MessagePropertyLookupInfoModel"/> class.
    /// </summary>
    public MessagePropertyLookupInfoModel()
    {
        _inner = new PropertyLookupInfoModel();
    }

    /// <summary>
    /// Gets or sets the name of the property to create lookup methods for.
    /// </summary>
    public string PropertyName 
    { 
        get => _inner.PropertyName; 
        set => _inner.PropertyName = value; 
    }

    /// <summary>
    /// Gets or sets the type of the property as a string.
    /// </summary>
    public string PropertyType 
    { 
        get => _inner.PropertyType; 
        set => _inner.PropertyType = value; 
    }

    /// <summary>
    /// Gets or sets the name of the generated lookup method.
    /// </summary>
    public string LookupMethodName 
    { 
        get => _inner.LookupMethodName; 
        set => _inner.LookupMethodName = value; 
    }

    /// <summary>
    /// Gets or sets a value indicating whether to generate a TryGet method in addition to the direct lookup method.
    /// </summary>
    public bool GenerateTryGet 
    { 
        get => _inner.GenerateTryGet; 
        set => _inner.GenerateTryGet = value; 
    }

    /// <summary>
    /// Gets or sets a value indicating whether to allow multiple matches for this property.
    /// </summary>
    public bool AllowMultiple 
    { 
        get => _inner.AllowMultiple; 
        set => _inner.AllowMultiple = value; 
    }

    /// <summary>
    /// Gets or sets a value indicating whether this is the default lookup property.
    /// </summary>
    public bool IsDefaultProperty 
    { 
        get => _inner.IsDefaultProperty; 
        set => _inner.IsDefaultProperty = value; 
    }

    /// <summary>
    /// Gets or sets the string comparison mode to use for string property lookups.
    /// </summary>
    public StringComparison StringComparison 
    { 
        get => _inner.StringComparison; 
        set => _inner.StringComparison = value; 
    }

    /// <summary>
    /// Gets or sets the name of a custom comparer to use for property comparisons.
    /// </summary>
    public string? Comparer 
    { 
        get => _inner.Comparer; 
        set => _inner.Comparer = value; 
    }

    /// <summary>
    /// Gets or sets a value indicating whether the property is nullable.
    /// </summary>
    public bool IsNullable 
    { 
        get => _inner.IsNullable; 
        set => _inner.IsNullable = value; 
    }

    /// <summary>
    /// Gets or sets the return type for this specific lookup method.
    /// If null, inherits from the EnumTypeInfoModel.ReturnType.
    /// </summary>
    public string? ReturnType 
    { 
        get => _inner.ReturnType; 
        set => _inner.ReturnType = value; 
    }

    /// <summary>
    /// Gets or sets a value indicating whether the property is abstract or virtual and needs to be overridden in the Empty class.
    /// </summary>
    public bool RequiresOverride 
    { 
        get => _inner.RequiresOverride; 
        set => _inner.RequiresOverride = value; 
    }

    /// <summary>
    /// Gets a hash string representing the contents of this property lookup infoModel for incremental generation.
    /// </summary>
    public string InputHash => _inner.InputHash;

    /// <summary>
    /// Gets a string representation of the default value for this property type.
    /// </summary>
    /// <returns>A string that represents the default value for the property type.</returns>
    public string GetDefaultValueString() => _inner.GetDefaultValueString();

    /// <summary>
    /// Determines whether the specified <see cref="MessagePropertyLookupInfoModel"/> is equal to the current instance.
    /// </summary>
    /// <param name="other">The MessagePropertyLookupInfoModel to compare with the current instance.</param>
    /// <returns>true if the specified MessagePropertyLookupInfoModel is equal to the current instance; otherwise, false.</returns>
    public bool Equals(MessagePropertyLookupInfoModel? other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return _inner.Equals(other._inner);
    }

    /// <summary>
    /// Determines whether the specified object is equal to the current instance.
    /// </summary>
    /// <param name="obj">The object to compare with the current instance.</param>
    /// <returns>true if the specified object is equal to the current instance; otherwise, false.</returns>
    public override bool Equals(object? obj) => Equals(obj as MessagePropertyLookupInfoModel);

    /// <summary>
    /// Returns a hash code for this instance.
    /// </summary>
    /// <returns>A hash code for the current instance.</returns>
    public override int GetHashCode() => _inner.GetHashCode();

    /// <summary>
    /// Returns a string representation of this instance.
    /// </summary>
    /// <returns>A string representation of this instance.</returns>
    public override string ToString() => $"MessagePropertyLookup {{ PropertyName = {PropertyName}, PropertyType = {PropertyType}, LookupMethodName = {LookupMethodName} }}";
}