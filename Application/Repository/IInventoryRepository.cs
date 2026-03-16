using Domain.Entity;

namespace Application.Repository;

public interface IInventoryRepository
{
    Task<List<Inventory>> FindAll(int? productId, int? stock, int skip, int take);
    Task<Inventory> Add(Inventory inventory);
    Task Update(Inventory inventory);
    Task<Inventory?> FindById(int id);
    Task<Inventory?> FindLessThanTenQuantity(CancellationToken stoppingToken);
    Task Delete(Inventory inventory);
}
