
using Domain.Entity;

namespace Application.Repository;

public interface IInventoryRepository
{
    Task<List<Inventory>> FindAll();
    Task<Inventory> Add(Inventory inventory);
    Task Update(Inventory inventory);
    Task<Inventory?> FindById(int Id);
    Task<List<Inventory>> FindByProductId(int ProductId);
    Task<List<Inventory>> FindByQuantity(int Quantity);
    Task<Inventory?> FindLessThanTenQuantity(CancellationToken stoppingToken);
    Task Delete(Inventory inventory);
}
