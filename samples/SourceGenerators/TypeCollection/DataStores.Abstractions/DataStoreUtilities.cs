using FractalDataWorks.Collections.Attributes;

namespace DataStore.Abstractions;

/// <summary>
/// Static utility class for data store operations.
/// Demonstrates static type inclusion in collections.
/// </summary>
[TypeOption("Utilities")]
public static class DataStoreUtilities
{
    /// <summary>
    /// Validates a connection string format.
    /// </summary>
    /// <param name="connectionString">The connection string to validate.</param>
    /// <returns>True if valid, false otherwise.</returns>
    public static bool ValidateConnectionString(string connectionString)
    {
        return !string.IsNullOrWhiteSpace(connectionString) && connectionString.Contains("=");
    }

    /// <summary>
    /// Generates a default connection string template.
    /// </summary>
    /// <param name="server">The server name.</param>
    /// <param name="database">The database name.</param>
    /// <returns>A connection string template.</returns>
    public static string GenerateConnectionTemplate(string server, string database)
    {
        return $"Server={server};Database={database};";
    }

    /// <summary>
    /// Gets the recommended timeout for database operations.
    /// </summary>
    public static int RecommendedTimeoutSeconds => 30;
}