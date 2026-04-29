using Data.Contexts;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.VisualStudio.TestPlatform.TestHost;

namespace Test.Core.Integration.Fixtures;

/// <summary>
/// Фикстура для API-тестов с использованием РЕАЛЬНОЙ базы данных PostgreSQL.
/// НЕИЗОЛИРОВАННАЯ среда: тесты работают с общей БД, данные НЕ очищаются между тестами.
/// </summary>
public class ApiTestFixture : WebApplicationFactory<Program>, IAsyncLifetime
{
    // Используем реальную строку подключения из конфигурации
    private const string TestConnectionString = 
        "Host=localhost;Port=5432;Database=book_of_receipts_test;Username=postgres;Password=postgres";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Удаляем конфигурацию БД из Program.cs
            services.RemoveAll(typeof(DbContextOptions<BookOfReceiptsDbContext>));
            
            // Добавляем РЕАЛЬНУЮ PostgreSQL БД для тестов
            services.AddDbContext<BookOfReceiptsDbContext>(options =>
            {
                options.UseNpgsql(TestConnectionString);
            });
        });
    }

    public async Task InitializeAsync()
    {
        // Применяем миграции к реальной БД (если нужно)
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<BookOfReceiptsDbContext>();
        
        // ВАЖНО: НЕ удаляем БД, НЕ очищаем данные
        // Просто убеждаемся, что БД существует
        await db.Database.EnsureCreatedAsync();
    }

    public new async Task DisposeAsync()
    {
        // ВАЖНО: НЕ удаляем БД после тестов
        // Данные остаются в БД для анализа
        await Task.CompletedTask;
    }

    /// <summary>
    /// Получает DbContext для прямого доступа к БД в тестах
    /// </summary>
    public BookOfReceiptsDbContext GetDbContext()
    {
        var scope = Services.CreateScope();
        return scope.ServiceProvider.GetRequiredService<BookOfReceiptsDbContext>();
    }
}