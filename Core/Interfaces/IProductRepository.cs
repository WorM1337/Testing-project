using Core.Models;
using Core.Models.Enums;

namespace Core.Interfaces;

public interface IProductRepository
{
    Task<Product?> GetByIdAsync(int id);
    Task<IEnumerable<Product>> GetAllAsync();
    Task<List<Product>> GetByIdsAsync(List<int> ids);
    Task<IEnumerable<Product>> GetByCategoryAsync(ProductCategory category);
    Task<Product> CreateAsync(Product product);
    Task UpdateAsync(Product product);
    Task DeleteAsync(Product product);
    Task<bool> IsUsedInDishesAsync(int productId);
}