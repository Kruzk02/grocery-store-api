using Domain.Entity;

using Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services;

public class DailyCheckService(ILogger<DailyCheckService> logger, IServiceScopeFactory scopeFactory) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("DailyCheckService is running.");

        while (!stoppingToken.IsCancellationRequested)
        {
            var scope = scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var now = DateTime.Now;
            var scheduledTime = DateTime.Today.AddHours(8);

            if (now > scheduledTime)
            {
                scheduledTime = scheduledTime.AddDays(1);
            }

            var delay = scheduledTime - now;
            logger.LogInformation("Next check at {time}", scheduledTime);

            await Task.Delay(delay, stoppingToken);

            var inventory = await dbContext.Inventories.Where(i => i.Quantity <= 10).FirstOrDefaultAsync(stoppingToken);
            var adminUsers = await (from user in dbContext.Users
                                    join userRole in dbContext.UserRoles on user.Id equals userRole.UserId
                                    join role in dbContext.Roles on userRole.RoleId equals role.Id
                                    where role.Name == "Admin"
                                    select user)
                .FirstOrDefaultAsync(cancellationToken: stoppingToken);

            if (adminUsers?.Id == null) continue;
            dbContext.Notifications.Add(new Notification
            {
                UserId = adminUsers.Id,
                Message = "The product quality currently less than 10: " + inventory?.Product,
                Type = NotificationType.Info,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            });
            await dbContext.SaveChangesAsync(stoppingToken);
        }
    }
}
