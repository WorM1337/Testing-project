using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Test.Core.Integration.Fixtures;
using Test.Core.Integration.Helpers;
using Testing_project.Dtos;
using Testing_project.Dtos.Dish;

namespace Test.Core.Integration.Dishes;

/// <summary>
/// Интеграционные тесты для удаления блюд (DELETE /api/dishes/{id})
/// НЕИЗОЛИРОВАННАЯ среда: данные НЕ очищаются между тестами
/// </summary>
[Collection("Integration Tests")]
public class DeleteDishApiTests : IntegrationTestBase
{
    public DeleteDishApiTests(ApiTestFixture fixture) : base(fixture) { }

    #region D. Удаление блюда

    /// <summary>
    /// Удаление существующего блюда — 204 No Content
    /// </summary>
    [Fact(DisplayName = "API: Удаление существующего блюда возвращает 204 NoContent")]
    public async Task DeleteDish_Existing_Returns204NoContent()
    {
        // Arrange: создаем блюдо
        var productDto = TestDataBuilder.CreateProduct();
        var product = await Client.PostAsync<CreateProductDto, ProductDto>("/api/products", productDto);
        
        var createDto = TestDataBuilder.CreateDish(productId: product!.Id);
        var created = await Client.PostAsync<CreateDishDto, DishDto>("/api/dishes", createDto);

        // Act
        var response = await Client.DeleteAsync($"/api/dishes/{created!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        
        // Проверяем, что блюдо действительно удалено
        var getResponse = await Client.GetAsync($"/api/dishes/{created.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    /// <summary>
    /// Удаление несуществующего блюда — 404 Not Found
    /// </summary>
    [Fact(DisplayName = "API: Удаление несуществующего блюда возвращает 404 NotFound")]
    public async Task DeleteDish_NonExistent_Returns404NotFound()
    {
        // Act
        var response = await Client.DeleteAsync("/api/dishes/999999");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    /// <summary>
    /// Повторное удаление блюда — 404 Not Found
    /// </summary>
    [Fact(DisplayName = "API: Повторное удаление блюда возвращает 404 NotFound")]
    public async Task DeleteDish_AlreadyDeleted_Returns404NotFound()
    {
        // Arrange: создаем и удаляем блюдо
        var productDto = TestDataBuilder.CreateProduct();
        var product = await Client.PostAsync<CreateProductDto, ProductDto>("/api/products", productDto);
        
        var createDto = TestDataBuilder.CreateDish(productId: product!.Id);
        var created = await Client.PostAsync<CreateDishDto, DishDto>("/api/dishes", createDto);
        
        await Client.DeleteAsync($"/api/dishes/{created!.Id}");

        // Act: пытаемся удалить снова
        var response = await Client.DeleteAsync($"/api/dishes/{created.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    /// <summary>
    /// Удаление блюда с невалидным ID — 404 Not Found
    /// </summary>
    [Theory(DisplayName = "API: Удаление блюда с невалидным ID возвращает 404 NotFound")]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task DeleteDish_InvalidId_Returns404NotFound(int id)
    {
        // Act
        var response = await Client.DeleteAsync($"/api/dishes/{id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion
}