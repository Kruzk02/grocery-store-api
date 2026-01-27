using Application.Interface;
using Application.Repository;

using Domain.Entity;
using Domain.Exception;

namespace Application.Services;

/// <summary>
/// Provides operations for Create, Retrieve, Delete and mark as read notification.
/// </summary>
/// <remarks>
/// This class interacts with database to performs CRUD operations related to notification.
/// </remarks>
/// <param name="ctx">The <see cref="ApplicationDbContext"/> used to access the database.</param>
public class NotificationService(INotificationRepository notificationRepository) : INotificationService
{
    /// <inheritdoc />
    public async Task<Notification> Create(Notification notification)
    {
        return await notificationRepository.Add(notification);
    }

    /// <inheritdoc />
    public async Task<List<Notification>> FindByUserId(string userId)
    {
        return await notificationRepository.FindByUserId(userId);
    }

    /// <inheritdoc />
    public async Task<string> DeleteById(int id)
    {
        var notification = await notificationRepository.FindById(id);
        if (notification == null)
        {
            throw new NotFoundException($"Notification with id {id} not found");
        }

        await notificationRepository.Delete(notification);
        return "Notification Deleted Successfully";
    }

    /// <inheritdoc />
    public async Task<Notification> MarkAsRead(int id)
    {
        var notification = await notificationRepository.FindById(id);
        if (notification == null)
        {
            throw new NotFoundException($"Notification with id {id} not found");
        }

        return await notificationRepository.MarkAsRead(notification);
    }

    /// <inheritdoc />
    public async Task<List<Notification>> MarkAllAsRead(string userId)
    {
        return await notificationRepository.MarkAllAsRead(userId);
    }
}
