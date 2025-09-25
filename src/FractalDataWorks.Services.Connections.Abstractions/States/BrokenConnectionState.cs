using FractalDataWorks.Collections.Attributes;

namespace FractalDataWorks.Services.Connections.Abstractions;

/// <summary>
/// The connection is in a broken or faulted state and cannot be used.
/// </summary>
[TypeOption(typeof(ConnectionStates), "Broken")]
public sealed class BrokenConnectionState() : ConnectionStateBase(7, "Broken");