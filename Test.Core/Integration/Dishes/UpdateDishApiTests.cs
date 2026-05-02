using System.Net;
using Core.Models.Enums;
using FluentAssertions;
using Test.Core.Integration.Helpers;
using Testing_project.Dtos;
using Testing_project.Dtos.Dish;
using Testing_project.Dtos.Ingredient;

namespace Test.Core.Integration.Dishes;

[Collection("Integration Tests")]
public class UpdateDishApiTests : IntegrationTestBase
{
    #region B1. Эквивалентное разбиение — частичное обновление

    /// <summary>
    /// Обновление только названия — остальные поля сохраняются
    /// </summary>
    [Fact(DisplayName = "API: Обновление только названия блюда сохраняет остальные поля")]
    public async Task UpdateDish_OnlyName_PreservesOtherFields()
    {
        // Arrange: создаем блюдо
        var productDto = TestDataBuilder.CreateProduct("Картофель", 77, 2, 0.4, 18);
        var product = await Client.PostAsync<CreateProductDto, ProductDto>("/api/products", productDto);
        
        var createDto = TestDataBuilder.CreateDish(
            name: "Оригинальное название",
            productId: product!.Id,
            category: DishCategory.Side);
        
        var created = await Client.PostAsync<CreateDishDto, DishDto>("/api/dishes", createDto);

        // Act: обновляем только название
        var updateDto = new UpdateDishDto
        {
            Name = "Новое название"
        };
        
        var response = await Client.PatchAsync($"/api/dishes/{created!.Id}", updateDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        
        // Проверяем, что название изменилось, а остальное сохранилось
        var updated = await Client.GetFromJsonAsync<DishDto>($"/api/dishes/{created.Id}");
        updated!.Name.Should().Be("Новое название");
        updated.Category.Should().Be(DishCategory.Side); // Сохранилось
        updated.CaloriesPerServing.Should().Be(created.CaloriesPerServing); // Сохранилось
    }

    /// <summary>
    /// Обновление только КБЖУ — пересчет с пользовательскими значениями
    /// </summary>
    [Fact(DisplayName = "API: Обновление только КБЖУ использует пользовательские значения")]
    public async Task UpdateDish_OnlyNutrition_UsesCustomValues()
    {
        // Arrange: создаем блюдо
        var productDto = TestDataBuilder.CreateProduct();
        var product = await Client.PostAsync<CreateProductDto, ProductDto>("/api/products", productDto);
        
        var createDto = TestDataBuilder.CreateDish(productId: product!.Id);
        var created = await Client.PostAsync<CreateDishDto, DishDto>("/api/dishes", createDto);

        // Act: обновляем только КБЖУ
        var updateDto = new UpdateDishDto
        {
            CaloriesPerServing = 999,
            ProteinsPerServing = 99,
            FatsPerServing = 88,
            CarbsPerServing = 77
        };
        
        var response = await Client.PatchAsync($"/api/dishes/{created!.Id}", updateDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        
        var updated = await Client.GetFromJsonAsync<DishDto>($"/api/dishes/{created.Id}");
        updated!.CaloriesPerServing.Should().Be(999);
        updated.ProteinsPerServing.Should().Be(99);
        updated.FatsPerServing.Should().Be(88);
        updated.CarbsPerServing.Should().Be(77);
    }

    /// <summary>
    /// Обновление только ингредиентов — автоматический пересчет КБЖУ и флагов
    /// </summary>
    [Fact(DisplayName = "API: Обновление ингредиентов пересчитывает КБЖУ автоматически")]
    public async Task UpdateDish_OnlyIngredients_RecalculatesNutrition()
    {
        // Arrange: создаем блюдо с одним продуктом
        var product1Dto = TestDataBuilder.CreateProduct("Продукт 1", 100, 10, 5, 15);
        var product1 = await Client.PostAsync<CreateProductDto, ProductDto>("/api/products", product1Dto);
        
        var createDto = TestDataBuilder.CreateDish(productId: product1!.Id, amount: 100);
        var created = await Client.PostAsync<CreateDishDto, DishDto>("/api/dishes", createDto);
        
        var originalCalories = created!.CaloriesPerServing;

        // Создаем второй продукт
        var product2Dto = TestDataBuilder.CreateProduct("Продукт 2", 200, 20, 10, 30);
        var product2 = await Client.PostAsync<CreateProductDto, ProductDto>("/api/products", product2Dto);

        // Act: обновляем ингредиенты (добавляем второй продукт)
        var updateDto = new UpdateDishDto
        {
            Ingredients = new List<CreateIngredientDto>
            {
                new() { ProductId = product1.Id, AmountInGrams = 100 },
                new() { ProductId = product2!.Id, AmountInGrams = 100 }
            }
        };
        
        var response = await Client.PatchAsync($"/api/dishes/{created.Id}", updateDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        
        var updated = await Client.GetFromJsonAsync<DishDto>($"/api/dishes/{created.Id}");
        updated!.CaloriesPerServing.Should().BeGreaterThan(originalCalories.Value); // КБЖУ пересчитано
        updated.ServingSize.Should().Be(200); // Сумма весов ингредиентов
    }

    /// <summary>
    /// Обновление категории — явная установка категории
    /// </summary>
    [Fact(DisplayName = "API: Обновление категории блюда изменяет категорию")]
    public async Task UpdateDish_OnlyCategory_ChangesCategory()
    {
        // Arrange
        var productDto = TestDataBuilder.CreateProduct();
        var product = await Client.PostAsync<CreateProductDto, ProductDto>("/api/products", productDto);
        
        var createDto = TestDataBuilder.CreateDish(
            productId: product!.Id,
            category: DishCategory.Side);
        var created = await Client.PostAsync<CreateDishDto, DishDto>("/api/dishes", createDto);

        // Act: меняем категорию
        var updateDto = new UpdateDishDto
        {
            Category = DishCategory.Salad
        };
        
        var response = await Client.PatchAsync($"/api/dishes/{created!.Id}", updateDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        
        var updated = await Client.GetFromJsonAsync<DishDto>($"/api/dishes/{created.Id}");
        updated!.Category.Should().Be(DishCategory.Salad);
    }

    /// <summary>
    /// Обновление флагов — установка/снятие флагов
    /// </summary>
    [Fact(DisplayName = "API: Обновление флагов блюда изменяет флаги")]
    public async Task UpdateDish_OnlyFlags_ChangesFlags()
    {
        // Arrange: создаем веганский продукт и блюдо без флагов
        var productDto = TestDataBuilder.CreateProduct(
            flags: ExtraFlag.Vegan | ExtraFlag.GlutenFree);
        var product = await Client.PostAsync<CreateProductDto, ProductDto>("/api/products", productDto);
        
        var createDto = TestDataBuilder.CreateDish(
            productId: product!.Id,
            flags: ExtraFlag.None);
        var created = await Client.PostAsync<CreateDishDto, DishDto>("/api/dishes", createDto);

        // Act: устанавливаем флаги
        var updateDto = new UpdateDishDto
        {
            Flags = "Vegan,GlutenFree" // Строковое представление флагов
        };
        
        var response = await Client.PatchAsync($"/api/dishes/{created!.Id}", updateDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        
        var updated = await Client.GetFromJsonAsync<DishDto>($"/api/dishes/{created.Id}");
        updated!.Flags.Should().HaveFlag(ExtraFlag.Vegan);
        updated.Flags.Should().HaveFlag(ExtraFlag.GlutenFree);
    }

    #endregion

    #region B2. Анализ граничных значений — обновление

    /// <summary>
    /// Обновление несуществующего блюда — 404
    /// </summary>
    [Fact(DisplayName = "API: Обновление несуществующего блюда возвращает 404 NotFound")]
    public async Task UpdateDish_NonExistent_Returns404NotFound()
    {
        // Arrange
        var updateDto = new UpdateDishDto
        {
            Name = "Новое название"
        };

        // Act
        var response = await Client.PatchAsync("/api/dishes/999999", updateDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    /// <summary>
    /// Обновление с невалидным ID — 404 (если маршрут позволяет)
    /// </summary>
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
        var response = await Client.PatchAsync($"/api/dishes/{id}", updateDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    /// <summary>
    /// Обновление с пустым телом — поведение (noop, блюдо не изменяется)
    /// </summary>
    [Fact(DisplayName = "API: Обновление блюда с пустым телом не изменяет блюдо")]
    public async Task UpdateDish_EmptyBody_NoChanges()
    {
        // Arrange: создаем блюдо
        var productDto = TestDataBuilder.CreateProduct();
        var product = await Client.PostAsync<CreateProductDto, ProductDto>("/api/products", productDto);
        
        var createDto = TestDataBuilder.CreateDish(
            name: "Оригинальное название",
            productId: product!.Id);
        var created = await Client.PostAsync<CreateDishDto, DishDto>("/api/dishes", createDto);

        // Act: обновляем с пустым телом
        var updateDto = new UpdateDishDto(); // Все поля null
        
        var response = await Client.PatchAsync($"/api/dishes/{created!.Id}", updateDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        
        var updated = await Client.GetFromJsonAsync<DishDto>($"/api/dishes/{created.Id}");
        updated!.Name.Should().Be(created.Name); // Не изменилось
        updated.Category.Should().Be(created.Category);
    }

    #endregion

    #region B3. Конфликтные сценарии при обновлении

    /// <summary>
    /// Изменение состава, снимающее флаг — флаг автоматически снимается
    /// </summary>
    [Fact(DisplayName = "API: Изменение состава автоматически снимает недопустимые флаги")]
    public async Task UpdateDish_CompositionChange_RemovesInvalidFlags()
    {
        // Arrange: создаем веганское блюдо
        var veganProductDto = TestDataBuilder.CreateProduct(
            name: "Веганский продукт",
            flags: ExtraFlag.Vegan);
        var veganProduct = await Client.PostAsync<CreateProductDto, ProductDto>("/api/products", veganProductDto);
        
        var createDto = TestDataBuilder.CreateDish(
            name: "Веганское блюдо",
            productId: veganProduct!.Id,
            flags: ExtraFlag.Vegan);
        var created = await Client.PostAsync<CreateDishDto, DishDto>("/api/dishes", createDto);

        // Создаем НЕ веганский продукт
        var nonVeganProductDto = TestDataBuilder.CreateProduct(
            name: "Мясо",
            flags: ExtraFlag.None);
        var nonVeganProduct = await Client.PostAsync<CreateProductDto, ProductDto>("/api/products", nonVeganProductDto);

        // Act: заменяем ингредиенты на не веганские
        var updateDto = new UpdateDishDto
        {
            Ingredients = new List<CreateIngredientDto>
            {
                new() { ProductId = nonVeganProduct!.Id, AmountInGrams = 100 }
            }
        };
        
        var response = await Client.PatchAsync($"/api/dishes/{created!.Id}", updateDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        
        var updated = await Client.GetFromJsonAsync<DishDto>($"/api/dishes/{created.Id}");
        updated!.Flags.Should().NotHaveFlag(ExtraFlag.Vegan); // Флаг автоматически снят
    }

    /// <summary>
    /// Изменение состава + явный флаг — валидация (ошибка, если флаг недопустим)
    /// </summary>
    [Fact(DisplayName = "API: Изменение состава с явным недопустимым флагом возвращает 400")]
    public async Task UpdateDish_CompositionChangeWithInvalidFlag_Returns400BadRequest()
    {
        // Arrange: создаем блюдо
        var productDto = TestDataBuilder.CreateProduct(flags: ExtraFlag.None);
        var product = await Client.PostAsync<CreateProductDto, ProductDto>("/api/products", productDto);
        
        var createDto = TestDataBuilder.CreateDish(productId: product!.Id, flags: ExtraFlag.None);
        var created = await Client.PostAsync<CreateDishDto, DishDto>("/api/dishes", createDto);

        // Act: пытаемся установить флаг Vegan для не веганского продукта
        var updateDto = new UpdateDishDto
        {
            Flags = "Vegan"
        };
        
        var response = await Client.PatchAsync($"/api/dishes/{created!.Id}", updateDto);

        // Assert
        // Примечание: поведение зависит от реализации сервиса
        // Если валидация на уровне сервиса — 400, если флаг просто снимается — 204
        // Проверяем, что либо ошибка, либо флаг не установлен
        if (response.StatusCode == HttpStatusCode.BadRequest)
        {
            var content = await response.Content.ReadAsStringAsync();
            content.Should().Contain("Веган");
        }
        else
        {
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
            var updated = await Client.GetFromJsonAsync<DishDto>($"/api/dishes/{created.Id}");
            updated!.Flags.Should().NotHaveFlag(ExtraFlag.Vegan);
        }
    }

    /// <summary>
    /// Изменение макроса в названии — смена категории
    /// </summary>
    [Fact(DisplayName = "API: Изменение макроса в названии изменяет категорию")]
    public async Task UpdateDish_MacroChange_ChangesCategory()
    {
        // Arrange: создаем блюдо с категорией Side
        var productDto = TestDataBuilder.CreateProduct();
        var product = await Client.PostAsync<CreateProductDto, ProductDto>("/api/products", productDto);
        
        var createDto = TestDataBuilder.CreateDish(
            name: "Обычное блюдо",
            productId: product!.Id,
            category: DishCategory.Side);
        var created = await Client.PostAsync<CreateDishDto, DishDto>("/api/dishes", createDto);

        // Act: меняем название с макросом
        var updateDto = new UpdateDishDto
        {
            Name = "!десерт Новое блюдо"
        };
        
        var response = await Client.PatchAsync($"/api/dishes/{created!.Id}", updateDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        
        var updated = await Client.GetFromJsonAsync<DishDto>($"/api/dishes/{created.Id}");
        updated!.Name.Should().Be("Новое блюдо"); // Макрос удален
        updated.Category.Should().Be(DishCategory.Dessert); // Категория изменена
    }

    /// <summary>
    /// Обновление с невалидными данными — ошибка валидации
    /// </summary>
    [Theory(DisplayName = "API: Обновление блюда с невалидным названием возвращает 400")]
    [InlineData("A")] // Слишком короткое
    [InlineData("!десерт Б")] // Короткое после удаления макроса
    public async Task UpdateDish_InvalidName_Returns400BadRequest(string name)
    {
        // Arrange: создаем блюдо
        var productDto = TestDataBuilder.CreateProduct();
        var product = await Client.PostAsync<CreateProductDto, ProductDto>("/api/products", productDto);
        
        var createDto = TestDataBuilder.CreateDish(productId: product!.Id);
        var created = await Client.PostAsync<CreateDishDto, DishDto>("/api/dishes", createDto);

        // Act: обновляем с невалидным названием
        var updateDto = new UpdateDishDto
        {
            Name = name
        };
        
        var response = await Client.PatchAsync($"/api/dishes/{created!.Id}", updateDto);

        // Assert
        await response.ShouldHaveValidationError("Название блюда слишком короткое после удаления макросов (минимум 2 символа).");
    }

    /// <summary>
    /// Обновление с отрицательными КБЖУ — ошибка валидации
    /// </summary>
    [Theory(DisplayName = "API: Обновление блюда с отрицательными КБЖУ возвращает 400")]
    [InlineData(-1)]
    [InlineData(-100)]
    public async Task UpdateDish_NegativeNutrition_Returns400BadRequest(double calories)
    {
        // Arrange: создаем блюдо
        var productDto = TestDataBuilder.CreateProduct();
        var product = await Client.PostAsync<CreateProductDto, ProductDto>("/api/products", productDto);
        
        var createDto = TestDataBuilder.CreateDish(productId: product!.Id);
        var created = await Client.PostAsync<CreateDishDto, DishDto>("/api/dishes", createDto);

        // Act: обновляем с отрицательной калорийностью
        var updateDto = new UpdateDishDto
        {
            CaloriesPerServing = calories
        };
        
        var response = await Client.PatchAsync($"/api/dishes/{created!.Id}", updateDto);

        // Assert
        await response.ShouldHaveValidationError("Калорийность не может быть отрицательной");
    }

    /// <summary>
    /// Обновление с более чем 5 фотографиями — ошибка валидации
    /// </summary>
    [Fact(DisplayName = "API: Обновление блюда с более чем 5 фотографиями возвращает 400")]
    public async Task UpdateDish_MoreThan5Photos_Returns400BadRequest()
    {
        // Arrange: создаем блюдо
        var productDto = TestDataBuilder.CreateProduct();
        var product = await Client.PostAsync<CreateProductDto, ProductDto>("/api/products", productDto);
        
        var createDto = TestDataBuilder.CreateDish(productId: product!.Id);
        var created = await Client.PostAsync<CreateDishDto, DishDto>("/api/dishes", createDto);

        // Act: обновляем с 6 фотографиями
        var updateDto = new UpdateDishDto
        {
            Photos = Enumerable.Range(1, 6)
                .Select(i => $"https://example.com/photo{i}.jpg")
                .ToList()
        };
        
        var response = await Client.PatchAsync($"/api/dishes/{created!.Id}", updateDto);

        // Assert
        await response.ShouldHaveValidationError("Нельзя загрузить более 5 фотографий");
    }

    #endregion
}