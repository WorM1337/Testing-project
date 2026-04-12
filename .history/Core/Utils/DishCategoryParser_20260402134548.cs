using Core.Models.Enums;

namespace Core.Utils;

public static class DishCategoryParser
{
    private static readonly Dictionary<string, DishCategory> Macros = new(StringComparer.OrdinalIgnoreCase)
    {
        { "!суп", DishCategory.Soup },
        { "!первое", DishCategory.Entree },
        { "!второе", DishCategory.Side },
        { "!десерт", DishCategory.Dessert },
        { "!напиток", DishCategory.Drink },
        { "!салат", DishCategory.Salad },
        { "!перекус", DishCategory.Snack }
    };

    public static (DishCategory? Category, string CleanName) Parse(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) 
            return (null, name);

        foreach (var (macro, category) in Macros)
        {
            if (name.StartsWith(macro))
            {
                var cleanName = name.Substring(macro.Length).Trim();
                return (category, cleanName);
            }
        }

        return (null, name);
    }
}