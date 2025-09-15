using FractalDataWorks.EnhancedEnums.Attributes;

namespace FractalDataWorks.Services.Connections.Abstractions;

/// <summary>
/// The connection has been disposed and cannot be reused.
/// </summary>
[EnumOption("Disposed")]
public sealed class DisposedConnectionState() : ConnectionStateBase(8, "Disposed");