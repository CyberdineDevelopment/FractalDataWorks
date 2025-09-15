using FractalDataWorks.EnhancedEnums.Attributes;

namespace FractalDataWorks.Configuration.Abstractions;

/// <summary>
/// Represents a configuration that was updated.
/// </summary>
[EnumOption("Updated")]
public sealed class Updated : ConfigurationChangeTypeBase
{
    /// <summary>
    /// Initializes a new instance of the class.
    /// </summary>
    public Updated() : base(2, "Updated", "A configuration was updated")
    {
    }
}