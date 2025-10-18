using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FractalDataWorks.Data.Abstractions;
using FractalDataWorks.Results;

namespace FractalDataWorks.Data.Sql;

/// <summary>
/// Translates universal data requests to T-SQL SELECT statements.
/// </summary>
public sealed class TSqlQueryTranslator : TranslatorBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TSqlQueryTranslator"/> class.
    /// </summary>
    public TSqlQueryTranslator()
        : base(1, "TSqlQuery")
    {
    }

    /// <summary>
    /// Gets the domain name (Sql).
    /// </summary>
    public override string DomainName => "Sql";

    /// <summary>
    /// Determines whether this translator supports the given container schema.
    /// </summary>
    public override bool SupportsSchema(IContainerSchema schema)
    {
        // TSqlQuery supports non-nested schemas (tables and views)
        return !schema.SupportsNesting;
    }

    /// <summary>
    /// Translates a data request into a T-SQL SELECT statement.
    /// </summary>
    public override Task<IGenericResult<string>> Translate(
        IContainer container,
        IDataRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (container.Path is not DatabasePath dbPath)
            {
                return Task.FromResult<IGenericResult<string>>(
                    GenericResult<string>.Failure("Container path must be a DatabasePath for TSqlQuery translator"));
            }

            var sql = new StringBuilder();

            // SELECT clause
            sql.Append("SELECT ");
            if (request.Top.HasValue)
            {
                sql.Append($"TOP {request.Top.Value} ");
            }

            if (request.SelectFields == null || request.SelectFields.Count == 0)
            {
                sql.Append("*");
            }
            else
            {
                sql.Append(string.Join(", ", request.SelectFields.Select(f => $"[{f}]")));
            }

            // FROM clause
            sql.AppendLine();
            sql.Append($"FROM {dbPath.QuotedIdentifier}");

            // WHERE clause
            if (request.Filters != null && request.Filters.Count > 0)
            {
                sql.AppendLine();
                sql.Append("WHERE ");
                var whereClauses = request.Filters.Select(f => TranslateFilter(f));
                sql.Append(string.Join(" AND ", whereClauses));
            }

            // ORDER BY clause
            if (request.Sorting != null && request.Sorting.Count > 0)
            {
                sql.AppendLine();
                sql.Append("ORDER BY ");
                var orderClauses = request.Sorting.Select(s =>
                    $"[{s.FieldName}] {(s.Direction == SortDirection.Ascending ? "ASC" : "DESC")}");
                sql.Append(string.Join(", ", orderClauses));
            }

            // OFFSET/FETCH clause (SQL Server 2012+)
            if (request.Skip.HasValue)
            {
                if (request.Sorting == null || request.Sorting.Count == 0)
                {
                    // OFFSET requires ORDER BY, so add a default one
                    sql.AppendLine();
                    sql.Append("ORDER BY (SELECT NULL)");
                }
                sql.AppendLine();
                sql.Append($"OFFSET {request.Skip.Value} ROWS");
            }

            return Task.FromResult<IGenericResult<string>>(
                GenericResult<string>.Success(sql.ToString()));
        }
        catch (Exception ex)
        {
            return Task.FromResult<IGenericResult<string>>(
                GenericResult<string>.Failure($"Failed to translate request: {ex.Message}"));
        }
    }

    private static string TranslateFilter(IFilterExpression filter)
    {
        var fieldName = $"[{filter.FieldName}]";
        var value = FormatValue(filter.Value);

        return filter.Operator.ToLowerInvariant() switch
        {
            "equals" => $"{fieldName} = {value}",
            "notequals" => $"{fieldName} <> {value}",
            "greaterthan" => $"{fieldName} > {value}",
            "greaterthanorequal" => $"{fieldName} >= {value}",
            "lessthan" => $"{fieldName} < {value}",
            "lessthanorequal" => $"{fieldName} <= {value}",
            "contains" => $"{fieldName} LIKE '%{EscapeLikeValue(filter.Value)}%'",
            "startswith" => $"{fieldName} LIKE '{EscapeLikeValue(filter.Value)}%'",
            "endswith" => $"{fieldName} LIKE '%{EscapeLikeValue(filter.Value)}'",
            "isnull" => $"{fieldName} IS NULL",
            "isnotnull" => $"{fieldName} IS NOT NULL",
            _ => throw new NotSupportedException($"Filter operator '{filter.Operator}' is not supported")
        };
    }

    private static string FormatValue(object? value)
    {
        if (value == null)
        {
            return "NULL";
        }

        return value switch
        {
            string s => $"'{s.Replace("'", "''")}'",
            bool b => b ? "1" : "0",
            DateTime dt => $"'{dt:yyyy-MM-dd HH:mm:ss}'",
            _ => value.ToString() ?? "NULL"
        };
    }

    private static string EscapeLikeValue(object? value)
    {
        if (value == null) return string.Empty;
        var str = value.ToString() ?? string.Empty;
        return str.Replace("'", "''")
                  .Replace("[", "[[]")
                  .Replace("%", "[%]")
                  .Replace("_", "[_]");
    }
}
