using FractalDataWorks.EnhancedEnums;

namespace FractalDataWorks.Commands.Data.Abstractions;

/// <summary>
/// Logical operator for combining filter conditions (AND / OR).
/// Uses EnhancedEnum pattern - simple enough for only 2 values.
/// </summary>
/// <remarks>
/// <para>
/// This replaces a traditional enum but adds properties for SQL and OData representations,
/// eliminating the need for switch statements.
/// </para>
/// <para>
/// Usage:
/// <code>
/// var logicalOp = LogicalOperator.And;
/// var sqlJoin = logicalOp.SqlOperator;     // "AND"
/// var odataJoin = logicalOp.ODataOperator; // "and"
/// </code>
/// </para>
/// </remarks>
public sealed class LogicalOperator : EnumOptionBase<LogicalOperator>
{
    /// <summary>
    /// AND logical operator.
    /// </summary>
    public static readonly LogicalOperator And = new(1, "And", "AND", "and");

    /// <summary>
    /// OR logical operator.
    /// </summary>
    public static readonly LogicalOperator Or = new(2, "Or", "OR", "or");

    private LogicalOperator(int id, string name, string sqlOperator, string odataOperator)
        : base(id, name)
    {
        SqlOperator = sqlOperator;
        ODataOperator = odataOperator;
    }

    /// <summary>
    /// Gets the SQL representation (AND / OR).
    /// No switch statements needed - direct property access!
    /// </summary>
    public string SqlOperator { get; }

    /// <summary>
    /// Gets the OData representation (and / or).
    /// No switch statements needed - direct property access!
    /// </summary>
    public string ODataOperator { get; }
}
