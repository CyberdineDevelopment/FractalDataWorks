using System.Collections.Generic;
using FluentValidation.Results;
using FractalDataWorks.Commands.Abstractions;
using FractalDataWorks.Results;

namespace FractalDataWorks.Commands.Data.Abstractions;

/// <summary>
/// Abstract base class for all data commands (non-generic).
/// This base class is used by TypeCollection source generators.
/// </summary>
/// <remarks>
/// <para>
/// Provides common implementation for IDataCommand interface.
/// Use the generic variants (<see cref="DataCommandBase{TResult}"/> or <see cref="DataCommandBase{TResult, TInput}"/>)
/// for actual command implementations.
/// </para>
/// <para>
/// Properties must be set in constructor for TypeCollection source generators to read them.
/// </para>
/// </remarks>
public abstract class DataCommandBase : IDataCommand
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DataCommandBase"/> class.
    /// </summary>
    /// <param name="id">Unique identifier for this command type.</param>
    /// <param name="name">Name of the command type (must match TypeOption attribute).</param>
    /// <param name="containerName">Name of the data container this command operates on.</param>
    /// <param name="category">The command category.</param>
    protected DataCommandBase(int id, string name, string containerName, IGenericCommandCategory category)
    {
        Id = id;
        Name = name;
        ContainerName = containerName;
        Category = category;
        Metadata = new Dictionary<string, object>(System.StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Gets the unique identifier for this command type.
    /// </summary>
    public int Id { get; }

    /// <summary>
    /// Gets the name of this command type.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the command type name (implements IGenericCommand).
    /// </summary>
    public string CommandType => Name;

    /// <summary>
    /// Gets the container name this command operates on.
    /// </summary>
    public string ContainerName { get; }

    /// <summary>
    /// Gets the metadata for this command.
    /// </summary>
    public IReadOnlyDictionary<string, object> Metadata { get; init; }

    /// <summary>
    /// Gets the command category (implements IGenericCommand).
    /// </summary>
    public IGenericCommandCategory Category { get; }

    /// <summary>
    /// Validates this command.
    /// </summary>
    /// <returns>A result containing the validation result.</returns>
    public virtual IGenericResult<ValidationResult> Validate()
    {
        var validationResult = new ValidationResult();

        if (string.IsNullOrWhiteSpace(ContainerName))
        {
            validationResult.Errors.Add(new ValidationFailure(nameof(ContainerName), "Container name is required"));
        }

        return GenericResult<ValidationResult>.Success(validationResult);
    }
}
