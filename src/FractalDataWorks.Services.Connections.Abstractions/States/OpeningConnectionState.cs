using FractalDataWorks.EnhancedEnums.Attributes;

namespace FractalDataWorks.Services.Connections.Abstractions;

/// <summary>
/// The connection is currently being opened.
/// </summary>
[EnumOption("Opening")]
public sealed class OpeningConnectionState() : ConnectionStateBase(2, "Opening");