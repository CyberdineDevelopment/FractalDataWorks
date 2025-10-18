using FractalDataWorks.Data.Abstractions;

namespace FractalDataWorks.Data.Sql;

/// <summary>
/// Represents a path to a SQL Server database object.
/// Format: Database.Schema.Object (e.g., "Northwind.dbo.Customers")
/// </summary>
public sealed class DatabasePath : PathBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DatabasePath"/> class.
    /// </summary>
    /// <param name="database">The database name.</param>
    /// <param name="schema">The schema name (default: "dbo").</param>
    /// <param name="objectName">The object name (table, view, stored procedure).</param>
    public DatabasePath(string database, string schema, string objectName)
        : base(1, "DatabasePath")
    {
        Database = database;
        Schema = schema;
        ObjectName = objectName;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DatabasePath"/> class with default schema "dbo".
    /// </summary>
    /// <param name="database">The database name.</param>
    /// <param name="objectName">The object name (table, view, stored procedure).</param>
    public DatabasePath(string database, string objectName)
        : this(database, "dbo", objectName)
    {
    }

    /// <summary>
    /// Gets the database name.
    /// </summary>
    public string Database { get; }

    /// <summary>
    /// Gets the schema name.
    /// </summary>
    public string Schema { get; }

    /// <summary>
    /// Gets the object name.
    /// </summary>
    public string ObjectName { get; }

    /// <summary>
    /// Gets the string representation in Database.Schema.Object format.
    /// </summary>
    public override string PathValue => $"{Database}.{Schema}.{ObjectName}";

    /// <summary>
    /// Gets the domain (Sql).
    /// </summary>
    public override string Domain => "Sql";

    /// <summary>
    /// Gets the quoted identifier format for T-SQL.
    /// </summary>
    public string QuotedIdentifier => $"[{Database}].[{Schema}].[{ObjectName}]";

    /// <summary>
    /// Gets the schema.object format (without database).
    /// </summary>
    public string SchemaQualifiedName => $"[{Schema}].[{ObjectName}]";
}
