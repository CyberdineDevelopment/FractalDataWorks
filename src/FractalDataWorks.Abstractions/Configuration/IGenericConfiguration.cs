namespace FractalDataWorks.Configuration;

/// <summary>
/// Base interface for all configuration objects in the FractalDataWorks framework.
/// Provides common properties for all configuration types.
/// </summary>
public interface IGenericConfiguration
{
    /// <summary>
    /// Gets the unique identifier for this configuration instance.
    /// </summary>
    int Id { get; }

    /// <summary>
    /// Gets the name of this configuration for lookup and display.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets the section name for this configuration in appsettings.
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
    /// </summary>
    string ServiceType { get; }
}
