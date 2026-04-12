using Core.Models.Enums;

namespace Core.Models;

public class Dish
{
    public int Id { get; set; }
    public string Name { get; set; } = null!; // Мин. 2 символа
    
    /// <summary>
    /// Калорийность на порцию. Может быть null до расчета.
    /// </summary>
    public double? CaloriesPerServing { get; set; }
    
    /// <summary>
    /// Белки на порцию. Могут быть null до расчета.
    /// </summary>
    public double? ProteinsPerServing { get; set; }
    
    /// <summary>
    /// Жиры на порцию. Могут быть null до расчета.
    /// </summary>
    public double? FatsPerServing { get; set; }
    
    /// <summary>
    /// Углеводы на порцию. Могут быть null до расчета.
    /// </summary>
    public double? CarbsPerServing { get; set; }
    
    /// <summary>
    /// Размер порции в граммах. Может быть null до расчета.
    /// </summary>
    public double? ServingSize { get; set; }
    
    public DishCategory Category { get; set; }
    public ExtraFlag Flags { get; set; } = ExtraFlag.None;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public IList<string> Photos { get; set; } = new List<string>(); // До 5
    public IList<Ingredient> Ingredients { get; set; } = new List<Ingredient>();
}
