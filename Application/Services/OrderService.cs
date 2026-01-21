using Application.Dtos.Request;
using Application.Interface;
using Application.Repository;

using Domain.Entity;
using Domain.Exception;

using Microsoft.Extensions.Caching.Memory;

namespace Application.Services;

public class OrderService(IOrderRepository orderRepository, ICustomerRepository customerRepository, IMemoryCache cache) : IOrderService
{
    public async Task<Order> Create(OrderDto orderDto)
    {
        var customer = await customerRepository.FindById(orderDto.CustomerId);
        if (customer == null)
        {
            throw new NotFoundException($"Customer with id {orderDto.CustomerId} not found");
        }

        var order = new Order
        {
            CustomerId = orderDto.CustomerId,
            Customer = customer
        };

        return await orderRepository.Add(order);
    }

    public async Task<Order> Update(int id, OrderDto orderDto)
    {
        var order = await orderRepository.FindById(id);
        if (order == null)
        {
            throw new NotFoundException($"Order with id {id} not found");
        }

        if (orderDto.CustomerId != order.CustomerId && orderDto.CustomerId != 0)
        {
            var customer = await customerRepository.FindById(orderDto.CustomerId);
            if (customer == null)
            {
                throw new NotFoundException($"Customer with id {orderDto.CustomerId} not found");
            }

            order.CustomerId = orderDto.CustomerId;
            order.Customer = customer;
        }

        await orderRepository.Update(order);
        return order;
    }

    public async Task<Order> FindById(int id)
    {
        var cacheKey = $"order:{id}";
        if (cache.TryGetValue(cacheKey, out Order? order))
            if (order != null)
                return order;
        order = await orderRepository.FindById(id);
        var cacheOptions = new MemoryCacheEntryOptions()
            .SetSlidingExpiration(TimeSpan.FromMinutes(10))
            .SetAbsoluteExpiration(TimeSpan.FromMinutes(30));

        cache.Set(cacheKey, order, cacheOptions);

        return order ?? throw new NotFoundException($"Order with id {id} not found");
    }

    public async Task<List<Order>> FindByCustomerId(int customerId)
    {
        var cacheKey = $"customer:{customerId}:orders";
        if (cache.TryGetValue(cacheKey, out List<Order>? orders))
            if (orders != null)
                return orders;

        orders = await orderRepository.FindByCustomerId(customerId);

        var cacheOptions = new MemoryCacheEntryOptions()
            .SetSlidingExpiration(TimeSpan.FromMinutes(10))
            .SetAbsoluteExpiration(TimeSpan.FromMinutes(30));
        cache.Set(cacheKey, orders, cacheOptions);

        return orders;
    }

    public async Task<bool> Delete(int id)
    {
        var order = await orderRepository.FindById(id);
        if (order == null)
        {
            throw new NotFoundException($"Order with id {id} not found");
        }

        cache.Remove($"order:{id}");
        await orderRepository.Delete(order);
        return true;
    }
}
