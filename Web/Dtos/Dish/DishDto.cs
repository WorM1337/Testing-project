using Core.Models.Enums;
using Testing_project.Dtos.Ingredient;

namespace Testing_project.Dtos.Dish;

public record DishDto
{
    public int Id { get; init; }
    public string Name { get; init; } = null!;
    public List<string> Photos { get; init; } = new();
    public double? CaloriesPerServing { get; init; }
    public double? ProteinsPerServing { get; init; }
    public double? FatsPerServing { get; init; }
    public double? CarbsPerServing { get; init; }
    public double? ServingSize { get; init; }
    public DishCategory Category { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
    public List<IngredientDto> Ingredients { get; init; } = new();
    public ExtraFlag Flags { get; init; }
}
