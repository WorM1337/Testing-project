using DotNetEnv;

namespace Test.Core.Integration;

/// <summary>
/// Базовый класс для интеграционных тестов API.
/// Загружает .env файл из корня проекта и использует URL API для тестов.
/// </summary>
[Collection("Integration Tests")]
public abstract class IntegrationTestBase : IAsyncLifetime
{
    protected HttpClient Client = null!;
    private readonly string _baseUrl;
    
    protected List<int> CreatedDishIds = new();
    protected List<int> CreatedProductIds = new();

    protected IntegrationTestBase()
    {
        //TODO: сделать поддержку .env
        _baseUrl = "http://localhost:5099";
    }

    public virtual async Task InitializeAsync()
    {
        Client = new HttpClient
        {
            BaseAddress = new Uri(_baseUrl),
            Timeout = TimeSpan.FromSeconds(30) // Увеличиваем таймаут для тестов
        };
        await Task.CompletedTask;
    }

    public virtual async Task DisposeAsync()
    {
        // TODO: сделать нормальную очистку данных после теста (трекинг измененных данных)
        List<Task> tasks = new();
        foreach (var id in CreatedDishIds)
        {
            tasks.Add(Client.DeleteAsync($"/api/dishes/{id}"));
        }
        await Task.WhenAll(tasks);
        tasks.Clear();
        foreach (var id in CreatedProductIds)
        {
            tasks.Add(Client.DeleteAsync($"/api/products/{id}"));
        }
        await Task.WhenAll(tasks);
        
        Client?.Dispose();
        await Task.CompletedTask;
    }
}
