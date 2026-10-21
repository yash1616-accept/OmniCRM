using System.ComponentModel.DataAnnotations;

namespace OmniCRM.Models;

/// <summary>
/// Represents a scheduled follow-up interaction with a Customer.
/// </summary>
public class FollowUp
{
    [Key]
    public int Id { get; set; }

    [Required(ErrorMessage = "Customer is required.")]
    public int CustomerId { get; set; }
    public virtual Customer? Customer { get; set; }

    [Required(ErrorMessage = "Scheduled date is required.")]
    public DateTime ScheduledDate { get; set; } = DateTime.UtcNow.AddDays(1);

    [Required(ErrorMessage = "Follow-up note is required.")]
    [StringLength(1000, ErrorMessage = "Notes cannot exceed 1000 characters.")]
    public string Notes { get; set; } = string.Empty;

    public bool IsCompleted { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
