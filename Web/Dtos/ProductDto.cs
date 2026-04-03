using Core.Models.Enums;

namespace Testing_project.Dtos;

public record ProductDto
{
    public int Id { get; init; }
    public string Name { get; init; } = null!;
    public List<string> Photos { get; init; } = new();
    public double CaloriesPer100g { get; init; }
    public double ProteinsPer100g { get; init; }
    public double FatsPer100g { get; init; }
    public double CarbsPer100g { get; init; }
    public string? Composition { get; init; }
    public ProductCategory Category { get; init; }
    public CookingRequirement CookingNeeded { get; init; }
    public ExtraFlag Flags { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}