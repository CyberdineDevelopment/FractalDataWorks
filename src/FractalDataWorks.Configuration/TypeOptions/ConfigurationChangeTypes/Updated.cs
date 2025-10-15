using FractalDataWorks.Configuration.Abstractions;
using FractalDataWorks.Collections.Attributes;

namespace FractalDataWorks.Configuration;

/// <summary>
/// Represents a configuration that was updated.
/// </summary>
[TypeOption(typeof(ConfigurationChangeTypes), "Updated")]
public sealed class Updated : ConfigurationChangeTypeBase
{
    /// <summary>
    /// Initializes a new instance of the class.
    /// </summary>
    public Updated() : base(2, "Updated", "A configuration was updated")
    {
    }
}