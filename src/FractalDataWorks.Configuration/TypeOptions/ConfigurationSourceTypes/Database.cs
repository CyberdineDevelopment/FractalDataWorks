using FractalDataWorks.Configuration.Abstractions;
using FractalDataWorks.Collections.Attributes;

namespace FractalDataWorks.Configuration;

/// <summary>
/// Represents configuration from a database.
/// </summary>
[TypeOption(typeof(ConfigurationSourceTypes), "Database")]
public sealed class Database : ConfigurationSourceTypeBase
{
    /// <summary>
    /// Initializes a new instance of the class.
    /// </summary>
    public Database() : base(3, "Database", "Configuration from a database")
    {
    }
}