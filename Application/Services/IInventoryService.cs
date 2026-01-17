using Application.Dtos.Request;

using Domain.Entity;

namespace Application.Services;

/// <summary>
/// Defines operations for managing inventories.
/// </summary>
public interface IInventoryService
{
    /// <summary>
    /// Asynchronously retrieve all inventory from the database.
    /// </summary>
    Task<List<Inventory>> FindAll();
    /// <summary>
    /// Asynchronously creates a new inventory in the database.
    /// </summary>
    /// <param name="inventoryDto">The <see cref="InventoryDto"/> that provide inventory data.</param>
    Task<Inventory> Create(InventoryDto inventoryDto);
    /// <summary>
    /// Asynchronously updates an existing inventory in the database.
    /// </summary>
    /// <param name="id">The identifier of the inventory to update</param>
    /// <param name="inventoryDto">The <see cref="InventoryDto"/> that provides updated inventory data</param>
    Task<Inventory> Update(int id, InventoryDto inventoryDto);
    /// <summary>
    /// Asynchronously retrieves an inventory by its identifier from the database.
    /// </summary>
    /// <param name="id">The identifier of the inventory to retrieve</param>
    Task<Inventory> FindById(int id);
    /// <summary>
    /// Asynchronously deletes an inventory by its identifier from the database.
    /// </summary>
    /// <param name="id">The identifier of the inventory to delete.</param>
    Task<string> Delete(int id);
}
