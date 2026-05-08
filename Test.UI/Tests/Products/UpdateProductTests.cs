using FluentAssertions;
using Microsoft.Playwright;
using Test.UI.Fixtures;
using Test.UI.Pages;

namespace Test.UI.Tests.Products;

/// <summary>
/// UI тесты для обновления продуктов
/// Использует техники тест-дизайна:
/// - Эквивалентное разбиение (классы валидных/невалидных данных)
/// - Анализ граничных значений (границы полей)
/// </summary>
[Collection("UI Tests")]
public class UpdateProductTests
{
    private readonly BrowserFixture _browserFixture;
    private readonly DatabaseFixture _databaseFixture;

    public UpdateProductTests(BrowserFixture browserFixture, DatabaseFixture databaseFixture)
    {
        _browserFixture = browserFixture;
        _databaseFixture = databaseFixture;
    }

    /// <summary>
    /// Тест: Обновление названия продукта
    /// Техника: Эквивалентное разбиение - валидное обновление
    /// </summary>
    [Fact(DisplayName = "UI: Обновление названия продукта")]
    public async Task UpdateProduct_Name_Success()
    {
        // Arrange
        await _databaseFixture.CleanDatabaseAsync();
        await using var context = await _browserFixture.CreateContextAsync();
        var page = await context.NewPageAsync();
        var productsPage = new ProductsPage(page, _browserFixture.BaseUrl);

        // Создаем продукт для обновления
        var product = new CreateProductDto
        {
            Name = "Старое название",
            Category = "Vegetables",
            CookingRequirement = "ReadyToUse",
            CaloriesPer100g = 50,
            ProteinsPer100g = 1,
            FatsPer100g = 0,
            CarbsPer100g = 10
        };

        await productsPage.GoToProductsTabAsync();
        await productsPage.CreateProductAsync(product);
        await productsPage.IsSuccessToastVisibleAsync();

        // Act: обновляем название
        var updatedProduct = new CreateProductDto
        {
            Name = "Новое название",
            Category = "Vegetables",
            CookingRequirement = "ReadyToUse",
            CaloriesPer100g = 50,
            ProteinsPer100g = 1,
            FatsPer100g = 0,
            CarbsPer100g = 10
        };

        await productsPage.EditProductAsync("Старое название", updatedProduct);

        // Assert
        var isSuccess = await productsPage.IsSuccessToastVisibleAsync();
        isSuccess.Should().BeTrue("обновление должно завершиться успешно");
        
        var isInTable = await productsPage.IsProductInTableAsync("Новое название");
        isInTable.Should().BeTrue("продукт с новым названием должен отображаться в таблице");
    }

    /// <summary>
    /// Тест: Обновление калорийности продукта
    /// Техника: Эквивалентное разбиение - обновление числовых полей
    /// </summary>
    [Fact(DisplayName = "UI: Обновление калорийности продукта")]
    public async Task UpdateProduct_Calories_Success()
    {
        // Arrange
        await _databaseFixture.CleanDatabaseAsync();
        await using var context = await _browserFixture.CreateContextAsync();
        var page = await context.NewPageAsync();
        var productsPage = new ProductsPage(page, _browserFixture.BaseUrl);

        var product = new CreateProductDto
        {
            Name = "Продукт для обновления калорий",
            Category = "Vegetables",
            CookingRequirement = "ReadyToUse",
            CaloriesPer100g = 50,
            ProteinsPer100g = 1,
            FatsPer100g = 0,
            CarbsPer100g = 10
        };

        await productsPage.GoToProductsTabAsync();
        await productsPage.CreateProductAsync(product);
        await productsPage.IsSuccessToastVisibleAsync();

        // Act: обновляем калорийность
        var updatedProduct = new CreateProductDto
        {
            Name = "Продукт для обновления калорий",
            Category = "Vegetables",
            CookingRequirement = "ReadyToUse",
            CaloriesPer100g = 150,
            ProteinsPer100g = 1,
            FatsPer100g = 0,
            CarbsPer100g = 10
        };

        await productsPage.EditProductAsync("Продукт для обновления калорий", updatedProduct);

        // Assert
        var isSuccess = await productsPage.IsSuccessToastVisibleAsync();
        isSuccess.Should().BeTrue();
    }

    /// <summary>
    /// Тест: Обновление названия на минимальную длину (2 символа)
    /// Техника: Анализ граничных значений - нижняя граница
    /// </summary>
    [Fact(DisplayName = "UI: Обновление названия на минимальную длину (2 символа)")]
    public async Task UpdateProduct_NameMinBoundary_Success()
    {
        // Arrange
        await _databaseFixture.CleanDatabaseAsync();
        await using var context = await _browserFixture.CreateContextAsync();
        var page = await context.NewPageAsync();
        var productsPage = new ProductsPage(page, _browserFixture.BaseUrl);

        var product = new CreateProductDto
        {
            Name = "Длинное название",
            Category = "Vegetables",
            CookingRequirement = "ReadyToUse",
            CaloriesPer100g = 50
        };

        await productsPage.GoToProductsTabAsync();
        await productsPage.CreateProductAsync(product);
        await productsPage.IsSuccessToastVisibleAsync();

        // Act: обновляем на минимальную длину
        var updatedProduct = new CreateProductDto
        {
            Name = "Ай",
            Category = "Vegetables",
            CookingRequirement = "ReadyToUse",
            CaloriesPer100g = 50
        };

        await productsPage.EditProductAsync("Длинное название", updatedProduct);

        // Assert
        var isSuccess = await productsPage.IsSuccessToastVisibleAsync();
        isSuccess.Should().BeTrue("название из 2 символов допустимо");
    }

    /// <summary>
    /// Тест: Обновление названия на максимальную длину (50 символов)
    /// Техника: Анализ граничных значений - верхняя граница
    /// </summary>
    [Fact(DisplayName = "UI: Обновление названия на максимальную длину (50 символов)")]
    public async Task UpdateProduct_NameMaxBoundary_Success()
    {
        // Arrange
        await _databaseFixture.CleanDatabaseAsync();
        await using var context = await _browserFixture.CreateContextAsync();
        var page = await context.NewPageAsync();
        var productsPage = new ProductsPage(page, _browserFixture.BaseUrl);

        var product = new CreateProductDto
        {
            Name = "Короткое",
            Category = "Vegetables",
            CookingRequirement = "ReadyToUse",
            CaloriesPer100g = 50
        };

        await productsPage.GoToProductsTabAsync();
        await productsPage.CreateProductAsync(product);
        await productsPage.IsSuccessToastVisibleAsync();

        // Act: обновляем на 50 символов
        var updatedProduct = new CreateProductDto
        {
            Name = new string('А', 50),
            Category = "Vegetables",
            CookingRequirement = "ReadyToUse",
            CaloriesPer100g = 50
        };

        await productsPage.EditProductAsync("Короткое", updatedProduct);

        // Assert
        var isSuccess = await productsPage.IsSuccessToastVisibleAsync();
        isSuccess.Should().BeTrue("название из 50 символов допустимо");
    }

    /// <summary>
    /// Тест: Обновление названия на слишком короткое (1 символ)
    /// Техника: Анализ граничных значений - за нижней границей
    /// </summary>
    [Fact(DisplayName = "UI: Обновление названия на слишком короткое (1 символ)")]
    public async Task UpdateProduct_NameTooShort_Error()
    {
        // Arrange
        await _databaseFixture.CleanDatabaseAsync();
        await using var context = await _browserFixture.CreateContextAsync();
        var page = await context.NewPageAsync();
        var productsPage = new ProductsPage(page, _browserFixture.BaseUrl);

        var product = new CreateProductDto
        {
            Name = "Нормальное название",
            Category = "Vegetables",
            CookingRequirement = "ReadyToUse",
            CaloriesPer100g = 50
        };

        await productsPage.GoToProductsTabAsync();
        await productsPage.CreateProductAsync(product);
        await productsPage.IsSuccessToastVisibleAsync();

        // Act: пытаемся обновить на 1 символ
        await productsPage.OpenEditModalAsync("Нормальное название");
        await productsPage.FillProductFormAsync(new CreateProductDto
        {
            Name = "А",
            Category = "Vegetables",
            CookingRequirement = "ReadyToUse",
            CaloriesPer100g = 50
        });
        await productsPage.SaveProductAsync();

        // Assert
        var isError = await productsPage.IsErrorToastVisibleAsync();
        isError.Should().BeTrue("название из 1 символя недопустимо");
    }

    /// <summary>
    /// Тест: Обновление с отрицательной калорийностью
    /// Техника: Анализ граничных значений - отрицательное значение
    /// Примечание: HTML5 валидация (min="0") блокирует отправку формы
    /// </summary>
    [Fact(DisplayName = "UI: Обновление с отрицательной калорийностью (ошибка)")]
    public async Task UpdateProduct_NegativeCalories_Error()
    {
        // Arrange
        await _databaseFixture.CleanDatabaseAsync();
        await using var context = await _browserFixture.CreateContextAsync();
        var page = await context.NewPageAsync();
        var productsPage = new ProductsPage(page, _browserFixture.BaseUrl);

        var product = new CreateProductDto
        {
            Name = "Продукт",
            Category = "Vegetables",
            CookingRequirement = "ReadyToUse",
            CaloriesPer100g = 50
        };

        await productsPage.GoToProductsTabAsync();
        await productsPage.CreateProductAsync(product);
        await productsPage.IsSuccessToastVisibleAsync();

        // Act: пытаемся установить отрицательную калорийность
        await productsPage.OpenEditModalAsync("Продукт");
        
        // Заполняем форму вручную, чтобы проверить HTML5 валидацию
        await productsPage.FillProductFormAsync(new CreateProductDto
        {
            Name = "Продукт",
            Category = "Vegetables",
            CookingRequirement = "ReadyToUse",
            CaloriesPer100g = -10
        });
        
        // Небольшая пауза, чтобы браузер применил валидацию
        await page.WaitForTimeoutAsync(500);
        
        // Проверяем значение поля калорий — браузер должен был отклонить отрицательное значение
        var caloriesInput = page.Locator("#calories");
        var inputValue = await caloriesInput.InputValueAsync();
        
        // Закрываем модалку (отмена)
        await page.Keyboard.PressAsync("Escape");
        await productsPage.ModalOverlay.WaitForAsync(new() { State = WaitForSelectorState.Hidden, Timeout = 2000 });
        
        // Assert: поле должно быть пустым или содержать 0 (браузер отклоняет отрицательные значения)
        (inputValue == "" || inputValue == "0").Should().BeTrue(
            $"HTML5 валидация должна отклонять отрицательные значения, но получено: '{inputValue}'");
    }

    /// <summary>
    /// Тест: Обновление с суммой БЖУ > 100
    /// Техника: Анализ граничных значений - превышение суммы
    /// </summary>
    [Fact(DisplayName = "UI: Обновление с суммой БЖУ больше 100г (ошибка)")]
    public async Task UpdateProduct_MacrosSumAbove100_Error()
    {
        // Arrange
        await _databaseFixture.CleanDatabaseAsync();
        await using var context = await _browserFixture.CreateContextAsync();
        var page = await context.NewPageAsync();
        var productsPage = new ProductsPage(page, _browserFixture.BaseUrl);

        var product = new CreateProductDto
        {
            Name = "Продукт",
            Category = "Vegetables",
            CookingRequirement = "ReadyToUse",
            CaloriesPer100g = 50,
            ProteinsPer100g = 10,
            FatsPer100g = 10,
            CarbsPer100g = 10
        };

        await productsPage.GoToProductsTabAsync();
        await productsPage.CreateProductAsync(product);
        await productsPage.IsSuccessToastVisibleAsync();

        // Act: устанавливаем сумму БЖУ > 100
        await productsPage.OpenEditModalAsync("Продукт");
        await productsPage.FillProductFormAsync(new CreateProductDto
        {
            Name = "Продукт",
            Category = "Vegetables",
            CookingRequirement = "ReadyToUse",
            CaloriesPer100g = 100,
            ProteinsPer100g = 50,
            FatsPer100g = 51,
            CarbsPer100g = 0
        });
        
        // Проверяем предупреждение
        var warningVisible = await productsPage.IsMacrosSumWarningVisibleAsync();
        warningVisible.Should().BeTrue("должно отображаться предупреждение");
        
        await productsPage.SaveProductAsync();

        // Assert
        var isError = await productsPage.IsErrorToastVisibleAsync();
        isError.Should().BeTrue("сумма БЖУ > 100г недопустима");
    }

    /// <summary>
    /// Тест: Обновление с добавлением флага Vegan
    /// Техника: Эквивалентное разбиение - класс с флагами
    /// </summary>
    [Fact(DisplayName = "UI: Обновление с добавлением флага Vegan")]
    public async Task UpdateProduct_AddVeganFlag_Success()
    {
        // Arrange
        await _databaseFixture.CleanDatabaseAsync();
        await using var context = await _browserFixture.CreateContextAsync();
        var page = await context.NewPageAsync();
        var productsPage = new ProductsPage(page, _browserFixture.BaseUrl);

        var product = new CreateProductDto
        {
            Name = "Яблоко",
            Category = "Vegetables",
            CookingRequirement = "ReadyToUse",
            CaloriesPer100g = 52
        };

        await productsPage.GoToProductsTabAsync();
        await productsPage.CreateProductAsync(product);
        await productsPage.IsSuccessToastVisibleAsync();

        // Act: добавляем флаг Vegan
        await productsPage.OpenEditModalAsync("Яблоко");
        await productsPage.SetFlagsAsync(vegan: true);
        await productsPage.SaveProductAsync();

        // Assert
        var isSuccess = await productsPage.IsSuccessToastVisibleAsync();
        isSuccess.Should().BeTrue();
    }
}
