using Application.Dtos.Request;

using Domain.Entity;

namespace Application.Services;

public interface IInvoiceService
{
    Task<Invoice> Create(InvoiceDto invoiceDto);
    Task<Invoice> FindById(int id);
    Task<Invoice> FindByOrderId(int orderId);
}
