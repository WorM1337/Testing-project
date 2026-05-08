using System.Globalization;
using Microsoft.Playwright;
using Test.UI.Pages.Locators;

namespace Test.UI.Pages;

/// <summary>
/// Page Object для страницы продуктов
/// </summary>
public class ProductsPage : BasePage
{
    public ProductsPage(IPage page, string baseUrl = "http://localhost:5000") 
        : base(page, baseUrl)
    {
    }

    #region Селекторы

    // Кнопки
    private ILocator AddButton => Page.Locator(ProductsLocators.AddButton);
    private ILocator SaveButton => Page.Locator(ProductsLocators.SaveButton);
    
    // Поля формы
    private ILocator NameInput => Page.Locator(ProductsLocators.NameInput);
    private ILocator CategorySelect => Page.Locator(ProductsLocators.CategorySelect);
    private ILocator CookingRequirementSelect => Page.Locator(ProductsLocators.CookingRequirementSelect);
    private ILocator CaloriesInput => Page.Locator(ProductsLocators.CaloriesInput);
    private ILocator ProteinsInput => Page.Locator(ProductsLocators.ProteinsInput);
    private ILocator FatsInput => Page.Locator(ProductsLocators.FatsInput);
    private ILocator CarbsInput => Page.Locator(ProductsLocators.CarbsInput);
    private ILocator CompositionTextarea => Page.Locator(ProductsLocators.CompositionTextarea);
    
    // Флаги
    private ILocator VeganFlagCheckbox => Page.Locator(ProductsLocators.VeganFlagCheckbox);
    private ILocator GlutenFreeFlagCheckbox => Page.Locator(ProductsLocators.GlutenFreeFlagCheckbox);
    private ILocator SugarFreeFlagCheckbox => Page.Locator(ProductsLocators.SugarFreeFlagCheckbox);
    
    // Макросы
    private ILocator MacrosSumValue => Page.Locator(ProductsLocators.MacrosSumValue);
    private ILocator MacrosSumWarning => Page.Locator(ProductsLocators.MacrosSumWarning);
    
    // Таблица
    private ILocator DataTable => Page.Locator(ProductsLocators.DataTable);
    private ILocator TableBody => Page.Locator(ProductsLocators.TableBody);
    
    // Фильтры
    private ILocator SearchInput => Page.Locator(ProductsLocators.SearchInput);
    private ILocator CategoryFilterSelect => Page.Locator(ProductsLocators.CategoryFilterSelect);
    private ILocator ApplyFiltersButton => Page.Locator(ProductsLocators.ApplyFiltersButton);
    private ILocator ResetFiltersButton => Page.Locator(ProductsLocators.ResetFiltersButton);
    
    // Модальное окно
    private ILocator ModalOverlay => Page.Locator(ProductsLocators.ModalOverlay);
    private ILocator ModalTitle => Page.Locator(ProductsLocators.ModalTitle);

    #endregion

    /// <summary>
    /// Переходит на вкладку продуктов
    /// </summary>
    public async Task GoToProductsTabAsync()
    {
        await GoToAsync();
        await Page.ClickAsync("[data-tab='products']");
        await Page.WaitForSelectorAsync(ProductsLocators.AddButton);
    }

    /// <summary>
    /// Открывает модальное окно создания продукта
    /// </summary>
    public async Task OpenCreateModalAsync()
    {
        await AddButton.ClickAsync();
        await ModalOverlay.WaitForAsync(new() { State = WaitForSelectorState.Visible });
    }

    /// <summary>
    /// Заполняет форму создания продукта
    /// </summary>
    public async Task FillProductFormAsync(CreateProductDto dto)
    {
        if (!string.IsNullOrEmpty(dto.Name))
            await NameInput.FillAsync(dto.Name);

        if (!string.IsNullOrEmpty(dto.Category))
            await CategorySelect.SelectOptionAsync(dto.Category);

        if (!string.IsNullOrEmpty(dto.CookingRequirement))
            await CookingRequirementSelect.SelectOptionAsync(dto.CookingRequirement);

        // Используем инвариантную культуру для чисел (точка вместо запятой)
        // Заполняем ВСЕ числовые поля, даже если они не указаны (используем 0 как default)
        await CaloriesInput.FillAsync((dto.CaloriesPer100g ?? 0).ToString(CultureInfo.InvariantCulture));
        await ProteinsInput.FillAsync((dto.ProteinsPer100g ?? 0).ToString(CultureInfo.InvariantCulture));
        await FatsInput.FillAsync((dto.FatsPer100g ?? 0).ToString(CultureInfo.InvariantCulture));
        await CarbsInput.FillAsync((dto.CarbsPer100g ?? 0).ToString(CultureInfo.InvariantCulture));

        if (!string.IsNullOrEmpty(dto.Composition))
            await CompositionTextarea.FillAsync(dto.Composition);
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
    /// Сохраняет продукт
    /// </summary>
    public async Task SaveProductAsync()
    {
        await SaveButton.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = 5000 });
        await SaveButton.ClickAsync();
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
    /// Создает продукт через UI
    /// </summary>
    public async Task CreateProductAsync(CreateProductDto dto)
    {
        await OpenCreateModalAsync();
        await FillProductFormAsync(dto);
        await SaveProductAsync();
    }

    /// <summary>
    /// Проверяет, что продукт отображается в таблице
    /// </summary>
    public async Task<bool> IsProductInTableAsync(string productName)
    {
        var row = TableBody.Locator($"tr:has-text('{productName}')");
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
    /// Фильтрует продукты по названию
    /// </summary>
    public async Task FilterBySearchAsync(string searchText)
    {
        await SearchInput.FillAsync(searchText);
        await ApplyFiltersButton.ClickAsync();
        await Page.WaitForTimeoutAsync(500); // Небольшая задержка для применения фильтра
    }

    /// <summary>
    /// Получает значение суммы БЖУ
    /// </summary>
    public async Task<string> GetMacrosSumAsync()
    {
        return await MacrosSumValue.TextContentAsync() ?? "0.00 г";
    }

    /// <summary>
    /// Проверяет, что отображается предупреждение о превышении БЖУ
    /// </summary>
    public async Task<bool> IsMacrosSumWarningVisibleAsync()
    {
        try
        {
            await MacrosSumWarning.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = 1000 });
            return true;
        }
        catch (TimeoutException)
        {
            return false;
        }
    }

    /// <summary>
    /// Открывает модальное окно редактирования продукта
    /// </summary>
    public async Task OpenEditModalAsync(string productName)
    {
        var row = TableBody.Locator($"tr:has-text('{productName}')");
        await row.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = 5000 });
        
        var editButton = row.Locator("[data-testid='edit-btn']");
        await editButton.ClickAsync();
        await ModalOverlay.WaitForAsync(new() { State = WaitForSelectorState.Visible });
    }

    /// <summary>
    /// Редактирует продукт через UI
    /// </summary>
    public async Task EditProductAsync(string productName, CreateProductDto dto)
    {
        await OpenEditModalAsync(productName);
        await FillProductFormAsync(dto);
        await SaveProductAsync();
    }

    /// <summary>
    /// Удаляет продукт через UI
    /// </summary>
    public async Task DeleteProductAsync(string productName)
    {
        var row = TableBody.Locator($"tr:has-text('{productName}')");
        await row.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = 5000 });
        
        var deleteButton = row.Locator("[data-testid='delete-btn']");
        await deleteButton.ClickAsync();
        
        // Подтверждаем удаление (если есть диалог)
        try
        {
            var confirmButton = Page.Locator("[data-testid='confirm-delete-btn']");
            await confirmButton.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = 2000 });
            await confirmButton.ClickAsync();
        }
        catch (TimeoutException)
        {
            // Диалога подтверждения нет, удаление произошло сразу
        }
    }

    /// <summary>
    /// Проверяет, что продукт НЕ отображается в таблице
    /// </summary>
    public async Task<bool> IsProductNotInTableAsync(string productName)
    {
        await Page.WaitForTimeoutAsync(1000); // Небольшая задержка для обновления таблицы
        var row = TableBody.Locator($"tr:has-text('{productName}')");
        var count = await row.CountAsync();
        return count == 0;
    }

    /// <summary>
    /// Получает значение из ячейки таблицы
    /// </summary>
    public async Task<string?> GetCellValueAsync(string productName, string columnClass)
    {
        var row = TableBody.Locator($"tr:has-text('{productName}')");
        var cell = row.Locator($".{columnClass}");
        try
        {
            await cell.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = 2000 });
            return await cell.TextContentAsync();
        }
        catch (TimeoutException)
        {
            return null;
        }
    }
}

/// <summary>
/// DTO для создания продукта в UI тестах
/// </summary>
public class CreateProductDto
{
    public string? Name { get; set; }
    public string? Category { get; set; }
    public string? CookingRequirement { get; set; }
    public decimal? CaloriesPer100g { get; set; }
    public decimal? ProteinsPer100g { get; set; }
    public decimal? FatsPer100g { get; set; }
    public decimal? CarbsPer100g { get; set; }
    public string? Composition { get; set; }
}
