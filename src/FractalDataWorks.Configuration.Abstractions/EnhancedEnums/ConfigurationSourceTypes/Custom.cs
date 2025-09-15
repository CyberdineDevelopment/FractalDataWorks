using FractalDataWorks.EnhancedEnums.Attributes;

namespace FractalDataWorks.Configuration.Abstractions;

/// <summary>
/// Represents a custom configuration source.
/// </summary>
[EnumOption("Custom")]
public sealed class Custom : ConfigurationSourceTypeBase
{
    /// <summary>
    /// Initializes a new instance of the class.
    /// </summary>
    public Custom() : base(7, "Custom", "Custom configuration source")
    {
    }
}