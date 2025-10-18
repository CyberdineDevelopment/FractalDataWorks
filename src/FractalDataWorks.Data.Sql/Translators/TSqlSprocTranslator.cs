using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FractalDataWorks.Data.Abstractions;
using FractalDataWorks.Results;

namespace FractalDataWorks.Data.Sql;

/// <summary>
/// Translates universal data requests to T-SQL EXEC statements for stored procedures.
/// </summary>
public sealed class TSqlSprocTranslator : TranslatorBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TSqlSprocTranslator"/> class.
    /// </summary>
    public TSqlSprocTranslator()
        : base(2, "TSqlSproc")
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
        // TSqlSproc supports any schema (parameters can be nested)
        return true;
    }

    /// <summary>
    /// Translates a data request into a T-SQL EXEC statement.
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
                    GenericResult<string>.Failure("Container path must be a DatabasePath for TSqlSproc translator"));
            }

            var sql = new StringBuilder();
            sql.Append($"EXEC {dbPath.QuotedIdentifier}");

            // Parameters from filters (filters represent parameter values)
            if (request.Filters != null && request.Filters.Count > 0)
            {
                sql.AppendLine();
                var parameters = request.Filters.Select(f => FormatParameter(f));
                sql.Append("    ");
                sql.Append(string.Join($",{Environment.NewLine}    ", parameters));
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

    private static string FormatParameter(IFilterExpression filter)
    {
        var paramName = filter.FieldName.StartsWith("@", StringComparison.Ordinal) ? filter.FieldName : $"@{filter.FieldName}";
        var value = FormatValue(filter.Value);
        return $"{paramName} = {value}";
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
}
