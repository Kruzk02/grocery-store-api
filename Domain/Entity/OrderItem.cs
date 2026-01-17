using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Domain.Entity;

public class OrderItem
{
    [Key]
    public int Id { get; init; }
    [Required]
    public int OrderId { get; init; }
    [ForeignKey(nameof(OrderId)), JsonIgnore]
    public required Order Order { get; init; }

    [Required]
    public int ProductId { get; set; }
    [ForeignKey(nameof(ProductId))]
    public required Product Product { get; init; }

    [Required]
    [Range(1, int.MaxValue)]
    public int Quantity { get; set; }

    public decimal SubTotal => Quantity * Product.Price;
}
