using FractalDataWorks.Collections;
using FractalDataWorks.Collections.Attributes;
using FractalDataWorks.Services.Connections.Abstractions.States;

namespace FractalDataWorks.Services.Connections.Abstractions;

/// <summary>
/// Collection of connection states.
/// </summary>
[TypeCollection(typeof(ConnectionStateBase), typeof(IConnectionState), typeof(ConnectionStates))]
public abstract class ConnectionStates : TypeCollectionBase<ConnectionStateBase, IConnectionState>
{
}