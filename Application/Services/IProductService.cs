using Application.Dtos.Request;

using Domain.Entity;

namespace Application.Services;

/// <summary>
/// Defines operations for managing products.
/// </summary>
public interface IProductService
{
    /// <summary>
    /// Asynchronously retrieves products with the name from the database.
    /// </summary>
    Task<(int total, List<Product> data)> SearchProducts(string? name, int skip, int take);

    /// <summary>
    /// Asynchronously creates a new product in the database.
    /// </summary>
    Task<Product> Create(ProductDto productDto);

    /// <summary>
    /// Asynchronously updates an existing product in the database.
    /// </summary>
    Task<Product> Update(int id, ProductDto productDto);

    /// <summary>
    /// Asynchronously retrieves a product by its identifier from the database.
    /// </summary>
    Task<Product> FindById(int id);

    /// <summary>
    /// Asynchronously deletes a product by its identifier from the database.
    /// </summary>
    Task<bool> DeleteById(int id);
}
