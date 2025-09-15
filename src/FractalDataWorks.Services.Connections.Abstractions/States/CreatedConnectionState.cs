using FractalDataWorks.EnhancedEnums.Attributes;

namespace FractalDataWorks.Services.Connections.Abstractions;

/// <summary>
/// The connection has been created but not yet initialized or opened.
/// </summary>
[EnumOption("Created")]
public sealed class CreatedConnectionState() : ConnectionStateBase(1, "Created");