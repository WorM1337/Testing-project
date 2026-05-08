using Microsoft.EntityFrameworkCore;
using Data;
using Data.Contexts;

namespace Test.UI.Fixtures;

/// <summary>
/// Фикстура для управления базой данных в UI тестах
/// Очищает БД перед каждым тестом для изоляции
/// </summary>
public class DatabaseFixture
{
    private readonly string _connectionString;

    public DatabaseFixture()
    {
        _connectionString = Environment.GetEnvironmentVariable("TEST_CONNECTION_STRING") 
            ?? "Host=localhost;Port=5432;Database=book_of_receipts;Username=ale.levchenko;Password=postgres";
    }

    /// <summary>
    /// Очищает базу данных перед тестом
    /// </summary>
    public async Task CleanDatabaseAsync()
    {
        var options = new DbContextOptionsBuilder<BookOfReceiptsDbContext>()
            .UseNpgsql(_connectionString)
            .Options;
            
        await using var context = new BookOfReceiptsDbContext(options);
        
        // Удаляем все блюда (сначала, т.к. есть FK на продукты)
        context.Dishes.RemoveRange(context.Dishes);
        
        // Удаляем все продукты
        context.Products.RemoveRange(context.Products);
        
        await context.SaveChangesAsync();
    }
}

/// <summary>
/// Фикстура для очистки БД перед каждым тестом в сьюте
/// </summary>
[CollectionDefinition("UI Tests")]
public class UiTestCollection : ICollectionFixture<BrowserFixture>, ICollectionFixture<DatabaseFixture>
{
    // Этот класс не содержит кода, только метаданные коллекции
}
