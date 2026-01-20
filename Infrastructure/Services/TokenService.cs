using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

using Application.Settings;

using Infrastructure.Users;

using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Infrastructure.Services;

public class TokenService(IOptions<JwtSettings> config)
{
    private readonly JwtSettings _jwtSettings = config.Value;

    public async Task<string> CreateToken(ApplicationUser user, UserManager<ApplicationUser> userManager)
    {
        var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(_jwtSettings.Key));
        var cred = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id),
            new(JwtRegisteredClaimNames.UniqueName, user.UserName!),
            new(JwtRegisteredClaimNames.Email, user.Email!),
        };

        var roles = await userManager.GetRolesAsync(user);
        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: cred);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
