using System.Security.Claims;

using Application.Repository;

using Domain.Entity;
using Domain.Exception;

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

        IdentityResult result = await userManager.CreateAsync(appUser, password);

        if (!result.Succeeded)
        {
            throw new Exception($"User creation failed: {result.Errors.Select(e => e.Description).FirstOrDefault()}");
        }

        user.Id = appUser.Id;

        IdentityResult roleResult = await userManager.AddToRoleAsync(appUser, "Employee");

        return roleResult.Succeeded ? user : throw new Exception($"User created, but role assigment failed: {result.Errors.Select(e => e.Description).FirstOrDefault()}");
    }

    public async Task<User> GetUser(ClaimsPrincipal user)
    {
        ApplicationUser? appUser = await userManager.GetUserAsync(user);
        return map(appUser!);
    }

    public async Task<User?> FindById(string id)
    {
        ApplicationUser? appUser = await userManager.FindByIdAsync(id) ?? throw new Exception($"User not found with id: {id}");
        IList<string> roles = await userManager.GetRolesAsync(appUser);
        return map(appUser, roles);
    }

    public async Task<User?> FindByUsername(string username)
    {
        ApplicationUser? appUser = await userManager.FindByNameAsync(username) ?? throw new Exception($"User not found with username: {username}");
        IList<string> roles = await userManager.GetRolesAsync(appUser);
        return map(appUser, roles);
    }

    public async Task<User?> FindByEmail(string email)
    {
        ApplicationUser? appUser = await userManager.FindByEmailAsync(email) ?? throw new Exception($"User not found with email: {email}");
        IList<string> roles = await userManager.GetRolesAsync(appUser);
        return map(appUser, roles);
    }

    public async Task<IList<string>> GetRoles(User user)
    {
        ApplicationUser? appUser = await userManager.FindByIdAsync(user.Id!);
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
        ApplicationUser? appUser = await userManager.FindByIdAsync(user.Id!);
        SignInResult result = await signInManager.CheckPasswordSignInAsync(appUser!, password, false);
        return result.Succeeded;
    }

    public async Task<bool> UpdateEmail(User user, string email)
    {
        ApplicationUser? appUser = await userManager.FindByIdAsync(user.Id!);
        IdentityResult result = await userManager.SetEmailAsync(appUser!, email);
        return result.Succeeded;
    }

    public async Task<bool> UpdateUsername(User user, string username)
    {
        ApplicationUser? appUser = await userManager.FindByIdAsync(user.Id!);
        IdentityResult result = await userManager.SetUserNameAsync(appUser!, username);
        return result.Succeeded;
    }

    public async Task<bool> UpdatePassword(User user, string password)
    {
        ApplicationUser? appUser = await userManager.FindByIdAsync(user.Id!);

        IdentityResult removePassword = await userManager.RemovePasswordAsync(appUser!);
        if (!removePassword.Succeeded)
        {
            throw new Exception($"Failed to remove password: {removePassword.Errors.Select(e => e.Description).FirstOrDefault()}");
        }

        IdentityResult addPassword = await userManager.AddPasswordAsync(appUser!, password);
        return !addPassword.Succeeded
            ? throw new Exception($"Failed to added password: {addPassword.Errors.Select(e => e.Description).FirstOrDefault()}")
            : true;
    }

    public async Task<bool> UpdateRoles(User user, string role)
    {
        ApplicationUser? appUser = await userManager.FindByIdAsync(user.Id!) ?? throw new NotFoundException($"User not found with a id: {user.Id}");
        string[] roles = ["Admin", "Manager", "Employee"];

        if (!roles.Any(r => r.Equals(role, StringComparison.OrdinalIgnoreCase)))
        {
            throw new Exception($"Unknown role name: {role}");
        }

        _ = await userManager.RemoveFromRolesAsync(appUser, await GetRoles(user));
        IdentityResult result = await userManager.AddToRoleAsync(appUser, role);

        return result.Succeeded;
    }

    public async Task<bool> Delete(User user)
    {
        ApplicationUser? appUser = await userManager.FindByIdAsync(user.Id!);
        IdentityResult result = await userManager.DeleteAsync(appUser!);
        return result.Succeeded;
    }

    private static User map(ApplicationUser user)
    {
        return new User
        {
            Id = user.Id,
            Username = user.UserName!,
            Email = user.Email!
        };
    }

    private static User map(ApplicationUser user, IList<string> roles)
    {
        return new User
        {
            Id = user.Id,
            Username = user.UserName!,
            Email = user.Email!,
            Roles = roles
        };
    }

}
