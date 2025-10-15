using FractalDataWorks.Collections.Attributes;

namespace FractalDataWorks.Services.Connections.Abstractions;

/// <summary>
/// The connection is currently being opened.
/// </summary>
[TypeOption(typeof(ConnectionStates), "Opening")]
public sealed class OpeningConnectionState() : ConnectionStateBase(2, "Opening");