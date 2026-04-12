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
    /// Ищет первый макрос в названии блюда и удаляет его.
    /// Если макросов несколько, применяется только первый найденный (слева направо).
    /// </summary>
    public static (DishCategory? Category, string CleanName) Parse(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) 
            return (null, name);

        // Ищем все макросы в названии (регистронезависимо)
        var foundMacros = new List<(int Index, string Macro, DishCategory Category)>();
        
        foreach (var (macro, category) in Macros)
        {
            var index = name.IndexOf(macro, StringComparison.OrdinalIgnoreCase);
            if (index >= 0)
            {
                foundMacros.Add((index, macro, category));
            }
        }

        if (!foundMacros.Any())
        {
            return (null, name);
        }

        // Берём первый найденный макрос (с наименьшим индексом)
        var firstMacro = foundMacros.OrderBy(m => m.Index).First();
        
        // Удаляем макрос из названия
        var cleanName = name.Remove(firstMacro.Index, firstMacro.Macro.Length);
        
        // Удаляем лишние пробелы, которые могли образоваться
        cleanName = Regex.Replace(cleanName, @"\s+", " ").Trim();
        
        return (firstMacro.Category, cleanName);
    }
}
