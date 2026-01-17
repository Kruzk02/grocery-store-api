using Domain.Entity;

using Microsoft.AspNetCore.Identity;

namespace Application.Services;

public interface ITokenService
{
    Task<string> CreateToken(ApplicationUser user, UserManager<ApplicationUser> userManager);
}
