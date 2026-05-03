using System.Net;
using Core.Models.Enums;
using FluentAssertions;
using Testing_project.Dtos;

namespace Test.Core.Integration.Products;

[Collection("Integration Tests")]
public class GetProductsApiTests : IntegrationTestBase
{
    #region Получение продуктов

    /// <summary>
    /// Получение всех продуктов — список продуктов
    /// </summary>
    [Fact(DisplayName = "API: Получение всех продуктов возвращает список")]
    public async Task GetProducts_All_ReturnsList()
    {
        // Arrange: создаем несколько продуктов
        for (int i = 0; i < 3; i++)
        {
            await CreateProductAsync($"Продукт {i + 1}");
        }

        // Act
        var getResult = await GetProductsAsync();

        // Assert
        getResult.IsSuccess.Should().BeTrue();
        getResult.Content.Should().NotBeNull();
        getResult.Content!.Should().HaveCountGreaterThanOrEqualTo(3);
    }

    /// <summary>
    /// Поиск продукта по названию — точное совпадение
    /// </summary>
    [Fact(DisplayName = "API: Поиск продукта по точному названию возвращает результат")]
    public async Task GetProducts_SearchExactName_ReturnsResult()
    {
        // Arrange: создаем продукт с уникальным названием
        var uniqueName = $"Уникальный продукт {Guid.NewGuid()}";
        await CreateProductAsync(uniqueName);

        // Act
        var getResult = await GetProductsAsync($"?search={uniqueName}");

        // Assert
        getResult.IsSuccess.Should().BeTrue();
        getResult.Content.Should().NotBeNull();
        getResult.Content!.Should().ContainSingle(p => p.Name == uniqueName);
    }

    /// <summary>
    /// Фильтрация по категории
    /// </summary>
    [Theory(DisplayName = "API: Фильтрация продуктов по категории возвращает только продукты этой категории")]
    [InlineData(ProductCategory.Vegetables)]
    [InlineData(ProductCategory.Meat)]
    [InlineData(ProductCategory.Cereals)]
    public async Task GetProducts_FilterByCategory_ReturnsOnlyThatCategory(ProductCategory category)
    {
        // Arrange: создаем продукт нужной категории
        await CreateProductAsync(
            name: $"Продукт категории {category}",
            category: category);

        // Act
        var getResult = await GetProductsAsync($"?category={category}");

        // Assert
        getResult.IsSuccess.Should().BeTrue();
        getResult.Content.Should().NotBeNull();
        getResult.Content!.Should().NotBeEmpty();
        getResult.Content.Should().OnlyContain(p => p.Category == category);
    }

    /// <summary>
    /// Сортировка по калорийности
    /// </summary>
    [Fact(DisplayName = "API: Сортировка продуктов по калорийности (descending) работает корректно")]
    public async Task GetProducts_SortByCaloriesDescending_ReturnsSorted()
    {
        // Act
        var getResult = await GetProductsAsync("?sort=Calories&ascending=false");

        // Assert
        getResult.IsSuccess.Should().BeTrue();
        getResult.Content.Should().NotBeNull();
        getResult.Content!.Should().BeInDescendingOrder(p => p.CaloriesPer100g);
    }

    /// <summary>
    /// Получение существующего продукта по ID — 200 OK
    /// </summary>
    [Fact(DisplayName = "API: Получение существующего продукта по ID возвращает продукт")]
    public async Task GetProduct_ExistingId_ReturnsOk()
    {
        // Arrange: создаем продукт
        var createResult = await CreateProductAsync();
        createResult.Content.Should().NotBeNull();

        // Act
        var getResult = await GetProductAsync(createResult.Content!.Id);

        // Assert
        getResult.IsSuccess.Should().BeTrue();
        getResult.StatusCode.Should().Be(HttpStatusCode.OK);
        getResult.Content.Should().NotBeNull();
        getResult.Content!.Id.Should().Be(createResult.Content.Id);
    }

    /// <summary>
    /// Получение несуществующего продукта по ID — 404 NotFound
    /// </summary>
    [Fact(DisplayName = "API: Получение несуществующего продукта возвращает 404 NotFound")]
    public async Task GetProduct_NonExistentId_Returns404NotFound()
    {
        // Act
        var getResult = await GetProductAsync(999999);

        // Assert
        getResult.IsSuccess.Should().BeFalse();
        getResult.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion
}
