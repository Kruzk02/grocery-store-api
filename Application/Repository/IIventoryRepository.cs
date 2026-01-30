
using Domain.Entity;

namespace Application.Repository;

public interface IInventoryRepository
{
    Task<List<Inventory>> FindAll();
    Task<Inventory> Add(Inventory inventory);
    Task Update(Inventory inventory);
    Task<Inventory?> FindById(int Id);
    Task<Inventory?> FindLessThanTenQuantity(CancellationToken stoppingToken);
    Task Delete(Inventory inventory);
}
