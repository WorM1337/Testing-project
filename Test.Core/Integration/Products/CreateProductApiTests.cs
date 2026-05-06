using System.Net;
using Core.Models.Enums;
using FluentAssertions;
using Test.Core.Integration.Helpers;
using Testing_project.Dtos;

namespace Test.Core.Integration.Products;

[Collection("Integration Tests")]
public class CreateProductApiTests : IntegrationTestBase
{
    #region E1. Создание продуктов — валидные данные

    [Fact(DisplayName = "API: Создание валидного продукта возвращает 201 Created")]
    public async Task CreateProduct_ValidData_Returns201Created()
    {
        // Act
        var result = await CreateProductAsync(
            name: "Тестовый продукт",
            calories: 100,
            proteins: 10,
            fats: 5,
            carbs: 15,
            category: ProductCategory.Vegetables);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(HttpStatusCode.Created);
        result.Content.Should().NotBeNull();
        result.Content!.Name.Should().Be("Тестовый продукт");
        result.Content.CaloriesPer100g.Should().Be(100);
        result.Content.ProteinsPer100g.Should().Be(10);
        result.Content.FatsPer100g.Should().Be(5);
        result.Content.CarbsPer100g.Should().Be(15);
        result.Content.Category.Should().Be(ProductCategory.Vegetables);
        result.Content.Id.Should().BeGreaterThan(0);
    }

    [Theory(DisplayName = "API: Создание продукта с флагами успешно")]
    [InlineData(ExtraFlag.Vegan)]
    [InlineData(ExtraFlag.GlutenFree)]
    [InlineData(ExtraFlag.SugarFree)]
    public async Task CreateProduct_WithSingleFlag_Succeeds(ExtraFlag flag)
    {
        // Act
        var result = await CreateProductAsync(
            name: $"Продукт с флагом {flag}",
            flags: flag);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(HttpStatusCode.Created);
        result.Content!.Flags.Should().HaveFlag(flag);
    }

    [Fact(DisplayName = "API: Создание продукта с несколькими флагами успешно")]
    public async Task CreateProduct_WithMultipleFlags_Succeeds()
    {
        // Arrange
        var flags = ExtraFlag.Vegan | ExtraFlag.GlutenFree | ExtraFlag.SugarFree;

        // Act
        var result = await CreateProductAsync(
            name: "Продукт со всеми флагами",
            flags: flags);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(HttpStatusCode.Created);
        result.Content!.Flags.Should().HaveFlag(ExtraFlag.Vegan);
        result.Content.Flags.Should().HaveFlag(ExtraFlag.GlutenFree);
        result.Content.Flags.Should().HaveFlag(ExtraFlag.SugarFree);
    }

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
        var result = await CreateProductAsync(createDto);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(HttpStatusCode.Created);
    }

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
        var result = await CreateProductAsync(createDto);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(HttpStatusCode.Created);
        result.Content!.Photos.Should().HaveCount(photoCount);
    }

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
        var result = await CreateProductAsync(createDto);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(HttpStatusCode.Created);
        result.Content!.Composition.Should().Be("Вода, соль, специи");
    }

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
        // Act
        var result = await CreateProductAsync(
            name: $"Продукт категории {category}",
            category: category);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(HttpStatusCode.Created);
        result.Content!.Category.Should().Be(category);
    }

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
        var result = await CreateProductAsync(createDto);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(HttpStatusCode.Created);
        result.Content!.CookingRequirement.Should().Be(cookingRequirement);
    }

    #endregion

    #region E2. Создание продуктов — невалидные данные

    [Theory(DisplayName = "API: Создание продукта с пустым названием возвращает 400 BadRequest")]
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
        var result = await CreateProductAsync(createDto);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        result.GetValidationErrorMessage().Should().Contain("Название продукта обязательно");
    }

    [Fact(DisplayName = "API: Создание продукта с null названием возвращает 400 BadRequest")]
    public async Task CreateProduct_NullName_Returns400BadRequest()
    {
        // Arrange
        var createDto = new CreateProductDto
        {
            Name = null!,
            CaloriesPer100g = 100,
            ProteinsPer100g = 10,
            FatsPer100g = 5,
            CarbsPer100g = 15,
            Category = ProductCategory.Vegetables,
            CookingRequirement = CookingRequirement.ReadyToUse
        };

        // Act
        var result = await CreateProductAsync(createDto);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        result.GetValidationErrorMessage().Should().Contain("Name field is required");
    }

    [Theory(DisplayName = "API: Создание продукта с коротким названием возвращает 400 BadRequest")]
    [InlineData("A")]
    [InlineData("Б")]
    public async Task CreateProduct_NameTooShort_Returns400BadRequest(string name)
    {
        // Act
        var result = await CreateProductAsync(name: name);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        result.GetValidationErrorMessage().Should().Contain("Минимальная длина названия");
    }

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
        var result = await CreateProductAsync(createDto);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        result.GetValidationErrorMessage().Should().Contain("более 5 фотографий");
    }

    [Theory(DisplayName = "API: Создание продукта с отрицательной калорийностью возвращает 400")]
    [InlineData(-0.1)]
    [InlineData(-1)]
    [InlineData(-100)]
    public async Task CreateProduct_NegativeCalories_Returns400BadRequest(double calories)
    {
        // Act
        var result = await CreateProductAsync(calories: calories);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        result.GetValidationErrorMessage().Should().Contain("Калорийность не может быть отрицательной");
    }

    [Theory(DisplayName = "API: Создание продукта с отрицательными белками возвращает 400")]
    [InlineData(-0.1)]
    [InlineData(-1)]
    public async Task CreateProduct_NegativeProteins_Returns400BadRequest(double proteins)
    {
        // Act
        var result = await CreateProductAsync(proteins: proteins);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        result.GetValidationErrorMessage().Should().Contain("белков не может быть отрицательным");
    }

    [Theory(DisplayName = "API: Создание продукта с отрицательными жирами возвращает 400")]
    [InlineData(-0.1)]
    [InlineData(-1)]
    public async Task CreateProduct_NegativeFats_Returns400BadRequest(double fats)
    {
        // Act
        var result = await CreateProductAsync(fats: fats);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        result.GetValidationErrorMessage().Should().Contain("жиров не может быть отрицательным");
    }

    [Theory(DisplayName = "API: Создание продукта с отрицательными углеводами возвращает 400")]
    [InlineData(-0.1)]
    [InlineData(-1)]
    public async Task CreateProduct_NegativeCarbs_Returns400BadRequest(double carbs)
    {
        // Act
        var result = await CreateProductAsync(carbs: carbs);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        result.GetValidationErrorMessage().Should().Contain("углеводов не может быть отрицательным");
    }

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
            Category = ProductCategory.None,
            CookingRequirement = CookingRequirement.ReadyToUse
        };

        // Act
        var result = await CreateProductAsync(createDto);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        result.GetValidationErrorMessage().Should().Contain("Категория продукта обязательна");
    }

    [Fact(DisplayName = "API: Создание продукта с суммой БЖУ > 100г возвращает 400 BadRequest")]
    public async Task CreateProduct_MacronutrientsExceed100_Returns400BadRequest()
    {
        // Arrange
        var createDto = new CreateProductDto
        {
            Name = "Продукт с избыточными БЖУ",
            CaloriesPer100g = 100,
            ProteinsPer100g = 50,
            FatsPer100g = 40,
            CarbsPer100g = 20,
            Category = ProductCategory.Vegetables,
            CookingRequirement = CookingRequirement.ReadyToUse
        };

        // Act
        var result = await CreateProductAsync(createDto);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        result.GetValidationErrorMessage().Should().Contain("Сумма белков, жиров и углеводов не может превышать 100 г");
    }

    [Fact(DisplayName = "API: Создание продукта с суммой БЖУ = 100г успешно")]
    public async Task CreateProduct_MacronutrientsExactly100_Succeeds()
    {
        // Arrange
        var createDto = new CreateProductDto
        {
            Name = "Продукт с БЖУ = 100г",
            CaloriesPer100g = 400,
            ProteinsPer100g = 30,
            FatsPer100g = 40,
            CarbsPer100g = 30,
            Category = ProductCategory.Vegetables,
            CookingRequirement = CookingRequirement.ReadyToUse
        };

        // Act
        var result = await CreateProductAsync(createDto);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    #endregion

    #region E3. Граничные значения КБЖУ

    [Fact(DisplayName = "API: Создание продукта с калорийностью 0 успешно")]
    public async Task CreateProduct_CaloriesZero_Succeeds()
    {
        // Act
        var result = await CreateProductAsync(
            name: "Продукт с нулевой калорийностью",
            calories: 0);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(HttpStatusCode.Created);
    }

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
        var result = await CreateProductAsync(createDto);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(HttpStatusCode.Created);
    }

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
        var result = await CreateProductAsync(createDto);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    #endregion
}
