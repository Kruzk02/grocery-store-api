using System.ComponentModel.DataAnnotations;

namespace Domain.Entity;

public class Category
{
    [Key]
    public int Id { get; init; }

    [Required, MaxLength(64)]
    public required string Name { get; init; }

    [MaxLength(512)]
    public required string Description { get; init; }
}
