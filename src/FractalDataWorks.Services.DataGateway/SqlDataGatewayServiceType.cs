using System;
using System.Collections.Generic;
using FractalDataWorks.ServiceTypes.Attributes;
using FractalDataWorks.Services;
using FractalDataWorks.Services.Abstractions;
using FractalDataWorks.Services.DataGateway.Abstractions;
using FractalDataWorks.Services.DataGateway.Services;

namespace FractalDataWorks.Services.DataGateway;

/// <summary>
/// Service type definition for SQL Server data provider.
/// </summary>
[ServiceTypeOption(typeof(DataGatewayTypes), "SqlDataGateway")]
public sealed class SqlDataGatewayServiceType :
    DataGatewayTypeBase<IGenericService, IDataGatewaysConfiguration, IServiceFactory<IGenericService, IDataGatewaysConfiguration>>
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
            description: "Microsoft SQL Server data provider service",
            supportedDataStores: ["MsSql", "AzureSql", "SqlServer"],
            priority: 100,
            supportsTransactions: true,
            supportsBulkOperations: true,
            supportsStreaming: true,
            maxBatchSize: 10000,
            providerName: "System.Data.SqlClient",
            connectionString: "Server={server};Database={database};Integrated Security=true;",
            supportsSchemaDiscovery: true,
            supportedCommands: new List<string> { "Query", "Insert", "Update", "Delete", "BulkInsert", "BulkUpsert", "Exists", "Count", "Truncate" }.AsReadOnly(),
            maxConnectionPoolSize: 100)
    {
    }

    /// <inheritdoc/>
    public override Type GetFactoryImplementationType()
    {
        // Use the generic factory for now
        // Can be overridden later to return a custom SqlDataGatewayServiceFactory if needed
        return typeof(GenericServiceFactory<IDataGateway, IDataGatewayConfiguration>);
    }
}