using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OmniCRM.Models;

public enum LeadStatus
{
    New,
    Contacted,
    Qualified,
    Converted,
    Lost
}

/// <summary>
/// Represents a sales lead in the OmniCRM pipeline.
/// </summary>
public class Lead
{
    [Key]
    public int Id { get; set; }

    [Required(ErrorMessage = "Lead title/subject is required.")]
    [StringLength(150, ErrorMessage = "Title cannot exceed 150 characters.")]
    public string Title { get; set; } = string.Empty;

    public LeadStatus Status { get; set; } = LeadStatus.New;

    [Column(TypeName = "decimal(18,2)")]
    public decimal EstimatedValue { get; set; } = 0.00m;

    [StringLength(500, ErrorMessage = "Notes cannot exceed 500 characters.")]
    public string Notes { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Foreign key to Customer
    public int CustomerId { get; set; }
    public virtual Customer? Customer { get; set; }
}
