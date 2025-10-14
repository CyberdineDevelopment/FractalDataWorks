using FractalDataWorks.Messages;
using FractalDataWorks.Messages.Attributes;

namespace FractalDataWorks.Commands.Data.Abstractions;

/// <summary>
/// Message collection for data command messages.
/// </summary>
[MessageCollection("DataCommandMessages")]
public abstract class DataCommandMessage : MessageTemplate<MessageSeverity>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DataCommandMessage"/> class.
    /// </summary>
    protected DataCommandMessage(
        int id,
        string name,
        MessageSeverity severity,
        string message,
        string? code = null)
        : base(id, name, severity, message, code, "DataCommands", null, null)
    {
    }
}

/// <summary>
/// Static class containing data command message instances.
/// </summary>
public static class DataCommandMessages
{
    /// <summary>
    /// Command is required.
    /// </summary>
    public static readonly DataCommandMessage CommandRequired = new CommandRequiredMessage();

    /// <summary>
    /// Container name is required.
    /// </summary>
    public static readonly DataCommandMessage ContainerNameRequired = new ContainerNameRequiredMessage();

    /// <summary>
    /// Command translation failed.
    /// </summary>
    public static readonly DataCommandMessage TranslationFailed = new TranslationFailedMessage();

    /// <summary>
    /// Translator not found.
    /// </summary>
    public static readonly DataCommandMessage TranslatorNotFound = new TranslatorNotFoundMessage();

    private sealed class CommandRequiredMessage : DataCommandMessage
    {
        public CommandRequiredMessage()
            : base(
                id: 1,
                name: "CommandRequired",
                severity: MessageSeverity.Error,
                message: "Command is required",
                code: "DATACMD_001")
        {
        }
    }

    private sealed class ContainerNameRequiredMessage : DataCommandMessage
    {
        public ContainerNameRequiredMessage()
            : base(
                id: 2,
                name: "ContainerNameRequired",
                severity: MessageSeverity.Error,
                message: "Container name is required for data command",
                code: "DATACMD_002")
        {
        }
    }

    private sealed class TranslationFailedMessage : DataCommandMessage
    {
        public TranslationFailedMessage()
            : base(
                id: 100,
                name: "TranslationFailed",
                severity: MessageSeverity.Error,
                message: "Failed to translate {0} command: {1}",
                code: "DATACMD_100")
        {
        }
    }

    private sealed class TranslatorNotFoundMessage : DataCommandMessage
    {
        public TranslatorNotFoundMessage()
            : base(
                id: 101,
                name: "TranslatorNotFound",
                severity: MessageSeverity.Error,
                message: "Translator '{0}' not found",
                code: "DATACMD_101")
        {
        }
    }
}
