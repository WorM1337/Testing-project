using Core.Models.Enums;

namespace Core.Models.Query;

public record DishQuery
{
    public string? Search { get; init; }
    public DishCategory? Category { get; init; }
    public List<ExtraFlag>? Flags { get; init; }
    public DishSortOption Sort { get; init; } = DishSortOption.Name;
    public bool Ascending { get; init; } = true;
}