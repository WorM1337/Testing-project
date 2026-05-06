using System.Net;
using Core.Models.Enums;
using FluentAssertions;
using Testing_project.Dtos.Dish;
using Testing_project.Dtos.Ingredient;

namespace Test.Core.Integration.Dishes;

[Collection("Integration Tests")]
public class CreateDishApiTests : IntegrationTestBase
{
    #region Валидные данные

    [Fact(DisplayName = "API: Создание блюда с валидными данными возвращает 201 Created")]
    public async Task CreateDish_ValidData_Returns201Created()
    {
        // Arrange: создаем тестовый продукт
        var productResult = await CreateProductAsync("Картофель", 77, 2, 0.4, 18);
        productResult.Content.Should().NotBeNull();
        
        var createDto = new CreateDishDto
        {
            Name = "Картофельное пюре",
            Category = DishCategory.Side,
            Ingredients = new List<CreateIngredientDto>
            {
                new() { ProductId = productResult.Content!.Id, AmountInGrams = 200 }
            }
        };

        // Act
        var dishResult = await CreateDishAsync(createDto);

        // Assert
        dishResult.IsSuccess.Should().BeTrue();
        dishResult.StatusCode.Should().Be(HttpStatusCode.Created);
        dishResult.Content!.Name.Should().Be("Картофельное пюре");
        dishResult.Content.Category.Should().Be(DishCategory.Side);
        dishResult.Content.Id.Should().BeGreaterThan(0);
    }

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
        var productResult = await CreateProductAsync("Продукт для блюда");
        productResult.Content.Should().NotBeNull();
        
        var createDto = new CreateDishDto
        {
            Name = nameWithMacro,
            Category = DishCategory.None, // Категория НЕ указана
            Ingredients = new List<CreateIngredientDto>
            {
                new() { ProductId = productResult.Content!.Id, AmountInGrams = 100 }
            }
        };

        // Act
        var dishResult = await CreateDishAsync(createDto);

        // Assert
        dishResult.IsSuccess.Should().BeTrue();
        dishResult.Content!.Name.Should().Be(expectedCleanName); // Макрос удален
        dishResult.Content.Category.Should().Be(expectedCategory); // Категория определена автоматически
    }

    [Fact(DisplayName = "API: Создание блюда с переопределением КБЖУ использует пользовательские значения")]
    public async Task CreateDish_WithNutritionOverride_UsesCustomValues()
    {
        // Arrange
        var productResult = await CreateProductAsync(calories: 100, proteins: 10, fats: 5, carbs: 15);
        productResult.Content.Should().NotBeNull();
        
        var createDto = new CreateDishDto
        {
            Name = "Блюдо с переопределением",
            Category = DishCategory.Side,
            Ingredients = new List<CreateIngredientDto>
            {
                new() { ProductId = productResult.Content!.Id, AmountInGrams = 100 }
            },
            // Переопределяем КБЖУ (вместо автоматического расчета)
            CaloriesPerServing = 500,
            ProteinsPerServing = 50,
            FatsPerServing = 30,
            CarbsPerServing = 60,
            ServingSize = 250
        };

        // Act
        var dishResult = await CreateDishAsync(createDto);

        // Assert
        dishResult.IsSuccess.Should().BeTrue();
        dishResult.Content!.CaloriesPerServing.Should().Be(500); // Пользовательское значение
        dishResult.Content.ProteinsPerServing.Should().Be(50);
        dishResult.Content.FatsPerServing.Should().Be(30);
        dishResult.Content.CarbsPerServing.Should().Be(60);
        dishResult.Content.ServingSize.Should().Be(250);
    }

    [Theory(DisplayName = "API: Создание блюда с несколькими продуктами рассчитывает КБЖУ")]
    [InlineData(3)] // 3 продукта
    [InlineData(5)] // 5 продуктов
    public async Task CreateDish_WithMultipleProducts_CalculatesNutrition(int productCount)
    {
        // Arrange: создаем несколько продуктов
        var productIds = new List<int>();
        for (int i = 0; i < productCount; i++)
        {
            var productResult = await CreateProductAsync($"Продукт {i + 1}", 100, 10, 5, 15);
            productResult.Content.Should().NotBeNull();
            productIds.Add(productResult.Content!.Id);
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
        var dishResult = await CreateDishAsync(createDto);

        // Assert
        dishResult.IsSuccess.Should().BeTrue();
        dishResult.Content!.CaloriesPerServing.Should().BeApproximately(100 * productCount, 0.01);
        dishResult.Content.ServingSize.Should().Be(100 * productCount); // Сумма веса ингредиентов
    }

    #endregion

    #region Невалидные данные (валидация)

    [Theory(DisplayName = "API: Создание блюда с пустым названием возвращает 400 BadRequest")]
    [InlineData("")]
    [InlineData("   ")]
    public async Task CreateDish_EmptyName_Returns400BadRequest(string? name)
    {
        // Arrange
        var productResult = await CreateProductAsync();
        productResult.Content.Should().NotBeNull();
        
        var createDto = new CreateDishDto
        {
            Name = name!,
            Category = DishCategory.Side,
            Ingredients = new List<CreateIngredientDto>
            {
                new() { ProductId = productResult.Content!.Id, AmountInGrams = 100 }
            }
        };

        // Act
        var dishResult = await CreateDishAsync(createDto);

        // Assert
        dishResult.IsSuccess.Should().BeFalse();
        dishResult.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        dishResult.GetValidationErrorMessage().Should().Be("Название блюда обязательно.");
    }

    [Fact(DisplayName = "API: Создание блюда с null названием возвращает 400 BadRequest")]
    public async Task CreateDish_NullName_Returns400BadRequest()
    {
        // Arrange
        var productResult = await CreateProductAsync();
        productResult.Content.Should().NotBeNull();
        
        var createDto = new CreateDishDto
        {
            Name = null!,
            Category = DishCategory.Side,
            Ingredients = new List<CreateIngredientDto>
            {
                new() { ProductId = productResult.Content!.Id, AmountInGrams = 100 }
            }
        };

        // Act
        var dishResult = await CreateDishAsync(createDto);

        // Assert
        dishResult.IsSuccess.Should().BeFalse();
        dishResult.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        dishResult.GetValidationErrorMessage().Should().Contain("Name field is required");
    }

    [Theory(DisplayName = "API: Создание блюда с коротким названием возвращает 400 BadRequest")]
    [InlineData("A")]
    [InlineData("Б")]
    public async Task CreateDish_NameTooShort_Returns400BadRequest(string name)
    {
        // Arrange
        var productResult = await CreateProductAsync();
        productResult.Content.Should().NotBeNull();
        
        var createDto = new CreateDishDto
        {
            Name = name,
            Category = DishCategory.Side,
            Ingredients = new List<CreateIngredientDto>
            {
                new() { ProductId = productResult.Content!.Id, AmountInGrams = 100 }
            }
        };

        // Act
        var dishResult = await CreateDishAsync(createDto);

        // Assert
        dishResult.IsSuccess.Should().BeFalse();
        dishResult.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        dishResult.GetValidationErrorMessage().Should().Be("Минимальная длина названия — 2 символа.");
    }

    [Theory(DisplayName = "API: Создание блюда с макросом, но коротким чистым названием возвращает 400")]
    [InlineData("!десерт A")]
    [InlineData("!первое Б")]
    public async Task CreateDish_MacroButShortCleanName_Returns400BadRequest(string name)
    {
        // Arrange
        var productResult = await CreateProductAsync();
        productResult.Content.Should().NotBeNull();
        
        var createDto = new CreateDishDto
        {
            Name = name,
            Category = DishCategory.None,
            Ingredients = new List<CreateIngredientDto>
            {
                new() { ProductId = productResult.Content!.Id, AmountInGrams = 100 }
            }
        };

        // Act
        var dishResult = await CreateDishAsync(createDto);

        // Assert
        dishResult.IsSuccess.Should().BeFalse();
        dishResult.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        dishResult.GetValidationErrorMessage().Should().Contain("слишком короткое после удаления макросов");
    }

    [Fact(DisplayName = "API: Создание блюда без категории и макроса возвращает 400 BadRequest")]
    public async Task CreateDish_NoCategoryAndNoMacro_Returns400BadRequest()
    {
        // Arrange
        var productResult = await CreateProductAsync();
        productResult.Content.Should().NotBeNull();
        
        var createDto = new CreateDishDto
        {
            Name = "Простое блюдо", // Без макроса
            Category = DishCategory.None, // Категория не указана
            Ingredients = new List<CreateIngredientDto>
            {
                new() { ProductId = productResult.Content!.Id, AmountInGrams = 100 }
            }
        };

        // Act
        var dishResult = await CreateDishAsync(createDto);

        // Assert
        dishResult.IsSuccess.Should().BeFalse();
        dishResult.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        dishResult.GetValidationErrorMessage().Should().Contain("Категория блюда обязательна");
    }

    [Theory(DisplayName = "API: Создание блюда с более чем 5 фотографиями возвращает 400 BadRequest")]
    [InlineData(6)]
    [InlineData(10)]
    public async Task CreateDish_MoreThan5Photos_Returns400BadRequest(int photoCount)
    {
        // Arrange
        var productResult = await CreateProductAsync();
        productResult.Content.Should().NotBeNull();
        
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
                new() { ProductId = productResult.Content!.Id, AmountInGrams = 100 }
            }
        };

        // Act
        var dishResult = await CreateDishAsync(createDto);

        // Assert
        dishResult.IsSuccess.Should().BeFalse();
        dishResult.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        dishResult.GetValidationErrorMessage().Should().Be("Нельзя загрузить более 5 фотографий.");
    }

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
        var dishResult = await CreateDishAsync(createDto);

        // Assert
        dishResult.IsSuccess.Should().BeFalse();
        dishResult.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        dishResult.GetValidationErrorMessage().Should().Be("Должен быть хотя бы один ингредиент.");
    }

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
        var dishResult = await CreateDishAsync(createDto);

        // Assert
        dishResult.IsSuccess.Should().BeFalse();
        dishResult.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        dishResult.GetValidationErrorMessage().Should().Contain("пустые значения");
    }

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
        var dishResult = await CreateDishAsync(createDto);

        // Assert
        dishResult.IsSuccess.Should().BeFalse();
        dishResult.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        dishResult.GetValidationErrorMessage().Should().Be("ID продукта должен быть больше нуля.");
    }

    [Theory(DisplayName = "API: Создание блюда с невалидным количеством возвращает 400 BadRequest")]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public async Task CreateDish_InvalidAmount_Returns400BadRequest(double amount)
    {
        // Arrange
        var productResult = await CreateProductAsync();
        productResult.Content.Should().NotBeNull();
        
        var createDto = new CreateDishDto
        {
            Name = "Блюдо с невалидным количеством",
            Category = DishCategory.Side,
            Ingredients = new List<CreateIngredientDto>
            {
                new() { ProductId = productResult.Content!.Id, AmountInGrams = amount }
            }
        };

        // Act
        var dishResult = await CreateDishAsync(createDto);

        // Assert
        dishResult.IsSuccess.Should().BeFalse();
        dishResult.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        dishResult.GetValidationErrorMessage().Should().Be("Количество продукта должно быть больше нуля.");
    }

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
        var dishResult = await CreateDishAsync(createDto);

        // Assert
        dishResult.IsSuccess.Should().BeFalse();
        dishResult.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        dishResult.Error.Should().Contain("не найден");
    }

    #endregion

    #region КБЖУ

    [Fact(DisplayName = "API: Создание блюда с калорийностью 0 успешно")]
    public async Task CreateDish_CaloriesZero_Succeeds()
    {
        // Arrange
        var productResult = await CreateProductAsync();
        productResult.Content.Should().NotBeNull();
        
        var createDto = new CreateDishDto
        {
            Name = "Блюдо с нулевой калорийностью",
            Category = DishCategory.Side,
            CaloriesPerServing = 0, // Граничное значение
            Ingredients = new List<CreateIngredientDto>
            {
                new() { ProductId = productResult.Content!.Id, AmountInGrams = 100 }
            }
        };

        // Act
        var dishResult = await CreateDishAsync(createDto);

        // Assert
        dishResult.IsSuccess.Should().BeTrue();
        dishResult.Content!.CaloriesPerServing.Should().Be(0);
    }

    [Theory(DisplayName = "API: Создание блюда с отрицательной калорийностью возвращает 400")]
    [InlineData(-0.1)]
    [InlineData(-1)]
    [InlineData(-100)]
    public async Task CreateDish_NegativeCalories_Returns400BadRequest(double calories)
    {
        // Arrange
        var productResult = await CreateProductAsync();
        productResult.Content.Should().NotBeNull();
        
        var createDto = new CreateDishDto
        {
            Name = "Блюдо с отрицательной калорийностью",
            Category = DishCategory.Side,
            CaloriesPerServing = calories,
            Ingredients = new List<CreateIngredientDto>
            {
                new() { ProductId = productResult.Content!.Id, AmountInGrams = 100 }
            }
        };

        // Act
        var dishResult = await CreateDishAsync(createDto);

        // Assert
        dishResult.IsSuccess.Should().BeFalse();
        dishResult.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        dishResult.GetValidationErrorMessage().Should().Be("Калорийность не может быть отрицательной.");
    }

    [Fact(DisplayName = "API: Создание блюда с очень большими КБЖУ успешно")]
    public async Task CreateDish_VeryLargeNutrition_Succeeds()
    {
        // Arrange
        var productResult = await CreateProductAsync();
        productResult.Content.Should().NotBeNull();
        
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
                new() { ProductId = productResult.Content!.Id, AmountInGrams = 100 }
            }
        };

        // Act
        var dishResult = await CreateDishAsync(createDto);

        // Assert
        dishResult.IsSuccess.Should().BeTrue();
        dishResult.Content!.CaloriesPerServing.Should().Be(999999.99);
    }

    [Fact(DisplayName = "API: Создание блюда с размером порции 0.1г успешно")]
    public async Task CreateDish_ServingSize0Point1_Succeeds()
    {
        // Arrange
        var productResult = await CreateProductAsync();
        productResult.Content.Should().NotBeNull();
        
        var createDto = new CreateDishDto
        {
            Name = "Блюдо с минимальной порцией",
            Category = DishCategory.Side,
            ServingSize = 0.1, // Минимальное положительное
            Ingredients = new List<CreateIngredientDto>
            {
                new() { ProductId = productResult.Content!.Id, AmountInGrams = 100 }
            }
        };

        // Act
        var dishResult = await CreateDishAsync(createDto);

        // Assert
        dishResult.IsSuccess.Should().BeTrue();
        dishResult.Content!.ServingSize.Should().Be(0.1);
    }

    [Theory(DisplayName = "API: Создание блюда с невалидным размером порции возвращает 400")]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public async Task CreateDish_InvalidServingSize_Returns400BadRequest(double servingSize)
    {
        // Arrange
        var productResult = await CreateProductAsync();
        productResult.Content.Should().NotBeNull();
        
        var createDto = new CreateDishDto
        {
            Name = "Блюдо с невалидной порцией",
            Category = DishCategory.Side,
            ServingSize = servingSize,
            Ingredients = new List<CreateIngredientDto>
            {
                new() { ProductId = productResult.Content!.Id, AmountInGrams = 100 }
            }
        };

        // Act
        var dishResult = await CreateDishAsync(createDto);

        // Assert
        dishResult.IsSuccess.Should().BeFalse();
        dishResult.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        dishResult.GetValidationErrorMessage().Should().Be("Размер порции должен быть больше 0.");
    }

    #endregion

    #region Количество ингредиентов

    [Fact(DisplayName = "API: Создание блюда с 1 ингредиентом успешно")]
    public async Task CreateDish_OneIngredient_Succeeds()
    {
        // Arrange
        var productResult = await CreateProductAsync();
        productResult.Content.Should().NotBeNull();
        
        var createDto = new CreateDishDto
        {
            Name = "Блюдо с одним ингредиентом",
            Category = DishCategory.Entree,
            Ingredients = new List<CreateIngredientDto>
            {
                new() { ProductId = productResult.Content!.Id, AmountInGrams = 100 }
            }
        };

        // Act
        var dishResult = await CreateDishAsync(createDto);

        // Assert
        dishResult.IsSuccess.Should().BeTrue();
        dishResult.Content!.Ingredients.Should().HaveCount(1);
    }

    [Theory(DisplayName = "API: Создание блюда с большим количеством ингредиентов успешно")]
    [InlineData(10)]
    [InlineData(20)]
    public async Task CreateDish_ManyIngredients_Succeeds(int ingredientCount)
    {
        // Arrange: создаем много продуктов
        var productIds = new List<int>();
        for (int i = 0; i < ingredientCount; i++)
        {
            var productResult = await CreateProductAsync($"Продукт {i + 1}");
            productResult.Content.Should().NotBeNull();
            productIds.Add(productResult.Content!.Id);
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
        var dishResult = await CreateDishAsync(createDto);

        // Assert
        dishResult.IsSuccess.Should().BeTrue();
        dishResult.Content!.Ingredients.Should().HaveCount(ingredientCount);
    }

    #endregion

    #region Конфликтные сценарии

    [Fact(DisplayName = "API: Создание блюда с флагом Vegan, но не веганским продуктом возвращает 400")]
    public async Task CreateDish_VeganFlagButNonVeganProduct_Returns400BadRequest()
    {
        // Arrange: создаем НЕ веганский продукт
        var productResult = await CreateProductAsync("Куриное филе", flags: ExtraFlag.None);
        productResult.Content.Should().NotBeNull();
        
        var createDto = new CreateDishDto
        {
            Name = "Псевдо-веганское блюдо",
            Category = DishCategory.Side,
            Flags = ExtraFlag.Vegan,
            Ingredients = new List<CreateIngredientDto>
            {
                new() { ProductId = productResult.Content!.Id, AmountInGrams = 100 }
            }
        };

        // Act
        var dishResult = await CreateDishAsync(createDto);

        // Assert
        dishResult.IsSuccess.Should().BeFalse();
        dishResult.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        dishResult.Error.Should().Contain("Веган");
    }

    [Fact(DisplayName = "API: Создание блюда с флагом GlutenFree, но продуктом с глютеном возвращает 400")]
    public async Task CreateDish_GlutenFreeFlagButGlutenProduct_Returns400BadRequest()
    {
        // Arrange: создаем продукт с глютеном
        var productResult = await CreateProductAsync("Пшеничная мука", flags: ExtraFlag.None);
        productResult.Content.Should().NotBeNull();
        
        var createDto = new CreateDishDto
        {
            Name = "Псевдо-безглютеновое блюдо",
            Category = DishCategory.Side,
            Flags = ExtraFlag.GlutenFree,
            Ingredients = new List<CreateIngredientDto>
            {
                new() { ProductId = productResult.Content!.Id, AmountInGrams = 100 }
            }
        };

        // Act
        var dishResult = await CreateDishAsync(createDto);

        // Assert
        dishResult.IsSuccess.Should().BeFalse();
        dishResult.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        dishResult.Error.Should().Contain("Без глютена");
    }

    [Fact(DisplayName = "API: Создание блюда с флагом SugarFree, но продуктом с сахаром возвращает 400")]
    public async Task CreateDish_SugarFreeFlagButSugarProduct_Returns400BadRequest()
    {
        // Arrange: создаем продукт с сахаром
        var productResult = await CreateProductAsync("Сахар", flags: ExtraFlag.None);
        productResult.Content.Should().NotBeNull();
        
        var createDto = new CreateDishDto
        {
            Name = "Псевдо-без сахара блюдо",
            Category = DishCategory.Side,
            Flags = ExtraFlag.SugarFree,
            Ingredients = new List<CreateIngredientDto>
            {
                new() { ProductId = productResult.Content!.Id, AmountInGrams = 100 }
            }
        };

        // Act
        var dishResult = await CreateDishAsync(createDto);

        // Assert
        dishResult.IsSuccess.Should().BeFalse();
        dishResult.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        dishResult.Error.Should().Contain("Без сахара");
    }

    [Fact(DisplayName = "API: Создание блюда с несколькими конфликтными флагами возвращает все ошибки")]
    public async Task CreateDish_MultipleConflictingFlags_ReturnsAllErrors()
    {
        // Arrange: создаем продукт без флагов
        var productResult = await CreateProductAsync("Обычный продукт", flags: ExtraFlag.None);
        productResult.Content.Should().NotBeNull();
        
        var createDto = new CreateDishDto
        {
            Name = "Блюдо с конфликтными флагами",
            Category = DishCategory.Side,
            Flags = ExtraFlag.Vegan | ExtraFlag.GlutenFree | ExtraFlag.SugarFree, // Все флаги
            Ingredients = new List<CreateIngredientDto>
            {
                new() { ProductId = productResult.Content!.Id, AmountInGrams = 100 }
            }
        };

        // Act
        var dishResult = await CreateDishAsync(createDto);

        // Assert
        dishResult.IsSuccess.Should().BeFalse();
        dishResult.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        dishResult.Error.Should().Contain("Веган");
        dishResult.Error.Should().Contain("Без глютена");
        dishResult.Error.Should().Contain("Без сахара");
    }

    #endregion

    #region Макросы — все типы

    [Fact(DisplayName = "API: Создание блюда с несколькими макросами использует первый")]
    public async Task CreateDish_MultipleMacros_UsesFirst()
    {
        // Arrange
        var productResult = await CreateProductAsync();
        productResult.Content.Should().NotBeNull();
        
        var createDto = new CreateDishDto
        {
            Name = "!десерт !первое Блюдо", // Два макроса
            Category = DishCategory.None,
            Ingredients = new List<CreateIngredientDto>
            {
                new() { ProductId = productResult.Content!.Id, AmountInGrams = 100 }
            }
        };

        // Act
        var dishResult = await CreateDishAsync(createDto);

        // Assert
        dishResult.IsSuccess.Should().BeTrue();
        dishResult.Content!.Category.Should().Be(DishCategory.Dessert); // Первый макрос
        dishResult.Content.Name.Should().Be("Блюдо"); // Оба макроса удалены
    }

    [Fact(DisplayName = "API: Создание блюда с неизвестным макросом игнорирует его")]
    public async Task CreateDish_UnknownMacro_IgnoresMacro()
    {
        // Arrange
        var productResult = await CreateProductAsync();
        productResult.Content.Should().NotBeNull();
        
        var createDto = new CreateDishDto
        {
            Name = "!неизвестный Блюдо", // Неизвестный макрос
            Category = DishCategory.Side, // Категория указана явно
            Ingredients = new List<CreateIngredientDto>
            {
                new() { ProductId = productResult.Content!.Id, AmountInGrams = 100 }
            }
        };

        // Act
        var dishResult = await CreateDishAsync(createDto);

        // Assert
        dishResult.IsSuccess.Should().BeTrue();
        dishResult.Content!.Name.Should().Be("!неизвестный Блюдо"); // Макрос не удален
        dishResult.Content.Category.Should().Be(DishCategory.Side); // Явная категория сохранена
    }

    #endregion
}
