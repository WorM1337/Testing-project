using Core.Models.Enums;
using Testing_project.Dtos.Ingredient;

namespace Testing_project.Dtos.Dish;

public record UpdateDishDto
{
    public string? Name { get; init; }
    
    public List<string>? Photos { get; init; }
    
    public List<CreateIngredientDto>? Ingredients { get; init; }
    
    public ExtraFlag? Flags { get; init; }
    public DishCategory? Category { get; init; }
}