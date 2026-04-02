using Core.Interfaces;
using Core.Models;

namespace Core.Services;


public class ProductService(IProductRepository productRepository, IDishRepository dishRepository)
    : IProductService
{
    private readonly IDishRepository _dishRepository = dishRepository;

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

        var dishes = await _dishRepository.GetDishesUsingProductAsync(id);
        if (dishes.Any())
        {
            var dishNames = string.Join(", ", dishes.Select(d => d.Name));
            throw new InvalidOperationException($"Нельзя удалить продукт: он используется в блюдах: {dishNames}");
        }
            

        await productRepository.DeleteAsync(product);
        return true;
    }
}