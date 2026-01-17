using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entity;

public class Invoice
{
    [Key]
    public int Id { get; init; }
    [Required]
    public int OrderId { get; init; }
    [ForeignKey(nameof(OrderId))]
    public required Order Order { get; init; }

    [Required]
    [MaxLength(50)]
    public string InvoiceNumber { get; init; } = string.Empty;

    public DateTime IssueDate { get; init; }
    public DateTime DueDate { get; init; }
}
