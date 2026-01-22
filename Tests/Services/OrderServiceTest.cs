using Application.Dtos.Request;
using Application.Interface;
using Application.Services;

using Domain.Entity;
using Domain.Exception;

using Infrastructure.Persistence;
using Infrastructure.Repository;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace Tests.Services;

[TestFixture]
public class OrderServiceTest
{

    private IOrderService _orderService;
    private ApplicationDbContext _dbContext;

    [SetUp]
    public void SetUp()
    {
        _dbContext = GetInMemoryDbContext();
        _orderService = new OrderService(new OrderRepository(_dbContext), new CustomerRepository(_dbContext), new MemoryCache(new MemoryCacheOptions()));
    }

    [TearDown]
    public void Destroy()
    {
        _dbContext.Dispose();
    }

    [Test]
    [TestCaseSource(nameof(CreateCustomer))]
    public async Task Create(Customer customer)
    {
        _dbContext.Customers.Add(customer);
        await _dbContext.SaveChangesAsync();

        var result = await _orderService.Create(new OrderDto(customer.Id));

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Id, Is.GreaterThan(0));
            Assert.That(result.CustomerId, Is.GreaterThan(0));
        }
    }

    [Test]
    [TestCaseSource(nameof(CreateCustomer))]
    public Task Create_ShouldThrowNotFoundException(Customer customer)
    {
        var ex = Assert.ThrowsAsync<NotFoundException>(async () =>
            await _orderService.Create(new OrderDto(customer.Id)));

        Assert.That(ex.Message, Is.EqualTo($"Customer with id {customer.Id} not found"));
        return Task.CompletedTask;
    }

    [Test]
    public async Task Update()
    {
        var customer1 = new Customer { Name = "Name", Email = "Email@gmail.com", Phone = "84 123 456 78", Address = "2aad3" };
        _dbContext.Customers.Add(customer1);
        var customer2 = new Customer { Name = "Name123", Email = "Email123@gmail.com", Phone = "84 123 456 78", Address = "2aad3" };
        _dbContext.Customers.Add(customer1);
        await _dbContext.SaveChangesAsync();

        await _orderService.Create(new OrderDto(customer1.Id));
        var result = await _orderService.Update(1, new OrderDto(customer2.Id));

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result, !Is.Null);
            Assert.That(result.Id, Is.EqualTo(1));
            Assert.That(result.CustomerId, Is.EqualTo(1));
        }
    }

    [Test]
    [TestCaseSource(nameof(CreateCustomer))]
    public Task Update_ShouldThrowNotFoundException(Customer customer)
    {
        var ex = Assert.ThrowsAsync<NotFoundException>(async () =>
            await _orderService.Update(1, new OrderDto(customer.Id)));

        Assert.That(ex.Message, Is.EqualTo($"Order with id 1 not found"));
        return Task.CompletedTask;
    }

    [Test]
    [TestCaseSource(nameof(CreateCustomer))]
    public async Task FindById(Customer customer)
    {
        var orderItemService = new OrderItemService(new OrderItemRepository(_dbContext), new OrderRepository(_dbContext), new ProductRepository(_dbContext), new MemoryCache(new MemoryCacheOptions()));

        var order = new Order
        {
            Id = 1,
            CustomerId = 1,
            CreatedAt = DateTime.UtcNow,
            Customer = customer,
        };

        var product = new Product
        {
            Id = 1,
            Name = "name",
            Description = "description",
            Price = 19.99m,
            CategoryId = 1,
            Quantity = 20,
            CreatedAt = DateTime.UtcNow,
            Category = new Category { Id = 1, Name = "Fresh Produce", Description = "Fruits, vegetables, herbs" }
        };

        _dbContext.Orders.Add(order);
        _dbContext.Products.Add(product);
        _dbContext.Customers.Add(customer);
        await _dbContext.SaveChangesAsync();
        await orderItemService.Create(new OrderItemDto(order.Id, product.Id, 20));
        await _orderService.Create(new OrderDto(customer.Id));

        var result = await _orderService.FindById(1);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Id, Is.GreaterThan(0));
            Assert.That(result.CustomerId, Is.GreaterThan(0));
            Assert.That(result.Total, Is.EqualTo(399.8m));
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
            await _orderService.FindById(id));

        Assert.That(ex.Message, Is.EqualTo($"Order with id {id} not found"));
        return Task.CompletedTask;
    }

    [Test]
    [TestCaseSource(nameof(CreateCustomer))]
    public async Task FindByCustomerId(Customer customer)
    {
        _dbContext.Customers.Add(customer);
        await _dbContext.SaveChangesAsync();
        await _orderService.Create(new OrderDto(customer.Id));

        var result = await _orderService.FindByCustomerId(customer.Id);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result, Is.Not.Empty);
            Assert.That(result[0].Id, Is.GreaterThan(0));
            Assert.That(result[0].CustomerId, Is.GreaterThan(0));
        }
    }

    [Test]
    [TestCaseSource(nameof(CreateCustomer))]
    public async Task Delete(Customer customer)
    {
        _dbContext.Customers.Add(customer);
        await _dbContext.SaveChangesAsync();
        await _orderService.Create(new OrderDto(customer.Id));

        var result = await _orderService.Delete(1);
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
            await _orderService.Delete(id));

        Assert.That(ex.Message, Is.EqualTo($"Order with id {id} not found"));
        return Task.CompletedTask;
    }

    private static IEnumerable<Customer> CreateCustomer()
    {
        yield return new Customer { Name = "Name", Email = "Email@gmail.com", Phone = "84 123 456 78", Address = "2aad3" };
    }

    private static ApplicationDbContext GetInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }
}
