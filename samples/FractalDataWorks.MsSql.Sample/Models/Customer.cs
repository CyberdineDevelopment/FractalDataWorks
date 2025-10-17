namespace FractalDataWorks.MsSql.Sample.Models;

/// <summary>
/// Represents a customer in the CRM system.
/// Maps to crm.Customers table.
/// </summary>
public sealed class Customer
{
    public int CustomerId { get; init; }
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string? Phone { get; init; }
    public DateTime? DateOfBirth { get; init; }
    public string CustomerType { get; init; } = "Standard";
    public bool IsActive { get; init; } = true;
    public DateTime CreatedDate { get; init; }
    public DateTime LastModifiedDate { get; init; }
    public decimal TotalSpent { get; init; }
    public int LoyaltyPoints { get; init; }
    public string CreatedBy { get; init; } = string.Empty;
    public string ModifiedBy { get; init; } = string.Empty;

    public string FullName => $"{FirstName} {LastName}";
}
