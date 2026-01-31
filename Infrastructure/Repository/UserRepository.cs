using System.Security.Claims;

using Application.Repository;

using Domain.Entity;

using Infrastructure.Persistence;
using Infrastructure.Users;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repository;

public class UserRepository(ApplicationDbContext ctx, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager) : IUserRepository
{
    public async Task<User> Add(User user, string password)
    {
        var appUser = new ApplicationUser
        {
            UserName = user.Username,
            Email = user.Email
        };

        var result = await userManager.CreateAsync(appUser, password);

        if (!result.Succeeded) throw new Exception($"User creation failed: {result.Errors.Select(e => e.Description).FirstOrDefault()}");
        user.Id = appUser.Id;

        var roleResult = await userManager.AddToRoleAsync(appUser, "user");

        return roleResult.Succeeded ? user : throw new Exception($"User created, but role assigment failed: {result.Errors.Select(e => e.Description).FirstOrDefault()}");
    }

    public async Task<User> GetUser(ClaimsPrincipal user)
    {
        var appUser = await userManager.GetUserAsync(user);
        return map(appUser!);
    }

    public async Task<User?> FindById(string id)
    {
        var appUser = await userManager.FindByIdAsync(id);
        if (appUser == null) return null;
        return map(appUser);
    }

    public async Task<User?> FindByUsername(string username)
    {
        var appUser = await userManager.FindByNameAsync(username);
        if (appUser == null) return null;
        return map(appUser);
    }

    public async Task<User?> FindByEmail(string email)
    {
        var appUser = await userManager.FindByEmailAsync(email);
        if (appUser == null) return null;
        return map(appUser);
    }

    public async Task<IList<string>> GetRoles(User user)
    {
        var appUser = await userManager.FindByIdAsync(user.Id);
        return await userManager.GetRolesAsync(appUser!);
    }

    public async Task<List<User>> GetUserByRole(string role, CancellationToken stoppingToken)
    {
        return await (from user in ctx.Users
                      join userRole in ctx.UserRoles on user.Id equals userRole.UserId
                      join roles in ctx.Roles on userRole.RoleId equals roles.Id
                      where roles.Name == role
                      select new User
                      {
                          Id = user.Id,
                          Username = user.UserName!,
                          Email = user.Email!,
                      })
            .ToListAsync(stoppingToken);
    }

    public async Task<bool> CheckPasswordSignIn(User user, string password)
    {
        var appUser = await userManager.FindByIdAsync(user.Id);
        var result = await signInManager.CheckPasswordSignInAsync(appUser!, password, false);
        return result.Succeeded;
    }

    public async Task<bool> UpdateEmail(User user, string email)
    {
        var appUser = await userManager.FindByIdAsync(user.Id);
        var result = await userManager.SetEmailAsync(appUser!, email);
        return result.Succeeded;
    }

    public async Task<bool> UpdateUsername(User user, string username)
    {
        var appUser = await userManager.FindByIdAsync(user.Id);
        var result = await userManager.SetUserNameAsync(appUser!, username);
        return result.Succeeded;
    }

    public async Task<bool> UpdatePassword(User user, string password)
    {
        var appUser = await userManager.FindByIdAsync(user.Id);

        var removePassword = await userManager.RemovePasswordAsync(appUser!);
        if (!removePassword.Succeeded) throw new Exception($"Failed to remove password: {removePassword.Errors.Select(e => e.Description).FirstOrDefault()}");

        var addPassword = await userManager.AddPasswordAsync(appUser!, password);
        if (!addPassword.Succeeded) throw new Exception($"Failed to added password: {addPassword.Errors.Select(e => e.Description).FirstOrDefault()}");

        return true;
    }

    public async Task<bool> Delete(User user)
    {
        var appUser = await userManager.FindByIdAsync(user.Id);
        var result = await userManager.DeleteAsync(appUser!);
        return result.Succeeded;
    }

    private static User map(ApplicationUser user) => new User
    {
        Id = user.Id,
        Username = user.UserName!,
        Email = user.Email!
    };
}
