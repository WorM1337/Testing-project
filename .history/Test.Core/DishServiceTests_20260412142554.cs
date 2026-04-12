using Core.Interfaces;
using Core.Models;
using Core.Models.Enums;
using Core.Services;
using FluentAssertions;
using Moq;
using Xunit;

namespace Test.Core;

public class DishServiceTests
{
    private readonly Mock<IDishRepository> _dishRepository;
    private readonly Mock<IProductRepository> _productRepository;
    private readonly DishService _dishService;

    public DishServiceTests()
    {
        _dishRepository = new Mock<IDishRepository>();
        _productRepository = new Mock<IProductRepository>();
        _dishService = new DishService(_dishRepository.Object, _productRepository.Object);
    }

    #region Тесты расчёта КБЖУ

    /// <summary>
    /// Проверяем расчет нутриентов на граничных значениях веса ингредиента.
    /// Эквивалентные классы: 0, малый вес (>0), стандартный вес (100), большой вес.
    /// </summary>
    [Theory]
    [InlineData(0, 0.0, 0.0, 0.0, 0.0)]
    [InlineData(1, 1.65, 0.31, 0.036, 0.0)]
    [InlineData(100, 165.0, 31.0, 3.6, 0.0)]
    [InlineData(250, 412.5, 77.5, 9.0, 0.0)]
    public async Task CreateDishAsync_CalculatesNutrition_BoundaryValues(
        int amountInGrams, 
        double expectedCalories, 
        double expectedProteins, 
        double expectedFats, 
        double expectedCarbs)
    {
        // Arrange
        var product = new Product
        {
            Id = 1,
            Name = "Test Product",
            CaloriesPer100g = 165,
            ProteinsPer100g = 31,
            FatsPer100g = 3.6,
            CarbsPer100g = 0
        };

        _productRepository.Setup(r => r.GetByIdsAsync(It.IsAny<List<int>>()))
            .ReturnsAsync(new List<Product> { product });

        var dish = new Dish
        {
            Name = "Test Dish",
            Ingredients = new List<Ingredient>
            {
                new Ingredient { ProductId = 1, AmountInGrams = amountInGrams }
            }
        };

        _dishRepository.Setup(r => r.CreateAsync(It.IsAny<Dish>()))
            .ReturnsAsync((Dish d) => d);

        // Act
        var result = await _dishService.CreateDishAsync(dish);

        // Assert
        result.CaloriesPerServing.Should().BeApproximately(expectedCalories, 0.01);
        result.ProteinsPerServing.Should().BeApproximately(expectedProteins, 0.01);
        result.FatsPerServing.Should().BeApproximately(expectedFats, 0.01);
        result.CarbsPerServing.Should().BeApproximately(expectedCarbs, 0.01);
        result.ServingSize.Should().BeApproximately(amountInGrams, 0.01);
    }

    #endregion

    #region Тесты Флагов

    /// <summary>
    /// Проверяем логику флагов на представителях разных классов эквивалентности.
    /// </summary>
    [Theory]
    [InlineData(true, true, true)] 
    [InlineData(true, false, false)] 
    [InlineData(false, true, false)] 
    [InlineData(false, false, false)] 
    public async Task CreateDishAsync_SetsFlags_EquivalenceClasses(
        bool firstIsVegan, 
        bool secondIsVegan, 
        bool expectedHasVeganFlag)
    {
        // Arrange
        var products = new List<Product>
        {
            new Product 
            { 
                Id = 1, 
                Name = "Product 1", 
                Flags = firstIsVegan ? ExtraFlag.Vegan : ExtraFlag.None 
            },
            new Product 
            { 
                Id = 2, 
                Name = "Product 2", 
                Flags = secondIsVegan ? ExtraFlag.Vegan : ExtraFlag.None 
            }
        };

        _productRepository.Setup(r => r.GetByIdsAsync(It.IsAny<List<int>>()))
            .ReturnsAsync(products);

        var dish = new Dish
        {
            Name = "Test Dish",
            Flags = ExtraFlag.None,
            Ingredients = new List<Ingredient>
            {
                new Ingredient { ProductId = 1, AmountInGrams = 100 },
                new Ingredient { ProductId = 2, AmountInGrams = 100 }
            }
        };

        _dishRepository.Setup(r => r.CreateAsync(It.IsAny<Dish>()))
            .ReturnsAsync((Dish d) => d);

        // Act
        var result = await _dishService.CreateDishAsync(dish);

        // Assert
        if (expectedHasVeganFlag)
        {
            result.Flags.Should().HaveFlag(ExtraFlag.Vegan);
        }
        else
        {
            result.Flags.Should().NotHaveFlag(ExtraFlag.Vegan);
        }
    }

    #endregion

    #region Тесты Обработки Ошибок (Граничные условия данных)

    [Fact]
    public async Task CreateDishAsync_Throws_WhenProductMissing()
    {
        // Arrange
        _productRepository.Setup(r => r.GetByIdsAsync(It.IsAny<List<int>>()))
            .ReturnsAsync(new List<Product>());

        var dish = new Dish
        {
            Name = "Impossible Dish",
            Ingredients = new List<Ingredient>
            {
                new Ingredient { ProductId = 999, AmountInGrams = 100 }
            }
        };

        _dishRepository.Setup(r => r.CreateAsync(It.IsAny<Dish>()))
            .ReturnsAsync((Dish d) => d);

        // Act
        var action = () => _dishService.CreateDishAsync(dish);
        
        // Assert
        await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*не найден*");
    }

    #endregion

    #region Тесты Парсинга Категории

    [Theory]
    [InlineData("!десерт Cake", DishCategory.Dessert, "Cake")]
    [InlineData("!суп Borscht", DishCategory.Soup, "Borscht")]
    [InlineData("Just Cake", DishCategory.None, "Just Cake")]
    [InlineData("!unknown Food", DishCategory.None, "!unknown Food")] 
    public async Task CreateDishAsync_ParseCategory_EquivalenceClasses(
        string inputName,
        DishCategory expectedCategory,
        string expectedCleanName)
    {
        // Arrange
        _productRepository.Setup(r => r.GetByIdsAsync(It.IsAny<List<int>>()))
            .ReturnsAsync(new List<Product>());

        var dish = new Dish
        {
            Name = inputName,
            Category = DishCategory.None,
            Ingredients = new List<Ingredient>()
        };

        _dishRepository.Setup(r => r.CreateAsync(It.IsAny<Dish>()))
            .ReturnsAsync((Dish d) => d);

        // Act
        var result = await _dishService.CreateDishAsync(dish);

        // Assert
        result.Category.Should().Be(expectedCategory);
        result.Name.Should().Be(expectedCleanName);
    }

    #endregion

    #region Тесты Обновления

    [Fact]
    public async Task UpdateDishAsync_Recalculates_WhenIngredientsChange()
    {
        // Arrange
        var product = new Product
        {
            Id = 1,
            Name = "Sugar",
            CaloriesPer100g = 400,
            Flags = ExtraFlag.None
        };

        _productRepository.Setup(r => r.GetByIdsAsync(It.IsAny<List<int>>()))
            .ReturnsAsync(new List<Product> { product });

        var dish = new Dish
        {
            Id = 1,
            Name = "Tea",
            Ingredients = new List<Ingredient>
            {
                new Ingredient { ProductId = 1, AmountInGrams = 50 }
            },
            CaloriesPerServing = null, // null означает "рассчитать автоматически"
            Flags = ExtraFlag.Vegan
        };

        _dishRepository.Setup(r => r.UpdateAsync(It.IsAny<Dish>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _dishService.UpdateDishAsync(dish);

        // Assert
        result.CaloriesPerServing.Should().BeApproximately(200.0, 0.01);
        result.Flags.Should().NotHaveFlag(ExtraFlag.Vegan);
        
        _dishRepository.Verify(r => r.UpdateAsync(dish), Times.Once);
    }

    #endregion
}
