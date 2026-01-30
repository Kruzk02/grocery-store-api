
using Application.Repository;

using Domain.Entity;

using Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repository;

public class InvoiceRepository(ApplicationDbContext ctx) : IInvoiceRepository
{
    public async Task<Invoice> Add(Invoice invoice)
    {
        var result = await ctx.Invoices.AddAsync(invoice);
        await ctx.SaveChangesAsync();
        return result.Entity;
    }

    public async Task<Invoice?> FindById(int id)
    {
        return await ctx.Invoices
            .Include(i => i.Order)
                .ThenInclude(o => o.Customer)
            .Include(i => i.Order)
                .ThenInclude(o => o.Items)
                    .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(i => i.Id == id);
    }

    public async Task<Invoice?> FindByOrderId(int orderId)
    {
        return await ctx.Invoices
            .Include(i => i.Order)
                .ThenInclude(o => o.Customer)
            .Include(i => i.Order)
                .ThenInclude(o => o.Items)
                    .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(i => i.OrderId == orderId);
    }
}
