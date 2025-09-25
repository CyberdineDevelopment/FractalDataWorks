using FractalDataWorks.Collections.Attributes;

namespace FractalDataWorks.Services.Connections.Abstractions;

/// <summary>
/// The connection is currently being closed.
/// </summary>
[TypeOption(typeof(ConnectionStates), "Closing")]
public sealed class ClosingConnectionState() : ConnectionStateBase(5, "Closing");