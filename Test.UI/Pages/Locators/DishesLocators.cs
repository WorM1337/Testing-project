namespace Test.UI.Pages.Locators;

/// <summary>
/// Локаторы для страницы блюд (Dishes)
/// </summary>
public static class DishesLocators
{
    // Кнопки
    public const string AddButton = "[data-testid='add-dish-btn']";
    public const string SaveButton = "[data-testid='save-btn']";
    public const string CancelButton = "[data-testid='cancel-btn']";
    public const string DeleteButton = "[data-testid='delete-btn']";
    public const string EditButton = "[data-testid='edit-btn']";
    public const string ViewButton = "[data-testid='view-btn']";
    public const string ApplyFiltersButton = "#filter-form .btn-primary";
    public const string ResetFiltersButton = "#filter-form .btn-secondary";

    // Поля формы создания/редактирования
    public const string NameInput = "#field-name";
    public const string CategorySelect = "#field-category";
    public const string ServingSizeInput = "#field-servingSize";
    public const string CaloriesInput = "#field-caloriesPerServing";
    public const string ProteinsInput = "#field-proteinsPerServing";
    public const string FatsInput = "#field-fatsPerServing";
    public const string CarbsInput = "#field-carbsPerServing";
    public const string PhotosContainer = "#container-photos";
    
    // Флаги
    public const string VeganFlagCheckbox = "input[name='flags[]'][value='Vegan']";
    public const string GlutenFreeFlagCheckbox = "input[name='flags[]'][value='GlutenFree']";
    public const string SugarFreeFlagCheckbox = "input[name='flags[]'][value='SugarFree']";

    // Секция ингредиентов
    public const string IngredientsContainer = "#ingredients-container";
    public const string IngredientsList = "#ingredients-list";
    public const string IngredientProductSelect = "#ingredient-product-select";
    public const string IngredientAmountInput = "#ingredient-amount";
    public const string AddIngredientButton = "[data-testid='add-ingredient-btn']";
    public const string RemoveIngredientButton = "[data-testid='remove-ingredient-btn']";
    public const string IngredientItem = ".ingredient-item";
    public const string IngredientProductName = ".product-name";
    public const string IngredientProductAmount = ".product-amount";
    
    // Расчет КБЖУ
    public const string NutritionCalculationContainer = "#nutrition-calculation-container";
    public const string CalcCaloriesInput = "#calc-calories";
    public const string CalcProteinsInput = "#calc-proteins";
    public const string CalcFatsInput = "#calc-fats";
    public const string CalcCarbsInput = "#calc-carbs";
    
    // Таблица
    public const string DataTable = "#data-table";
    public const string TableHeader = "#table-header-row";
    public const string TableBody = "#table-body";
    public const string TableRow = "tr[data-id]";
    
    // Фильтры
    public const string SearchInput = "#filter-search";
    public const string CategoryFilterSelect = "#filter-category";
    public const string SortSelect = "#filter-sort";
    public const string OrderSelect = "#filter-order";
    public const string VeganFlagFilter = ".flag-filter[value='Vegan']";
    public const string GlutenFreeFlagFilter = ".flag-filter[value='GlutenFree']";
    public const string SugarFreeFlagFilter = ".flag-filter[value='SugarFree']";
    
    // Состояния
    public const string LoadingIndicator = "#loading-indicator";
    public const string EmptyState = "#empty-state";
    
    // Уведомления
    public const string ToastContainer = "#toast-container";
    public const string ToastSuccess = ".toast-success";
    public const string ToastError = ".toast-error";
    public const string ToastMessage = ".toast-message";
    
    // Модальное окно
    public const string ModalOverlay = "#entity-modal";
    public const string ModalTitle = "#modal-title";
    public const string ModalForm = "#entity-form";
    public const string ModalFields = "#modal-fields";
    
    // Атрибуты для строк таблицы
    public const string RowIdAttribute = "data-id";
}
