using FractalDataWorks.Collections.Attributes;

namespace FractalDataWorks.Services.Connections.Abstractions;

/// <summary>
/// The connection is currently executing an operation.
/// </summary>
[TypeOption(typeof(ConnectionStates), "Executing")]
public sealed class ExecutingConnectionState() : ConnectionStateBase(4, "Executing");