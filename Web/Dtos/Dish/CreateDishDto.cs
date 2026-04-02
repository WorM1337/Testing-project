using Core.Models.Enums;
using Testing_project.Dtos.Ingredient;

namespace Testing_project.Dtos.Dish;

public record CreateDishDto(
    string Name,
    List<string>? Photos,
    List<CreateIngredientDto> Ingredients,
    HashSet<ExtraFlag> Flags = null!,
    DishCategory Category = DishCategory.None
);