using Core.Interfaces;
using Core.Models;
using Core.Services;
using FluentAssertions;
using Moq;
using Test.Core.TestData;

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

    #region Расчет КБЖУ без переопределения

    [Theory(DisplayName = "Создание блюда с корректными продуктами без переопределения - расчет КБЖУ - КБЖУ совпадают с ожидаемыми")]
    [MemberData(nameof(DishNutritionTestData.GetTestCases), MemberType = typeof(DishNutritionTestData))]
    public async Task CreateDishAsync_CalculatesNutritionBasedOnProducts_EqualsToExpectedNutrition(
        List<(Product Product, double Amount)> recipe,
        double expectedCalories,
        double expectedProteins,
        double expectedFats,
        double expectedCarbs,
        double expectedServingSize)
    {
        //Arrange
        var (ingredients, products) = ProductHelper.BuildIngredientsAndProducts(recipe);

        _productRepository.Setup(r => r.GetByIdsAsync(It.IsAny<List<int>>()))
            .ReturnsAsync(products);

        var dish = new Dish
        {
            Name = "Тестовое блюдо",
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
        result.ServingSize.Should().BeApproximately(expectedServingSize, 0.01);
    }

    #endregion

    #region Расчет КБЖУ с переопределением

    #region Переопределения КБЖУ при создании блюда (корректные значения)
    
    [Fact(DisplayName = "Создание блюда с полным переопределением КБЖУ - используются пользовательские значения")]
    public async Task CreateDishAsync_WithFullOverride_UsesUserValues()
    {
        // Arrange: 3 продукта, но все значения переопределены пользователем
        var recipe = new List<(Product, double)>
        {
            (ProductHelper.Potato, 100),
            (ProductHelper.ChickenBreast, 100),
            (ProductHelper.OliveOil, 50)
        };
        var (ingredients, products) = ProductHelper.BuildIngredientsAndProducts(recipe);

        _productRepository.Setup(r => r.GetByIdsAsync(It.IsAny<List<int>>()))
            .ReturnsAsync(products);

        var dish = new Dish
        {
            Name = "Блюдо с переопределением",
            Ingredients = ingredients,
            CaloriesPerServing = 1000,
            ProteinsPerServing = 50,
            FatsPerServing = 30,
            CarbsPerServing = 100,
            ServingSize = 300
        };

        _dishRepository.Setup(r => r.CreateAsync(It.IsAny<Dish>()))
            .ReturnsAsync((Dish d) => d);

        // Act
        var result = await _dishService.CreateDishAsync(dish);

        // Assert - все значения взяты от пользователя, не рассчитаны
        result.CaloriesPerServing.Should().Be(1000);
        result.ProteinsPerServing.Should().Be(50);
        result.FatsPerServing.Should().Be(30);
        result.CarbsPerServing.Should().Be(100);
        result.ServingSize.Should().Be(300);
    }

    [Fact(DisplayName = "Создание блюда с частичным переопределением КБЖУ - не непереопределенные значения рассчитываются автоматически")]
    public async Task CreateDishAsync_WithPartialOverride_CalculatesMissingValues()
    {
        // Arrange: переопределяем только калории и белки, остальное рассчитывается
        var recipe = new List<(Product, double)>
        {
            (ProductHelper.Potato, 100),
            (ProductHelper.ChickenBreast, 100)
        };
        var (ingredients, products) = ProductHelper.BuildIngredientsAndProducts(recipe);

        _productRepository.Setup(r => r.GetByIdsAsync(It.IsAny<List<int>>()))
            .ReturnsAsync(products);

        var dish = new Dish
        {
            Name = "Блюдо с частичным переопределением",
            Ingredients = ingredients,
            CaloriesPerServing = 500, // Переопределяем (должно быть 265)
            ProteinsPerServing = 40   // Переопределяем (должно быть 33)
            // Fats, Carbs, ServingSize - должны рассчитаться автоматически
        };

        _dishRepository.Setup(r => r.CreateAsync(It.IsAny<Dish>()))
            .ReturnsAsync((Dish d) => d);

        // Act
        var result = await _dishService.CreateDishAsync(dish);

        // Assert
        result.CaloriesPerServing.Should().Be(500); // Переопределено
        result.ProteinsPerServing.Should().Be(40);  // Переопределено
        result.FatsPerServing.Should().BeApproximately(4.1, 0.01); // Рассчитано
        result.CarbsPerServing.Should().BeApproximately(4.0, 0.01); // Рассчитано
        result.ServingSize.Should().BeApproximately(200.0, 0.01); // Рассчитано
    }

    [Fact(DisplayName = "Создание блюда с переопределением только размера порции - КБЖУ рассчитывается автоматически")]
    public async Task CreateDishAsync_WithOnlyServingSizeOverride_KeepsNutritionCalculated()
    {
        // Arrange: переопределяем только размер порции
        var recipe = new List<(Product, double)>
        {
            (ProductHelper.Potato, 100)
        };
        var (ingredients, products) = ProductHelper.BuildIngredientsAndProducts(recipe);

        _productRepository.Setup(r => r.GetByIdsAsync(It.IsAny<List<int>>()))
            .ReturnsAsync(products);

        var dish = new Dish
        {
            Name = "Блюдо с переопределением порции",
            Ingredients = ingredients,
            ServingSize = 50 // Переопределяем: 50г вместо 100г
        };

        _dishRepository.Setup(r => r.CreateAsync(It.IsAny<Dish>()))
            .ReturnsAsync((Dish d) => d);

        // Act
        var result = await _dishService.CreateDishAsync(dish);

        // Assert - КБЖУ рассчитано от продуктов, servingSize взята от пользователя
        result.CaloriesPerServing.Should().BeApproximately(100.0, 0.01);
        result.ProteinsPerServing.Should().BeApproximately(2.0, 0.01);
        result.FatsPerServing.Should().BeApproximately(0.5, 0.01);
        result.CarbsPerServing.Should().BeApproximately(4.0, 0.01);
        result.ServingSize.Should().Be(50);
    }
    #endregion

    #region Обновление блюда (корректные значения)
    
    [Fact(DisplayName = "Обновление блюда без явных флагов - все значения КБЖУ пересчитываются автоматически")]
    public async Task UpdateDishAsync_WithoutExplicitFlags_OverridesUserValues()
    {
        // Arrange
        var recipe = new List<(Product, double)>
        {
            (ProductHelper.Potato, 100)
        };
        var (ingredients, products) = ProductHelper.BuildIngredientsAndProducts(recipe);

        _productRepository.Setup(r => r.GetByIdsAsync(It.IsAny<List<int>>()))
            .ReturnsAsync(products);

        var dish = new Dish
        {
            Id = 1,
            Name = "Обновляемое блюдо",
            Ingredients = ingredients,
            CaloriesPerServing = 500,
            ProteinsPerServing = 10,
            FatsPerServing = 5,
            CarbsPerServing = 20,
            ServingSize = 150
        };

        _dishRepository.Setup(r => r.UpdateAsync(It.IsAny<Dish>()))
            .Returns(Task.CompletedTask);

        // Act: все флаги false - значения должны пересчитаться
        await _dishService.UpdateDishAsync(
            dish,
            caloriesWasExplicitlySet: false,
            proteinsWasExplicitlySet: false,
            fatsWasExplicitlySet: false,
            carbsWasExplicitlySet: false,
            servingSizeWasExplicitlySet: false);

        // Assert - значения пересчитаны автоматически
        dish.CaloriesPerServing.Should().BeApproximately(100.0, 0.01);
        dish.ProteinsPerServing.Should().BeApproximately(2.0, 0.01);
        dish.FatsPerServing.Should().BeApproximately(0.5, 0.01);
        dish.CarbsPerServing.Should().BeApproximately(4.0, 0.01);
        dish.ServingSize.Should().BeApproximately(100.0, 0.01);
    }

    [Fact(DisplayName = "Обновление блюда с явными флагами 'Было явно переопределено' равными true - все пользовательские значения КБЖУ сохраняются")]
    public async Task UpdateDishAsync_WithExplicitFlagsTrue_KeepsUserValues()
    {
        // Arrange
        var recipe = new List<(Product, double)>
        {
            (ProductHelper.Potato, 100)
        };
        var (ingredients, products) = ProductHelper.BuildIngredientsAndProducts(recipe);

        _productRepository.Setup(r => r.GetByIdsAsync(It.IsAny<List<int>>()))
            .ReturnsAsync(products);

        var dish = new Dish
        {
            Id = 1,
            Name = "Обновляемое блюдо",
            Ingredients = ingredients,
            CaloriesPerServing = 500,
            ProteinsPerServing = 10,
            FatsPerServing = 5,
            CarbsPerServing = 20,
            ServingSize = 150
        };

        _dishRepository.Setup(r => r.UpdateAsync(It.IsAny<Dish>()))
            .Returns(Task.CompletedTask);

        // Act: все флаги true - значения должны сохраниться
        await _dishService.UpdateDishAsync(
            dish,
            caloriesWasExplicitlySet: true,
            proteinsWasExplicitlySet: true,
            fatsWasExplicitlySet: true,
            carbsWasExplicitlySet: true,
            servingSizeWasExplicitlySet: true);

        // Assert - значения сохранены пользователем
        dish.CaloriesPerServing.Should().Be(500);
        dish.ProteinsPerServing.Should().Be(10);
        dish.FatsPerServing.Should().Be(5);
        dish.CarbsPerServing.Should().Be(20);
        dish.ServingSize.Should().Be(150);
    }

    [Fact(DisplayName = "Обновление блюда с частичными явными флагами - только непереопределенные значения пересчитываются")]
    public async Task UpdateDishAsync_WithPartialExplicitFlags_OverridesOnlyMissing()
    {
        // Arrange
        var recipe = new List<(Product, double)>
        {
            (ProductHelper.Potato, 100),
            (ProductHelper.ChickenBreast, 100)
        };
        var (ingredients, products) = ProductHelper.BuildIngredientsAndProducts(recipe);

        _productRepository.Setup(r => r.GetByIdsAsync(It.IsAny<List<int>>()))
            .ReturnsAsync(products);

        var dish = new Dish
        {
            Id = 1,
            Name = "Обновляемое блюдо",
            Ingredients = ingredients,
            CaloriesPerServing = 500, // Пользователь установил
            ProteinsPerServing = 10,  // Пользователь установил
            FatsPerServing = 5,       // Пользователь установил
            CarbsPerServing = 20,     // Пользователь установил
            ServingSize = 150         // Пользователь установил
        };

        _dishRepository.Setup(r => r.UpdateAsync(It.IsAny<Dish>()))
            .Returns(Task.CompletedTask);

        // Act: только калории явно установлены, остальное пересчитывается
        await _dishService.UpdateDishAsync(
            dish,
            caloriesWasExplicitlySet: true,
            proteinsWasExplicitlySet: false,
            fatsWasExplicitlySet: false,
            carbsWasExplicitlySet: false,
            servingSizeWasExplicitlySet: false);

        // Assert - калории сохранены, остальное пересчитано
        dish.CaloriesPerServing.Should().Be(500); // Сохранено
        dish.ProteinsPerServing.Should().BeApproximately(33.0, 0.01); // Пересчитано
        dish.FatsPerServing.Should().BeApproximately(4.1, 0.01); // Пересчитано
        dish.CarbsPerServing.Should().BeApproximately(4.0, 0.01); // Пересчитано
        dish.ServingSize.Should().BeApproximately(200.0, 0.01); // Пересчитано
    }    

    #endregion

    #endregion
}