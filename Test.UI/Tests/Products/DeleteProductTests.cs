using FluentAssertions;
using Test.UI.Fixtures;
using Test.UI.Pages;

namespace Test.UI.Tests.Products;

/// <summary>
/// UI тесты для удаления продуктов
/// Использует техники тест-дизайна:
/// - Эквивалентное разбиение (существующие/несуществующие продукты)
/// - Анализ граничных значений (ID продуктов)
/// </summary>
[Collection("UI Tests")]
public class DeleteProductTests : UiTestBase
{
    public DeleteProductTests(BrowserFixture browserFixture, DatabaseFixture databaseFixture)
        : base(browserFixture, databaseFixture)
    {
    }

    /// <summary>
    /// Тест: Удаление существующего продукта
    /// Техника: Эквивалентное разбиение - валидное удаление
    /// </summary>
    [Fact(DisplayName = "UI: Удаление существующего продукта")]
    public async Task DeleteProduct_Existing_Success()
    {
        // Arrange
        var (page, _, productsPage) = await InitializeTestAsync();

        // Создаем продукт для удаления
        var product = new CreateProductDto
        {
            Name = "Продукт для удаления",
            Category = "Vegetables",
            CookingRequirement = "ReadyToUse",
            CaloriesPer100g = 50
        };

        await productsPage.GoToProductsTabAsync();
        await productsPage.CreateProductAsync(product);
        await productsPage.IsSuccessToastVisibleAsync();

        // Act: удаляем продукт
        await productsPage.DeleteProductAsync("Продукт для удаления");

        // Assert
        var isNotInTable = await productsPage.IsProductNotInTableAsync("Продукт для удаления");
        isNotInTable.Should().BeTrue("продукт должен исчезнуть из таблицы");
    }

    /// <summary>
    /// Тест: Удаление нескольких продуктов по очереди
    /// Техника: Эквивалентное разбиение - множественное удаление
    /// </summary>
    [Fact(DisplayName = "UI: Удаление нескольких продуктов по очереди")]
    public async Task DeleteProduct_Multiple_Success()
    {
        // Arrange
        var (page, _, productsPage) = await InitializeTestAsync();

        // Создаем несколько продуктов
        var products = new[]
        {
            new CreateProductDto { Name = "Продукт 1", Category = "Vegetables", CookingRequirement = "ReadyToUse", CaloriesPer100g = 50 },
            new CreateProductDto { Name = "Продукт 2", Category = "Vegetables", CookingRequirement = "ReadyToUse", CaloriesPer100g = 60 },
            new CreateProductDto { Name = "Продукт 3", Category = "Vegetables", CookingRequirement = "ReadyToUse", CaloriesPer100g = 70 }
        };

        await productsPage.GoToProductsTabAsync();
        foreach (var p in products)
        {
            await productsPage.CreateProductAsync(p);
            await productsPage.IsSuccessToastVisibleAsync();
        }

        // Act: удаляем все продукты
        await productsPage.DeleteProductAsync("Продукт 1");
        await productsPage.DeleteProductAsync("Продукт 2");
        await productsPage.DeleteProductAsync("Продукт 3");

        // Assert
        var rowCount = await productsPage.GetRowCountAsync();
        rowCount.Should().Be(0, "все продукты должны быть удалены");
    }

    /// <summary>
    /// Тест: Проверка отсутствия кнопок удаления для пустой таблицы
    /// Техника: Эквивалентное разбиение - пустая таблица
    /// </summary>
    [Fact(DisplayName = "UI: Проверка отсутствия кнопки удаления для пустой таблицы")]
    public async Task DeleteProduct_EmptyTable_NoDeleteButton()
    {
        // Arrange
        var (page, _, productsPage) = await InitializeTestAsync();

        await productsPage.GoToProductsTabAsync();

        // Act & Assert: в пустой таблице нет кнопок удаления
        var rowCount = await productsPage.GetRowCountAsync();
        rowCount.Should().Be(0, "таблица пуста");
    }

    /// <summary>
    /// Тест: Удаление продукта с особыми символами в названии
    /// Техника: Эквивалентное разбиение - специальные символы
    /// </summary>
    [Fact(DisplayName = "UI: Удаление продукта со специальными символами в названии")]
    public async Task DeleteProduct_SpecialCharacters_Success()
    {
        // Arrange
        var (page, _, productsPage) = await InitializeTestAsync();

        var product = new CreateProductDto
        {
            Name = "Продукт с символами !@#$%",
            Category = "Vegetables",
            CookingRequirement = "ReadyToUse",
            CaloriesPer100g = 50
        };

        await productsPage.GoToProductsTabAsync();
        await productsPage.CreateProductAsync(product);
        await productsPage.IsSuccessToastVisibleAsync();

        // Act: удаляем продукт
        await productsPage.DeleteProductAsync("Продукт с символами !@#$%");

        // Assert
        var isNotInTable = await productsPage.IsProductNotInTableAsync("Продукт с символами !@#$%");
        isNotInTable.Should().BeTrue("продукт должен исчезнуть из таблицы");
    }

    /// <summary>
    /// Тест: Удаление продукта с длинным названием
    /// Техника: Анализ граничных значений - длинное название
    /// </summary>
    [Fact(DisplayName = "UI: Удаление продукта с длинным названием (50 символов)")]
    public async Task DeleteProduct_LongName_Success()
    {
        // Arrange
        var (page, _, productsPage) = await InitializeTestAsync();

        var product = new CreateProductDto
        {
            Name = new string('А', 50),
            Category = "Vegetables",
            CookingRequirement = "ReadyToUse",
            CaloriesPer100g = 50
        };

        await productsPage.GoToProductsTabAsync();
        await productsPage.CreateProductAsync(product);
        await productsPage.IsSuccessToastVisibleAsync();

        // Act: удаляем продукт
        await productsPage.DeleteProductAsync(new string('А', 50));

        // Assert
        var isNotInTable = await productsPage.IsProductNotInTableAsync(new string('А', 50));
        isNotInTable.Should().BeTrue("продукт должен исчезнуть из таблицы");
    }

    /// <summary>
    /// Тест: Удаление и повторное создание продукта с тем же названием
    /// Техника: Эквивалентное разбиение - повторное использование имени
    /// </summary>
    [Fact(DisplayName = "UI: Удаление и повторное создание продукта с тем же названием")]
    public async Task DeleteProduct_RecreateSameName_Success()
    {
        // Arrange
        var (page, _, productsPage) = await InitializeTestAsync();

        var product = new CreateProductDto
        {
            Name = "Уникальное название",
            Category = "Vegetables",
            CookingRequirement = "ReadyToUse",
            CaloriesPer100g = 50
        };

        await productsPage.GoToProductsTabAsync();
        await productsPage.CreateProductAsync(product);

        // Act 1: удаляем продукт
        await productsPage.DeleteProductAsync("Уникальное название");

        // Act 2: создаем продукт с тем же названием
        await productsPage.CreateProductAsync(product);
        await productsPage.IsSuccessToastVisibleAsync();

        // Assert
        var isInTable = await productsPage.IsProductInTableAsync("Уникальное название");
        isInTable.Should().BeTrue("повторное создание с тем же названием должно работать");
    }
}
