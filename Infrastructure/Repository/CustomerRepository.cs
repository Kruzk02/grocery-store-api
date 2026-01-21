using Application.Repository;

using Domain.Entity;

using Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repository;

public class CustomerRepository(ApplicationDbContext ctx) : ICustomerRepository
{
    public async Task<(int total, List<Customer>)> Search(string? name, int skip, int take)
    {
        var query = ctx.Customers.AsQueryable();
        if (!string.IsNullOrEmpty(name))
        {
            query = query.Where(p => EF.Functions.Like(p.Name.ToLower(), $"%{name.ToLower()}%"));
        }
        var total = await query.CountAsync();
        var data = await query.Skip(skip).Take(take).ToListAsync();

        return (total, data);
    }

    public async Task<List<Customer>> FindAll()
    {
        return await ctx.Customers.ToListAsync();
    }

    public async Task<Customer> Add(Customer customer)
    {
        var result = await ctx.AddAsync(customer);
        await ctx.SaveChangesAsync();
        return result.Entity;
    }

    public async Task Update(Customer customer)
    {
        ctx.Customers.Update(customer);
        await ctx.SaveChangesAsync();
    }

    public async Task<Customer?> FindById(int id)
    {
        return await ctx.Customers.FindAsync(id);
    }

    public async Task<Customer?> FindByEmail(string email)
    {
        return await ctx.Customers.Where(c => c.Email == email).FirstOrDefaultAsync();
    }

    public async Task<Customer?> FindByName(string name)
    {
        return await ctx.Customers.Where(c => c.Name == name).FirstOrDefaultAsync();
    }

    public async Task<Customer?> FindByPhoneNumber(string phoneNumber)
    {
        return await ctx.Customers.Where(c => c.Phone == phoneNumber).FirstOrDefaultAsync();
    }

    public async Task Delete(Customer customer)
    {
        ctx.Customers.Remove(customer);
        await ctx.SaveChangesAsync();
    }

}
