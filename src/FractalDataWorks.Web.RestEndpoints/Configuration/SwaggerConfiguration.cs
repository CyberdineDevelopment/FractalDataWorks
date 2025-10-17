namespace FractalDataWorks.Web.RestEndpoints.Configuration;

/// <summary>
/// Swagger/OpenAPI documentation configuration.
/// </summary>
public sealed class SwaggerConfiguration
{
    /// <summary>
    /// Gets or sets a value indicating whether Swagger documentation is enabled.
    /// </summary>
    public bool Enabled { get; init; } = true;

    /// <summary>
    /// Gets or sets the title for the API documentation.
    /// </summary>
    public string Title { get; init; } = "FractalDataWorks Web API";

    /// <summary>
    /// Gets or sets the description for the API documentation.
    /// </summary>
    public string Description { get; init; } = "API built with FractalDataWorks Web Framework";

    /// <summary>
    /// Gets or sets the version for the API documentation.
    /// </summary>
    public string Version { get; init; } = "v1";

    /// <summary>
    /// Gets or sets the route prefix for Swagger UI.
    /// </summary>
    public string RoutePrefix { get; init; } = "swagger";
}