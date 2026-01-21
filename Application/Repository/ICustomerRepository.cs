
using Domain.Entity;

namespace Application.Repository;

public interface ICustomerRepository
{
    Task<(int total, List<Customer>)> Search(string? name, int skip, int take);
    Task<List<Customer>> FindAll();
    Task<Customer> Add(Customer customer);
    Task Update(Customer customer);
    Task<Customer?> FindById(int id);
    Task<Customer?> FindByEmail(string email);
    Task<Customer?> FindByName(string name);
    Task<Customer?> FindByPhoneNumber(string phoneNumber);
    Task Delete(Customer customer);
}
