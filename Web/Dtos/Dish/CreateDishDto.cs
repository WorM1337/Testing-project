using Core.Models.Enums;
using Testing_project.Dtos.Ingredient;

namespace Testing_project.Dtos.Dish;

public record CreateDishDto
{
    public string Name { get; init; } = null!;
    public List<string>? Photos { get; init; }
    public List<CreateIngredientDto> Ingredients { get; init; } = new();
    
    /// <summary>
    /// КБЖУ на порцию. Если не указано, рассчитывается автоматически на основе состава.
    /// Пользователь может переопределить рассчитанные значения.
    /// </summary>
    public double? CaloriesPerServing { get; init; }
    public double? ProteinsPerServing { get; init; }
    public double? FatsPerServing { get; init; }
    public double? CarbsPerServing { get; init; }
    
    /// <summary>
    /// Размер порции в граммах. Если не указан, рассчитывается как сумма веса всех ингредиентов.
    /// </summary>
    public double? ServingSize { get; init; }
    
    public ExtraFlag Flags { get; init; } = ExtraFlag.None;
    public DishCategory Category { get; init; } = DishCategory.None;
}
