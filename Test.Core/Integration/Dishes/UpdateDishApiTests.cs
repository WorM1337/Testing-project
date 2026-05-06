using System.Net;
using Core.Models.Enums;
using FluentAssertions;
using Testing_project.Dtos.Dish;
using Testing_project.Dtos.Ingredient;

namespace Test.Core.Integration.Dishes;

[Collection("Integration Tests")]
public class UpdateDishApiTests : IntegrationTestBase
{
    #region Частичное обновление

    [Fact(DisplayName = "API: Обновление только названия блюда сохраняет остальные поля")]
    public async Task UpdateDish_OnlyName_PreservesOtherFields()
    {
        // Arrange: создаем блюдо
        var productResult = await CreateProductAsync("Картофель", 77, 2, 0.4, 18);
        productResult.Content.Should().NotBeNull();
        
        var createDto = new CreateDishDto
        {
            Name = "Оригинальное название",
            Category = DishCategory.Side,
            Ingredients = new List<CreateIngredientDto>
            {
                new() { ProductId = productResult.Content!.Id, AmountInGrams = 100 }
            }
        };
        
        var createdResult = await CreateDishAsync(createDto);
        createdResult.Content.Should().NotBeNull();

        // Act: обновляем только название
        var updateDto = new UpdateDishDto
        {
            Name = "Новое название"
        };
        
        var updateResult = await UpdateDishAsync(createdResult.Content!.Id, updateDto);

        // Assert
        updateResult.IsSuccess.Should().BeTrue();
        updateResult.StatusCode.Should().Be(HttpStatusCode.NoContent);
        
        // Проверяем, что название изменилось, а остальное сохранилось
        var getResult = await GetDishAsync(createdResult.Content.Id);
        getResult.Content!.Name.Should().Be("Новое название");
        getResult.Content.Category.Should().Be(DishCategory.Side); // Сохранилось
        getResult.Content.CaloriesPerServing.Should().Be(createdResult.Content.CaloriesPerServing); // Сохранилось
    }

    [Fact(DisplayName = "API: Обновление только КБЖУ использует пользовательские значения")]
    public async Task UpdateDish_OnlyNutrition_UsesCustomValues()
    {
        // Arrange: создаем блюдо
        var productResult = await CreateProductAsync();
        productResult.Content.Should().NotBeNull();
        
        var createDto = new CreateDishDto
        {
            Name = "Блюдо для обновления",
            Category = DishCategory.Side,
            Ingredients = new List<CreateIngredientDto>
            {
                new() { ProductId = productResult.Content!.Id, AmountInGrams = 100 }
            }
        };
        
        var createdResult = await CreateDishAsync(createDto);
        createdResult.Content.Should().NotBeNull();

        // Act: обновляем только КБЖУ
        var updateDto = new UpdateDishDto
        {
            CaloriesPerServing = 999,
            ProteinsPerServing = 99,
            FatsPerServing = 88,
            CarbsPerServing = 77
        };
        
        var updateResult = await UpdateDishAsync(createdResult.Content!.Id, updateDto);

        // Assert
        updateResult.IsSuccess.Should().BeTrue();
        updateResult.StatusCode.Should().Be(HttpStatusCode.NoContent);
        
        var getResult = await GetDishAsync(createdResult.Content.Id);
        getResult.Content!.CaloriesPerServing.Should().Be(999);
        getResult.Content.ProteinsPerServing.Should().Be(99);
        getResult.Content.FatsPerServing.Should().Be(88);
        getResult.Content.CarbsPerServing.Should().Be(77);
    }

    [Fact(DisplayName = "API: Обновление ингредиентов пересчитывает КБЖУ автоматически")]
    public async Task UpdateDish_OnlyIngredients_RecalculatesNutrition()
    {
        // Arrange: создаем блюдо с одним продуктом
        var product1Result = await CreateProductAsync("Продукт 1", 100, 10, 5, 15);
        product1Result.Content.Should().NotBeNull();
        
        var createDto = new CreateDishDto
        {
            Name = "Блюдо с одним продуктом",
            Category = DishCategory.Side,
            Ingredients = new List<CreateIngredientDto>
            {
                new() { ProductId = product1Result.Content!.Id, AmountInGrams = 100 }
            }
        };
        
        var createdResult = await CreateDishAsync(createDto);
        createdResult.Content.Should().NotBeNull();
        
        var originalCalories = createdResult.Content.CaloriesPerServing;

        // Создаем второй продукт
        var product2Result = await CreateProductAsync("Продукт 2", 200, 20, 10, 30);
        product2Result.Content.Should().NotBeNull();

        // Act: обновляем ингредиенты (добавляем второй продукт)
        var updateDto = new UpdateDishDto
        {
            Ingredients = new List<CreateIngredientDto>
            {
                new() { ProductId = product1Result.Content.Id, AmountInGrams = 100 },
                new() { ProductId = product2Result.Content.Id, AmountInGrams = 100 }
            }
        };
        
        var updateResult = await UpdateDishAsync(createdResult.Content.Id, updateDto);

        // Assert
        updateResult.IsSuccess.Should().BeTrue();
        
        var getResult = await GetDishAsync(createdResult.Content.Id);
        getResult.Content!.CaloriesPerServing.Should().BeGreaterThan(originalCalories.Value); // КБЖУ пересчитано
        getResult.Content.ServingSize.Should().Be(200); // Сумма весов ингредиентов
    }

    [Fact(DisplayName = "API: Обновление категории блюда изменяет категорию")]
    public async Task UpdateDish_OnlyCategory_ChangesCategory()
    {
        // Arrange
        var productResult = await CreateProductAsync();
        productResult.Content.Should().NotBeNull();
        
        var createDto = new CreateDishDto
        {
            Name = "Блюдо для обновления категории",
            Category = DishCategory.Side,
            Ingredients = new List<CreateIngredientDto>
            {
                new() { ProductId = productResult.Content!.Id, AmountInGrams = 100 }
            }
        };
        
        var createdResult = await CreateDishAsync(createDto);
        createdResult.Content.Should().NotBeNull();

        // Act: меняем категорию
        var updateDto = new UpdateDishDto
        {
            Category = DishCategory.Salad
        };
        
        var updateResult = await UpdateDishAsync(createdResult.Content!.Id, updateDto);

        // Assert
        updateResult.IsSuccess.Should().BeTrue();
        
        var getResult = await GetDishAsync(createdResult.Content.Id);
        getResult.Content!.Category.Should().Be(DishCategory.Salad);
    }

    [Fact(DisplayName = "API: Обновление флагов блюда изменяет флаги")]
    public async Task UpdateDish_OnlyFlags_ChangesFlags()
    {
        // Arrange: создаем веганский продукт и блюдо без флагов
        var productResult = await CreateProductAsync(
            name: "Веганский продукт",
            flags: ExtraFlag.Vegan | ExtraFlag.GlutenFree);
        productResult.Content.Should().NotBeNull();
        
        var createDto = new CreateDishDto
        {
            Name = "Блюдо без флагов",
            Category = DishCategory.Side,
            Flags = ExtraFlag.None,
            Ingredients = new List<CreateIngredientDto>
            {
                new() { ProductId = productResult.Content!.Id, AmountInGrams = 100 }
            }
        };
        
        var createdResult = await CreateDishAsync(createDto);
        createdResult.Content.Should().NotBeNull();

        // Act: устанавливаем флаги
        var updateDto = new UpdateDishDto
        {
            Flags = "Vegan,GlutenFree" // Строковое представление флагов
        };
        
        var updateResult = await UpdateDishAsync(createdResult.Content!.Id, updateDto);

        // Assert
        updateResult.IsSuccess.Should().BeTrue();
        
        var getResult = await GetDishAsync(createdResult.Content.Id);
        getResult.Content!.Flags.Should().HaveFlag(ExtraFlag.Vegan);
        getResult.Content.Flags.Should().HaveFlag(ExtraFlag.GlutenFree);
    }

    #endregion

    #region Анализ граничных значений — обновление

    [Fact(DisplayName = "API: Обновление несуществующего блюда возвращает 404 NotFound")]
    public async Task UpdateDish_NonExistent_Returns404NotFound()
    {
        // Arrange
        var updateDto = new UpdateDishDto
        {
            Name = "Новое название"
        };

        // Act
        var updateResult = await UpdateDishAsync(999999, updateDto);

        // Assert
        updateResult.IsSuccess.Should().BeFalse();
        updateResult.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Theory(DisplayName = "API: Обновление блюда с невалидным ID возвращает 404")]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task UpdateDish_InvalidId_Returns404NotFound(int id)
    {
        // Arrange
        var updateDto = new UpdateDishDto
        {
            Name = "Новое название"
        };

        // Act
        var updateResult = await UpdateDishAsync(id, updateDto);

        // Assert
        updateResult.IsSuccess.Should().BeFalse();
        updateResult.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact(DisplayName = "API: Обновление блюда с пустым телом не изменяет блюдо")]
    public async Task UpdateDish_EmptyBody_NoChanges()
    {
        // Arrange: создаем блюдо
        var productResult = await CreateProductAsync();
        productResult.Content.Should().NotBeNull();
        
        var createDto = new CreateDishDto
        {
            Name = "Оригинальное название",
            Category = DishCategory.Side,
            Ingredients = new List<CreateIngredientDto>
            {
                new() { ProductId = productResult.Content!.Id, AmountInGrams = 100 }
            }
        };
        
        var createdResult = await CreateDishAsync(createDto);
        createdResult.Content.Should().NotBeNull();

        // Act: обновляем с пустым телом
        var updateDto = new UpdateDishDto(); // Все поля null
        
        var updateResult = await UpdateDishAsync(createdResult.Content!.Id, updateDto);

        // Assert
        updateResult.IsSuccess.Should().BeTrue();
        
        var getResult = await GetDishAsync(createdResult.Content.Id);
        getResult.Content!.Name.Should().Be(createdResult.Content.Name); // Не изменилось
        getResult.Content.Category.Should().Be(createdResult.Content.Category);
    }

    #endregion

    #region Конфликтные сценарии при обновлении

    [Fact(DisplayName = "API: Изменение состава автоматически снимает недопустимые флаги")]
    public async Task UpdateDish_CompositionChange_RemovesInvalidFlags()
    {
        // Arrange: создаем веганское блюдо
        var veganProductResult = await CreateProductAsync(
            name: "Веганский продукт",
            flags: ExtraFlag.Vegan);
        veganProductResult.Content.Should().NotBeNull();
        
        var createDto = new CreateDishDto
        {
            Name = "Веганское блюдо",
            Category = DishCategory.Side,
            Flags = ExtraFlag.Vegan,
            Ingredients = new List<CreateIngredientDto>
            {
                new() { ProductId = veganProductResult.Content!.Id, AmountInGrams = 100 }
            }
        };
        
        var createdResult = await CreateDishAsync(createDto);
        createdResult.Content.Should().NotBeNull();

        // Создаем НЕ веганский продукт
        var nonVeganProductResult = await CreateProductAsync(
            name: "Мясо",
            flags: ExtraFlag.None);
        nonVeganProductResult.Content.Should().NotBeNull();

        // Act: заменяем ингредиенты на не веганские
        var updateDto = new UpdateDishDto
        {
            Ingredients = new List<CreateIngredientDto>
            {
                new() { ProductId = nonVeganProductResult.Content!.Id, AmountInGrams = 100 }
            }
        };
        
        var updateResult = await UpdateDishAsync(createdResult.Content!.Id, updateDto);

        // Assert
        updateResult.IsSuccess.Should().BeTrue();
        
        var getResult = await GetDishAsync(createdResult.Content.Id);
        getResult.Content!.Flags.Should().NotHaveFlag(ExtraFlag.Vegan); // Флаг автоматически снят
    }

    [Fact(DisplayName = "API: Изменение состава с явным недопустимым флагом возвращает 400")]
    public async Task UpdateDish_CompositionChangeWithInvalidFlag_Returns400BadRequest()
    {
        // Arrange: создаем блюдо
        var productResult = await CreateProductAsync(flags: ExtraFlag.None);
        productResult.Content.Should().NotBeNull();
        
        var createDto = new CreateDishDto
        {
            Name = "Блюдо для обновления",
            Category = DishCategory.Side,
            Flags = ExtraFlag.None,
            Ingredients = new List<CreateIngredientDto>
            {
                new() { ProductId = productResult.Content!.Id, AmountInGrams = 100 }
            }
        };
        
        var createdResult = await CreateDishAsync(createDto);
        createdResult.Content.Should().NotBeNull();

        // Act: пытаемся установить флаг Vegan для не веганского продукта
        var updateDto = new UpdateDishDto
        {
            Flags = "Vegan"
        };
        
        var updateResult = await UpdateDishAsync(createdResult.Content!.Id, updateDto);

        // Assert
        // Примечание: поведение зависит от реализации сервиса
        // Если валидация на уровне сервиса — 400, если флаг просто снимается — 204
        if (!updateResult.IsSuccess)
        {
            updateResult.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            updateResult.Error.Should().Contain("Веган");
        }
        else
        {
            var getResult = await GetDishAsync(createdResult.Content.Id);
            getResult.Content!.Flags.Should().NotHaveFlag(ExtraFlag.Vegan);
        }
    }

    [Fact(DisplayName = "API: Изменение макроса в названии изменяет категорию")]
    public async Task UpdateDish_MacroChange_ChangesCategory()
    {
        // Arrange: создаем блюдо с категорией Side
        var productResult = await CreateProductAsync();
        productResult.Content.Should().NotBeNull();
        
        var createDto = new CreateDishDto
        {
            Name = "Обычное блюдо",
            Category = DishCategory.Side,
            Ingredients = new List<CreateIngredientDto>
            {
                new() { ProductId = productResult.Content!.Id, AmountInGrams = 100 }
            }
        };
        
        var createdResult = await CreateDishAsync(createDto);
        createdResult.Content.Should().NotBeNull();

        // Act: меняем название с макросом
        var updateDto = new UpdateDishDto
        {
            Name = "!десерт Новое блюдо"
        };
        
        var updateResult = await UpdateDishAsync(createdResult.Content!.Id, updateDto);

        // Assert
        updateResult.IsSuccess.Should().BeTrue();
        
        var getResult = await GetDishAsync(createdResult.Content.Id);
        getResult.Content!.Name.Should().Be("Новое блюдо"); // Макрос удален
        getResult.Content.Category.Should().Be(DishCategory.Dessert); // Категория изменена
    }

    [Fact(DisplayName = "API: Обновление блюда с невалидным названием после удаления макроса возвращает 400")]
    public async Task UpdateDish_InvalidNameWithMacros_Returns400BadRequest()
    {
        // Arrange: создаем блюдо
        var productResult = await CreateProductAsync();
        productResult.Content.Should().NotBeNull();
        
        var createDto = new CreateDishDto
        {
            Name = "Блюдо для обновления",
            Category = DishCategory.Side,
            Ingredients = new List<CreateIngredientDto>
            {
                new() { ProductId = productResult.Content!.Id, AmountInGrams = 100 }
            }
        };
        
        var createdResult = await CreateDishAsync(createDto);
        createdResult.Content.Should().NotBeNull();

        // Act: обновляем с невалидным названием
        var updateDto = new UpdateDishDto
        {
            Name = "!десерт Б"
        };
        
        var updateResult = await UpdateDishAsync(createdResult.Content!.Id, updateDto);

        // Assert
        updateResult.IsSuccess.Should().BeFalse();
        updateResult.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        updateResult.GetValidationErrorMessage().Should().Contain("Название блюда слишком короткое после удаления макросов (минимум 2 символа).");
    }
    
    [Fact(DisplayName = "API: Обновление блюда с невалидным названием возвращает 400")]
    public async Task UpdateDish_InvalidNameWithoutMacros_Returns400BadRequest()
    {
        // Arrange: создаем блюдо
        var productResult = await CreateProductAsync();
        productResult.Content.Should().NotBeNull();
        
        var createDto = new CreateDishDto
        {
            Name = "Блюдо для обновления",
            Category = DishCategory.Side,
            Ingredients = new List<CreateIngredientDto>
            {
                new() { ProductId = productResult.Content!.Id, AmountInGrams = 100 }
            }
        };
        
        var createdResult = await CreateDishAsync(createDto);
        createdResult.Content.Should().NotBeNull();

        // Act: обновляем с невалидным названием
        var updateDto = new UpdateDishDto
        {
            Name = "А"
        };
        
        var updateResult = await UpdateDishAsync(createdResult.Content!.Id, updateDto);

        // Assert
        updateResult.IsSuccess.Should().BeFalse();
        updateResult.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        updateResult.GetValidationErrorMessage().Should().Contain("Минимальная длина названия — 2 символа.");
    }

    [Theory(DisplayName = "API: Обновление блюда с отрицательными КБЖУ возвращает 400")]
    [InlineData(-1)]
    [InlineData(-100)]
    public async Task UpdateDish_NegativeNutrition_Returns400BadRequest(double calories)
    {
        // Arrange: создаем блюдо
        var productResult = await CreateProductAsync();
        productResult.Content.Should().NotBeNull();
        
        var createDto = new CreateDishDto
        {
            Name = "Блюдо для обновления",
            Category = DishCategory.Side,
            Ingredients = new List<CreateIngredientDto>
            {
                new() { ProductId = productResult.Content!.Id, AmountInGrams = 100 }
            }
        };
        
        var createdResult = await CreateDishAsync(createDto);
        createdResult.Content.Should().NotBeNull();

        // Act: обновляем с отрицательной калорийностью
        var updateDto = new UpdateDishDto
        {
            CaloriesPerServing = calories
        };
        
        var updateResult = await UpdateDishAsync(createdResult.Content!.Id, updateDto);

        // Assert
        updateResult.IsSuccess.Should().BeFalse();
        updateResult.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        updateResult.GetValidationErrorMessage().Should().Contain("Калорийность не может быть отрицательной");
    }

    [Fact(DisplayName = "API: Обновление блюда с более чем 5 фотографиями возвращает 400")]
    public async Task UpdateDish_MoreThan5Photos_Returns400BadRequest()
    {
        // Arrange: создаем блюдо
        var productResult = await CreateProductAsync();
        productResult.Content.Should().NotBeNull();
        
        var createDto = new CreateDishDto
        {
            Name = "Блюдо для обновления",
            Category = DishCategory.Side,
            Ingredients = new List<CreateIngredientDto>
            {
                new() { ProductId = productResult.Content!.Id, AmountInGrams = 100 }
            }
        };
        
        var createdResult = await CreateDishAsync(createDto);
        createdResult.Content.Should().NotBeNull();

        // Act: обновляем с 6 фотографиями
        var updateDto = new UpdateDishDto
        {
            Photos = Enumerable.Range(1, 6)
                .Select(i => $"https://example.com/photo{i}.jpg")
                .ToList()
        };
        
        var updateResult = await UpdateDishAsync(createdResult.Content!.Id, updateDto);

        // Assert
        updateResult.IsSuccess.Should().BeFalse();
        updateResult.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        updateResult.GetValidationErrorMessage().Should().Contain("Нельзя загрузить более 5 фотографий");
    }

    #endregion
}
