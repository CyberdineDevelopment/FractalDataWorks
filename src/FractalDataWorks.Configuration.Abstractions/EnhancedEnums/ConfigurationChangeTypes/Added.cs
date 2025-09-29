using FractalDataWorks.EnhancedEnums.Attributes;

namespace FractalDataWorks.Configuration.Abstractions;

/// <summary>
/// Represents a configuration that was added.
/// </summary>
[EnumOption(typeof(ConfigurationChangeTypes), "Added")]
public sealed class Added : ConfigurationChangeTypeBase
{
    /// <summary>
    /// Initializes a new instance of the class.
    /// </summary>
    public Added() : base(1, "Added", "A configuration was added")
    {
    }
}