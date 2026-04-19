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

        // 2. Валидация флагов: нельзя установить флаг, если не все ингредиенты его имеют
        // Флаги устанавливаются пользователем, мы только валидируем
        ValidateFlags(dish);

        // 3. Расчеты КБЖУ (только если пользователь не указал свои значения)
        CalculateNutrition(dish, 
            overrideCalories: dish.CaloriesPerServing,
            overrideProteins: dish.ProteinsPerServing,
            overrideFats: dish.FatsPerServing,
            overrideCarbs: dish.CarbsPerServing,
            overrideServingSize: dish.ServingSize);

        dish.CreatedAt = DateTime.UtcNow;

        return await dishRepository.CreateAsync(dish);
    }

    public async Task<Dish> UpdateDishAsync(Dish dish, bool categoryWasExplicitlySet = false,
        bool caloriesWasExplicitlySet = false,
        bool proteinsWasExplicitlySet = false,
        bool fatsWasExplicitlySet = false,
        bool carbsWasExplicitlySet = false,
        bool servingSizeWasExplicitlySet = false)
    {
        // 0. Обработка макросов в названии (аналогично CreateDishAsync)
        var (macroCategory, cleanName) = DishCategoryParser.Parse(dish.Name);
        dish.Name = cleanName;

        // Если категория не была явно установлена через форму, используем макрос
        // Если категория была явно установлена (categoryWasExplicitlySet = true), оставляем её
        // Если макросов нет и категория не была установлена явно — оставляем текущую категорию блюда
        if (!categoryWasExplicitlySet && macroCategory.HasValue)
        {
            dish.Category = macroCategory.Value;
        }

        // 1. Сначала подгружаем продукты, если ингредиенты изменились или их нет в памяти
        await LoadProductsForIngredientsAsync(dish);

        // 2. Пересчет флагов: при изменении состава автоматически снимаем недопустимые флаги
        // Флаги устанавливаются пользователем, но при изменении состава недопустимые снимаются
        RecalculateFlags(dish);

        // 3. Пересчет КБЖУ с учётом возможных пользовательских переопределений
        // Используем null для значений, которые не были явно установлены пользователем
        CalculateNutrition(dish,
            overrideCalories: caloriesWasExplicitlySet ? dish.CaloriesPerServing : null,
            overrideProteins: proteinsWasExplicitlySet ? dish.ProteinsPerServing : null,
            overrideFats: fatsWasExplicitlySet ? dish.FatsPerServing : null,
            overrideCarbs: carbsWasExplicitlySet ? dish.CarbsPerServing : null,
            overrideServingSize: servingSizeWasExplicitlySet ? dish.ServingSize : null);

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

        // Используем пользовательские значения (если они были явно указаны) или рассчитанные
        // null означает "рассчитать автоматически"
        dish.CaloriesPerServing = overrideCalories ?? calories;
        dish.ProteinsPerServing = overrideProteins ?? proteins;
        dish.FatsPerServing = overrideFats ?? fats;
        dish.CarbsPerServing = overrideCarbs ?? carbs;
        dish.ServingSize = overrideServingSize ?? totalWeight;
    }

    /// <summary>
    /// Валидация флагов: проверяет, что пользователь не пытается установить флаг,
    /// который не поддерживается всеми ингредиентами.
    /// Используется при создании блюда.
    /// </summary>
    private void ValidateFlags(Dish dish)
    {
        // Проверяем только если пользователь явно указал флаги
        if (dish.Flags == ExtraFlag.None)
            return;

        var invalidFlags = new List<string>();

        // Проверка флага "Веган"
        if (dish.Flags.HasFlag(ExtraFlag.Vegan))
        {
            var nonVeganProducts = dish.Ingredients
                .Where(i => !i.Product.Flags.HasFlag(ExtraFlag.Vegan))
                .Select(i => i.Product.Name)
                .ToList();

            if (nonVeganProducts.Any())
            {
                invalidFlags.Add($"Веган: продукты без флага — {string.Join(", ", nonVeganProducts)}");
            }
        }

        // Проверка флага "Без глютена"
        if (dish.Flags.HasFlag(ExtraFlag.GlutenFree))
        {
            var nonGlutenFreeProducts = dish.Ingredients
                .Where(i => !i.Product.Flags.HasFlag(ExtraFlag.GlutenFree))
                .Select(i => i.Product.Name)
                .ToList();

            if (nonGlutenFreeProducts.Any())
            {
                invalidFlags.Add($"Без глютена: продукты без флага — {string.Join(", ", nonGlutenFreeProducts)}");
            }
        }

        // Проверка флага "Без сахара"
        if (dish.Flags.HasFlag(ExtraFlag.SugarFree))
        {
            var nonSugarFreeProducts = dish.Ingredients
                .Where(i => !i.Product.Flags.HasFlag(ExtraFlag.SugarFree))
                .Select(i => i.Product.Name)
                .ToList();

            if (nonSugarFreeProducts.Any())
            {
                invalidFlags.Add($"Без сахара: продукты без флага — {string.Join(", ", nonSugarFreeProducts)}");
            }
        }

        if (invalidFlags.Any())
        {
            throw new InvalidOperationException(
                $"Нельзя установить флаги: {string.Join("; ", invalidFlags)}");
        }
    }

    /// <summary>
    /// Пересчет флагов: автоматически снимает недопустимые флаги при изменении состава.
    /// Используется при обновлении блюда.
    /// </summary>
    private void RecalculateFlags(Dish dish)
    {
        // Если флаги не установлены, нечего пересчитывать
        if (dish.Flags == ExtraFlag.None)
            return;

        // Проверяем каждый установленный флаг и снимаем недопустимые
        if (dish.Flags.HasFlag(ExtraFlag.Vegan))
        {
            var allVegan = dish.Ingredients.All(i => i.Product.Flags.HasFlag(ExtraFlag.Vegan));
            if (!allVegan)
            {
                dish.Flags &= ~ExtraFlag.Vegan; // Снимаем флаг
            }
        }

        if (dish.Flags.HasFlag(ExtraFlag.GlutenFree))
        {
            var allGlutenFree = dish.Ingredients.All(i => i.Product.Flags.HasFlag(ExtraFlag.GlutenFree));
            if (!allGlutenFree)
            {
                dish.Flags &= ~ExtraFlag.GlutenFree; // Снимаем флаг
            }
        }

        if (dish.Flags.HasFlag(ExtraFlag.SugarFree))
        {
            var allSugarFree = dish.Ingredients.All(i => i.Product.Flags.HasFlag(ExtraFlag.SugarFree));
            if (!allSugarFree)
            {
                dish.Flags &= ~ExtraFlag.SugarFree; // Снимаем флаг
            }
        }
    }

}
