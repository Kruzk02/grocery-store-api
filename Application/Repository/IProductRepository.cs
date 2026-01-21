using Domain.Entity;

namespace Application.Repository;

public interface IProductRepository
{
    Task<(int total, List<Product>)> Search(string? name, int skip, int take);
    Task<Product> Add(Product product);
    Task Update(Product product);
    Task<Product?> FindById(int Id);
    Task Delete(Product product);
}
