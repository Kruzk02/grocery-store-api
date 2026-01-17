using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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
    [ForeignKey(nameof(UserId))]
    public required ApplicationUser User { get; init; }
    public DateTime CreatedAt { get; init; } = DateTime.Now;
    public DateTime ExpiresAt { get; init; }
}