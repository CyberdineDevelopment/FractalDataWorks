using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using FractalDataWorks.DataSets.Abstractions;
using FractalDataWorks.Results;
using FractalDataWorks.Services.Connections.Abstractions.Translators;

namespace FractalDataWorks.Services.Connections.MsSql.Mappers;

/// <summary>
/// Maps SQL Server results into universal dataset records.
/// Handles SQL Server specific data types, null values, and type conversions.
/// </summary>
/// <remarks>
/// This mapper converts SqlDataReader, DataTable, and other SQL Server result formats
/// into strongly-typed objects that match the dataset schema. It provides comprehensive
/// type mapping between SQL Server types and .NET types, with proper null handling
/// and conversion logic.
/// </remarks>
internal sealed class SqlServerResultMapper
{
    private const int DefaultMaxResultSize = 100000;

    /// <inheritdoc/>
    public string ConnectionType => "MsSql";

    /// <inheritdoc/>
    public IEnumerable<string> SupportedResultTypes => new[]
    {
        "SqlDataReader",
        "DataTable",
        "DataSet",
        "IDataRecord"
    };

    /// <inheritdoc/>
    public async Task<IGenericResult<IEnumerable<TResult>>> MapAsync<TResult>(
        object connectionResult,
        IDataSetType dataSet,
        string containerType) where TResult : class
    {
        if (connectionResult == null)
            return GenericResult<IEnumerable<TResult>>.Failure("Connection result cannot be null");
        if (dataSet == null)
            return GenericResult<IEnumerable<TResult>>.Failure("DataSet cannot be null");

        try
        {
            // Validate the result before mapping
            var validation = await ValidateResultAsync(connectionResult, dataSet, containerType, typeof(TResult)).ConfigureAwait(false);
            if (!validation.IsSuccess)
            {
                return GenericResult<IEnumerable<TResult>>.Failure(validation.ErrorMessage!);
            }

            var results = connectionResult switch
            {
                SqlDataReader reader => await MapFromDataReaderAsync<TResult>(reader, dataSet).ConfigureAwait(false),
                DataTable table => await MapFromDataTableAsync<TResult>(table, dataSet).ConfigureAwait(false),
                DataSet dataset when dataset.Tables.Count > 0 => await MapFromDataTableAsync<TResult>(dataset.Tables[0]!, dataSet).ConfigureAwait(false),
                IDataRecord record => await MapFromDataRecordAsync<TResult>(record, dataSet).ConfigureAwait(false),
                _ => throw new NotSupportedException($"Result type '{connectionResult.GetType().Name}' is not supported")
            };

            return GenericResult<IEnumerable<TResult>>.Success(results);
        }
        catch (Exception ex)
        {
            return GenericResult<IEnumerable<TResult>>.Failure($"Mapping failed: {ex.Message}");
        }
    }

    /// <inheritdoc/>
    public async Task<IGenericResult<IEnumerable<object>>> MapAsync(
        object connectionResult,
        IDataSetType dataSet,
        string containerType,
        Type targetType)
    {
        ArgumentNullException.ThrowIfNull(connectionResult);
        ArgumentNullException.ThrowIfNull(dataSet);
        ArgumentNullException.ThrowIfNull(targetType);

        try
        {
            // Validate the result before mapping
            var validation = await ValidateResultAsync(connectionResult, dataSet, containerType, targetType).ConfigureAwait(false);
            if (!validation.IsSuccess)
            {
                return GenericResult<IEnumerable<object>>.Failure(validation.ErrorMessage!);
            }

            var results = connectionResult switch
            {
                SqlDataReader reader => await MapFromDataReaderAsync(reader, dataSet, targetType).ConfigureAwait(false),
                DataTable table => await MapFromDataTableAsync(table, dataSet, targetType).ConfigureAwait(false),
                DataSet dataset when dataset.Tables.Count > 0 => await MapFromDataTableAsync(dataset.Tables[0]!, dataSet, targetType).ConfigureAwait(false),
                IDataRecord record => await MapFromDataRecordAsync(record, dataSet, targetType).ConfigureAwait(false),
                _ => throw new NotSupportedException($"Result type '{connectionResult.GetType().Name}' is not supported")
            };

            return GenericResult<IEnumerable<object>>.Success(results);
        }
        catch (Exception ex)
        {
            return GenericResult<IEnumerable<object>>.Failure($"Dynamic mapping failed: {ex.Message}");
        }
    }

    /// <inheritdoc/>
    public async Task<IGenericResult> ValidateResultAsync(
        object connectionResult,
        IDataSetType dataSet,
        string containerType,
        Type targetType)
    {
        ArgumentNullException.ThrowIfNull(connectionResult);
        ArgumentNullException.ThrowIfNull(dataSet);
        ArgumentNullException.ThrowIfNull(targetType);

        try
        {
            // Check if result type is supported
            var resultTypeName = connectionResult.GetType().Name;
            if (!SupportedResultTypes.Contains(resultTypeName))
            {
                return GenericResult.Failure($"Result type '{resultTypeName}' is not supported by SQL Server mapper");
            }

            // Validate target type has default constructor or can be instantiated
            if (!CanInstantiate(targetType))
            {
                return GenericResult.Failure($"Target type '{targetType.Name}' cannot be instantiated - it must have a parameterless constructor");
            }

            // For DataReader, check if it's readable
            if (connectionResult is SqlDataReader reader && reader.IsClosed)
            {
                return GenericResult.Failure("SqlDataReader is closed and cannot be read");
            }

            // Basic schema validation would go here
            // In a full implementation, we'd check column types vs target properties

            return await Task.FromResult(GenericResult.Success()).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            return GenericResult.Failure($"Result validation failed: {ex.Message}");
        }
    }


    private static async Task<IEnumerable<TResult>> MapFromDataReaderAsync<TResult>(
        SqlDataReader reader,
        IDataSetType dataSet) where TResult : class
    {
        var results = new List<TResult>();
        var targetType = typeof(TResult);
        var properties = GetMappableProperties(targetType);

        while (await reader.ReadAsync().ConfigureAwait(false))
        {
            var instance = CreateInstance<TResult>();
            MapRecordToObject(reader, instance, properties, dataSet);
            results.Add(instance);
        }

        return results;
    }

    private static async Task<IEnumerable<object>> MapFromDataReaderAsync(
        SqlDataReader reader,
        IDataSetType dataSet,
        Type targetType)
    {
        var results = new List<object>();
        var properties = GetMappableProperties(targetType);

        while (await reader.ReadAsync().ConfigureAwait(false))
        {
            var instance = CreateInstance(targetType);
            MapRecordToObject(reader, instance, properties, dataSet);
            results.Add(instance);
        }

        return results;
    }

    private static async Task<IEnumerable<TResult>> MapFromDataTableAsync<TResult>(
        DataTable table,
        IDataSetType dataSet) where TResult : class
    {
        var results = new List<TResult>();
        var targetType = typeof(TResult);
        var properties = GetMappableProperties(targetType);

        foreach (DataRow row in table.Rows)
        {
            var instance = CreateInstance<TResult>();
            MapRecordToObject(row, instance, properties, dataSet);
            results.Add(instance);
        }

        return await Task.FromResult<IEnumerable<TResult>>(results).ConfigureAwait(false);
    }

    private static async Task<IEnumerable<object>> MapFromDataTableAsync(
        DataTable table,
        IDataSetType dataSet,
        Type targetType)
    {
        var results = new List<object>();
        var properties = GetMappableProperties(targetType);

        foreach (DataRow row in table.Rows)
        {
            var instance = CreateInstance(targetType);
            MapRecordToObject(row, instance, properties, dataSet);
            results.Add(instance);
        }

        return await Task.FromResult<IEnumerable<object>>(results).ConfigureAwait(false);
    }

    private static async Task<IEnumerable<TResult>> MapFromDataRecordAsync<TResult>(
        IDataRecord record,
        IDataSetType dataSet) where TResult : class
    {
        var targetType = typeof(TResult);
        var properties = GetMappableProperties(targetType);
        var instance = CreateInstance<TResult>();
        
        MapRecordToObject(record, instance, properties, dataSet);
        
        return await Task.FromResult<IEnumerable<TResult>>(new[] { instance }).ConfigureAwait(false);
    }

    private static async Task<IEnumerable<object>> MapFromDataRecordAsync(
        IDataRecord record,
        IDataSetType dataSet,
        Type targetType)
    {
        var properties = GetMappableProperties(targetType);
        var instance = CreateInstance(targetType);
        
        MapRecordToObject(record, instance, properties, dataSet);
        
        return await Task.FromResult<IEnumerable<object>>(new[] { instance }).ConfigureAwait(false);
    }

    private static void MapRecordToObject(
        IDataRecord record,
        object target,
        Dictionary<string, PropertyInfo> properties,
        IDataSetType dataSet)
    {
        for (int i = 0; i < record.FieldCount; i++)
        {
            var fieldName = record.GetName(i);
            var fieldValue = record.IsDBNull(i) ? null : record.GetValue(i);

            if (properties.TryGetValue(fieldName, out var property))
            {
                try
                {
                    var convertedValue = ConvertValue(fieldValue, property.PropertyType);
                    property.SetValue(target, convertedValue);
                }
                catch (Exception ex)
                {
                    throw new InvalidCastException($"Failed to convert field '{fieldName}' to type '{property.PropertyType.Name}': {ex.Message}", ex);
                }
            }
        }
    }

    private static void MapRecordToObject(
        DataRow row,
        object target,
        Dictionary<string, PropertyInfo> properties,
        IDataSetType dataSet)
    {
        foreach (DataColumn column in row.Table.Columns)
        {
            var fieldName = column.ColumnName;
            var fieldValue = row.IsNull(column) ? null : row[column];

            if (properties.TryGetValue(fieldName, out var property))
            {
                try
                {
                    var convertedValue = ConvertValue(fieldValue, property.PropertyType);
                    property.SetValue(target, convertedValue);
                }
                catch (Exception ex)
                {
                    throw new InvalidCastException($"Failed to convert field '{fieldName}' to type '{property.PropertyType.Name}': {ex.Message}", ex);
                }
            }
        }
    }

    private static Dictionary<string, PropertyInfo> GetMappableProperties(Type targetType)
    {
        return targetType
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanWrite)
            .ToDictionary(p => p.Name, StringComparer.OrdinalIgnoreCase);
    }

    private static TResult CreateInstance<TResult>() where TResult : class
    {
        return (TResult)Activator.CreateInstance(typeof(TResult))!;
    }

    private static object CreateInstance(Type type)
    {
        return Activator.CreateInstance(type)!;
    }

    private static bool CanInstantiate(Type type)
    {
        return type.IsClass && !type.IsAbstract && type.GetConstructor(Type.EmptyTypes) != null;
    }

    private static object? ConvertValue(object? value, Type targetType)
    {
        if (value == null || value == DBNull.Value)
        {
            return targetType.IsValueType ? Activator.CreateInstance(targetType) : null;
        }

        var targetTypeNonNullable = Nullable.GetUnderlyingType(targetType) ?? targetType;

        // Handle same type
        if (targetTypeNonNullable.IsAssignableFrom(value.GetType()))
        {
            return value;
        }

        // Handle enum conversion
        if (targetTypeNonNullable.IsEnum)
        {
            return Enum.ToObject(targetTypeNonNullable, value);
        }

        // Handle GUID conversion
        if (targetTypeNonNullable == typeof(Guid))
        {
            return value switch
            {
                string s => Guid.Parse(s),
                byte[] bytes => new Guid(bytes),
                _ => throw new InvalidCastException($"Cannot convert {value.GetType().Name} to Guid")
            };
        }

        // Handle standard type conversion
        try
        {
            return Convert.ChangeType(value, targetTypeNonNullable, CultureInfo.InvariantCulture);
        }
        catch (InvalidCastException ex)
        {
            throw new InvalidCastException($"Cannot convert value '{value}' of type '{value.GetType().Name}' to type '{targetTypeNonNullable.Name}'", ex);
        }
    }
}