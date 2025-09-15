namespace FractalDataWorksApiHost.Models;

/// <summary>
/// Response model representing a user in the system.
/// </summary>
public class UserResponse
{
    /// <summary>
    /// The unique identifier for the user.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// The user's full name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The user's email address.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// When the user was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// When the user was last updated.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Whether the user account is active.
    /// </summary>
    public bool IsActive { get; set; } = true;
}