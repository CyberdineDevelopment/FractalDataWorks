using FractalDataWorks.EnhancedEnums.Attributes;

namespace FractalDataWorks.Services.SecretManagement.Abstractions;

/// <summary>
/// Health status is unknown or could not be determined.
/// </summary>
[EnumOption("Unknown")]
public sealed class UnknownHealthStatus : HealthStatus
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UnknownHealthStatus"/> class.
    /// </summary>
    public UnknownHealthStatus() : base(0, "Unknown") { }
}