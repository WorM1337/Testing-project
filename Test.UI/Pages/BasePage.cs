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
        var toast = Page.Locator(".toast.success");
        try
        {
            await toast.First.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = 5000 });
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
    /// Проверяет наличие уведомления об ошибке
    /// </summary>
    public async Task<bool> IsErrorToastVisibleAsync()
    {
        var toast = Page.Locator(".toast.error");
        try
        {
            await toast.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = 5000 });
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
        var closeButtons = Page.Locator(".toast-close");
        var count = await closeButtons.CountAsync();
        
        for (int i = 0; i < count; i++)
        {
            try
            {
                await closeButtons.First.ClickAsync();
                await Page.WaitForTimeoutAsync(100);
            }
            catch
            {
                // Toast уже закрыт
                break;
            }
        }
    }
}
