using FractalDataWorks.EnhancedEnums.Attributes;

namespace FractalDataWorks.Configuration.Abstractions;

/// <summary>
/// Represents a configuration source that was reloaded.
/// </summary>
[EnumOption("Reloaded")]
public sealed class Reloaded : ConfigurationChangeTypeBase
{
    /// <summary>
    /// Initializes a new instance of the class.
    /// </summary>
    public Reloaded() : base(4, "Reloaded", "The configuration source was reloaded")
    {
    }
}