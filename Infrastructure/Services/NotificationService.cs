using Application.Services;

using Domain.Entity;
using Domain.Exception;

using Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

/// <summary>
/// Provides operations for Create, Retrieve, Delete and mark as read notification.
/// </summary>
/// <remarks>
/// This class interacts with database to performs CRUD operations related to notification.
/// </remarks>
/// <param name="ctx">The <see cref="ApplicationDbContext"/> used to access the database.</param>
public class NotificationService(ApplicationDbContext ctx) : INotificationService
{
    /// <inheritdoc />
    public async Task<Notification> Create(Notification notification)
    {
        var result = await ctx.Notifications.AddAsync(notification);
        await ctx.SaveChangesAsync();

        return result.Entity;
    }

    /// <inheritdoc />
    public async Task<List<Notification>> FindByUserId(string userId)
    {
        return await ctx.Notifications
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<string> DeleteById(int id)
    {
        var notification = await ctx.Notifications.FindAsync(id);
        if (notification == null)
        {
            throw new NotFoundException($"Notification with id {id} not found");
        }

        ctx.Notifications.Remove(notification);
        await ctx.SaveChangesAsync();

        return "Notification Deleted Successfully";
    }

    /// <inheritdoc />
    public async Task<Notification> MarkAsRead(int id)
    {
        var notification = await ctx.Notifications.FindAsync(id);
        if (notification == null)
        {
            throw new NotFoundException($"Notification with id {id} not found");
        }

        notification.IsRead = true;
        await ctx.SaveChangesAsync();
        return notification;
    }

    /// <inheritdoc />
    public async Task<List<Notification>> MarkAllAsRead(string userId)
    {
        var notifications = await ctx.Notifications.Where(n => n.UserId == userId && !n.IsRead).ToListAsync();
        foreach (var n in notifications)
        {
            n.IsRead = true;
        }

        await ctx.SaveChangesAsync();
        return notifications;
    }
}
