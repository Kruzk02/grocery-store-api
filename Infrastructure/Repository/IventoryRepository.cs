
using Application.Repository;

using Domain.Entity;

using Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repository;

public class InventoryRepository(ApplicationDbContext ctx) : IInventoryRepository
{
    public async Task<List<Inventory>> FindAll()
    {
        return await ctx.Inventories.ToListAsync();
    }

    public async Task<Inventory> Add(Inventory inventory)
    {
        var result = await ctx.Inventories.AddAsync(inventory);
        await ctx.SaveChangesAsync();
        return result.Entity;
    }

    public async Task Update(Inventory inventory)
    {
        ctx.Inventories.Update(inventory);
        await ctx.SaveChangesAsync();
    }

    public async Task<Inventory?> FindById(int Id)
    {
        return await ctx.Inventories.FindAsync(Id);
    }

    public async Task<Inventory?> FindLessThanTen(CancellationToken stoppingToken) {
        return await ctx.Inventories.Where(i => i.Quantity <= 10).FirstOrDefaultAsync(stoppingToken);
    }

    public async Task Delete(Inventory inventory)
    {
        ctx.Inventories.Remove(inventory);
        await ctx.SaveChangesAsync();
    }
}
