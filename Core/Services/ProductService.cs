
using Core.Extensions;
using Core.Interfaces;
using Core.Models;
using Core.Models.Query;
using Microsoft.EntityFrameworkCore;

namespace Core.Services;


public class ProductService(IProductRepository productRepository, IDishRepository dishRepository)
    : IProductService
{
    public async Task<List<Product>> GetProductsAsync(ProductQuery query)
    {
        return await productRepository.GetQueryable()
            .ApplyFilters(query)
            .ApplySorting(query.Sort, query.Ascending)
            .ToListAsync();
    }

    public async Task<Product> CreateProductAsync(Product product)
    {
        // Логика по умолчанию
        product.CreatedAt = DateTime.UtcNow;
        return await productRepository.CreateAsync(product);
    }

    public async Task<Product> UpdateProductAsync(Product product)
    {
        product.UpdatedAt = DateTime.UtcNow;
        await productRepository.UpdateAsync(product);
        return product;
    }

    public async Task<bool> DeleteProductAsync(int id)
    {
        var product = await productRepository.GetByIdAsync(id);
        if (product == null) return false;

        var dishes = await dishRepository.GetDishesUsingProductAsync(id);
        if (dishes.Any())
        {
            var dishNames = string.Join(", ", dishes.Select(d => d.Name));
            throw new InvalidOperationException($"Нельзя удалить продукт: он используется в блюдах: {dishNames}");
        }
            

        await productRepository.DeleteAsync(product);
        return true;
    }
}