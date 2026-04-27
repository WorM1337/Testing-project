using Core.Models;
using Core.Models.Enums;

namespace Test.Core;

public static class ProductHelper
{
    public static readonly Product Potato = new()
    {
        Id = 1,
        Name = "Картошка",
        CaloriesPer100g = 100,
        ProteinsPer100g = 2,
        FatsPer100g = 0.5,
        CarbsPer100g = 4,
        Category = ProductCategory.Vegetables,
        CookingRequirement = CookingRequirement.RequiresCooking,
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow,
        Flags = ExtraFlag.None,
        Photos = new List<string>()
    };

    public static readonly Product ChickenBreast = new()
    {
        Id = 2,
        Name = "Куриное филе",
        CaloriesPer100g = 165,
        ProteinsPer100g = 31,
        FatsPer100g = 3.6,
        CarbsPer100g = 0,
        Category = ProductCategory.Meat,
        CookingRequirement = CookingRequirement.RequiresCooking,
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow,
        Flags = ExtraFlag.None,
        Photos = new List<string>()
    };

    public static readonly Product OliveOil = new()
    {
        Id = 3,
        Name = "Оливковое масло",
        CaloriesPer100g = 900,
        ProteinsPer100g = 0,
        FatsPer100g = 100,
        CarbsPer100g = 0,
        Category = ProductCategory.Liquid,
        CookingRequirement = CookingRequirement.ReadyToUse,
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow,
        Flags = ExtraFlag.None,
        Photos = new List<string>()
    };

    public static readonly Product Rice = new()
    {
        Id = 4,
        Name = "Рис",
        CaloriesPer100g = 130,
        ProteinsPer100g = 2.7,
        FatsPer100g = 0.3,
        CarbsPer100g = 28,
        Category = ProductCategory.Cereals,
        CookingRequirement = CookingRequirement.RequiresCooking,
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow,
        Flags = ExtraFlag.None,
        Photos = new List<string>()
    };

    public static readonly Product Tomato = new()
    {
        Id = 5,
        Name = "Томаты",
        CaloriesPer100g = 18,
        ProteinsPer100g = 0.9,
        FatsPer100g = 0.2,
        CarbsPer100g = 3.9,
        Category = ProductCategory.Vegetables,
        CookingRequirement = CookingRequirement.ReadyToUse,
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow,
        Flags = ExtraFlag.None,
        Photos = new List<string>()
    };

    // Продукты для проверки граничных значений
    
    public static readonly Product Water = new()
    {
        Id = 6,
        Name = "Вода",
        CaloriesPer100g = 0,
        ProteinsPer100g = 0,
        FatsPer100g = 0,
        CarbsPer100g = 0,
        Category = ProductCategory.Liquid,
        CookingRequirement = CookingRequirement.ReadyToUse,
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow,
        Flags = ExtraFlag.None,
        Photos = new List<string>()
    };

    public static readonly Product Butter = new()
    {
        Id = 7,
        Name = "Масло топлёное",
        CaloriesPer100g = 900,
        ProteinsPer100g = 0,
        FatsPer100g = 100,
        CarbsPer100g = 0,
        Category = ProductCategory.Liquid,
        CookingRequirement = CookingRequirement.ReadyToUse,
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow,
        Flags = ExtraFlag.None,
        Photos = new List<string>()
    };

    public static readonly Product Protein = new()
    {
        Id = 8,
        Name = "Протеин",
        CaloriesPer100g = 370,
        ProteinsPer100g = 100,
        FatsPer100g = 1,
        CarbsPer100g = 0,
        Category = ProductCategory.Sweets,
        CookingRequirement = CookingRequirement.ReadyToUse,
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow,
        Flags = ExtraFlag.None,
        Photos = new List<string>()
    };

    public static readonly Product Sugar = new()
    {
        Id = 9,
        Name = "Сахар",
        CaloriesPer100g = 400,
        ProteinsPer100g = 0,
        FatsPer100g = 0,
        CarbsPer100g = 100,
        Category = ProductCategory.Sweets,
        CookingRequirement = CookingRequirement.ReadyToUse,
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow,
        Flags = ExtraFlag.None,
        Photos = new List<string>()
    };

    public static readonly Product[] AllProducts = 
    [
        Potato,
        ChickenBreast,
        OliveOil,
        Rice,
        Tomato,
        Water,
        Butter,
        Protein,
        Sugar
    ];
    public static (List<Ingredient>, List<Product>) BuildIngredientsAndProductsByIdsAndAmounts(int[] productIdsAndAmounts)
    {
        var ingredients = new List<Ingredient>();
        var products = new List<Product>();
        
        for (int i = 0; i < productIdsAndAmounts.Length; i += 2)
        {
            var productId = productIdsAndAmounts[i];
            var amount = productIdsAndAmounts[i + 1];
            ingredients.Add(new Ingredient { ProductId = productId, AmountInGrams = amount });
            products.Add(ProductHelper.AllProducts.First(p => p.Id == productId));
        }

        return (ingredients, products);
    }
}
