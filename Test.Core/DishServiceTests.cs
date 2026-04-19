using Core.Interfaces;
using Core.Models;
using Core.Services;
using FluentAssertions;
using Moq;

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

    #region Тесты расчёта КБЖУ - Эквивалентное разбиение и Граничные значения
    
    [Theory]
    [InlineData(0, 0.0, 0.0, 0.0, 0.0)] // 1. Граничное значение: 0 г
    [InlineData(0.1, 0.165, 0.031, 0.0036, 0.0)] // 2. Граничное значение: 0.1 г
    [InlineData(50, 82.5, 15.5, 1.8, 0.0)] // 3. Эквивалентный класс: 50 г
    [InlineData(100, 165.0, 31.0, 3.6, 0.0)] // 4. Граничное значение: 100 г
    [InlineData(250, 412.5, 77.5, 9.0, 0.0)] // 5. Эквивалентный класс: 250 г
    [InlineData(1000, 1650.0, 310.0, 36.0, 0.0)] // 6. Граничное значение: 1000 г
    public async Task CreateDishAsync_CalculatesNutrition_BoundaryValues(
        double amountInGrams, 
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

    // Эквивалентные классы для КБЖУ продукта: 0 (граница), низкая (25), средняя (165), высокая (900), макс (900/50/100/50)
    [Theory]
    [InlineData(0, 0, 0, 0, 100, 0.0, 0.0, 0.0, 0.0)] // Граница: продукт без калорий
    [InlineData(25, 2, 0.5, 4, 100, 25.0, 2.0, 0.5, 4.0)] // Низкая калорийность (овощи)
    [InlineData(165, 31, 3.6, 0, 100, 165.0, 31.0, 3.6, 0.0)] // Средняя калорийность (мясо)
    [InlineData(900, 0, 100, 0, 100, 900.0, 0.0, 100.0, 0.0)] // Высокая калорийность (масло)
    [InlineData(900, 50, 100, 50, 100, 900.0, 50.0, 100.0, 50.0)] // Граница: максимальные значения
    public async Task CreateDishAsync_CalculatesNutrition_DifferentProductTypes(
        double calories,
        double proteins,
        double fats,
        double carbs,
        double amount,
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
            CaloriesPer100g = calories,
            ProteinsPer100g = proteins,
            FatsPer100g = fats,
            CarbsPer100g = carbs
        };

        _productRepository.Setup(r => r.GetByIdsAsync(It.IsAny<List<int>>()))
            .ReturnsAsync(new List<Product> { product });

        var dish = new Dish
        {
            Name = "Test Dish",
            Ingredients = new List<Ingredient>
            {
                new Ingredient { ProductId = 1, AmountInGrams = amount }
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
    }

    // Эквивалентные классы: 1, 3, 5 ингредиентов
    [Theory]
    [InlineData(1)] // 1 ингредиент
    [InlineData(3)] // 3 ингредиента
    [InlineData(5)] // 5 ингредиентов
    public async Task CreateDishAsync_CalculatesNutrition_MultipleIngredients(
        int ingredientCount)
    {
        // Arrange
        var products = new List<Product>
        {
            new Product { Id = 1, Name = "Product 1", CaloriesPer100g = 100, ProteinsPer100g = 10, FatsPer100g = 5, CarbsPer100g = 10 },
            new Product { Id = 2, Name = "Product 2", CaloriesPer100g = 200, ProteinsPer100g = 20, FatsPer100g = 10, CarbsPer100g = 20 },
            new Product { Id = 3, Name = "Product 3", CaloriesPer100g = 300, ProteinsPer100g = 30, FatsPer100g = 15, CarbsPer100g = 30 },
            new Product { Id = 4, Name = "Product 4", CaloriesPer100g = 400, ProteinsPer100g = 40, FatsPer100g = 20, CarbsPer100g = 40 },
            new Product { Id = 5, Name = "Product 5", CaloriesPer100g = 500, ProteinsPer100g = 50, FatsPer100g = 25, CarbsPer100g = 50 }
        };

        _productRepository.Setup(r => r.GetByIdsAsync(It.IsAny<List<int>>()))
            .ReturnsAsync(products);

        var ingredients = new List<Ingredient>();
        double expectedCalories = 0, expectedProteins = 0, expectedFats = 0, expectedCarbs = 0;
        double totalWeight = 0;

        for (int i = 0; i < ingredientCount; i++)
        {
            var amount = 100.0;
            ingredients.Add(new Ingredient { ProductId = i + 1, AmountInGrams = amount });
            
            var product = products[i];
            expectedCalories += product.CaloriesPer100g;
            expectedProteins += product.ProteinsPer100g;
            expectedFats += product.FatsPer100g;
            expectedCarbs += product.CarbsPer100g;
            totalWeight += amount;
        }

        var dish = new Dish
        {
            Name = "Complex Dish",
            Ingredients = ingredients
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
        result.ServingSize.Should().BeApproximately(totalWeight, 0.01);
    }

    // Граничные значения веса: 0.5г, 1.5г, 99.9г
    [Theory]
    [InlineData(0.5, 0.825, 0.155, 0.018, 0.0)] // 0.5 г
    [InlineData(1.5, 2.475, 0.465, 0.054, 0.0)] // 1.5 г
    [InlineData(99.9, 164.835, 30.969, 3.5964, 0.0)] // 99.9 г
    public async Task CreateDishAsync_CalculatesNutrition_FractionalWeights(
        double amountInGrams,
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
    }

    #endregion

    #region Тесты Обработки Ошибок

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
}
