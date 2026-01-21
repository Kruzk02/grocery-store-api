using Application.Dtos.Request;

using Domain.Entity;

namespace Application.Interface;

public interface IOrderItemService
{
    Task<OrderItem> Create(OrderItemDto orderItemDto);
    Task<OrderItem> Update(int id, OrderItemDto orderItemDto);
    Task<OrderItem> FindById(int id);
    Task<List<OrderItem>> FindByOrderId(int orderId);
    Task<List<OrderItem>> FindByProductId(int productId);
    Task<bool> Delete(int id);
}
