using Application.Dtos;
using Application.Dtos.Request;
using Domain.Entity;
using Domain.Exception;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace Application.Services.impl;

public class OrderService(ApplicationDbContext ctx, IMemoryCache cache) : IOrderService
{
    public async Task<Order> Create(OrderDto orderDto)
    {
        var customer = await ctx.Customers.FindAsync(orderDto.CustomerId);
        if (customer == null)
        {
            throw new NotFoundException($"Customer with id {orderDto.CustomerId} not found");
        }

        var order = new Order
        {
            CustomerId = orderDto.CustomerId,
            Customer = customer
        };

        var result = await ctx.Orders.AddAsync(order);
        await ctx.SaveChangesAsync();

        return result.Entity;
    }

    public async Task<Order> Update(int id, OrderDto orderDto)
    {
        var order = await ctx.Orders.FindAsync(id);
        if (order == null)
        {
            throw new NotFoundException($"Order with id {id} not found");
        }

        if (orderDto.CustomerId != order.CustomerId && orderDto.CustomerId != 0)
        {
            var customer = await ctx.Customers.FindAsync(orderDto.CustomerId);
            if (customer == null)
            {
                throw new NotFoundException($"Customer with id {orderDto.CustomerId} not found");
            }

            order.CustomerId = orderDto.CustomerId;
            order.Customer = customer;
        }

        await ctx.SaveChangesAsync();

        return order;
    }

    public async Task<Order> FindById(int id)
    {
        var cacheKey = $"order:{id}";
        if (cache.TryGetValue(cacheKey, out Order? order))
            if (order != null)
                return order;
        order = await ctx.Orders.FindAsync(id);
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

        orders = await ctx.Orders.Where(o => o.CustomerId == customerId).ToListAsync();

        var cacheOptions = new MemoryCacheEntryOptions()
            .SetSlidingExpiration(TimeSpan.FromMinutes(10))
            .SetAbsoluteExpiration(TimeSpan.FromMinutes(30));
        cache.Set(cacheKey, orders, cacheOptions);

        return orders;
    }

    public async Task<bool> Delete(int id)
    {
        var order = await ctx.Orders.FindAsync(id);
        if (order == null)
        {
            throw new NotFoundException($"Order with id {id} not found");
        }

        cache.Remove($"order:{id}");
        ctx.Orders.Remove(order);
        await ctx.SaveChangesAsync();
        return true;
    }
}