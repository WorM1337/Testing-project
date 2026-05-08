using Microsoft.Playwright;

namespace Test.UI.Pages;

/// <summary>
/// Базовый класс для всех страниц приложения
/// </summary>
public abstract class BasePage
{
    protected readonly IPage Page;
    protected readonly string BaseUrl;

    protected BasePage(IPage page, string baseUrl)
    {
        Page = page;
        BaseUrl = baseUrl;
    }

    private EventHandler<IDialog>? _dialogHandler;

    /// <summary>
    /// Переходит на страницу
    /// </summary>
    public async Task GoToAsync(string relativeUrl = "")
    {
        var url = string.IsNullOrEmpty(relativeUrl) 
            ? BaseUrl 
            : $"{BaseUrl}{relativeUrl}";
        await Page.GotoAsync(url);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }

    /// <summary>
    /// Проверяет, что страница загружена
    /// </summary>
    public async Task<bool> IsPageLoadedAsync()
    {
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        return true;
    }

    /// <summary>
    /// Получает текст уведомления
    /// </summary>
    public async Task<string?> GetToastMessageAsync()
    {
        var toast = Page.Locator(".toast-message");
        await toast.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = 5000 });
        return await toast.TextContentAsync();
    }

    /// <summary>
    /// Проверяет наличие уведомления об успехе и закрывает его
    /// </summary>
    public async Task<bool> IsSuccessToastVisibleAsync()
    {
        var toast = Page.Locator(".toast.success").Last;
        try
        {
            // Ждём появления toast с небольшим таймаутом
            await toast.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = 2000 });
            // Небольшая пауза, чтобы toast точно отрисовался
            await Page.WaitForTimeoutAsync(100);
            // Закрываем все toast уведомления после проверки
            await CloseAllToastsAsync();
            return true;
        }
        catch (TimeoutException)
        {
            return false;
        }
    }

    /// <summary>
    /// Проверяет наличие уведомления об ошибке и закрывает его
    /// </summary>
    public async Task<bool> IsErrorToastVisibleAsync()
    {
        var toast = Page.Locator(".toast.error").Last;
        try
        {
            // Ждём появления toast с небольшим таймаутом
            await toast.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = 2000 });
            // Небольшая пауза, чтобы toast точно отрисовался
            await Page.WaitForTimeoutAsync(100);
            // Закрываем все toast уведомления после проверки
            await CloseAllToastsAsync();
            return true;
        }
        catch (TimeoutException)
        {
            return false;
        }
    }

    /// <summary>
    /// Закрывает модальное окно
    /// </summary>
    public async Task CloseModalAsync()
    {
        var cancelButton = Page.Locator("[data-testid='cancel-btn']");
        if (await cancelButton.IsVisibleAsync())
        {
            await cancelButton.ClickAsync();
        }
        else
        {
            // Альтернативно: клик по крестику
            var closeBtn = Page.Locator("#entity-modal .btn-icon-btn");
            if (await closeBtn.IsVisibleAsync())
            {
                await closeBtn.ClickAsync();
            }
        }
        
        await Page.WaitForSelectorAsync("#entity-modal", new() { State = WaitForSelectorState.Hidden });
    }

    /// <summary>
    /// Делает скриншот
    /// </summary>
    public async Task TakeScreenshotAsync(string fileName)
    {
        await Page.ScreenshotAsync(new() 
        { 
            Path = $"screenshots/{fileName}",
            FullPage = true
        });
    }

    /// <summary>
    /// Закрывает все toast уведомления
    /// </summary>
    public async Task CloseAllToastsAsync()
    {
        var toasts = Page.Locator(".toast.success, .toast.error");
        var count = await toasts.CountAsync();
        
        for (int i = 0; i < count; i++)
        {
            try
            {
                // Закрываем каждый toast по индексу
                var closeButton = toasts.Nth(i).Locator(".toast-close");
                if (await closeButton.IsVisibleAsync())
                {
                    await closeButton.ClickAsync();
                    await Page.WaitForTimeoutAsync(100);
                }
            }
            catch
            {
                // Toast уже закрыт
                break;
            }
        }
    }

    /// <summary>
    /// Обрабатывает диалог подтверждения (confirm)
    /// </summary>
    public async Task HandleConfirmationDialogAsync(Func<Task> clickAction)
    {
        var dialogTcs = new TaskCompletionSource<IDialog>();
        
        // Создаём обработчик, который выполнится только один раз
        _dialogHandler = async (sender, dialog) =>
        {
            if (!dialogTcs.Task.IsCompleted)
            {
                dialogTcs.TrySetResult(dialog);
                await dialog.AcceptAsync();
            }
        };
        
        // Подписываемся на диалог
        Page.Dialog += _dialogHandler;
        
        try
        {
            // Выполняем действие, которое вызывает диалог
            await clickAction();
            
            // Ждём обработки диалога
            var dialog = await dialogTcs.Task;
            await Page.WaitForTimeoutAsync(300);
        }
        finally
        {
            // Отписываемся после обработки
            if (_dialogHandler != null)
            {
                Page.Dialog -= _dialogHandler;
                _dialogHandler = null;
            }
        }
    }
}
