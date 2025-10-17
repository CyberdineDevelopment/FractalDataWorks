using FractalDataWorks.EnhancedEnums;

namespace FractalDataWorks.Commands.Data.Abstractions;

/// <summary>
/// Sort direction for ordering expressions (ASC / DESC).
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
/// var direction = SortDirection.Ascending;
/// var sqlSort = $"ORDER BY [Name] {direction.SqlKeyword}";       // "ORDER BY [Name] ASC"
/// var odataSort = $"$orderby=Name {direction.ODataKeyword}";     // "$orderby=Name asc"
/// </code>
/// </para>
/// </remarks>
public sealed class SortDirection : EnumOptionBase<SortDirection>
{
    /// <summary>
    /// Ascending sort direction.
    /// </summary>
    public static readonly SortDirection Ascending = new(1, "Ascending", "ASC", "asc");

    /// <summary>
    /// Descending sort direction.
    /// </summary>
    public static readonly SortDirection Descending = new(2, "Descending", "DESC", "desc");

    private SortDirection(int id, string name, string sqlKeyword, string odataKeyword)
        : base(id, name)
    {
        SqlKeyword = sqlKeyword;
        ODataKeyword = odataKeyword;
    }

    /// <summary>
    /// Gets the SQL keyword (ASC / DESC).
    /// No switch statements needed - direct property access!
    /// </summary>
    public string SqlKeyword { get; }

    /// <summary>
    /// Gets the OData keyword (asc / desc).
    /// No switch statements needed - direct property access!
    /// </summary>
    public string ODataKeyword { get; }
}
