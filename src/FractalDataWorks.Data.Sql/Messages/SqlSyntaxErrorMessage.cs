using FractalDataWorks.Messages;

namespace FractalDataWorks.Data.Sql.Messages;

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