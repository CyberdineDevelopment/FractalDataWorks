using FractalDataWorks.Web.Http.Abstractions.Policies;
using FractalDataWorks.Web.Http.Abstractions.Security;

namespace FractalDataWorks.Web.RestEndpoints.Configuration;

/// <summary>
/// Configuration values specific to different endpoint types.
/// </summary>
public static class EndpointTypeDefaults
{
    /// <summary>
    /// Default settings for CRUD endpoints.
    /// </summary>
    public static EndpointDefaultSettings Crud => new(
        "JWT",
        "FixedWindow",
        EndpointDefaults.DefaultRequestTimeoutMs,
        EndpointDefaults.DefaultMaxRequestBodySize,
        true,
        ["User", "Admin"]);

    /// <summary>
    /// Default settings for Query endpoints.
    /// </summary>
    public static EndpointDefaultSettings Query => new(
        "ApiKey",
        "SlidingWindow",
        EndpointDefaults.DefaultRequestTimeoutMs / 2,
        1024 * 1024, // 1MB for query parameters
        false,
        []);

    /// <summary>
    /// Default settings for Command endpoints.
    /// </summary>
    public static EndpointDefaultSettings Command => new(
        "JWT",
        "TokenBucket",
        EndpointDefaults.DefaultRequestTimeoutMs * 2,
        EndpointDefaults.DefaultMaxRequestBodySize,
        true,
        ["Admin"]);

    /// <summary>
    /// Default settings for Health endpoints.
    /// </summary>
    public static EndpointDefaultSettings Health => new(
        "None",
        "Concurrency",
        5000, // 5 seconds
        1024, // 1KB
        false,
        []);

    /// <summary>
    /// Default settings for FileConfigurationSource endpoints.
    /// </summary>
    public static EndpointDefaultSettings File => new(
        "JWT",
        "Concurrency",
        EndpointDefaults.DefaultRequestTimeoutMs * 6, // 3 minutes
        104857600, // 100MB
        true,
        ["User", "Admin"]);

    /// <summary>
    /// Default settings for Event endpoints.
    /// </summary>
    public static EndpointDefaultSettings Event => new(
        "ApiKey",
        "TokenBucket",
        EndpointDefaults.DefaultRequestTimeoutMs,
        EndpointDefaults.DefaultMaxRequestBodySize,
        true,
        ["System", "Admin"]);
}