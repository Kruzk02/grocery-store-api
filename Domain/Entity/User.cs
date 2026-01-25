
namespace Domain.Entity;

public class User
{
    public string? Id { get; set; }
    public required string Email { get; set; }
    public required string Username { get; set; }
    public required string Password { get; set; }
}
