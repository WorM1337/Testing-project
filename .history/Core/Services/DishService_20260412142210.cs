using Core.Extensions;
using Core.Interfaces;
using Core.Models;
using Core.Models.Enums;
using Core.Models.Query;
using Core.Utils;
using Microsoft.EntityFrameworkCore; // Если нужно Include, но тут лучше явный запрос

namespace Core.Services;

public class DishService(IDishRepository dishRepository, IProductRepository productRepository)
    : IDishService
{
    public async Task<List<Dish>> GetDishesAsync(DishQuery query)
    {
        return await dishRepository.GetQueryable()
            .ApplyFilters(query)
            .ApplySorting(query.Sort, query.Ascending)
            .ToListAsync();
    }
    public async Task<Dish> CreateDishAsync(Dish dish)
    {
        var (macroCategory, cleanName) = DishCategoryParser.Parse(dish.Name);
        dish.Name = cleanName;

        if (dish.Category == DishCategory.None && macroCategory.HasValue)
        {
            dish.Category = macroCategory.Value;
        }

        // 1. Загружаем продукты одним запросом
        await LoadProductsForIngredientsAsync(dish);

        // 2. Расчеты КБЖУ (только если пользователь не указал свои значения)
        CalculateNutrition(dish, 
            overrideCalories: dish.CaloriesPerServing,
            overrideProteins: dish.ProteinsPerServing,
            overrideFats: dish.FatsPerServing,
            overrideCarbs: dish.CarbsPerServing,
            overrideServingSize: dish.ServingSize);
        
        // 3. Установка флагов на основе ингредиентов
        SetFlagsBasedOnIngredients(dish);

        dish.CreatedAt = DateTime.UtcNow;

        return await dishRepository.CreateAsync(dish);
    }

    public async Task<Dish> UpdateDishAsync(Dish dish)
    {
        // 1. Сначала подгружаем продукты, если ингредиенты изменились или их нет в памяти
        // Важно: при PATCH ингредиенты могут быть уже в dish, но без навигационного свойства Product
        await LoadProductsForIngredientsAsync(dish);

        // 2. Пересчет КБЖУ с учётом возможных пользовательских переопределений
        CalculateNutrition(dish,
            overrideCalories: dish.CaloriesPerServing,
            overrideProteins: dish.ProteinsPerServing,
            overrideFats: dish.FatsPerServing,
            overrideCarbs: dish.CarbsPerServing,
            overrideServingSize: dish.ServingSize);
        
        // 3. Пересчет флагов
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

    /// <summary>
    /// Умная загрузка продуктов для всех ингредиентов одним запросом.
    /// Заполняет свойство Product у каждого ингредиента.
    /// </summary>
    private async Task LoadProductsForIngredientsAsync(Dish dish)
    {
        if (dish.Ingredients == null || dish.Ingredients.Count == 0)
            return;

        // Собираем уникальные ID продуктов
        var productIds = dish.Ingredients
            .Select(i => i.ProductId)
            .Distinct()
            .ToList();

        if (!productIds.Any())
            return;

        // ОДИН запрос в БД: получаем только нужные продукты
        // Предполагаем, что productRepository.GetAllAsync() возвращает IQueryable или Task<IEnumerable<Product>>.
        // Если есть метод GetByIdsAsync - лучше использовать его.
        // Если нет, фильтруем на стороне БД (если репозиторий поддерживает) или в памяти.
        
        // Вариант А: Если репозиторий позволяет фильтровать (рекомендуется добавить в интерфейс)
        // var products = await productRepository.GetByIdsAsync(productIds);
        
        // Вариант Б: Если есть только GetAll (неэффективно при большой БД, но работает)
        // Лучше измените интерфейс репозитория! Но пока сделаем так, если GetAll возвращает IQueryable:
        // var products = await productRepository.GetAllAsync()
        //     .Where(p => productIds.Contains(p.Id))
        //     .ToListAsync();
        
        // Самый надежный вариант без изменения интерфейса (если GetAll возвращает список):
        // Придется грузить все, если нет метода по ID. 
        // НО: Правильнее добавить метод в репозиторий. Давайте предположим, вы можете добавить метод или у вас есть доступ к DbSet.
        // Ниже пример с гипотетическим GetByIds, который стоит реализовать в репозитории.
        
        var products = await productRepository.GetByIdsAsync(productIds);

        var productsMap = products.ToDictionary(p => p.Id);

        // Заполняем навигационные свойства
        foreach (var ingredient in dish.Ingredients)
        {
            if (productsMap.TryGetValue(ingredient.ProductId, out var product))
            {
                ingredient.Product = product;
            }
            else
            {
                throw new InvalidOperationException($"Продукт с ID {ingredient.ProductId} не найден в базе.");
            }
        }
    }

    /// <summary>
    /// Расчет КБЖУ. Теперь синхронный, так как данные уже загружены в память.
    /// Если пользователь указал свои значения (override), они используются вместо рассчитанных.
    /// </summary>
    private void CalculateNutrition(Dish dish, 
        double? overrideCalories = null, 
        double? overrideProteins = null, 
        double? overrideFats = null, 
        double? overrideCarbs = null,
        double? overrideServingSize = null)
    {
        double calories = 0, proteins = 0, fats = 0, carbs = 0;
        double totalWeight = 0;

        foreach (var ingredient in dish.Ingredients)
        {
            // Продукт уже загружен методом LoadProductsForIngredientsAsync
            var product = ingredient.Product;
            if (product == null) 
                throw new InvalidOperationException($"Продукт для ингредиента {ingredient.ProductId} не загружен.");

            var qty = ingredient.AmountInGrams;
            totalWeight += qty;

            calories += (product.CaloriesPer100g * qty) / 100;
            proteins += (product.ProteinsPer100g * qty) / 100;
            fats += (product.FatsPer100g * qty) / 100;
            carbs += (product.CarbsPer100g * qty) / 100;
        }

        // Используем пользовательские значения или рассчитанные
        dish.CaloriesPerServing = overrideCalories ?? calories;
        dish.ProteinsPerServing = overrideProteins ?? proteins;
        dish.FatsPerServing = overrideFats ?? fats;
        dish.CarbsPerServing = overrideCarbs ?? carbs;
        dish.ServingSize = overrideServingSize ?? totalWeight;
    }

    private void SetFlagsBasedOnIngredients(Dish dish)
    {
        if (dish.Ingredients.Any(i => i.Product == null))
            throw new InvalidOperationException("Нельзя установить флаги: не все продукты загружены.");

        // Флаг "Веган"
        var allVegan = dish.Ingredients.All(i => i.Product.Flags.HasFlag(ExtraFlag.Vegan));
        if (allVegan) dish.Flags |= ExtraFlag.Vegan; // Используем побитовое ИЛИ
        else dish.Flags &= ~ExtraFlag.Vegan; // Снимаем флаг

        // Флаг "Без глютена"
        var allGlutenFree = dish.Ingredients.All(i => i.Product.Flags.HasFlag(ExtraFlag.GlutenFree));
        if (allGlutenFree) dish.Flags |= ExtraFlag.GlutenFree;
        else dish.Flags &= ~ExtraFlag.GlutenFree;

        // Флаг "Без сахара"
        var allSugarFree = dish.Ingredients.All(i => i.Product.Flags.HasFlag(ExtraFlag.SugarFree));
        if (allSugarFree) dish.Flags |= ExtraFlag.SugarFree;
        else dish.Flags &= ~ExtraFlag.SugarFree;
    }
}
