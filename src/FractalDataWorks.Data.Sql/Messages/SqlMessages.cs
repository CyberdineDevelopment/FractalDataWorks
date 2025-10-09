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

/// <summary>
/// Collection of SQL messages.
/// </summary>
public static partial class SqlMessages
{
    /// <summary>
    /// Gets the translator not registered message.
    /// </summary>
    public static class TranslatorNotRegistered
    {
        /// <summary>
        /// Gets the singleton instance.
        /// </summary>
        public static TranslatorNotRegisteredMessage Instance { get; } = new();
    }

    /// <summary>
    /// Gets the SQL syntax error message.
    /// </summary>
    public static SqlSyntaxErrorMessage SyntaxError(string error) => new(error);

    /// <summary>
    /// Gets the unsupported expression message.
    /// </summary>
    public static UnsupportedExpressionMessage UnsupportedExpression(string expression) => new(expression);
}

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

/// <summary>
/// Message for SQL syntax errors.
/// </summary>
public sealed class SqlSyntaxErrorMessage : SqlMessage
{
    /// <summary>
    /// Gets the syntax error details.
    /// </summary>
    public string Error { get; }

    internal SqlSyntaxErrorMessage(string error)
        : base(2002, "SqlSyntaxError", MessageSeverity.Error,
               $"SQL syntax error: {error}", "SQL_SYNTAX")
    {
        Error = error;
    }
}

/// <summary>
/// Message for unsupported LINQ expressions.
/// </summary>
public sealed class UnsupportedExpressionMessage : SqlMessage
{
    /// <summary>
    /// Gets the unsupported expression description.
    /// </summary>
    public string Expression { get; }

    internal UnsupportedExpressionMessage(string expression)
        : base(2003, "UnsupportedExpression", MessageSeverity.Error,
               $"Expression not supported in SQL: {expression}", "SQL_EXPR")
    {
        Expression = expression;
    }
}