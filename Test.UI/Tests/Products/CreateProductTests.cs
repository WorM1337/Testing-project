using FluentAssertions;
using Microsoft.Playwright;
using Test.UI.Fixtures;
using Test.UI.Pages;

namespace Test.UI.Tests.Products;

/// <summary>
/// UI тесты для создания продуктов
/// Использует техники тест-дизайна:
/// - Эквивалентное разбиение (разбиение входных данных на классы)
/// - Анализ граничных значений (тестирование на границах диапазонов)
/// </summary>
[Collection("UI Tests")]
public class CreateProductTests : UiTestBase
{
    public CreateProductTests(BrowserFixture browserFixture, DatabaseFixture databaseFixture)
        : base(browserFixture, databaseFixture)
    {
    }

    /// <summary>
    /// Тест: Создание продукта с валидными данными
    /// Техника: Эквивалентное разбиение - валидный класс данных
    /// </summary>
    [Fact(DisplayName = "UI: Создание продукта с валидными данными")]
    public async Task CreateProduct_ValidData_Success()
    {
        // Arrange
        var (page, _, productsPage) = await InitializeTestAsync();

        var product = new CreateProductDto
        {
            Name = "Картофель",
            Category = "Vegetables",
            CookingRequirement = "RequiresCooking",
            CaloriesPer100g = 77,
            ProteinsPer100g = 2,
            FatsPer100g = 0.1m,
            CarbsPer100g = 17
        };

        // Act
        await productsPage.GoToProductsTabAsync();
        await productsPage.CreateProductAsync(product);

        // Assert
        var isSuccess = await productsPage.IsSuccessToastVisibleAsync();
        isSuccess.Should().BeTrue("создание продукта должно завершиться успешно");
        
        var isInTable = await productsPage.IsProductInTableAsync("Картофель");
        isInTable.Should().BeTrue("продукт должен отображаться в таблице");
    }

    /// <summary>
    /// Тест: Создание продукта с названием на нижней границе (2 символа)
    /// Техника: Анализ граничных значений - минимальная длина названия
    /// </summary>
    [Fact(DisplayName = "UI: Создание продукта с названием минимальной длины (2 символа)")]
    public async Task CreateProduct_NameMinBoundary_Success()
    {
        // Arrange
        var (page, _, productsPage) = await InitializeTestAsync();

        var product = new CreateProductDto
        {
            Name = "Ай",
            Category = "Vegetables",
            CookingRequirement = "ReadyToUse",
            CaloriesPer100g = 50,
            ProteinsPer100g = 1,
            FatsPer100g = 0,
            CarbsPer100g = 10
        };

        // Act
        await productsPage.GoToProductsTabAsync();
        await productsPage.CreateProductAsync(product);

        // Assert
        var isSuccess = await productsPage.IsSuccessToastVisibleAsync();
        isSuccess.Should().BeTrue("название из 2 символов допустимо");
    }

    /// <summary>
    /// Тест: Создание продукта с названием на верхней границе (50 символов)
    /// Техника: Анализ граничных значений - максимальная длина названия
    /// </summary>
    [Fact(DisplayName = "UI: Создание продукта с названием максимальной длины (50 символов)")]
    public async Task CreateProduct_NameMaxBoundary_Success()
    {
        // Arrange
        var (page, _, productsPage) = await InitializeTestAsync();

        var product = new CreateProductDto
        {
            Name = new string('А', 50),
            Category = "Vegetables",
            CookingRequirement = "ReadyToUse",
            CaloriesPer100g = 50,
            ProteinsPer100g = 1,
            FatsPer100g = 0,
            CarbsPer100g = 10
        };

        // Act
        await productsPage.GoToProductsTabAsync();
        await productsPage.CreateProductAsync(product);

        // Assert
        var isSuccess = await productsPage.IsSuccessToastVisibleAsync();
        isSuccess.Should().BeTrue("название из 50 символов допустимо");
    }

    /// <summary>
    /// Тест: Создание продукта с названием ниже нижней границы (1 символ)
    /// Техника: Анализ граничных значений - название слишком короткое
    /// </summary>
    [Fact(DisplayName = "UI: Создание продукта с названием ниже минимальной длины (1 символ)")]
    public async Task CreateProduct_NameBelowMinBoundary_Error()
    {
        // Arrange
        var (page, _, productsPage) = await InitializeTestAsync();

        var product = new CreateProductDto
        {
            Name = "А",
            Category = "Vegetables",
            CookingRequirement = "ReadyToUse",
            CaloriesPer100g = 50,
            ProteinsPer100g = 1,
            FatsPer100g = 0,
            CarbsPer100g = 10
        };

        // Act
        await productsPage.GoToProductsTabAsync();
        await productsPage.OpenCreateModalAsync();
        await productsPage.FillProductFormAsync(product);
        await productsPage.SaveProductAsync();

        // Assert
        var isError = await productsPage.IsErrorToastVisibleAsync();
        isError.Should().BeTrue("название из 1 символя недопустимо");
    }

    /// <summary>
    /// Тест: Создание продукта с нулевыми калориями
    /// Техника: Анализ граничных значений - нижняя граница калорий
    /// </summary>
    [Fact(DisplayName = "UI: Создание продукта с нулевыми калориями (граница)")]
    public async Task CreateProduct_ZeroCalories_Success()
    {
        // Arrange
        var (page, _, productsPage) = await InitializeTestAsync();

        var product = new CreateProductDto
        {
            Name = "Вода",
            Category = "Liquid",
            CookingRequirement = "ReadyToUse",
            CaloriesPer100g = 0,
            ProteinsPer100g = 0,
            FatsPer100g = 0,
            CarbsPer100g = 0
        };

        // Act
        await productsPage.GoToProductsTabAsync();
        await productsPage.CreateProductAsync(product);

        // Assert
        var isSuccess = await productsPage.IsSuccessToastVisibleAsync();
        isSuccess.Should().BeTrue("нулевые калории допустимы");
    }

    /// <summary>
    /// Тест: Создание продукта с суммой БЖУ = 100 (граница)
    /// Техника: Анализ граничных значений - максимальная сумма БЖУ
    /// </summary>
    [Fact(DisplayName = "UI: Создание продукта с суммой БЖУ ровно 100г (граница)")]
    public async Task CreateProduct_MacrosSumExactly100_Success()
    {
        // Arrange
        var (page, _, productsPage) = await InitializeTestAsync();

        var product = new CreateProductDto
        {
            Name = "Масло чистое",
            Category = "Liquid",
            CookingRequirement = "ReadyToUse",
            CaloriesPer100g = 900,
            ProteinsPer100g = 0,
            FatsPer100g = 100,
            CarbsPer100g = 0
        };

        // Act
        await productsPage.GoToProductsTabAsync();
        await productsPage.OpenCreateModalAsync();
        await productsPage.FillProductFormAsync(product);
        
        // Проверяем, что сумма БЖУ отображается корректно
        var macrosSum = await productsPage.GetMacrosSumAsync();
        macrosSum.Should().Contain("100.00 г");
        
        // Проверяем, что предупреждение НЕ отображается
        var warningVisible = await productsPage.IsMacrosSumWarningVisibleAsync();
        warningVisible.Should().BeFalse("сумма БЖУ ровно 100г допустима");
        
        await productsPage.SaveProductAsync();

        // Assert
        var isSuccess = await productsPage.IsSuccessToastVisibleAsync();
        isSuccess.Should().BeTrue("сумма БЖУ 100г допустима");
    }

    /// <summary>
    /// Тест: Создание продукта с суммой БЖУ > 100 (за границей)
    /// Техника: Анализ граничных значений - превышение суммы БЖУ
    /// </summary>
    [Fact(DisplayName = "UI: Создание продукта с суммой БЖУ больше 100г (ошибка)")]
    public async Task CreateProduct_MacrosSumAbove100_Error()
    {
        // Arrange
        var (page, _, productsPage) = await InitializeTestAsync();

        var product = new CreateProductDto
        {
            Name = "Невозможный продукт",
            Category = "Liquid",
            CookingRequirement = "ReadyToUse",
            CaloriesPer100g = 1000,
            ProteinsPer100g = 50,
            FatsPer100g = 51,
            CarbsPer100g = 0
        };

        // Act
        await productsPage.GoToProductsTabAsync();
        await productsPage.OpenCreateModalAsync();
        await productsPage.FillProductFormAsync(product);
        
        // Проверяем, что отображается предупреждение
        var warningVisible = await productsPage.IsMacrosSumWarningVisibleAsync();
        warningVisible.Should().BeTrue("сумма БЖУ больше 100г должна показывать предупреждение");
        
        // Проверяем, что кнопка сохранения неактивна или появляется ошибка
        var isButtonEnabled = await productsPage.IsSaveButtonEnabledAsync();
        
        // Если кнопка активна, пытаемся нажать и проверяем ошибку
        if (isButtonEnabled)
        {
            await productsPage.SaveProductAsync();
            var isError = await productsPage.IsErrorToastVisibleAsync();
            isError.Should().BeTrue("сумма БЖУ больше 100г недопустима");
        }
        else
        {
            // Кнопка неактивна - это тоже корректное поведение
            isButtonEnabled.Should().BeFalse("кнопка сохранения должна быть неактивна при сумме БЖУ > 100г");
        }
    }

    /// <summary>
    /// Тест: Создание продукта с флагом Vegan
    /// Техника: Эквивалентное разбиение - класс продуктов с флагами
    /// </summary>
    [Fact(DisplayName = "UI: Создание веганского продукта с флагом Vegan")]
    public async Task CreateProduct_WithVeganFlag_Success()
    {
        // Arrange
        var (page, _, productsPage) = await InitializeTestAsync();

        var product = new CreateProductDto
        {
            Name = "Яблоко",
            Category = "Vegetables",
            CookingRequirement = "ReadyToUse",
            CaloriesPer100g = 52,
            ProteinsPer100g = 0.3m,
            FatsPer100g = 0.2m,
            CarbsPer100g = 14
        };

        // Act
        await productsPage.GoToProductsTabAsync();
        await productsPage.OpenCreateModalAsync();
        await productsPage.FillProductFormAsync(product);
        await productsPage.SetFlagsAsync(vegan: true);
        await productsPage.SaveProductAsync();

        // Assert
        var isSuccess = await productsPage.IsSuccessToastVisibleAsync();
        isSuccess.Should().BeTrue("создание веганского продукта должно завершиться успешно");
    }

    /// <summary>
    /// Тест: Создание продукта со всеми флагами
    /// Техника: Эквивалентное разбиение - класс продуктов со всеми флагами
    /// </summary>
    [Fact(DisplayName = "UI: Создание продукта со всеми флагами (Vegan, GlutenFree, SugarFree)")]
    public async Task CreateProduct_WithAllFlags_Success()
    {
        // Arrange
        var (page, _, productsPage) = await InitializeTestAsync();

        var product = new CreateProductDto
        {
            Name = "Вода чистая",
            Category = "Liquid",
            CookingRequirement = "ReadyToUse",
            CaloriesPer100g = 0,
            ProteinsPer100g = 0,
            FatsPer100g = 0,
            CarbsPer100g = 0
        };

        // Act
        await productsPage.GoToProductsTabAsync();
        await productsPage.OpenCreateModalAsync();
        await productsPage.FillProductFormAsync(product);
        await productsPage.SetFlagsAsync(vegan: true, glutenFree: true, sugarFree: true);
        await productsPage.SaveProductAsync();

        // Assert
        var isSuccess = await productsPage.IsSuccessToastVisibleAsync();
        isSuccess.Should().BeTrue("создание продукта со всеми флагами должно завершиться успешно");
    }
}
