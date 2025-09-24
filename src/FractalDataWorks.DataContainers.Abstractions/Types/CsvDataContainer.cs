using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FractalDataWorks.DataStores.Abstractions;
using FractalDataWorks.Results;

namespace FractalDataWorks.DataContainers.Abstractions.Types;

/// <summary>
/// CSV data container implementation.
/// </summary>
internal sealed class CsvDataContainer : IDataContainer
{
    public CsvDataContainer(DataLocation location, CsvContainerConfiguration configuration)
    {
        Location = location ?? throw new ArgumentNullException(nameof(location));
        Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        Id = Guid.NewGuid().ToString();
        Name = $"CSV Container ({location})";
        Schema = null!; // TODO: Implement proper schema - should come from source generator
        Metadata = new Dictionary<string, object>(StringComparer.Ordinal);
    }

    public string Id { get; }
    public string Name { get; }
    public string ContainerType => "CSV";
    public IDataSchema Schema { get; }
    public IReadOnlyDictionary<string, object> Metadata { get; }
    public DataLocation Location { get; }
    public IContainerConfiguration Configuration { get; }

    public Task<IFdwResult> ValidateReadAccessAsync(DataLocation location) =>
        Task.FromResult<IFdwResult>(FdwResult.Success());

    public Task<IFdwResult> ValidateWriteAccessAsync(DataLocation location) =>
        Task.FromResult<IFdwResult>(FdwResult.Success());

    public Task<IFdwResult<ContainerMetrics>> GetReadMetricsAsync(DataLocation location) =>
        Task.FromResult(FdwResult<ContainerMetrics>.Failure("Not implemented"));

    public Task<IFdwResult<IDataReader>> CreateReaderAsync(DataLocation location) =>
        Task.FromResult(FdwResult<IDataReader>.Failure("Not implemented"));

    public Task<IFdwResult<IDataWriter>> CreateWriterAsync(DataLocation location, ContainerWriteMode writeMode = ContainerWriteMode.Overwrite) =>
        Task.FromResult(FdwResult<IDataWriter>.Failure("Not implemented"));

    public Task<IFdwResult<IDataSchema>> DiscoverSchemaAsync(DataLocation location, int sampleSize = 1000) =>
        Task.FromResult(FdwResult<IDataSchema>.Failure("Not implemented"));
}