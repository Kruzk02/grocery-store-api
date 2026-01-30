using Application.Common;
using Application.Dtos.Request;
using Application.Interface;
using Application.Repository;

using Domain.Entity;
using Domain.Exception;

using Microsoft.Extensions.Caching.Memory;

namespace Application.Services;

/// <summary>
/// Provides operations for create, retrieve, update and delete product.
/// </summary>
/// <remarks>
/// This class interacts with database to performs CRUD operations related to produts.
/// </remarks>
/// <param name="ctx">the <see cref="ApplicationDbContext"/> used to access the database.</param>
public class ProductService(IProductRepository productRepository, ICategoryRepository categoryRepository, IImageStorage imageStorage, IMemoryCache cache) : IProductService
{
    public async Task<(int total, List<Product> data)> SearchProducts(string? name, int skip, int take)
    {
        return await productRepository.Search(name, skip, take);
    }

    ///  <inheritdoc/>
    public async Task<Product> Create(ProductDto productDto)
    {
        var category = await categoryRepository.FindById(productDto.CategoryId);
        if (category == null)
        {
            throw new NotFoundException($"Category with id {productDto.CategoryId} not found");
        }

        var product = new Product
        {
            Name = productDto.Name,
            Description = productDto.Description,
            Price = productDto.Price,
            Quantity = productDto.Quantity,
            CategoryId = productDto.CategoryId,
            Category = category,
            imagePath = productDto.filename,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        return await productRepository.Add(product);
    }

    /// <inheritdoc/>
    public async Task<Product> Update(int id, ProductDto productDto)
    {
        var product = await productRepository.FindById(id);
        if (product == null)
        {
            throw new NotFoundException($"Product with id {id} not found");
        }

        if (!string.IsNullOrEmpty(productDto.Name) && productDto.Name != product.Name)
            product.Name = productDto.Name;

        if (!string.IsNullOrEmpty(productDto.Description) && productDto.Description != product.Description)
            product.Description = productDto.Description;

        if (productDto.Price >= 0 && productDto.Price != product.Price)
            product.Price = productDto.Price;

        if (productDto.Quantity >= 0 && productDto.Quantity != product.Quantity)
            product.Quantity = productDto.Quantity;

        if (productDto.CategoryId != product.CategoryId)
        {
            var category = await categoryRepository.FindById(productDto.CategoryId);
            if (category == null)
            {
                throw new NotFoundException($"Category with id {productDto.CategoryId} not found");
            }

            product.CategoryId = category.Id;
            product.Category = category;
        }

        if (productDto.filename != null && product.imagePath != null)
        {
            await imageStorage.Delete(product.imagePath);
            product.imagePath = productDto.filename;
        }

        product.UpdatedAt = DateTime.UtcNow;

        await productRepository.Update(product);

        return product;
    }

    /// <inheritdoc/>
    public async Task<Product> FindById(int id)
    {
        var cacheKey = $"product:{id}";
        if (cache.TryGetValue(cacheKey, out Product? product))
        {
            Console.WriteLine("Hit");
            if (product != null)
                return product;
        }

        product = await productRepository.FindById(id);
        var cacheOption = new MemoryCacheEntryOptions()
            .SetSlidingExpiration(TimeSpan.FromMinutes(10))
            .SetAbsoluteExpiration(TimeSpan.FromMinutes(30));

        cache.Set(cacheKey, product, cacheOption);
        return product ?? throw new NotFoundException($"Product with id {id} not found");
    }

    /// <inheritdoc/>
    public async Task<bool> DeleteById(int id)
    {
        var product = await productRepository.FindById(id);
        if (product == null)
        {
            throw new NotFoundException($"Product with id {id} not found");
        }

        cache.Remove($"product:{id}");
        await productRepository.Delete(product);
        if (product.imagePath != null)
            await imageStorage.Delete(product.imagePath);
        return true;
    }
}
