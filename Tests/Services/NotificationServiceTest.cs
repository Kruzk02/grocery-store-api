using Application.Services.impl;

using Domain.Entity;
using Domain.Exception;

using Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;

namespace Tests.Services;

[TestFixture]
public class NotificationServiceTest
{
    private NotificationService _notificationService;
    private ApplicationDbContext _dbContext;

    [SetUp]
    public void Setup()
    {
        _dbContext = GetInMemoryDbContext();
        _notificationService = new NotificationService(_dbContext);
    }

    [TearDown]
    public void TearDown()
    {
        _dbContext.Dispose();

    }

    private static ApplicationDbContext GetInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    [Test]
    [TestCaseSource(nameof(CreateNotification))]
    public async Task CreateNotification_shouldCreate(Notification notification)
    {
        var result = await _notificationService.Create(notification);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Id, Is.EqualTo(notification.Id));
            Assert.That(result.Message, Is.EqualTo(notification.Message));
            Assert.That(result.IsRead, Is.False);
            Assert.That(result.Type, Is.EqualTo(NotificationType.Info));
        }
    }

    [Test]
    [TestCaseSource(nameof(CreateNotification))]
    public async Task FindByUserId_shouldReturnNotification(Notification notification)
    {
        var user = new ApplicationUser { Id = "1a" };
        _dbContext.Users.Add(user);

        _dbContext.Notifications.Add(notification);

        await _dbContext.SaveChangesAsync();

        var result = await _notificationService.FindByUserId(notification.UserId);

        Assert.That(result, Has.Count.EqualTo(1));
    }

    [Test]
    [TestCaseSource(nameof(CreateNotification))]
    public async Task DeleteById_shouldDelete(Notification notification)
    {
        await _notificationService.Create(notification);

        var serviceResult = await _notificationService.DeleteById(notification.Id);

        Assert.That(serviceResult, Is.EqualTo("Notification Deleted Successfully"));
    }

    [Test]
    [TestCase(1)]
    [TestCase(2)]
    [TestCase(3)]
    [TestCase(4)]
    public Task DeleteById_ShouldThrowNotFoundException(int id)
    {
        var ex = Assert.ThrowsAsync<NotFoundException>(async () =>
            await _notificationService.DeleteById(id));

        Assert.That(ex.Message, Is.EqualTo($"Notification with id {id} not found"));
        return Task.CompletedTask;
    }

    [Test]
    [TestCaseSource(nameof(CreateNotification))]
    public async Task MarkAsRead_shouldMarkAsRead(Notification notification)
    {
        await _notificationService.Create(notification);

        var result = await _notificationService.MarkAsRead(notification.Id);
        Assert.That(result.IsRead, Is.True);
    }

    [Test]
    [TestCase(1)]
    [TestCase(2)]
    [TestCase(3)]
    [TestCase(4)]
    public Task MarkAsRead_ShouldThrowNotFoundException(int id)
    {
        var ex = Assert.ThrowsAsync<NotFoundException>(async () =>
            await _notificationService.MarkAsRead(id));

        Assert.That(ex.Message, Is.EqualTo($"Notification with id {id} not found"));
        return Task.CompletedTask;
    }

    [Test]
    [TestCaseSource(nameof(CreateNotification))]
    public async Task MarkAllAsRead_shouldMarkAllAsRead(Notification notification)
    {
        await _notificationService.Create(notification);

        var result = await _notificationService.MarkAllAsRead("1a");
        Assert.That(result, !Is.Empty);
    }

    private static IEnumerable<Notification> CreateNotification()
    {
        yield return new Notification
        {
            Id = 1,
            Message = "asap",
            CreatedAt = DateTime.UtcNow,
            IsRead = false,
            Type = NotificationType.Info,
            UserId = "1a"
        };

        yield return new Notification
        {
            Id = 2,
            Message = "asap444",
            CreatedAt = DateTime.UtcNow,
            IsRead = false,
            Type = NotificationType.Info,
            UserId = "1a"
        };

        yield return new Notification
        {
            Id = 3,
            Message = "asap555",
            CreatedAt = DateTime.UtcNow,
            IsRead = false,
            Type = NotificationType.Info,
            UserId = "1a"
        };
    }
}
