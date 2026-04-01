namespace Core.Models;

public class Ingredient
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public int DishId { get; set; }
    public Product Product { get; set; } = null!;
    public Dish Dish { get; set; } = null!;
    public double AmountInGrams { get; set; } // грамм продукта в порции
}