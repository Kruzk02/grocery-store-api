
using Domain.Entity;

namespace Application.Repository;

public interface IOrderItemRepository
{
    Task<OrderItem> Add(OrderItem orderItem);
    Task Update(OrderItem orderItem);
    Task<OrderItem?> FindById(int Id);
    Task<List<OrderItem>> FindByOrderId(int OrderId);
    Task<List<OrderItem>> FindByProductId(int ProductId);
    Task Delete(OrderItem orderItem);
}
