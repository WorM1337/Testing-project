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

    #region Валидация названия
    
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

    [Theory(DisplayName = "Валидация: когда название валидное - не должно быть ошибки")]
    [InlineData("Борщ")]
    [InlineData("Салат Цезарь")]
    [InlineData("!десерт Торт Наполеон")]
    public void Validate_WhenNameIsValid_ShouldNotHaveError(string name)
    {
        // Arrange
        var dto = new CreateDishDto
        {
            Name = name,
            Ingredients = new List<CreateIngredientDto> { new() { ProductId = 1, AmountInGrams = 100 } }
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Name);
    }
    
    #endregion

    #region Валидация фотографий

    [Theory(DisplayName = "Валидация: когда фотографий больше 5 - должна быть ошибка")]
    [InlineData(6)]
    [InlineData(10)]
    [InlineData(25)]
    public void Validate_WhenMoreThan5Photos_ShouldHaveError(int photoCount)
    {
        // Arrange
        var dto = new CreateDishDto
        {
            Name = "Тестовое блюдо",
            Photos = GeneratePhotoUrls(photoCount),
            Ingredients = new List<CreateIngredientDto> { new() { ProductId = 1, AmountInGrams = 100 } }
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Photos)
            .WithErrorMessage("Нельзя загрузить более 5 фотографий.");
    }

    [Theory(DisplayName = "Валидация: когда фотографий 5 или меньше - не должно быть ошибки")]
    [InlineData(0)]
    [InlineData(3)]
    [InlineData(5)]
    public void Validate_When5OrFewerPhotos_ShouldNotHaveError(int photoCount)
    {
        // Arrange
        var dto = new CreateDishDto
        {
            Name = "Тестовое блюдо",
            Photos = GeneratePhotoUrls(photoCount),
            Ingredients = new List<CreateIngredientDto> { new() { ProductId = 1, AmountInGrams = 100 } }
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Photos);
    }

    private List<string> GeneratePhotoUrls(int count)
    {
        var photos = new List<string>();
        for (int i = 0; i < count; i++)
        {
            photos.Add($"https://example.com/photo{i + 1}.jpg");
        }
        return photos;
    }

    #endregion

    #region Валидация категории
    
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
            Category = DishCategory.None,
            Ingredients = new List<CreateIngredientDto> { new() { ProductId = 1, AmountInGrams = 100 } }
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Category);
    }

    [Theory(DisplayName = "Валидация: когда категория указана явно - не должно быть ошибки")]
    [InlineData(DishCategory.Entree)]
    [InlineData(DishCategory.Side)]
    [InlineData(DishCategory.Dessert)]
    public void Validate_WhenCategoryIsSpecified_ShouldNotHaveError(DishCategory category)
    {
        // Arrange
        var dto = new CreateDishDto
        {
            Name = "Тестовое блюдо",
            Category = category,
            Ingredients = new List<CreateIngredientDto> { new() { ProductId = 1, AmountInGrams = 100 } }
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Category);
    }

    #endregion

    #region Валидация КБЖУ (отрицательные значения)
    
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

    [Theory(DisplayName = "Валидация: когда калорийность валидная - не должно быть ошибки")]
    [InlineData(0)]
    [InlineData(100)]
    [InlineData(500.5)]
    public void Validate_WhenCaloriesIsValid_ShouldNotHaveError(double calories)
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
        result.ShouldNotHaveValidationErrorFor(x => x.CaloriesPerServing);
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

    [Theory(DisplayName = "Валидация: когда белки валидные - не должно быть ошибки")]
    [InlineData(0)]
    [InlineData(25)]
    [InlineData(50.3)]
    public void Validate_WhenProteinsIsValid_ShouldNotHaveError(double proteins)
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
        result.ShouldNotHaveValidationErrorFor(x => x.ProteinsPerServing);
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

    [Theory(DisplayName = "Валидация: когда жиры валидные - не должно быть ошибки")]
    [InlineData(0)]
    [InlineData(15)]
    [InlineData(30.7)]
    public void Validate_WhenFatsIsValid_ShouldNotHaveError(double fats)
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
        result.ShouldNotHaveValidationErrorFor(x => x.FatsPerServing);
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

    [Theory(DisplayName = "Валидация: когда углеводы валидные - не должно быть ошибки")]
    [InlineData(0)]
    [InlineData(45)]
    [InlineData(80.2)]
    public void Validate_WhenCarbsIsValid_ShouldNotHaveError(double carbs)
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
        result.ShouldNotHaveValidationErrorFor(x => x.CarbsPerServing);
    }

    #endregion

    #region Валидация размера порции
    
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

    [Theory(DisplayName = "Валидация: когда размер порции валидный - не должно быть ошибки")]
    [InlineData(0.1)]
    [InlineData(100)]
    [InlineData(250.5)]
    public void Validate_WhenServingSizeIsValid_ShouldNotHaveError(double servingSize)
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
        result.ShouldNotHaveValidationErrorFor(x => x.ServingSize);
    }

    #endregion

    #region Валидация ингредиентов
    
    [Fact(DisplayName = "Валидация: когда список ингредиентов null - должна быть ошибка")]
    public void Validate_WhenIngredientsIsNull_ShouldHaveError()
    {
        // Arrange
        var dto = new CreateDishDto
        {
            Name = "!десерт Тестовое блюдо",
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

    [Theory(DisplayName = "Валидация: когда список ингредиентов валидный - не должно быть ошибки")]
    [InlineData(1)]
    [InlineData(3)]
    [InlineData(10)]
    public void Validate_WhenIngredientsIsValid_ShouldNotHaveError(int ingredientCount)
    {
        // Arrange
        var dto = new CreateDishDto
        {
            Name = "Тестовое блюдо",
            Ingredients = GenerateIngredients(ingredientCount)
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Ingredients);
    }

    private List<CreateIngredientDto> GenerateIngredients(int count)
    {
        var ingredients = new List<CreateIngredientDto>();
        for (int i = 0; i < count; i++)
        {
            ingredients.Add(new CreateIngredientDto { ProductId = i + 1, AmountInGrams = 100 });
        }
        return ingredients;
    }

    #endregion

    #region Валидация каждого ингредиента
    
    [Fact(DisplayName = "Валидация: когда у ингредиента невалидный ProductId - должна быть ошибка")]
    public void Validate_WhenIngredientHasInvalidProductId_ShouldHaveError()
    {
        // Arrange
        var dto = new CreateDishDto
        {
            Name = "!десерт Тестовое блюдо",
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
            Name = "!десерт Тестовое блюдо",
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

    [Theory(DisplayName = "Валидация: когда ингредиент валидный - не должно быть ошибки")]
    [InlineData(1, 0.1)]
    [InlineData(5, 100)]
    [InlineData(10, 250.5)]
    public void Validate_WhenIngredientIsValid_ShouldNotHaveError(int productId, double amount)
    {
        // Arrange
        var dto = new CreateDishDto
        {
            Name = "Тестовое блюдо",
            Ingredients = new List<CreateIngredientDto>
            {
                new() { ProductId = productId, AmountInGrams = amount }
            }
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveValidationErrorFor("Ingredients[0].ProductId");
        result.ShouldNotHaveValidationErrorFor("Ingredients[0].AmountInGrams");
    }

    #endregion

    #region Комбинированные ошибки
    
    [Fact(DisplayName = "Валидация: когда несколько ошибок - должны быть все ошибки")]
    public void Validate_WhenMultipleErrors_ShouldHaveAllErrors()
    {
        // Arrange
        var dto = new CreateDishDto
        {
            Name = "",
            CaloriesPerServing = -10,
            ProteinsPerServing = -5,
            ServingSize = 0,
            Ingredients = new List<CreateIngredientDto>
            {
                new() { ProductId = 0, AmountInGrams = 0 }
            }
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

    #endregion
}