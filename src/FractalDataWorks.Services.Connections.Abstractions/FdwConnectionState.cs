namespace FractalDataWorks.Services.Connections.Abstractions;

/// <summary>
/// Represents the state of a connection.
/// </summary>
public enum FdwConnectionState
{
    /// <summary>
    /// The connection has been created but not yet opened.
    /// </summary>
    Created,

    /// <summary>
    /// The connection is currently being opened.
    /// </summary>
    Opening,

    /// <summary>
    /// The connection is open and ready for use.
    /// </summary>
    Open,

    /// <summary>
    /// The connection is currently being closed.
    /// </summary>
    Closing,

    /// <summary>
    /// The connection is closed.
    /// </summary>
    Closed,

    /// <summary>
    /// The connection is in a faulted state.
    /// </summary>
    Faulted
}