using FractalDataWorks.Configuration.Abstractions;
using FractalDataWorks.Collections.Attributes;

namespace FractalDataWorks.Configuration;

/// <summary>
/// Represents a configuration source that was reloaded.
/// </summary>
[TypeOption(typeof(ConfigurationChangeTypes), "Reloaded")]
public sealed class Reloaded : ConfigurationChangeTypeBase
{
    /// <summary>
    /// Initializes a new instance of the class.
    /// </summary>
    public Reloaded() : base(4, "Reloaded", "The configuration source was reloaded")
    {
    }
}