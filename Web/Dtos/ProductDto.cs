using Core.Models.Enums;

namespace Testing_project.Dtos;
public record ProductDto(
    int Id,
    string Name,
    List<string> Photos,
    double CaloriesPer100g,
    double ProteinsPer100g,
    double FatsPer100g,
    double CarbsPer100g,
    string? Composition,
    ProductCategory Category,
    CookingRequirement CookingNeeded,
    ExtraFlag Flags,
    DateTime CreatedAt,
    DateTime? UpdatedAt
)
{
    public ProductDto() : this(0, "", new List<string>(), 0.0, 0.0, 0.0, 0.0, null, ProductCategory.None, CookingRequirement.ReadyToUse, ExtraFlag.None, DateTime.MinValue, null)
    {
    }
}