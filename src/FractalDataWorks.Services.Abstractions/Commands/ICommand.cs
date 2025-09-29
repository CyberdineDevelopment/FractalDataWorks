using FluentValidation.Results;
using FractalDataWorks.Results;
using System;

namespace FractalDataWorks.Services.Abstractions.Commands;
/// <summary>
/// Represents a command that can be executed with a payload.
/// </summary>
/// <typeparam name="T">The type of the payload carried by this command.</typeparam>
public interface ICommand<T> : ICommand
{
    /// <summary>
    /// Gets the payload of the command;
    /// </summary>
    T? Payload { get; init; }

    /// <summary>
    /// Validates this command.
    /// </summary>
    /// <returns>A FdwResult containing the validation result.</returns>
    new IFdwResult<ValidationResult> Validate();
}