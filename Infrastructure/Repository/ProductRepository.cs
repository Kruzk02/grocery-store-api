using Application.Repository;

using Domain.Entity;

using Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repository;

public class ProductRepository(ApplicationDbContext ctx) : IProductRepository
{
    public async Task<(int total, List<Product>)> Search(string? name, int skip, int take)
    {
        var query = ctx.Products.AsQueryable();
        if (!string.IsNullOrEmpty(name))
        {
            query = query.Where(p => EF.Functions.Like(p.Name.ToLower(), $"%{name.ToLower()}%"));
        }
        var total = await query.CountAsync();
        var data = await query.Skip(skip).Take(take).ToListAsync();
        return (total, data);
    }

    public async Task<Product> Add(Product product)
    {
        var result = await ctx.Products.AddAsync(product);
        await ctx.SaveChangesAsync();
        return result.Entity;
    }

    public async Task Update(Product product)
    {
        ctx.Products.Update(product);
        await ctx.SaveChangesAsync();
    }

    public async Task<Product?> FindById(int Id)
    {
        return await ctx.Products.FindAsync(Id);
    }

    public async Task Delete(Product product)
    {
        ctx.Products.Remove(product);
        await ctx.SaveChangesAsync();
    }
}
