using FractalDataWorks.EnhancedEnums.Attributes;

namespace FractalDataWorks.Services.Connections.Abstractions;

/// <summary>
/// The connection is currently executing an operation.
/// </summary>
[EnumOption("Executing")]
public sealed class ExecutingConnectionState() : ConnectionStateBase(4, "Executing");