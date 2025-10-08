using FractalDataWorks.Messages;
using FractalDataWorks.Messages.Attributes;
using FractalDataWorks.Services.Abstractions;

namespace FractalDataWorks.Services.SecretManagers.Abstractions.Messages;

/// <summary>
/// CurrentMessage indicating that the command was null.
/// </summary>
[Message("CommandNull")]
public sealed class CommandNullMessage : SecretManagerMessage, IServiceMessage
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CommandNullMessage"/> class.
    /// </summary>
    public CommandNullMessage()
        : base(1001, "CommandNull", MessageSeverity.Error,
               "Command cannot be null.", "SM_CMD_NULL") { }
}
