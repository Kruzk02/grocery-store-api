using System.ComponentModel.DataAnnotations;

namespace Domain.Entity;

public class Notification
{
    [Key]
    public int Id { get; init; }
    [Required, MaxLength(128)]
    public required string UserId { get; init; }
    public NotificationType Type { get; init; } = NotificationType.Info;
    [Required, MaxLength(500)]
    public required string Message { get; init; }
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; init; }

}

public enum NotificationType
{
    Info,
    Warning,
    Success,
    Error
}