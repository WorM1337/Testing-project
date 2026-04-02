using Core.Models.Enums;

namespace Core.Models;

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public double CaloriesPer100g { get; set; }
    public double ProteinsPer100g { get; set; } 
    public double FatsPer100g { get; set; } 
    public double CarbsPer100g { get; set; }
    public string? Composition { get; set; }
    public ProductCategory Category { get; set; }
    public CookingRequirement CookingRequirement { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    
    public ExtraFlag Flags { get; set; } = ExtraFlag.None;
    public ICollection<string> Photos { get; set; } = new List<string>();
    public ICollection<Ingredient> Ingredients { get; set; } = new List<Ingredient>();
}