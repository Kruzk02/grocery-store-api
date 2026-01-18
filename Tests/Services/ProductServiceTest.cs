using Application.Dtos.Request;
using Application.Services.impl;

using Domain.Entity;
using Domain.Exception;

using Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace Tests.Services;

[TestFixture]
public class ProductServiceTest
{
    private ProductService _productService;
    private ApplicationDbContext _context;

    [SetUp]
    public void SetUp()
    {
        _context = GetInMemoryDbContext();
        _productService = new ProductService(_context, new MemoryCache(new MemoryCacheOptions()));
    }

    [TearDown]
    public void Destroy()
    {
        _context.Dispose();
    }

    [Test]
    [TestCaseSource(nameof(CreateProductDto))]
    public async Task CreateProduct_ShouldCreateProduct(ProductDto productDto)
    {
        var category = new Category { Id = 1, Name = "Fresh Produce", Description = "Fruits, vegetables, herbs" };
        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        var result = await _productService.Create(productDto);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Id, Is.GreaterThan(0));
            Assert.That(result.Name, Does.StartWith("name"));
            Assert.That(result.Description, Does.StartWith("description"));
            Assert.That(result.Price, Is.EqualTo(productDto.Price));
            Assert.That(result.CategoryId, Is.EqualTo(1));
        }
    }

    [Test]
    [TestCaseSource(nameof(CreateProductDto))]
    public Task CreateProduct_ShouldThrowNotFoundException(ProductDto productDto)
    {
        var ex = Assert.ThrowsAsync<NotFoundException>(async () =>
            await _productService.Create(productDto));

        Assert.That(ex!.Message, Is.EqualTo($"Category with id {productDto.CategoryId} not found"));
        return Task.CompletedTask;
    }

    [Test]
    [TestCaseSource(nameof(CreateProductDto))]
    public async Task UpdateProduct_ShouldUpdateProduct(ProductDto productDto)
    {
        var category = new Category { Id = 1, Name = "Fresh Produce", Description = "Fruits, vegetables, herbs" };
        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        var product = await _productService.Create(productDto);

        var result = await _productService.Update(product.Id, new ProductDto(Name: "name123", Description: "description123", Price: 11.99m, CategoryId: 1, Quantity: 44, "image.jpg"));

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result, !Is.Null);
            Assert.That(result.Id, Is.GreaterThan(0));
            Assert.That(result.Name, Does.StartWith("name"));
            Assert.That(result.Description, Does.StartWith("description"));
            Assert.That(result.CategoryId, Is.EqualTo(1));
        }
    }

    [Test]
    [TestCaseSource(nameof(CreateProductDto))]
    public Task UpdateProduct_ShouldThrowNotFoundException(ProductDto productDto)
    {
        var ex = Assert.ThrowsAsync<NotFoundException>(async () =>
            await _productService.Update(1, productDto));

        Assert.That(ex!.Message, Is.EqualTo("Product with id 1 not found"));
        return Task.CompletedTask;
    }

    [Test]
    [TestCaseSource(nameof(CreateProductDto))]
    public async Task SearchProducts_shouldReturnListOfProduct(ProductDto productDto)
    {
        var category = new Category { Id = 1, Name = "Fresh Produce", Description = "Fruits, vegetables, herbs" };
        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        var product = await _productService.Create(productDto);

        var result = await _productService.SearchProducts(product.Name, 0, 10);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.total, Is.GreaterThan(0));
            Assert.That(result.data, Is.Not.Null);
        }
    }

    [Test]
    [TestCaseSource(nameof(CreateProductDto))]
    public async Task FindById_ShouldReturnProduct(ProductDto productDto)
    {
        var category = new Category { Id = 1, Name = "Fresh Produce", Description = "Fruits, vegetables, herbs" };
        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        var product = await _productService.Create(productDto);

        var result = await _productService.FindById(product.Id);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Id, Is.EqualTo(product.Id));
            Assert.That(result.Name, Is.EqualTo(product.Name));
            Assert.That(result.Description, Is.EqualTo(product.Description));
            Assert.That(result.Price, Is.EqualTo(product.Price));
            Assert.That(result.CategoryId, Is.EqualTo(product.CategoryId));
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
            await _productService.FindById(id));

        Assert.That(ex!.Message, Is.EqualTo($"Product with id {id} not found"));
        return Task.CompletedTask;
    }

    [Test]
    [TestCaseSource(nameof(CreateProductDto))]
    public async Task DeleteById(ProductDto productDto)
    {
        var category = new Category { Id = 1, Name = "Fresh Produce", Description = "Fruits, vegetables, herbs" };
        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        var product = await _productService.Create(productDto);

        var result = await _productService.DeleteById(product.Id);

        Assert.That(result, Is.True);
    }

    [Test]
    [TestCase(1)]
    [TestCase(2)]
    [TestCase(3)]
    [TestCase(4)]
    public Task DeleteById_ShouldThrowNotFoundException(int id)
    {
        var ex = Assert.ThrowsAsync<NotFoundException>(async () =>
            await _productService.DeleteById(id));

        Assert.That(ex!.Message, Is.EqualTo($"Product with id {id} not found"));
        return Task.CompletedTask;
    }

    private static IEnumerable<ProductDto> CreateProductDto()
    {
        yield return new ProductDto(Name: "name123", Description: "description3", Price: 2.99m, CategoryId: 1, Quantity: 1, "image.jpg");
        yield return new ProductDto(Name: "name4", Description: "description", Price: 5.99m, CategoryId: 1, Quantity: 1, "image.jpg");
        yield return new ProductDto(Name: "name56", Description: "description", Price: 6.99m, CategoryId: 1, Quantity: 1, "image.jpg");
    }

    private static ApplicationDbContext GetInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }
}
