namespace FractalDataWorks.Services.DataGateway.Abstractions.Models;

/// <summary>
/// Represents the type of data container.
/// </summary>
public enum ContainerType
{
    /// <summary>
    /// A database table.
    /// </summary>
    Table,

    /// <summary>
    /// A database view.
    /// </summary>
    View,

    /// <summary>
    /// A collection (e.g., in NoSQL databases).
    /// </summary>
    Collection,

    /// <summary>
    /// A file-based container.
    /// </summary>
    File
}
