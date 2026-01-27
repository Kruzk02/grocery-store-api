using System.Security.Claims;

using Application.Dtos.Request;

using Domain.Entity;

namespace Application.Interface;

public interface IUserService
{
    Task<User> CreateUser(RegisterDto dto);
    Task<string> Login(LoginDto dto);
    Task<string> UpdateUser(ClaimsPrincipal user, UpdateUserDto dto);
    Task<bool> DeleteUser(string id);
}
