using Core.Models;

namespace Core.Interfaces;

public interface IDishRepository
{
    Task<Dish?> GetByIdAsync(int id);
    Task<IEnumerable<Dish>> GetAllAsync();
    Task<Dish> CreateAsync(Dish dish);
    Task UpdateAsync(Dish dish);
    Task DeleteAsync(Dish dish);
}