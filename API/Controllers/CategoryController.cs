using Application.Services;

using Domain.Entity;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

/// <summary>
/// Handles requests to categories.
/// </summary>
/// <remarks>
/// This controller provided endpoints to retrieve categories.
/// </remarks>
/// <param name="service"></param>
[ApiController, Route("[controller]"), Authorize]
public class CategoryController(ICategoryService service) : ControllerBase
{
    /// <summary>
    /// Retrieves all categories
    /// </summary>
    /// <returns>
    /// A list of <see cref="Category"/> object
    /// </returns>
    /// <response code="200">Returns the list of categories.</response>
    /// <response code="500">If an unexpected error occurs.</response>
    [HttpGet]
    [ProducesResponseType(typeof(List<Category>), 200)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> GetAll() => Ok(await service.FindAll());
}
