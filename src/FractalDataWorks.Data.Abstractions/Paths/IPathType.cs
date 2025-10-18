using FractalDataWorks.Collections;

namespace FractalDataWorks.Data.Abstractions;

/// <summary>
/// Represents a path type definition - metadata about navigation paths to data containers.
/// </summary>
/// <remarks>
/// Path types describe how to navigate to containers within different data store types
/// (e.g., Database.Schema.Table for SQL, /api/endpoint for REST, file paths for FileSystem).
/// </remarks>
public interface IPathType : ITypeOption
{
    /// <summary>
    /// Gets the domain this path type belongs to (Sql, Rest, File, GraphQL).
    /// </summary>
    string Domain { get; }
}
