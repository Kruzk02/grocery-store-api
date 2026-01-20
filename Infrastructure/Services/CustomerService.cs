using Application.Dtos.Request;
using Application.Services;

using Domain.Entity;
using Domain.Exception;

using Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace Infrastructure.Services;

public class CustomerService(ApplicationDbContext ctx, IMemoryCache cache) : ICustomerService
{
    public async Task<(int total, List<Customer> data)> SearchCustomers(string? name, int skip, int take)
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
        const string cacheKey = $"customers";
        if (cache.TryGetValue(cacheKey, out List<Customer>? customers))
            if (customers != null)
                return customers;

        customers = await ctx.Customers.ToListAsync();
        var cacheOption = new MemoryCacheEntryOptions()
            .SetSlidingExpiration(TimeSpan.FromMinutes(10))
            .SetAbsoluteExpiration(TimeSpan.FromMinutes(20));

        cache.Set(cacheKey, customers, cacheOption);

        return customers;
    }

    public async Task<Customer> Create(CustomerDto customerDto)
    {
        if (string.IsNullOrEmpty(customerDto.Name))
            throw new ValidationException(new Dictionary<string, string[]> { { "Name", ["Name is required"] } });
        if (string.IsNullOrEmpty(customerDto.Email))
            throw new ValidationException(new Dictionary<string, string[]> { { "Email", ["Email is required"] } });
        if (string.IsNullOrEmpty(customerDto.Phone))
            throw new ValidationException(new Dictionary<string, string[]> { { "Phone", ["Phone is required"] } });
        if (string.IsNullOrEmpty(customerDto.Address))
            throw new ValidationException(new Dictionary<string, string[]> { { "Address", ["Address is required"] } });

        var customer = new Customer
        {
            Name = customerDto.Name,
            Email = customerDto.Email,
            Phone = customerDto.Phone,
            Address = customerDto.Address
        };

        var result = await ctx.AddAsync(customer);
        await ctx.SaveChangesAsync();

        return result.Entity;
    }

    public async Task<string> Update(int id, CustomerDto customerDto)
    {
        var existingCustomer = await ctx.Customers.FindAsync(id);
        if (existingCustomer == null)
            throw new NotFoundException($"Customer with id: {id} not found");

        if (!string.IsNullOrEmpty(customerDto.Name) && customerDto.Name != existingCustomer.Name)
            existingCustomer.Name = customerDto.Name;
        if (!string.IsNullOrEmpty(customerDto.Email) && customerDto.Email != existingCustomer.Email)
            existingCustomer.Email = customerDto.Email;
        if (!string.IsNullOrEmpty(customerDto.Phone) && customerDto.Phone != existingCustomer.Phone)
            existingCustomer.Phone = customerDto.Phone;
        if (!string.IsNullOrEmpty(customerDto.Address) && customerDto.Address != existingCustomer.Address)
            existingCustomer.Address = customerDto.Address;

        existingCustomer.UpdatedAt = DateTime.UtcNow;
        await ctx.SaveChangesAsync();

        return "Customer updated successfully";
    }

    public async Task<Customer> FindById(int id)
    {
        var cacheKey = $"customer:{id}";

        if (cache.TryGetValue(cacheKey, out Customer? customer))
            if (customer != null)
                return customer;

        customer = await ctx.Customers.FindAsync(id);
        var cacheOption = new MemoryCacheEntryOptions()
            .SetSlidingExpiration(TimeSpan.FromMinutes(10))
            .SetAbsoluteExpiration(TimeSpan.FromMinutes(20));

        cache.Set(cacheKey, customer, cacheOption);
        return customer ?? throw new NotFoundException($"Customer with id: {id} not found");
    }

    public async Task<Customer> FindByEmail(string email)
    {
        var cacheKey = $"customer:email:{email}";
        if (cache.TryGetValue(cacheKey, out Customer? customer))
            if (customer != null)
                return customer;

        customer = await ctx.Customers.Where(c => c.Email == email).FirstOrDefaultAsync();
        var cacheOption = new MemoryCacheEntryOptions()
            .SetSlidingExpiration(TimeSpan.FromMinutes(10))
            .SetAbsoluteExpiration(TimeSpan.FromMinutes(20));

        cache.Set(cacheKey, customer, cacheOption);

        return customer ?? throw new NotFoundException($"Customer with email: {email} not found");
    }

    public async Task<Customer> FindByName(string name)
    {
        var cacheKey = $"customer:name:{name}";
        if (cache.TryGetValue(cacheKey, out Customer? customer))
            if (customer != null)
                return customer;

        customer = await ctx.Customers.Where(c => c.Name == name).FirstOrDefaultAsync();
        var cacheOption = new MemoryCacheEntryOptions()
            .SetSlidingExpiration(TimeSpan.FromMinutes(10))
            .SetAbsoluteExpiration(TimeSpan.FromMinutes(20));

        cache.Set(cacheKey, customer, cacheOption);
        return customer ?? throw new NotFoundException($"Customer with name: {name} not found");
    }

    public async Task<Customer> FindByPhoneNumber(string phoneNumber)
    {
        var cacheKey = $"customer:phone:{phoneNumber}";
        if (cache.TryGetValue(cacheKey, out Customer? customer))
            if (customer != null)
                return customer;

        customer = await ctx.Customers.Where(c => c.Phone == phoneNumber).FirstOrDefaultAsync();
        var cacheOption = new MemoryCacheEntryOptions()
            .SetSlidingExpiration(TimeSpan.FromMinutes(10))
            .SetAbsoluteExpiration(TimeSpan.FromMinutes(20));

        cache.Set(cacheKey, customer, cacheOption);
        return customer ?? throw new NotFoundException($"Customer with number: {phoneNumber} not found");
    }

    public async Task<string> DeleteById(int id)
    {
        var customer = await ctx.Customers.FindAsync(id);
        if (customer == null)
        {
            throw new NotFoundException($"Customer with id: {id} not found");
        }

        cache.Remove($"customer:{id}");
        ctx.Customers.Remove(customer);
        await ctx.SaveChangesAsync();

        return "Customer deleted successfully";
    }
}
