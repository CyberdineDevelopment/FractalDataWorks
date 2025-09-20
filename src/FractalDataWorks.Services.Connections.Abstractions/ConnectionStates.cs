namespace FractalDataWorks.Services.Connections.Abstractions;

/// <summary>
/// Static collection of connection states.
/// Manually created since Enhanced Enums generator was removed to prevent ServiceLifetimes duplication.
/// </summary>
public static class ConnectionStates
{
    /// <summary>
    /// The connection has been created but not yet opened.
    /// </summary>
    public static IConnectionState Created { get; } = new CreatedConnectionState();

    /// <summary>
    /// The connection is currently being opened.
    /// </summary>
    public static IConnectionState Opening { get; } = new OpeningConnectionState();

    /// <summary>
    /// The connection is open and ready for use.
    /// </summary>
    public static IConnectionState Open { get; } = new OpenConnectionState();

    /// <summary>
    /// The connection is currently being closed.
    /// </summary>
    public static IConnectionState Closing { get; } = new ClosingConnectionState();

    /// <summary>
    /// The connection is closed.
    /// </summary>
    public static IConnectionState Closed { get; } = new ClosedConnectionState();

    /// <summary>
    /// The connection is in a broken/faulted state.
    /// </summary>
    public static IConnectionState Broken { get; } = new BrokenConnectionState();

    /// <summary>
    /// The connection has been disposed.
    /// </summary>
    public static IConnectionState Disposed { get; } = new DisposedConnectionState();

    /// <summary>
    /// The connection is executing a command.
    /// </summary>
    public static IConnectionState Executing { get; } = new ExecutingConnectionState();

    /// <summary>
    /// Unknown connection state.
    /// </summary>
    public static IConnectionState Unknown { get; } = new UnknownConnectionState();
}