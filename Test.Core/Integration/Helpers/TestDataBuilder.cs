using Core.Models;
using Core.Models.Enums;
using Testing_project.Dtos;
using Testing_project.Dtos.Dish;
using Testing_project.Dtos.Ingredient;

namespace Test.Core.Integration.Helpers;

/// <summary>
/// Билдер для создания тестовых данных
/// </summary>
public static class TestDataBuilder
{
    /// <summary>
    /// Создает базовый продукт для тестов
    /// </summary>
    public static CreateProductDto CreateProduct(
        string name = "Тестовый продукт",
        double calories = 100,
        double proteins = 10,
        double fats = 5,
        double carbs = 15,
        ProductCategory category = ProductCategory.Vegetables,
        ExtraFlag flags = ExtraFlag.None)
    {
        return new CreateProductDto
        {
            Name = name,
            CaloriesPer100g = calories,
            ProteinsPer100g = proteins,
            FatsPer100g = fats,
            CarbsPer100g = carbs,
            Category = category,
            CookingRequirement = CookingRequirement.ReadyToUse,
            Flags = flags
        };
    }

    /// <summary>
    /// Создает базовое блюдо для тестов
    /// </summary>
    public static CreateDishDto CreateDish(
        string name = "Тестовое блюдо",
        int productId = 1,
        double amount = 100,
        DishCategory category = DishCategory.Dessert,
        ExtraFlag flags = ExtraFlag.None)
    {
        return new CreateDishDto
        {
            Name = name,
            Category = category,
            Flags = flags,
            Ingredients = new List<CreateIngredientDto>
            {
                new() { ProductId = productId, AmountInGrams = amount }
            }
        };
    }

    /// <summary>
    /// Создает список ингредиентов
    /// </summary>
    public static List<CreateIngredientDto> CreateIngredients(params (int ProductId, double Amount)[] ingredients)
    {
        return ingredients
            .Select(i => new CreateIngredientDto 
            { 
                ProductId = i.ProductId, 
                AmountInGrams = i.Amount 
            })
            .ToList();
    }
}