namespace Testing_project.Dtos.Ingredient;

public record IngredientDto(
    int ProductId,
    string ProductName,
    double QuantityInGrams,
    double CaloriesPer100g,
    double ProteinsPer100g,
    double FatsPer100g,
    double CarbsPer100g
);