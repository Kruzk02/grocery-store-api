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
public class InventoryServiceTest
{
    private IInventoryService _inventoryService;
    private ApplicationDbContext _dbContext;

    [SetUp]
    public void Setup()
    {
        _dbContext = GetInMemoryDbContext();
        _inventoryService = new InventoryService(new InventoryRepository(_dbContext), new ProductRepository(_dbContext), new MemoryCache(new MemoryCacheOptions()));
    }

    [TearDown]
    public void TearDown()
    {
        _dbContext.Dispose();
    }

    [Test]
    [TestCaseSource(nameof(CreateProduct))]
    public async Task CreateInventoryShouldCreateProduct(Product product)
    {
        _ = _dbContext.Products.Add(product);
        _ = await _dbContext.SaveChangesAsync();

        Inventory result = await _inventoryService.Create(new InventoryDto(product.Id, 20));

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
    public Task CreateInventoryShouldThrowNotFoundException(int productId, int quantity)
    {
        NotFoundException? ex = Assert.ThrowsAsync<NotFoundException>(async () =>
            await _inventoryService.Create(new InventoryDto(productId, quantity)));

        Assert.That(ex.Message, Is.EqualTo($"Product with id: {productId} not found"));
        return Task.CompletedTask;
    }

    [Test]
    [TestCaseSource(nameof(CreateProduct))]
    public async Task UpdateInventoryShouldUpdateInventory(Product product)
    {
        _ = _dbContext.Products.Add(product);
        _ = await _dbContext.SaveChangesAsync();

        Inventory inventory = await _inventoryService.Create(new InventoryDto(ProductId: product.Id, Quantity: 20));

        Inventory result = await _inventoryService.Update(1, new InventoryDto(ProductId: product.Id, Quantity: 10));

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
    public Task UpdateInventoryShouldThrowNotFoundExceptionWhenInventoryNotFound(int productId, int quantity)
    {
        NotFoundException? ex = Assert.ThrowsAsync<NotFoundException>(async () =>
            await _inventoryService.Update(1, new InventoryDto(productId, quantity)));

        Assert.That(ex.Message, Is.EqualTo("Inventory with id: 1 not found"));
        return Task.CompletedTask;
    }

    [Test]
    [TestCaseSource(nameof(CreateProduct))]
    public async Task FindAllShouldReturnListOfInventory(Product product)
    {
        _ = _dbContext.Products.Add(product);
        _ = await _dbContext.SaveChangesAsync();

        _ = await _inventoryService.Create(new InventoryDto(ProductId: product.Id, Quantity: 20));

        List<Inventory> result = await _inventoryService.FindAll();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Has.Count.EqualTo(1));
        }
    }

    [Test]
    [TestCaseSource(nameof(CreateProduct))]
    public async Task FindByIdShouldReturnInventory(Product product)
    {
        _ = _dbContext.Products.Add(product);
        _ = await _dbContext.SaveChangesAsync();

        Inventory inventory = await _inventoryService.Create(new InventoryDto(ProductId: product.Id, Quantity: 20));

        Inventory result = await _inventoryService.FindById(inventory.Id);

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
    public Task FindByIdShouldThrowNotFoundException(int id)
    {
        NotFoundException? ex = Assert.ThrowsAsync<NotFoundException>(async () =>
            await _inventoryService.FindById(id));

        Assert.That(ex.Message, Is.EqualTo($"Inventory with id: {id} not found"));
        return Task.CompletedTask;

    }

    [Test]
    [TestCaseSource(nameof(CreateProduct))]
    public async Task FindByProductIdShouldReturnListOfInventory(Product product)
    {
        _ = _dbContext.Products.Add(product);
        _ = await _dbContext.SaveChangesAsync();

        Inventory inventory = await _inventoryService.Create(new InventoryDto(product.Id, 20));

        List<Inventory> result = await _inventoryService.FindByProductId(product.Id);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result[0].Id, Is.EqualTo(inventory.Id));
            Assert.That(result[0].ProductId, Is.EqualTo(inventory.ProductId));
            Assert.That(result[0].Product, Is.EqualTo(inventory.Product));
            Assert.That(result[0].Quantity, Is.EqualTo(inventory.Quantity));
        }
    }

    [Test]
    [TestCaseSource(nameof(CreateProduct))]
    public async Task FindByQuantityShouldRetrunListOfInventory(Product product)
    {
        _ = _dbContext.Products.Add(product);
        _ = await _dbContext.SaveChangesAsync();

        Inventory inventory = await _inventoryService.Create(new InventoryDto(product.Id, 20));

        List<Inventory> result = await _inventoryService.FindByQuantity(20);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result[0].Id, Is.EqualTo(inventory.Id));
            Assert.That(result[0].ProductId, Is.EqualTo(inventory.ProductId));
            Assert.That(result[0].Product, Is.EqualTo(inventory.Product));
            Assert.That(result[0].Quantity, Is.EqualTo(inventory.Quantity));
        }
    }

    [Test]
    [TestCaseSource(nameof(CreateProduct))]
    public async Task DeleteByIdShouldDeleteInventory(Product product)
    {
        _ = _dbContext.Products.Add(product);
        _ = await _dbContext.SaveChangesAsync();

        Inventory inventory = await _inventoryService.Create(new InventoryDto(ProductId: product.Id, Quantity: 20));

        var result = await _inventoryService.Delete(inventory.Id);
        Assert.That(result, Is.EqualTo("Inventory deleted successfully"));
    }

    [Test]
    [TestCase(1)]
    [TestCase(2)]
    [TestCase(3)]
    [TestCase(4)]
    public Task DeleteByIdShouldThrowNotFoundException(int id)
    {
        NotFoundException? ex = Assert.ThrowsAsync<NotFoundException>(async () =>
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
        DbContextOptions<ApplicationDbContext> options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }
}
