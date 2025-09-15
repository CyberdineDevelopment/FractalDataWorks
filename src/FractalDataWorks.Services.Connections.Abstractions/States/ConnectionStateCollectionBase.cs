using FractalDataWorks.EnhancedEnums;
using FractalDataWorks.EnhancedEnums.Attributes;

namespace FractalDataWorks.Services.Connections.Abstractions;

/// <summary>
/// Collection of connection states.
/// </summary>
[EnumCollection(CollectionName = "ConnectionStates")]
public abstract class ConnectionStateCollectionBase : EnumCollectionBase<ConnectionStateBase>
{
}