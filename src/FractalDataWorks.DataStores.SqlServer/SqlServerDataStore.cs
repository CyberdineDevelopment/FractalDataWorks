using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using FractalDataWorks.DataStores.Abstractions;
using FractalDataWorks.Messages;
using FractalDataWorks.Results;

namespace FractalDataWorks.DataStores.SqlServer;

/// <summary>
/// SQL Server data store implementation.
/// </summary>
public sealed class SqlServerDataStore : IDataStore<SqlServerConfiguration>
{
    private SqlServerConfiguration _configuration;
    private readonly Dictionary<string, object> _metadata = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="SqlServerDataStore"/> class.
    /// </summary>
    /// <param name="id">The unique identifier.</param>
    /// <param name="name">The display name.</param>
    /// <param name="configuration">The configuration.</param>
    public SqlServerDataStore(string id, string name, SqlServerConfiguration configuration)
    {
        Id = id;
        Name = name;
        _configuration = configuration;
        Location = configuration.ConnectionString;
        StoreType = SqlServerDataStoreType.Instance.Name;
    }

    /// <inheritdoc/>
    public string Id { get; }

    /// <inheritdoc/>
    public string Name { get; }

    /// <inheritdoc/>
    public string StoreType { get; }

    /// <inheritdoc/>
    public string Location { get; private set; }

    /// <inheritdoc/>
    public SqlServerConfiguration Configuration => _configuration;

    /// <inheritdoc/>
    public IReadOnlyDictionary<string, object> Metadata => _metadata;

    /// <inheritdoc/>
    public IEnumerable<IDataPath> AvailablePaths => throw new NotImplementedException("Path discovery not yet implemented");

    /// <inheritdoc/>
    public async Task<IGenericResult> TestConnectionAsync()
    {
        try
        {
            using var connection = new SqlConnection(_configuration.ConnectionString);
            await connection.OpenAsync();
            return GenericResult.Success();
        }
        catch (SqlException ex)
        {
            return GenericResult.Failure(new ErrorMessage($"SQL Server connection failed: {ex.Message}"));
        }
        catch (Exception ex)
        {
            return GenericResult.Failure(new ErrorMessage($"Connection test failed: {ex.Message}"));
        }
    }

    /// <inheritdoc/>
    public async Task<IGenericResult<IEnumerable<IDataPath>>> DiscoverPathsAsync()
    {
        try
        {
            using var connection = new SqlConnection(_configuration.ConnectionString);
            await connection.OpenAsync();

            // Query for tables and views
            var query = @"
                SELECT
                    TABLE_SCHEMA + '.' + TABLE_NAME as FullName,
                    TABLE_TYPE as Type
                FROM INFORMATION_SCHEMA.TABLES
                ORDER BY TABLE_SCHEMA, TABLE_NAME";

            using var command = new SqlCommand(query, connection);
            using var reader = await command.ExecuteReaderAsync();

            var paths = new List<IDataPath>();
            while (await reader.ReadAsync())
            {
                var fullName = reader.GetString(0);
                var type = reader.GetString(1);

                // Create path implementation (would need SqlServerDataPath class)
                // paths.Add(new SqlServerDataPath(fullName, type));
            }

            return GenericResult<IEnumerable<IDataPath>>.Success(paths);
        }
        catch (Exception ex)
        {
            return GenericResult<IEnumerable<IDataPath>>.Failure(
                new ErrorMessage($"Path discovery failed: {ex.Message}"));
        }
    }

    /// <inheritdoc/>
    public IDataPath? GetPath(string pathName)
    {
        throw new NotImplementedException("GetPath not yet implemented");
    }

    /// <inheritdoc/>
    public IGenericResult ValidateConnectionCompatibility(string connectionType)
    {
        if (connectionType.Equals("MsSql", StringComparison.OrdinalIgnoreCase) ||
            connectionType.Equals("SqlServer", StringComparison.OrdinalIgnoreCase))
        {
            return GenericResult.Success();
        }

        return GenericResult.Failure(
            new ValidationMessage($"Connection type '{connectionType}' is not compatible with SQL Server data store"));
    }

    /// <inheritdoc/>
    public async Task<IGenericResult> UpdateConfigurationAsync(SqlServerConfiguration configuration)
    {
        _configuration = configuration;
        Location = configuration.ConnectionString;

        // Test the new configuration
        return await TestConnectionAsync();
    }
}
