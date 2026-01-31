using System.Security.Claims;

using Domain.Entity;

namespace Application.Repository;

public interface IUserRepository
{
    Task<User> Add(User user, string password);
    Task<User> GetUser(ClaimsPrincipal user);
    Task<User?> FindById(string id);
    Task<User?> FindByUsername(string username);
    Task<User?> FindByEmail(string email);
    Task<IList<string>> GetRoles(User user);
    Task<List<User>> GetUserByRole(string role, CancellationToken stoppingToken);
    Task<bool> CheckPasswordSignIn(User user, string password);
    Task<bool> UpdateEmail(User user, string email);
    Task<bool> UpdateUsername(User user, string username);
    Task<bool> UpdatePassword(User user, string password);
    Task<bool> Delete(User user);
}
