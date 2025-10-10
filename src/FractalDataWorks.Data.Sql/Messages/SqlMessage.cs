using FractalDataWorks.Messages;
using FractalDataWorks.Messages.Attributes;

namespace FractalDataWorks.Data.Sql.Messages;

/// <summary>
/// Base class for SQL-related messages.
/// </summary>
[MessageCollection("SqlMessages")]
public abstract class SqlMessage : MessageTemplate<MessageSeverity>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SqlMessage"/> class.
    /// </summary>
    protected SqlMessage(int id, string name, MessageSeverity severity,
        string message, string? code = null)
        : base(id, name, severity, "SQL", message, code, null, null)
    {
    }
}