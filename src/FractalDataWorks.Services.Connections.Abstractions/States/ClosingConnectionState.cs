using FractalDataWorks.EnhancedEnums.Attributes;

namespace FractalDataWorks.Services.Connections.Abstractions;

/// <summary>
/// The connection is currently being closed.
/// </summary>
[EnumOption("Closing")]
public sealed class ClosingConnectionState() : ConnectionStateBase(5, "Closing");