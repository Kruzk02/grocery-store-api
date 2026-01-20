using Domain.Entity;

using Infrastructure.Persistence;
using Infrastructure.Services;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace Tests.Services;

[TestFixture]
public class CategoryServiceTest
{

    private static ApplicationDbContext GetInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    [Test]
    [TestCaseSource(nameof(CreateCategories))]
    public async Task GetAllCategories_ShouldReturn_ListOfCategory(List<Category> category)
    {
        var ctx = GetInMemoryDbContext();
        foreach (var item in category) ctx.Categories.Add(item);
        await ctx.SaveChangesAsync();

        var service = new CategoryService(ctx, new MemoryCache(new MemoryCacheOptions()));

        var result = service.FindAll();
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Result, Is.Not.Null);
            Assert.That(result.Result, Has.Count.EqualTo(13));
        }
    }

    private static IEnumerable<List<Category>> CreateCategories()
    {
        yield return
        [
            new Category { Id = 1, Name = "Fresh Produce", Description = "Fruits, vegetables, herbs" },
            new Category { Id = 2, Name = "Dairy & Eggs", Description = "Milk, cheese, yogurt, eggs" },
            new Category { Id = 3, Name = "Meat & Seafood", Description = "Fresh meat, poultry, seafood" },
            new Category { Id = 4, Name = "Bakery", Description = "Bread and baked products" },
            new Category { Id = 5, Name = "Pantry Staples", Description = "Rice, pasta, spices" },
            new Category { Id = 6, Name = "Canned & Packaged", Description = "Canned goods, sauces, instant" },
            new Category { Id = 7, Name = "Frozen Foods", Description = "Frozen meat, ice cream" },
            new Category { Id = 8, Name = "Snacks & Confectionery", Description = "Chips, chocolates, biscuits" },
            new Category { Id = 9, Name = "Beverages", Description = "Water, soda, tea, coffee" },
            new Category { Id = 10, Name = "Household & Cleaning", Description = "Detergents, cleaning items" },
            new Category { Id = 11, Name = "Personal Care & Health", Description = "Toiletries, health items" },
            new Category { Id = 12, Name = "Baby & Kids", Description = "Baby food, diapers" },
            new Category { Id = 13, Name = "Miscellaneous", Description = "Other / seasonal products" }
        ];
    }
}
