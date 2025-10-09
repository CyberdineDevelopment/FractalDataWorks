using System;
using FractalDataWorks.Collections;
using FractalDataWorks.Commands.Abstractions;
using FractalDataWorks.Data.Abstractions;
using FractalDataWorks.Data.Abstractions.Formats;
using FractalDataWorks.EnhancedEnums;
using FractalDataWorks.Results;
using FractalDataWorks.ServiceTypes.Attributes;

namespace FractalDataWorks.Data.Sql;

/// <summary>
/// SQL translator type definition for LINQ to SQL translation.
/// </summary>
[ServiceTypeOption(typeof(TranslatorTypes), "SqlTranslator")]
public sealed class SqlTranslatorType : TranslatorTypeBase, IEnumOption<SqlTranslatorType>
{
    /// <summary>
    /// Gets the singleton instance of the SQL translator type.
    /// </summary>
    public static SqlTranslatorType Instance { get; } = new();

    /// <inheritdoc/>
    public override IDataFormat SourceFormat => LinqFormat.Instance;

    /// <inheritdoc/>
    public override IDataFormat TargetFormat => SqlFormat.Instance;

    /// <inheritdoc/>
    public override TranslationCapabilities Capabilities => new()
    {
        SupportsProjection = true,
        SupportsFiltering = true,
        SupportsOrdering = true,
        SupportsPaging = true,
        SupportsJoins = true,
        SupportsGrouping = true,
        SupportsAggregation = true,
        SupportsSubqueries = true,
        SupportsTransactions = true,
        SupportsBulkOperations = true,
        SupportsParameterization = true,
        MaxComplexityLevel = 10
    };

    /// <inheritdoc/>
    public override int Priority => 100;

    /// <inheritdoc/>
    public override IGenericResult<ICommandTranslator> CreateTranslator(IServiceProvider services)
    {
        var translator = services.GetService(typeof(SqlCommandTranslator)) as SqlCommandTranslator;

        return translator != null
            ? GenericResult.Success<ICommandTranslator>(translator)
            : GenericResult.Failure<ICommandTranslator>(
                Messages.SqlMessages.TranslatorNotRegistered.Instance);
    }

    private SqlTranslatorType()
        : base(100, "SqlTranslator", "Translates LINQ expressions to T-SQL using Microsoft.SqlServer.TransactSql.ScriptDom")
    {
    }
}