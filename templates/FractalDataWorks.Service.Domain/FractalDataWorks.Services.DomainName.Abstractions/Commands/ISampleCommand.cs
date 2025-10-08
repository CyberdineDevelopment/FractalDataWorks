namespace FractalDataWorks.Services.DomainName.Abstractions.Commands;

/// <summary>
/// Sample command for DomainName operations.
/// TODO: Replace with your actual command interfaces.
/// </summary>
public interface ISampleCommand : IDomainNameCommand
{
    /// <summary>
    /// Gets the sample parameter.
    /// </summary>
    string SampleParameter { get; init; }
}
