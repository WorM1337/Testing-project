using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Test.Core.Integration.Fixtures;
using Test.Core.Integration.Helpers;
using Testing_project.Dtos;
using Testing_project.Dtos.Dish;

namespace Test.Core.Integration.Products;

/// <summary>
/// Интеграционные тесты для удаления продуктов (DELETE /api/products/{id})
/// НЕИЗОЛИРОВАННАЯ среда: данные НЕ очищаются между тестами
/// </summary>
[Collection("Integration Tests")]
public class DeleteProductApiTests : IntegrationTestBase
{
    public DeleteProductApiTests(ApiTestFixture fixture) : base(fixture) { }

    #region Удаление продукта

    /// <summary>
    /// Удаление существующего неиспользуемого продукта — 204 No Content
    /// </summary>
    [Fact(DisplayName = "API: Удаление неиспользуемого продукта возвращает 204 NoContent")]
    public async Task DeleteProduct_UnusedProduct_Returns204NoContent()
    {
        // Arrange: создаем продукт
        var createDto = TestDataBuilder.CreateProduct("Продукт для удаления");
        var created = await Client.PostAsync<CreateProductDto, ProductDto>("/api/products", createDto);

        // Act
        var response = await Client.DeleteAsync($"/api/products/{created!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        
        // Проверяем, что продукт действительно удален
        var getResponse = await Client.GetAsync($"/api/products/{created.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    /// <summary>
    /// Удаление несуществующего продукта — 404 Not Found
    /// </summary>
    [Fact(DisplayName = "API: Удаление несуществующего продукта возвращает 404 NotFound")]
    public async Task DeleteProduct_NonExistent_Returns404NotFound()
    {
        // Act
        var response = await Client.DeleteAsync("/api/products/999999");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    /// <summary>
    /// Повторное удаление продукта — 404 Not Found
    /// </summary>
    [Fact(DisplayName = "API: Повторное удаление продукта возвращает 404 NotFound")]
    public async Task DeleteProduct_AlreadyDeleted_Returns404NotFound()
    {
        // Arrange: создаем и удаляем продукт
        var createDto = TestDataBuilder.CreateProduct("Продукт для повторного удаления");
        var created = await Client.PostAsync<CreateProductDto, ProductDto>("/api/products", createDto);
        
        await Client.DeleteAsync($"/api/products/{created!.Id}");

        // Act: пытаемся удалить снова
        var response = await Client.DeleteAsync($"/api/products/{created.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    /// <summary>
    /// Удаление продукта с невалидным ID — 404 Not Found
    /// </summary>
    [Theory(DisplayName = "API: Удаление продукта с невалидным ID возвращает 404 NotFound")]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task DeleteProduct_InvalidId_Returns404NotFound(int id)
    {
        // Act
        var response = await Client.DeleteAsync($"/api/products/{id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    /// <summary>
    /// Удаление продукта, используемого в блюде — 400 Bad Request
    /// </summary>
    [Fact(DisplayName = "API: Удаление продукта, используемого в блюде, возвращает 400 BadRequest")]
    public async Task DeleteProduct_UsedInDish_Returns400BadRequest()
    {
        // Arrange: создаем продукт и блюдо с этим продуктом
        var productDto = TestDataBuilder.CreateProduct("Продукт в блюде");
        var product = await Client.PostAsync<CreateProductDto, ProductDto>("/api/products", productDto);
        
        var dishDto = TestDataBuilder.CreateDish(
            name: "Блюдо с продуктом",
            productId: product!.Id);
        await Client.PostAsJsonAsync("/api/dishes", dishDto);

        // Act: пытаемся удалить продукт
        var response = await Client.DeleteAsync($"/api/products/{product.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("используется в блюдах");
    }

    /// <summary>
    /// Удаление продукта, используемого в нескольких блюдах — 400 Bad Request с перечислением блюд
    /// </summary>
    [Fact(DisplayName = "API: Удаление продукта из нескольких блюд возвращает список блюд")]
    public async Task DeleteProduct_UsedInMultipleDishes_ReturnsAllDishNames()
    {
        // Arrange: создаем продукт и несколько блюд с этим продуктом
        var productDto = TestDataBuilder.CreateProduct("Популярный продукт");
        var product = await Client.PostAsync<CreateProductDto, ProductDto>("/api/products", productDto);
        
        var dishNames = new[] { "Блюдо 1", "Блюдо 2", "Блюдо 3" };
        foreach (var dishName in dishNames)
        {
            var dishDto = TestDataBuilder.CreateDish(
                name: dishName,
                productId: product!.Id);
            await Client.PostAsJsonAsync("/api/dishes", dishDto);
        }

        // Act: пытаемся удалить продукт
        var response = await Client.DeleteAsync($"/api/products/{product!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("используется в блюдах");
        foreach (var dishName in dishNames)
        {
            content.Should().Contain(dishName);
        }
    }

    /// <summary>
    /// Удаление продукта после удаления всех блюд с ним — 204 No Content
    /// </summary>
    [Fact(DisplayName = "API: Удаление продукта после удаления блюд успешно")]
    public async Task DeleteProduct_AfterDeletingDishes_Succeeds()
    {
        // Arrange: создаем продукт и блюдо
        var productDto = TestDataBuilder.CreateProduct("Продукт с блюдом");
        var product = await Client.PostAsync<CreateProductDto, ProductDto>("/api/products", productDto);
        
        var dishDto = TestDataBuilder.CreateDish(productId: product!.Id);
        var dish = await Client.PostAsync<CreateDishDto, DishDto>("/api/dishes", dishDto);
        
        // Удаляем блюдо
        await Client.DeleteAsync($"/api/dishes/{dish!.Id}");

        // Act: теперь можем удалить продукт
        var response = await Client.DeleteAsync($"/api/products/{product.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    #endregion
}