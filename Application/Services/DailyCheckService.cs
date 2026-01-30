using Application.Repository;

using Domain.Entity;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Application.Services;

public class DailyCheckService(ILogger<DailyCheckService> logger, IInventoryRepository inventoryRepository, IUserRepository userRepository, INotificationRepository notificationRepository) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("DailyCheckService is running.");

        while (!stoppingToken.IsCancellationRequested)
        {
            var now = DateTime.Now;
            var scheduledTime = DateTime.Today.AddHours(8);

            if (now > scheduledTime)
            {
                scheduledTime = scheduledTime.AddDays(1);
            }

            var delay = scheduledTime - now;
            logger.LogInformation("Next check at {time}", scheduledTime);

            await Task.Delay(delay, stoppingToken);

            var inventory = await inventoryRepository.FindLessThanTenQuantity(stoppingToken);
            var adminUsers = await userRepository.GetUserByRole("Admin", stoppingToken);

            foreach (var adminUser in adminUsers)
            {
                if (adminUser.Id == null) continue;
                await notificationRepository.Add(new Notification
                {
                    UserId = adminUser.Id,
                    Message = $"The product quality currently less than 10: {inventory?.Product}",
                    Type = NotificationType.Info,
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow
                }, stoppingToken);
            }
        }
    }
}
