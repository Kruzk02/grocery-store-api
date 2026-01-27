using Domain.Entity;

namespace Application.Interface;

/// <summary>
/// Defines operations for managing notifications.
/// </summary>
public interface INotificationService
{
    /// <summary>
    /// Asynchronously create a new notification in the database.
    /// </summary>
    /// <param name="notification">The <see cref="Notification"/> that provides notification data.</param>
    Task<Notification> Create(Notification notification);
    /// <summary>
    /// Asynchronously retrieves a notification by user identifier from the database.
    /// </summary>
    /// <param name="userId">The identifier of the user to retrieve</param>
    Task<List<Notification>> FindByUserId(string userId);
    /// <summary>
    /// Asynchronously deletes a notification by its identifier from the database.
    /// </summary>
    /// <param name="id">The identifier of the notification to delete.</param>
    Task<string> DeleteById(int id);
    /// <summary>
    /// Asynchronously mark a notification is read by its identifier from the database.
    /// </summary>
    /// <param name="id">The identifier of the notification to mark as read.</param>
    Task<Notification> MarkAsRead(int id);
    /// <summary>
    /// Asynchronously mark all notifications is read by its identifier from the database.
    /// </summary>
    /// <param name="userId">The identifier of the user own notifications to mark all read.</param>
    Task<List<Notification>> MarkAllAsRead(string userId);
}
