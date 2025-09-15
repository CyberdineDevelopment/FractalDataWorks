using FractalDataWorks.Services.Abstractions;

namespace FractalDataWorks.Services.DataGateway.Abstractions;

/// <summary>
/// Non-generic marker interface for data service factories.
/// </summary>
public interface IDataServiceFactory : IServiceFactory
{
}

/// <summary>
/// Interface for data service factories that create specific data service implementations.
/// </summary>
/// <typeparam name="TDataService">The data service type to create.</typeparam>
public interface IDataServiceFactory<TDataService> : IDataServiceFactory, IServiceFactory<TDataService>
    where TDataService : class, IDataService
{
}

/// <summary>
/// Interface for data service factories that create data services with configuration.
/// </summary>
/// <typeparam name="TDataService">The data service type to create.</typeparam>
/// <typeparam name="TConfiguration">The configuration type for the data service.</typeparam>
public interface IDataServiceFactory<TDataService, TConfiguration> : IDataServiceFactory<TDataService>, IServiceFactory<TDataService, TConfiguration>
    where TDataService : class, IDataService
    where TConfiguration : class, IDataGatewaysConfiguration
{
}