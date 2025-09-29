using FractalDataWorks.Collections;
using FractalDataWorks.DataContainers.Abstractions;

namespace FractalDataWorks.DataContainers;

/// <summary>
/// Abstract base class for data container type definitions.
/// Provides the foundation for data container implementations in the framework.
/// </summary>
public abstract class DataContainerType : IDataContainerType
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DataContainerType"/> class.
    /// </summary>
    /// <param name="id">The unique identifier for this container type.</param>
    /// <param name="name">The name of the container type.</param>
    /// <param name="description">The description of this container type.</param>
    /// <param name="supportedOperations">The operations this container supports.</param>
    protected DataContainerType(int id, string name, string description, string[] supportedOperations)
    {
        Id = id;
        Name = name;
        Description = description;
        SupportedOperations = supportedOperations;
    }

    /// <inheritdoc/>
    public int Id { get; }

    /// <inheritdoc/>
    public string Name { get; }

    /// <summary>
    /// Gets the description of this container type.
    /// </summary>
    public string Description { get; }

    /// <summary>
    /// Gets the operations this container supports.
    /// </summary>
    public string[] SupportedOperations { get; }
}