
using Domain.Entity;

namespace Application.Repository;

public interface IInvoiceRepository
{
    Task<Invoice> Add(Invoice invoice);
    Task<Invoice?> FindById(int id);
    Task<Invoice?> FindByOrderId(int orderId);
}
