using Application.Dtos.Request;

using Domain.Entity;
using Domain.Exception;

using Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace Application.Services.impl;

public class OrderItemService(ApplicationDbContext ctx, IMemoryCache cache) : IOrderItemService
{
    public async Task<OrderItem> Create(OrderItemDto orderItemDto)
    {
        var order = await ctx.Orders.FindAsync(orderItemDto.OrderId);
        if (order == null)
        {
            throw new NotFoundException($"Order with id {orderItemDto.OrderId} not found");
        }

        var product = await ctx.Products.FindAsync(orderItemDto.ProductId);
        if (product == null)
        {
            throw new NotFoundException($"Product with id {orderItemDto.ProductId} not found");
        }

        var quantity = orderItemDto.Quantity;
        if (quantity <= 0)
        {
            throw new ValidationException(new Dictionary<string, string[]> { { "Quantity", ["Quantity is negative or zero"] } });
        }

        if (product.Quantity < quantity)
        {
            throw new ValidationException(new Dictionary<string, string[]> { { "Quantity", ["Insufficient stock"] } });
        }

        product.Quantity -= quantity;

        var orderItem = new OrderItem
        {
            OrderId = orderItemDto.OrderId,
            Order = order,
            ProductId = orderItemDto.ProductId,
            Product = product,
            Quantity = orderItemDto.Quantity,
        };

        var result = await ctx.OrderItems.AddAsync(orderItem);
        await ctx.SaveChangesAsync();

        return result.Entity;
    }

    public async Task<OrderItem> Update(int id, OrderItemDto orderItemDto)
    {
        var orderItem = await ctx.OrderItems.FindAsync(id);
        if (orderItem == null)
        {
            throw new NotFoundException($"Order item with id {id} not found");
        }

        if (orderItem.ProductId != orderItemDto.ProductId)
        {
            var product = await ctx.Products.FindAsync(orderItemDto.ProductId);
            if (product == null)
            {
                throw new NotFoundException($"Product with id {orderItemDto.ProductId} not found");
            }
            orderItem.ProductId = orderItemDto.ProductId;
        }

        if (orderItem.OrderId != orderItemDto.OrderId)
        {
            throw new ValidationException(new Dictionary<string, string[]> { { "OrderId", ["You cannot change the order"] } });
        }

        if (orderItem.Quantity != orderItemDto.Quantity && orderItemDto.Quantity >= 0)
        {
            var product = await ctx.Products.FindAsync(orderItem.ProductId);
            if (product == null)
            {
                throw new NotFoundException($"Product with id {orderItem.ProductId} not found");
            }

            var availableStock = product.Quantity + orderItem.Quantity;

            if (availableStock < orderItemDto.Quantity)
            {
                throw new ValidationException(new Dictionary<string, string[]>
                    { { "Quantity", ["Insufficient stock"] } });
            }

            product.Quantity += orderItem.Quantity;
            orderItem.Quantity = orderItemDto.Quantity;
            product.Quantity -= orderItem.Quantity;
        }

        await ctx.SaveChangesAsync();

        return orderItem;
    }

    public async Task<OrderItem> FindById(int id)
    {
        var cacheKey = $"orderItem:{id}";
        if (cache.TryGetValue(cacheKey, out OrderItem? orderItem))
            if (orderItem != null)
                return orderItem;

        orderItem = await ctx.OrderItems.FindAsync(id);

        var cacheOption = new MemoryCacheEntryOptions()
            .SetSlidingExpiration(TimeSpan.FromMinutes(10))
            .SetAbsoluteExpiration(TimeSpan.FromMinutes(20));

        cache.Set(cacheKey, orderItem, cacheOption);

        return orderItem ?? throw new NotFoundException($"Order item with id {id} not found");
    }

    public async Task<List<OrderItem>> FindByOrderId(int orderId)
    {
        var cacheKey = $"order:{orderId}:orderItem";
        if (cache.TryGetValue(cacheKey, out List<OrderItem>? orderItems))
            if (orderItems != null)
                return orderItems;

        orderItems = await ctx.OrderItems
            .Where(oi => oi.OrderId == orderId)
            .Include(oi => oi.Order)
                .ThenInclude(o => o.Items)
            .Include(oi => oi.Product)
            .ToListAsync();

        var cacheOption = new MemoryCacheEntryOptions()
            .SetSlidingExpiration(TimeSpan.FromMinutes(10))
            .SetAbsoluteExpiration(TimeSpan.FromMinutes(20));

        cache.Set(cacheKey, orderItems, cacheOption);
        return orderItems;
    }

    public async Task<List<OrderItem>> FindByProductId(int productId)
    {
        var cacheKey = $"product:{productId}:orderItem";
        if (cache.TryGetValue(cacheKey, out List<OrderItem>? orderItems))
            if (orderItems != null)
                return orderItems;

        orderItems = await ctx.OrderItems
            .Where(oi => oi.ProductId == productId)
            .Include(oi => oi.Product)
            .ToListAsync();

        var cacheOption = new MemoryCacheEntryOptions()
            .SetSlidingExpiration(TimeSpan.FromMinutes(10))
            .SetAbsoluteExpiration(TimeSpan.FromMinutes(20));

        cache.Set(cacheKey, orderItems, cacheOption);
        return orderItems;
    }

    public async Task<bool> Delete(int id)
    {
        var orderItem = await ctx.OrderItems.FindAsync(id);
        if (orderItem == null)
        {
            throw new NotFoundException($"Order item with id: {id} not found");
        }

        cache.Remove($"orderItem:{id}");

        ctx.OrderItems.Remove(orderItem);
        await ctx.SaveChangesAsync();
        return true;
    }
}
