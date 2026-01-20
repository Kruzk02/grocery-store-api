using System.ComponentModel.DataAnnotations;

namespace Domain.Entity;

public class VerificationToken
{
    [Key]
    public int Id { get; init; }
    [Required]
    [MaxLength(256)]
    public required string Token { get; init; }
    [Required, MaxLength(128)]
    public required string UserId { get; init; }
    public DateTime CreatedAt { get; init; } = DateTime.Now;
    public DateTime ExpiresAt { get; init; }
}
