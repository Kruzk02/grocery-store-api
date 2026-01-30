using Domain.Entity;

namespace Application.Repository;

public interface INotificationRepository
{
    Task<Notification> Add(Notification notification);
    Task Add(Notification notification, CancellationToken stoppingToken);
    Task<List<Notification>> FindByUserId(string userId);
    Task<Notification?> FindById(int id);
    Task Delete(Notification notification);
    Task<Notification> MarkAsRead(Notification notification);
    Task<List<Notification>> MarkAllAsRead(string userId);
}
