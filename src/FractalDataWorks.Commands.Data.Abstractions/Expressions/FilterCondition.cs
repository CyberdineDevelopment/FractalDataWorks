namespace FractalDataWorks.Commands.Data.Abstractions;

/// <summary>
/// Represents a single filter condition (property, operator, value).
/// </summary>
/// <remarks>
/// <para>
/// Filter conditions use FilterOperatorBase instead of enums, eliminating switch statements.
/// Each operator knows its own SQL and OData representations.
/// </para>
/// <para>
/// Example:
/// <code>
/// var condition = new FilterCondition
/// {
///     PropertyName = "Name",
///     Operator = FilterOperators.Contains,  // No enum, no switch!
///     Value = "Acme"
/// };
///
/// // Direct property access - no switch statements
/// var sqlCondition = $"[{condition.PropertyName}] {condition.Operator.SqlOperator} {condition.Operator.FormatSqlParameter(condition.PropertyName)}";
/// </code>
/// </para>
/// </remarks>
public sealed record FilterCondition
{
    /// <summary>
    /// Gets the property name to filter on.
    /// </summary>
    public required string PropertyName { get; init; }

    /// <summary>
    /// Gets the filter operator.
    /// This is a FilterOperatorBase (TypeCollection), not an enum!
    /// </summary>
    public required FilterOperatorBase Operator { get; init; }

    /// <summary>
    /// Gets the value to compare against (null for IS NULL / IS NOT NULL operators).
    /// </summary>
    public object? Value { get; init; }
}
