using Application.Dtos.Request;
using Application.Interface;
using Application.Repository;

using Domain.Entity;
using Domain.Exception;

using Microsoft.Extensions.Caching.Memory;

namespace Application.Services;

/// <summary>
/// Provides operations for create, retrieve, update and delete inventory.
/// </summary>
/// <remarks>
/// This class interacts with database to performs CRUD operations relate to inventory.
/// </remarks>
/// <param name="ctx">The <see cref="ApplicationDbContext"/> used to access the database</param>
public class InventoryService(IInventoryRepository inventoryRepository, IProductRepository productRepository, IMemoryCache cache) : IInventoryService
{
    /// <inheritdoc />
    public async Task<List<Inventory>> FindAll(int? productId, int? stock, int skip, int take)
    {
        var cacheKey = $"inventories:{productId}:{stock}:{skip}:{take}";
        return await cache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.SetSlidingExpiration(TimeSpan.FromMinutes(10));
            entry.SetAbsoluteExpiration(TimeSpan.FromMinutes(20));

            return await inventoryRepository.FindAll(productId, stock, skip, take);
        }) ?? [];
    }

    /// <inheritdoc />
    public async Task<Inventory> Create(InventoryDto inventoryDto)
    {
        Product? product = await productRepository.FindById(inventoryDto.ProductId) ?? throw new NotFoundException($"Product with id: {inventoryDto.ProductId} not found");
        var inventory = new Inventory
        {
            Product = product,
            ProductId = product.Id,
            Stock = inventoryDto.Stock,
            UpdatedAt = DateTime.UtcNow
        };

        return await inventoryRepository.Add(inventory);
    }

    /// <inheritdoc />
    public async Task<Inventory> Update(int id, InventoryDto inventoryDto)
    {
        Inventory? inventory = await inventoryRepository.FindById(id) ?? throw new NotFoundException($"Inventory with id: {id} not found");
        if (inventoryDto.Stock >= 0 && inventoryDto.Stock != inventory.Stock)
        {
            inventory.Stock = inventoryDto.Stock;
        }

        if (inventoryDto.ProductId != inventory.ProductId)
        {
            Product? product = await productRepository.FindById(inventoryDto.ProductId) ?? throw new NotFoundException($"Product with id: {inventoryDto.ProductId} not found");
            inventory.Product = product;
            inventory.ProductId = product.Id;
        }

        inventory.UpdatedAt = DateTime.UtcNow;

        await inventoryRepository.Update(inventory);
        return inventory;
    }

    /// <inheritdoc />
    public async Task<Inventory> FindById(int id)
    {
        var cacheKey = $"inventory:{id}";
        if (cache.TryGetValue(cacheKey, out Inventory? inventory))
        {
            if (inventory != null)
            {
                return inventory;
            }
        }

        inventory = await inventoryRepository.FindById(id);

        MemoryCacheEntryOptions cacheOption = new MemoryCacheEntryOptions()
            .SetSlidingExpiration(TimeSpan.FromMinutes(10))
            .SetAbsoluteExpiration(TimeSpan.FromMinutes(20));

        _ = cache.Set(cacheKey, inventory, cacheOption);

        return inventory ?? throw new NotFoundException($"Inventory with id: {id} not found");
    }

    /// <inheritdoc />
    public async Task<string> Delete(int id)
    {
        Inventory? inventory = await inventoryRepository.FindById(id) ?? throw new NotFoundException($"Inventory with id: {id} not found");
        cache.Remove($"inventory:{id}");
        await inventoryRepository.Delete(inventory);
        return "Inventory deleted successfully";
    }
}
