using FractalDataWorks.Messages;

namespace FractalDataWorks.Data.Sql.Messages;

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