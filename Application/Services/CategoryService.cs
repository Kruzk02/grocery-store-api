using Application.Interface;
using Application.Repository;

using Domain.Entity;

using Microsoft.Extensions.Caching.Memory;

namespace Application.Services;

/// <summary>
/// Provides operations for retrieving categories.
/// </summary>
/// <remarks>
/// This class interacts with database to performs retrieve operations related to categories.
/// </remarks>
/// <param name="ctx">the <see cref="ApplicationDbContext"/> used to access the database.</param>
public class CategoryService(ICategoryRepository categoryRepository, IMemoryCache cache) : ICategoryService
{
    /// <inheritdoc />
    public async Task<List<Category>> FindAll()
    {
        const string cacheKey = $"categories";
        if (cache.TryGetValue(cacheKey, out List<Category>? categories))
            if (categories != null)
                return categories;

        categories = await categoryRepository.FindAll();
        var cacheOption = new MemoryCacheEntryOptions()
            .SetSlidingExpiration(TimeSpan.FromMinutes(10))
            .SetAbsoluteExpiration(TimeSpan.FromMinutes(20));

        cache.Set(cacheKey, categories, cacheOption);
        return categories;
    }
}
