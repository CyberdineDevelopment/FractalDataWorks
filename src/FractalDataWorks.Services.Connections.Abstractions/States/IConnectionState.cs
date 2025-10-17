using FractalDataWorks.EnhancedEnums;

namespace FractalDataWorks.Services.Connections.Abstractions;

/// <summary>
/// Interface for connection states.
/// </summary>
public interface IConnectionState : IEnumOption<ConnectionStateBase>
{
}