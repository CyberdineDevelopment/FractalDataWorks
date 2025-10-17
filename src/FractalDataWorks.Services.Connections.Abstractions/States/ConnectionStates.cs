using FractalDataWorks.Collections;
using FractalDataWorks.Collections.Attributes;
using FractalDataWorks.Services.Connections.Abstractions;

namespace FractalDataWorks.Services.Connections.Abstractions;

/// <summary>
/// Collection of connection states.
/// </summary>
[TypeCollection(typeof(ConnectionStateBase), typeof(IConnectionState), typeof(ConnectionStates))]
public abstract partial class ConnectionStates : TypeCollectionBase<ConnectionStateBase, IConnectionState>
{

}