using Core.Models;
using Core.Models.Query;

namespace Core.Interfaces;

public interface IDishService
{
    Task<Dish> CreateDishAsync(Dish dish);
    Task<Dish> UpdateDishAsync(Dish dish);
    Task<bool> DeleteDishAsync(int id);
    Task<List<Dish>> GetDishesAsync(DishQuery query);
}