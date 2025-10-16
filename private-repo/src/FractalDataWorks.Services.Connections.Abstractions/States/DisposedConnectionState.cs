using FractalDataWorks.Collections.Attributes;

namespace FractalDataWorks.Services.Connections.Abstractions;

/// <summary>
/// The connection has been disposed and cannot be reused.
/// </summary>
[TypeOption(typeof(ConnectionStates), "Disposed")]
public sealed class DisposedConnectionState() : ConnectionStateBase(8, "Disposed");