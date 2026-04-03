using Core.Interfaces;
using Core.Models;
using Core.Models.Enums;
using Core.Services;
using FluentAssertions;
using Moq;

namespace Test.Core;

public class DishServiceTests
{
    private readonly Mock<IDishRepository> _dishRepoMock;
    private readonly Mock<IProductRepository> _productRepoMock;
    private readonly DishService _dishService;

    public DishServiceTests()
    {
        _dishRepoMock = new Mock<IDishRepository>();
        _productRepoMock = new Mock<IProductRepository>();
        _dishService = new DishService(_dishRepoMock.Object, _productRepoMock.Object);
    }

    [Fact]
    public async Task CreateDishAsync_CalculatesNutritionCorrectly()
    {
        // Arrange
        var product1 = new Product
        {
            Id = 1,
            Name = "Chicken",
            CaloriesPer100g = 165,
            ProteinsPer100g = 31,
            FatsPer100g = 3.6,
            CarbsPer100g = 0
        };

        var product2 = new Product
        {
            Id = 2,
            Name = "Rice",
            CaloriesPer100g = 130,
            ProteinsPer100g = 2.7,
            FatsPer100g = 0.3,
            CarbsPer100g = 28
        };

        var products = new List<Product> { product1, product2 };
        
        // Настраиваем мок репозитория продуктов
        _productRepoMock.Setup(r => r.GetByIdsAsync(It.IsAny<List<int>>()))
            .ReturnsAsync(products);

        var dish = new Dish
        {
            Name = "Chicken Rice Bowl",
            Ingredients = new List<Ingredient>
            {
                new Ingredient { ProductId = 1, AmountInGrams = 200 }, // 200g курицы
                new Ingredient { ProductId = 2, AmountInGrams = 150 }  // 150g риса
            }
        };

        // Ожидаемые значения:
        // Курица (200г): Ккал = 165*2 = 330, Белки = 31*2 = 62, Жиры = 3.6*2 = 7.2, Углеводы = 0
        // Рис (150г):   Ккал = 130*1.5 = 195, Белки = 2.7*1.5 = 4.05, Жиры = 0.3*1.5 = 0.45, Углеводы = 28*1.5 = 42
        // Итого: Ккал = 525, Белки = 66.05, Жиры = 7.65, Углеводы = 42, Вес = 350
        double expectedCalories = 525.0;
        double expectedProteins = 66.05;
        double expectedFats = 7.65;
        double expectedCarbs = 42.0;
        double expectedWeight = 350.0;

        // Mock для сохранения (просто возвращает тот же объект)
        _dishRepoMock.Setup(r => r.CreateAsync(It.IsAny<Dish>()))
            .ReturnsAsync((Dish d) => d);

        // Act
        var result = await _dishService.CreateDishAsync(dish);

        // Assert
        result.CaloriesPerServing.Should().BeApproximately(expectedCalories, 0.01);
        result.ProteinsPerServing.Should().BeApproximately(expectedProteins, 0.01);
        result.FatsPerServing.Should().BeApproximately(expectedFats, 0.01);
        result.CarbsPerServing.Should().BeApproximately(expectedCarbs, 0.01);
        result.ServingSize.Should().BeApproximately(expectedWeight, 0.01);
    }

    [Fact]
    public async Task CreateDishAsync_SetsVeganFlag_WhenAllIngredientsAreVegan()
    {
        // Arrange
        var veganProduct = new Product
        {
            Id = 1,
            Name = "Apple",
            Flags = ExtraFlag.Vegan | ExtraFlag.GlutenFree | ExtraFlag.SugarFree
        };

        _productRepoMock.Setup(r => r.GetByIdsAsync(It.IsAny<List<int>>()))
            .ReturnsAsync(new List<Product> { veganProduct });

        var dish = new Dish
        {
            Name = "Apple Salad",
            Flags = ExtraFlag.None,
            Ingredients = new List<Ingredient>
            {
                new Ingredient { ProductId = 1, AmountInGrams = 100 }
            }
        };

        _dishRepoMock.Setup(r => r.CreateAsync(It.IsAny<Dish>()))
            .ReturnsAsync((Dish d) => d);

        // Act
        var result = await _dishService.CreateDishAsync(dish);

        // Assert
        result.Flags.Should().HaveFlag(ExtraFlag.Vegan);
        result.Flags.Should().HaveFlag(ExtraFlag.GlutenFree);
        result.Flags.Should().HaveFlag(ExtraFlag.SugarFree);
    }

    [Fact]
    public async Task CreateDishAsync_RemovesVeganFlag_WhenOneIngredientIsNotVegan()
    {
        // Arrange
        var veganProduct = new Product
        {
            Id = 1,
            Name = "Lettuce",
            Flags = ExtraFlag.Vegan
        };

        var meatProduct = new Product
        {
            Id = 2,
            Name = "Beef",
            Flags = ExtraFlag.None // Не веганский
        };

        _productRepoMock.Setup(r => r.GetByIdsAsync(It.IsAny<List<int>>()))
            .ReturnsAsync(new List<Product> { veganProduct, meatProduct });

        var dish = new Dish
        {
            Name = "Salad with Beef",
            Flags = ExtraFlag.Vegan, // Изначально стоит флаг, но должен сняться
            Ingredients = new List<Ingredient>
            {
                new Ingredient { ProductId = 1, AmountInGrams = 100 },
                new Ingredient { ProductId = 2, AmountInGrams = 100 }
            }
        };

        _dishRepoMock.Setup(r => r.CreateAsync(It.IsAny<Dish>()))
            .ReturnsAsync((Dish d) => d);

        // Act
        var result = await _dishService.CreateDishAsync(dish);

        // Assert
        result.Flags.Should().NotHaveFlag(ExtraFlag.Vegan);
    }
    
    [Fact]
    public async Task CreateDishAsync_ThrowsException_WhenProductNotFound()
    {
        // Arrange
        // Возвращаем пустой список, хотя запрос был на ID
        _productRepoMock.Setup(r => r.GetByIdsAsync(It.IsAny<List<int>>()))
            .ReturnsAsync(new List<Product>()); 

        var dish = new Dish
        {
            Name = "Impossible Dish",
            Ingredients = new List<Ingredient>
            {
                new Ingredient { ProductId = 999, AmountInGrams = 100 }
            }
        };

        // Act
        var action = async () => await _dishService.CreateDishAsync(dish); 
        
        // Assert
        await action.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task CreateDishAsync_AutoSetsCategory_FromName()
    {
        // Arrange
        _productRepoMock.Setup(r => r.GetByIdsAsync(It.IsAny<List<int>>()))
            .ReturnsAsync(new List<Product>()); // Ингредиенты не важны для этого теста

        var dish = new Dish
        {
            Name = "!десерт Cake", // Парсер должен вытащить Dessert
            Category = DishCategory.None,
            Ingredients = new List<Ingredient>()
        };

        _dishRepoMock.Setup(r => r.CreateAsync(It.IsAny<Dish>()))
            .ReturnsAsync((Dish d) => d);

        // Act
        var result = await _dishService.CreateDishAsync(dish);

        // Assert
        result.Category.Should().Be(DishCategory.Dessert);
        result.Name.Should().Be("Cake");
    }
    
    [Fact]
    public async Task UpdateDishAsync_RecalculatesNutrition()
    {
        // Arrange
        var product = new Product
        {
            Id = 1,
            Name = "Sugar",
            CaloriesPer100g = 400,
            ProteinsPer100g = 0,
            FatsPer100g = 0,
            CarbsPer100g = 100,
            Flags = ExtraFlag.None
        };

        _productRepoMock.Setup(r => r.GetByIdsAsync(It.IsAny<List<int>>()))
            .ReturnsAsync(new List<Product> { product });

        var dish = new Dish
        {
            Id = 1,
            Name = "Sweet Tea",
            Ingredients = new List<Ingredient>
            {
                new Ingredient { ProductId = 1, AmountInGrams = 50 } // 50г сахара
            },
            // Старые данные (неверные), которые должны пересчитаться
            CaloriesPerServing = 0,
            Flags = ExtraFlag.Vegan 
        };

        _dishRepoMock.Setup(r => r.UpdateAsync(It.IsAny<Dish>()))
            .Returns(Task.CompletedTask); // UpdateAsync обычно void или Task

        // Act
        var result = await _dishService.UpdateDishAsync(dish);

        // Assert
        // 50г сахара = 200 ккал
        result.CaloriesPerServing.Should().BeApproximately(200.0, 0.01);
        // Флаг Vegan должен сняться, так как сахар (в нашем примере) не имеет флага Vegan (или имеет, зависит от данных)
        // В данном случае у продукта Flags = None, значит флаг Vegan должен сняться с блюда
        result.Flags.Should().NotHaveFlag(ExtraFlag.Vegan);
    }
}