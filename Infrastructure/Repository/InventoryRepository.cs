using Application.Repository;
using Domain.Entity;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Infrastructure.Repository;

public class InventoryRepository(ApplicationDbContext ctx) : IInventoryRepository
{
    public async Task<List<Inventory>> FindAll()
    {
        return await ctx
            .Inventories.Include(i => i.Product)
                .ThenInclude(p => p.Category)
            .ToListAsync();
    }

    public async Task<Inventory> Add(Inventory inventory)
    {
        EntityEntry<Inventory> result = await ctx.Inventories.AddAsync(inventory);
        _ = await ctx.SaveChangesAsync();
        return result.Entity;
    }

    public async Task Update(Inventory inventory)
    {
        _ = ctx.Inventories.Update(inventory);
        _ = await ctx.SaveChangesAsync();
    }

    public async Task<Inventory?> FindById(int id)
    {
        return await ctx.Inventories.FindAsync(id);
    }

    public async Task<List<Inventory>> FindByProductId(int productId)
    {
        return await ctx
            .Inventories.Where(i => i.ProductId == productId)
            .Include(i => i.Product)
            .ToListAsync();
    }

    public async Task<List<Inventory>> FindByStock(int stock)
    {
        return await ctx
            .Inventories.Where(i => i.Stock == stock)
            .Include(i => i.Product)
            .ToListAsync();
    }

    public async Task<Inventory?> FindLessThanTenQuantity(CancellationToken stoppingToken)
    {
        return await ctx.Inventories.Where(i => i.Stock <= 10).FirstOrDefaultAsync(stoppingToken);
    }

    public async Task Delete(Inventory inventory)
    {
        _ = ctx.Inventories.Remove(inventory);
        _ = await ctx.SaveChangesAsync();
    }
}
