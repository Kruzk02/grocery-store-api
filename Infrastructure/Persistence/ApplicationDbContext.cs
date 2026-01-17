using Domain.Entity;

using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<VerificationToken> VerificationTokens { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<Inventory> Inventories { get; set; }
    public DbSet<Notification> Notifications { get; set; }
    public DbSet<Customer> Customers { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
    public DbSet<Invoice> Invoices { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Category>().HasData(
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
        );
    }
}
