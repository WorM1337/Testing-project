using Core.Models.Enums;
using Testing_project.Dtos.Ingredient;

namespace Testing_project.Dtos.Dish;

public record CreateDishDto
{
    public string Name { get; init; } = null!;
    public List<string>? Photos { get; init; }
    public List<CreateIngredientDto> Ingredients { get; init; } = new();
    public ExtraFlag Flags { get; init; } = ExtraFlag.None;
    public DishCategory Category { get; init; } = DishCategory.None;
}