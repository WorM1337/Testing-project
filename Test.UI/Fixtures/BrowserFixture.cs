using Microsoft.Playwright;

namespace Test.UI.Fixtures;

/// <summary>
/// Глобальная фикстура для управления браузером Playwright
/// Инициализируется один раз на все тесты через ICollectionFixture
/// </summary>
public class BrowserFixture : IDisposable
{
    private IPlaywright? _playwright;
    private IBrowser? _browser;
    private readonly string _baseUrl;
    private bool _disposed;

    public BrowserFixture()
    {
        _baseUrl = Environment.GetEnvironmentVariable("TEST_BASE_URL") ?? "http://localhost:5099";
        InitializeAsync().GetAwaiter().GetResult();
    }

    public IBrowser Browser => _browser ?? throw new InvalidOperationException("Browser not initialized");
    public string BaseUrl => _baseUrl;

    private async Task InitializeAsync()
    {
        _playwright = await Playwright.CreateAsync();
        
        _browser = await _playwright.Chromium.LaunchAsync(new()
        {
            Headless = false,
            SlowMo = 100 // Замедление для лучшей наблюдаемости
        });
    }

    /// <summary>
    /// Создает новый контекст браузера для теста
    /// Каждый тест получает изолированный контекст
    /// </summary>
    public async Task<IBrowserContext> CreateContextAsync()
    {
        if (_browser == null)
            throw new InvalidOperationException("Browser not initialized");
            
        var context = await _browser.NewContextAsync(new()
        {
            ViewportSize = new ViewportSize { Width = 1920, Height = 1080 },
            IgnoreHTTPSErrors = true
        });

        return context;
    }

    public void Dispose()
    {
        if (_disposed) return;
        
        _browser?.CloseAsync().GetAwaiter().GetResult();
        _playwright?.Dispose();
        _disposed = true;
    }
}

/// <summary>
/// Фикстура для управления контекстом браузера на уровне тест-сьюта
/// </summary>
public class BrowserContextFixture : IAsyncLifetime
{
    private readonly BrowserFixture _browserFixture;
    private IBrowserContext? _context;

    public BrowserContextFixture(BrowserFixture browserFixture)
    {
        _browserFixture = browserFixture;
    }

    public IBrowserContext Context => _context ?? throw new InvalidOperationException("Context not initialized");
    public IPage Page => _context?.Pages.FirstOrDefault() 
        ?? throw new InvalidOperationException("No page in context");

    public async Task InitializeAsync()
    {
        _context = await _browserFixture.CreateContextAsync();
        
        // Создаем первую страницу
        await _context.NewPageAsync();
    }

    public async Task DisposeAsync()
    {
        if (_context != null)
        {
            await _context.CloseAsync();
        }
    }
}

