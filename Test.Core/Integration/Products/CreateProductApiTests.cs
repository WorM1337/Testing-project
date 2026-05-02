using System.Net;
using Core.Models.Enums;
using FluentAssertions;
using Test.Core.Integration.Helpers;
using Testing_project.Dtos;
using static Test.Core.Integration.Helpers.ApiHelpers;

namespace Test.Core.Integration.Products;

[Collection("Integration Tests")]
public class CreateProductApiTests : IntegrationTestBase
{
    #region E1. Создание продуктов — валидные данные

    /// <summary>
    /// Создание валидного продукта со всеми полями — 201 Created
    /// </summary>
    [Fact(DisplayName = "API: Создание валидного продукта возвращает 201 Created")]
    public async Task CreateProduct_ValidData_Returns201Created()
    {
        // Arrange
        var createDto = TestDataBuilder.CreateProduct(
            name: "Тестовый продукт",
            calories: 100,
            proteins: 10,
            fats: 5,
            carbs: 15,
            category: ProductCategory.Vegetables);

        // Act
        var response = await Client.PostAsJsonAsync("/api/products", createDto, JsonOptions);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var result = await response.Content.ReadFromJsonAsync<ProductDto>(JsonOptions);
        result.Should().NotBeNull();
        result!.Name.Should().Be(createDto.Name);
        result.CaloriesPer100g.Should().Be(100);
        result.ProteinsPer100g.Should().Be(10);
        result.FatsPer100g.Should().Be(5);
        result.CarbsPer100g.Should().Be(15);
        result.Category.Should().Be(ProductCategory.Vegetables);
        result.Id.Should().BeGreaterThan(0);
        
        // Сохраняем для очистки
        CreatedProductIds.Add(result.Id);
    }

    /// <summary>
    /// Создание продукта с флагами — комбинации флагов
    /// </summary>
    [Theory(DisplayName = "API: Создание продукта с флагами успешно")]
    [InlineData(ExtraFlag.Vegan)]
    [InlineData(ExtraFlag.GlutenFree)]
    [InlineData(ExtraFlag.SugarFree)]
    public async Task CreateProduct_WithSingleFlag_Succeeds(ExtraFlag flag)
    {
        // Arrange
        var createDto = TestDataBuilder.CreateProduct(
            name: $"Продукт с флагом {flag}",
            flags: flag);

        // Act
        var response = await Client.PostAsJsonAsync("/api/products", createDto, JsonOptions);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var result = await response.Content.ReadFromJsonAsync<ProductDto>(JsonOptions);
        result!.Flags.Should().HaveFlag(flag);
        CreatedProductIds.Add(result.Id);
    }

    /// <summary>
    /// Создание продукта с несколькими флагами
    /// </summary>
    [Fact(DisplayName = "API: Создание продукта с несколькими флагами успешно")]
    public async Task CreateProduct_WithMultipleFlags_Succeeds()
    {
        // Arrange
        var flags = ExtraFlag.Vegan | ExtraFlag.GlutenFree | ExtraFlag.SugarFree;
        var createDto = TestDataBuilder.CreateProduct(
            name: "Продукт со всеми флагами",
            flags: flags);

        // Act
        var response = await Client.PostAsJsonAsync("/api/products", createDto, JsonOptions);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var result = await response.Content.ReadFromJsonAsync<ProductDto>(JsonOptions);
        result!.Flags.Should().HaveFlag(ExtraFlag.Vegan);
        result.Flags.Should().HaveFlag(ExtraFlag.GlutenFree);
        result.Flags.Should().HaveFlag(ExtraFlag.SugarFree);
        CreatedProductIds.Add(result.Id);
    }

    /// <summary>
    /// Создание продукта с минимальными обязательными полями
    /// </summary>
    [Fact(DisplayName = "API: Создание продукта с минимальными полями успешно")]
    public async Task CreateProduct_MinimalFields_Succeeds()
    {
        // Arrange
        var createDto = new CreateProductDto
        {
            Name = "Минимальный продукт",
            CaloriesPer100g = 0,
            ProteinsPer100g = 0,
            FatsPer100g = 0,
            CarbsPer100g = 0,
            Category = ProductCategory.Vegetables,
            CookingRequirement = CookingRequirement.ReadyToUse
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/products", createDto, JsonOptions);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var result = await response.Content.ReadFromJsonAsync<ProductDto>(JsonOptions);
        CreatedProductIds.Add(result.Id);
    }

    /// <summary>
    /// Создание продукта с фотографиями (до 5)
    /// </summary>
    [Theory(DisplayName = "API: Создание продукта с фотографиями успешно")]
    [InlineData(1)]
    [InlineData(3)]
    [InlineData(5)]
    public async Task CreateProduct_WithPhotos_Succeeds(int photoCount)
    {
        // Arrange
        var photos = Enumerable.Range(1, photoCount)
            .Select(i => $"https://example.com/photo{i}.jpg")
            .ToList();
        
        var createDto = new CreateProductDto
        {
            Name = $"Продукт с {photoCount} фото",
            Photos = photos,
            CaloriesPer100g = 100,
            ProteinsPer100g = 10,
            FatsPer100g = 5,
            CarbsPer100g = 15,
            Category = ProductCategory.Vegetables,
            CookingRequirement = CookingRequirement.ReadyToUse
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/products", createDto, JsonOptions);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var result = await response.Content.ReadFromJsonAsync<ProductDto>(JsonOptions);
        result!.Photos.Should().HaveCount(photoCount);
        CreatedProductIds.Add(result.Id);
    }

    /// <summary>
    /// Создание продукта с составом
    /// </summary>
    [Fact(DisplayName = "API: Создание продукта с составом успешно")]
    public async Task CreateProduct_WithComposition_Succeeds()
    {
        // Arrange
        var createDto = new CreateProductDto
        {
            Name = "Продукт с составом",
            Composition = "Вода, соль, специи",
            CaloriesPer100g = 100,
            ProteinsPer100g = 10,
            FatsPer100g = 5,
            CarbsPer100g = 15,
            Category = ProductCategory.CannedFood,
            CookingRequirement = CookingRequirement.ReadyToUse
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/products", createDto, JsonOptions);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var result = await response.Content.ReadFromJsonAsync<ProductDto>(JsonOptions);
        result!.Composition.Should().Be("Вода, соль, специи");
        CreatedProductIds.Add(result.Id);
    }

    /// <summary>
    /// Создание продуктов всех категорий
    /// </summary>
    [Theory(DisplayName = "API: Создание продукта каждой категории успешно")]
    [InlineData(ProductCategory.Frozen)]
    [InlineData(ProductCategory.Meat)]
    [InlineData(ProductCategory.Vegetables)]
    [InlineData(ProductCategory.Herbs)]
    [InlineData(ProductCategory.Spices)]
    [InlineData(ProductCategory.Cereals)]
    [InlineData(ProductCategory.CannedFood)]
    [InlineData(ProductCategory.Liquid)]
    [InlineData(ProductCategory.Sweets)]
    public async Task CreateProduct_AllCategories_Succeeds(ProductCategory category)
    {
        // Arrange
        var createDto = TestDataBuilder.CreateProduct(
            name: $"Продукт категории {category}",
            category: category);

        // Act
        var response = await Client.PostAsJsonAsync("/api/products", createDto, JsonOptions);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var result = await response.Content.ReadFromJsonAsync<ProductDto>(JsonOptions);
        result!.Category.Should().Be(category);
        CreatedProductIds.Add(result.Id);
    }

    /// <summary>
    /// Создание продуктов с разными требованиями к готовке
    /// </summary>
    [Theory(DisplayName = "API: Создание продукта с разными требованиями к готовке успешно")]
    [InlineData(CookingRequirement.ReadyToUse)]
    [InlineData(CookingRequirement.SemiFinished)]
    [InlineData(CookingRequirement.RequiresCooking)]
    public async Task CreateProduct_AllCookingRequirements_Succeeds(CookingRequirement cookingRequirement)
    {
        // Arrange
        var createDto = new CreateProductDto
        {
            Name = $"Продукт {cookingRequirement}",
            CaloriesPer100g = 100,
            ProteinsPer100g = 10,
            FatsPer100g = 5,
            CarbsPer100g = 15,
            Category = ProductCategory.Vegetables,
            CookingRequirement = cookingRequirement
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/products", createDto, JsonOptions);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var result = await response.Content.ReadFromJsonAsync<ProductDto>(JsonOptions);
        result!.CookingRequirement.Should().Be(cookingRequirement);
        CreatedProductIds.Add(result.Id);
    }

    #endregion

    #region E2. Создание продуктов — невалидные данные

    /// <summary>
    /// Создание продукта с пустым названием — ошибка валидации
    /// </summary>
    [Theory(DisplayName = "API: Создание продукта с пустым названием возвращает 400 BadRequest")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task CreateProduct_EmptyName_Returns400BadRequest(string? name)
    {
        // Arrange
        var createDto = new CreateProductDto
        {
            Name = name!,
            CaloriesPer100g = 100,
            ProteinsPer100g = 10,
            FatsPer100g = 5,
            CarbsPer100g = 15,
            Category = ProductCategory.Vegetables,
            CookingRequirement = CookingRequirement.ReadyToUse
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/products", createDto, JsonOptions);

        // Assert
        await response.ShouldHaveValidationError("Название продукта обязательно");
    }

    /// <summary>
    /// Создание продукта с названием короче 2 символов — ошибка валидации
    /// </summary>
    [Theory(DisplayName = "API: Создание продукта с коротким названием возвращает 400 BadRequest")]
    [InlineData("A")]
    [InlineData("Б")]
    public async Task CreateProduct_NameTooShort_Returns400BadRequest(string name)
    {
        // Arrange
        var createDto = TestDataBuilder.CreateProduct(name: name);

        // Act
        var response = await Client.PostAsJsonAsync("/api/products", createDto, JsonOptions);

        // Assert
        await response.ShouldHaveValidationError("Минимальная длина названия — 2 символа");
    }

    /// <summary>
    /// Создание продукта с более чем 5 фотографиями — ошибка валидации
    /// </summary>
    [Theory(DisplayName = "API: Создание продукта с более чем 5 фотографиями возвращает 400 BadRequest")]
    [InlineData(6)]
    [InlineData(10)]
    public async Task CreateProduct_MoreThan5Photos_Returns400BadRequest(int photoCount)
    {
        // Arrange
        var photos = Enumerable.Range(1, photoCount)
            .Select(i => $"https://example.com/photo{i}.jpg")
            .ToList();
        
        var createDto = new CreateProductDto
        {
            Name = "Продукт с фото",
            Photos = photos,
            CaloriesPer100g = 100,
            ProteinsPer100g = 10,
            FatsPer100g = 5,
            CarbsPer100g = 15,
            Category = ProductCategory.Vegetables,
            CookingRequirement = CookingRequirement.ReadyToUse
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/products", createDto, JsonOptions);

        // Assert
        await response.ShouldHaveValidationError("Нельзя загрузить более 5 фотографий");
    }

    /// <summary>
    /// Создание продукта с отрицательной калорийностью — ошибка валидации
    /// </summary>
    [Theory(DisplayName = "API: Создание продукта с отрицательной калорийностью возвращает 400")]
    [InlineData(-0.1)]
    [InlineData(-1)]
    [InlineData(-100)]
    public async Task CreateProduct_NegativeCalories_Returns400BadRequest(double calories)
    {
        // Arrange
        var createDto = TestDataBuilder.CreateProduct(calories: calories);

        // Act
        var response = await Client.PostAsJsonAsync("/api/products", createDto, JsonOptions);

        // Assert
        await response.ShouldHaveValidationError("Калорийность не может быть отрицательной");
    }

    /// <summary>
    /// Создание продукта с отрицательными белками — ошибка валидации
    /// </summary>
    [Theory(DisplayName = "API: Создание продукта с отрицательными белками возвращает 400")]
    [InlineData(-0.1)]
    [InlineData(-1)]
    public async Task CreateProduct_NegativeProteins_Returns400BadRequest(double proteins)
    {
        // Arrange
        var createDto = TestDataBuilder.CreateProduct(proteins: proteins);

        // Act
        var response = await Client.PostAsJsonAsync("/api/products", createDto, JsonOptions);

        // Assert
        await response.ShouldHaveValidationError("Количество белков не может быть отрицательным");
    }

    /// <summary>
    /// Создание продукта с отрицательными жирами — ошибка валидации
    /// </summary>
    [Theory(DisplayName = "API: Создание продукта с отрицательными жирами возвращает 400")]
    [InlineData(-0.1)]
    [InlineData(-1)]
    public async Task CreateProduct_NegativeFats_Returns400BadRequest(double fats)
    {
        // Arrange
        var createDto = TestDataBuilder.CreateProduct(fats: fats);

        // Act
        var response = await Client.PostAsJsonAsync("/api/products", createDto, JsonOptions);

        // Assert
        await response.ShouldHaveValidationError("Количество жиров не может быть отрицательным");
    }

    /// <summary>
    /// Создание продукта с отрицательными углеводами — ошибка валидации
    /// </summary>
    [Theory(DisplayName = "API: Создание продукта с отрицательными углеводами возвращает 400")]
    [InlineData(-0.1)]
    [InlineData(-1)]
    public async Task CreateProduct_NegativeCarbs_Returns400BadRequest(double carbs)
    {
        // Arrange
        var createDto = TestDataBuilder.CreateProduct(carbs: carbs);

        // Act
        var response = await Client.PostAsJsonAsync("/api/products", createDto, JsonOptions);

        // Assert
        await response.ShouldHaveValidationError("Количество углеводов не может быть отрицательным");
    }

    /// <summary>
    /// Создание продукта с категорией None — ошибка валидации
    /// </summary>
    [Fact(DisplayName = "API: Создание продукта с категорией None возвращает 400 BadRequest")]
    public async Task CreateProduct_CategoryNone_Returns400BadRequest()
    {
        // Arrange
        var createDto = new CreateProductDto
        {
            Name = "Продукт без категории",
            CaloriesPer100g = 100,
            ProteinsPer100g = 10,
            FatsPer100g = 5,
            CarbsPer100g = 15,
            Category = ProductCategory.None, // Невалидная категория
            CookingRequirement = CookingRequirement.ReadyToUse
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/products", createDto, JsonOptions);

        // Assert
        await response.ShouldHaveValidationError("Категория продукта обязательна");
    }

    /// <summary>
    /// Создание продукта с суммой макронутриентов > 100г — ошибка валидации
    /// </summary>
    [Fact(DisplayName = "API: Создание продукта с суммой БЖУ > 100г возвращает 400 BadRequest")]
    public async Task CreateProduct_MacronutrientsExceed100_Returns400BadRequest()
    {
        // Arrange
        var createDto = new CreateProductDto
        {
            Name = "Продукт с избыточными БЖУ",
            CaloriesPer100g = 100,
            ProteinsPer100g = 50, // 50 + 40 + 20 = 110 > 100
            FatsPer100g = 40,
            CarbsPer100g = 20,
            Category = ProductCategory.Vegetables,
            CookingRequirement = CookingRequirement.ReadyToUse
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/products", createDto, JsonOptions);

        // Assert
        await response.ShouldHaveValidationError("Сумма белков, жиров и углеводов не может превышать 100 г");
    }

    /// <summary>
    /// Создание продукта с суммой макронутриентов = 100г — граничное значение (валидно)
    /// </summary>
    [Fact(DisplayName = "API: Создание продукта с суммой БЖУ = 100г успешно")]
    public async Task CreateProduct_MacronutrientsExactly100_Succeeds()
    {
        // Arrange
        var createDto = new CreateProductDto
        {
            Name = "Продукт с БЖУ = 100г",
            CaloriesPer100g = 400,
            ProteinsPer100g = 30, // 30 + 40 + 30 = 100
            FatsPer100g = 40,
            CarbsPer100g = 30,
            Category = ProductCategory.Vegetables,
            CookingRequirement = CookingRequirement.ReadyToUse
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/products", createDto, JsonOptions);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var result = await response.Content.ReadFromJsonAsync<ProductDto>(JsonOptions);
        CreatedProductIds.Add(result!.Id);
    }

    #endregion

    #region E3. Граничные значения КБЖУ

    /// <summary>
    /// Создание продукта с калорийностью = 0 — граничное значение (валидно)
    /// </summary>
    [Fact(DisplayName = "API: Создание продукта с калорийностью 0 успешно")]
    public async Task CreateProduct_CaloriesZero_Succeeds()
    {
        // Arrange
        var createDto = TestDataBuilder.CreateProduct(
            name: "Продукт с нулевой калорийностью",
            calories: 0);

        // Act
        var response = await Client.PostAsJsonAsync("/api/products", createDto, JsonOptions);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var result = await response.Content.ReadFromJsonAsync<ProductDto>(JsonOptions);
        CreatedProductIds.Add(result!.Id);
    }

    /// <summary>
    /// Создание продукта с очень большими значениями КБЖУ — проверка на overflow
    /// </summary>
    [Fact(DisplayName = "API: Создание продукта с очень большими КБЖУ успешно")]
    public async Task CreateProduct_VeryLargeNutrition_Succeeds()
    {
        // Arrange
        var createDto = new CreateProductDto
        {
            Name = "Продукт с большими КБЖУ",
            CaloriesPer100g = 999999.99,
            ProteinsPer100g = 0,
            FatsPer100g = 0,
            CarbsPer100g = 0,
            Category = ProductCategory.Vegetables,
            CookingRequirement = CookingRequirement.ReadyToUse
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/products", createDto, JsonOptions);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var result = await response.Content.ReadFromJsonAsync<ProductDto>(JsonOptions);
        CreatedProductIds.Add(result!.Id);
    }

    /// <summary>
    /// Создание продукта с минимальными положительными значениями БЖУ
    /// </summary>
    [Fact(DisplayName = "API: Создание продукта с минимальными БЖУ успешно")]
    public async Task CreateProduct_MinimalPositiveNutrition_Succeeds()
    {
        // Arrange
        var createDto = new CreateProductDto
        {
            Name = "Продукт с минимальными БЖУ",
            CaloriesPer100g = 0.1,
            ProteinsPer100g = 0.1,
            FatsPer100g = 0.1,
            CarbsPer100g = 0.1,
            Category = ProductCategory.Vegetables,
            CookingRequirement = CookingRequirement.ReadyToUse
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/products", createDto, JsonOptions);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var result = await response.Content.ReadFromJsonAsync<ProductDto>(JsonOptions);
        CreatedProductIds.Add(result!.Id);
    }

    #endregion
}