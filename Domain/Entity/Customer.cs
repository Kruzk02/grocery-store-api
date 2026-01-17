using System.ComponentModel.DataAnnotations;

namespace Domain.Entity;

public class Customer
{
    [Key]
    public int Id { get; init; }
    [Required, MinLength(3), MaxLength(255)]
    public required string Name { get; set; }
    [Required, EmailAddress, MinLength(3), MaxLength(255)]
    public required string Email { get; set; }
    [Required, Phone, MaxLength(15)]
    public required string Phone { get; set; }
    [Required, MinLength(3), MaxLength(255)]
    public required string Address { get; set; }

    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
