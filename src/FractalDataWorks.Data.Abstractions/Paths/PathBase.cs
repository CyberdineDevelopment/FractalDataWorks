using FractalDataWorks.Collections;

namespace FractalDataWorks.Data.Abstractions;

/// <summary>
/// Base class for path implementations.
/// </summary>
public abstract class PathBase : IPath, ITypeOption<PathBase>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PathBase"/> class.
    /// </summary>
    /// <param name="id">The unique identifier for this path type.</param>
    /// <param name="name">The name of this path type.</param>
    protected PathBase(int id, string name)
    {
        Id = id;
        Name = name;
    }

    /// <summary>
    /// Gets the unique identifier for this path type.
    /// </summary>
    public int Id { get; }

    /// <summary>
    /// Gets the name of this path type.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the category for this path type (default: "Path").
    /// </summary>
    public virtual string Category => "Path";

    /// <summary>
    /// Gets the string representation of the path.
    /// </summary>
    public abstract string PathValue { get; }

    /// <summary>
    /// Gets the domain this path belongs to.
    /// </summary>
    public abstract string Domain { get; }
}
