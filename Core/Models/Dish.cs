using Core.Models.Enums;

namespace Core.Models;

public class Dish
{
    public int Id { get; set; }
    public string Name { get; set; } // Мин. 2 символа
    public List<Photo> Photos { get; set; } // До 5
    public double CaloriesPerServing { get; set; }
    public double ProteinsPerServing { get; set; }
    public double FatsPerServing { get; set; }
    public double CarbsPerServing { get; set; }
    public double ServingSize { get; set; } 
    public DishCategory Category { get; set; }
    public ExtraFlag Flags { get; set; } = ExtraFlag.None;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public ICollection<Ingredient> Ingredients { get; set; } = null!;
}