using Core.Models;
using Microsoft.EntityFrameworkCore;

namespace Data.Contexts;

public class BookOfReceiptsDbContext(DbContextOptions<BookOfReceiptsDbContext> options) : DbContext(options)
{
    public DbSet<Product> Products { get; set; }
    public DbSet<Ingredient> Ingredients { get; set; }
    public DbSet<Dish> Dishes { get; set; }
}