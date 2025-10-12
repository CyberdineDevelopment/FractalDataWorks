using FractalDataWorks.Abstractions;

namespace FractalDataWorks.Services.Abstractions;

/// <summary>
/// A command for a data service.
/// </summary>
public interface IDataCommand : IGenericCommand
{

}

/// <summary>
/// A Command for a data service with a payload.
/// </summary>
/// <typeparam name="T"></typeparam>
public interface IDataCommand<T> : IDataCommand
{

}
