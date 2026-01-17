using Application.Dtos;
using Application.Dtos.Request;
using Application.Services.impl;
using Domain.Entity;
using Domain.Exception;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using NUnit.Framework.Internal;

namespace Tests.Services;

[TestFixture]
public class OrderItemServiceTest
{

    private OrderItemService _orderItemService;
    private ApplicationDbContext _dbContext;

    [SetUp]
    public void Setup()
    {
        _dbContext = GetInMemoryDbContext();
        _orderItemService = new OrderItemService(_dbContext, new MemoryCache(new MemoryCacheOptions()));
    }

    [TearDown]
    public void TearDown()
    {
        _dbContext.Dispose();
    }

    private static void CreateProductAndOrder(ApplicationDbContext ctx)
    {
        var order = new Order
        {
            Id = 1,
            CustomerId = 1,
            CreatedAt = DateTime.UtcNow,
            Customer = new Customer
            {
                Name = "Name",
                Email = "Email@gmail.com",
                Phone = "841231245",
                Address = "asap"
            }
        };

        var product = new Product
        {
            Id = 1,
            Name = "name",
            Description = "description",
            Price = 19.99m,
            CategoryId = 1,
            Quantity = 25,
            CreatedAt = DateTime.UtcNow,
            Category = new Category { Id = 1, Name = "Fresh Produce", Description = "Fruits, vegetables, herbs" }
        };

        ctx.Orders.Add(order);
        ctx.Products.Add(product);
    }

    [Test]
    [TestCaseSource(nameof(CreateOrderItemsDto))]
    public async Task CreateOrderItem(OrderItemDto orderItemDto)
    {
        CreateProductAndOrder(_dbContext);
        await _dbContext.SaveChangesAsync();

        var result = await _orderItemService.Create(orderItemDto);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Id, Is.EqualTo(1));
            Assert.That(result.ProductId, Is.EqualTo(1));
            Assert.That(result.OrderId, Is.EqualTo(1));
            Assert.That(result.Quantity, Is.EqualTo(24));
            Assert.That(result.Product.Quantity, Is.EqualTo(1));
        }
    }

    [Test]
    [TestCaseSource(nameof(CreateOrderItemsDto))]
    public Task CreateOrderItem_ShouldThrowNotFoundException(OrderItemDto orderItemDto)
    {
        var ex = Assert.ThrowsAsync<NotFoundException>(async () =>
            await _orderItemService.Create(orderItemDto));

        Assert.That(ex.Message, Does.Not.Empty);
        return Task.CompletedTask;
    }

    [Test]
    [TestCaseSource(nameof(CreateOrderItemsDto))]
    public async Task Update(OrderItemDto orderItemDto)
    {
        CreateProductAndOrder(_dbContext);

        await _dbContext.SaveChangesAsync();
        await _orderItemService.Create(orderItemDto);
        var result = await _orderItemService.Update(1, new OrderItemDto(1, 1, 2));

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Id, Is.EqualTo(1));
            Assert.That(result.ProductId, Is.EqualTo(1));
            Assert.That(result.OrderId, Is.EqualTo(1));
            Assert.That(result.Quantity, Is.EqualTo(2));
            Assert.That(result.Product.Quantity, Is.EqualTo(23));
        }

    }

    [Test]
    [TestCaseSource(nameof(CreateOrderItemsDto))]
    public Task Update_ShouldThrowNotFoundException(OrderItemDto orderItemDto)
    {
        var ex = Assert.ThrowsAsync<NotFoundException>(async () =>
            await _orderItemService.Update(1, orderItemDto));

        Assert.That(ex.Message, Does.Not.Empty);
        return Task.CompletedTask;
    }

    [Test]
    [TestCaseSource(nameof(CreateOrderItemsDto))]
    public async Task FindById(OrderItemDto orderItemDto)
    {
        CreateProductAndOrder(_dbContext);
        await _dbContext.SaveChangesAsync();
        await _orderItemService.Create(orderItemDto);
        var result = await _orderItemService.FindById(orderItemDto.OrderId);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Id, Is.EqualTo(1));
            Assert.That(result.ProductId, Is.EqualTo(1));
            Assert.That(result.OrderId, Is.EqualTo(1));
            Assert.That(result.SubTotal, Is.EqualTo(479.76m));
            Assert.That(result.Quantity, Is.EqualTo(24));
        }
    }

    [Test]
    [TestCase(1)]
    [TestCase(2)]
    [TestCase(3)]
    [TestCase(4)]
    public Task FindById_ShouldThrowNotFoundException(int id)
    {
        var ex = Assert.ThrowsAsync<NotFoundException>(async () =>
            await _orderItemService.FindById(id));

        Assert.That(ex.Message, Is.EqualTo($"Order item with id {id} not found"));
        return Task.CompletedTask;
    }

    [Test]
    [TestCaseSource(nameof(CreateOrderItemsDto))]
    public async Task FindByOrderId(OrderItemDto orderItemDto)
    {
        CreateProductAndOrder(_dbContext);
        await _dbContext.SaveChangesAsync();
        var orderItem = await _orderItemService.Create(orderItemDto);
        var result = await _orderItemService.FindByOrderId(orderItemDto.OrderId);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result, !Is.Empty);
            Assert.That(result, Has.Count.EqualTo(1));
        }
    }

    [Test]
    [TestCaseSource(nameof(CreateOrderItemsDto))]
    public async Task FindByProductId(OrderItemDto orderItemDto)
    {
        CreateProductAndOrder(_dbContext);
        await _dbContext.SaveChangesAsync();
        var orderItem = await _orderItemService.Create(orderItemDto);
        var result = await _orderItemService.FindByProductId(orderItemDto.ProductId);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result, !Is.Empty);
            Assert.That(result, Has.Count.EqualTo(1));
        }
    }

    [Test]
    [TestCaseSource(nameof(CreateOrderItemsDto))]
    public async Task Delete(OrderItemDto orderItemDto)
    {
        CreateProductAndOrder(_dbContext);
        await _dbContext.SaveChangesAsync();
        await _orderItemService.Create(orderItemDto);
        var result = await _orderItemService.Delete(orderItemDto.OrderId);
        Assert.That(result, Is.True);
    }

    [Test]
    [TestCase(1)]
    [TestCase(2)]
    [TestCase(3)]
    [TestCase(4)]
    public Task Delete_ShouldThrowNotFoundException(int id)
    {
        var ex = Assert.ThrowsAsync<NotFoundException>(async () =>
            await _orderItemService.Delete(id));

        Assert.That(ex.Message, Is.EqualTo($"Order item with id: {id} not found"));
        return Task.CompletedTask;
    }

    private static IEnumerable<OrderItemDto> CreateOrderItemsDto()
    {
        yield return new OrderItemDto(1, 1, 24);
    }

    private static ApplicationDbContext GetInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }
}