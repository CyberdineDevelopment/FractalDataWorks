using System;
using System.Collections.Generic;
using FractalDataWorks.EnhancedEnums;
using FractalDataWorks.Services;
using FractalDataWorks.Services.Abstractions;
using FractalDataWorks.Services.DataGateway.Abstractions;

namespace FractalDataWorks.Services.DataGateway;

/// <summary>
/// Service type definition for SQL Server data provider.
/// </summary>
public sealed class SqlDataGatewayServiceType :
    DataGatewayTypeBase<IDataService, IDataGatewaysConfiguration, IServiceFactory<IDataService, IDataGatewaysConfiguration>>,
    IEnumOption<SqlDataGatewayServiceType>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SqlDataGatewayServiceType"/> class.
    /// </summary>
    public SqlDataGatewayServiceType()
        : base(
            id: 1,
            name: "SqlServer",
            sectionName: "DataGateway:SqlServer",
            displayName: "SQL Server Data Gateway",
            description: "Microsoft SQL Server data provider service")
    {
    }

    /// <inheritdoc/>
    public override string[] SupportedDataStores => ["MsSql", "AzureSql", "SqlServer"];

    /// <inheritdoc/>
    public override int Priority => 100;

    /// <inheritdoc/>
    public override bool SupportsTransactions => true;

    /// <inheritdoc/>
    public override bool SupportsBulkOperations => true;

    /// <inheritdoc/>
    public override bool SupportsStreaming => true;

    /// <inheritdoc/>
    public override int MaxBatchSize => 10000;

    /// <inheritdoc/>
    public override string ProviderName => "System.Data.SqlClient";

    /// <inheritdoc/>
    public override string ConnectionString => "Server={server};Database={database};Integrated Security=true;";

    /// <inheritdoc/>
    public override bool SupportsSchemaDiscovery => true;

    /// <inheritdoc/>
    public override IReadOnlyList<string> SupportedCommands => ["Query", "Insert", "Update", "Delete", "BulkInsert", "BulkUpsert", "Exists", "Count", "Truncate"];

    /// <inheritdoc/>
    public override int MaxConnectionPoolSize => 100;

    /// <inheritdoc/>
    public override Type GetFactoryImplementationType()
    {
        // Use the generic factory for now
        // Can be overridden later to return a custom SqlDataGatewayServiceFactory if needed
        return typeof(GenericServiceFactory<IDataGateway, IDataGatewayConfiguration>);
    }
}