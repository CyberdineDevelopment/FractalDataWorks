namespace FractalDataWorks.Configuration.Abstractions;

/// <summary>
/// Base interface for all configuration types in the FractalDataWorks framework.
/// Provides identity and section metadata for configuration objects.
/// </summary>
public interface IGenericConfiguration
{
    /// <summary>
    /// Gets or sets the unique identifier for this configuration instance.
    /// </summary>
    int Id { get; set; }

    /// <summary>
    /// Gets or sets the name of this configuration for lookup and display.
    /// </summary>
    string Name { get; set; }

    /// <summary>
    /// Gets the section name for this configuration (e.g., "Email", "Database").
    /// This is used when binding to IConfiguration sections.
    /// </summary>
    string SectionName { get; }
}

/// <summary>
/// Generic configuration interface with type-safe service type identification.
/// </summary>
/// <typeparam name="T">The concrete configuration type.</typeparam>
public interface IGenericConfiguration<T> : IGenericConfiguration
    where T : IGenericConfiguration<T>
{
    /// <summary>
    /// Gets the service type identifier for this configuration.
    /// Automatically set to the type name by ConfigurationBase{T}.
    /// </summary>
    string ServiceType { get; }
}
