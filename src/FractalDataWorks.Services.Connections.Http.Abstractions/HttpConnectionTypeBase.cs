using System;
using FractalDataWorks.Abstractions;
using FractalDataWorks.EnhancedEnums;
using FractalDataWorks.Services;
using FractalDataWorks.Services.Abstractions;
using FractalDataWorks.Services.Connections.Abstractions;

namespace FractalDataWorks.Services.Connections.Http.Abstractions;

/// <summary>
/// Abstract base class for HTTP connection service types.
/// Concrete implementations should inherit from this class and specify their specific service implementation.
/// </summary>
public abstract class HttpConnectionTypeBase<TService, TConfiguration, TFactory> : 
    ConnectionTypeBase<TService, TConfiguration, TFactory>
    where TService : class, IGenericConnection, IGenericService
    where TConfiguration : class, IConnectionConfiguration
    where TFactory : class, IConnectionFactory<TService, TConfiguration>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="HttpConnectionTypeBase{TService, TConfiguration, TFactory}"/> class.
    /// </summary>
    /// <param name="id">The unique identifier for the HTTP connection type.</param>
    /// <param name="name">The name of the HTTP connection type.</param>
    /// <param name="sectionName">The configuration section name for appsettings.json.</param>
    /// <param name="displayName">The display name for this service type.</param>
    /// <param name="description">The description of what this service type provides.</param>
    /// <param name="category">The category for this HTTP connection type (defaults to "HTTP Connection").</param>
    protected HttpConnectionTypeBase(
        int id,
        string name,
        string sectionName,
        string displayName,
        string description,
        string? category = null)
        : base(id, name, sectionName, displayName, description, category ?? "HTTP Connection")
    {
    }
}