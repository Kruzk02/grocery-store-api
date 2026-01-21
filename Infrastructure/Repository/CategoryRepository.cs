using Application.Repository;

using Domain.Entity;

using Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repository;

public class CategoryRepository(ApplicationDbContext ctx) : ICategoryRepository
{

    public async Task<List<Category>> FindAll()
    {
        return await ctx.Categories.ToListAsync();
    }
}
