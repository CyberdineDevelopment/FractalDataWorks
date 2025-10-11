namespace FractalDataWorks.Commands.Abstractions.Commands;

/// <summary>
/// Conflict resolution behaviors.
/// </summary>
public enum ConflictResolution
{
    /// <summary>
    /// Fail the operation on conflict.
    /// </summary>
    Fail,

    /// <summary>
    /// Skip conflicting records.
    /// </summary>
    Skip,

    /// <summary>
    /// Update existing records.
    /// </summary>
    Update,

    /// <summary>
    /// Replace existing records entirely.
    /// </summary>
    Replace,

    /// <summary>
    /// Merge with existing records.
    /// </summary>
    Merge
}