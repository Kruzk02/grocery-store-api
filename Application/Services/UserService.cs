using System.Security.Claims;

using Application.Dtos.Request;
using Application.Interface;
using Application.Repository;

using Domain.Entity;
using Domain.Exception;

namespace Application.Services;

public class UserService(
    IUserRepository userRepository,
    TokenService tokenService
    ) : IUserService
{
    public async Task<User> CreateUser(RegisterDto dto)
    {
        var user = new User
        {
            Username = dto.Username,
            Email = dto.Email,
        };

        var result = await userRepository.Add(user, dto.Password);
        return result;
    }

    public async Task<string> Login(LoginDto dto)
    {
        var user = await userRepository.FindByUsername(dto.UserNameOrEmail) ?? await userRepository.FindByEmail(dto.UserNameOrEmail);
        if (user == null) throw new NotFoundException($"User with Username or Email: {dto.UserNameOrEmail} not found");

        var result = await userRepository.CheckPasswordSignIn(user, dto.Password);
        if (!result)
            throw new ValidationException(new Dictionary<string, string[]> { { "Password", ["Invalid password"] } });

        var roles = await userRepository.GetRoles(user);
        var token = await tokenService.CreateToken(user, roles);
        return token;
    }

    public async Task<string> UpdateUser(ClaimsPrincipal user, UpdateUserDto dto)
    {
        var existingUser = await userRepository.GetUser(user);
        if (existingUser == null) throw new NotFoundException($"User not found");

        if (!string.IsNullOrEmpty(dto.Email))
        {
            var emailResult = await userRepository.UpdateEmail(existingUser, dto.Email);
            if (!emailResult) throw new Exception("Failed to update email");
        }

        if (!string.IsNullOrEmpty(dto.Username))
        {
            var usernameResult = await userRepository.UpdateUsername(existingUser, dto.Username);
            if (!usernameResult) throw new Exception("Failed to update username");
        }

        if (!string.IsNullOrEmpty(dto.Password))
        {
            var result = await userRepository.UpdatePassword(existingUser, dto.Password);
            if (!result) throw new Exception("Failed to add password");
        }

        return "User updated successfully";
    }

    public async Task<bool> DeleteUser(string id)
    {
        var existingUser = await userRepository.FindById(id);
        if (existingUser == null) throw new NotFoundException($"User with id: {id} not found");

        var result = await userRepository.Delete(existingUser);
        return result ?
            true :
            throw new Exception("Failed to delete user");
    }
}
