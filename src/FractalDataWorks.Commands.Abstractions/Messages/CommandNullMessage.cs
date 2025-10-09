using FractalDataWorks.Messages;

namespace FractalDataWorks.Commands.Abstractions.Messages;

/// <summary>
/// Message for when a command is null.
/// </summary>
public sealed class CommandNullMessage : CommandMessage
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CommandNullMessage"/> class.
    /// </summary>
    public CommandNullMessage()
        : base(1001, "CommandNull", MessageSeverity.Error,
               "Command cannot be null", "CMD_NULL")
    {
    }
}