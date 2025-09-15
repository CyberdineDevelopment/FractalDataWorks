namespace FractalDataWorks.DataSets.Abstractions;

/// <summary>
/// Configuration class for dataset field definitions.
/// </summary>
public sealed class DataFieldConfiguration
{
    /// <summary>
    /// Gets or sets the name of the field.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the description of the field.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the .NET type name of the field.
    /// </summary>
    public string TypeName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether this field is part of the primary key.
    /// </summary>
    public bool IsKey { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this field is required (non-nullable).
    /// </summary>
    public bool IsRequired { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this field can be used for indexing/searching.
    /// </summary>
    public bool IsIndexed { get; set; }

    /// <summary>
    /// Gets or sets the maximum length for string fields, or null if not applicable.
    /// </summary>
    public int? MaxLength { get; set; }

    /// <summary>
    /// Gets or sets the default value for this field as a string representation.
    /// </summary>
    public string? DefaultValue { get; set; }

    /// <summary>
    /// Creates a clone of this field configuration.
    /// </summary>
    /// <returns>A cloned instance of the field configuration.</returns>
    public DataFieldConfiguration Clone()
    {
        return new DataFieldConfiguration
        {
            Name = Name,
            Description = Description,
            TypeName = TypeName,
            IsKey = IsKey,
            IsRequired = IsRequired,
            IsIndexed = IsIndexed,
            MaxLength = MaxLength,
            DefaultValue = DefaultValue
        };
    }
}