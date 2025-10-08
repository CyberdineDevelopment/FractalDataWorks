using FractalDataWorks.Messages;
using FractalDataWorks.Services.Abstractions.Messages;

namespace FractalDataWorks.Services.DomainName.Abstractions.Messages;

/// <summary>
/// Base message class for DomainName service messages.
/// </summary>
public abstract class DomainNameMessage : MessageTemplate<IServiceMessage>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DomainNameMessage"/> class.
    /// </summary>
    protected DomainNameMessage(
        int id,
        string name,
        MessageSeverity severity,
        string message,
        string code,
        string source,
        string details = "")
        : base(id, name, severity, message, code, source, details)
    {
    }
}
