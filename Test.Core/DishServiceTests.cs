using Core.Interfaces;
using Core.Models;
using Core.Models.Enums;
using Core.Services;
using FluentAssertions;
using Moq;
using System.Linq;

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

    // ===== ГРУППА 1: Один продукт (Картошка) с разным количеством (проверка расчёта от веса) =====
    // Стандартная порция (100г)
    // Граница: пустое блюдо (0г)
    // Граница: мало (50г)
    // Граница: много (500г)
    [Theory]
    [InlineData(new int[] { 1, 100 }, 100.0, 2.0, 0.5, 4.0, 100.0)]
    [InlineData(new int[] { 1, 50 }, 50.0, 1.0, 0.25, 2.0, 50.0)]
    [InlineData(new int[] { 1, 500 }, 500.0, 10.0, 2.5, 20.0, 500.0)]
    [InlineData(new int[] { 1, 0 }, 0.0, 0.0, 0.0, 0.0, 0.0)]

    // ===== ГРУППА 2: Несколько продуктов с одинаковым весом =====
    // 2 продукта: Картошка + Курица
    [InlineData(new int[] { 1, 100, 2, 100 }, 265.0, 33.0, 4.1, 4.0, 200.0)]
    // 3 продукта: Картошка + Курица + Масло
    [InlineData(new int[] { 1, 10, 2, 10, 3, 10 }, 116.50, 3.30, 10.41, 0.4, 30.0)]
    // 5 продуктов: Картошка + Курица + Масло + Рис + Томаты
    [InlineData(new int[] { 1, 100, 2, 100, 3, 100, 4, 100, 5, 100 }, 1313.0, 36.6, 104.6, 35.9, 500.0)]
    // Граница: все продукты по 0г (пустое блюдо)
    [InlineData(new int[] { 1, 0, 2, 0, 3, 0 }, 0.0, 0.0, 0.0, 0.0, 0.0)]

    // ===== ГРУППА 3: Несколько продуктов с разным весом =====
    // Сложное блюдо с разными порциями (Картошка 200г + Курица 150г + Масло 10г)
    [InlineData(new int[] { 1, 200, 2, 150, 3, 10 }, 537.5, 50.5, 16.4, 8.0, 360.0)]
    // Большой приём пищи (Картошка 300г + Курица 200г + Рис 150г + Томаты 100г)
    [InlineData(new int[] { 1, 300, 2, 200, 4, 150, 5, 100 }, 843.0, 72.95, 9.35, 57.9, 750.0)]
    // Блюдо с доминированием одного продукта (Масло 50г + Курица 100г + Томаты 20г)
    [InlineData(new int[] { 3, 50, 2, 100, 5, 20 }, 618.6, 31.18, 53.64, 0.78, 170.0)]
    // Граница: один продукт с весом, остальные 0г (Картошка 100г + Курица 0г + Масло 0г)
    [InlineData(new int[] { 1, 100, 2, 0, 3, 0 }, 100.0, 2.0, 0.5, 4.0, 100.0)]

    // ===== ГРУППА 4: Граничные значения КБЖУ (продукты с граничными значениями) =====
    // 0 калорий (Вода)
    [InlineData(new int[] { 6, 100 }, 0.0, 0.0, 0.0, 0.0, 100.0)]
    // Макс жиры (Масло), мало веса (10г)
    [InlineData(new int[] { 7, 10 }, 90.0, 0.0, 10.0, 0.0, 10.0)]
    // Макс белки (Протеин), стандартная порция (30г)
    [InlineData(new int[] { 8, 30 }, 111.0, 30.0, 0.3, 0.0, 30.0)]
    // Макс углеводы (Сахар), мало веса (5г - чайная ложка)
    [InlineData(new int[] { 9, 5 }, 20.0, 0.0, 0.0, 5.0, 5.0)]
    public async Task CreateDishAsync_CalculatesNutritionBasedOnProducts_EqualsToExpectedNutrition(
        int[] productIdsAndAmounts,
        double expectedCalories,
        double expectedProteins,
        double expectedFats,
        double expectedCarbs,
        double expectedServingSize)
    {
        
        var (ingredients, products) = ProductHelper.BuildIngredientsAndProductsByIdsAndAmounts(productIdsAndAmounts);

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
    
    //TODO: сделать тесты на невалдные для создания блюда параметры
}
