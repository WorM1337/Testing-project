using System.Globalization;
using Microsoft.Playwright;
using Test.UI.Pages.Locators;

namespace Test.UI.Pages;

/// <summary>
/// Page Object для страницы блюд
/// </summary>
public class DishesPage : BasePage
{
    public DishesPage(IPage page, string baseUrl = "http://localhost:5000") 
        : base(page, baseUrl)
    {
    }

    #region Селекторы

    // Кнопки
    private ILocator AddButton => Page.Locator(DishesLocators.AddButton);
    public ILocator SaveButton => Page.Locator(DishesLocators.SaveButton);
    private ILocator AddIngredientButton => Page.Locator(DishesLocators.AddIngredientButton);
    
    // Поля формы
    private ILocator NameInput => Page.Locator(DishesLocators.NameInput);
    private ILocator CategorySelect => Page.Locator(DishesLocators.CategorySelect);
    private ILocator ServingSizeInput => Page.Locator(DishesLocators.ServingSizeInput);
    private ILocator CaloriesInput => Page.Locator(DishesLocators.CaloriesInput);
    private ILocator ProteinsInput => Page.Locator(DishesLocators.ProteinsInput);
    private ILocator FatsInput => Page.Locator(DishesLocators.FatsInput);
    private ILocator CarbsInput => Page.Locator(DishesLocators.CarbsInput);
    
    // Флаги
    private ILocator VeganFlagCheckbox => Page.Locator(DishesLocators.VeganFlagCheckbox);
    private ILocator GlutenFreeFlagCheckbox => Page.Locator(DishesLocators.GlutenFreeFlagCheckbox);
    private ILocator SugarFreeFlagCheckbox => Page.Locator(DishesLocators.SugarFreeFlagCheckbox);
    
    // Ингредиенты
    private ILocator IngredientsContainer => Page.Locator(DishesLocators.IngredientsContainer);
    private ILocator IngredientProductSelect => Page.Locator(DishesLocators.IngredientProductSelect);
    private ILocator IngredientAmountInput => Page.Locator(DishesLocators.IngredientAmountInput);
    private ILocator IngredientsList => Page.Locator(DishesLocators.IngredientsList);
    
    // Таблица
    private ILocator DataTable => Page.Locator(DishesLocators.DataTable);
    private ILocator TableBody => Page.Locator(DishesLocators.TableBody);
    
    // Фильтры
    private ILocator SearchInput => Page.Locator(DishesLocators.SearchInput);
    private ILocator CategoryFilterSelect => Page.Locator(DishesLocators.CategoryFilterSelect);
    private ILocator ApplyFiltersButton => Page.Locator(DishesLocators.ApplyFiltersButton);
    private ILocator ResetFiltersButton => Page.Locator(DishesLocators.ResetFiltersButton);
    
    // Модальное окно
    private ILocator ModalOverlay => Page.Locator(DishesLocators.ModalOverlay);

    #endregion

    /// <summary>
    /// Переходит на вкладку блюд
    /// </summary>
    public async Task GoToDishesTabAsync()
    {
        await GoToAsync();
        await Page.ClickAsync("[data-tab='dishes']");
        await Page.WaitForSelectorAsync(DishesLocators.AddButton);
    }

    /// <summary>
    /// Открывает модальное окно создания блюда
    /// </summary>
    public async Task OpenCreateModalAsync()
    {
        await AddButton.ClickAsync();
        await ModalOverlay.WaitForAsync(new() { State = WaitForSelectorState.Visible });
    }

    /// <summary>
    /// Заполняет форму создания/редактирования блюда
    /// </summary>
    public async Task FillDishFormAsync(CreateDishDto dto, bool isEdit = false)
    {
        if (!string.IsNullOrEmpty(dto.Name))
            await NameInput.FillAsync(dto.Name);

        if (!string.IsNullOrEmpty(dto.Category))
            await CategorySelect.SelectOptionAsync(dto.Category);

        // Используем инвариантную культуру для чисел
        // Для создания заполняем все поля (даже 0), для редактирования — только указанные
        if (!isEdit)
        {
            await ServingSizeInput.FillAsync((dto.ServingSize ?? 0).ToString(CultureInfo.InvariantCulture));
            await CaloriesInput.FillAsync((dto.CaloriesPerServing ?? 0).ToString(CultureInfo.InvariantCulture));
            await ProteinsInput.FillAsync((dto.ProteinsPerServing ?? 0).ToString(CultureInfo.InvariantCulture));
            await FatsInput.FillAsync((dto.FatsPerServing ?? 0).ToString(CultureInfo.InvariantCulture));
            await CarbsInput.FillAsync((dto.CarbsPerServing ?? 0).ToString(CultureInfo.InvariantCulture));
        }
        else
        {
            // Для редактирования заполняем только указанные поля
            if (dto.ServingSize.HasValue)
                await ServingSizeInput.FillAsync(dto.ServingSize.Value.ToString(CultureInfo.InvariantCulture));
            if (dto.CaloriesPerServing.HasValue)
                await CaloriesInput.FillAsync(dto.CaloriesPerServing.Value.ToString(CultureInfo.InvariantCulture));
            if (dto.ProteinsPerServing.HasValue)
                await ProteinsInput.FillAsync(dto.ProteinsPerServing.Value.ToString(CultureInfo.InvariantCulture));
            if (dto.FatsPerServing.HasValue)
                await FatsInput.FillAsync(dto.FatsPerServing.Value.ToString(CultureInfo.InvariantCulture));
            if (dto.CarbsPerServing.HasValue)
                await CarbsInput.FillAsync(dto.CarbsPerServing.Value.ToString(CultureInfo.InvariantCulture));
        }
    }

    /// <summary>
    /// Добавляет ингредиент к блюду
    /// </summary>
    /// <param name="productName">Название продукта для выбора из списка</param>
    /// <param name="amountInGrams">Количество в граммах</param>
    public async Task AddIngredientAsync(string productName, double amountInGrams)
    {
        // Выбираем продукт из выпадающего списка по названию
        var productSelect = IngredientProductSelect;
        await productSelect.SelectOptionAsync(productName);
        
        // Вводим количество
        await IngredientAmountInput.FillAsync(amountInGrams.ToString(CultureInfo.InvariantCulture));
        
        // Нажимаем "Добавить"
        await AddIngredientButton.ClickAsync();
        await Page.WaitForTimeoutAsync(200); // Ждём добавления ингредиента в список
    }

    /// <summary>
    /// Устанавливает флаги
    /// </summary>
    public async Task SetFlagsAsync(bool vegan = false, bool glutenFree = false, bool sugarFree = false)
    {
        if (vegan && !await VeganFlagCheckbox.IsCheckedAsync())
            await VeganFlagCheckbox.CheckAsync();

        if (glutenFree && !await GlutenFreeFlagCheckbox.IsCheckedAsync())
            await GlutenFreeFlagCheckbox.CheckAsync();

        if (sugarFree && !await SugarFreeFlagCheckbox.IsCheckedAsync())
            await SugarFreeFlagCheckbox.CheckAsync();
    }

    /// <summary>
    /// Сохраняет блюдо
    /// </summary>
    public async Task SaveDishAsync()
    {
        await SaveButton.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = 5000 });
        await SaveButton.ClickAsync();
        
        // Ждём появления toast-уведомления (это сигнал, что сохранение прошло)
        var toast = Page.Locator(".toast.success, .toast.error").Last;
        await toast.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = 5000 });
        await Page.WaitForTimeoutAsync(500); // Пауза для закрытия модалки и отрисовки toast
    }

    /// <summary>
    /// Создает блюдо через UI
    /// </summary>
    public async Task CreateDishAsync(CreateDishDto dto)
    {
        await OpenCreateModalAsync();
        await FillDishFormAsync(dto);
        
        // Добавляем ингредиенты
        if (dto.Ingredients != null)
        {
            foreach (var ingredient in dto.Ingredients)
            {
                await AddIngredientAsync(ingredient.ProductName, ingredient.AmountInGrams);
            }
        }
        
        await SaveDishAsync();
    }

    /// <summary>
    /// Проверяет, что блюдо отображается в таблице
    /// </summary>
    public async Task<bool> IsDishInTableAsync(string dishName)
    {
        var row = TableBody.Locator($"tr:has-text('{dishName}')");
        try
        {
            await row.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = 5000 });
            return true;
        }
        catch (TimeoutException)
        {
            return false;
        }
    }

    /// <summary>
    /// Получает количество строк в таблице
    /// </summary>
    public async Task<int> GetRowCountAsync()
    {
        return await TableBody.Locator("tr").CountAsync();
    }

    /// <summary>
    /// Фильтрует блюда по названию
    /// </summary>
    public async Task FilterBySearchAsync(string searchText)
    {
        await SearchInput.FillAsync(searchText);
        await ApplyFiltersButton.ClickAsync();
        await Page.WaitForTimeoutAsync(500); // Небольшая задержка для применения фильтра
    }

    /// <summary>
    /// Проверяет наличие предупреждения о пустых ингредиентах
    /// </summary>
    public async Task<bool> IsEmptyIngredientsWarningVisibleAsync()
    {
        try
        {
            var warning = Page.Locator("#ingredients-warning");
            await warning.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = 2000 });
            return true;
        }
        catch (TimeoutException)
        {
            return false;
        }
    }

    /// <summary>
    /// Проверяет, активна ли кнопка сохранения
    /// </summary>
    public async Task<bool> IsSaveButtonEnabledAsync()
    {
        try
        {
            await SaveButton.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = 2000 });
            var isDisabled = await SaveButton.GetAttributeAsync("disabled");
            return string.IsNullOrEmpty(isDisabled);
        }
        catch (TimeoutException)
        {
            return false;
        }
    }

    /// <summary>
    /// Открывает модальное окно редактирования блюда
    /// </summary>
    public async Task OpenEditModalAsync(string dishName)
    {
        var row = TableBody.Locator($"tr:has-text('{dishName}')");
        await row.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = 5000 });
        
        var editButton = row.Locator("[data-testid='edit-btn']");
        await editButton.ClickAsync();
        await ModalOverlay.WaitForAsync(new() { State = WaitForSelectorState.Visible });
    }

    /// <summary>
    /// Редактирует блюдо через UI
    /// </summary>
    public async Task EditDishAsync(string dishName, CreateDishDto dto)
    {
        await OpenEditModalAsync(dishName);
        await FillDishFormAsync(dto, isEdit: true);
        await SaveDishAsync();
    }

    /// <summary>
    /// Удаляет блюдо через UI
    /// </summary>
    public async Task DeleteDishAsync(string dishName)
    {
        var row = TableBody.Locator($"tr:has-text('{dishName}')");
        await row.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = 5000 });
        
        var deleteButton = row.Locator("[data-testid='delete-btn']");
        
        // Обрабатываем диалог подтверждения
        await HandleConfirmationDialogAsync(async () =>
        {
            await deleteButton.ClickAsync();
        });
        
        // Ждём появления toast-уведомления об успешном удалении
        var toast = Page.Locator(".toast.success").Last;
        await toast.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = 5000 });
        await Page.WaitForTimeoutAsync(100);
        
        // Ждём обновления таблицы (исчезновения строки)
        await row.WaitForAsync(new() { State = WaitForSelectorState.Hidden, Timeout = 5000 });
    }

    /// <summary>
    /// Проверяет, что блюдо НЕ отображается в таблице
    /// </summary>
    public async Task<bool> IsDishNotInTableAsync(string dishName)
    {
        await Page.WaitForTimeoutAsync(1000); // Небольшая задержка для обновления таблицы
        var row = TableBody.Locator($"tr:has-text('{dishName}')");
        var count = await row.CountAsync();
        return count == 0;
    }
}

/// <summary>
/// DTO для создания блюда в UI тестах
/// </summary>
public class CreateDishDto
{
    public string? Name { get; set; }
    public string? Category { get; set; }
    public decimal? ServingSize { get; set; }
    public decimal? CaloriesPerServing { get; set; }
    public decimal? ProteinsPerServing { get; set; }
    public decimal? FatsPerServing { get; set; }
    public decimal? CarbsPerServing { get; set; }
    public List<IngredientDto>? Ingredients { get; set; }
}

/// <summary>
/// DTO для ингредиента
/// </summary>
public class IngredientDto
{
    /// <summary>
    /// Название продукта для выбора из списка
    /// </summary>
    public string ProductName { get; set; } = string.Empty;
    
    /// <summary>
    /// Количество в граммах
    /// </summary>
    public double AmountInGrams { get; set; }
}
