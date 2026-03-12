
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
        return await ctx.Inventories
            .Include(i => i.Product)
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

    public async Task<Inventory?> FindById(int Id)
    {
        return await ctx.Inventories.FindAsync(Id);
    }

    public async Task<List<Inventory>> FindByProductId(int ProductId)
    {
        return await ctx.Inventories
            .Where(i => i.ProductId == ProductId)
            .Include(i => i.Product)
            .ToListAsync();
    }

    public async Task<List<Inventory>> FindByStock(int Stock)
    {
        return await ctx.Inventories
            .Where(i => i.Stock == Stock)
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
