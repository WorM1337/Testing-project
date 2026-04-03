namespace Testing_project.Dtos.Ingredient;

public record CreateIngredientDto
{
    public int ProductId { get; init; }
    public double AmountInGrams { get; init; }
}