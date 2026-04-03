using Core.Models;

namespace Core.Interfaces;

public interface IDishRepository
{
    Task<Dish?> GetByIdAsync(int id);
    IQueryable<Dish> GetQueryable();
    Task<Dish> CreateAsync(Dish dish);
    Task UpdateAsync(Dish dish);
    Task DeleteAsync(Dish dish);
    Task<IEnumerable<Dish>> GetDishesUsingProductAsync(int productId);
}