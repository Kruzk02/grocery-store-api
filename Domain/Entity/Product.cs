using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entity;

public class Product
{
    [Key]
    public int Id { get; init; }
    [Required, MinLength(3), MaxLength(256)]
    public required string Name { get; set; }
    [MaxLength(512)]
    public required string Description { get; set; }
    [Required]
    public decimal Price { get; set; }
    public int CategoryId { get; set; }
    [ForeignKey(nameof(CategoryId))]
    public required Category Category { get; set; }
    public int Quantity { get; set; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; set; }
}