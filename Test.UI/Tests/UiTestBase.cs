using Microsoft.Playwright;
using Test.UI.Fixtures;
using Test.UI.Pages;

namespace Test.UI.Tests;

/// <summary>
/// Базовый класс для UI тестов
/// Содержит общую логику инициализации браузера и базы данных
/// </summary>
public class UiTestBase
{
    protected readonly BrowserFixture BrowserFixture;
    protected readonly DatabaseFixture DatabaseFixture;

    public UiTestBase(BrowserFixture browserFixture, DatabaseFixture databaseFixture)
    {
        BrowserFixture = browserFixture;
        DatabaseFixture = databaseFixture;
    }

    /// <summary>
    /// Создает новый контекст браузера и страницу для теста
    /// </summary>
    protected async Task<(IPage Page, DishesPage DishesPage, ProductsPage ProductsPage)> CreateTestPageAsync()
    {
        var context = await BrowserFixture.CreateContextAsync();
        var page = await context.NewPageAsync();
        var dishesPage = new DishesPage(page, BrowserFixture.BaseUrl);
        var productsPage = new ProductsPage(page, BrowserFixture.BaseUrl);
        return (page, dishesPage, productsPage);
    }

    /// <summary>
    /// Очищает базу данных и создает тестовую страницу
    /// </summary>
    protected async Task<(IPage Page, DishesPage DishesPage, ProductsPage ProductsPage)> InitializeTestAsync()
    {
        await DatabaseFixture.CleanDatabaseAsync();
        return await CreateTestPageAsync();
    }
}
