using System.Security.Claims;

using Application.Dtos.Request;
using Application.Dtos.Response;
using Application.Interface;
using Application.Repository;

using Domain.Entity;
using Domain.Exception;

namespace Application.Services;

public class UserService(
    IUserRepository userRepository,
    IRefreshTokenRepository refreshTokenRepository,
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

        User result = await userRepository.Add(user, dto.Password);
        return result;
    }

    public async Task<AuthResponse> Login(LoginDto dto)
    {
        User? user = await GetUser(dto.UserNameOrEmail);
        var result = await userRepository.CheckPasswordSignIn(user, dto.Password);
        if (!result)
        {
            throw new ValidationException(new Dictionary<string, string[]> { { "Password", ["Invalid password"] } });
        }

        IList<string> roles = await userRepository.GetRoles(user);
        var accessToken = tokenService.CreateToken(user, roles);
        var refreshToken = tokenService.GenerateRefreshToken();
        var refreshTokenEntity = new RefreshToken {
            Token = refreshToken,
            UserId = user.Id,
            ExpiryDate = DateTime.UtcNow.AddDays(7)
        };

        _ = refreshTokenRepository.Add(refreshTokenEntity);

        return new AuthResponse(accessToken, refreshToken, refreshTokenEntity.ExpiryDate);
    }

    public async Task<User> GetUser(string usernameOrEmail)
    {
        return (await userRepository.FindByUsername(usernameOrEmail) ?? await userRepository.FindByEmail(usernameOrEmail)) ?? throw new NotFoundException($"User with username or email: {usernameOrEmail} not found");

    }

    public async Task<AuthResponse> RefreshToken(string RefreshToken)
    {
        RefreshToken? storedToken = await refreshTokenRepository.FindByToken(RefreshToken) ?? throw new Exception("Invalid refresh token");
        if (storedToken.IsRevoked || storedToken.ExpiryDate < DateTime.UtcNow)
        {
            throw new Exception("Refresh token expired or revoked");
        }

        User? user = await userRepository.FindById(storedToken.UserId) ?? throw new Exception($"User not found with id:{storedToken.UserId}");
        IList<string> roles = await userRepository.GetRoles(user);

        storedToken.IsRevoked = true;

        var newRefreshToken = tokenService.GenerateRefreshToken();

        var newRefreshTokenEntity = new RefreshToken
        {
            Token = newRefreshToken,
            UserId = user.Id,
            ExpiryDate = DateTime.UtcNow.AddDays(7)
        };

        await refreshTokenRepository.Add(newRefreshTokenEntity);
        var newACcessToken = tokenService.CreateToken(user, roles);

        return new AuthResponse(newACcessToken, newRefreshToken, newRefreshTokenEntity.ExpiryDate);
    }

    public async Task<string> UpdateUser(string id, UpdateUserDto dto)
    {
        User existingUser = await userRepository.FindById(id) ?? throw new NotFoundException($"User not found");
        if (!string.IsNullOrEmpty(dto.Email))
        {
            var emailResult = await userRepository.UpdateEmail(existingUser, dto.Email);
            if (!emailResult)
            {
                throw new Exception("Failed to update email");
            }
        }

        if (!string.IsNullOrEmpty(dto.Username))
        {
            var usernameResult = await userRepository.UpdateUsername(existingUser, dto.Username);
            if (!usernameResult)
            {
                throw new Exception("Failed to update username");
            }
        }

        if (!string.IsNullOrEmpty(dto.Password))
        {
            var result = await userRepository.UpdatePassword(existingUser, dto.Password);
            if (!result)
            {
                throw new Exception("Failed to add password");
            }
        }

        if (!string.IsNullOrEmpty(dto.Role))
        {
            IList<string> roles = await userRepository.GetRoles(existingUser);
            if (roles.Contains("Manager") || roles.Contains("Employee"))
            {
                var result = await userRepository.UpdateRoles(existingUser, dto.Role);

                if (!result)
                {
                    throw new Exception("Failed to update new role");
                }
            }

        }
        return "User updated successfully";
    }

    public async Task<bool> DeleteUser(string id)
    {
        User? existingUser = await userRepository.FindById(id) ?? throw new NotFoundException($"User with id: {id} not found");
        var result = await userRepository.Delete(existingUser);
        return result ?
            true :
            throw new Exception("Failed to delete user");
    }

    public Task Logout()
    {
        throw new NotImplementedException();
    }
}
