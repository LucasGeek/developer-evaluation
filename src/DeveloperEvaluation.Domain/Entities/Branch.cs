using DeveloperEvaluation.Domain.Common;

namespace DeveloperEvaluation.Domain.Entities;

/// <summary>
/// Represents a business branch/location in the system
/// </summary>
public class Branch : BaseEntity
{
    /// <summary>
    /// Gets the branch name
    /// </summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the branch description
    /// </summary>
    public string Description { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the branch address
    /// </summary>
    public string Address { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the branch city
    /// </summary>
    public string City { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the branch state
    /// </summary>
    public string State { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the branch postal code
    /// </summary>
    public string PostalCode { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the branch phone number
    /// </summary>
    public string Phone { get; private set; } = string.Empty;

    /// <summary>
    /// Gets whether the branch is active
    /// </summary>
    public bool IsActive { get; private set; } = true;

    /// <summary>
    /// Protected constructor for EF Core
    /// </summary>
    protected Branch() { }

    /// <summary>
    /// Creates a new branch
    /// </summary>
    public Branch(string name, string description, string address, string city, string state, string postalCode, string phone)
    {
        Name = name;
        Description = description;
        Address = address;
        City = city;
        State = state;
        PostalCode = postalCode;
        Phone = phone;
        IsActive = true;
    }

    /// <summary>
    /// Updates branch details
    /// </summary>
    public void UpdateDetails(string name, string description, string address, string city, string state, string postalCode, string phone)
    {
        Name = name;
        Description = description;
        Address = address;
        City = city;
        State = state;
        PostalCode = postalCode;
        Phone = phone;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Activates the branch
    /// </summary>
    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Deactivates the branch
    /// </summary>
    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }
}