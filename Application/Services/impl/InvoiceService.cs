using Application.Dtos;
using Application.Dtos.Request;
using Domain.Entity;
using Domain.Exception;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Application.Services.impl;

public class InvoiceService(ApplicationDbContext ctx) : IInvoiceService
{
    public async Task<Invoice> Create(InvoiceDto invoiceDto)
    {
        var order = await ctx.Orders.FindAsync(invoiceDto.OrderId);
        if (order == null) throw new NotFoundException($"Order with id: {invoiceDto.OrderId} not found");

        var invoice = new Invoice
        {
            OrderId = order.Id,
            Order = order,
            IssueDate = DateTime.UtcNow,
            DueDate = DateTime.UtcNow.AddDays(30),
            InvoiceNumber = $"INV-{DateTime.UtcNow.Year}:{order.Id:D4}"
        };

        var result = await ctx.Invoices.AddAsync(invoice);
        await ctx.SaveChangesAsync();

        return result.Entity;
    }

    public async Task<Invoice> FindById(int id)
    {
        var invoice = await ctx.Invoices
            .Include(i => i.Order)
                .ThenInclude(o => o.Customer)
            .Include(i => i.Order)
                .ThenInclude(o => o.Items)
                    .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(i => i.Id == id);

        return invoice ?? throw new NotFoundException($"Invoice with id: {id} not found");
    }

    public async Task<Invoice> FindByOrderId(int orderId)
    {
        var invoice = await ctx.Invoices
            .Include(i => i.Order)
                .ThenInclude(o => o.Customer)
            .Include(i => i.Order)
                .ThenInclude(o => o.Items)
                    .ThenInclude(oi => oi.Product)
            .FirstOrDefaultAsync(i => i.OrderId == orderId);

        return invoice ?? throw new NotFoundException($"Invoice with order id: {orderId} not found");
    }
}