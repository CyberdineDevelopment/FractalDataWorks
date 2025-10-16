using FractalDataWorks.Collections.Attributes;

namespace FractalDataWorks.Services.Connections.Abstractions;

/// <summary>
/// The connection is open and ready for use.
/// </summary>
[TypeOption(typeof(ConnectionStates), "Open")]
public sealed class OpenConnectionState() : ConnectionStateBase(3, "Open");