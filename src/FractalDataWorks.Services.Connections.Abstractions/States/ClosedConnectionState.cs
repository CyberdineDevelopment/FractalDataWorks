using FractalDataWorks.Collections.Attributes;

namespace FractalDataWorks.Services.Connections.Abstractions;

/// <summary>
/// The connection is closed.
/// </summary>
[TypeOption(typeof(ConnectionStates), "Closed")]
public sealed class ClosedConnectionState() : ConnectionStateBase(6, "Closed");