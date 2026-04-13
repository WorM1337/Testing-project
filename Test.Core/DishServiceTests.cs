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
    /// Проверяем, что флаг "Веган" валидируется правильно, когда все продукты веганские.
    /// </summary>
    [Fact]
    public async Task CreateDishAsync_ValidatesVeganFlag_AllProductsVegan()
    {
        // Arrange
        var products = new List<Product>
        {
            new Product { Id = 1, Name = "Product 1", Flags = ExtraFlag.Vegan },
            new Product { Id = 2, Name = "Product 2", Flags = ExtraFlag.Vegan }
        };

        _productRepository.Setup(r => r.GetByIdsAsync(It.IsAny<List<int>>()))
            .ReturnsAsync(products);

        var dish = new Dish
        {
            Name = "Vegan Dish",
            Flags = ExtraFlag.Vegan, // Пользователь установил флаг
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
        result.Flags.Should().HaveFlag(ExtraFlag.Vegan); // Флаг сохранился
    }

    /// <summary>
    /// Проверяем, что флаг "Веган" не может быть установлен, если не все продукты веганские.
    /// </summary>
    [Fact]
    public async Task CreateDishAsync_Throws_WhenVeganFlagButNonVeganProducts()
    {
        // Arrange
        var products = new List<Product>
        {
            new Product { Id = 1, Name = "Vegan Product", Flags = ExtraFlag.Vegan },
            new Product { Id = 2, Name = "Meat Product", Flags = ExtraFlag.None }
        };

        _productRepository.Setup(r => r.GetByIdsAsync(It.IsAny<List<int>>()))
            .ReturnsAsync(products);

        var dish = new Dish
        {
            Name = "Not Vegan Dish",
            Flags = ExtraFlag.Vegan, // Пользователь пытается установить флаг
            Ingredients = new List<Ingredient>
            {
                new Ingredient { ProductId = 1, AmountInGrams = 100 },
                new Ingredient { ProductId = 2, AmountInGrams = 100 }
            }
        };

        _dishRepository.Setup(r => r.CreateAsync(It.IsAny<Dish>()))
            .ReturnsAsync((Dish d) => d);

        // Act
        var action = () => _dishService.CreateDishAsync(dish);

        // Assert
        await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Веган*");
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

    /// <summary>
    /// Проверяем, что при нескольких макросах применяется только первый, а все макросы удаляются.
    /// </summary>
    [Theory]
    [InlineData("!десерт !суп Cake", DishCategory.Dessert, "Cake")]
    [InlineData("!салат !десерт !перекус Salad Bowl", DishCategory.Salad, "Salad Bowl")]
    [InlineData("!напиток !второе !десерт Tea", DishCategory.Drink, "Tea")]
    [InlineData("!перекус!десерт Snack", DishCategory.Snack, "Snack")]
    public async Task CreateDishAsync_MultipleMacros_UsesFirstAndRemovesAll(
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

    /// <summary>
    /// Проверяем, что выбрасывается исключение, если название слишком короткое после удаления макросов.
    /// </summary>
    [Theory]
    [InlineData("!десерт A")]
    [InlineData("!суп X")]
    [InlineData("!салат !десерт Q")]
    public async Task CreateDishAsync_Throws_WhenNameTooShortAfterMacros(string inputName)
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

        // Act & Assert
        var act = async () => await _dishService.CreateDishAsync(dish);
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*слишком короткое*");
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
            Flags = ExtraFlag.None // Флаги не установлены (пользователь не указал)
        };

        _dishRepository.Setup(r => r.UpdateAsync(It.IsAny<Dish>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _dishService.UpdateDishAsync(dish, categoryWasExplicitlySet: false);

        // Assert
        result.CaloriesPerServing.Should().BeApproximately(200.0, 0.01);
        // Флаги остаются как установил пользователь (в данном случае None)
        result.Flags.Should().Be(ExtraFlag.None);
        
        _dishRepository.Verify(r => r.UpdateAsync(dish), Times.Once);
    }

    /// <summary>
    /// Проверяем, что при обновлении флаг "Веган" сохраняется, если все продукты веганские.
    /// </summary>
    [Fact]
    public async Task UpdateDishAsync_RecalculatesFlags_AllProductsVegan()
    {
        // Arrange
        var products = new List<Product>
        {
            new Product { Id = 1, Name = "Vegan Product", Flags = ExtraFlag.Vegan }
        };

        _productRepository.Setup(r => r.GetByIdsAsync(It.IsAny<List<int>>()))
            .ReturnsAsync(products);

        var dish = new Dish
        {
            Id = 1,
            Name = "Vegan Dish",
            Ingredients = new List<Ingredient>
            {
                new Ingredient { ProductId = 1, AmountInGrams = 100 }
            },
            Flags = ExtraFlag.Vegan // Пользователь установил флаг
        };

        _dishRepository.Setup(r => r.UpdateAsync(It.IsAny<Dish>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _dishService.UpdateDishAsync(dish, categoryWasExplicitlySet: false);

        // Assert
        result.Flags.Should().HaveFlag(ExtraFlag.Vegan); // Флаг сохранился
    }

    /// <summary>
    /// Проверяем, что при обновлении флаг "Веган" автоматически снимается, если продукты не веганские.
    /// </summary>
    [Fact]
    public async Task UpdateDishAsync_RecalculatesFlags_RemovesInvalidVeganFlag()
    {
        // Arrange
        var products = new List<Product>
        {
            new Product { Id = 1, Name = "Vegan Product", Flags = ExtraFlag.Vegan },
            new Product { Id = 2, Name = "Meat Product", Flags = ExtraFlag.None }
        };

        _productRepository.Setup(r => r.GetByIdsAsync(It.IsAny<List<int>>()))
            .ReturnsAsync(products);

        var dish = new Dish
        {
            Id = 1,
            Name = "Was Vegan Dish",
            Ingredients = new List<Ingredient>
            {
                new Ingredient { ProductId = 1, AmountInGrams = 100 },
                new Ingredient { ProductId = 2, AmountInGrams = 100 }
            },
            Flags = ExtraFlag.Vegan // Ранее был установлен флаг
        };

        _dishRepository.Setup(r => r.UpdateAsync(It.IsAny<Dish>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _dishService.UpdateDishAsync(dish, categoryWasExplicitlySet: false);

        // Assert
        result.Flags.Should().NotHaveFlag(ExtraFlag.Vegan); // Флаг снят автоматически
    }

    /// <summary>
    /// Проверяем, что макросы работают при обновлении блюда
    /// </summary>
    [Theory]
    [InlineData("!десерт Cake", DishCategory.Dessert, "Cake")]
    [InlineData("!суп Borscht", DishCategory.Soup, "Borscht")]
    [InlineData("!салат !десерт Salad", DishCategory.Salad, "Salad")]
    public async Task UpdateDishAsync_ParseMacros_UpdatesCategory(
        string inputName,
        DishCategory expectedCategory,
        string expectedCleanName)
    {
        // Arrange
        _productRepository.Setup(r => r.GetByIdsAsync(It.IsAny<List<int>>()))
            .ReturnsAsync(new List<Product>());

        var dish = new Dish
        {
            Id = 1,
            Name = inputName,
            Category = DishCategory.None,
            Ingredients = new List<Ingredient>()
        };

        _dishRepository.Setup(r => r.UpdateAsync(It.IsAny<Dish>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _dishService.UpdateDishAsync(dish, categoryWasExplicitlySet: false);

        // Assert
        result.Category.Should().Be(expectedCategory);
        result.Name.Should().Be(expectedCleanName);
    }

    /// <summary>
    /// Проверяем, что при явном указании категории макросы не переопределяют её
    /// </summary>
    [Fact]
    public async Task UpdateDishAsync_ExplicitCategory_TakesPrecedenceOverMacros()
    {
        // Arrange
        _productRepository.Setup(r => r.GetByIdsAsync(It.IsAny<List<int>>()))
            .ReturnsAsync(new List<Product>());

        var dish = new Dish
        {
            Id = 1,
            Name = "!десерт Soup",
            Category = DishCategory.Soup, // Явно установлена категория
            Ingredients = new List<Ingredient>()
        };

        _dishRepository.Setup(r => r.UpdateAsync(It.IsAny<Dish>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _dishService.UpdateDishAsync(dish, categoryWasExplicitlySet: true);

        // Assert
        result.Category.Should().Be(DishCategory.Soup); // Остаётся Soup, а не Dessert
        result.Name.Should().Be("Soup");
    }

    #endregion
}
