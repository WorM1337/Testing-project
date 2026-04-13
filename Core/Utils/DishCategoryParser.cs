using Core.Models.Enums;
using System.Text.RegularExpressions;

namespace Core.Utils;

public static class DishCategoryParser
{
    private static readonly List<(string Macro, DishCategory Category)> Macros = new()
    {
        ("!десерт", DishCategory.Dessert),
        ("!первое", DishCategory.Entree),
        ("!второе", DishCategory.Side),
        ("!напиток", DishCategory.Drink),
        ("!салат", DishCategory.Salad),
        ("!суп", DishCategory.Soup),
        ("!перекус", DishCategory.Snack)
    };

    /// <summary>
    /// Ищет все макросы в названии блюда и удаляет их.
    /// Если макросов несколько, применяется только первый найденный (слева направо), остальные игнорируются.
    /// Все макросы удаляются из названия.
    /// </summary>
    public static (DishCategory? Category, string CleanName) Parse(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) 
            return (null, name);

        // Ищем все макросы в названии (регистронезависимо)
        var foundMacros = new List<(int Index, string Macro, DishCategory Category)>();
        
        foreach (var (macro, macroCategory) in Macros)
        {
            // Ищем все вхождения макроса (может быть несколько одинаковых)
            int startIndex = 0;
            while ((startIndex = name.IndexOf(macro, startIndex, StringComparison.OrdinalIgnoreCase)) >= 0)
            {
                foundMacros.Add((startIndex, macro, macroCategory));
                startIndex += macro.Length;
            }
        }

        if (!foundMacros.Any())
        {
            return (null, name);
        }

        // Сортируем по индексу (порядок появления в названии)
        var sortedMacros = foundMacros.OrderBy(m => m.Index).ToList();
        
        // Берём первый макрос для определения категории
        var firstMacro = sortedMacros.First();
        var category = firstMacro.Category;
        
        // Удаляем ВСЕ макросы из названия (начиная с конца, чтобы индексы не сдвигались)
        var cleanName = name;
        foreach (var macro in sortedMacros.OrderByDescending(m => m.Index))
        {
            cleanName = cleanName.Remove(macro.Index, macro.Macro.Length);
        }
        
        // Удаляем лишние пробелы, которые могли образоваться
        cleanName = Regex.Replace(cleanName, @"\s+", " ").Trim();
        
        // Проверяем, что название не короче 2 символов после удаления макросов
        if (cleanName.Length < 2)
        {
            throw new InvalidOperationException(
                $"Название блюда слишком короткое после удаления макросов (минимум 2 символа). Текущее название: '{cleanName}'");
        }
        
        return (category, cleanName);
    }
}
