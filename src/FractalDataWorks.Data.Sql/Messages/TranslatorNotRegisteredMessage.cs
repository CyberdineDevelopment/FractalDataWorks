using FractalDataWorks.Messages;

namespace FractalDataWorks.Data.Sql.Messages;

/// <summary>
/// Message for when SQL translator is not registered.
/// </summary>
public sealed class TranslatorNotRegisteredMessage : SqlMessage
{
    internal TranslatorNotRegisteredMessage()
        : base(2001, "TranslatorNotRegistered", MessageSeverity.Error,
               "SQL translator service is not registered in dependency injection", "SQL_NOT_REG")
    {
    }
}