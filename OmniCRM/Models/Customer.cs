using System.ComponentModel.DataAnnotations;

namespace OmniCRM.Models;

/// <summary>
/// Represents a Customer record in the OmniCRM system.
/// Defined according to Stage 2 specifications.
/// </summary>
public class Customer
{
    [Key]
    public int Id { get; set; }

    [Required(ErrorMessage = "Customer name is required.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 100 characters.")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email address is required.")]
    [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
    [StringLength(120, ErrorMessage = "Email cannot exceed 120 characters.")]
    public string Email { get; set; } = string.Empty;

    [Phone(ErrorMessage = "Please enter a valid phone number.")]
    [StringLength(25, ErrorMessage = "Phone number cannot exceed 25 characters.")]
    public string Phone { get; set; } = string.Empty;

    [StringLength(100, ErrorMessage = "Company name cannot exceed 100 characters.")]
    public string CompanyName { get; set; } = string.Empty;

    [StringLength(250, ErrorMessage = "Address cannot exceed 250 characters.")]
    public string Address { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual ICollection<Lead> Leads { get; set; } = new List<Lead>();
    public virtual ICollection<FollowUp> FollowUps { get; set; } = new List<FollowUp>();
}
