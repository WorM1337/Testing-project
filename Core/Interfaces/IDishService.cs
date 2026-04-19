using Core.Models;
using Core.Models.Query;

namespace Core.Interfaces;

public interface IDishService
{
    Task<Dish> CreateDishAsync(Dish dish);
    Task<Dish> UpdateDishAsync(Dish dish, 
        bool categoryWasExplicitlySet = false,
        bool caloriesWasExplicitlySet = false,
        bool proteinsWasExplicitlySet = false,
        bool fatsWasExplicitlySet = false,
        bool carbsWasExplicitlySet = false,
        bool servingSizeWasExplicitlySet = false);
    Task<bool> DeleteDishAsync(int id);
    Task<List<Dish>> GetDishesAsync(DishQuery query);
}
