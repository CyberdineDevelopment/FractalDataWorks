using FractalDataWorks.EnhancedEnums.Attributes;

namespace FractalDataWorks.Services.Connections.Abstractions;

/// <summary>
/// The connection is closed.
/// </summary>
[EnumOption("Closed")]
public sealed class ClosedConnectionState() : ConnectionStateBase(6, "Closed");