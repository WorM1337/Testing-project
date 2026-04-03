using Core.Models;
using Core.Models.Query;

namespace Core.Interfaces;

public interface IProductService
{
    Task<Product> CreateProductAsync(Product product);
    Task<Product> UpdateProductAsync(Product product);
    Task<bool> DeleteProductAsync(int id);
    Task<List<Product>> GetProductsAsync(ProductQuery query);
}
