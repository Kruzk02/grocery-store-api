using System.Security.Claims;

using Application.Dtos.Request;

using Domain.Entity;
using Domain.Exception;

using Microsoft.AspNetCore.Identity;

namespace Application.Services.impl;

public class UserService(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    ITokenService tokenService
    ) : IUserService
{
    public async Task<string> CreateUser(RegisterDto dto)
    {
        var user = new ApplicationUser
        {
            UserName = dto.Username,
            Email = dto.Email
        };

        var result = await userManager.CreateAsync(user, dto.Password);

        if (!result.Succeeded) throw new ValidationException(new Dictionary<string, string[]> { { "User creation failed", [result.Errors.Select(e => e.Description).FirstOrDefault()!] } });
        var roleResult = await userManager.AddToRoleAsync(user, "User");

        return roleResult.Succeeded ?
            "User created successfully" :
            throw new ValidationException(new Dictionary<string, string[]> { { "User created, but role assignment failed", [result.Errors.Select(e => e.Description).FirstOrDefault()!] } });
    }

    public async Task<string> Login(LoginDto dto)
    {
        var user = await userManager.FindByNameAsync(dto.UserNameOrEmail) ?? await userManager.FindByEmailAsync(dto.UserNameOrEmail);
        if (user == null) throw new NotFoundException($"User with Username or Email: {dto.UserNameOrEmail} not found");

        var result = await signInManager.CheckPasswordSignInAsync(user, dto.Password, false);
        if (!result.Succeeded)
            throw new ValidationException(new Dictionary<string, string[]> { { "Password", ["Invalid password"] } });

        var token = await tokenService.CreateToken(user, userManager);
        return token;
    }

    public async Task<string> UpdateUser(ClaimsPrincipal user, UpdateUserDto dto)
    {
        var existingUser = await userManager.GetUserAsync(user);
        if (existingUser == null) throw new NotFoundException($"User not found");

        if (!string.IsNullOrEmpty(dto.Email))
        {
            var emailResult = await userManager.SetEmailAsync(existingUser, dto.Email);
            if (!emailResult.Succeeded) throw new ValidationException(new Dictionary<string, string[]> { { "Failed to update email", [emailResult.Errors.Select(e => e.Description).FirstOrDefault()!] } });
        }

        if (!string.IsNullOrEmpty(dto.Username))
        {
            var usernameResult = await userManager.SetUserNameAsync(existingUser, dto.Username);
            if (!usernameResult.Succeeded) throw new ValidationException(new Dictionary<string, string[]> { { "Failed to update username", [usernameResult.Errors.Select(e => e.Description).FirstOrDefault()!] } });
        }

        if (!string.IsNullOrEmpty(dto.Password))
        {
            var removePassword = await userManager.RemovePasswordAsync(existingUser);
            if (!removePassword.Succeeded) throw new ValidationException(new Dictionary<string, string[]> { { "Failed to remove password", [removePassword.Errors.Select(e => e.Description).FirstOrDefault()!] } });

            var addPassword = await userManager.AddPasswordAsync(existingUser, dto.Password);
            if (!addPassword.Succeeded) throw new ValidationException(new Dictionary<string, string[]> { { "Failed to add password", [addPassword.Errors.Select(e => e.Description).FirstOrDefault()!] } });
        }

        return "User updated successfully";
    }

    public async Task<ApplicationUser> GetUser(ClaimsPrincipal user)
    {
        var existingUser = await userManager.GetUserAsync(user);
        return existingUser ?? throw new NotFoundException("User not found");
    }

    public async Task<bool> DeleteUser(string id)
    {
        var existingUser = await userManager.FindByIdAsync(id);
        if (existingUser == null) throw new NotFoundException($"User with id: {id} not found");

        var result = await userManager.DeleteAsync(existingUser);
        return result.Succeeded ?
            true :
            throw new ValidationException(new Dictionary<string, string[]> { { "Failed to delete user", [result.Errors.Select(e => e.Description).FirstOrDefault()!] } });
    }
}
