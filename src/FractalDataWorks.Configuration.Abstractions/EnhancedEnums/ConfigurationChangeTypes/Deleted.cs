using FractalDataWorks.EnhancedEnums.Attributes;

namespace FractalDataWorks.Configuration.Abstractions;

/// <summary>
/// Represents a configuration that was deleted.
/// </summary>
[EnumOption("Deleted")]
public sealed class Deleted : ConfigurationChangeTypeBase
{
    /// <summary>
    /// Initializes a new instance of the class.
    /// </summary>
    public Deleted() : base(3, "Deleted", "A configuration was deleted")
    {
    }
}