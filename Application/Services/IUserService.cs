using System.Security.Claims;

using Application.Dtos.Request;

namespace Application.Services;

public interface IUserService
{
    Task<string> CreateUser(RegisterDto dto);
    Task<string> Login(LoginDto dto);
    Task<string> UpdateUser(ClaimsPrincipal user, UpdateUserDto dto);
    Task<bool> DeleteUser(string id);
}
