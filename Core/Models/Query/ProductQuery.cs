using Core.Models.Enums;

namespace Core.Models.Query;

public record ProductQuery
{
    public string? Search { get; init; }
    public ProductCategory? Category { get; init; }
    public CookingRequirement? CookingRequirement { get; init; }
    public List<ExtraFlag>? Flags { get; init; }
    public ProductSortOption Sort { get; init; } = ProductSortOption.Name;
    public bool Ascending { get; init; } = true;
}