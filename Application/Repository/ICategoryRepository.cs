using Domain.Entity;

namespace Application.Repository;

public interface ICategoryRepository
{
    Task<List<Category>> FindAll();
    Task<Category?> FindById(int Id);
}
