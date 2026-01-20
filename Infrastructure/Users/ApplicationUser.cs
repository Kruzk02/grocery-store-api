using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Users;

public class ApplicationUser : IdentityUser
{
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
