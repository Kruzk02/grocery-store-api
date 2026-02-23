
using Application.Dtos.Request;

using Domain.Entity;

namespace Application.Interface;

public interface IUserService
{
    Task<User> CreateUser(RegisterDto dto);
    Task<string> Login(LoginDto dto);
    Task<User> GetUser(string usernameOrEmail);
    Task<string> UpdateUser(string id, UpdateUserDto dto);
    Task<bool> DeleteUser(string id);
}
