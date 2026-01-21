using Application.Dtos.Request;

using Domain.Entity;

namespace Application.Interface;

public interface IOrderService
{
    Task<Order> Create(OrderDto orderDto);
    Task<Order> Update(int id, OrderDto orderDto);
    Task<Order> FindById(int id);
    Task<List<Order>> FindByCustomerId(int customerId);
    Task<bool> Delete(int id);
}
