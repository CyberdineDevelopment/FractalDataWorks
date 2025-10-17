using FractalDataWorks.Collections.Attributes;

namespace FractalDataWorks.Services.Connections.Abstractions;

/// <summary>
/// The connection is in an unknown or uninitialized state.
/// </summary>
[TypeOption(typeof(ConnectionStates), "Unknown")]
public sealed class UnknownConnectionState() : ConnectionStateBase(0, "Unknown");