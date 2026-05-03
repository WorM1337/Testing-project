using System.Net;
using Core.Models.Enums;
using FluentAssertions;
using Testing_project.Dtos.Dish;
using Testing_project.Dtos.Ingredient;

namespace Test.Core.Integration.Products;

[Collection("Integration Tests")]
public class DeleteProductApiTests : IntegrationTestBase
{
    #region Удаление продукта

    /// <summary>
    /// Удаление существующего неиспользуемого продукта — 204 No Content
    /// </summary>
    [Fact(DisplayName = "API: Удаление неиспользуемого продукта возвращает 204 NoContent")]
    public async Task DeleteProduct_UnusedProduct_Returns204NoContent()
    {
        // Arrange: создаем продукт
        var createResult = await CreateProductAsync("Продукт для удаления");

        // Act
        var deleteResult = await DeleteProductAsync(createResult.Content!.Id);

        // Assert
        deleteResult.IsSuccess.Should().BeTrue();
        deleteResult.StatusCode.Should().Be(HttpStatusCode.NoContent);
        
        // Проверяем, что продукт действительно удален
        var getResult = await GetProductAsync(createResult.Content.Id);
        getResult.IsSuccess.Should().BeFalse();
        getResult.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    /// <summary>
    /// Удаление несуществующего продукта — 404 Not Found
    /// </summary>
    [Fact(DisplayName = "API: Удаление несуществующего продукта возвращает 404 NotFound")]
    public async Task DeleteProduct_NonExistent_Returns404NotFound()
    {
        // Act
        var deleteResult = await DeleteProductAsync(999999);

        // Assert
        deleteResult.IsSuccess.Should().BeFalse();
        deleteResult.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    /// <summary>
    /// Повторное удаление продукта — 404 Not Found
    /// </summary>
    [Fact(DisplayName = "API: Повторное удаление продукта возвращает 404 NotFound")]
    public async Task DeleteProduct_AlreadyDeleted_Returns404NotFound()
    {
        // Arrange: создаем и удаляем продукт
        var createResult = await CreateProductAsync("Продукт для повторного удаления");
        
        await DeleteProductAsync(createResult.Content!.Id);

        // Act: пытаемся удалить снова
        var deleteResult = await DeleteProductAsync(createResult.Content.Id);

        // Assert
        deleteResult.IsSuccess.Should().BeFalse();
        deleteResult.StatusCode.Should().Be(HttpStatusCode.NotFound);
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
        var deleteResult = await DeleteProductAsync(id);

        // Assert
        deleteResult.IsSuccess.Should().BeFalse();
        deleteResult.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    /// <summary>
    /// Удаление продукта, используемого в блюде — 400 Bad Request
    /// </summary>
    [Fact(DisplayName = "API: Удаление продукта, используемого в блюде, возвращает 400 BadRequest")]
    public async Task DeleteProduct_UsedInDish_Returns400BadRequest()
    {
        // Arrange: создаем продукт и блюдо с этим продуктом
        var productResult = await CreateProductAsync("Продукт в блюде");
        productResult.Content.Should().NotBeNull();
        
        var createDishDto = new CreateDishDto
        {
            Name = "Блюдо с продуктом",
            Category = DishCategory.Side,
            Ingredients = new List<CreateIngredientDto>
            {
                new() { ProductId = productResult.Content!.Id, AmountInGrams = 100 }
            }
        };
        
        var dishResult = await CreateDishAsync(createDishDto);
        dishResult.Content.Should().NotBeNull();

        // Act: пытаемся удалить продукт
        var deleteResult = await DeleteProductAsync(productResult.Content.Id);

        // Assert
        deleteResult.IsSuccess.Should().BeFalse();
        deleteResult.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        
        deleteResult.GetValidationErrorMessage().Should().Contain("используется в блюдах");
    }

    /// <summary>
    /// Удаление продукта, используемого в нескольких блюдах — 400 Bad Request с перечислением блюд
    /// </summary>
    [Fact(DisplayName = "API: Удаление продукта из нескольких блюд возвращает список блюд")]
    public async Task DeleteProduct_UsedInMultipleDishes_ReturnsAllDishNames()
    {
        // Arrange: создаем продукт и несколько блюд с этим продуктом
        var productResult = await CreateProductAsync("Популярный продукт");
        productResult.Content.Should().NotBeNull();
        
        var dishNames = new[] { "Блюдо 1", "Блюдо 2", "Блюдо 3" };
        foreach (var dishName in dishNames)
        {
            var createDishDto = new CreateDishDto
            {
                Name = dishName,
                Category = DishCategory.Side,
                Ingredients = new List<CreateIngredientDto>
                {
                    new() { ProductId = productResult.Content!.Id, AmountInGrams = 100 }
                }
            };
            
            await CreateDishAsync(createDishDto);
        }

        // Act: пытаемся удалить продукт
        var deleteResult = await DeleteProductAsync(productResult.Content.Id);

        // Assert
        deleteResult.IsSuccess.Should().BeFalse();
        deleteResult.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        
        var errorMessage = deleteResult.GetValidationErrorMessage();
        errorMessage.Should().Contain("используется в блюдах");
        foreach (var dishName in dishNames)
        {
            errorMessage.Should().Contain(dishName);
        }
    }

    /// <summary>
    /// Удаление продукта после удаления всех блюд с ним — 204 No Content
    /// </summary>
    [Fact(DisplayName = "API: Удаление продукта после удаления блюд успешно")]
    public async Task DeleteProduct_AfterDeletingDishes_Succeeds()
    {
        // Arrange: создаем продукт и блюдо
        var productResult = await CreateProductAsync("Продукт с блюдом");
        productResult.Content.Should().NotBeNull();
        
        var createDishDto = new CreateDishDto
        {
            Name = "Блюдо для удаления",
            Category = DishCategory.Side,
            Ingredients = new List<CreateIngredientDto>
            {
                new() { ProductId = productResult.Content!.Id, AmountInGrams = 100 }
            }
        };
        
        var dishResult = await CreateDishAsync(createDishDto);
        dishResult.Content.Should().NotBeNull();
        
        // Удаляем блюдо
        await DeleteDishAsync(dishResult.Content.Id);

        // Act: теперь можем удалить продукт
        var deleteResult = await DeleteProductAsync(productResult.Content.Id);

        // Assert
        deleteResult.IsSuccess.Should().BeTrue();
        deleteResult.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    #endregion
}
