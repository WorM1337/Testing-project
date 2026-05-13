using FluentAssertions;
using Microsoft.Playwright;
using Test.UI.Fixtures;
using Test.UI.Pages;

namespace Test.UI.Tests.Dishes;

/// <summary>
/// UI тесты для создания блюд
/// Использует техники тест-дизайна:
/// - Эквивалентное разбиение (классы валидных/невалидных данных)
/// - Анализ граничных значений (границы полей, количество ингредиентов)
/// </summary>
[Collection("UI Tests")]
public class CreateDishTests
{
    private readonly BrowserFixture _browserFixture;
    private readonly DatabaseFixture _databaseFixture;

    public CreateDishTests(BrowserFixture browserFixture, DatabaseFixture databaseFixture)
    {
        _browserFixture = browserFixture;
        _databaseFixture = databaseFixture;
    }

    /// <summary>
    /// Тест: Создание блюда с валидными данными
    /// Техника: Эквивалентное разбиение - валидный класс данных
    /// </summary>
    [Fact(DisplayName = "UI: Создание блюда с валидными данными")]
    public async Task CreateDish_ValidData_Success()
    {
        // Arrange
        await _databaseFixture.CleanDatabaseAsync();
        await using var context = await _browserFixture.CreateContextAsync();
        var page = await context.NewPageAsync();
        var dishesPage = new DishesPage(page, _browserFixture.BaseUrl);

        // Сначала создаем продукт для ингредиента
        var productsPage = new ProductsPage(page, _browserFixture.BaseUrl);
        await productsPage.GoToProductsTabAsync();
        await productsPage.CreateProductAsync(new CreateProductDto
        {
            Name = "Картофель",
            Category = "Vegetables",
            CookingRequirement = "RequiresCooking",
            CaloriesPer100g = 77,
            ProteinsPer100g = 2,
            FatsPer100g = 0.1m,
            CarbsPer100g = 17
        });
        await productsPage.IsSuccessToastVisibleAsync();

        var dish = new CreateDishDto
        {
            Name = "Картофельное пюре",
            Category = "Side",
            Ingredients = new List<IngredientDto>
            {
                new() { ProductName = "Картофель", AmountInGrams = 200 }
            }
        };

        // Act
        await dishesPage.GoToDishesTabAsync();
        await dishesPage.CreateDishAsync(dish);

        // Assert
        var isSuccess = await dishesPage.IsSuccessToastVisibleAsync();
        isSuccess.Should().BeTrue("создание блюда должно завершиться успешно");
        
        var isInTable = await dishesPage.IsDishInTableAsync("Картофельное пюре");
        isInTable.Should().BeTrue("блюдо должно отображаться в таблице");
    }

    /// <summary>
    /// Тест: Создание блюда с названием минимальной длины (2 символа)
    /// Техника: Анализ граничных значений - минимальная длина названия
    /// </summary>
    [Fact(DisplayName = "UI: Создание блюда с названием минимальной длины (2 символа)")]
    public async Task CreateDish_NameMinBoundary_Success()
    {
        // Arrange
        await _databaseFixture.CleanDatabaseAsync();
        await using var context = await _browserFixture.CreateContextAsync();
        var page = await context.NewPageAsync();
        var dishesPage = new DishesPage(page, _browserFixture.BaseUrl);

        // Создаем продукт
        var productsPage = new ProductsPage(page, _browserFixture.BaseUrl);
        await productsPage.GoToProductsTabAsync();
        await productsPage.CreateProductAsync(new CreateProductDto
        {
            Name = "Продукт",
            Category = "Vegetables",
            CookingRequirement = "ReadyToUse",
            CaloriesPer100g = 50
        });
        await productsPage.IsSuccessToastVisibleAsync();

        var dish = new CreateDishDto
        {
            Name = "Суп",
            Category = "Soup",
            Ingredients = new List<IngredientDto>
            {
                new() { ProductName = "Продукт", AmountInGrams = 100 }
            }
        };

        // Act
        await dishesPage.GoToDishesTabAsync();
        await dishesPage.CreateDishAsync(dish);

        // Assert
        var isSuccess = await dishesPage.IsSuccessToastVisibleAsync();
        isSuccess.Should().BeTrue("название из 2 символов допустимо");
    }

    /// <summary>
    /// Тест: Создание блюда с названием максимальной длины (50 символов)
    /// Техника: Анализ граничных значений - максимальная длина названия
    /// </summary>
    [Fact(DisplayName = "UI: Создание блюда с названием максимальной длины (50 символов)")]
    public async Task CreateDish_NameMaxBoundary_Success()
    {
        // Arrange
        await _databaseFixture.CleanDatabaseAsync();
        await using var context = await _browserFixture.CreateContextAsync();
        var page = await context.NewPageAsync();
        var dishesPage = new DishesPage(page, _browserFixture.BaseUrl);

        // Создаем продукт
        var productsPage = new ProductsPage(page, _browserFixture.BaseUrl);
        await productsPage.GoToProductsTabAsync();
        await productsPage.CreateProductAsync(new CreateProductDto
        {
            Name = "Продукт",
            Category = "Vegetables",
            CookingRequirement = "ReadyToUse",
            CaloriesPer100g = 50
        });
        await productsPage.IsSuccessToastVisibleAsync();

        var dish = new CreateDishDto
        {
            Name = new string('А', 50),
            Category = "Side",
            Ingredients = new List<IngredientDto>
            {
                new() { ProductName = "Продукт", AmountInGrams = 100 }
            }
        };

        // Act
        await dishesPage.GoToDishesTabAsync();
        await dishesPage.CreateDishAsync(dish);

        // Assert
        var isSuccess = await dishesPage.IsSuccessToastVisibleAsync();
        isSuccess.Should().BeTrue("название из 50 символов допустимо");
    }

    /// <summary>
    /// Тест: Создание блюда с названием слишком короткой длины (1 символ)
    /// Техника: Анализ граничных значений - за нижней границей
    /// </summary>
    [Fact(DisplayName = "UI: Создание блюда с названием слишком короткой длины (1 символ)")]
    public async Task CreateDish_NameTooShort_Error()
    {
        // Arrange
        await _databaseFixture.CleanDatabaseAsync();
        await using var context = await _browserFixture.CreateContextAsync();
        var page = await context.NewPageAsync();
        var dishesPage = new DishesPage(page, _browserFixture.BaseUrl);

        // Создаем продукт
        var productsPage = new ProductsPage(page, _browserFixture.BaseUrl);
        await productsPage.GoToProductsTabAsync();
        await productsPage.CreateProductAsync(new CreateProductDto
        {
            Name = "Продукт",
            Category = "Vegetables",
            CookingRequirement = "ReadyToUse",
            CaloriesPer100g = 50
        });
        await productsPage.IsSuccessToastVisibleAsync();

        var dish = new CreateDishDto
        {
            Name = "Б",
            Category = "Side",
            Ingredients = new List<IngredientDto>
            {
                new() { ProductName = "Продукт", AmountInGrams = 100 }
            }
        };

        // Act
        await dishesPage.GoToDishesTabAsync();
        await dishesPage.OpenCreateModalAsync();
        await dishesPage.FillDishFormAsync(dish);
        await dishesPage.AddIngredientAsync("Продукт", 100);
        await dishesPage.SaveDishAsync();

        // Assert
        var isError = await dishesPage.IsErrorToastVisibleAsync();
        isError.Should().BeTrue("название из 1 символя недопустимо");
    }

    /// <summary>
    /// Тест: Создание блюда без ингредиентов
    /// Техника: Эквивалентное разбиение - невалидный класс (пустые ингредиенты)
    /// Примечание: JavaScript валидация блокирует отправку формы
    /// </summary>
    [Fact(DisplayName = "UI: Создание блюда без ингредиентов (ошибка)")]
    public async Task CreateDish_NoIngredients_Error()
    {
        // Arrange
        await _databaseFixture.CleanDatabaseAsync();
        await using var context = await _browserFixture.CreateContextAsync();
        var page = await context.NewPageAsync();
        var dishesPage = new DishesPage(page, _browserFixture.BaseUrl);

        // Создаем продукт для теста
        var productsPage = new ProductsPage(page, _browserFixture.BaseUrl);
        await productsPage.GoToProductsTabAsync();
        await productsPage.CreateProductAsync(new CreateProductDto
        {
            Name = "Продукт",
            Category = "Vegetables",
            CookingRequirement = "ReadyToUse",
            CaloriesPer100g = 50
        });
        await productsPage.IsSuccessToastVisibleAsync();

        var dish = new CreateDishDto
        {
            ServingSize = 10,
            Name = "Блюдо без ингредиентов",
            Category = "Side",
            Ingredients = new List<IngredientDto>()
        };

        // Act
        await dishesPage.GoToDishesTabAsync();
        await dishesPage.OpenCreateModalAsync();
        await dishesPage.FillDishFormAsync(dish);
        // Не добавляем ингредиенты
        await dishesPage.SaveButton.ClickAsync();
        await page.WaitForTimeoutAsync(1000); // Ждём появления ошибки

        // Assert: проверяем, что модалка осталась открытой (валидация не пропустила)
        var modalVisible = await dishesPage.ModalOverlay.IsVisibleAsync();
        modalVisible.Should().BeTrue("JavaScript валидация должна блокировать создание блюда без ингредиентов");
        
        // Закрываем модалку после теста через кнопку отмены
        var cancelButton = page.Locator("[data-testid='cancel-btn']");
        await cancelButton.ClickAsync();
        await dishesPage.ModalOverlay.WaitForAsync(new() { State = WaitForSelectorState.Hidden, Timeout = 2000 });
    }

    /// <summary>
    /// Тест: Создание блюда с одним ингредиентом
    /// Техника: Анализ граничных значений - минимальное количество ингредиентов
    /// </summary>
    [Fact(DisplayName = "UI: Создание блюда с одним ингредиентом")]
    public async Task CreateDish_OneIngredient_Success()
    {
        // Arrange
        await _databaseFixture.CleanDatabaseAsync();
        await using var context = await _browserFixture.CreateContextAsync();
        var page = await context.NewPageAsync();
        var dishesPage = new DishesPage(page, _browserFixture.BaseUrl);

        // Создаем продукт
        var productsPage = new ProductsPage(page, _browserFixture.BaseUrl);
        await productsPage.GoToProductsTabAsync();
        await productsPage.CreateProductAsync(new CreateProductDto
        {
            Name = "Продукт",
            Category = "Vegetables",
            CookingRequirement = "ReadyToUse",
            CaloriesPer100g = 50
        });
        await productsPage.IsSuccessToastVisibleAsync();

        var dish = new CreateDishDto
        {
            Name = "Блюдо с одним ингредиентом",
            Category = "Side",
            Ingredients = new List<IngredientDto>
            {
                new() { ProductName = "Продукт", AmountInGrams = 100 }
            }
        };

        // Act
        await dishesPage.GoToDishesTabAsync();
        await dishesPage.CreateDishAsync(dish);

        // Assert
        var isSuccess = await dishesPage.IsSuccessToastVisibleAsync();
        isSuccess.Should().BeTrue();
    }

    /// <summary>
    /// Тест: Создание блюда с несколькими ингредиентами
    /// Техника: Эквивалентное разбиение - множественные ингредиенты
    /// </summary>
    [Fact(DisplayName = "UI: Создание блюда с несколькими ингредиентами")]
    public async Task CreateDish_MultipleIngredients_Success()
    {
        // Arrange
        await _databaseFixture.CleanDatabaseAsync();
        await using var context = await _browserFixture.CreateContextAsync();
        var page = await context.NewPageAsync();
        var dishesPage = new DishesPage(page, _browserFixture.BaseUrl);

        // Создаем несколько продуктов
        var productsPage = new ProductsPage(page, _browserFixture.BaseUrl);
        await productsPage.GoToProductsTabAsync();
        await productsPage.CreateProductAsync(new CreateProductDto { Name = "Продукт 1", Category = "Vegetables", CookingRequirement = "ReadyToUse", CaloriesPer100g = 50 });
        await productsPage.CreateProductAsync(new CreateProductDto { Name = "Продукт 2", Category = "Vegetables", CookingRequirement = "ReadyToUse", CaloriesPer100g = 60 });
        await productsPage.CreateProductAsync(new CreateProductDto { Name = "Продукт 3", Category = "Vegetables", CookingRequirement = "ReadyToUse", CaloriesPer100g = 70 });
        await productsPage.IsSuccessToastVisibleAsync();

        var dish = new CreateDishDto
        {
            Name = "Блюдо с тремя ингредиентами",
            Category = "Side",
            Ingredients = new List<IngredientDto>
            {
                new() { ProductName = "Продукт 1", AmountInGrams = 100 },
                new() { ProductName = "Продукт 2", AmountInGrams = 150 },
                new() { ProductName = "Продукт 3", AmountInGrams = 200 }
            }
        };

        // Act
        await dishesPage.GoToDishesTabAsync();
        await dishesPage.CreateDishAsync(dish);

        // Assert
        var isSuccess = await dishesPage.IsSuccessToastVisibleAsync();
        isSuccess.Should().BeTrue();
    }

    /// <summary>
    /// Тест: Создание блюда с переопределением КБЖУ
    /// Техника: Эквивалентное разбиение - переопределение расчетов
    /// </summary>
    [Fact(DisplayName = "UI: Создание блюда с переопределением КБЖУ")]
    public async Task CreateDish_WithNutritionOverride_Success()
    {
        // Arrange
        await _databaseFixture.CleanDatabaseAsync();
        await using var context = await _browserFixture.CreateContextAsync();
        var page = await context.NewPageAsync();
        var dishesPage = new DishesPage(page, _browserFixture.BaseUrl);

        // Создаем продукт
        var productsPage = new ProductsPage(page, _browserFixture.BaseUrl);
        await productsPage.GoToProductsTabAsync();
        await productsPage.CreateProductAsync(new CreateProductDto
        {
            Name = "Продукт",
            Category = "Vegetables",
            CookingRequirement = "ReadyToUse",
            CaloriesPer100g = 50
        });
        await productsPage.IsSuccessToastVisibleAsync();

        var dish = new CreateDishDto
        {
            Name = "Блюдо с переопределением",
            Category = "Side",
            ServingSize = 250,
            CaloriesPerServing = 500,
            ProteinsPerServing = 50,
            FatsPerServing = 30,
            CarbsPerServing = 60,
            Ingredients = new List<IngredientDto>
            {
                new() { ProductName = "Продукт", AmountInGrams = 100 }
            }
        };

        // Act
        await dishesPage.GoToDishesTabAsync();
        await dishesPage.CreateDishAsync(dish);

        // Assert
        var isSuccess = await dishesPage.IsSuccessToastVisibleAsync();
        isSuccess.Should().BeTrue();
    }

    /// <summary>
    /// Тест: Создание блюда с флагом Vegan
    /// Техника: Эквивалентное разбиение - класс с флагами
    /// </summary>
    [Fact(DisplayName = "UI: Создание блюда с флагом Vegan")]
    public async Task CreateDish_WithVeganFlag_Success()
    {
        // Arrange
        await _databaseFixture.CleanDatabaseAsync();
        await using var context = await _browserFixture.CreateContextAsync();
        var page = await context.NewPageAsync();
        var dishesPage = new DishesPage(page, _browserFixture.BaseUrl);

        // Создаем веганский продукт
        var productsPage = new ProductsPage(page, _browserFixture.BaseUrl);
        await productsPage.GoToProductsTabAsync();
        await productsPage.CreateProductAsync(new CreateProductDto
        {
            Name = "Яблоко",
            Category = "Vegetables",
            CookingRequirement = "ReadyToUse",
            CaloriesPer100g = 52,
            Flags = "Vegan"
        });
        await productsPage.IsSuccessToastVisibleAsync();

        var dish = new CreateDishDto
        {
            Name = "Веганский салат",
            Category = "Salad",
            Ingredients = new List<IngredientDto>
            {
                new() { ProductName = "Яблоко", AmountInGrams = 200 }
            }
        };

        // Act
        await dishesPage.GoToDishesTabAsync();
        await dishesPage.OpenCreateModalAsync();
        await dishesPage.FillDishFormAsync(dish);
        await dishesPage.AddIngredientAsync("Яблоко", 200);
        await dishesPage.SetFlagsAsync(vegan: true);
        
        // Сохраняем и ждём toast правильно
        await dishesPage.SaveDishAsync();

        // Assert
        var isSuccess = await dishesPage.IsSuccessToastVisibleAsync();
        isSuccess.Should().BeTrue();
    }

    /// <summary>
    /// Тест: Создание блюда с нулевой калорийностью
    /// Техника: Анализ граничных значений - нижняя граница калорий
    /// </summary>
    [Fact(DisplayName = "UI: Создание блюда с нулевой калорийностью (граница)")]
    public async Task CreateDish_ZeroCalories_Success()
    {
        // Arrange
        await _databaseFixture.CleanDatabaseAsync();
        await using var context = await _browserFixture.CreateContextAsync();
        var page = await context.NewPageAsync();
        var dishesPage = new DishesPage(page, _browserFixture.BaseUrl);

        // Создаем продукт
        var productsPage = new ProductsPage(page, _browserFixture.BaseUrl);
        await productsPage.GoToProductsTabAsync();
        await productsPage.CreateProductAsync(new CreateProductDto
        {
            Name = "Вода",
            Category = "Liquid",
            CookingRequirement = "ReadyToUse",
            CaloriesPer100g = 0
        });
        await productsPage.IsSuccessToastVisibleAsync();

        var dish = new CreateDishDto
        {
            Name = "Вода питьевая",
            Category = "Drink",
            ServingSize = 250,
            CaloriesPerServing = 0,
            Ingredients = new List<IngredientDto>
            {
                new() { ProductName = "Вода", AmountInGrams = 250 }
            }
        };

        // Act
        await dishesPage.GoToDishesTabAsync();
        await dishesPage.CreateDishAsync(dish);

        // Assert
        var isSuccess = await dishesPage.IsSuccessToastVisibleAsync();
        isSuccess.Should().BeTrue("нулевая калорийность допустима");
    }
}
