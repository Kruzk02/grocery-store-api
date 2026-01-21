
using Domain.Entity;

namespace Application.Repository;

public interface IOrderRepository
{
    Task<Order> Add(Order order);
    Task Update(Order order);
    Task<Order?> FindById(int Id);
    Task<List<Order>> FindByCustomerId(int customerId);
    Task Delete(Order order);
}
