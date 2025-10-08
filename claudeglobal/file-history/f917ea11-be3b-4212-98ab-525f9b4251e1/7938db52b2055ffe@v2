using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using FractalDataWorks.Results;
using FractalDataWorks.Services.DomainName.Abstractions.Configuration;
using FractalDataWorks.Services.DomainName.Abstractions.Messages;
using FractalDataWorks.Services.DomainName.Abstractions.Providers;
using FractalDataWorks.Services.DomainName.Abstractions.ServiceTypes;

namespace FractalDataWorks.Services.DomainName;

/// <summary>
/// Provider for creating and resolving DomainName services using the ServiceType pattern.
/// </summary>
public sealed class DomainNameProvider : IDomainNameProvider
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IConfiguration _configuration;
    private readonly ILogger<DomainNameProvider> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="DomainNameProvider"/> class.
    /// </summary>
    public DomainNameProvider(
        IServiceProvider serviceProvider,
        IConfiguration configuration,
        ILogger<DomainNameProvider> logger)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public async Task<IGenericResult<IDomainNameService>> GetDomainNameService(IDomainNameConfiguration configuration)
    {
        if (configuration == null)
        {
            return GenericResult<IDomainNameService>.Failure(new ConfigurationNullMessage());
        }

        // Use DomainNameTypes collection to resolve ServiceType
        var serviceType = DomainNameTypes.Name(configuration.DomainNameType);
        if (serviceType == null)
        {
            return GenericResult<IDomainNameService>.Failure(new UnknownServiceTypeMessage(configuration.DomainNameType));
        }

        // TODO: Implement factory resolution and service creation
        // Example:
        // var factory = _serviceProvider.GetRequiredService(serviceType.FactoryType) as IGenericServiceFactory;
        // return await factory.Create(configuration);

        throw new NotImplementedException("Service resolution not yet implemented. Add factory pattern here.");
    }

    /// <inheritdoc/>
    public async Task<IGenericResult<IDomainNameService>> GetDomainNameService(string configurationName)
    {
        // TODO: Load configuration from appsettings section
        // var config = _configuration.GetSection($"Services:DomainName:{configurationName}").Get<DomainNameConfiguration>();
        // return await GetDomainNameService(config);

        throw new NotImplementedException("Configuration loading not yet implemented.");
    }
}
