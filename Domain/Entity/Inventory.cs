using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entity;

public class Inventory
{
    [Key]
    public int Id { get; init; }
    public int ProductId { get; set; }
    [ForeignKey(nameof(ProductId))]
    public required Product Product { get; set; }
    public int Quantity { get; set; }
    public DateTime UpdatedAt { get; set; }
}
