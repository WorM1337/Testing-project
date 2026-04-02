using Core.Extentions;
using Core.Interfaces;
using Core.Models;
using Core.Models.Enums;
using Core.Utils;

namespace Core.Services;

public class DishService(IDishRepository dishRepository, IProductRepository productRepository)
    : IDishService
{
    public async Task<Dish> CreateDishAsync(Dish dish)
    {
        // Парсим макрос
        var (macroCategory, cleanName) = DishCategoryParser.Parse(dish.Name);

        // Сохраняем чистое имя
        dish.Name = cleanName;

        // Если категория не задана вручную — используем макрос
        if (dish.Category == DishCategory.None && macroCategory.HasValue)
        {
            dish.Category = macroCategory.Value;
        }
        
        // 1. Рассчитать КБЖУ автоматически
        await CalculateNutritionAsync(dish);

        // 2. Определить флаги на основе состава
        SetFlagsBasedOnIngredients(dish);

        // 3. Заполнить системные поля
        dish.CreatedAt = DateTime.UtcNow;

        return await dishRepository.CreateAsync(dish);
    }

    public async Task<Dish> UpdateDishAsync(Dish dish)
    {
        // Пересчитать при обновлении
        await CalculateNutritionAsync(dish);
        SetFlagsBasedOnIngredients(dish);
        dish.UpdatedAt = DateTime.UtcNow;

        await dishRepository.UpdateAsync(dish);
        return dish;
    }

    public async Task<bool> DeleteDishAsync(int id)
    {
        var dish = await dishRepository.GetByIdAsync(id);
        if (dish == null) return false;

        await dishRepository.DeleteAsync(dish);
        return true;
    }

    private async Task CalculateNutritionAsync(Dish dish)
    {
        double calories = 0, proteins = 0, fats = 0, carbs = 0;
        double totalWeight = 0;

        foreach (var ingredient in dish.Ingredients)
        {
            var product = await productRepository.GetByIdAsync(ingredient.ProductId);
            if (product == null) continue;

            var qty = ingredient.AmountInGrams;
            totalWeight += qty;

            calories += (product.CaloriesPer100g * qty) / 100;
            proteins += (product.ProteinsPer100g * qty) / 100;
            fats += (product.FatsPer100g * qty) / 100;
            carbs += (product.CarbsPer100g * qty) / 100;
        }

        dish.CaloriesPerServing = calories;
        dish.ProteinsPerServing = proteins;
        dish.FatsPerServing = fats;
        dish.CarbsPerServing = carbs;
        dish.ServingSize = totalWeight;
    }

    private void SetFlagsBasedOnIngredients(Dish dish)
    {
        // Флаг "Веган" — только если все продукты веганские
        var allVegan = dish.Ingredients.All(i => i.Product.Flags.HasFlag(ExtraFlag.Vegan));
        if (allVegan) dish.Flags.Add(ExtraFlag.Vegan);
        else dish.Flags.Remove(ExtraFlag.Vegan);

        // Аналогично для "Без глютена", "Без сахара"
        var allGlutenFree = dish.Ingredients.All(i => i.Product.Flags.HasFlag(ExtraFlag.GlutenFree));
        if (allGlutenFree) dish.Flags.Add(ExtraFlag.GlutenFree);
        else dish.Flags.Remove(ExtraFlag.GlutenFree);

        var allSugarFree = dish.Ingredients.All(i => i.Product.Flags.HasFlag(ExtraFlag.SugarFree));
        if (allSugarFree) dish.Flags.Add(ExtraFlag.SugarFree);
        else dish.Flags.Remove(ExtraFlag.SugarFree);
    }
}