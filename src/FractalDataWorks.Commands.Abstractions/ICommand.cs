using System;
using FluentValidation.Results;
using FractalDataWorks.Results;

namespace FractalDataWorks.Commands.Abstractions;

/// <summary>
/// Base interface for all command type definitions in the FractalDataWorks framework.
/// Commands represent static type definitions (singletons) that define operation metadata.
/// </summary>
/// <remarks>
/// <para>
/// Commands follow the TypeCollection pattern and are static singletons, not runtime instances.
/// For runtime execution tracking, use <see cref="CommandExecution"/> which wraps a command type
/// with execution-specific metadata (ExecutionId, CreatedAt, Payload).
/// </para>
/// <para>
/// Command types define:
/// <list type="bullet">
/// <item>What category of operation (Query, Mutation, Bulk)</item>
/// <item>Which translators can process this command type</item>
/// <item>Execution characteristics (batching, pipelining, transaction requirements)</item>
/// </list>
/// </para>
/// </remarks>
public interface IGenericCommand
{
    /// <summary>
    /// Gets the command type name for routing and translation.
    /// </summary>
    /// <value>The type name used by TypeCollections for command routing (e.g., "SqlQuery", "RestGet").</value>
    string CommandType { get; }

    /// <summary>
    /// Gets the command category for classification.
    /// </summary>
    /// <value>The category that determines command behavior and requirements.</value>
    IGenericCommandCategory Category { get; }

    /// <summary>
    /// Validates this command type definition.
    /// </summary>
    /// <returns>A GenericResult containing the validation result.</returns>
    IGenericResult<ValidationResult> Validate();
}

/// <summary>
/// Represents a command that can be executed with a payload.
/// </summary>
/// <typeparam name="T">The type of the payload carried by this command.</typeparam>
public interface IGenericCommand<T> : IGenericCommand
{
    /// <summary>
    /// Gets the payload of the command.
    /// </summary>
    T? Payload { get; init; }

    /// <summary>
    /// Validates this command.
    /// </summary>
    /// <returns>A GenericResult containing the validation result.</returns>
    new IGenericResult<ValidationResult> Validate();
}