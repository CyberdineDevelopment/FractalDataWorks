using FractalDataWorks.EnhancedEnums.Attributes;

namespace FractalDataWorks.Configuration.Abstractions;

/// <summary>
/// Represents configuration from a database.
/// </summary>
[EnumOption("Database")]
public sealed class Database : ConfigurationSourceTypeBase
{
    /// <summary>
    /// Initializes a new instance of the class.
    /// </summary>
    public Database() : base(3, "Database", "Configuration from a database")
    {
    }
}