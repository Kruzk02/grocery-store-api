using Microsoft.AspNetCore.Identity;

namespace Domain.Entity;

public class ApplicationUser : IdentityUser
{
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
