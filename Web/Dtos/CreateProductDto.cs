using Core.Models.Enums;

namespace Testing_project.Dtos;

public record CreateProductDto(
    string Name,
    List<string>? Photos,
    double CaloriesPer100g,
    double ProteinsPer100g,
    double FatsPer100g,
    double CarbsPer100g,
    string? Composition,
    ProductCategory Category,
    CookingRequirement CookingNeeded,
    ExtraFlag Flags = ExtraFlag.None
);