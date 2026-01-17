using Application.Dtos.Request;
using Application.Services.impl;

using Domain.Entity;
using Domain.Exception;

using Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace Tests.Services;

[TestFixture]
public class InventoryServiceTest
{
    private InventoryService _inventoryService;
    private ApplicationDbContext _dbContext;

    [SetUp]
    public void Setup()
    {
        _dbContext = GetInMemoryDbContext();
        _inventoryService = new InventoryService(_dbContext, new MemoryCache(new MemoryCacheOptions()));
    }

    [TearDown]
    public void TearDown()
    {
        _dbContext.Dispose();
    }

    [Test]
    [TestCaseSource(nameof(CreateProduct))]
    public async Task CreateInventory_shouldCreateProduct(Product product)
    {
        _dbContext.Products.Add(product);
        await _dbContext.SaveChangesAsync();

        var result = await _inventoryService.Create(new InventoryDto(product.Id, 20));

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Id, Is.EqualTo(1));
            Assert.That(result.ProductId, Is.EqualTo(product.Id));
            Assert.That(result.Product, Is.EqualTo(product));
            Assert.That(result.Quantity, Is.EqualTo(20));
        }
    }

    [Test]
    [TestCase(1, 1)]
    [TestCase(1, 2)]
    [TestCase(1, 3)]
    [TestCase(1, 4)]
    public Task CreateInventory_shouldThrowNotFoundException(int productId, int quantity)
    {
        var ex = Assert.ThrowsAsync<NotFoundException>(async () =>
            await _inventoryService.Create(new InventoryDto(productId, quantity)));

        Assert.That(ex.Message, Is.EqualTo($"Product with id: {productId} not found"));
        return Task.CompletedTask;
    }

    [Test]
    [TestCaseSource(nameof(CreateProduct))]
    public async Task UpdateInventory_shouldUpdateInventory(Product product)
    {
        _dbContext.Products.Add(product);
        await _dbContext.SaveChangesAsync();

        var inventory = await _inventoryService.Create(new InventoryDto(ProductId: product.Id, Quantity: 20));

        var result = await _inventoryService.Update(1, new InventoryDto(ProductId: product.Id, Quantity: 10));

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Id, Is.EqualTo(inventory.Id));
            Assert.That(result.ProductId, Is.EqualTo(inventory.ProductId));
            Assert.That(result.Product, Is.EqualTo(inventory.Product));
            Assert.That(result.Quantity, Is.EqualTo(10));
        }
    }

    [Test]
    [TestCase(1, 1)]
    [TestCase(1, 2)]
    [TestCase(1, 3)]
    [TestCase(1, 4)]
    public Task UpdateInventory_shouldThrowNotFoundException_whenInventoryNotFound(int productId, int quantity)
    {
        var ex = Assert.ThrowsAsync<NotFoundException>(async () =>
            await _inventoryService.Update(1, new InventoryDto(productId, quantity)));

        Assert.That(ex.Message, Is.EqualTo("Inventory with id: 1 not found"));
        return Task.CompletedTask;
    }

    [Test]
    [TestCaseSource(nameof(CreateProduct))]
    public async Task FindAll_shouldReturnListOfInventory(Product product)
    {
        _dbContext.Products.Add(product);
        await _dbContext.SaveChangesAsync();

        await _inventoryService.Create(new InventoryDto(ProductId: product.Id, Quantity: 20));

        var result = await _inventoryService.FindAll();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Has.Count.EqualTo(1));
        }
    }

    [Test]
    [TestCaseSource(nameof(CreateProduct))]
    public async Task FindById_shouldReturnInventory(Product product)
    {
        _dbContext.Products.Add(product);
        await _dbContext.SaveChangesAsync();

        var inventory = await _inventoryService.Create(new InventoryDto(ProductId: product.Id, Quantity: 20));

        var result = await _inventoryService.FindById(inventory.Id);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Id, Is.EqualTo(inventory.Id));
            Assert.That(result.ProductId, Is.EqualTo(inventory.ProductId));
            Assert.That(result.Product, Is.EqualTo(inventory.Product));
            Assert.That(result.Quantity, Is.EqualTo(inventory.Quantity));
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
            await _inventoryService.FindById(id));

        Assert.That(ex.Message, Is.EqualTo($"Inventory with id: {id} not found"));
        return Task.CompletedTask;

    }

    [Test]
    [TestCaseSource(nameof(CreateProduct))]
    public async Task DeleteById_shouldDeleteInventory(Product product)
    {
        _dbContext.Products.Add(product);
        await _dbContext.SaveChangesAsync();

        var inventory = await _inventoryService.Create(new InventoryDto(ProductId: product.Id, Quantity: 20));

        var result = await _inventoryService.Delete(inventory.Id);
        Assert.That(result, Is.EqualTo("Inventory deleted successfully"));
    }

    [Test]
    [TestCase(1)]
    [TestCase(2)]
    [TestCase(3)]
    [TestCase(4)]
    public Task DeleteById_ShouldThrowNotFoundException(int id)
    {
        var ex = Assert.ThrowsAsync<NotFoundException>(async () =>
            await _inventoryService.Delete(id));

        Assert.That(ex.Message, Is.EqualTo($"Inventory with id: {id} not found"));
        return Task.CompletedTask;

    }

    private static IEnumerable<Product> CreateProduct()
    {
        yield return new Product
        {
            Id = 1,
            Name = "name1",
            Description = "description",
            Price = 49.99m,
            CategoryId = 1,
            Category = new Category { Id = 1, Name = "Fresh Produce", Description = "Fruits, vegetables, herbs" }
        };
        yield return new Product
        {
            Id = 2,
            Name = "name2",
            Description = "description",
            Price = 99.99m,
            CategoryId = 10,
            Category = new Category
            { Id = 10, Name = "Household & Cleaning", Description = "Detergents, cleaning items" }
        };
        yield return new Product
        {
            Id = 3,
            Name = "name2",
            Description = "description",
            Price = 599.99m,
            CategoryId = 13,
            Category = new Category { Id = 13, Name = "Miscellaneous", Description = "Other / seasonal products" }
        };

    }

    private static ApplicationDbContext GetInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }
}
