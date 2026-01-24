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
    public async Task<List<Inventory>> FindAll()
    {
        const string cacheKey = $"inventories";
        if (cache.TryGetValue(cacheKey, out List<Inventory>? inventories))
            if (inventories != null)
                return inventories;

        inventories = await inventoryRepository.FindAll();

        var cacheOption = new MemoryCacheEntryOptions()
            .SetSlidingExpiration(TimeSpan.FromMinutes(10))
            .SetAbsoluteExpiration(TimeSpan.FromMinutes(20));

        cache.Set(cacheKey, inventories, cacheOption);

        return inventories;
    }

    /// <inheritdoc />
    public async Task<Inventory> Create(InventoryDto inventoryDto)
    {
        var product = await productRepository.FindById(inventoryDto.ProductId);
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

        return await inventoryRepository.Add(inventory);
    }

    /// <inheritdoc />
    public async Task<Inventory> Update(int id, InventoryDto inventoryDto)
    {
        var inventory = await inventoryRepository.FindById(id);
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
            var product = await productRepository.FindById(inventoryDto.ProductId);
            if (product == null)
            {
                throw new NotFoundException($"Product with id: {inventoryDto.ProductId} not found");
            }

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
            if (inventory != null)
                return inventory;

        inventory = await inventoryRepository.FindById(id);

        var cacheOption = new MemoryCacheEntryOptions()
            .SetSlidingExpiration(TimeSpan.FromMinutes(10))
            .SetAbsoluteExpiration(TimeSpan.FromMinutes(20));

        cache.Set(cacheKey, inventory, cacheOption);

        return inventory ?? throw new NotFoundException($"Inventory with id: {id} not found");
    }

    /// <inheritdoc />
    public async Task<string> Delete(int id)
    {
        var inventory = await inventoryRepository.FindById(id);
        if (inventory == null)
        {
            throw new NotFoundException($"Inventory with id: {id} not found");
        }

        cache.Remove($"inventory:{id}");
        await inventoryRepository.Delete(inventory);
        return "Inventory deleted successfully";
    }
}
