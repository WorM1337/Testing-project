using System.Net;
using Core.Models.Enums;
using FluentAssertions;
using Testing_project.Dtos.Dish;
using Testing_project.Dtos.Ingredient;

namespace Test.Core.Integration.Dishes;

[Collection("Integration Tests")]
public class DeleteDishApiTests : IntegrationTestBase
{
    #region D. Удаление блюда

    [Fact(DisplayName = "API: Удаление существующего блюда возвращает 204 NoContent")]
    public async Task DeleteDish_Existing_Returns204NoContent()
    {
        // Arrange: создаем блюдо
        var productResult = await CreateProductAsync();
        productResult.Content.Should().NotBeNull();
        
        var createDto = new CreateDishDto
        {
            Name = "Блюдо для удаления",
            Category = DishCategory.Side,
            Ingredients = new List<CreateIngredientDto>
            {
                new() { ProductId = productResult.Content!.Id, AmountInGrams = 100 }
            }
        };
        
        var createResult = await CreateDishAsync(createDto);
        createResult.Content.Should().NotBeNull();

        // Act
        var deleteResult = await DeleteDishAsync(createResult.Content.Id);

        // Assert
        deleteResult.IsSuccess.Should().BeTrue();
        deleteResult.StatusCode.Should().Be(HttpStatusCode.NoContent);
        
        // Проверяем, что блюдо действительно удалено
        var getResult = await GetDishAsync(createResult.Content.Id);
        getResult.IsSuccess.Should().BeFalse();
        getResult.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact(DisplayName = "API: Удаление несуществующего блюда возвращает 404 NotFound")]
    public async Task DeleteDish_NonExistent_Returns404NotFound()
    {
        // Act
        var deleteResult = await DeleteDishAsync(999999);

        // Assert
        deleteResult.IsSuccess.Should().BeFalse();
        deleteResult.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
    
    [Theory(DisplayName = "API: Удаление блюда с невалидным ID возвращает 400 BadRequest")]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-1000)]
    public async Task DeleteDish_InvalidId_Returns400BadRequest(int id)
    {
        // Act
        var deleteResult = await DeleteDishAsync(id);

        // Assert
        deleteResult.IsSuccess.Should().BeFalse();
        deleteResult.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion
}
