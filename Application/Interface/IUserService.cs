
using System.Security.Claims;

using Application.Dtos.Request;
using Application.Dtos.Response;

using Domain.Entity;

namespace Application.Interface;

public interface IUserService
{
    Task<User> CreateUser(RegisterDto dto);
    Task<AuthResponse> Login(LoginDto dto);
    Task<User> GetUser(string usernameOrEmail);
    Task<AuthResponse> RefreshToken(string RefreshToken);
    Task<string> UpdateUser(string id, UpdateUserDto dto);
    Task Logout(ClaimsPrincipal claims);
    Task<bool> DeleteUser(string id);
}
