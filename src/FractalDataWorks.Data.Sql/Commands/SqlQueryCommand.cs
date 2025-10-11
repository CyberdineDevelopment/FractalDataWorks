using System;
using System.Collections.Generic;
using FluentValidation.Results;
using FractalDataWorks.Commands.Abstractions;
using FractalDataWorks.Commands.Abstractions.Commands;
using FractalDataWorks.Results;

namespace FractalDataWorks.Data.Sql.Commands;

/// <summary>
/// Represents a SQL query command.
/// </summary>
public sealed record SqlQueryCommand : IQueryCommand
{
    /// <inheritdoc/>
    public Guid CommandId { get; init; } = Guid.NewGuid();

    /// <inheritdoc/>
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;

    /// <inheritdoc/>
    public string CommandType { get; init; } = "SqlQuery";

    /// <inheritdoc/>
    public ICommandCategory Category => SqlCommandCategory.Query;

    /// <inheritdoc/>
    public string? DataSource { get; init; }

    /// <inheritdoc/>
    public IDataSchema? TargetSchema { get; init; }

    /// <inheritdoc/>
    public bool RequiresTransaction { get; init; }

    /// <inheritdoc/>
    public int TimeoutMs { get; init; } = 30000;

    /// <inheritdoc/>
    public IReadOnlyCollection<string>? SelectFields { get; init; }

    /// <inheritdoc/>
    public object? FilterCriteria { get; init; }

    /// <inheritdoc/>
    public IReadOnlyCollection<IOrderSpecification>? OrderBy { get; init; }

    /// <inheritdoc/>
    public int? Skip { get; init; }

    /// <inheritdoc/>
    public int? Take { get; init; }

    /// <inheritdoc/>
    public IReadOnlyCollection<string>? IncludeRelations { get; init; }

    /// <inheritdoc/>
    public bool IsCacheable { get; init; } = true;

    /// <inheritdoc/>
    public int? CacheDurationSeconds { get; init; }

    /// <summary>
    /// Gets the SQL text for this command.
    /// </summary>
    public string SqlText { get; init; } = string.Empty;

    /// <summary>
    /// Gets the SQL parameters for this command.
    /// </summary>
    public IReadOnlyDictionary<string, object>? Parameters { get; init; }

    /// <summary>
    /// Gets whether this command has been optimized.
    /// </summary>
    public bool IsOptimized { get; init; }

    /// <inheritdoc/>
    public IGenericResult<ValidationResult> Validate()
    {
        var result = new ValidationResult();

        if (string.IsNullOrWhiteSpace(SqlText))
        {
            result.Errors.Add(new ValidationFailure(nameof(SqlText), "SQL text cannot be empty"));
        }

        return result.IsValid
            ? GenericResult<ValidationResult>.Success(result)
            : GenericResult<ValidationResult>.Failure(result);
    }
}