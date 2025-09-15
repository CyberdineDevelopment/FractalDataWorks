using FractalDataWorks.EnhancedEnums.Attributes;

namespace FractalDataWorks.Services.Connections.Abstractions;

/// <summary>
/// The connection is open and ready for use.
/// </summary>
[EnumOption("Open")]
public sealed class OpenConnectionState() : ConnectionStateBase(3, "Open");