
using Domain.Entity;

namespace Application.Repository;

public interface IUserRepository
{
    Task<User> Add(User user);
    Task<User> FindById(string id);
    Task<User> FindByUsername(string username);
    Task<User> FindBuEmail(string email);
    Task<bool> CheckPasswordSignIn(User user, string password);
    Task<bool> UpdateEmail(User user, string email);
    Task<bool> UpdateUser(User user, string username);
    Task<bool> UpdatePassword(User user, string password);
    Task<bool> Delete(User user);
}
