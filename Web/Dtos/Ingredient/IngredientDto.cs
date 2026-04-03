namespace Testing_project.Dtos.Ingredient;

public record IngredientDto
{
    public int ProductId { get; init; }
    public string ProductName { get; init; } = null!;
    public double AmountInGrams { get; init; }
    public double CaloriesPer100g { get; init; }
    public double ProteinsPer100g { get; init; }
    public double FatsPer100g { get; init; }
    public double CarbsPer100g { get; init; }
}