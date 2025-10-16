using FractalDataWorks.Configuration.Abstractions;
using FractalDataWorks.Collections.Attributes;

namespace FractalDataWorks.Configuration;

/// <summary>
/// Represents a configuration that was deleted.
/// </summary>
[TypeOption(typeof(ConfigurationChangeTypes), "Deleted")]
public sealed class Deleted : ConfigurationChangeTypeBase
{
    /// <summary>
    /// Initializes a new instance of the class.
    /// </summary>
    public Deleted() : base(3, "Deleted", "A configuration was deleted")
    {
    }
}