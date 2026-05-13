using FluentAssertions;
using Microsoft.Playwright;
using Test.UI.Fixtures;
using Test.UI.Pages;

namespace Test.UI.Tests.Products;

/// <summary>
/// UI тесты для просмотра, фильтрации и сортировки продуктов
/// Использует техники тест-дизайна:
/// - Эквивалентное разбиение (разные категории, условия)
/// - Анализ граничных значений (поиск по названию)
/// </summary>
[Collection("UI Tests")]
public class GetProductsTests : UiTestBase
{
    public GetProductsTests(BrowserFixture browserFixture, DatabaseFixture databaseFixture)
        : base(browserFixture, databaseFixture)
    {
    }

    /// <summary>
    /// Тест: Отображение списка продуктов
    /// Техника: Эквивалентное разбиение - базовый сценарий
    /// </summary>
    [Fact(DisplayName = "UI: Отображение списка продуктов")]
    public async Task GetProducts_AllProducts_Displayed()
    {
        // Arrange
        var (_, _, productsPage) = await InitializeTestAsync();

        // Создаем несколько продуктов
        var products = new[]
        {
            new CreateProductDto { Name = "Продукт 1", Category = "Vegetables", CookingRequirement = "ReadyToUse", CaloriesPer100g = 50 },
            new CreateProductDto { Name = "Продукт 2", Category = "Meat", CookingRequirement = "RequiresCooking", CaloriesPer100g = 200 },
            new CreateProductDto { Name = "Продукт 3", Category = "Cereals", CookingRequirement = "ReadyToUse", CaloriesPer100g = 350 }
        };

        await productsPage.GoToProductsTabAsync();
        foreach (var p in products)
        {
            await productsPage.CreateProductAsync(p);
            await productsPage.IsSuccessToastVisibleAsync();
        }

        // Act: переходим на страницу продуктов
        await productsPage.GoToProductsTabAsync();

        // Assert
        var rowCount = await productsPage.GetRowCountAsync();
        rowCount.Should().BeGreaterThanOrEqualTo(3, "все созданные продукты должны отображаться");
    }

    /// <summary>
    /// Тест: Поиск продукта по точному названию
    /// Техника: Эквивалентное разбиение - точное совпадение
    /// </summary>
    [Fact(DisplayName = "UI: Поиск продукта по точному названию")]
    public async Task GetProducts_SearchExactName_Found()
    {
        // Arrange
        var (_, _, productsPage) = await InitializeTestAsync();

        var uniqueName = $"Уникальный продукт {Guid.NewGuid():N}";
        var product = new CreateProductDto
        {
            Name = uniqueName,
            Category = "Vegetables",
            CookingRequirement = "ReadyToUse",
            CaloriesPer100g = 50
        };

        await productsPage.GoToProductsTabAsync();
        await productsPage.CreateProductAsync(product);
        await productsPage.IsSuccessToastVisibleAsync();

        // Act: ищем по точному названию
        await productsPage.FilterBySearchAsync(uniqueName);

        // Assert
        var isInTable = await productsPage.IsProductInTableAsync(uniqueName);
        isInTable.Should().BeTrue("продукт должен быть найден по точному названию");
    }

    /// <summary>
    /// Тест: Поиск продукта по части названия
    /// Техника: Эквивалентное разбиение - частичное совпадение
    /// </summary>
    [Fact(DisplayName = "UI: Поиск продукта по части названия")]
    public async Task GetProducts_SearchPartialName_Found()
    {
        // Arrange
        var (_, _, productsPage) = await InitializeTestAsync();

        await productsPage.GoToProductsTabAsync();
        await productsPage.CreateProductAsync(new CreateProductDto
        {
            Name = "Картофель красный",
            Category = "Vegetables",
            CookingRequirement = "RequiresCooking",
            CaloriesPer100g = 77
        });
        await productsPage.IsSuccessToastVisibleAsync();

        // Act: ищем по части названия
        await productsPage.FilterBySearchAsync("Картофель");

        // Assert
        var isInTable = await productsPage.IsProductInTableAsync("Картофель красный");
        isInTable.Should().BeTrue("продукт должен быть найден по части названия");
    }

    /// <summary>
    /// Тест: Поиск несуществующего продукта
    /// Техника: Эквивалентное разбиение - отсутствие результатов
    /// </summary>
    [Fact(DisplayName = "UI: Поиск несуществующего продукта возвращает пустой результат")]
    public async Task GetProducts_SearchNonExistent_NotFound()
    {
        // Arrange
        var (_, _, productsPage) = await InitializeTestAsync();

        await productsPage.GoToProductsTabAsync();
        await productsPage.CreateProductAsync(new CreateProductDto
        {
            Name = "Существующий продукт",
            Category = "Vegetables",
            CookingRequirement = "ReadyToUse",
            CaloriesPer100g = 50
        });
        await productsPage.IsSuccessToastVisibleAsync();

        // Act: ищем несуществующий продукт
        await productsPage.FilterBySearchAsync("Несуществующий продукт XYZ");

        // Assert
        var rowCount = await productsPage.GetRowCountAsync();
        rowCount.Should().Be(0, "поиск несуществующего продукта должен вернуть пустой результат");
    }

    /// <summary>
    /// Тест: Фильтрация по категории
    /// Техника: Эквивалентное разбиение - фильтрация по категории
    /// </summary>
    [Fact(DisplayName = "UI: Фильтрация продуктов по категории")]
    public async Task GetProducts_FilterByCategory_Success()
    {
        // Arrange
        var (page, _, productsPage) = await InitializeTestAsync();

        await productsPage.GoToProductsTabAsync();
        
        // Создаем продукты разных категорий
        await productsPage.CreateProductAsync(new CreateProductDto
        {
            Name = "Овощ",
            Category = "Vegetables",
            CookingRequirement = "ReadyToUse",
            CaloriesPer100g = 50
        });
        await productsPage.IsSuccessToastVisibleAsync();
        await productsPage.CreateProductAsync(new CreateProductDto
        {
            Name = "Мясо",
            Category = "Meat",
            CookingRequirement = "RequiresCooking",
            CaloriesPer100g = 200
        });
        await productsPage.IsSuccessToastVisibleAsync();
        await productsPage.CreateProductAsync(new CreateProductDto
        {
            Name = "Крупа",
            Category = "Cereals",
            CookingRequirement = "ReadyToUse",
            CaloriesPer100g = 350
        });
        await productsPage.IsSuccessToastVisibleAsync();

        // Act: фильтруем по категории Vegetables
        var categorySelect = page.Locator("#filter-category");
        await categorySelect.SelectOptionAsync("Vegetables");
        await page.ClickAsync("[data-testid='apply-filters-btn']");
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Assert
        var rowCount = await productsPage.GetRowCountAsync();
        rowCount.Should().Be(1, "должен отображаться только один продукт категории Vegetables");
    }

    /// <summary>
    /// Тест: Сортировка по калорийности (по убыванию)
    /// Техника: Эквивалентное разбиение - сортировка
    /// </summary>
    [Fact(DisplayName = "UI: Сортировка продуктов по калорийности (по убыванию)")]
    public async Task GetProducts_SortByCaloriesDescending_Success()
    {
        // Arrange
        var (page, _, productsPage) = await InitializeTestAsync();

        await productsPage.GoToProductsTabAsync();
        
        // Создаем продукты с разной калорийностью
        await productsPage.CreateProductAsync(new CreateProductDto
        {
            Name = "Низкокалорийный",
            Category = "Vegetables",
            CookingRequirement = "ReadyToUse",
            CaloriesPer100g = 30
        });
        await productsPage.IsSuccessToastVisibleAsync();
        await productsPage.CreateProductAsync(new CreateProductDto
        {
            Name = "Среднекалорийный",
            Category = "Vegetables",
            CookingRequirement = "ReadyToUse",
            CaloriesPer100g = 100
        });
        await productsPage.IsSuccessToastVisibleAsync();
        await productsPage.CreateProductAsync(new CreateProductDto
        {
            Name = "Высококалорийный",
            Category = "Vegetables",
            CookingRequirement = "ReadyToUse",
            CaloriesPer100g = 500
        });
        await productsPage.IsSuccessToastVisibleAsync();

        // Act: сортируем по убыванию
        var sortSelect = page.Locator("#filter-sort");
        await sortSelect.SelectOptionAsync("Calories");
        
        var orderSelect = page.Locator("#filter-order");
        await orderSelect.SelectOptionAsync("false"); // false = descending
        
        await page.ClickAsync("[data-testid='apply-filters-btn']");
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Assert: проверяем порядок
        var firstRow = page.Locator("#table-body tr").First;
        var firstText = await firstRow.TextContentAsync();
        firstText.Should().Contain("Высококалорийный", "первым должен идти самый калорийный продукт");
    }

    /// <summary>
    /// Тест: Сброс фильтров
    /// Техника: Эквивалентное разбиение - сброс состояния
    /// </summary>
    [Fact(DisplayName = "UI: Сброс фильтров возвращает все продукты")]
    public async Task GetProducts_ResetFilters_ShowAll()
    {
        // Arrange
        var (page, _, productsPage) = await InitializeTestAsync();

        await productsPage.GoToProductsTabAsync();
        
        await productsPage.CreateProductAsync(new CreateProductDto
        {
            Name = "Овощ",
            Category = "Vegetables",
            CookingRequirement = "ReadyToUse",
            CaloriesPer100g = 50
        });
        await productsPage.IsSuccessToastVisibleAsync();
        await productsPage.CreateProductAsync(new CreateProductDto
        {
            Name = "Мясо",
            Category = "Meat",
            CookingRequirement = "RequiresCooking",
            CaloriesPer100g = 200
        });
        await productsPage.IsSuccessToastVisibleAsync();

        // Применяем фильтр
        var categorySelect = page.Locator("#filter-category");
        await categorySelect.SelectOptionAsync("Vegetables");
        await page.ClickAsync("[data-testid='apply-filters-btn']");
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Act: сбрасываем фильтры
        await page.ClickAsync("[data-testid='reset-filters-btn']");
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Assert: должны отображаться все продукты
        var rowCount = await productsPage.GetRowCountAsync();
        rowCount.Should().Be(2, "после сброса фильтров должны отображаться все продукты");
    }

    /// <summary>
    /// Тест: Поиск по названию с минимальной длиной (1 символ)
    /// Техника: Анализ граничных значений - минимальная длина поиска
    /// </summary>
    [Fact(DisplayName = "UI: Поиск продукта по одному символу")]
    public async Task GetProducts_SearchSingleCharacter_Success()
    {
        // Arrange
        var (_, _, productsPage) = await InitializeTestAsync();

        await productsPage.GoToProductsTabAsync();
        await productsPage.CreateProductAsync(new CreateProductDto
        {
            Name = "Арбуз",
            Category = "Vegetables",
            CookingRequirement = "ReadyToUse",
            CaloriesPer100g = 30
        });
        await productsPage.IsSuccessToastVisibleAsync();
        await productsPage.CreateProductAsync(new CreateProductDto
        {
            Name = "Яблоко",
            Category = "Vegetables",
            CookingRequirement = "ReadyToUse",
            CaloriesPer100g = 52
        });
        await productsPage.IsSuccessToastVisibleAsync();

        // Act: ищем по одному символу
        await productsPage.FilterBySearchAsync("А");

        // Assert: должны найтись оба продукта, начинающиеся на "А"
        var rowCount = await productsPage.GetRowCountAsync();
        rowCount.Should().BeGreaterThanOrEqualTo(1, "поиск по одному символу должен работать");
    }
}
