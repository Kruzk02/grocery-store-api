using Application.Dtos.Request;
using Application.Interface;
using Application.Repository;

using Domain.Entity;
using Domain.Exception;


namespace Application.Services;

public class InvoiceService(IInvoiceRepository invoiceRepository, IOrderRepository orderRepository) : IInvoiceService
{
    public async Task<Invoice> Create(InvoiceDto invoiceDto)
    {
        var order = await orderRepository.FindById(invoiceDto.OrderId);
        if (order == null) throw new NotFoundException($"Order with id: {invoiceDto.OrderId} not found");

        var invoice = new Invoice
        {
            OrderId = order.Id,
            Order = order,
            IssueDate = DateTime.UtcNow,
            DueDate = DateTime.UtcNow.AddDays(30),
            InvoiceNumber = $"INV-{DateTime.UtcNow.Year}:{order.Id:D4}"
        };

        return await invoiceRepository.Add(invoice);
    }

    public async Task<Invoice> FindById(int id)
    {
        var invoice = await invoiceRepository.FindById(id);
        return invoice ?? throw new NotFoundException($"Invoice with id: {id} not found");
    }

    public async Task<Invoice> FindByOrderId(int orderId)
    {
        var invoice = await invoiceRepository.FindByOrderId(orderId);
        return invoice ?? throw new NotFoundException($"Invoice with order id: {orderId} not found");
    }
}
