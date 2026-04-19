using Core.Models.Enums;

namespace Testing_project.Dtos;

public record UpdateProductDto
{
    public string? Name { get; init; }
    public List<string>? Photos { get; init; }
    public double? CaloriesPer100g { get; init; }
    public double? ProteinsPer100g { get; init; }
    public double? FatsPer100g { get; init; }
    public double? CarbsPer100g { get; init; }
    public string? Composition { get; init; }
    public ProductCategory? Category { get; init; }
    public CookingRequirement? CookingRequirement { get; init; }
    
    /// <summary>
    /// Flags as comma-separated string (e.g., "Vegan,GlutenFree") for easier frontend handling.
    /// Will be parsed into ExtraFlag enum by the controller.
    /// </summary>
    public string? Flags { get; init; }
}
