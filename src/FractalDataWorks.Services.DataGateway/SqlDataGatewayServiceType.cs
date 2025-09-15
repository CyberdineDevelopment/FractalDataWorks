using System;
using FractalDataWorks.EnhancedEnums;
using FractalDataWorks.Services;
using FractalDataWorks.Services.Abstractions;
using FractalDataWorks.Services.DataGateway.Abstractions;

namespace FractalDataWorks.Services.DataGateway;

/// <summary>
/// Service type definition for SQL Server data provider.
/// </summary>
public sealed class SqlDataGatewayServiceType : 
    DataGatewayServiceType<SqlDataGatewayServiceType, IDataService, IDataGatewaysConfiguration, IServiceFactory<IDataService, IDataGatewaysConfiguration>>,
    IEnumOption<SqlDataGatewayServiceType>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SqlDataGatewayServiceType"/> class.
    /// </summary>
    public SqlDataGatewayServiceType() 
        : base(
            id: 1, 
            name: "SqlServer", 
            description: "Microsoft SQL Server data provider service",
            providerName: "System.Data.SqlClient",
            connectionString: "Server={server};Database={database};Integrated Security=true;",
            supportedDataStores: ["MsSql", "AzureSql", "SqlServer"],
            supportedCommands: ["Query", "Insert", "Update", "Delete", "BulkInsert", "BulkUpsert", "Exists", "Count", "Truncate"
            ],
            priority: 100,
            supportsTransactions: true,
            supportsBulkOperations: true,
            supportsStreaming: true,
            supportsSchemaDiscovery: true,
            maxBatchSize: 10000,
            maxConnectionPoolSize: 100)
    {
    }

    /// <inheritdoc/>
    public override Type GetFactoryImplementationType()
    {
        // Use the generic factory for now
        // Can be overridden later to return a custom SqlDataGatewayServiceFactory if needed
        return typeof(GenericServiceFactory<IDataService, IDataGatewaysConfiguration>);
    }
}