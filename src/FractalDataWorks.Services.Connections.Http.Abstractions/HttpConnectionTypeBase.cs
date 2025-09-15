using System;
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
    where TService : class, IFdwConnection, IFdwService
    where TConfiguration : class, IConnectionConfiguration
    where TFactory : class, IConnectionFactory<TService, TConfiguration>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="HttpConnectionTypeBase{TService, TConfiguration, TFactory}"/> class.
    /// </summary>
    /// <param name="id">The unique identifier for the HTTP connection type.</param>
    /// <param name="name">The name of the HTTP connection type.</param>
    /// <param name="category">The category for this HTTP connection type (defaults to "HTTP Connection").</param>
    protected HttpConnectionTypeBase(
        int id, 
        string name, 
        string? category = null) 
        : base(id, name, category ?? "HTTP Connection")
    {
    }
}