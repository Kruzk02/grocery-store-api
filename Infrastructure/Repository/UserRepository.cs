
using Application.Repository;

using Domain.Entity;

using Infrastructure.Users;

using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Repository;

public class UserRepository(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager) : IUserRepository
{
    public async Task<User> Add(User user)
    {
        var appUser = new ApplicationUser
        {
            UserName = user.Username,
            Email = user.Email
        };

        var result = await userManager.CreateAsync(appUser, user.Password);

        if (!result.Succeeded) throw new Exception($"User creation failed: {result.Errors.Select(e => e.Description).FirstOrDefault()}");
        user.Id = appUser.Id;

        var roleResult = await userManager.AddToRoleAsync(appUser, "user");

        return roleResult.Succeeded ? user : throw new Exception($"User created, but role assigment failed: {result.Errors.Select(e => e.Description).FirstOrDefault()}");
    }

    public async Task<User> FindById(string id)
    {
        var appUser = await userManager.FindByIdAsync(id);
        return new User
        {
            Id = appUser!.Id,
            Username = appUser.UserName!,
            Email = appUser.Email!,
            Password = appUser.PasswordHash!
        };
    }

    public async Task<User> FindByUsername(string username)
    {
        var appUser = await userManager.FindByNameAsync(username);
        return new User
        {
            Id = appUser!.Id,
            Username = appUser.UserName!,
            Email = appUser.Email!,
            Password = appUser.PasswordHash!
        };
    }

    public async Task<User> FindBuEmail(string email)
    {
        var appUser = await userManager.FindByEmailAsync(email);
        return new User
        {
            Id = appUser!.Id,
            Username = appUser.UserName!,
            Email = appUser.Email!,
            Password = appUser.PasswordHash!
        };
    }

    public async Task<bool> CheckPasswordSignIn(User user, string password)
    {
        var appUser = ConvertUserToApplicationUser(user);
        var result = await signInManager.CheckPasswordSignInAsync(appUser, password, false);
        return result.Succeeded;
    }

    public async Task<bool> UpdateEmail(User user, string email)
    {
        var appUser = ConvertUserToApplicationUser(user);
        var result = await userManager.SetEmailAsync(appUser, email);
        return result.Succeeded;
    }

    public async Task<bool> UpdateUser(User user, string username)
    {
        var appUser = ConvertUserToApplicationUser(user);
        var result = await userManager.SetUserNameAsync(appUser, username);
        return result.Succeeded;
    }

    public async Task<bool> UpdatePassword(User user, string password)
    {
        var appUser = ConvertUserToApplicationUser(user);

        var removePassword = await userManager.RemovePasswordAsync(appUser);
        if (!removePassword.Succeeded) throw new Exception($"Failed to remove password: {removePassword.Errors.Select(e => e.Description).FirstOrDefault()}");

        var addPassword = await userManager.AddPasswordAsync(appUser, password);
        if (!addPassword.Succeeded) throw new Exception($"Failed to added password: {addPassword.Errors.Select(e => e.Description).FirstOrDefault()}");

        return true;
    }

    public async Task<bool> Delete(User user)
    {
        var appUser = ConvertUserToApplicationUser(user);
        var result = await userManager.DeleteAsync(appUser);
        return result.Succeeded;
    }

    private ApplicationUser ConvertUserToApplicationUser(User user)
    {
        return new ApplicationUser
        {
            Id = user.Id!,
            UserName = user.Username,
            Email = user.Email,
            PasswordHash = user.Password
        };
    }
}
