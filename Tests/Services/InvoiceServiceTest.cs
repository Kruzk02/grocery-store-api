using Application.Dtos.Request;
using Application.Services.impl;

using Domain.Entity;
using Domain.Exception;

using Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;

namespace Tests.Services;

[TestFixture]
public class InvoiceServiceTest
{
    private InvoiceService _invoiceService;
    private ApplicationDbContext _db;

    [SetUp]
    public void Setup()
    {
        _db = GetInMemoryDbContext();
        _invoiceService = new InvoiceService(_db);
    }

    [TearDown]
    public void TearDown()
    {
        _db.Dispose();
    }

    [Test]
    public async Task Create()
    {
        var order = new Order
        {
            Id = 1,
            CustomerId = 1,
            CreatedAt = DateTime.UtcNow,
            Customer = new Customer { Name = "Name", Email = "Email@gmail.com", Phone = "841231245", Address = "asap" },
        };
        _db.Orders.Add(order);
        await _db.SaveChangesAsync();

        var result = await _invoiceService.Create(new InvoiceDto(1));

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Id, Is.GreaterThan(0));
            Assert.That(result.OrderId, Is.EqualTo(order.Id));
            Assert.That(result.Order, Is.Not.Null);
            Assert.That(result.InvoiceNumber, Is.EqualTo("INV-2026:0001"));
        }
    }

    [Test]
    public Task Create_ShouldThrowNotFoundException()
    {
        var ex = Assert.ThrowsAsync<NotFoundException>(async ()
            => await _invoiceService.Create(new InvoiceDto(1)));

        Assert.That(ex.Message, Is.EqualTo($"Order with id: 1 not found"));
        return Task.CompletedTask;
    }

    [Test]
    public async Task FindById()
    {
        var customer = new Customer
        {
            Name = "Name",
            Email = "Email@gmail.com",
            Phone = "841231245",
            Address = "asap"
        };
        _db.Customers.Add(customer);
        await _db.SaveChangesAsync();

        var product = new Product
        {
            Name = "Sample Product",
            Description = "Description",
            Price = 10m,
            Category = new Category { Id = 1, Name = "Fresh Produce", Description = "Fruits, vegetables, herbs" }
        };
        _db.Products.Add(product);
        await _db.SaveChangesAsync();

        var order = new Order
        {
            CustomerId = customer.Id,
            CreatedAt = DateTime.UtcNow,
            Items =
            [
                new OrderItem
                    {
                        ProductId = product.Id,
                        Quantity = 1,
                        Order = new Order
                        {
                            Customer = customer,
                            CreatedAt = DateTime.UtcNow,
                        },
                        Product = product
                    }
            ],
            Customer = new Customer { Name = "Name", Email = "Email@gmail.com", Phone = "841231245", Address = "asap" },
        };

        _db.Orders.Add(order);
        await _db.SaveChangesAsync();

        var invoice = new Invoice
        {
            OrderId = order.Id,
            InvoiceNumber = $"INV-{DateTime.UtcNow.Year}:{order.Id:D4}",
            Order = order
        };
        _db.Invoices.Add(invoice);
        await _db.SaveChangesAsync();

        var result = await _invoiceService.FindById(1);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Id, Is.GreaterThan(0));
            Assert.That(result.OrderId, Is.EqualTo(order.Id));
            Assert.That(result.Order, Is.Not.Null);
            Assert.That(result.InvoiceNumber, Is.EqualTo("INV-2026:0001"));
            Assert.That(result.Order.Items, Is.Not.Empty);
        }
    }

    [Test]
    public Task FindById_ShouldThrowNotFoundException()
    {
        var ex = Assert.ThrowsAsync<NotFoundException>(async ()
            => await _invoiceService.FindById(1));

        Assert.That(ex.Message, Does.Not.Empty);
        return Task.CompletedTask;
    }

    [Test]
    public async Task FindByOrderId()
    {
        var customer = new Customer
        {
            Name = "Name",
            Email = "Email@gmail.com",
            Phone = "841231245",
            Address = "asap"
        };
        _db.Customers.Add(customer);
        await _db.SaveChangesAsync();

        var product = new Product
        {
            Name = "Sample Product",
            Description = "Description",
            Price = 10m,
            Category = new Category { Id = 1, Name = "Fresh Produce", Description = "Fruits, vegetables, herbs" }
        };
        _db.Products.Add(product);
        await _db.SaveChangesAsync();

        var order = new Order
        {
            CustomerId = customer.Id,
            CreatedAt = DateTime.UtcNow,
            Items =
            [
                new OrderItem
                {
                    ProductId = product.Id,
                    Quantity = 1,
                    Order = new Order
                    {
                        Customer = customer,
                        CreatedAt = DateTime.UtcNow,
                    },
                    Product = product
                }
            ],
            Customer = customer
        };

        _db.Orders.Add(order);
        await _db.SaveChangesAsync();

        var invoice = new Invoice
        {
            OrderId = order.Id,
            InvoiceNumber = $"INV-{DateTime.UtcNow.Year}:{order.Id:D4}",
            Order = order
        };
        _db.Invoices.Add(invoice);
        await _db.SaveChangesAsync();

        var result = await _invoiceService.FindByOrderId(order.Id);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Id, Is.GreaterThan(0));
            Assert.That(result.OrderId, Is.EqualTo(order.Id));
            Assert.That(result.Order, Is.Not.Null);
            Assert.That(result.InvoiceNumber, Is.EqualTo($"INV-{DateTime.UtcNow.Year}:{order.Id:D4}"));
            Assert.That(result.Order.Items, Is.Not.Empty);
        }
    }

    [Test]
    public Task FindByOrderId_ShouldThrowNotFoundException()
    {
        var ex = Assert.ThrowsAsync<NotFoundException>(async ()
            => await _invoiceService.FindByOrderId(1));

        Assert.That(ex.Message, Does.Not.Empty);
        return Task.CompletedTask;
    }


    private static ApplicationDbContext GetInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }
}
