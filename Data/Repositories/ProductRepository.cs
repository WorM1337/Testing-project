using Core.Interfaces;
using Core.Models;
using Core.Models.Enums;
using Data.Contexts;
using Microsoft.EntityFrameworkCore;

namespace Data.Repositories;

public class ProductRepository(BookOfReceiptsDbContext context) : IProductRepository
{
    public async Task<Product?> GetByIdAsync(int id)
    {
        return await context.Products
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public IQueryable<Product> GetQueryable() => context.Products.AsQueryable();
    public async Task<List<Product>> GetByIdsAsync(List<int> ids)
    {
        return await context.Products
            .Where(p => ids.Contains(p.Id))
            .ToListAsync();
    }

    public async Task<IEnumerable<Product>> GetByCategoryAsync(ProductCategory category)
    {
        return await context.Products
            .Where(p => p.Category == category)
            .ToListAsync();
    }

    public async Task<Product> CreateAsync(Product product)
    {
        context.Products.Add(product);
        await context.SaveChangesAsync();
        return product;
    }

    public async Task UpdateAsync(Product product)
    {
        context.Products.Update(product);
        await context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Product product)
    {
        context.Products.Remove(product);
        await context.SaveChangesAsync();
    }

    public async Task<bool> IsUsedInDishesAsync(int productId)
    {
        return await context.Ingredients.AnyAsync(i => i.ProductId == productId);
    }
}