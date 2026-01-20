using API.Dto;

using Application.Common;
using Application.Dtos.Request;
using Application.Services;

using Domain.Entity;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

/// <summary>
/// Handles requests to products.
/// </summary>
/// <remarks>
/// This controller provided endpoints to Create, Retrieve, Update and Delete products.
/// </remarks>
/// <param name="productService"></param>
[ApiController, Route("[controller]"), Authorize]
public class ProductController(IProductService productService, IOrderItemService itemService, IImageStorage imageStorage) : ControllerBase
{
    /// <summary>
    /// Searches for products by name, or returns all products if no name is provided.
    /// </summary>
    /// <param name="name">
    /// Optional. The product name (full or partial) to search for.
    /// Case-insensitive. If null or empty, all products are returned.
    /// </param>
    /// <param name="skip">
    /// The number of products to skip (for pagination).
    /// Defaults to 0.
    /// </param>
    /// <param name="take">
    /// The maximum number of products to return.
    /// Defaults to 10.
    /// </param>
    /// <returns>
    /// A tuple containing:
    /// <list type="bullet">
    /// <item><description><c>total</c>: The total number of matching products before pagination.</description></item>
    /// <item><description><c>data</c>: The list of products after applying skip/take pagination.</description></item>
    /// </list>
    /// </returns>
    [HttpGet]
    [ProducesResponseType(typeof(List<Product>), 200)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> FindProducts([FromQuery] string? name, [FromQuery] int skip = 0, [FromQuery] int take = 10)
    {
        var (total, data) = await productService.SearchProducts(name, skip, take);
        return Ok(new { total, data });
    }

    /// <summary>
    /// Create a new product.
    /// </summary>
    /// <param name="productDto">The product data to create.</param>
    /// <returns>
    /// The newly created <see cref="Product"/>.
    /// </returns>
    /// <response code="201">Returns the created product.</response>
    /// <response code="400">If the product data is invalid.</response>
    /// <response code="500">If an unexpected error occurs.</response>
    [HttpPost]
    [ProducesResponseType(typeof(Product), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> Create([FromForm] CreatedProductDto createdProductDto)
    {
        string filename = "";
        if (createdProductDto.photo != null)
        {
            using var stream = createdProductDto.photo.OpenReadStream();

            filename = await imageStorage.Save(stream, Path.GetExtension(createdProductDto.photo.FileName), createdProductDto.photo.ContentType);
        }

        ProductDto productDto = new(createdProductDto.Name, createdProductDto.Description, createdProductDto.Price, createdProductDto.CategoryId, createdProductDto.Quantity, filename);

        var result = await productService.Create(productDto);
        return CreatedAtAction(nameof(FindById), new { id = result.Id }, result);
    }

    [HttpGet("{filename}")]
    [ProducesResponseType(400), ProducesResponseType(500)]
    public IActionResult GetImage(string filename)
    {
        var path = imageStorage.GetImage(filename);
        if (!System.IO.File.Exists(path)) NotFound();
        var extension = Path.GetExtension(filename);

        return PhysicalFile(path, $"image/{extension.Replace(".", string.Empty)}");
    }

    /// <summary>
    /// Updates an existing product.
    /// </summary>
    /// <param name="id">The ID of the product to update.</param>
    /// <param name="productDto">The update product data.</param>
    /// <returns>
    /// No content if the update was successful.
    /// </returns>
    /// <response code="204">Product was successfully updated.</response>
    /// <response code="400">If the product data is invalid.</response>
    /// <response code="404">If the product does not exist.</response>
    /// <response code="500">If an unexpected error occurs.</response>
    [HttpPut("{id:int}")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> Update(int id, [FromForm] UpdatedProductDto updatedProductDto)
    {
        string filename = "";
        if (updatedProductDto.photo != null)
        {
            using var stream = updatedProductDto.photo.OpenReadStream();
            filename = await imageStorage.Save(stream, Path.GetExtension(updatedProductDto.photo.FileName), updatedProductDto.photo.ContentType);
        }

        ProductDto productDto = new(updatedProductDto.Name, updatedProductDto.Description, updatedProductDto.Price, updatedProductDto.CategoryId, updatedProductDto.Quantity, filename);
        var result = await productService.Update(id, productDto);
        return Ok(result);
    }

    /// <summary>
    /// Retrieves a product by it ID.
    /// </summary>
    /// <param name="id">The ID of the product to retrieve.</param>
    /// <returns>
    /// The requested <see cref="Product"/>
    /// </returns>
    /// <response code="200">Returns the requested product.</response>
    /// <response code="404">If the product is not found.</response>
    /// <response code="500">If an unexpected error occurs.</response>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(Product), 200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> FindById(int id)
    {
        var result = await productService.FindById(id);
        return Ok(result);
    }

    [HttpGet("{id:int}/ordersItem")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> FindOrderItemById(int id)
    {
        var result = await itemService.FindByProductId(id);
        return Ok(result);
    }

    /// <summary>
    /// Deletes a product by its ID.
    /// </summary>
    /// <param name="id">The ID of the product to delete.</param>
    /// <returns>
    /// No content if the product was successfully deleted.
    /// </returns>
    /// <response code="204">Product was successfully deleted.</response>
    /// <response code="404">If the product is not found.</response>
    /// <response code="500">If an unexpected error occurs.</response>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> DeleteById(int id)
    {
        var result = await productService.DeleteById(id);
        return result ? NoContent() : BadRequest();
    }
}
