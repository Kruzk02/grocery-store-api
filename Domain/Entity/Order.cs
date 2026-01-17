using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entity;

public class Order
{
    [Key]
    public int Id { get; init; }
    [Required]
    public int CustomerId { get; set; }
    [ForeignKey(nameof(CustomerId))]
    public required Customer Customer { get; set; }
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;

    public List<OrderItem> Items { get; init; } = [];

    public decimal Total => Items.Sum(i => i.Quantity * i.Product.Price);
}