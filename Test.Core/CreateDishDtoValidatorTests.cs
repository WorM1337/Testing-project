using Core.Models.Enums;
using FluentAssertions;
using FluentValidation;
using FluentValidation.TestHelper;
using Testing_project.Dtos.Dish;
using Testing_project.Dtos.Ingredient;
using Testing_project.Validators;
using Xunit;

namespace Test.Core;

public class CreateDishDtoValidatorTests
{
    private readonly CreateDishDtoValidator _validator;

    public CreateDishDtoValidatorTests()
    {
        _validator = new CreateDishDtoValidator();
    }

    // ===== ГРУППА 1: Валидация названия =====
    
    [Theory(DisplayName = "Валидация: когда название пустое - должна быть ошибка")]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Validate_WhenNameIsEmpty_ShouldHaveError(string? name)
    {
        // Arrange
        var dto = new CreateDishDto
        {
            Name = name!,
            Ingredients = new List<CreateIngredientDto> { new() { ProductId = 1, AmountInGrams = 100 } }
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("Название блюда обязательно.");
    }

    [Fact(DisplayName = "Валидация: когда название слишком короткое - должна быть ошибка")]
    public void Validate_WhenNameIsTooShort_ShouldHaveError()
    {
        // Arrange
        var dto = new CreateDishDto
        {
            Name = "A",
            Ingredients = new List<CreateIngredientDto> { new() { ProductId = 1, AmountInGrams = 100 } }
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("Минимальная длина названия — 2 символа.");
    }

    [Fact(DisplayName = "Валидация: когда в названии есть макрос, но чистое название короткое - должна быть ошибка")]
    public void Validate_WhenNameHasMacroButCleanNameTooShort_ShouldHaveError()
    {
        // Arrange
        var dto = new CreateDishDto
        {
            Name = "!десерт A", // После удаления макроса останется "A"
            Ingredients = new List<CreateIngredientDto> { new() { ProductId = 1, AmountInGrams = 100 } }
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("Название блюда слишком короткое после удаления макросов (минимум 2 символа).");
    }

    // ===== ГРУППА 2: Валидация фотографий =====
    
    [Fact(DisplayName = "Валидация: когда фотографий больше 5 - должна быть ошибка")]
    public void Validate_WhenMoreThan5Photos_ShouldHaveError()
    {
        // Arrange
        var dto = new CreateDishDto
        {
            Name = "Тестовое блюдо",
            Photos = new List<string> { "1.jpg", "2.jpg", "3.jpg", "4.jpg", "5.jpg", "6.jpg" },
            Ingredients = new List<CreateIngredientDto> { new() { ProductId = 1, AmountInGrams = 100 } }
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Photos)
            .WithErrorMessage("Нельзя загрузить более 5 фотографий.");
    }

    // ===== ГРУППА 3: Валидация категории =====
    
    [Fact(DisplayName = "Валидация: когда нет макроса и категория None - должна быть ошибка")]
    public void Validate_WhenNoMacroAndCategoryIsNone_ShouldHaveError()
    {
        // Arrange
        var dto = new CreateDishDto
        {
            Name = "Простое название",
            Category = DishCategory.None,
            Ingredients = new List<CreateIngredientDto> { new() { ProductId = 1, AmountInGrams = 100 } }
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Category)
            .WithErrorMessage("Категория блюда обязательна, если в названии не указан макрос (!десерт, !первое, и т.д.).");
    }

    [Fact(DisplayName = "Валидация: когда есть макрос, категория может быть None - ошибки нет")]
    public void Validate_WhenHasMacro_CategoryCanInfer_ShouldNotHaveError()
    {
        // Arrange
        var dto = new CreateDishDto
        {
            Name = "!десерт Торт",
            Category = DishCategory.None, // Макрос укажет категорию
            Ingredients = new List<CreateIngredientDto> { new() { ProductId = 1, AmountInGrams = 100 } }
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Category);
    }

    // ===== ГРУППА 4: Валидация КБЖУ (отрицательные значения) =====
    
    [Theory(DisplayName = "Валидация: когда калорийность отрицательная - должна быть ошибка")]
    [InlineData(-1)]
    [InlineData(-100)]
    [InlineData(-0.1)]
    public void Validate_WhenCaloriesIsNegative_ShouldHaveError(double calories)
    {
        // Arrange
        var dto = new CreateDishDto
        {
            Name = "Тестовое блюдо",
            CaloriesPerServing = calories,
            Ingredients = new List<CreateIngredientDto> { new() { ProductId = 1, AmountInGrams = 100 } }
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.CaloriesPerServing)
            .WithErrorMessage("Калорийность не может быть отрицательной.");
    }

    [Theory(DisplayName = "Валидация: когда белки отрицательные - должна быть ошибка")]
    [InlineData(-1)]
    [InlineData(-100)]
    [InlineData(-0.1)]
    public void Validate_WhenProteinsIsNegative_ShouldHaveError(double proteins)
    {
        // Arrange
        var dto = new CreateDishDto
        {
            Name = "Тестовое блюдо",
            ProteinsPerServing = proteins,
            Ingredients = new List<CreateIngredientDto> { new() { ProductId = 1, AmountInGrams = 100 } }
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ProteinsPerServing)
            .WithErrorMessage("Белки не могут быть отрицательными.");
    }

    [Theory(DisplayName = "Валидация: когда жиры отрицательные - должна быть ошибка")]
    [InlineData(-1)]
    [InlineData(-100)]
    [InlineData(-0.1)]
    public void Validate_WhenFatsIsNegative_ShouldHaveError(double fats)
    {
        // Arrange
        var dto = new CreateDishDto
        {
            Name = "Тестовое блюдо",
            FatsPerServing = fats,
            Ingredients = new List<CreateIngredientDto> { new() { ProductId = 1, AmountInGrams = 100 } }
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.FatsPerServing)
            .WithErrorMessage("Жиры не могут быть отрицательными.");
    }

    [Theory(DisplayName = "Валидация: когда углеводы отрицательные - должна быть ошибка")]
    [InlineData(-1)]
    [InlineData(-100)]
    [InlineData(-0.1)]
    public void Validate_WhenCarbsIsNegative_ShouldHaveError(double carbs)
    {
        // Arrange
        var dto = new CreateDishDto
        {
            Name = "Тестовое блюдо",
            CarbsPerServing = carbs,
            Ingredients = new List<CreateIngredientDto> { new() { ProductId = 1, AmountInGrams = 100 } }
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.CarbsPerServing)
            .WithErrorMessage("Углеводы не могут быть отрицательными.");
    }

    // ===== ГРУППА 5: Валидация размера порции =====
    
    [Theory(DisplayName = "Валидация: когда размер порции ноль или отрицательный - должна быть ошибка")]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Validate_WhenServingSizeIsZeroOrNegative_ShouldHaveError(double servingSize)
    {
        // Arrange
        var dto = new CreateDishDto
        {
            Name = "Тестовое блюдо",
            ServingSize = servingSize,
            Ingredients = new List<CreateIngredientDto> { new() { ProductId = 1, AmountInGrams = 100 } }
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ServingSize)
            .WithErrorMessage("Размер порции должен быть больше 0.");
    }

    // ===== ГРУППА 6: Валидация ингредиентов =====
    
    [Fact(DisplayName = "Валидация: когда список ингредиентов null - должна быть ошибка")]
    public void Validate_WhenIngredientsIsNull_ShouldHaveError()
    {
        // Arrange
        var dto = new CreateDishDto
        {
            Name = "!десерт Тестовое блюдо", // Макрос чтобы не было ошибки категории
            Ingredients = null!
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Ingredients)
            .WithErrorMessage("Список ингредиентов не может быть пустым.");
    }

    [Fact(DisplayName = "Валидация: когда список ингредиентов пустой - должна быть ошибка")]
    public void Validate_WhenIngredientsIsEmpty_ShouldHaveError()
    {
        // Arrange
        var dto = new CreateDishDto
        {
            Name = "Тестовое блюдо",
            Ingredients = new List<CreateIngredientDto>()
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Ingredients)
            .WithErrorMessage("Должен быть хотя бы один ингредиент.");
    }

    [Fact(DisplayName = "Валидация: когда в списке ингредиентов есть null - должна быть ошибка")]
    public void Validate_WhenIngredientHasNull_ShouldHaveError()
    {
        // Arrange
        var dto = new CreateDishDto
        {
            Name = "Тестовое блюдо",
            Ingredients = new List<CreateIngredientDto> { null! }
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Ingredients)
            .WithErrorMessage("Среди ингредиентов есть пустые значения.");
    }

    // ===== ГРУППА 7: Валидация каждого ингредиента =====
    
    [Fact(DisplayName = "Валидация: когда у ингредиента невалидный ProductId - должна быть ошибка")]
    public void Validate_WhenIngredientHasInvalidProductId_ShouldHaveError()
    {
        // Arrange
        var dto = new CreateDishDto
        {
            Name = "!десерт Тестовое блюдо", // Макрос чтобы не было ошибки категории
            Ingredients = new List<CreateIngredientDto>
            {
                new() { ProductId = 0, AmountInGrams = 100 }
            }
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor("Ingredients[0].ProductId")
            .WithErrorMessage("ID продукта должен быть больше нуля.");
    }

    [Theory(DisplayName = "Валидация: когда у ингредиента невалидное количество - должна быть ошибка")]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Validate_WhenIngredientHasInvalidAmount_ShouldHaveError(double amount)
    {
        // Arrange
        var dto = new CreateDishDto
        {
            Name = "!десерт Тестовое блюдо", // Макрос чтобы не было ошибки категории
            Ingredients = new List<CreateIngredientDto>
            {
                new() { ProductId = 1, AmountInGrams = amount }
            }
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor("Ingredients[0].AmountInGrams")
            .WithErrorMessage("Количество продукта должно быть больше нуля.");
    }

    // ===== ГРУППА 8: Комбинированные ошибки =====
    
    [Fact(DisplayName = "Валидация: когда несколько ошибок - должны быть все ошибки")]
    public void Validate_WhenMultipleErrors_ShouldHaveAllErrors()
    {
        // Arrange
        var dto = new CreateDishDto
        {
            Name = "", // Ошибка 1
            CaloriesPerServing = -10, // Ошибка 2
            ProteinsPerServing = -5, // Ошибка 3
            ServingSize = 0, // Ошибка 4
            Ingredients = new List<CreateIngredientDto>
            {
                new() { ProductId = 0, AmountInGrams = 0 } // Ошибки 5 и 6
            }
            // Категория не указана, макроса нет - ошибка 7
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.Errors.Count.Should().BeGreaterThanOrEqualTo(5);
        result.ShouldHaveValidationErrorFor(x => x.Name);
        result.ShouldHaveValidationErrorFor(x => x.CaloriesPerServing);
        result.ShouldHaveValidationErrorFor(x => x.ProteinsPerServing);
        result.ShouldHaveValidationErrorFor(x => x.ServingSize);
        result.ShouldHaveValidationErrorFor("Ingredients[0].ProductId");
        result.ShouldHaveValidationErrorFor("Ingredients[0].AmountInGrams");
    }
}