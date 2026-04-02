using Core.Models.Enums;
using Testing_project.Dtos.Ingredient;

namespace Testing_project.Dtos.Dish;

public record DishDto(
    int Id,
    string Name,
    List<string> Photos,
    double CaloriesPerServing,
    double ProteinsPerServing,
    double FatsPerServing,
    double CarbsPerServing,
    double ServingSize,
    DishCategory Category,
    HashSet<ExtraFlag> Flags,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    List<IngredientDto> Ingredients
);