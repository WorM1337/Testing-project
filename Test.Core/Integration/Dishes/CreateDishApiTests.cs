using System.Net;
using System.Net.Http.Json;
using Core.Models.Enums;
using FluentAssertions;
using Test.Core.Integration.Fixtures;
using Test.Core.Integration.Helpers;
using Testing_project.Dtos;
using Testing_project.Dtos.Dish;
using Testing_project.Dtos.Ingredient;
using Test.Core.Integration.Helpers;
using static Test.Core.Integration.Helpers.ApiHelpers;

namespace Test.Core.Integration.Dishes;

/// <summary>
/// Интеграционные тесты для создания блюд (POST /api/dishes)
/// НЕИЗОЛИРОВАННАЯ среда: данные НЕ очищаются между тестами
/// </summary>
[Collection("Integration Tests")]
public class CreateDishApiTests : IntegrationTestBase
{
    public CreateDishApiTests(ApiTestFixture fixture) : base(fixture) { }

    #region A1. Эквивалентное разбиение — валидные данные

    /// <summary>
    /// Создание блюда с валидными данными и явной категорией
    /// </summary>
    [Fact(DisplayName = "API: Создание блюда с валидными данными возвращает 201 Created")]
    public async Task CreateDish_ValidData_Returns201Created()
    {
        // Arrange: создаем тестовый продукт
        var productDto = TestDataBuilder.CreateProduct("Картофель", 77, 2, 0.4, 18);
        var product = await Client.PostAsync<CreateProductDto, ProductDto>("/api/products", productDto);
        
        var createDto = TestDataBuilder.CreateDish(
            name: "Картофельное пюре",
            productId: product!.Id,
            amount: 200,
            category: DishCategory.Side);

        // Act
        var response = await Client.PostAsJsonAsync("/api/dishes", createDto, JsonOptions);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var result = await response.Content.ReadFromJsonAsync<DishDto>(JsonOptions);
        result.Should().NotBeNull();
        result!.Name.Should().Be("Картофельное пюре");
        result.Category.Should().Be(DishCategory.Side);
        result.Id.Should().BeGreaterThan(0);
    }

    /// <summary>
    /// Создание блюда с макросом в названии — категория определяется автоматически
    /// </summary>
    [Theory(DisplayName = "API: Создание блюда с макросом автоматически определяет категорию")]
    [InlineData("!десерт Тирамису", DishCategory.Dessert, "Тирамису")]
    [InlineData("!первое Борщ", DishCategory.Entree, "Борщ")]
    [InlineData("!салат Цезарь", DishCategory.Salad, "Цезарь")]
    [InlineData("!напиток Смузи", DishCategory.Drink, "Смузи")]
    [InlineData("!суп Харчо", DishCategory.Soup, "Харчо")]
    [InlineData("!перекус Снэк", DishCategory.Snack, "Снэк")]
    public async Task CreateDish_WithMacro_AutomaticallyDeterminesCategory(
        string nameWithMacro, 
        DishCategory expectedCategory,
        string expectedCleanName)
    {
        // Arrange
        var productDto = TestDataBuilder.CreateProduct();
        var product = await Client.PostAsync<CreateProductDto, ProductDto>("/api/products", productDto);
        
        var createDto = TestDataBuilder.CreateDish(
            name: nameWithMacro,
            productId: product!.Id,
            category: DishCategory.None); // Категория НЕ указана

        // Act
        var response = await Client.PostAsJsonAsync("/api/dishes", createDto, JsonOptions);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var result = await response.Content.ReadFromJsonAsync<DishDto>(JsonOptions);
        result.Should().NotBeNull();
        result!.Name.Should().Be(expectedCleanName); // Макрос удален
        result.Category.Should().Be(expectedCategory); // Категория определена автоматически
    }

    /// <summary>
    /// Создание блюда с переопределением КБЖУ — используются пользовательские значения
    /// </summary>
    [Fact(DisplayName = "API: Создание блюда с переопределением КБЖУ использует пользовательские значения")]
    public async Task CreateDish_WithNutritionOverride_UsesCustomValues()
    {
        // Arrange
        var productDto = TestDataBuilder.CreateProduct(calories: 100, proteins: 10, fats: 5, carbs: 15);
        var product = await Client.PostAsync<CreateProductDto, ProductDto>("/api/products", productDto);
        
        var createDto = new CreateDishDto
        {
            Name = "Блюдо с переопределением",
            Category = DishCategory.Side,
            Ingredients = new List<CreateIngredientDto>
            {
                new() { ProductId = product!.Id, AmountInGrams = 100 }
            },
            // Переопределяем КБЖУ (вместо автоматического расчета)
            CaloriesPerServing = 500,
            ProteinsPerServing = 50,
            FatsPerServing = 30,
            CarbsPerServing = 60,
            ServingSize = 250
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/dishes", createDto, JsonOptions);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var result = await response.Content.ReadFromJsonAsync<DishDto>(JsonOptions);
        result.Should().NotBeNull();
        result!.CaloriesPerServing.Should().Be(500); // Пользовательское значение
        result.ProteinsPerServing.Should().Be(50);
        result.FatsPerServing.Should().Be(30);
        result.CarbsPerServing.Should().Be(60);
        result.ServingSize.Should().Be(250);
    }

    /// <summary>
    /// Создание блюда с флагами — флаги устанавливаются, если все ингредиенты поддерживают
    /// </summary>
    [Fact(DisplayName = "API: Создание блюда с флагами успешно, если все ингредиенты поддерживают")]
    public async Task CreateDish_WithFlags_SucceedsWhenAllIngredientsSupport()
    {
        // Arrange: создаем веганский продукт
        var productDto = TestDataBuilder.CreateProduct(
            name: "Соевое молоко",
            flags: ExtraFlag.Vegan | ExtraFlag.GlutenFree);
        var product = await Client.PostAsync<CreateProductDto, ProductDto>("/api/products", productDto);
        
        var createDto = TestDataBuilder.CreateDish(
            name: "Веганский смузи",
            productId: product!.Id,
            category: DishCategory.Drink,
            flags: ExtraFlag.Vegan | ExtraFlag.GlutenFree);

        // Act
        var response = await Client.PostAsJsonAsync("/api/dishes", createDto, JsonOptions);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var result = await response.Content.ReadFromJsonAsync<DishDto>(JsonOptions);
        result.Should().NotBeNull();
        result!.Flags.Should().HaveFlag(ExtraFlag.Vegan);
        result.Flags.Should().HaveFlag(ExtraFlag.GlutenFree);
    }

    /// <summary>
    /// Создание блюда с несколькими продуктами — КБЖУ рассчитывается автоматически
    /// </summary>
    [Theory(DisplayName = "API: Создание блюда с несколькими продуктами рассчитывает КБЖУ")]
    [InlineData(3)] // 3 продукта
    [InlineData(5)] // 5 продуктов
    public async Task CreateDish_WithMultipleProducts_CalculatesNutrition(int productCount)
    {
        // Arrange: создаем несколько продуктов
        var productIds = new List<int>();
        for (int i = 0; i < productCount; i++)
        {
            var productDto = TestDataBuilder.CreateProduct($"Продукт {i + 1}", 100, 10, 5, 15);
            var product = await Client.PostAsync<CreateProductDto, ProductDto>("/api/products", productDto);
            productIds.Add(product!.Id);
        }
        
        var createDto = new CreateDishDto
        {
            Name = $"Блюдо из {productCount} продуктов",
            Category = DishCategory.Side,
            Ingredients = productIds.Select(id => new CreateIngredientDto 
            { 
                ProductId = id, 
                AmountInGrams = 100 
            }).ToList()
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/dishes", createDto, JsonOptions);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var result = await response.Content.ReadFromJsonAsync<DishDto>(JsonOptions);
        result.Should().NotBeNull();
        result!.CaloriesPerServing.Should().BeApproximately(100 * productCount, 0.01);
        result.ServingSize.Should().Be(100 * productCount); // Сумма веса ингредиентов
    }

    #endregion

    #region A2. Эквивалентное разбиение — невалидные данные (валидация)

    /// <summary>
    /// Создание блюда с пустым названием — ошибка валидации
    /// </summary>
    [Theory(DisplayName = "API: Создание блюда с пустым названием возвращает 400 BadRequest")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task CreateDish_EmptyName_Returns400BadRequest(string? name)
    {
        // Arrange
        var productDto = TestDataBuilder.CreateProduct();
        var product = await Client.PostAsync<CreateProductDto, ProductDto>("/api/products", productDto);
        
        var createDto = new CreateDishDto
        {
            Name = name!,
            Category = DishCategory.Side,
            Ingredients = new List<CreateIngredientDto>
            {
                new() { ProductId = product!.Id, AmountInGrams = 100 }
            }
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/dishes", createDto, JsonOptions);

        // Assert
        await response.ShouldHaveValidationError("Название блюда обязательно");
    }

    /// <summary>
    /// Создание блюда с названием короче 2 символов — ошибка валидации
    /// </summary>
    [Theory(DisplayName = "API: Создание блюда с коротким названием возвращает 400 BadRequest")]
    [InlineData("A")]
    [InlineData("Б")]
    public async Task CreateDish_NameTooShort_Returns400BadRequest(string name)
    {
        // Arrange
        var productDto = TestDataBuilder.CreateProduct();
        var product = await Client.PostAsync<CreateProductDto, ProductDto>("/api/products", productDto);
        
        var createDto = TestDataBuilder.CreateDish(name: name, productId: product!.Id);

        // Act
        var response = await Client.PostAsJsonAsync("/api/dishes", createDto, JsonOptions);

        // Assert
        await response.ShouldHaveValidationError("Минимальная длина названия — 2 символа");
    }

    /// <summary>
    /// Создание блюда с макросом, но чистое название короче 2 символов — ошибка валидации
    /// </summary>
    [Theory(DisplayName = "API: Создание блюда с макросом, но коротким чистым названием возвращает 400")]
    [InlineData("!десерт A")]
    [InlineData("!первое Б")]
    public async Task CreateDish_MacroButShortCleanName_Returns400BadRequest(string name)
    {
        // Arrange
        var productDto = TestDataBuilder.CreateProduct();
        var product = await Client.PostAsync<CreateProductDto, ProductDto>("/api/products", productDto);
        
        var createDto = TestDataBuilder.CreateDish(name: name, productId: product!.Id);

        // Act
        var response = await Client.PostAsJsonAsync("/api/dishes", createDto, JsonOptions);

        // Assert
        await response.ShouldHaveValidationError("слишком короткое после удаления макросов");
    }

    /// <summary>
    /// Создание блюда без категории и без макроса — ошибка валидации
    /// </summary>
    [Fact(DisplayName = "API: Создание блюда без категории и макроса возвращает 400 BadRequest")]
    public async Task CreateDish_NoCategoryAndNoMacro_Returns400BadRequest()
    {
        // Arrange
        var productDto = TestDataBuilder.CreateProduct();
        var product = await Client.PostAsync<CreateProductDto, ProductDto>("/api/products", productDto);
        
        var createDto = new CreateDishDto
        {
            Name = "Простое блюдо", // Без макроса
            Category = DishCategory.None, // Категория не указана
            Ingredients = new List<CreateIngredientDto>
            {
                new() { ProductId = product!.Id, AmountInGrams = 100 }
            }
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/dishes", createDto, JsonOptions);

        // Assert
        await response.ShouldHaveValidationError("Категория блюда обязательна");
    }

    /// <summary>
    /// Создание блюда с более чем 5 фотографиями — ошибка валидации
    /// </summary>
    [Theory(DisplayName = "API: Создание блюда с более чем 5 фотографиями возвращает 400 BadRequest")]
    [InlineData(6)]
    [InlineData(10)]
    public async Task CreateDish_MoreThan5Photos_Returns400BadRequest(int photoCount)
    {
        // Arrange
        var productDto = TestDataBuilder.CreateProduct();
        var product = await Client.PostAsync<CreateProductDto, ProductDto>("/api/products", productDto);
        
        var photos = Enumerable.Range(1, photoCount)
            .Select(i => $"https://example.com/photo{i}.jpg")
            .ToList();
        
        var createDto = new CreateDishDto
        {
            Name = "Блюдо с фото",
            Category = DishCategory.Side,
            Photos = photos,
            Ingredients = new List<CreateIngredientDto>
            {
                new() { ProductId = product!.Id, AmountInGrams = 100 }
            }
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/dishes", createDto, JsonOptions);

        // Assert
        await response.ShouldHaveValidationError("Нельзя загрузить более 5 фотографий");
    }

    /// <summary>
    /// Создание блюда с пустым списком ингредиентов — ошибка валидации
    /// </summary>
    [Fact(DisplayName = "API: Создание блюда с пустым списком ингредиентов возвращает 400 BadRequest")]
    public async Task CreateDish_EmptyIngredients_Returns400BadRequest()
    {
        // Arrange
        var createDto = new CreateDishDto
        {
            Name = "Блюдо без ингредиентов",
            Category = DishCategory.Side,
            Ingredients = new List<CreateIngredientDto>() // Пустой список
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/dishes", createDto, JsonOptions);

        // Assert
        await response.ShouldHaveValidationError("Должен быть хотя бы один ингредиент");
    }

    /// <summary>
    /// Создание блюда с null в списке ингредиентов — ошибка валидации
    /// </summary>
    [Fact(DisplayName = "API: Создание блюда с null в ингредиентах возвращает 400 BadRequest")]
    public async Task CreateDish_NullIngredient_Returns400BadRequest()
    {
        // Arrange
        var createDto = new CreateDishDto
        {
            Name = "Блюдо с null ингредиентом",
            Category = DishCategory.Side,
            Ingredients = new List<CreateIngredientDto> { null! }
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/dishes", createDto, JsonOptions);

        // Assert
        await response.ShouldHaveValidationError("пустые значения");
    }

    /// <summary>
    /// Создание блюда с невалидным ProductId (0 или отрицательный) — ошибка валидации
    /// </summary>
    [Theory(DisplayName = "API: Создание блюда с невалидным ProductId возвращает 400 BadRequest")]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public async Task CreateDish_InvalidProductId_Returns400BadRequest(int productId)
    {
        // Arrange
        var createDto = new CreateDishDto
        {
            Name = "Блюдо с невалидным ProductId",
            Category = DishCategory.Side,
            Ingredients = new List<CreateIngredientDto>
            {
                new() { ProductId = productId, AmountInGrams = 100 }
            }
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/dishes", createDto, JsonOptions);

        // Assert
        await response.ShouldHaveValidationError("ID продукта должен быть больше нуля");
    }

    /// <summary>
    /// Создание блюда с невалидным AmountInGrams (0 или отрицательный) — ошибка валидации
    /// </summary>
    [Theory(DisplayName = "API: Создание блюда с невалидным количеством возвращает 400 BadRequest")]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public async Task CreateDish_InvalidAmount_Returns400BadRequest(double amount)
    {
        // Arrange
        var productDto = TestDataBuilder.CreateProduct();
        var product = await Client.PostAsync<CreateProductDto, ProductDto>("/api/products", productDto);
        
        var createDto = new CreateDishDto
        {
            Name = "Блюдо с невалидным количеством",
            Category = DishCategory.Side,
            Ingredients = new List<CreateIngredientDto>
            {
                new() { ProductId = product!.Id, AmountInGrams = amount }
            }
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/dishes", createDto, JsonOptions);

        // Assert
        await response.ShouldHaveValidationError("Количество продукта должно быть больше нуля");
    }

    /// <summary>
    /// Создание блюда с несуществующим ProductId — ошибка бизнес-логики
    /// </summary>
    [Fact(DisplayName = "API: Создание блюда с несуществующим ProductId возвращает 400 BadRequest")]
    public async Task CreateDish_NonExistentProductId_Returns400BadRequest()
    {
        // Arrange
        var createDto = new CreateDishDto
        {
            Name = "Блюдо с несуществующим продуктом",
            Category = DishCategory.Side,
            Ingredients = new List<CreateIngredientDto>
            {
                new() { ProductId = 999999, AmountInGrams = 100 } // Несуществующий ID
            }
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/dishes", createDto, JsonOptions);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("не найден");
    }

    #endregion

    #region A3. Анализ граничных значений — КБЖУ

    /// <summary>
    /// Создание блюда с калорийностью = 0 — граничное значение (валидно)
    /// </summary>
    [Fact(DisplayName = "API: Создание блюда с калорийностью 0 успешно")]
    public async Task CreateDish_CaloriesZero_Succeeds()
    {
        // Arrange
        var productDto = TestDataBuilder.CreateProduct();
        var product = await Client.PostAsync<CreateProductDto, ProductDto>("/api/products", productDto);
        
        var createDto = new CreateDishDto
        {
            Name = "Блюдо с нулевой калорийностью",
            Category = DishCategory.Side,
            CaloriesPerServing = 0, // Граничное значение
            Ingredients = new List<CreateIngredientDto>
            {
                new() { ProductId = product!.Id, AmountInGrams = 100 }
            }
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/dishes", createDto, JsonOptions);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    /// <summary>
    /// Создание блюда с отрицательной калорийностью — ошибка валидации
    /// </summary>
    [Theory(DisplayName = "API: Создание блюда с отрицательной калорийностью возвращает 400")]
    [InlineData(-0.1)]
    [InlineData(-1)]
    [InlineData(-100)]
    public async Task CreateDish_NegativeCalories_Returns400BadRequest(double calories)
    {
        // Arrange
        var productDto = TestDataBuilder.CreateProduct();
        var product = await Client.PostAsync<CreateProductDto, ProductDto>("/api/products", productDto);
        
        var createDto = new CreateDishDto
        {
            Name = "Блюдо с отрицательной калорийностью",
            Category = DishCategory.Side,
            CaloriesPerServing = calories,
            Ingredients = new List<CreateIngredientDto>
            {
                new() { ProductId = product!.Id, AmountInGrams = 100 }
            }
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/dishes", createDto, JsonOptions);

        // Assert
        await response.ShouldHaveValidationError("Калорийность не может быть отрицательной");
    }

    /// <summary>
    /// Создание блюда с очень большими значениями КБЖУ — проверка на overflow
    /// </summary>
    [Fact(DisplayName = "API: Создание блюда с очень большими КБЖУ успешно")]
    public async Task CreateDish_VeryLargeNutrition_Succeeds()
    {
        // Arrange
        var productDto = TestDataBuilder.CreateProduct();
        var product = await Client.PostAsync<CreateProductDto, ProductDto>("/api/products", productDto);
        
        var createDto = new CreateDishDto
        {
            Name = "Блюдо с большими КБЖУ",
            Category = DishCategory.Side,
            CaloriesPerServing = 999999.99,
            ProteinsPerServing = 999999.99,
            FatsPerServing = 999999.99,
            CarbsPerServing = 999999.99,
            Ingredients = new List<CreateIngredientDto>
            {
                new() { ProductId = product!.Id, AmountInGrams = 100 }
            }
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/dishes", createDto, JsonOptions);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    /// <summary>
    /// Создание блюда с размером порции 0.1 грамма — минимальное положительное значение
    /// </summary>
    [Fact(DisplayName = "API: Создание блюда с размером порции 0.1г успешно")]
    public async Task CreateDish_ServingSize0Point1_Succeeds()
    {
        // Arrange
        var productDto = TestDataBuilder.CreateProduct();
        var product = await Client.PostAsync<CreateProductDto, ProductDto>("/api/products", productDto);
        
        var createDto = new CreateDishDto
        {
            Name = "Блюдо с минимальной порцией",
            Category = DishCategory.Side,
            ServingSize = 0.1, // Минимальное положительное
            Ingredients = new List<CreateIngredientDto>
            {
                new() { ProductId = product!.Id, AmountInGrams = 100 }
            }
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/dishes", createDto, JsonOptions);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    /// <summary>
    /// Создание блюда с размером порции 0 или отрицательным — ошибка валидации
    /// </summary>
    [Theory(DisplayName = "API: Создание блюда с невалидным размером порции возвращает 400")]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public async Task CreateDish_InvalidServingSize_Returns400BadRequest(double servingSize)
    {
        // Arrange
        var productDto = TestDataBuilder.CreateProduct();
        var product = await Client.PostAsync<CreateProductDto, ProductDto>("/api/products", productDto);
        
        var createDto = new CreateDishDto
        {
            Name = "Блюдо с невалидной порцией",
            Category = DishCategory.Side,
            ServingSize = servingSize,
            Ingredients = new List<CreateIngredientDto>
            {
                new() { ProductId = product!.Id, AmountInGrams = 100 }
            }
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/dishes", createDto, JsonOptions);

        // Assert
        await response.ShouldHaveValidationError("Размер порции должен быть больше 0");
    }

    #endregion

    #region A4. Анализ граничных значений — количество ингредиентов

    /// <summary>
    /// Создание блюда с 1 ингредиентом — минимально валидно
    /// </summary>
    [Fact(DisplayName = "API: Создание блюда с 1 ингредиентом успешно")]
    public async Task CreateDish_OneIngredient_Succeeds()
    {
        // Arrange
        var productDto = TestDataBuilder.CreateProduct();
        var product = await Client.PostAsync<CreateProductDto, ProductDto>("/api/products", productDto);
        
        var createDto = TestDataBuilder.CreateDish(productId: product!.Id);

        // Act
        var response = await Client.PostAsJsonAsync("/api/dishes", createDto, JsonOptions);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    /// <summary>
    /// Создание блюда с 10+ ингредиентами — проверка производительности
    /// </summary>
    [Theory(DisplayName = "API: Создание блюда с большим количеством ингредиентов успешно")]
    [InlineData(10)]
    [InlineData(20)]
    public async Task CreateDish_ManyIngredients_Succeeds(int ingredientCount)
    {
        // Arrange: создаем много продуктов
        var productIds = new List<int>();
        for (int i = 0; i < ingredientCount; i++)
        {
            var productDto = TestDataBuilder.CreateProduct($"Продукт {i + 1}");
            var product = await Client.PostAsync<CreateProductDto, ProductDto>("/api/products", productDto);
            productIds.Add(product!.Id);
        }
        
        var createDto = new CreateDishDto
        {
            Name = $"Блюдо с {ingredientCount} ингредиентами",
            Category = DishCategory.Side,
            Ingredients = productIds.Select(id => new CreateIngredientDto 
            { 
                ProductId = id, 
                AmountInGrams = 100 
            }).ToList()
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/dishes", createDto, JsonOptions);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var result = await response.Content.ReadFromJsonAsync<DishDto>(JsonOptions);
        result!.Ingredients.Should().HaveCount(ingredientCount);
    }

    #endregion

    #region A5. Флаги — конфликтные сценарии

    /// <summary>
    /// Создание блюда с флагом Vegan, но ингредиент без флага — ошибка бизнес-логики
    /// </summary>
    [Fact(DisplayName = "API: Создание блюда с флагом Vegan, но не веганским продуктом возвращает 400")]
    public async Task CreateDish_VeganFlagButNonVeganProduct_Returns400BadRequest()
    {
        // Arrange: создаем НЕ веганский продукт
        var productDto = TestDataBuilder.CreateProduct(
            name: "Куриное филе",
            flags: ExtraFlag.None); // Без флага Vegan
        var product = await Client.PostAsync<CreateProductDto, ProductDto>("/api/products", productDto);
        
        var createDto = TestDataBuilder.CreateDish(
            name: "Псевдо-веганское блюдо",
            productId: product!.Id,
            category: DishCategory.Side,
            flags: ExtraFlag.Vegan); // Пытаемся установить флаг Vegan

        // Act
        var response = await Client.PostAsJsonAsync("/api/dishes", createDto, JsonOptions);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Веган");
    }

    /// <summary>
    /// Создание блюда с флагом GlutenFree, но ингредиент без флага — ошибка бизнес-логики
    /// </summary>
    [Fact(DisplayName = "API: Создание блюда с флагом GlutenFree, но продуктом с глютеном возвращает 400")]
    public async Task CreateDish_GlutenFreeFlagButGlutenProduct_Returns400BadRequest()
    {
        // Arrange: создаем продукт с глютеном
        var productDto = TestDataBuilder.CreateProduct(
            name: "Пшеничная мука",
            flags: ExtraFlag.None); // Без флага GlutenFree
        var product = await Client.PostAsync<CreateProductDto, ProductDto>("/api/products", productDto);
        
        var createDto = TestDataBuilder.CreateDish(
            name: "Псевдо-безглютеновое блюдо",
            productId: product!.Id,
            category: DishCategory.Side,
            flags: ExtraFlag.GlutenFree); // Пытаемся установить флаг GlutenFree

        // Act
        var response = await Client.PostAsJsonAsync("/api/dishes", createDto, JsonOptions);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Без глютена");
    }

    /// <summary>
    /// Создание блюда с флагом SugarFree, но ингредиент без флага — ошибка бизнес-логики
    /// </summary>
    [Fact(DisplayName = "API: Создание блюда с флагом SugarFree, но продуктом с сахаром возвращает 400")]
    public async Task CreateDish_SugarFreeFlagButSugarProduct_Returns400BadRequest()
    {
        // Arrange: создаем продукт с сахаром
        var productDto = TestDataBuilder.CreateProduct(
            name: "Сахар",
            flags: ExtraFlag.None); // Без флага SugarFree
        var product = await Client.PostAsync<CreateProductDto, ProductDto>("/api/products", productDto);
        
        var createDto = TestDataBuilder.CreateDish(
            name: "Псевдо-без сахара блюдо",
            productId: product!.Id,
            category: DishCategory.Side,
            flags: ExtraFlag.SugarFree); // Пытаемся установить флаг SugarFree

        // Act
        var response = await Client.PostAsJsonAsync("/api/dishes", createDto, JsonOptions);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Без сахара");
    }

    /// <summary>
    /// Создание блюда с несколькими конфликтными флагами — все ошибки в одном сообщении
    /// </summary>
    [Fact(DisplayName = "API: Создание блюда с несколькими конфликтными флагами возвращает все ошибки")]
    public async Task CreateDish_MultipleConflictingFlags_ReturnsAllErrors()
    {
        // Arrange: создаем продукт без флагов
        var productDto = TestDataBuilder.CreateProduct(
            name: "Обычный продукт",
            flags: ExtraFlag.None);
        var product = await Client.PostAsync<CreateProductDto, ProductDto>("/api/products", productDto);
        
        var createDto = TestDataBuilder.CreateDish(
            name: "Блюдо с конфликтными флагами",
            productId: product!.Id,
            category: DishCategory.Side,
            flags: ExtraFlag.Vegan | ExtraFlag.GlutenFree | ExtraFlag.SugarFree); // Все флаги

        // Act
        var response = await Client.PostAsJsonAsync("/api/dishes", createDto, JsonOptions);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Веган");
        content.Should().Contain("Без глютена");
        content.Should().Contain("Без сахара");
    }

    #endregion

    #region A6. Макросы — все типы

    /// <summary>
    /// Создание блюда с несколькими макросами — применяется первый
    /// </summary>
    [Fact(DisplayName = "API: Создание блюда с несколькими макросами использует первый")]
    public async Task CreateDish_MultipleMacros_UsesFirst()
    {
        // Arrange
        var productDto = TestDataBuilder.CreateProduct();
        var product = await Client.PostAsync<CreateProductDto, ProductDto>("/api/products", productDto);
        
        var createDto = TestDataBuilder.CreateDish(
            name: "!десерт !первое Блюдо", // Два макроса
            productId: product!.Id);

        // Act
        var response = await Client.PostAsJsonAsync("/api/dishes", createDto, JsonOptions);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var result = await response.Content.ReadFromJsonAsync<DishDto>(JsonOptions);
        result!.Category.Should().Be(DishCategory.Dessert); // Первый макрос
        result.Name.Should().Be("Блюдо"); // Оба макроса удалены
    }

    /// <summary>
    /// Создание блюда с неизвестным макросом — макрос игнорируется
    /// </summary>
    [Fact(DisplayName = "API: Создание блюда с неизвестным макросом игнорирует его")]
    public async Task CreateDish_UnknownMacro_IgnoresMacro()
    {
        // Arrange
        var productDto = TestDataBuilder.CreateProduct();
        var product = await Client.PostAsync<CreateProductDto, ProductDto>("/api/products", productDto);
        
        var createDto = new CreateDishDto
        {
            Name = "!неизвестный Блюдо", // Неизвестный макрос
            Category = DishCategory.Side, // Категория указана явно
            Ingredients = new List<CreateIngredientDto>
            {
                new() { ProductId = product!.Id, AmountInGrams = 100 }
            }
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/dishes", createDto, JsonOptions);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var result = await response.Content.ReadFromJsonAsync<DishDto>(JsonOptions);
        result!.Name.Should().Be("!неизвестный Блюдо"); // Макрос не удален
        result.Category.Should().Be(DishCategory.Side); // Явная категория сохранена
    }

    #endregion
}