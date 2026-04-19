using Core.Models.Enums;
using Testing_project.Dtos.Ingredient;

namespace Testing_project.Dtos.Dish;

public record UpdateDishDto
{
    public string? Name { get; init; }
    
    public List<string>? Photos { get; init; }
    
    public List<CreateIngredientDto>? Ingredients { get; init; }
    
    /// <summary>
    /// КБЖУ на порцию. Если указано, переопределяет автоматически рассчитанные значения.
    /// </summary>
    public double? CaloriesPerServing { get; init; }
    public double? ProteinsPerServing { get; init; }
    public double? FatsPerServing { get; init; }
    public double? CarbsPerServing { get; init; }
    
    /// <summary>
    /// Размер порции в граммах. Если указан, переопределяет сумму веса ингредиентов.
    /// </summary>
    public double? ServingSize { get; init; }
    
    /// <summary>
    /// Flags as comma-separated string (e.g., "Vegan,GlutenFree") for easier frontend handling.
    /// Will be parsed into ExtraFlag enum by the controller.
    /// </summary>
    public string? Flags { get; init; }
    
    public DishCategory? Category { get; init; }
}
