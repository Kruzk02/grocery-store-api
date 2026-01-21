
using Application.Repository;

using Domain.Entity;

using Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repository;

public class OrderItemRepository(ApplicationDbContext ctx) : IOrderItemRepository
{
    public async Task<OrderItem> Add(OrderItem orderItem)
    {
        var result = await ctx.OrderItems.AddAsync(orderItem);
        await ctx.SaveChangesAsync();
        return result.Entity;
    }

    public async Task Update(OrderItem orderItem)
    {
        ctx.OrderItems.Update(orderItem);
        await ctx.SaveChangesAsync();
    }

    public async Task<OrderItem?> FindById(int Id)
    {
        return await ctx.OrderItems.FindAsync(Id);
    }

    public async Task<List<OrderItem>> FindByOrderId(int OrderId)
    {
        return await ctx.OrderItems
            .Where(oi => oi.OrderId == OrderId)
            .Include(oi => oi.Order)
                .ThenInclude(o => o.Items)
            .Include(oi => oi.Product)
            .ToListAsync();
    }

    public async Task<List<OrderItem>> FindByProductId(int ProductId)
    {
        return await ctx.OrderItems
            .Where(oi => oi.ProductId == ProductId)
            .Include(oi => oi.Product)
            .ToListAsync();
    }

    public async Task Delete(OrderItem orderItem)
    {
        ctx.OrderItems.Remove(orderItem);
        await ctx.SaveChangesAsync();
    }
}
