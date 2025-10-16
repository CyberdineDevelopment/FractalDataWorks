using FractalDataWorks.Collections.Attributes;

namespace FractalDataWorks.Services.Connections.Abstractions;

/// <summary>
/// The connection has been created but not yet initialized or opened.
/// </summary>
[TypeOption(typeof(ConnectionStates), "Created")]
public sealed class CreatedConnectionState() : ConnectionStateBase(1, "Created");