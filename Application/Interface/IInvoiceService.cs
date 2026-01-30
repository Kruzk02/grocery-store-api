using Application.Dtos.Request;

using Domain.Entity;

namespace Application.Interface;

public interface IInvoiceService
{
    Task<Invoice> Create(InvoiceDto invoiceDto);
    Task<Invoice> FindById(int id);
    Task<Invoice> FindByOrderId(int orderId);
}
