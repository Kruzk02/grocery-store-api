using Application.Dtos.Request;

using Domain.Entity;
using Domain.Exception;

using Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace Application.Services.impl;

/// <summary>
/// Provides operations for create, retrieve, update and delete inventory.
/// </summary>
/// <remarks>
/// This class interacts with database to performs CRUD operations relate to inventory.
/// </remarks>
/// <param name="ctx">The <see cref="ApplicationDbContext"/> used to access the database</param>
public class InventoryService(ApplicationDbContext ctx, IMemoryCache cache) : IInventoryService
{
    /// <inheritdoc />
    public async Task<List<Inventory>> FindAll()
    {
        const string cacheKey = $"inventories";
        if (cache.TryGetValue(cacheKey, out List<Inventory>? inventories))
            if (inventories != null)
                return inventories;

        inventories = await ctx.Inventories.ToListAsync();

        var cacheOption = new MemoryCacheEntryOptions()
            .SetSlidingExpiration(TimeSpan.FromMinutes(10))
            .SetAbsoluteExpiration(TimeSpan.FromMinutes(20));

        cache.Set(cacheKey, inventories, cacheOption);

        return inventories;
    }

    /// <inheritdoc />
    public async Task<Inventory> Create(InventoryDto inventoryDto)
    {
        var product = await ctx.Products.FindAsync(inventoryDto.ProductId);
        if (product == null)
        {
            throw new NotFoundException($"Product with id: {inventoryDto.ProductId} not found");
        }

        var inventory = new Inventory
        {
            Product = product,
            ProductId = product.Id,
            Quantity = inventoryDto.Quantity,
            UpdatedAt = DateTime.UtcNow
        };

        var result = await ctx.Inventories.AddAsync(inventory);
        await ctx.SaveChangesAsync();

        return result.Entity;
    }

    /// <inheritdoc />
    public async Task<Inventory> Update(int id, InventoryDto inventoryDto)
    {
        var inventory = await ctx.Inventories.FindAsync(id);
        if (inventory == null)
        {
            throw new NotFoundException($"Inventory with id: {id} not found");
        }

        if (inventoryDto.Quantity >= 0 && inventoryDto.Quantity != inventory.Quantity)
        {
            inventory.Quantity = inventoryDto.Quantity;
        }

        if (inventoryDto.ProductId != inventory.ProductId)
        {
            var product = await ctx.Products.FindAsync(inventoryDto.ProductId);
            if (product == null)
            {
                throw new NotFoundException($"Product with id: {inventoryDto.ProductId} not found");
            }

            inventory.Product = product;
            inventory.ProductId = product.Id;
        }

        inventory.UpdatedAt = DateTime.UtcNow;
        await ctx.SaveChangesAsync();

        return inventory;
    }

    /// <inheritdoc />
    public async Task<Inventory> FindById(int id)
    {
        var cacheKey = $"inventory:{id}";
        if (cache.TryGetValue(cacheKey, out Inventory? inventory))
            if (inventory != null)
                return inventory;

        inventory = await ctx.Inventories.FindAsync(id);

        var cacheOption = new MemoryCacheEntryOptions()
            .SetSlidingExpiration(TimeSpan.FromMinutes(10))
            .SetAbsoluteExpiration(TimeSpan.FromMinutes(20));

        cache.Set(cacheKey, inventory, cacheOption);

        return inventory ?? throw new NotFoundException($"Inventory with id: {id} not found");
    }

    /// <inheritdoc />
    public async Task<string> Delete(int id)
    {
        var inventory = await ctx.Inventories.FindAsync(id);
        if (inventory == null)
        {
            throw new NotFoundException($"Inventory with id: {id} not found");
        }

        cache.Remove($"inventory:{id}");
        ctx.Inventories.Remove(inventory);
        await ctx.SaveChangesAsync();

        return "Inventory deleted successfully";
    }
}
