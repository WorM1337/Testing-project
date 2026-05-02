using System.Net;
using Core.Models.Enums;
using FluentAssertions;
using Test.Core.Integration.Helpers;
using Testing_project.Dtos;
using static Test.Core.Integration.Helpers.ApiHelpers;

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
            var createDto = TestDataBuilder.CreateProduct($"Продукт {i + 1}");
            await Client.PostAsJsonAsync("/api/products", createDto);
        }

        // Act
        var products = await Client.GetFromJsonAsync<List<ProductDto>>("/api/products");

        // Assert
        products.Should().NotBeNull();
        products!.Should().HaveCountGreaterThanOrEqualTo(3);
    }

    /// <summary>
    /// Поиск продукта по названию — точное совпадение
    /// </summary>
    [Fact(DisplayName = "API: Поиск продукта по точному названию возвращает результат")]
    public async Task GetProducts_SearchExactName_ReturnsResult()
    {
        // Arrange: создаем продукт с уникальным названием
        var uniqueName = $"Уникальный продукт {Guid.NewGuid()}";
        var createDto = TestDataBuilder.CreateProduct(uniqueName);
        await Client.PostAsJsonAsync("/api/products", createDto);

        // Act
        var products = await Client.GetFromJsonAsync<List<ProductDto>>($"/api/products?search={uniqueName}");

        // Assert
        products.Should().NotBeNull();
        products!.Should().ContainSingle(p => p.Name == uniqueName);
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
        var createDto = TestDataBuilder.CreateProduct(
            name: $"Продукт категории {category}",
            category: category);
        await Client.PostAsJsonAsync("/api/products", createDto, JsonOptions);

        // Act
        var products = await Client.GetFromJsonAsync<List<ProductDto>>($"/api/products?category={category}", JsonOptions);

        // Assert
        products.Should().NotBeNull();
        products!.Should().NotBeEmpty();
        products.Should().OnlyContain(p => p.Category == category);
    }

    /// <summary>
    /// Сортировка по калорийности
    /// </summary>
    [Fact(DisplayName = "API: Сортировка продуктов по калорийности (descending) работает корректно")]
    public async Task GetProducts_SortByCaloriesDescending_ReturnsSorted()
    {
        // Act
        var products = await Client.GetFromJsonAsync<List<ProductDto>>("/api/products?sort=Calories&ascending=false");

        // Assert
        products.Should().NotBeNull();
        products!.Should().BeInDescendingOrder(p => p.CaloriesPer100g);
    }

    /// <summary>
    /// Получение существующего продукта по ID — 200 OK
    /// </summary>
    [Fact(DisplayName = "API: Получение существующего продукта по ID возвращает продукт")]
    public async Task GetProduct_ExistingId_ReturnsOk()
    {
        // Arrange: создаем продукт
        var createDto = TestDataBuilder.CreateProduct();
        var created = await Client.PostAsync<CreateProductDto, ProductDto>("/api/products", createDto);

        // Act
        var response = await Client.GetAsync($"/api/products/{created!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var product = await response.Content.ReadFromJsonAsync<ProductDto>(JsonOptions);
        product.Should().NotBeNull();
        product!.Id.Should().Be(created.Id);
    }

    /// <summary>
    /// Получение несуществующего продукта по ID — 404 NotFound
    /// </summary>
    [Fact(DisplayName = "API: Получение несуществующего продукта возвращает 404 NotFound")]
    public async Task GetProduct_NonExistentId_Returns404NotFound()
    {
        // Act
        var response = await Client.GetAsync("/api/products/999999");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion
}