using FluentAssertions;
using Microsoft.Playwright;
using Test.UI.Fixtures;
using Test.UI.Pages;

namespace Test.UI.Tests.Dishes;

/// <summary>
/// UI тесты для обновления и удаления блюд
/// Использует техники тест-дизайна:
/// - Эквивалентное разбиение (валидные/невалидные данные)
/// - Анализ граничных значений
/// </summary>
[Collection("UI Tests")]
public class UpdateDeleteDishTests(BrowserFixture browserFixture, DatabaseFixture databaseFixture)
    : UiTestBase(browserFixture, databaseFixture)
{
    #region Обновление блюд

    /// <summary>
    /// Тест: Обновление названия блюда
    /// Техника: Эквивалентное разбиение - валидное обновление
    /// </summary>
    [Fact(DisplayName = "UI: Обновление названия блюда")]
    public async Task UpdateDish_Name_Success()
    {
        // Arrange
        var (_, dishesPage, productsPage) = await InitializeTestAsync();

        // Создаем продукт и блюдо
        await productsPage.GoToProductsTabAsync();
        await productsPage.CreateProductAsync(new CreateProductDto
        {
            Name = "Продукт",
            Category = "Vegetables",
            CookingRequirement = "ReadyToUse",
            CaloriesPer100g = 50
        });
        await productsPage.IsSuccessToastVisibleAsync();

        await dishesPage.GoToDishesTabAsync();
        await dishesPage.CreateDishAsync(new CreateDishDto
        {
            Name = "Старое название",
            Category = "Side",
            Ingredients = new List<IngredientDto> { new() { ProductName = "Продукт", AmountInGrams = 100 } }
        });
        await dishesPage.IsSuccessToastVisibleAsync();

        // Act: обновляем название
        await dishesPage.EditDishAsync("Старое название", new CreateDishDto
        {
            Name = "Новое название",
            Category = "Side"
        });

        // Assert
        var isSuccess = await dishesPage.IsSuccessToastVisibleAsync();
        isSuccess.Should().BeTrue();
        
        var isInTable = await dishesPage.IsDishInTableAsync("Новое название");
        isInTable.Should().BeTrue();
    }

    /// <summary>
    /// Тест: Обновление категории блюда
    /// Техника: Эквивалентное разбиение - смена категории
    /// </summary>
    [Fact(DisplayName = "UI: Обновление категории блюда")]
    public async Task UpdateDish_Category_Success()
    {
        // Arrange
        var (_, dishesPage, productsPage) = await InitializeTestAsync();
        
        await productsPage.GoToProductsTabAsync();
        await productsPage.CreateProductAsync(new CreateProductDto
        {
            Name = "Продукт",
            Category = "Vegetables",
            CookingRequirement = "ReadyToUse",
            CaloriesPer100g = 50
        });
        await productsPage.IsSuccessToastVisibleAsync();

        await dishesPage.GoToDishesTabAsync();
        await dishesPage.CreateDishAsync(new CreateDishDto
        {
            Name = "Блюдо",
            Category = "Side",
            Ingredients = new List<IngredientDto> { new() { ProductName = "Продукт", AmountInGrams = 100 } }
        });
        await dishesPage.IsSuccessToastVisibleAsync();

        // Act: меняем категорию
        await dishesPage.EditDishAsync("Блюдо", new CreateDishDto
        {
            Name = "Блюдо",
            Category = "Salad"
        });

        // Assert
        var isSuccess = await dishesPage.IsSuccessToastVisibleAsync();
        isSuccess.Should().BeTrue();
    }

    /// <summary>
    /// Тест: Обновление КБЖУ блюда
    /// Техника: Эквивалентное разбиение - обновление числовых полей
    /// </summary>
    [Fact(DisplayName = "UI: Обновление КБЖУ блюда")]
    public async Task UpdateDish_Nutrition_Success()
    {
        // Arrange
        var (_, dishesPage, productsPage) = await InitializeTestAsync();

        await productsPage.GoToProductsTabAsync();
        await productsPage.CreateProductAsync(new CreateProductDto
        {
            Name = "Продукт",
            Category = "Vegetables",
            CookingRequirement = "ReadyToUse",
            CaloriesPer100g = 50
        });
        await productsPage.IsSuccessToastVisibleAsync();

        await dishesPage.GoToDishesTabAsync();
        await dishesPage.CreateDishAsync(new CreateDishDto
        {
            Name = "Блюдо",
            Category = "Side",
            Ingredients = new List<IngredientDto> { new() { ProductName = "Продукт", AmountInGrams = 100 } }
        });
        await dishesPage.IsSuccessToastVisibleAsync();

        // Act: обновляем КБЖУ
        await dishesPage.EditDishAsync("Блюдо", new CreateDishDto
        {
            Name = "Блюдо",
            Category = "Side",
            CaloriesPerServing = 500,
            ProteinsPerServing = 50,
            FatsPerServing = 30,
            CarbsPerServing = 60
        });

        // Assert
        var isSuccess = await dishesPage.IsSuccessToastVisibleAsync();
        isSuccess.Should().BeTrue();
    }

    /// <summary>
    /// Тест: Обновление названия на минимальную длину
    /// Техника: Анализ граничных значений - нижняя граница
    /// </summary>
    [Fact(DisplayName = "UI: Обновление названия блюда на минимальную длину (2 символа)")]
    public async Task UpdateDish_NameMinBoundary_Success()
    {
        // Arrange
        var (_, dishesPage, productsPage) = await InitializeTestAsync();

        await productsPage.GoToProductsTabAsync();
        await productsPage.CreateProductAsync(new CreateProductDto
        {
            Name = "Продукт",
            Category = "Vegetables",
            CookingRequirement = "ReadyToUse",
            CaloriesPer100g = 50
        });

        await dishesPage.GoToDishesTabAsync();
        await dishesPage.CreateDishAsync(new CreateDishDto
        {
            Name = "Длинное название",
            Category = "Side",
            Ingredients = new List<IngredientDto> { new() { ProductName = "Продукт", AmountInGrams = 100 } }
        });
        await dishesPage.IsSuccessToastVisibleAsync();

        // Act: обновляем на минимальную длину
        await dishesPage.EditDishAsync("Длинное название", new CreateDishDto
        {
            Name = "Суп",
            Category = "Soup"
        });

        // Assert
        var isSuccess = await dishesPage.IsSuccessToastVisibleAsync();
        isSuccess.Should().BeTrue();
    }

    /// <summary>
    /// Тест: Обновление названия на слишком короткое
    /// Техника: Анализ граничных значений - за нижней границей
    /// </summary>
    [Fact(DisplayName = "UI: Обновление названия блюда на слишком короткое (ошибка)")]
    public async Task UpdateDish_NameTooShort_Error()
    {
        // Arrange
        var (page, dishesPage, productsPage) = await InitializeTestAsync();

        await productsPage.GoToProductsTabAsync();
        await productsPage.CreateProductAsync(new CreateProductDto
        {
            Name = "Продукт",
            Category = "Vegetables",
            CookingRequirement = "ReadyToUse",
            CaloriesPer100g = 50
        });
        await productsPage.IsSuccessToastVisibleAsync();

        await dishesPage.GoToDishesTabAsync();
        await dishesPage.CreateDishAsync(new CreateDishDto
        {
            Name = "Нормальное название",
            Category = "Side",
            Ingredients = new List<IngredientDto> { new() { ProductName = "Продукт", AmountInGrams = 100 } }
        });
        await dishesPage.IsSuccessToastVisibleAsync();

        // Act: пытаемся обновить на 1 символ
        await dishesPage.OpenEditModalAsync("Нормальное название");
        await dishesPage.FillDishFormAsync(new CreateDishDto
        {
            Name = "Б"
        }, isEdit: true);
        // Кликаем кнопку сохранения - валидация должна показать ошибку на поле
        await dishesPage.SaveButton.ClickAsync();
        await page.WaitForTimeoutAsync(500); // Ждём появления ошибки
        
        // Assert: проверяем, что на поле названия появилась ошибка валидации
        var nameInput = page.Locator("#field-name");
        var nameFormGroup = nameInput.Locator(".."); // Родительский .form-group
        var errorMessage = nameFormGroup.Locator(".error-message");
        await errorMessage.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = 2000 });
        
        var errorText = await errorMessage.TextContentAsync();
        errorText.Should().Contain("2 символов", "валидация должна требовать минимум 2 символа");
        
        // Закрываем модалку после ошибки через кнопку отмены
        var cancelButton = page.Locator("[data-testid='cancel-btn']");
        await cancelButton.ClickAsync();
        await dishesPage.ModalOverlay.WaitForAsync(new() { State = WaitForSelectorState.Hidden, Timeout = 2000 });
    }

    /// <summary>
    /// Тест: Обновление с отрицательной калорийностью
    /// Техника: Анализ граничных значений - отрицательное значение
    /// Примечание: HTML5 валидация (min="0") блокирует отправку формы
    /// </summary>
    [Fact(DisplayName = "UI: Обновление блюда с отрицательной калорийностью (ошибка)")]
    public async Task UpdateDish_NegativeCalories_Error()
    {
        // Arrange
        var (page, dishesPage, productsPage) = await InitializeTestAsync();

        await productsPage.GoToProductsTabAsync();
        await productsPage.CreateProductAsync(new CreateProductDto
        {
            Name = "Продукт",
            Category = "Vegetables",
            CookingRequirement = "ReadyToUse",
            CaloriesPer100g = 50
        });
        await productsPage.IsSuccessToastVisibleAsync();

        await dishesPage.GoToDishesTabAsync();
        await dishesPage.CreateDishAsync(new CreateDishDto
        {
            Name = "Блюдо",
            Category = "Side",
            Ingredients = new List<IngredientDto> { new() { ProductName = "Продукт", AmountInGrams = 100 } }
        });
        await dishesPage.IsSuccessToastVisibleAsync();

        // Act: пытаемся установить отрицательную калорийность
        await dishesPage.OpenEditModalAsync("Блюдо");
        await dishesPage.FillDishFormAsync(new CreateDishDto
        {
            Name = "Блюдо",
            CaloriesPerServing = -100
        }, isEdit: true);
        
        // Кликаем кнопку сохранения - HTML5 валидация должна заблокировать отправку
        await dishesPage.SaveButton.ClickAsync();
        
        // Небольшая пауза, чтобы браузер применил валидацию
        await page.WaitForTimeoutAsync(500);
        
        // Assert: модалка должна остаться открытой (форма не отправилась из-за валидации)
        var isModalVisible = await dishesPage.ModalOverlay.IsVisibleAsync();
        isModalVisible.Should().BeTrue("HTML5 валидация должна блокировать отправку формы с отрицательной калорийностью");
        
        // Закрываем модалку через кнопку отмены
        var cancelButton = page.Locator("[data-testid='cancel-btn']");
        await cancelButton.ClickAsync();
        await dishesPage.ModalOverlay.WaitForAsync(new() { State = WaitForSelectorState.Hidden, Timeout = 2000 });
    }

    #endregion

    #region Удаление блюд

    /// <summary>
    /// Тест: Удаление существующего блюда
    /// Техника: Эквивалентное разбиение - валидное удаление
    /// </summary>
    [Fact(DisplayName = "UI: Удаление существующего блюда")]
    public async Task DeleteDish_Existing_Success()
    {
        // Arrange
        var (_, dishesPage, productsPage) = await InitializeTestAsync();

        await productsPage.GoToProductsTabAsync();
        await productsPage.CreateProductAsync(new CreateProductDto
        {
            Name = "Продукт",
            Category = "Vegetables",
            CookingRequirement = "ReadyToUse",
            CaloriesPer100g = 50
        });
        await productsPage.IsSuccessToastVisibleAsync();

        await dishesPage.GoToDishesTabAsync();
        await dishesPage.CreateDishAsync(new CreateDishDto
        {
            Name = "Блюдо для удаления",
            Category = "Side",
            Ingredients = new List<IngredientDto> { new() { ProductName = "Продукт", AmountInGrams = 100 } }
        });
        await dishesPage.IsSuccessToastVisibleAsync();

        // Act: удаляем блюдо
        await dishesPage.DeleteDishAsync("Блюдо для удаления");

        // Assert
        var isSuccess = await dishesPage.IsSuccessToastVisibleAsync();
        isSuccess.Should().BeTrue();
        
        var isNotInTable = await dishesPage.IsDishNotInTableAsync("Блюдо для удаления");
        isNotInTable.Should().BeTrue();
    }

    /// <summary>
    /// Тест: Удаление нескольких блюд по очереди
    /// Техника: Эквивалентное разбиение - множественное удаление
    /// </summary>
    [Fact(DisplayName = "UI: Удаление нескольких блюд по очереди")]
    public async Task DeleteDish_Multiple_Success()
    {
        // Arrange
        var (_, dishesPage, productsPage) = await InitializeTestAsync();

        await productsPage.GoToProductsTabAsync();
        await productsPage.CreateProductAsync(new CreateProductDto
        {
            Name = "Продукт",
            Category = "Vegetables",
            CookingRequirement = "ReadyToUse",
            CaloriesPer100g = 50
        });

        await dishesPage.GoToDishesTabAsync();
        
        // Создаем несколько блюд
        await dishesPage.CreateDishAsync(new CreateDishDto
        {
            Name = "Блюдо 1",
            Category = "Side",
            Ingredients = new List<IngredientDto> { new() { ProductName = "Продукт", AmountInGrams = 100 } }
        });
        await dishesPage.IsSuccessToastVisibleAsync();
        await dishesPage.CreateDishAsync(new CreateDishDto
        {
            Name = "Блюдо 2",
            Category = "Soup",
            Ingredients = new List<IngredientDto> { new() { ProductName = "Продукт", AmountInGrams = 150 } }
        });
        await dishesPage.IsSuccessToastVisibleAsync();
        await dishesPage.CreateDishAsync(new CreateDishDto
        {
            Name = "Блюдо 3",
            Category = "Salad",
            Ingredients = new List<IngredientDto> { new() { ProductName = "Продукт", AmountInGrams = 200 } }
        });
        await dishesPage.IsSuccessToastVisibleAsync();

        // Act: удаляем все блюда
        await dishesPage.DeleteDishAsync("Блюдо 1");
        await dishesPage.DeleteDishAsync("Блюдо 2");
        await dishesPage.DeleteDishAsync("Блюдо 3");

        // Assert
        var rowCount = await dishesPage.GetRowCountAsync();
        rowCount.Should().Be(0, "все блюда должны быть удалены");
    }

    /// <summary>
    /// Тест: Удаление блюда с длинным названием
    /// Техника: Анализ граничных значений - длинное название
    /// </summary>
    [Fact(DisplayName = "UI: Удаление блюда с длинным названием (50 символов)")]
    public async Task DeleteDish_LongName_Success()
    {
        // Arrange
        var (_, dishesPage, productsPage) = await InitializeTestAsync();

        await productsPage.GoToProductsTabAsync();
        await productsPage.CreateProductAsync(new CreateProductDto
        {
            Name = "Продукт",
            Category = "Vegetables",
            CookingRequirement = "ReadyToUse",
            CaloriesPer100g = 50
        });
        await productsPage.IsSuccessToastVisibleAsync();

        var longName = new string('А', 50);
        await dishesPage.GoToDishesTabAsync();
        await dishesPage.CreateDishAsync(new CreateDishDto
        {
            Name = longName,
            Category = "Side",
            Ingredients = new List<IngredientDto> { new() { ProductName = "Продукт", AmountInGrams = 100 } }
        });
        await dishesPage.IsSuccessToastVisibleAsync();

        // Act: удаляем блюдо
        await dishesPage.DeleteDishAsync(longName);

        // Assert
        var isSuccess = await dishesPage.IsSuccessToastVisibleAsync();
        isSuccess.Should().BeTrue();
    }

    /// <summary>
    /// Тест: Удаление и повторное создание блюда с тем же названием
    /// Техника: Эквивалентное разбиение - повторное использование имени
    /// </summary>
    [Fact(DisplayName = "UI: Удаление и повторное создание блюда с тем же названием")]
    public async Task DeleteDish_RecreateSameName_Success()
    {
        // Arrange
        var (_, dishesPage, productsPage) = await InitializeTestAsync();

        await productsPage.GoToProductsTabAsync();
        await productsPage.CreateProductAsync(new CreateProductDto
        {
            Name = "Продукт",
            Category = "Vegetables",
            CookingRequirement = "ReadyToUse",
            CaloriesPer100g = 50
        });
        await productsPage.IsSuccessToastVisibleAsync();

        var dishName = "Уникальное блюдо";
        await dishesPage.GoToDishesTabAsync();
        await dishesPage.CreateDishAsync(new CreateDishDto
        {
            Name = dishName,
            Category = "Side",
            Ingredients = new List<IngredientDto> { new() { ProductName = "Продукт", AmountInGrams = 100 } }
        });
        await dishesPage.IsSuccessToastVisibleAsync();

        // Act 1: удаляем блюдо
        await dishesPage.DeleteDishAsync(dishName);
        await dishesPage.IsSuccessToastVisibleAsync();

        // Act 2: создаем блюдо с тем же названием
        await dishesPage.CreateDishAsync(new CreateDishDto
        {
            Name = dishName,
            Category = "Side",
            Ingredients = new List<IngredientDto> { new() { ProductName = "Продукт", AmountInGrams = 100 } }
        });

        // Assert
        var isSuccess = await dishesPage.IsSuccessToastVisibleAsync();
        isSuccess.Should().BeTrue();
        
        var isInTable = await dishesPage.IsDishInTableAsync(dishName);
        isInTable.Should().BeTrue();
    }

    #endregion
}
