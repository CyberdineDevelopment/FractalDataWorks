using FractalDataWorks.EnhancedEnums.Attributes;

namespace FractalDataWorks.Services.Connections.Abstractions;

/// <summary>
/// The connection is in an unknown or uninitialized state.
/// </summary>
[EnumOption("Unknown")]
public sealed class UnknownConnectionState() : ConnectionStateBase(0, "Unknown");