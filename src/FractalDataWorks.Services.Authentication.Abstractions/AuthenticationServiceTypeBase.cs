using System;
using System.Collections.Generic;
using FractalDataWorks.EnhancedEnums;

namespace FractalDataWorks.Services.Authentication.Abstractions;

/// <summary>
/// Base authentication service type for Enhanced Enum pattern.
/// </summary>
public abstract class AuthenticationServiceTypeBase : EnumOptionBase<AuthenticationServiceTypeBase>, IEnumOption<AuthenticationServiceTypeBase>, IAuthenticationServiceType
{
    /// <summary>
    /// Gets the authentication provider name.
    /// </summary>
    public string ProviderName { get; }

    /// <summary>
    /// Gets the supported protocols.
    /// </summary>
    public string[] SupportedProtocols { get; }

    /// <summary>
    /// Gets the supported flows.
    /// </summary>
    public IReadOnlyList<string> SupportedFlows { get; }

    /// <summary>
    /// Gets the supported token types.
    /// </summary>
    public IReadOnlyList<string> SupportedTokenTypes { get; }

    /// <summary>
    /// Gets the priority for provider selection.
    /// </summary>
    public int Priority { get; }

    /// <summary>
    /// Gets whether this provider supports multi-tenant scenarios.
    /// </summary>
    public bool SupportsMultiTenant { get; }

    /// <summary>
    /// Gets whether this provider supports token caching.
    /// </summary>
    public bool SupportsTokenCaching { get; }

    /// <summary>
    /// Gets the description of this service type.
    /// </summary>
    public string Description { get; }

    /// <summary>
    /// Gets the service type.
    /// </summary>
    public Type ServiceType { get; }

    /// <summary>
    /// Gets the configuration type.
    /// </summary>
    public Type ConfigurationType { get; }

    /// <summary>
    /// Gets the factory type.
    /// </summary>
    public Type FactoryType { get; }

    /// <summary>
    /// Gets the category of this service type.
    /// </summary>
    public string Category { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthenticationServiceTypeBase"/> class.
    /// </summary>
    /// <param name="id">The unique identifier.</param>
    /// <param name="name">The name.</param>
    /// <param name="description">The description.</param>
    /// <param name="providerName">The provider name.</param>
    /// <param name="supportedProtocols">The supported protocols.</param>
    /// <param name="supportedFlows">The supported flows.</param>
    /// <param name="supportedTokenTypes">The supported token types.</param>
    /// <param name="serviceType">The service type.</param>
    /// <param name="configurationType">The configuration type.</param>
    /// <param name="factoryType">The factory type.</param>
    /// <param name="priority">The priority for provider selection.</param>
    /// <param name="supportsMultiTenant">Whether this provider supports multi-tenant scenarios.</param>
    /// <param name="supportsTokenCaching">Whether this provider supports token caching.</param>
    protected AuthenticationServiceTypeBase(
        int id, 
        string name,
        string description, 
        string providerName, 
        string[] supportedProtocols,
        string[] supportedFlows,
        string[] supportedTokenTypes,
        Type serviceType,
        Type configurationType,
        Type factoryType,
        int priority = 50,
        bool supportsMultiTenant = false,
        bool supportsTokenCaching = true)
        : base(id, name)
    {
        Description = description;
        ProviderName = providerName;
        SupportedProtocols = supportedProtocols;
        SupportedFlows = new List<string>(supportedFlows).AsReadOnly();
        SupportedTokenTypes = new List<string>(supportedTokenTypes).AsReadOnly();
        ServiceType = serviceType;
        ConfigurationType = configurationType;
        FactoryType = factoryType;
        Category = "Authentication";
        Priority = priority;
        SupportsMultiTenant = supportsMultiTenant;
        SupportsTokenCaching = supportsTokenCaching;
    }
}