using FractalDataWorks.Services.DataGateway.Abstractions;

namespace FractalDataWorks.Services.DataGateway;

/// <summary>
/// Configuration for the DataGateway service.
/// </summary>
public sealed class DataGatewayConfiguration : IDataGatewayConfiguration
{
    /// <inheritdoc/>
    public required int Id { get; init; }

    /// <inheritdoc/>
    public required string Type { get; init; }

    /// <inheritdoc/>
    public required string Name { get; init; }

    /// <inheritdoc/>
    public required string SectionName { get; init; }

    /// <inheritdoc/>
    public bool IsEnabled { get; init; } = true;
}
